Imports System.Data.Odbc

Public Class FormAdminApproval
    Private ReadOnly _logger As ILogger = LoggerFactory.Instance

    Public Property RequesterRole As String = ""
    Public Property ActionDescription As String = ""
    Public Property ApprovedByUsername As String = ""

    Private Sub FormAdminApproval_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.StartPosition = FormStartPosition.CenterParent
            Me.TopMost = True

            lblMessage.Text = $"Admin approval is required for:{vbCrLf}{vbCrLf}" &
                             $"Action: {ActionDescription}{vbCrLf}" &
                             $"Requested by: {MainForm.currentUsername} ({RequesterRole.ToUpper()}){vbCrLf}{vbCrLf}" &
                             $"Please enter admin credentials to proceed."

            txtUsername.Focus()

        Catch ex As Exception
            _logger.LogError($"FormAdminApproval - Error loading form: {ex.Message}")
        End Try
    End Sub

    Private Sub btnApprove_Click(sender As Object, e As EventArgs) Handles btnApprove.Click
        Try
            If String.IsNullOrWhiteSpace(txtUsername.Text) Then
                MessageBox.Show("Please enter admin username.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtUsername.Focus()
                Return
            End If

            If String.IsNullOrWhiteSpace(txtPassword.Text) Then
                MessageBox.Show("Please enter admin password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtPassword.Focus()
                Return
            End If

            ' Verify admin credentials
            If VerifyAdminCredentials(txtUsername.Text.Trim(), txtPassword.Text) Then
                ApprovedByUsername = txtUsername.Text.Trim()
                _logger.LogInfo($"FormAdminApproval - Admin approval granted by {ApprovedByUsername}")
                Me.DialogResult = DialogResult.OK
                Me.Close()
            Else
                MessageBox.Show("Invalid admin credentials or insufficient privileges.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error)
                txtPassword.Clear()
                txtPassword.Focus()
            End If

        Catch ex As Exception
            _logger.LogError($"FormAdminApproval - Error approving: {ex.Message}")
            MessageBox.Show("Error verifying credentials. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function VerifyAdminCredentials(username As String, password As String) As Boolean
        Try
            connectDB()

            Dim query As String = "SELECT * FROM Logins WHERE username = ? AND password = ?"
            Dim cmd As New OdbcCommand(query, con)
            cmd.Parameters.AddWithValue("?", username)
            cmd.Parameters.AddWithValue("?", password) ' Note: In production, use hashed passwords

            Dim reader As OdbcDataReader = cmd.ExecuteReader()

            If reader.Read() Then
                Dim role As String = reader("role").ToString().ToLower()
                Dim isActive As Integer = If(IsDBNull(reader("isActive")), 0, Convert.ToInt32(reader("isActive")))
                
                reader.Close()
                con.Close()

                ' Check if user is active and has admin role
                If isActive = 0 Then
                    _logger.LogWarning($"FormAdminApproval - User '{username}' is inactive")
                    Return False
                End If

                ' Only admin role can approve
                Dim isAdmin As Boolean = (role = "admin")
                If Not isAdmin Then
                    _logger.LogWarning($"FormAdminApproval - User '{username}' has role '{role}', not admin")
                End If
                
                Return isAdmin
            End If

            reader.Close()
            con.Close()
            _logger.LogWarning($"FormAdminApproval - No user found with username '{username}'")
            Return False

        Catch ex As Exception
            _logger.LogError($"FormAdminApproval - Error verifying credentials: {ex.Message}")
            If con.State = ConnectionState.Open Then con.Close()
            Return False
        End Try
    End Function

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        _logger.LogInfo("FormAdminApproval - Approval cancelled by user")
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub txtPassword_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtPassword.KeyPress
        If e.KeyChar = ChrW(Keys.Enter) Then
            e.Handled = True
            btnApprove.PerformClick()
        End If
    End Sub

End Class
