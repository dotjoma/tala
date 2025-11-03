-- ============================================================================
-- Faculty Attendance Table
-- ============================================================================
-- Tracks faculty/teacher attendance with Present/Absent status only
-- ============================================================================

CREATE TABLE IF NOT EXISTS faculty_attendance (
    attendance_id INT AUTO_INCREMENT PRIMARY KEY,
    teacherID INT NOT NULL COMMENT 'Foreign key to teacherinformation table',
    attendance_date DATE NOT NULL COMMENT 'Date of attendance',
    time_in TIME NULL COMMENT 'Time faculty arrived',
    time_out TIME NULL COMMENT 'Time faculty left',
    status ENUM('Present', 'Absent') DEFAULT 'Absent' COMMENT 'Attendance status - Present or Absent only',
    remarks TEXT NULL COMMENT 'Additional notes or comments',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Indexes for better query performance
    INDEX idx_teacher (teacherID),
    INDEX idx_date (attendance_date),
    INDEX idx_status (status),
    INDEX idx_composite (teacherID, attendance_date),
    
    -- Unique constraint: one record per teacher per day
    UNIQUE KEY unique_teacher_date (teacherID, attendance_date),
    
    -- Foreign key constraint
    FOREIGN KEY (teacherID) REFERENCES teacherinformation(teacherID) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Faculty/Teacher attendance records - Present/Absent only';

-- ============================================================================
-- Sample data (optional - for testing)
-- ============================================================================
-- INSERT INTO faculty_attendance (teacherID, attendance_date, time_in, time_out, status)
-- VALUES 
--     (1, '2025-10-15', '08:00:00', '17:00:00', 'Present'),
--     (1, '2025-10-16', NULL, NULL, 'Absent'),
--     (2, '2025-10-15', '08:15:00', '17:30:00', 'Present');

-- ============================================================================
-- Useful queries
-- ============================================================================

-- View all attendance for a specific teacher
-- SELECT * FROM faculty_attendance WHERE teacherID = 1 ORDER BY attendance_date DESC;

-- View attendance for a date range
-- SELECT 
--     CONCAT(ti.firstname, ' ', ti.lastname) AS faculty_name,
--     fa.attendance_date,
--     fa.time_in,
--     fa.time_out,
--     fa.status
-- FROM faculty_attendance fa
-- JOIN teacherinformation ti ON fa.teacherID = ti.teacherID
-- WHERE fa.attendance_date BETWEEN '2025-10-01' AND '2025-10-31'
-- ORDER BY ti.lastname, fa.attendance_date;

-- Count present/absent days for a teacher
-- SELECT 
--     status,
--     COUNT(*) as count
-- FROM faculty_attendance
-- WHERE teacherID = 1
-- GROUP BY status;

-- Monthly attendance summary
-- SELECT 
--     CONCAT(ti.firstname, ' ', ti.lastname) AS faculty_name,
--     DATE_FORMAT(fa.attendance_date, '%Y-%m') AS month,
--     SUM(CASE WHEN fa.status = 'Present' THEN 1 ELSE 0 END) AS present_days,
--     SUM(CASE WHEN fa.status = 'Absent' THEN 1 ELSE 0 END) AS absent_days,
--     COUNT(*) AS total_days
-- FROM faculty_attendance fa
-- JOIN teacherinformation ti ON fa.teacherID = ti.teacherID
-- GROUP BY ti.teacherID, DATE_FORMAT(fa.attendance_date, '%Y-%m')
-- ORDER BY ti.lastname, month DESC;
