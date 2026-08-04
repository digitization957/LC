using System;
using System.IO;
using System.Web;
using ComplianceV2._2.App_Code;

namespace ComplianceV2._2
{
    // Handles compliance-attachment uploads. Not a WebMethod because [WebMethod]/JSON can't carry binary file bodies.
    public class Upload : IHttpHandler
    {
        private const long MaxBytes = 5 * 1024 * 1024; // 5MB

        public void ProcessRequest(HttpContext ctx)
        {
            ctx.Response.ContentType = "application/json";
            try
            {
                var sessionId = ctx.Request.Form["sessionId"];
                var complianceIdRaw = ctx.Request.Form["complianceId"];
                var session = SessionStore.Validate(sessionId);
                if (session == null) throw new UnauthorizedAccessException("Session expired.");
                if (!int.TryParse(complianceIdRaw, out int complianceId)) throw new ArgumentException("Invalid compliance id.");

                var row = Db.QuerySingle("SELECT owner_token FROM compliances WHERE compliance_id=@id AND is_active=1", Db.P("@id", complianceId));
                if (row == null) throw new ArgumentException("Compliance not found.");
                if (session.Role != "owner" || (string)row["owner_token"] != session.Token)
                    throw new UnauthorizedAccessException("Not yours.");

                var file = ctx.Request.Files["file"];
                if (file == null || file.ContentLength == 0) throw new ArgumentException("No file uploaded.");
                if (file.ContentLength > MaxBytes) throw new ArgumentException("File exceeds 5MB limit.");

                var ext = DetectSafeExtension(file);
                if (ext == null) throw new ArgumentException("Only PDF, JPG, PNG, DOC or DOCX files are allowed.");

                var uploadsDir = ctx.Server.MapPath("~/App_Data/Uploads");
                if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                var storedName = Guid.NewGuid().ToString("N") + ext;
                var fullPath = Path.Combine(uploadsDir, storedName);
                FileCrypto.EncryptToFile(file.InputStream, fullPath);

                var originalName = Path.GetFileName(file.FileName);
                ctx.Response.Write(Json.Ok(new { fileName = originalName, fileUrl = storedName }));
            }
            catch (UnauthorizedAccessException ex)
            {
                ctx.Response.StatusCode = 403;
                ctx.Response.Write(Json.Err(ex.Message));
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 400;
                ctx.Response.Write(Json.Err(ex.Message));
            }
        }

        // Validates the file's magic bytes match a real PDF/JPG/PNG — extension alone can be spoofed.
        private static string DetectSafeExtension(HttpPostedFile file)
        {
            // Do not dispose file.InputStream here — it's the same live stream SaveAs() reads from afterward.
            var header = new byte[8];
            var s = file.InputStream;
            s.Position = 0;
            s.Read(header, 0, 8);
            s.Position = 0;

            if (header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46) return ".pdf";
            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) return ".jpg";
            if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47) return ".png";
            // OLE2 compound document — .doc
            if (header[0] == 0xD0 && header[1] == 0xCF && header[2] == 0x11 && header[3] == 0xE0) return ".doc";
            // ZIP-based OOXML — only allow if extension is explicitly .docx
            if (header[0] == 0x50 && header[1] == 0x4B && header[2] == 0x03 && header[3] == 0x04) {
                var origExt = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (origExt == ".docx") return ".docx";
            }
            return null;
        }

        public bool IsReusable => false;
    }
}
