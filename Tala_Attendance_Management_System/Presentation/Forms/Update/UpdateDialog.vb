Imports System
Imports System.Threading.Tasks
Imports System.Windows.Forms

Public Class UpdateDialog
    Private ReadOnly _updateService As UpdateService
    Private ReadOnly _versionInfo As VersionInfo
    Private ReadOnly _logger As ILogger
    
    Public Sub New(updateService As UpdateService, versionInfo As VersionInfo, logger As ILogger)
        InitializeComponent()
        _updateService = updateService
        _versionInfo = versionInfo
        _logger = logger
        
        InitializeDialog()
    End Sub

    Private Sub InitializeDialog()
        Try
            lblMessage.Text = $"A new version ({_versionInfo.Version}) of Tala Attendance Management is available."
            txtChangeLog.Text = _versionInfo.ChangeLog

            Me.TopMost = True
            Me.ShowInTaskbar = False

        Catch ex As Exception
            _logger.LogError($"Error initializing update dialog: {ex.Message}")
        End Try
    End Sub

    Private Async Sub btnYes_Click(sender As Object, e As EventArgs) Handles btnYes.Click
        Try
            btnYes.Enabled = False
            btnNo.Enabled = False
            progressBar.Visible = True
            lblProgress.Visible = True
            lblProgress.ForeColor = Color.FromArgb(64, 64, 64)
            lblProgress.Text = "Starting download..."

            Dim updateSuccess = Await _updateService.DownloadAndInstallUpdateAsync(_versionInfo, AddressOf UpdateProgress, AddressOf UpdateStatus)

            If Not updateSuccess Then
                lblProgress.ForeColor = Color.Red
                lblProgress.Text = "Update failed. Please try again later."
                System.Threading.Thread.Sleep(2000)
                Me.DialogResult = DialogResult.Cancel
                Me.Close()
            End If

        Catch ex As Exception
            _logger.LogError($"Error during update process: {ex.Message}")
            lblProgress.ForeColor = Color.Red
            lblProgress.Text = $"Update failed: {ex.Message}"
            System.Threading.Thread.Sleep(2000)
            Me.DialogResult = DialogResult.Cancel
            Me.Close()
        End Try
    End Sub

    Private Sub btnNo_Click(sender As Object, e As EventArgs) Handles btnNo.Click
        _logger.LogInfo("User declined update")
        Me.DialogResult = DialogResult.No
        Me.Close()
    End Sub

    Private Sub UpdateProgress(percentage As Integer)
        Try
            If Me.InvokeRequired Then
                Me.Invoke(New Action(Of Integer)(AddressOf UpdateProgress), percentage)
                Return
            End If
            
            progressBar.Value = Math.Min(percentage, 100)
            
        Catch ex As Exception

        End Try
    End Sub

    Private Sub UpdateStatus(message As String)
        Try
            If Me.InvokeRequired Then
                Me.Invoke(New Action(Of String)(AddressOf UpdateStatus), message)
                Return
            End If

            lblProgress.ForeColor = Color.FromArgb(64, 64, 64)
            lblProgress.Text = message
            
        Catch ex As Exception

        End Try
    End Sub

    Private Sub UpdateDialog_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If btnYes.Enabled = False AndAlso btnNo.Enabled = False AndAlso progressBar.Visible Then
            If e.CloseReason = CloseReason.UserClosing Then
                e.Cancel = True
                MessageBox.Show("Please wait for the update to complete.", "Update in Progress", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        End If
    End Sub

End Class