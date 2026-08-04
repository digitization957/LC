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

## mail_job_run
```sql
CREATE TABLE mail_job_run (
  run_id INT AUTO_INCREMENT PRIMARY KEY,
  job_name VARCHAR(30) NOT NULL,           -- 'MonthlyMailJob', 'MondayMailJob'
  run_for_date DATE NOT NULL,              -- the 1st-of-month / the Monday this run is for
  started_at DATETIME NOT NULL,
  finished_at DATETIME NULL,
  total_sent INT NOT NULL DEFAULT 0,
  total_failed INT NOT NULL DEFAULT 0,
  total_skipped INT NOT NULL DEFAULT 0,
  status VARCHAR(10) NOT NULL DEFAULT 'running',   -- running / completed / crashed
  error_message TEXT NULL,
  INDEX (job_name, run_for_date)
);
```
One row per execution of an automail console app. `finished_at`/`status`/totals are always written
in a `finally` block, even if the run crashes mid-way, so a run is never left stuck at `running`.

## mail_send_log
```sql
CREATE TABLE mail_send_log (
  log_id INT AUTO_INCREMENT PRIMARY KEY,
  run_id INT NOT NULL,
  job_name VARCHAR(30) NOT NULL,
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
  FOREIGN KEY (run_id) REFERENCES mail_job_run(run_id),
  INDEX (job_name, plant_id),
  INDEX (run_id)
);
```
One row per mail attempt (one per plant for MondayMailJob, one per owner for MonthlyMailJob).
Every plant/owner in scope gets exactly one row here per run — `sent`, `failed` (with
`error_message`), or `skipped` (with `skip_reason`, e.g. "no recipients" / "no compliances") — so
nothing is ever silently dropped. One plant/owner failing does not stop the rest of the run; the
job moves on and logs the next one, then the run totals in `mail_job_run` reflect the true outcome.
