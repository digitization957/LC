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

Note: mail config is now scoped per plant - `plant_id` (references `plant_master.tbl_plant.Plant_ID`) is required on every row, and the uniqueness of a recipient is per plant + mail type + group.
