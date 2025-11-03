-- ============================================================================
-- User Activity Audit Log Table
-- ============================================================================
-- This table stores all user activities for audit and compliance purposes
-- Tracks: logins, logouts, creates, updates, deletes, and other actions
-- ============================================================================

CREATE TABLE IF NOT EXISTS user_activity_logs (
    log_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NULL COMMENT 'Foreign key to logins table, NULL if user deleted',
    username VARCHAR(100) NOT NULL COMMENT 'Username who performed the action',
    action_type VARCHAR(50) NOT NULL COMMENT 'Type of action: LOGIN, LOGOUT, CREATE, UPDATE, DELETE, PASSWORD_CHANGE, etc.',
    module VARCHAR(100) NOT NULL COMMENT 'Module/Feature: Authentication, Users, Faculty, Departments, etc.',
    description TEXT NOT NULL COMMENT 'Detailed description of the action performed',
    ip_address VARCHAR(45) NULL COMMENT 'IP address of the user (IPv4 or IPv6)',
    timestamp DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT 'When the action occurred',
    
    -- Indexes for better query performance
    INDEX idx_user_id (user_id),
    INDEX idx_username (username),
    INDEX idx_action_type (action_type),
    INDEX idx_module (module),
    INDEX idx_timestamp (timestamp),
    INDEX idx_composite (username, action_type, timestamp),
    
    -- Foreign key constraint (SET NULL on delete to preserve audit trail)
    FOREIGN KEY (user_id) REFERENCES logins(login_id) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Audit log table for tracking all user activities in the system';

-- ============================================================================
-- Sample queries for viewing logs
-- ============================================================================

-- View all login activities
-- SELECT * FROM user_activity_logs WHERE action_type = 'LOGIN' ORDER BY timestamp DESC LIMIT 100;

-- View activities by specific user
-- SELECT * FROM user_activity_logs WHERE username = 'admin' ORDER BY timestamp DESC;

-- View activities in date range
-- SELECT * FROM user_activity_logs WHERE timestamp BETWEEN '2025-01-01' AND '2025-12-31' ORDER BY timestamp DESC;

-- Count activities by action type
-- SELECT action_type, COUNT(*) as count FROM user_activity_logs GROUP BY action_type ORDER BY count DESC;

-- Recent activities (last 24 hours)
-- SELECT * FROM user_activity_logs WHERE timestamp >= DATE_SUB(NOW(), INTERVAL 24 HOUR) ORDER BY timestamp DESC;

-- ============================================================================
-- Maintenance queries
-- ============================================================================

-- Archive old logs (older than 1 year) - Run periodically
-- CREATE TABLE user_activity_logs_archive LIKE user_activity_logs;
-- INSERT INTO user_activity_logs_archive SELECT * FROM user_activity_logs WHERE timestamp < DATE_SUB(NOW(), INTERVAL 1 YEAR);
-- DELETE FROM user_activity_logs WHERE timestamp < DATE_SUB(NOW(), INTERVAL 1 YEAR);

-- Check table size
-- SELECT 
--     table_name AS 'Table',
--     ROUND(((data_length + index_length) / 1024 / 1024), 2) AS 'Size (MB)'
-- FROM information_schema.TABLES 
-- WHERE table_schema = DATABASE() AND table_name = 'user_activity_logs';
