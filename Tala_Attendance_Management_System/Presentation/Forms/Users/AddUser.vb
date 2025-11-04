Imports System.Data.Odbc
Imports System.Data.SqlClient

Public Class AddUser
    Public userID As Integer = 0
    Private copiedCooldown = 15
    Private ReadOnly _auditLogger As AuditLogger = AuditLogger.Instance

    Function GetUserID() As Integer
        Try
            Dim dbContext As New DatabaseContext()
            Dim result = dbContext.ExecuteScalar("SELECT MAX(login_id) FROM logins")

            If Not IsDBNull(result) AndAlso result IsNot Nothing Then
                Return Convert.ToInt32(result)
            Else
                Return 0
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message)
            Return 0
        End Try
    End Function

    Private Function IsUsernameUnique(username As String, Optional excludeUserID As Integer = 0) As Boolean
        Try
            Dim dbContext As New DatabaseContext()
            Dim query As String = "SELECT COUNT(*) FROM logins WHERE username = ? AND login_id <> ? AND isActive=1"
            Dim result As Integer = Convert.ToInt32(dbContext.ExecuteScalar(query, username, excludeUserID))
            Return result = 0
        Catch ex As Exception
            MessageBox.Show("Error checking for duplicate username: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function


    Private Sub ResetFormState()
        txtUsername.Clear()
        txtPassword.Clear()
        txtID.Text = "0"
        btnCancel.Text = "Cancel"
        userID = 0
    End Sub

    Private Sub AddUser_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If cboUserRole.Items.Count = 0 Then
            cboUserRole.Items.Clear()
            cboUserRole.Items.Add("hr")
            cboUserRole.Items.Add("admin")
        End If

        If userID = 0 Then
            btnCopy.Visible = False

            lblTitle.Text = "Add User"
            cboUserRole.SelectedIndex = 0
            chkShowPassword.Enabled = True
            txtPassword.Enabled = True
            btnGenerate.Enabled = True

            lblPassword.Visible = True
            chkShowPassword.Visible = True
            txtPassword.Visible = True
            btnGenerate.Visible = True
        Else
            btnCopy.Visible = True

            lblTitle.Text = "Edit User"
            chkShowPassword.Enabled = True
            txtPassword.Enabled = False
            btnGenerate.Enabled = False

            lblPassword.Visible = True
            chkShowPassword.Visible = True
            txtPassword.Visible = True
            btnGenerate.Visible = False
        End If
    End Sub
    Private Function ValidatePasswordComplexity(password As String, ByRef errorMessage As String) As Boolean
        Dim missing As New List(Of String)

        ' Check length
        If password.Length < 13 Then
            missing.Add($"at least 13 characters (currently {password.Length})")
        End If

        ' Check for lowercase
        If Not password.Any(AddressOf Char.IsLower) Then
            missing.Add("lowercase letter (a-z)")
        End If

        ' Check for uppercase
        If Not password.Any(AddressOf Char.IsUpper) Then
            missing.Add("uppercase letter (A-Z)")
        End If

        ' Check for digit
        If Not password.Any(AddressOf Char.IsDigit) Then
            missing.Add("number (0-9)")
        End If

        ' Check for special character
        If Not password.Any(Function(c) Not Char.IsLetterOrDigit(c)) Then
            missing.Add("special character (!@#$%^&*()-_=+[]{}|;:,.<>?/`~)")
        End If

        If missing.Count > 0 Then
            errorMessage = "Password is missing: " & String.Join(", ", missing)
            Return False
        End If

        errorMessage = ""
        Return True
    End Function
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Dim cmd As Odbc.OdbcCommand
        Dim user_id As Integer = GetUserID()

        If Not IsUsernameUnique(txtUsername.Text, If(userID = 0, 0, userID)) Then
            MessageBox.Show("Username must be unique.", "Duplicate username", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        Dim passwordError As String = ""
        If Not ValidatePasswordComplexity(txtPassword.Text, passwordError) Then
            MessageBox.Show(passwordError, "Password Requirements", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If fieldChecker(panelContainer) = True Then
            Try
                connectDB()
                If userID = 0 Then
                    If cboUserRole.SelectedIndex = -1 Then
                        MessageBox.Show("Please select a user role.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        Return
                    End If

                    cmd = New Odbc.OdbcCommand("INSERT INTO logins(fullname, username, password, email, address, role, created_at) VALUES(?,?,?,?,?,?,?)", con)
                    With cmd.Parameters
                        .AddWithValue("?", Trim(txtName.Text))
                        .AddWithValue("?", Trim(txtUsername.Text))
                        .AddWithValue("?", Trim(txtPassword.Text))
                        .AddWithValue("?", Trim(txtEmail.Text))
                        .AddWithValue("?", Trim(txtAddress.Text))
                        .AddWithValue("?", LCase(cboUserRole.Text))
                        .AddWithValue("?", DateAndTime.Now)
                    End With
                    cmd.ExecuteNonQuery()

                    _auditLogger.LogCreate(MainForm.currentUsername, "User Account",
                        $"Created user account '{Trim(txtUsername.Text)}' with role '{cboUserRole.Text}' for {Trim(txtName.Text)}")

                    MsgBox("Created user successfully", MsgBoxStyle.Information, "Success")
                Else
                    If cboUserRole.SelectedIndex = -1 Then
                        MessageBox.Show("Please select a user role.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        Return
                    End If

                    cmd = New Odbc.OdbcCommand("UPDATE logins SET fullname=?, username=?, password=?, email=?, address=?, role=? WHERE login_id=?", con)
                    With cmd.Parameters
                        .AddWithValue("?", Trim(txtName.Text))
                        .AddWithValue("?", Trim(txtUsername.Text))
                        .AddWithValue("?", Trim(txtPassword.Text))
                        .AddWithValue("?", Trim(txtEmail.Text))
                        .AddWithValue("?", Trim(txtAddress.Text))
                        .AddWithValue("?", LCase(cboUserRole.Text))
                        .AddWithValue("?", userID)
                    End With
                    cmd.ExecuteNonQuery()
                    _auditLogger.LogUpdate(MainForm.currentUsername, "User Account",
                        $"Updated user account '{Trim(txtUsername.Text)}' (ID: {userID}) - Role: {cboUserRole.Text}")

                    MsgBox("Updated successfully", MsgBoxStyle.Information, "Success")
                    Me.Close()
                End If

            Catch ex As Exception
                MsgBox("Error: " & ex.Message, MsgBoxStyle.Critical, "Error")
            Finally
                con.Close()
                GC.Collect()
            End Try

            ClearFields(panelContainer)
            btnCancel.Text = "Close"
            userID = 0
        End If
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub

    Private Sub btnGenerate_Click(sender As Object, e As EventArgs) Handles btnGenerate.Click
        Dim passwordLength As Integer = 13
        Dim lowercase As String = "abcdefghijklmnopqrstuvwxyz"
        Dim uppercase As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
        Dim digits As String = "0123456789"
        Dim specialChars As String = "!@#$%^&*()-_=+[]{}|;:,.<>?/`~"
        Dim allChars As String = lowercase & uppercase & digits & specialChars
        Dim random As New Random()
        Dim password As New System.Text.StringBuilder()

        password.Append(lowercase(random.Next(0, lowercase.Length)))
        password.Append(uppercase(random.Next(0, uppercase.Length)))
        password.Append(digits(random.Next(0, digits.Length)))
        password.Append(specialChars(random.Next(0, specialChars.Length)))

        For i As Integer = 5 To passwordLength
            Dim index As Integer = random.Next(0, allChars.Length)
            password.Append(allChars(index))
        Next

        txtPassword.Text = New String(password.ToString().OrderBy(Function() random.Next()).ToArray())
    End Sub

    Private Sub cbUsers_SelectedIndexChanged(sender As Object, e As EventArgs) 
    End Sub

    Private Sub cbUsers_Click(sender As Object, e As EventArgs) 
        Try
            If userID > 0 Then
                'cbUsers.Items.Clear()
            End If
        Catch ex As Exception
            MsgBox(ex.Message.ToString)
        End Try
    End Sub

    Private Sub AddUser_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        ClearFields(panelContainer)
        txtID.Text = "0"
        userID = 0
        Dim manageUser As ManageUser = TryCast(Application.OpenForms("ManageUser"), ManageUser)
        If manageUser IsNot Nothing Then
            manageUser.DefaultSettings()
        End If
    End Sub

    Private Sub chkShowPassword_CheckedChanged(sender As Object, e As EventArgs) Handles chkShowPassword.CheckedChanged
        If txtPassword.UseSystemPasswordChar = True Then
            txtPassword.UseSystemPasswordChar = False
        Else
            txtPassword.UseSystemPasswordChar = True
        End If
    End Sub

    Private Sub btnCopy_Click(sender As Object, e As EventArgs) Handles btnCopy.Click
        Dim copyText As String = Trim(txtPassword.Text)

        If Not String.IsNullOrEmpty(copyText) Then
            Clipboard.SetText(copyText)
            Timer1.Start()
            btnCopy.Enabled = False
            btnCopy.BackgroundImage = My.Resources.copied_24x24
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        copiedCooldown -= 1

        If copiedCooldown <= 0 Then
            btnCopy.BackgroundImage = My.Resources.copy_24x24
            copiedCooldown = 5
            btnCopy.Enabled = True
            Timer1.Stop()
        End If
    End Sub
End Class
