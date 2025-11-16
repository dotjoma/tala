SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;


DROP TABLE IF EXISTS `announcements`;
CREATE TABLE `announcements` (
  `id` int(11) NOT NULL,
  `pictureHeader` longblob DEFAULT NULL,
  `header` varchar(255) NOT NULL,
  `dayInfo` date DEFAULT NULL,
  `timeInfo` varchar(255) NOT NULL,
  `description` varchar(255) NOT NULL,
  `isActive` tinyint(4) DEFAULT 1,
  `lookFor` varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

DROP TABLE IF EXISTS `attendance_record`;
CREATE TABLE `attendance_record` (
  `attendanceID` int(10) UNSIGNED NOT NULL,
  `tag_id` varchar(125) NOT NULL,
  `studID` int(11) DEFAULT NULL,
  `classroom_id` int(11) DEFAULT NULL,
  `subject_id` int(11) DEFAULT NULL,
  `teacherID` int(11) DEFAULT NULL,
  `logDate` date NOT NULL,
  `arrivalTime` datetime DEFAULT NULL,
  `arrStatus` varchar(155) NOT NULL,
  `departureTime` datetime DEFAULT NULL,
  `depStatus` varchar(155) NOT NULL,
  `depState` tinyint(1) NOT NULL DEFAULT 0,
  `countRead` tinyint(1) DEFAULT 1,
  `remarks` TEXT DEFAULT NULL COMMENT 'Remarks for attendance (Under time, Over time, Edit reason, Manual input reason)',
  `edited_by` varchar(255) DEFAULT NULL COMMENT 'Username who edited the record',
  `edited_at` datetime DEFAULT NULL COMMENT 'Timestamp when record was edited',
  `manual_input_by` varchar(255) DEFAULT NULL COMMENT 'Username who manually input the record',
  `manual_input_reason` TEXT DEFAULT NULL COMMENT 'Reason for manual input'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

DROP TABLE IF EXISTS `classrooms`;
CREATE TABLE `classrooms` (
  `id` int(11) NOT NULL,
  `classroom_id` int(11) DEFAULT NULL,
  `classroom_name` varchar(100) NOT NULL,
  `location` varchar(100) DEFAULT NULL,
  `isActive` tinyint(4) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

DROP TABLE IF EXISTS `class_schedules`;
CREATE TABLE `class_schedules` (
  `schedule_id` int(11) NOT NULL,
  `subject_id` int(11) DEFAULT NULL,
  `section_id` int(11) DEFAULT NULL,
  `classroom_id` int(11) DEFAULT NULL,
  `teacherID` int(11) DEFAULT NULL,
  `day` varchar(50) DEFAULT NULL,
  `start_time` time NOT NULL,
  `end_time` time NOT NULL,
  `isActive` tinyint(4) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

DROP TABLE IF EXISTS `departments`;
CREATE TABLE `departments` (
  `department_id` int(11) NOT NULL,
  `department_code` varchar(10) NOT NULL,
  `department_name` varchar(100) NOT NULL,
  `description` text DEFAULT NULL,
  `head_teacher_id` int(11) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT 1,
  `created_at` timestamp NOT NULL DEFAULT current_timestamp(),
  `updated_at` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

DROP TABLE IF EXISTS `enrollments`;
CREATE TABLE `enrollments` (
  `enrollment_id` int(11) NOT NULL,
  `studID` int(11) DEFAULT NULL,
  `schedule_id` int(11) DEFAULT NULL,
  `enrollment_date` date DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

DROP TABLE IF EXISTS `gradelevel`;
CREATE TABLE `gradelevel` (
  `gradeID` int(11) NOT NULL,
  `gradelevel` varchar(255) NOT NULL,
  `isActive` tinyint(4) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

DROP TABLE IF EXISTS `logins`;
CREATE TABLE `logins` (
  `login_id` int(11) NOT NULL,
  `fullname` varchar(255) NOT NULL,
  `username` varchar(55) NOT NULL,
  `password` varchar(255) NOT NULL,
  `email` varchar(255) NOT NULL,
  `address` varchar(255) NOT NULL,
  `role` enum('admin','hr','attendance') NOT NULL,
  `user_id` int(11) DEFAULT NULL,
  `created_at` date DEFAULT NULL,
  `isActive` tinyint(4) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

DROP TABLE IF EXISTS `parentrecords`;
CREATE TABLE `parentrecords` (
  `parentID` int(11) NOT NULL,
  `firstname` varchar(255) NOT NULL,
  `middlename` varchar(255) NOT NULL,
  `lastname` varchar(255) NOT NULL,
  `gender` varchar(15) NOT NULL,
  `birthdate` date DEFAULT NULL,
  `contactNo` varchar(25) NOT NULL,
  `address` varchar(255) NOT NULL,
  `relationship` varchar(55) NOT NULL,
  `isActive` tinyint(4) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

DROP TABLE IF EXISTS `refbrgy`;
CREATE TABLE `refbrgy` (
  `id` int(11) NOT NULL,
  `brgyCode` varchar(255) DEFAULT NULL,
  `brgyDesc` text DEFAULT NULL,
  `regCode` varchar(255) DEFAULT NULL,
  `provCode` varchar(255) DEFAULT NULL,
  `citymunCode` varchar(255) DEFAULT NULL
) ENGINE=MyISAM DEFAULT CHARSET=utf8 COLLATE=utf8_general_ci;

DROP TABLE IF EXISTS `refprovince`;
CREATE TABLE `refprovince` (
  `id` int(11) NOT NULL,
  `psgcCode` varchar(255) DEFAULT NULL,
  `provDesc` text DEFAULT NULL,
  `regCode` varchar(255) DEFAULT NULL,
  `provCode` varchar(255) DEFAULT NULL
) ENGINE=MyISAM DEFAULT CHARSET=utf8 COLLATE=utf8_general_ci;

DROP TABLE IF EXISTS `refregion`;
CREATE TABLE `refregion` (
  `id` int(11) NOT NULL,
  `psgcCode` varchar(255) DEFAULT NULL,
  `regDesc` text DEFAULT NULL,
  `regCode` varchar(255) DEFAULT NULL
) ENGINE=MyISAM DEFAULT CHARSET=utf8 COLLATE=utf8_general_ci;

DROP TABLE IF EXISTS `sections`;
CREATE TABLE `sections` (
  `section_id` int(11) NOT NULL,
  `year_level` varchar(50) NOT NULL,
  `section_name` varchar(50) NOT NULL,
  `isActive` tinyint(4) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

DROP TABLE IF EXISTS `studentrecords`;
CREATE TABLE `studentrecords` (
  `studID` int(11) NOT NULL,
  `profilepicture` longblob DEFAULT NULL,
  `tagID` varchar(25) NOT NULL,
  `lrn` varchar(255) NOT NULL,
  `firstname` varchar(255) NOT NULL,
  `middlename` varchar(255) NOT NULL,
  `lastname` varchar(255) NOT NULL,
  `extName` varchar(25) NOT NULL,
  `gender` varchar(15) NOT NULL,
  `birthdate` date DEFAULT NULL,
  `section_id` int(11) DEFAULT NULL,
  `gradeID` int(11) DEFAULT NULL,
  `parentID` int(11) DEFAULT NULL,
  `contactNo` varchar(25) NOT NULL,
  `regionID` int(11) DEFAULT NULL,
  `provinceID` int(11) DEFAULT NULL,
  `cityID` int(11) DEFAULT NULL,
  `brgyID` int(11) DEFAULT NULL,
  `homeadd` varchar(255) NOT NULL,
  `isActive` tinyint(4) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

DROP TABLE IF EXISTS `student_sections`;
CREATE TABLE `student_sections` (
  `id` int(11) NOT NULL,
  `studID` int(11) DEFAULT NULL,
  `section_id` int(11) DEFAULT NULL,
  `assignment_date` date DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

DROP TABLE IF EXISTS `subjects`;
CREATE TABLE `subjects` (
  `subject_id` int(11) NOT NULL,
  `subject_name` varchar(100) NOT NULL,
  `teacherID` int(11) DEFAULT NULL,
  `isActive` tinyint(4) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

DROP TABLE IF EXISTS `teacherinformation`;
CREATE TABLE `teacherinformation` (
  `teacherID` int(11) NOT NULL,
  `user_id` int(11) DEFAULT NULL,
  `profileImg` longblob DEFAULT NULL,
  `tagID` varchar(255) NOT NULL,
  `employeeID` varchar(255) NOT NULL,
  `lastname` varchar(255) NOT NULL,
  `firstname` varchar(255) NOT NULL,
  `middlename` varchar(255) DEFAULT 'N/A',
  `extName` varchar(25) NOT NULL,
  `gender` varchar(25) NOT NULL,
  `email` varchar(255) NOT NULL,
  `birthdate` date DEFAULT NULL,
  `phoneNo` varchar(255) NOT NULL,
  `homeadd` varchar(255) NOT NULL,
  `brgyID` int(11) DEFAULT NULL,
  `cityID` int(11) DEFAULT NULL,
  `provinceID` int(11) DEFAULT NULL,
  `regionID` int(11) DEFAULT NULL,
  `contactNo` varchar(255) NOT NULL,
  `emergencyContact` varchar(255) NOT NULL,
  `relationship` varchar(255) NOT NULL,
  `department_id` int(11) DEFAULT NULL,
  `shift_start_time` TIME DEFAULT NULL COMMENT 'Teacher shift start time (e.g., 07:00:00 for morning, 13:00:00 for afternoon)',
  `shift_end_time` TIME DEFAULT NULL COMMENT 'Teacher shift end time (e.g., 12:00:00 for morning, 19:00:00 for afternoon)',
  `isActive` tinyint(4) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

DROP TABLE IF EXISTS `teacher_display`;
CREATE TABLE `teacher_display` (
  `teacherID` int(11) DEFAULT NULL,
  `employeeID` varchar(255) DEFAULT NULL,
  `teacher_name` text DEFAULT NULL,
  `gender` varchar(25) DEFAULT NULL,
  `birthdate` date DEFAULT NULL,
  `contactNo` varchar(255) DEFAULT NULL,
  `teacher_address` mediumtext DEFAULT NULL,
  `email` varchar(255) DEFAULT NULL,
  `emergencyContact` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

DROP TABLE IF EXISTS `userlogin`;
CREATE TABLE `userlogin` (
  `userID` int(11) NOT NULL,
  `username` varchar(255) NOT NULL,
  `password` varchar(255) NOT NULL,
  `lastname` varchar(255) NOT NULL,
  `firstname` varchar(255) NOT NULL,
  `middlename` varchar(255) NOT NULL,
  `gender` varchar(25) NOT NULL,
  `role` varchar(255) NOT NULL,
  `isActive` tinyint(4) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

DROP TABLE IF EXISTS `user_activity_logs`;
CREATE TABLE `user_activity_logs` (
  `log_id` int(11) NOT NULL,
  `user_id` int(11) DEFAULT NULL COMMENT 'Foreign key to logins table, NULL if user deleted',
  `username` varchar(100) NOT NULL COMMENT 'Username who performed the action',
  `action_type` varchar(50) NOT NULL COMMENT 'Type of action: LOGIN, LOGOUT, CREATE, UPDATE, DELETE, PASSWORD_CHANGE, etc.',
  `module` varchar(100) NOT NULL COMMENT 'Module/Feature: Authentication, Users, Faculty, Departments, etc.',
  `description` text NOT NULL COMMENT 'Detailed description of the action performed',
  `ip_address` varchar(45) DEFAULT NULL COMMENT 'IP address of the user (IPv4 or IPv6)',
  `timestamp` datetime DEFAULT current_timestamp() COMMENT 'When the action occurred'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Audit log table for tracking all user activities in the system';

ALTER TABLE `attendance_record`
  ADD PRIMARY KEY (`attendanceID`);

ALTER TABLE `departments`
  ADD PRIMARY KEY (`department_id`),
  ADD UNIQUE KEY `department_code` (`department_code`),
  ADD UNIQUE KEY `uk_department_code` (`department_code`),
  ADD KEY `idx_department_active` (`is_active`),
  ADD KEY `fk_head_teacher` (`head_teacher_id`);

ALTER TABLE `logins`
  ADD PRIMARY KEY (`login_id`);

ALTER TABLE `teacherinformation`
  ADD PRIMARY KEY (`teacherID`),
  ADD KEY `fk_teacher_department` (`department_id`);

ALTER TABLE `user_activity_logs`
  ADD PRIMARY KEY (`log_id`),
  ADD KEY `idx_user_id` (`user_id`),
  ADD KEY `idx_username` (`username`),
  ADD KEY `idx_action_type` (`action_type`),
  ADD KEY `idx_module` (`module`),
  ADD KEY `idx_timestamp` (`timestamp`),
  ADD KEY `idx_composite` (`username`,`action_type`,`timestamp`);


ALTER TABLE `attendance_record`
  MODIFY `attendanceID` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=98;

ALTER TABLE `departments`
  MODIFY `department_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

ALTER TABLE `logins`
  MODIFY `login_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=25;

ALTER TABLE `teacherinformation`
  MODIFY `teacherID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=26;

ALTER TABLE `user_activity_logs`
  MODIFY `log_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=360;


ALTER TABLE `departments`
  ADD CONSTRAINT `fk_departments_head_teacher` FOREIGN KEY (`head_teacher_id`) REFERENCES `teacherinformation` (`teacherID`) ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE `teacherinformation`
  ADD CONSTRAINT `fk_teacherinformation_department` FOREIGN KEY (`department_id`) REFERENCES `departments` (`department_id`) ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE `user_activity_logs`
  ADD CONSTRAINT `user_activity_logs_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `logins` (`login_id`) ON DELETE SET NULL ON UPDATE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
