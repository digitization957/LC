## mail_config_recipient
```sql
CREATE TABLE mail_config_recipient (
  id INT AUTO_INCREMENT PRIMARY KEY,
  plant_id INT NOT NULL,
  mail_type VARCHAR(20) NOT NULL,
  group_key VARCHAR(10) NOT NULL,
  token VARCHAR(50) NOT NULL,
  name VARCHAR(150) NOT NULL,
  is_manual TINYINT(1) NOT NULL DEFAULT 0,
  added_by VARCHAR(50) NOT NULL,
  added_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY uq_mail_recipient (plant_id, mail_type, group_key, token),
  INDEX (plant_id)
);
```
Per-plant recipient config for automated mails. `mail_type` = `monday` (group `cc`) or `reminders` (groups `r1`-`r4`).

## mail_config_log
```sql
CREATE TABLE mail_config_log (
  id INT AUTO_INCREMENT PRIMARY KEY,
  plant_id INT NOT NULL,
  mail_type VARCHAR(20) NOT NULL,
  group_key VARCHAR(10) NOT NULL,
  action VARCHAR(10) NOT NULL,
  token VARCHAR(50) NOT NULL,
  name VARCHAR(150) NOT NULL,
  is_manual TINYINT(1) NOT NULL DEFAULT 0,
  performed_by VARCHAR(50) NOT NULL,
  performed_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  INDEX (plant_id)
);
```
Audit trail of who added/removed a mail-config recipient and when.

---

## mail_send_log
```sql
CREATE TABLE mail_send_log (
  log_id INT AUTO_INCREMENT PRIMARY KEY,
  job_name VARCHAR(30) NOT NULL,           -- 'MonthlyMailJob', 'MondayMailJob'
  plant_id INT NULL,
  owner_token VARCHAR(50) NULL,
  to_emails TEXT NOT NULL,
  cc_emails TEXT NULL,
  subject VARCHAR(255) NOT NULL,
  status VARCHAR(10) NOT NULL,             -- sent / failed / skipped
  skip_reason VARCHAR(255) NULL,
  error_message TEXT NULL,
  overdue_count INT NULL,
  due_count INT NULL,
  sent_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  INDEX (job_name, sent_at)
);
```
One row per mail actually sent (or attempted) — not per job execution. MonthlyMailJob logs one row
per owner (`owner_token` set, `plant_id` NULL). MondayMailJob logs one row per plant (`plant_id`
set, `owner_token` NULL). `status` is `sent`, `failed` (with `error_message`), or `skipped` (with
`skip_reason`, e.g. "no recipients" / "no compliances") — one owner/plant failing never stops the
rest of the batch, and every owner/plant in scope always gets logged.
