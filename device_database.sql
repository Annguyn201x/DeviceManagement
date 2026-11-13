CREATE DATABASE IF NOT EXISTS device
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;
USE device;

CREATE TABLE IF NOT EXISTS `user` (
  id INT NOT NULL AUTO_INCREMENT,
  user_name VARCHAR(100) NOT NULL,
  password VARCHAR(64) NOT NULL,
  `Role` VARCHAR(20) NOT NULL DEFAULT 'member',
  PRIMARY KEY (id),
  UNIQUE KEY uq_user_name (user_name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Add the Role column if this script is run over the older user table in the supplied screenshot.
SET @has_user_role = (
  SELECT COUNT(*) FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'user' AND COLUMN_NAME = 'Role'
);
SET @role_migration = IF(@has_user_role = 0,
  'ALTER TABLE `user` ADD COLUMN `Role` VARCHAR(20) NOT NULL DEFAULT ''member''',
  'SELECT 1'
);
PREPARE role_migration_stmt FROM @role_migration;
EXECUTE role_migration_stmt;
DEALLOCATE PREPARE role_migration_stmt;

CREATE TABLE IF NOT EXISTS ho_nuoi (
  id INT NOT NULL AUTO_INCREMENT,
  ten_ho VARCHAR(200) NOT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_ho_nuoi_ten (ten_ho)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS thiet_bi (
  ma_may VARCHAR(100) NOT NULL,
  ho_nuoi_id INT NOT NULL,
  current_ip VARCHAR(45) NOT NULL DEFAULT '0.0.0.0',
  trang_thai BOOLEAN NOT NULL DEFAULT TRUE,
  PRIMARY KEY (ma_may),
  KEY ix_thiet_bi_ho_nuoi (ho_nuoi_id),
  CONSTRAINT fk_thiet_bi_ho_nuoi FOREIGN KEY (ho_nuoi_id)
    REFERENCES ho_nuoi(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS thong_so (
  id INT NOT NULL AUTO_INCREMENT,
  ma_may VARCHAR(100) NOT NULL,
  nong_do_ph DECIMAL(5,2) NOT NULL,
  tds INT NOT NULL,
  nhiet_do DECIMAL(5,2) NOT NULL,
  thoi_gian_do TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  KEY ix_thong_so_may_thoi_gian (ma_may, thoi_gian_do),
  CONSTRAINT fk_thong_so_thiet_bi FOREIGN KEY (ma_may)
    REFERENCES thiet_bi(ma_may) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Demo login: demo / Demo@123. The app verifies lowercase SHA-256 hex hashes.
INSERT INTO `user` (user_name, password, `Role`)
VALUES ('demo', LOWER(SHA2('Demo@123', 256)), 'root')
ON DUPLICATE KEY UPDATE password = VALUES(password), `Role` = VALUES(`Role`);

INSERT INTO ho_nuoi (ten_ho)
VALUES ('Ho demo')
ON DUPLICATE KEY UPDATE id = LAST_INSERT_ID(id);
SET @demo_lake_id = (SELECT id FROM ho_nuoi WHERE ten_ho = 'Ho demo' LIMIT 1);

INSERT INTO thiet_bi (ma_may, ho_nuoi_id, current_ip, trang_thai)
VALUES ('ESP32_DEMO', @demo_lake_id, '192.168.1.100', TRUE)
ON DUPLICATE KEY UPDATE ho_nuoi_id = VALUES(ho_nuoi_id), current_ip = VALUES(current_ip), trang_thai = VALUES(trang_thai);

INSERT INTO thong_so (ma_may, nong_do_ph, tds, nhiet_do, thoi_gian_do)
SELECT 'ESP32_DEMO', 7.10, 340, 26.4, NOW() - INTERVAL 2 HOUR
FROM DUAL;
INSERT INTO thong_so (ma_may, nong_do_ph, tds, nhiet_do, thoi_gian_do)
SELECT 'ESP32_DEMO', 7.18, 345, 26.6, NOW() - INTERVAL 1 HOUR
FROM DUAL;
INSERT INTO thong_so (ma_may, nong_do_ph, tds, nhiet_do, thoi_gian_do)
SELECT 'ESP32_DEMO', 7.20, 342, 26.5, NOW()
FROM DUAL;
