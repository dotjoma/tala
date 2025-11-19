Imports System.IO
Imports System.Data.Odbc
Imports System.Drawing

Public Class FormFacultyDetails
    Private ReadOnly _logger As ILogger = LoggerFactory.Instance
    Private _facultyId As Integer = 0

    Public Sub New(facultyId As Integer)
        InitializeComponent()
        _facultyId = facultyId
    End Sub

    Private Sub FormFacultyDetails_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            _logger.LogInfo($"FormFacultyDetails - Loading details for Faculty ID: {_facultyId}")
            LoadFacultyDetails()
        Catch ex As Exception
            _logger.LogError($"FormFacultyDetails - Error loading form: {ex.Message}")
            MessageBox.Show("Error loading faculty details: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Me.Close()
        End Try
    End Sub

    Private Sub LoadFacultyDetails()
        Dim cmd As OdbcCommand = Nothing
        Dim reader As OdbcDataReader = Nothing

        Try
            connectDB()

            Dim query As String = "
                SELECT 
                    ti.teacherID,
                    ti.profileImg,
                    ti.employeeID,
                    ti.tagID,
                    ti.firstname,
                    ti.middlename,
                    ti.lastname,
                    ti.extName,
                    ti.email,
                    ti.gender,
                    ti.birthdate,
                    ti.phoneNo,
                    ti.contactNo,
                    ti.homeadd,
                    ti.emergencyContact,
                    ti.relationship,
                    ti.isActive,
                    ti.shift_start_time,
                    ti.shift_end_time,
                    COALESCE(d.department_name, 'N/A') AS department_name,
                    COALESCE(d.department_code, 'N/A') AS department_code,
                    COALESCE(rg.regDesc, '') AS region_name,
                    COALESCE(rp.provDesc, '') AS province_name,
                    COALESCE(rc.citymunDesc, '') AS city_name,
                    COALESCE(rb.brgyDesc, '') AS barangay_name
                FROM teacherinformation ti
                LEFT JOIN departments d ON ti.department_id = d.department_id
                LEFT JOIN refregion rg ON ti.regionID = rg.id
                LEFT JOIN refprovince rp ON ti.provinceID = rp.id
                LEFT JOIN refcitymun rc ON ti.cityID = rc.id
                LEFT JOIN refbrgy rb ON ti.brgyID = rb.id
                WHERE ti.teacherID = ?"

            cmd = New OdbcCommand(query, con)
            cmd.Parameters.Add("?", OdbcType.Int).Value = _facultyId
            reader = cmd.ExecuteReader()

            If reader.Read() Then
                ' Personal Information
                lblEmployeeIDValue.Text = If(IsDBNull(reader("employeeID")), "N/A", reader("employeeID").ToString())
                lblTagIDValue.Text = If(IsDBNull(reader("tagID")), "N/A", reader("tagID").ToString())

                Dim fullName As String = FormatFullName(
                    If(IsDBNull(reader("firstname")), "", reader("firstname").ToString()),
                    If(IsDBNull(reader("middlename")), "", reader("middlename").ToString()),
                    If(IsDBNull(reader("lastname")), "", reader("lastname").ToString()),
                    If(IsDBNull(reader("extName")), "", reader("extName").ToString())
                )
                lblFullNameValue.Text = fullName

                lblFirstNameValue.Text = If(IsDBNull(reader("firstname")), "N/A", reader("firstname").ToString())
                lblMiddleNameValue.Text = If(IsDBNull(reader("middlename")), "N/A", reader("middlename").ToString())
                lblLastNameValue.Text = If(IsDBNull(reader("lastname")), "N/A", reader("lastname").ToString())
                lblExtNameValue.Text = If(IsDBNull(reader("extName")), "N/A", reader("extName").ToString())

                lblGenderValue.Text = If(IsDBNull(reader("gender")), "N/A", reader("gender").ToString())

                If Not IsDBNull(reader("birthdate")) Then
                    Dim birthDate As DateTime = Convert.ToDateTime(reader("birthdate"))
                    lblBirthdateValue.Text = birthDate.ToString("MMMM dd, yyyy")
                    lblAgeValue.Text = CalculateAge(birthDate).ToString() & " years old"
                Else
                    lblBirthdateValue.Text = "N/A"
                    lblAgeValue.Text = "N/A"
                End If

                ' Contact Information
                lblEmailValue.Text = If(IsDBNull(reader("email")), "N/A", reader("email").ToString())
                lblPhoneNoValue.Text = If(IsDBNull(reader("phoneNo")), "N/A", reader("phoneNo").ToString())
                lblContactNoValue.Text = If(IsDBNull(reader("contactNo")), "N/A", reader("contactNo").ToString())

                ' Address Information
                Dim homeAddress As String = If(IsDBNull(reader("homeadd")), "", reader("homeadd").ToString())
                Dim barangay As String = If(IsDBNull(reader("barangay_name")), "", reader("barangay_name").ToString())
                Dim city As String = If(IsDBNull(reader("city_name")), "", reader("city_name").ToString())
                Dim province As String = If(IsDBNull(reader("province_name")), "", reader("province_name").ToString())
                Dim region As String = If(IsDBNull(reader("region_name")), "", reader("region_name").ToString())

                lblHomeAddressValue.Text = If(String.IsNullOrWhiteSpace(homeAddress), "N/A", homeAddress)
                lblBarangayValue.Text = If(String.IsNullOrWhiteSpace(barangay), "N/A", barangay)
                lblCityValue.Text = If(String.IsNullOrWhiteSpace(city), "N/A", city)
                lblProvinceValue.Text = If(String.IsNullOrWhiteSpace(province), "N/A", province)
                lblRegionValue.Text = If(String.IsNullOrWhiteSpace(region), "N/A", region)

                ' Emergency Contact
                lblEmergencyContactValue.Text = If(IsDBNull(reader("emergencyContact")), "N/A", reader("emergencyContact").ToString())
                lblRelationshipValue.Text = If(IsDBNull(reader("relationship")), "N/A", reader("relationship").ToString())

                ' Department Information
                lblDepartmentValue.Text = reader("department_name").ToString()
                lblDepartmentCodeValue.Text = reader("department_code").ToString()

                ' Work Schedule / Shift Information
                Dim shiftStartTime As Object = reader("shift_start_time")
                Dim shiftEndTime As Object = reader("shift_end_time")
                
                If Not IsDBNull(shiftStartTime) AndAlso Not IsDBNull(shiftEndTime) Then
                    Dim startTime As TimeSpan = CType(shiftStartTime, TimeSpan)
                    Dim endTime As TimeSpan = CType(shiftEndTime, TimeSpan)
                    
                    ' Format time to 12-hour format
                    Dim startTimeFormatted As String = FormatTimeSpan(startTime)
                    Dim endTimeFormatted As String = FormatTimeSpan(endTime)
                    
                    lblShiftStartValue.Text = startTimeFormatted
                    lblShiftEndValue.Text = endTimeFormatted
                    
                    ' Determine shift type (Morning/Afternoon/Evening)
                    lblShiftTypeValue.Text = DetermineShiftType(startTime, endTime)
                Else
                    lblShiftStartValue.Text = "Not Set"
                    lblShiftEndValue.Text = "Not Set"
                    lblShiftTypeValue.Text = "Not Assigned"
                End If

                ' Status
                Dim isActive As Boolean = If(IsDBNull(reader("isActive")), False, Convert.ToBoolean(reader("isActive")))
                lblStatusValue.Text = If(isActive, "Active", "Inactive")
                lblStatusValue.ForeColor = If(isActive, Color.Green, Color.Red)

                ' Load Profile Image
                Try
                    If Not IsDBNull(reader("profileImg")) Then
                        Dim profileImg As Byte() = DirectCast(reader("profileImg"), Byte())
                        If profileImg IsNot Nothing AndAlso profileImg.Length > 0 Then
                            Using ms As New MemoryStream(profileImg)
                                pbProfile.Image = Image.FromStream(ms)
                                pbProfile.SizeMode = PictureBoxSizeMode.Zoom
                            End Using
                            _logger.LogInfo($"FormFacultyDetails - Profile image loaded successfully")
                        Else
                            SetDefaultProfileImage()
                        End If
                    Else
                        SetDefaultProfileImage()
                    End If
                Catch ex As Exception
                    _logger.LogWarning($"FormFacultyDetails - Could not load profile image: {ex.Message}")
                    SetDefaultProfileImage()
                End Try

                _logger.LogInfo($"FormFacultyDetails - Faculty details loaded successfully for: {fullName}")
            Else
                _logger.LogWarning($"FormFacultyDetails - No faculty found with ID: {_facultyId}")
                MessageBox.Show("Faculty member not found.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Me.Close()
            End If

        Catch ex As Exception
            _logger.LogError($"FormFacultyDetails - Error loading faculty details: {ex.Message}")
            Throw
        Finally
            If reader IsNot Nothing AndAlso Not reader.IsClosed Then
                reader.Close()
            End If
            If cmd IsNot Nothing Then
                cmd.Dispose()
            End If
            If con IsNot Nothing AndAlso con.State = ConnectionState.Open Then
                con.Close()
            End If
        End Try
    End Sub

    Private Function CalculateAge(birthDate As DateTime) As Integer
        Dim today As DateTime = DateTime.Today
        Dim age As Integer = today.Year - birthDate.Year
        If birthDate.Date > today.AddYears(-age) Then
            age -= 1
        End If
        Return age
    End Function

    Private Function FormatFullName(firstName As String, middleName As String, lastName As String, extName As String) As String
        Dim nameParts As New List(Of String)

        If Not String.IsNullOrWhiteSpace(lastName) Then
            nameParts.Add(lastName.Trim())
        End If

        ' Only add suffix if it's not null, empty, or "--"
        If Not String.IsNullOrWhiteSpace(extName) AndAlso extName.Trim() <> "--" Then
            nameParts.Add(extName.Trim())
        End If

        If Not String.IsNullOrWhiteSpace(firstName) Then
            If nameParts.Count > 0 Then
                nameParts.Add(",")
            End If
            nameParts.Add(firstName.Trim())
        End If

        If Not String.IsNullOrWhiteSpace(middleName) Then
            nameParts.Add(middleName.Trim())
        End If

        Return String.Join(" ", nameParts)
    End Function

    Private Function FormatTimeSpan(timeValue As TimeSpan) As String
        Try
            Dim hours As Integer = timeValue.Hours
            Dim minutes As Integer = timeValue.Minutes
            Dim period As String = If(hours >= 12, "PM", "AM")
            
            ' Convert to 12-hour format
            If hours = 0 Then
                hours = 12
            ElseIf hours > 12 Then
                hours = hours - 12
            End If
            
            Return $"{hours:D2}:{minutes:D2} {period}"
        Catch ex As Exception
            _logger.LogError($"FormFacultyDetails - Error formatting time: {ex.Message}")
            Return "Invalid Time"
        End Try
    End Function

    Private Function DetermineShiftType(startTime As TimeSpan, endTime As TimeSpan) As String
        Try
            Dim startHour As Integer = startTime.Hours
            
            ' Morning Shift: 6:00 AM - 11:59 AM
            If startHour >= 6 AndAlso startHour < 12 Then
                Return "Morning Shift"
            ' Afternoon Shift: 12:00 PM - 5:59 PM
            ElseIf startHour >= 12 AndAlso startHour < 18 Then
                Return "Afternoon Shift"
            ' Evening/Night Shift: 6:00 PM - 5:59 AM
            Else
                Return "Evening Shift"
            End If
        Catch ex As Exception
            _logger.LogError($"FormFacultyDetails - Error determining shift type: {ex.Message}")
            Return "Unknown Shift"
        End Try
    End Function

    Private Sub SetDefaultProfileImage()
        Try
            ' Create a default gray placeholder image with user icon text
            Dim bmp As New Bitmap(200, 200)
            Using g As Graphics = Graphics.FromImage(bmp)
                ' Fill with light gray background
                g.FillRectangle(New SolidBrush(Color.FromArgb(189, 195, 199)), 0, 0, 200, 200)

                ' Draw a simple user icon circle
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                g.FillEllipse(New SolidBrush(Color.FromArgb(149, 165, 166)), 60, 40, 80, 80)

                ' Draw body
                Dim bodyPath As New Drawing2D.GraphicsPath()
                bodyPath.AddEllipse(30, 100, 140, 120)
                g.FillPath(New SolidBrush(Color.FromArgb(149, 165, 166)), bodyPath)

                ' Draw "No Photo" text
                Dim font As New Font("Segoe UI", 10, FontStyle.Bold)
                Dim textBrush As New SolidBrush(Color.FromArgb(127, 140, 141))
                Dim text As String = "No Photo"
                Dim textSize As SizeF = g.MeasureString(text, font)
                g.DrawString(text, font, textBrush, (200 - textSize.Width) / 2, 170)
            End Using

            pbProfile.Image = bmp
            pbProfile.SizeMode = PictureBoxSizeMode.Zoom
            _logger.LogInfo("FormFacultyDetails - Default profile image set")
        Catch ex As Exception
            _logger.LogError($"FormFacultyDetails - Error creating default profile image: {ex.Message}")
        End Try
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        _logger.LogInfo("FormFacultyDetails - Close button clicked")
        Me.Close()
    End Sub
End Class
