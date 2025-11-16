Imports System.Data.Odbc
Imports System.Windows.Forms.DataVisualization.Charting
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Threading.Tasks

Public Class MainForm
    Private ReadOnly _logger As ILogger = LoggerFactory.Instance
    Private ReadOnly _updateManager As UpdateManager
    Public currentChild As Form
    Public currentButton As Button
    Public currentUserRole As String = ""
    Public Shared currentUsername As String = ""
    Private isLoggingOut As Boolean = False

    Public Sub New()
        InitializeComponent()
        _updateManager = New UpdateManager(_logger)
    End Sub
    Private Sub OpenForm(ByVal childForm As Form, ByVal isMaximized As Boolean)
        Try
            If currentChild IsNot Nothing Then
                currentChild.Close()
            End If
            currentChild = childForm
            Me.IsMdiContainer = True
            childForm.MdiParent = Me
            childForm.FormBorderStyle = FormBorderStyle.None
            childForm.Dock = DockStyle.Fill
            childForm.WindowState = If(isMaximized, FormWindowState.Maximized, FormWindowState.Normal)
            'PictureBox1.Hide()
            childForm.Show()
        Catch ex As Exception
            MessageBox.Show(ex.Message.ToString())
        End Try
    End Sub

    Public Sub HighlightButton(selectedButton As Button)
        If currentButton IsNot Nothing Then
            currentButton.BackColor = Color.White
            currentButton.ForeColor = Color.DimGray
        End If

        selectedButton.BackColor = Color.DeepSkyBlue
        selectedButton.ForeColor = Color.White
        currentButton = selectedButton
    End Sub

    Private Sub AttendanceToolStripMenuItem1_Click(sender As Object, e As EventArgs)
        FormReportsAttendance.ShowDialog()
    End Sub

    Public Sub FacultyAttendanceLineGraph()
        Dim attendanceDict As New Dictionary(Of Date, Integer)
        For i As Integer = 8 To 0 Step -1
            attendanceDict.Add(Date.Today.AddDays(-i), 0)
        Next

        Dim sql As String = "SELECT `logDate`, COUNT(DISTINCT teacherID) AS count " &
                        "FROM attendance_record " &
                        "WHERE `logDate` >= ? " &
                        "GROUP BY `logDate`"

        Try
            connectDB()
            Using cmd As New OdbcCommand(sql, con)
                cmd.Parameters.AddWithValue("?", Date.Today.AddDays(-6))

                Using reader As OdbcDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim logDate As Date = Convert.ToDateTime(reader("logDate"))
                        Dim count As Integer = Convert.ToInt32(reader("count"))

                        If attendanceDict.ContainsKey(logDate) Then
                            attendanceDict(logDate) = count
                        End If
                    End While
                End Using
            End Using

            Dim chartArea As New ChartArea("MainArea")
            chartArea.AxisX.LabelStyle.Format = "MMM dd"
            chartArea.AxisX.Interval = 1
            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray
            'attendanceChart.ChartAreas.Add(chartArea)

            Dim series As New Series("Weekly Attendance")
            series.ChartType = SeriesChartType.Line
            series.XValueType = ChartValueType.Date
            series.BorderWidth = 3
            series.IsValueShownAsLabel = True
            series.MarkerStyle = MarkerStyle.Circle
            series.MarkerSize = 6

            For Each kvp In attendanceDict
                series.Points.AddXY(kvp.Key, kvp.Value)
            Next

            'attendanceChart.Series.Add(series)

        Catch ex As Exception
            MessageBox.Show("Error loading chart: " & ex.Message)
        Finally
            If con IsNot Nothing AndAlso con.State = ConnectionState.Open Then
                con.Close()
            End If
        End Try
    End Sub

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim config = AppConfig.Instance
        Try
            Me.Text = $"{APP_NAME} v{APP_VERSION} - {config.Environment}"

            Timer1.Enabled = True

            ApplyRoleBasedAccess()
            CheckForUpdatesOnStartup()

            _logger.LogInfo($"MainForm loaded for user role: {currentUserRole}")
        Catch ex As Exception
            _logger.LogError($"Error in MainForm_Load: {ex.Message}")
        End Try
    End Sub

    Private Sub ApplyRoleBasedAccess()
        Try
            _logger.LogInfo($"Applying role-based access control for role: {currentUserRole}")

            If currentUserRole.ToLower() <> "admin" Then
                AdminToolStripMenuItem.Visible = False
                tsManageAccounts.Visible = False
                _logger.LogInfo($"Administration menu hidden for role: {currentUserRole}")
            Else
                AdminToolStripMenuItem.Visible = True
                tsManageAccounts.Visible = True
                _logger.LogInfo($"Administration menu visible for admin role")
            End If

        Catch ex As Exception
            _logger.LogError($"Error applying role-based access control: {ex.Message}")
        End Try
    End Sub

    Private Async Sub CheckForUpdatesOnStartup()
        Try
            _logger.LogInfo("Starting update check on application startup")

            Await Task.Run(Sub()
                               Try
                                   _updateManager.CheckForUpdatesOnStartup(Me)
                               Catch ex As Exception
                                   _logger.LogError($"Background update check error: {ex.Message}")
                               End Try
                           End Sub)
        Catch ex As Exception
            _logger.LogError($"Error during startup update check: {ex.Message}")
        End Try
    End Sub

    Public Async Sub CheckForUpdatesManually()
        Try
            _logger.LogInfo("Manual update check initiated")

            Me.Cursor = Cursors.WaitCursor
            Dim hasUpdate As Boolean = Await _updateManager.CheckForUpdatesAsync(Me)
            Me.Cursor = Cursors.Default

            If Not hasUpdate Then
                MessageBox.Show("Your application is up to date.", "No Updates Available", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            Me.Cursor = Cursors.Default
            _logger.LogError($"Error during manual update check: {ex.Message}")
            MessageBox.Show("Unable to check for updates. Please try again later.", "Update Check Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Function FacultyCount() As Integer
        Dim sql As String = "SELECT COUNT(*) FROM teacherinformation WHERE isActive > 0"

        Try
            connectDB()
            Using cmd As New Odbc.OdbcCommand(sql, con)
                Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
                Return count
            End Using
        Catch ex As Exception
            MessageBox.Show("Error fetching faculty count: " & ex.Message)
            Return -1
        Finally
            con.Close()
            GC.Collect()
        End Try
    End Function

    Function AttendanceCount() As Integer
        Dim sql As String = "SELECT COUNT(*) FROM attendance_record WHERE DATE(logDate) = CURDATE()"

        Try
            connectDB()
            Using cmd As New Odbc.OdbcCommand(sql, con)
                Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
                Return count
            End Using
        Catch ex As Exception
            MessageBox.Show("Error fetching attendance count: " & ex.Message)
            Return -1
        Finally
            con.Close()
            GC.Collect()
        End Try
    End Function



    Private Sub MainForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            Dim userName As String = lblUser.Text.Replace("Logged in as: ", "")
            If userName.Contains("(") Then
                userName = userName.Substring(0, userName.IndexOf("(")).Trim()
            End If

            _logger.LogInfo($"MainForm closing - User: '{userName}' ({currentUserRole})")
            If isLoggingOut Then
                _logger.LogInfo($"User '{userName}' ({currentUserRole}) is logging out - skipping exit confirmation")
                isLoggingOut = False
                Return
            End If
            If e.CloseReason = CloseReason.UserClosing Then
                Dim result As DialogResult = MessageBox.Show(
                    "Are you sure you want to exit the application?",
                    "Confirm Exit",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question)

                If result = DialogResult.No Then
                    e.Cancel = True
                    _logger.LogInfo($"User '{userName}' ({currentUserRole}) cancelled application exit")
                    Return
                End If

                _logger.LogInfo($"User '{userName}' ({currentUserRole}) exited application via X button")
            End If

            Try
                If con IsNot Nothing AndAlso con.State = ConnectionState.Open Then
                    con.Close()
                End If
            Catch
            End Try

            If currentChild IsNot Nothing Then
                currentChild.Close()
                currentChild = Nothing
            End If

            For Each frm As Form In Application.OpenForms.Cast(Of Form)().ToList()
                If frm IsNot Me AndAlso Not frm.IsDisposed AndAlso Not TypeOf frm Is RFIDScanMonitor Then
                    Try
                        frm.Close()
                    Catch
                    End Try
                End If
            Next

            _logger.LogInfo("MainForm closed successfully - Application exiting")

        Catch ex As Exception
            _logger.LogError("Error during MainForm closing", ex)
            Application.Exit()
        End Try
    End Sub

    Private Sub btnAnnouncement_Click(sender As Object, e As EventArgs)
        OpenForm(New FormAnnouncement, True)
    End Sub

    Private Sub SubjectsToolStripMenuItem_Click(sender As Object, e As EventArgs)
        OpenForm(New FormSubjects, False)
    End Sub

    Private Sub ManageUserToolStripMenuItem_Click(sender As Object, e As EventArgs)
        OpenForm(New ManageUser, True)
    End Sub

    Private Sub LogOut()
        Try
            _logger.LogInfo($"User '{labelCurrentUser.Text}' initiated logout (legacy method)")

            If MsgBox("Are you sure you want to Log Out?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Log Out") = DialogResult.Yes Then
                _logger.LogInfo($"User '{labelCurrentUser.Text}' logged out successfully")

                isLoggingOut = True

                If currentChild IsNot Nothing Then
                    currentChild.Close()
                    currentChild = Nothing
                End If

                For Each frm As Form In Application.OpenForms.Cast(Of Form).ToList()
                    If frm IsNot Me AndAlso frm IsNot LoginForm AndAlso Not TypeOf frm Is RFIDScanMonitor Then
                        Try
                            frm.Close()
                        Catch

                        End Try
                    End If
                Next

                Me.Hide()
                LoginForm.Show()
                LoginForm.ttxtUser.Clear()
                LoginForm.ttxtPass.Clear()
                LoginForm.ttxtUser.Focus()
            Else
                _logger.LogInfo($"User '{labelCurrentUser.Text}' cancelled logout")
            End If
        Catch ex As Exception
            _logger.LogError("Error in LogOut method", ex)
            MessageBox.Show($"An error occurred during logout: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub btnLogout_Click(sender As Object, e As EventArgs)
        LogOut()
    End Sub


    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Dim faculty As Integer = FacultyCount()
        Dim attendance As Integer = AttendanceCount()
        FacultyAttendanceLineGraph()
    End Sub

    Private Sub ReportsToolStripMenuItem_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub LogOutToolStripMenuItem_Click(sender As Object, e As EventArgs)
        Me.Close()
        FormFaculty.Close()
        FormAttendace.Close()
        ManageUser.Close()
        LoginForm.Show()
    End Sub

    Private Sub AnnouncementToolStripMenuItem_Click(sender As Object, e As EventArgs)
        FormAnnouncement.Show()
    End Sub

    Private Sub FacultyDataToolStripMenuItem2_Click(sender As Object, e As EventArgs)
        Dim modalForm As New FormFaculty()
        modalForm.ShowDialog(Me)
        FormAttendace.Close()
        ManageUser.Close()
    End Sub

    Private Sub ManageAccountsToolStripMenuItem1_Click(sender As Object, e As EventArgs)
        ManageUser.Show()
        FormFaculty.Close()
        FormAttendace.Close()
    End Sub

    Private Sub AttendanceRecordsToolStripMenuItem1_Click(sender As Object, e As EventArgs)
        FormAttendace.Show()
        FormFaculty.Close()
        ManageUser.Close()
    End Sub

    Private Sub GenerateReportsToolStripMenuItem1_Click(sender As Object, e As EventArgs)
        FormReportsAttendance.ShowDialog()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs)
        FormAttendace.Show()
        FormReportsAttendance.Close()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs)
        FormReportsAttendance.ShowDialog()
        FormAttendace.Close()
    End Sub

    Private Sub tsBtnFaculty_Click(sender As Object, e As EventArgs) Handles tsBtnFaculty.Click
        Dim modalForm As New FormFaculty()
        modalForm.ShowDialog(Me)
        FormAttendace.Close()
        ManageUser.Close()
    End Sub

    Private Sub tsBtnAttendance_Click(sender As Object, e As EventArgs) Handles tsBtnAttendance.Click
        FormAttendace.Show()
        FormFaculty.Close()
        ManageUser.Close()
    End Sub

    Private Sub tsLogs_Click(sender As Object, e As EventArgs) Handles tsLogs.Click
        FormUserActivityLogs.ShowDialog()
    End Sub

    Private Sub tsFaculty_Click(sender As Object, e As EventArgs) Handles tsFaculty.Click
        FormFacultyList.ShowDialog()
    End Sub

    Private Sub tsManageAccounts_Click(sender As Object, e As EventArgs) Handles tsManageAccounts.Click
        ManageUser.Show()
        FormFaculty.Close()
        FormAttendace.Close()
    End Sub

    Private Sub tsAnnouncements_Click(sender As Object, e As EventArgs)
        FormAnnouncement.Show()
    End Sub

    Private Sub msLogout_Click(sender As Object, e As EventArgs) Handles msLogout.Click
        Try
            Dim userName As String = lblUser.Text.Replace("Logged in as: ", "")
            If userName.Contains("(") Then
                userName = userName.Substring(0, userName.IndexOf("(")).Trim()
            End If

            _logger.LogInfo($"User '{userName}' ({currentUserRole}) initiated logout")

            Dim result As DialogResult = MessageBox.Show(
                "Are you sure you want to log out?",
                "Confirm Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)

            If result = DialogResult.Yes Then
                _logger.LogInfo($"User '{userName}' ({currentUserRole}) logged out successfully")
                isLoggingOut = True
                If currentChild IsNot Nothing Then
                    currentChild.Close()
                    currentChild = Nothing
                End If

                For Each frm As Form In Application.OpenForms.Cast(Of Form).ToList()
                    If frm IsNot Me AndAlso frm IsNot LoginForm AndAlso Not TypeOf frm Is RFIDScanMonitor Then
                        Try
                            frm.Close()
                        Catch
                        End Try
                    End If
                Next

                Me.Hide()
                LoginForm.Show()
                LoginForm.ttxtUser.Clear()
                LoginForm.ttxtPass.Clear()
                LoginForm.ttxtUser.Focus()
            Else
                _logger.LogInfo($"User '{userName}' ({currentUserRole}) cancelled logout")
            End If
        Catch ex As Exception
            _logger.LogError("Error during logout", ex)
            MessageBox.Show($"An error occurred during logout: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub msExit_Click(sender As Object, e As EventArgs) Handles msExit.Click
        Try
            Dim userName As String = lblUser.Text.Replace("Logged in as: ", "")
            If userName.Contains("(") Then
                userName = userName.Substring(0, userName.IndexOf("(")).Trim()
            End If

            _logger.LogInfo($"User '{userName}' ({currentUserRole}) initiated application exit")

            Dim result As DialogResult = MessageBox.Show(
                "Are you sure you want to exit the application?",
                "Confirm Exit",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)

            If result = DialogResult.Yes Then
                _logger.LogInfo($"User '{userName}' ({currentUserRole}) exited application")

                Try
                    If con IsNot Nothing AndAlso con.State = ConnectionState.Open Then
                        con.Close()
                    End If
                Catch
                End Try

                Application.Exit()
            Else
                _logger.LogInfo($"User '{userName}' ({currentUserRole}) cancelled exit")
            End If
        Catch ex As Exception
            _logger.LogError("Error during application exit", ex)
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Application.Exit()
        End Try
    End Sub

    Private Sub ManageDepartmentToolStripMenuItem_Click(sender As Object, e As EventArgs)
        Try
            _logger.LogInfo($"ManageDepartment menu clicked by user role: {currentUserRole}")

            If currentUserRole.ToLower() <> "admin" Then
                _logger.LogWarning($"Unauthorized access attempt to Department Management by role: {currentUserRole}")
                MessageBox.Show("Access denied. Only administrators can manage departments.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Using deptForm As New FormDepartments()
                _logger.LogInfo("Opening Department Management form")
                deptForm.ShowDialog(Me)
                _logger.LogInfo("Department Management form closed")
            End Using

        Catch ex As Exception
            _logger.LogError($"Error opening Department Management form: {ex.Message}")
            MessageBox.Show("Error opening Department Management. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Public Sub SetUserRole(role As String)
        Try
            currentUserRole = role
            _logger.LogInfo($"User role set to: {role}")

            If Me.IsHandleCreated Then
                ApplyRoleBasedAccess()
            End If
        Catch ex As Exception
            _logger.LogError($"Error setting user role: {ex.Message}")
        End Try
    End Sub

    Private Sub UserActivityToolStripMenuItem_Click(sender As Object, e As EventArgs)
        FormUserActivityLogs.ShowDialog()
    End Sub

    Private Sub AdministrationToolStripMenuItem_Click(sender As Object, e As EventArgs)
        _logger.LogDebug("Administration menu clicked - showing dropdown items")
    End Sub

    Private Sub AuditLogsToolStripMenuItem_Click(sender As Object, e As EventArgs)
        _logger.LogDebug("Audit Logs menu clicked - showing dropdown items")
    End Sub

    Private Sub ManageDepartmentToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles ManageDepartmentToolStripMenuItem1.Click
        Try
            _logger.LogInfo($"ManageDepartment menu clicked by user role: {currentUserRole}")

            If currentUserRole.ToLower() <> "admin" Then
                _logger.LogWarning($"Unauthorized access attempt to Department Management by role: {currentUserRole}")
                MessageBox.Show("Access denied. Only administrators can manage departments.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Using deptForm As New FormDepartments()
                _logger.LogInfo("Opening Department Management form")
                deptForm.ShowDialog(Me)
                _logger.LogInfo("Department Management form closed")
            End Using

        Catch ex As Exception
            _logger.LogError($"Error opening Department Management form: {ex.Message}")
            MessageBox.Show("Error opening Department Management. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub AuditLogsToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles AuditLogsToolStripMenuItem1.Click
        FormUserActivityLogs.ShowDialog()
    End Sub

    Private Sub SystemBackupToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SystemBackupToolStripMenuItem.Click
        Try
            Using frm As New FormBackup()
                frm.ShowDialog(Me)
            End Using
        Catch ex As Exception
            _logger.LogError($"Error opening Backup/Restore form: {ex.Message}")
            MessageBox.Show("Unable to open Backup/Restore form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub DailyAttendanceToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DailyAttendanceToolStripMenuItem.Click
        FormDailyAttendance.ShowDialog()
    End Sub
End Class
