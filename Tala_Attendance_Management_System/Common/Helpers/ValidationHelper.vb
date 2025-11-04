Imports System.Windows.Forms

Public Class ValidationHelper
    Private Shared ReadOnly _logger As ILogger = LoggerFactory.Instance

    Public Shared Function ValidateRequiredFields(panel As Panel, Optional logErrors As Boolean = True, Optional excludeFields As String() = Nothing) As Boolean
        Dim defaultExcluded As String() = {"txtMiddleName", "txtExtName"}
        Dim allExcluded As New List(Of String)(defaultExcluded)
        If excludeFields IsNot Nothing Then
            allExcluded.AddRange(excludeFields)
        End If

        For Each obj As Control In panel.Controls
            If TypeOf obj Is TextBox OrElse TypeOf obj Is ComboBox Then
                If allExcluded.Contains(obj.Name) Then
                    Continue For
                End If

                If String.IsNullOrEmpty(obj.Text.Trim()) Then
                    If logErrors Then
                        _logger.LogWarning($"Field validation failed - Empty field: '{obj.Name}' in panel '{panel.Name}'")
                    End If

                    MessageBox.Show("Please fill up every required field in the form.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    obj.Focus()
                    Return False
                End If
            End If
        Next
        Return True
    End Function

    Public Shared Function IsUsernameUnique(username As String) As Boolean
        Try
            Dim dbContext As New DatabaseContext()
            Dim query As String = "SELECT COUNT(*) FROM logins WHERE username = ? AND isActive=1"
            Dim result As Integer = Convert.ToInt32(dbContext.ExecuteScalar(query, username))

            Return result = 0
        Catch ex As Exception
            _logger.LogError($"Error checking username uniqueness for '{username}': {ex.Message}")
            MessageBox.Show("Error checking for duplicate username: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    Public Shared Function IsValidEmail(email As String) As Boolean
        If String.IsNullOrWhiteSpace(email) Then Return False

        Try
            Dim addr As New System.Net.Mail.MailAddress(email)
            Return addr.Address = email
        Catch
            Return False
        End Try
    End Function

    Public Shared Function ValidateDateOfBirth(dateOfBirth As DateTime, Optional minimumAge As Integer = Constants.MIN_FACULTY_AGE, Optional maximumAge As Integer = Constants.MAX_FACULTY_AGE, Optional logErrors As Boolean = True) As Boolean
        Try
            Dim today As DateTime = DateTime.Today

            If dateOfBirth > today Then
                If logErrors Then
                    _logger.LogWarning($"Date of birth validation failed - Future date: {dateOfBirth:yyyy-MM-dd}")
                End If
                MessageBox.Show("Invalid date of birth. Please enter a valid past date.", "Invalid Date of Birth", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If

            Dim age As Integer = CalculateAge(dateOfBirth, today)

            If age < minimumAge Then
                If logErrors Then
                    _logger.LogWarning($"Date of birth validation failed - Too young: Age {age}, Minimum required: {minimumAge}")
                End If
                MessageBox.Show($"Faculty member must be at least {minimumAge} years old. Current age: {age} years.", "Age Requirement Not Met", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If

            If age > maximumAge Then
                If logErrors Then
                    _logger.LogWarning($"Date of birth validation failed - Too old: Age {age}, Maximum allowed: {maximumAge}")
                End If
                MessageBox.Show($"Faculty member cannot be older than {maximumAge} years. Current age: {age} years.", "Age Limit Exceeded", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If

            If dateOfBirth.Year <= Constants.MIN_BIRTH_YEAR Then
                If logErrors Then
                    _logger.LogWarning($"Date of birth validation failed - Unrealistic date: {dateOfBirth:yyyy-MM-dd}")
                End If
                MessageBox.Show("Invalid date of birth. Please enter a realistic date.", "Invalid Date of Birth", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If

            If logErrors Then
                _logger.LogInfo($"Date of birth validation passed - DOB: {dateOfBirth:yyyy-MM-dd}, Age: {age} years")
            End If

            Return True

        Catch ex As Exception
            If logErrors Then
                _logger.LogError($"Error validating date of birth: {ex.Message}")
            End If
            MessageBox.Show("Error validating date of birth. Please check the date format and try again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function
    Public Shared Function CalculateAge(dateOfBirth As DateTime, referenceDate As DateTime) As Integer
        Try
            Dim age As Integer = referenceDate.Year - dateOfBirth.Year

            If referenceDate.Month < dateOfBirth.Month OrElse
               (referenceDate.Month = dateOfBirth.Month AndAlso referenceDate.Day < dateOfBirth.Day) Then
                age -= 1
            End If

            Return age
        Catch ex As Exception
            _logger.LogError($"Error calculating age: {ex.Message}")
            Return 0
        End Try
    End Function

    Public Shared Function ValidateDateOfBirthControl(dateTimePicker As DateTimePicker, Optional minimumAge As Integer = Constants.MIN_FACULTY_AGE, Optional maximumAge As Integer = Constants.MAX_FACULTY_AGE, Optional logErrors As Boolean = True) As Boolean
        Try
            If logErrors Then
                _logger.LogInfo($"Validating DateTimePicker '{dateTimePicker.Name}' - Value: {dateTimePicker.Value:yyyy-MM-dd}")
            End If

            Dim isValid As Boolean = ValidateDateOfBirth(dateTimePicker.Value.Date, minimumAge, maximumAge, logErrors)

            If Not isValid Then
                dateTimePicker.Focus()
            End If

            Return isValid

        Catch ex As Exception
            If logErrors Then
                _logger.LogError($"Error validating DateTimePicker control '{dateTimePicker.Name}': {ex.Message}")
            End If
            MessageBox.Show("Error validating date of birth control. Please try again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    Public Shared Function RegionHasProvinces(regionName As String, Optional regionCode As String = "") As Boolean
        Try
            If String.IsNullOrWhiteSpace(regionName) Then
                Return True
            End If

            If regionName.ToUpper().Contains("NATIONAL CAPITAL REGION") OrElse
               regionName.ToUpper().Contains("NCR") OrElse
               regionCode = Constants.NCR_REGION_CODE Then
                _logger.LogInfo($"Region '{regionName}' identified as NCR - no provinces")
                Return False
            End If

            _logger.LogInfo($"Region '{regionName}' has provinces")
            Return True

        Catch ex As Exception
            _logger.LogError($"Error checking if region has provinces: {ex.Message}")
            Return True
        End Try
    End Function

    Public Shared Function ValidateDepartmentSelection(departmentComboBox As ComboBox, Optional isRequired As Boolean = True, Optional logErrors As Boolean = True) As Boolean
        Try
            If Not isRequired Then
                Return True
            End If

            If departmentComboBox.SelectedValue Is Nothing OrElse IsDBNull(departmentComboBox.SelectedValue) Then
                If logErrors Then
                    _logger.LogWarning($"Department validation failed - No department selected in '{departmentComboBox.Name}'")
                End If
                MessageBox.Show("Please select a department for this faculty member.", "Department Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                departmentComboBox.Focus()
                Return False
            End If

            Dim selectedValue As String = departmentComboBox.SelectedValue.ToString()
            If String.IsNullOrWhiteSpace(selectedValue) OrElse selectedValue = "0" OrElse selectedValue.ToUpper() = "NULL" Then
                If logErrors Then
                    _logger.LogWarning($"Department validation failed - Invalid department selection: '{selectedValue}' in '{departmentComboBox.Name}'")
                End If
                MessageBox.Show("Please select a valid department for this faculty member.", "Invalid Department Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                departmentComboBox.Focus()
                Return False
            End If

            If Not IsNumeric(selectedValue) Then
                If logErrors Then
                    _logger.LogWarning($"Department validation failed - Non-numeric department ID: '{selectedValue}' in '{departmentComboBox.Name}'")
                End If
                MessageBox.Show("Please select a valid department for this faculty member.", "Invalid Department Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                departmentComboBox.Focus()
                Return False
            End If

            If logErrors Then
                _logger.LogInfo($"Department validation passed - Selected department ID: {selectedValue}")
            End If

            Return True

        Catch ex As Exception
            If logErrors Then
                _logger.LogError($"Error validating department selection: {ex.Message}")
            End If
            MessageBox.Show("Error validating department selection. Please try again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    Public Shared Function ValidateRequiredFieldsWithDynamicProvince(panel As Panel, regionComboBox As ComboBox, Optional logErrors As Boolean = True, Optional excludeFields As String() = Nothing) As Boolean
        Try
            Dim regionHasProvinces As Boolean = True
            If regionComboBox IsNot Nothing AndAlso regionComboBox.SelectedItem IsNot Nothing Then
                regionHasProvinces = ValidationHelper.RegionHasProvinces(regionComboBox.Text)
            End If

            Dim defaultExcluded As New List(Of String)({"txtMiddleName", "txtExtName"})

            If Not regionHasProvinces Then
                defaultExcluded.AddRange({"cbProvince", "cbprovince"})
                If logErrors Then
                    _logger.LogInfo($"Province validation excluded for region: {regionComboBox?.Text}")
                End If
            End If

            If excludeFields IsNot Nothing Then
                defaultExcluded.AddRange(excludeFields)
            End If

            Return ValidateRequiredFields(panel, logErrors, defaultExcluded.ToArray())

        Catch ex As Exception
            If logErrors Then
                _logger.LogError($"Error in dynamic province validation: {ex.Message}")
            End If
            Return ValidateRequiredFields(panel, logErrors, excludeFields)
        End Try
    End Function

    Public Shared Function IsRfidTagUnique(rfidTag As String, Optional excludeFacultyId As Integer? = Nothing, Optional logErrors As Boolean = True) As Boolean
        Try
            If String.IsNullOrWhiteSpace(rfidTag) OrElse rfidTag.Trim() = "--" Then
                If logErrors Then
                    _logger.LogInfo("RFID validation skipped - Empty or placeholder tag")
                End If
                Return True
            End If

            Dim trimmedTag As String = rfidTag.Trim()
            Dim query As String = "SELECT COUNT(*) FROM teacherinformation WHERE tagID = ? AND isActive = 1"
            Dim parameters As New List(Of Object)
            parameters.Add(trimmedTag)

            If excludeFacultyId.HasValue Then
                query &= " AND teacherID != ?"
                parameters.Add(excludeFacultyId.Value)
                If logErrors Then
                    _logger.LogInfo($"RFID validation - Excluding faculty ID {excludeFacultyId.Value} from check")
                End If
            End If

            connectDB()
            Dim cmd As New System.Data.Odbc.OdbcCommand(query, con)

            For i As Integer = 0 To parameters.Count - 1
                cmd.Parameters.Add("?", System.Data.Odbc.OdbcType.VarChar).Value = parameters(i)
            Next

            Dim result As Integer = Convert.ToInt32(cmd.ExecuteScalar())
            con.Close()

            Dim isUnique As Boolean = (result = 0)

            If logErrors Then
                If isUnique Then
                    _logger.LogInfo($"RFID validation passed - Tag '{trimmedTag}' is unique")
                Else
                    _logger.LogWarning($"RFID validation failed - Tag '{trimmedTag}' already exists (found {result} matches)")
                End If
            End If

            If Not isUnique Then
                MessageBox.Show("This RFID tag is already in use by another faculty member. Please use a different RFID tag or scan a new one.",
                              "Duplicate RFID Tag", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If

            Return isUnique

        Catch ex As Exception
            If logErrors Then
                _logger.LogError($"Error validating RFID tag uniqueness for '{rfidTag}': {ex.Message}")
            End If

            Try
                If con IsNot Nothing AndAlso con.State = ConnectionState.Open Then
                    con.Close()
                End If
            Catch

            End Try

            MessageBox.Show("Error checking RFID tag uniqueness. Please try again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    Public Shared Function IsEmployeeIdUnique(employeeId As String, Optional excludeFacultyId As Integer? = Nothing, Optional logErrors As Boolean = True) As Boolean
        Try
            If String.IsNullOrWhiteSpace(employeeId) Then
                If logErrors Then
                    _logger.LogWarning("Employee ID validation failed - Empty employee ID")
                    MessageBox.Show("Employee ID is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End If
                Return False
            End If

            Dim trimmedId As String = employeeId.Trim()
            Dim query As String = "SELECT COUNT(*) FROM teacherinformation WHERE employeeID = ? AND isActive = 1"
            Dim parameters As New List(Of Object)
            parameters.Add(trimmedId)

            If excludeFacultyId.HasValue Then
                query &= " AND teacherID != ?"
                parameters.Add(excludeFacultyId.Value)
                If logErrors Then
                    _logger.LogInfo($"Employee ID validation - Excluding faculty ID {excludeFacultyId.Value} from check")
                End If
            End If

            connectDB()
            Dim cmd As New System.Data.Odbc.OdbcCommand(query, con)
            For i As Integer = 0 To parameters.Count - 1
                cmd.Parameters.Add("?", System.Data.Odbc.OdbcType.VarChar).Value = parameters(i)
            Next

            Dim result As Integer = Convert.ToInt32(cmd.ExecuteScalar())
            con.Close()

            Dim isUnique As Boolean = (result = 0)

            If logErrors Then
                If isUnique Then
                    _logger.LogInfo($"Employee ID validation passed - ID '{trimmedId}' is unique")
                Else
                    _logger.LogWarning($"Employee ID validation failed - ID '{trimmedId}' already exists (found {result} matches)")
                End If
            End If

            If Not isUnique Then
                MessageBox.Show("This Employee ID is already in use by another faculty member. Please use a different Employee ID.",
                              "Duplicate Employee ID", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If

            Return isUnique

        Catch ex As Exception
            If logErrors Then
                _logger.LogError($"Error validating Employee ID uniqueness for '{employeeId}': {ex.Message}")
            End If

            Try
                If con IsNot Nothing AndAlso con.State = ConnectionState.Open Then
                    con.Close()
                End If
            Catch

            End Try

            MessageBox.Show("Error checking Employee ID uniqueness. Please try again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function
End Class
