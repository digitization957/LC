using System;
using System.Web.UI;
using ComplianceV2._2.App_Code;

namespace ComplianceV2._2
{
    public partial class Training : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var sessionId = Request.QueryString["sessionId"];
            if (SessionStore.Validate(sessionId) == null) Response.Redirect("Default.aspx");
        }
    }
}
