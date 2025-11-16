-- Migration Script for Attendance System Updates
-- Run this script on your existing database to add new fields

-- Add new columns to attendance_record table
ALTER TABLE attendance_record 
ADD COLUMN IF NOT EXISTS remarks TEXT DEFAULT NULL COMMENT 'Remarks for attendance (Under time, Over time, Edit reason, Manual input reason)';

ALTER TABLE attendance_record 
ADD COLUMN IF NOT EXISTS edited_by VARCHAR(255) DEFAULT NULL COMMENT 'Username who edited the record';

ALTER TABLE attendance_record 
ADD COLUMN IF NOT EXISTS edited_at DATETIME DEFAULT NULL COMMENT 'Timestamp when record was edited';

ALTER TABLE attendance_record 
ADD COLUMN IF NOT EXISTS manual_input_by VARCHAR(255) DEFAULT NULL COMMENT 'Username who manually input the record';

ALTER TABLE attendance_record 
ADD COLUMN IF NOT EXISTS manual_input_reason TEXT DEFAULT NULL COMMENT 'Reason for manual input';

-- Add new columns to teacherinformation table
ALTER TABLE teacherinformation
ADD COLUMN IF NOT EXISTS shift_start_time TIME DEFAULT NULL COMMENT 'Teacher shift start time (e.g., 07:00:00 for morning, 13:00:00 for afternoon)';

ALTER TABLE teacherinformation
ADD COLUMN IF NOT EXISTS shift_end_time TIME DEFAULT NULL COMMENT 'Teacher shift end time (e.g., 12:00:00 for morning, 19:00:00 for afternoon)';

-- Verify the changes
SELECT 'attendance_record columns added successfully' AS status;
SELECT 'teacherinformation columns added successfully' AS status;

-- Optional: Set default shift times for existing teachers (morning shift 7am-12pm)
-- Uncomment the lines below if you want to set default shift times
-- UPDATE teacherinformation 
-- SET shift_start_time = '07:00:00', shift_end_time = '12:00:00' 
-- WHERE shift_start_time IS NULL AND isActive = 1;
