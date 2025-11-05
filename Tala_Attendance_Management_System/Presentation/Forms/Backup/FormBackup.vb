Imports System
Imports System.IO
Imports System.Windows.Forms
Imports System.Threading.Tasks

Public Class FormBackup
    Private ReadOnly _logger As ILogger = LoggerFactory.Instance
    Private ReadOnly _backupService As BackupService

    Public Sub New()
        InitializeComponent()
        _backupService = New BackupService(_logger)
    End Sub

    Private Sub FormBackup_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            lblStatus.Text = "Ready"
        Catch ex As Exception
            _logger.LogError($"FormBackup_Load error: {ex.Message}")
        End Try
    End Sub

    Private Sub btnBrowseBackup_Click(sender As Object, e As EventArgs) Handles btnBrowseBackup.Click
        Try
            Using sfd As New SaveFileDialog()
                sfd.Title = "Choose backup destination"
                sfd.Filter = "Backup Files (*.zip)|*.zip"
                sfd.FileName = $"TalaAMS_Backup_{Date.Now:yyyyMMdd_HHmmss}.zip"
                If sfd.ShowDialog(Me) = DialogResult.OK Then
                    txtBackupPath.Text = sfd.FileName
                End If
            End Using
        Catch ex As Exception
            _logger.LogError($"Browse backup path error: {ex.Message}")
        End Try
    End Sub

    Private Sub btnBrowseRestore_Click(sender As Object, e As EventArgs) Handles btnBrowseRestore.Click
        Try
            Using ofd As New OpenFileDialog()
                ofd.Title = "Select backup file"
                ofd.Filter = "Backup Files (*.zip)|*.zip"
                If ofd.ShowDialog(Me) = DialogResult.OK Then
                    txtRestorePath.Text = ofd.FileName
                End If
            End Using
        Catch ex As Exception
            _logger.LogError($"Browse restore path error: {ex.Message}")
        End Try
    End Sub

    Private Sub btnCreateBackup_Click(sender As Object, e As EventArgs) Handles btnCreateBackup.Click
        Try
            ToggleUi(False)
            lblStatus.Text = "Creating backup..."
            lstLog.Items.Add($"[{Date.Now:HH:mm:ss}] Starting backup")

            Dim targetPath As String = txtBackupPath.Text.Trim()
            If String.IsNullOrWhiteSpace(targetPath) Then
                _backupService.CreateBackup(Me)
            Else
                _backupService.CreateBackup(Me, targetPath)
            End If

            lstLog.Items.Add($"[{Date.Now:HH:mm:ss}] Backup completed")
            lblStatus.Text = "Backup complete"
        Catch ex As Exception
            _logger.LogError($"Create backup error: {ex.Message}")
            lstLog.Items.Add($"[{Date.Now:HH:mm:ss}] Error: {ex.Message}")
            lblStatus.Text = "Backup failed"
        Finally
            ToggleUi(True)
            progressBar.Value = 0
        End Try
    End Sub

    Private Sub btnRestore_Click(sender As Object, e As EventArgs) Handles btnRestore.Click
        Try
            ToggleUi(False)
            progressBar.Style = ProgressBarStyle.Continuous
            progressBar.Minimum = 0
            progressBar.Maximum = 100
            progressBar.Value = 0
            lblStatus.Text = "Restoring backup..."
            lstLog.Items.Add($"[{Date.Now:HH:mm:ss}] Starting restore")

            Dim sourcePath As String = txtRestorePath.Text.Trim()

            Dim progress As Action(Of Integer, String) = Sub(p As Integer, m As String)
                                                             If Me.InvokeRequired Then
                                                                 Me.BeginInvoke(CType(Sub()
                                                                                          progressBar.Value = Math.Min(Math.Max(p, 0), 100)
                                                                                          If Not String.IsNullOrWhiteSpace(m) Then
                                                                                              lblStatus.Text = m
                                                                                              lstLog.Items.Add($"[{Date.Now:HH:mm:ss}] {m}")
                                                                                          End If
                                                                                      End Sub, MethodInvoker))
                                                             Else
                                                                 progressBar.Value = Math.Min(Math.Max(p, 0), 100)
                                                                 If Not String.IsNullOrWhiteSpace(m) Then
                                                                     lblStatus.Text = m
                                                                     lstLog.Items.Add($"[{Date.Now:HH:mm:ss}] {m}")
                                                                 End If
                                                             End If
                                                         End Sub

            Task.Run(Sub()
                         Try
                             If String.IsNullOrWhiteSpace(sourcePath) Then
                                 _backupService.RestoreBackup(Me)
                             Else
                                 _backupService.RestoreBackup(Me, sourcePath, progress)
                             End If
                         Catch ex As Exception
                             If Me.InvokeRequired Then
                                 Me.BeginInvoke(CType(Sub()
                                                          lstLog.Items.Add($"[{Date.Now:HH:mm:ss}] Error: {ex.Message}")
                                                          lblStatus.Text = "Restore failed"
                                                      End Sub, MethodInvoker))
                             End If
                         Finally
                             If Me.InvokeRequired Then
                                 Me.BeginInvoke(CType(Sub()
                                                          ToggleUi(True)
                                                      End Sub, MethodInvoker))
                             End If
                         End Try
                     End Sub)
        Catch ex As Exception
            _logger.LogError($"Restore error: {ex.Message}")
            lstLog.Items.Add($"[{Date.Now:HH:mm:ss}] Error: {ex.Message}")
            lblStatus.Text = "Restore failed"
        Finally
        End Try
    End Sub

    Private Sub ToggleUi(enabled As Boolean)
        grpBackup.Enabled = enabled
        grpRestore.Enabled = enabled
        Cursor = If(enabled, Cursors.Default, Cursors.WaitCursor)
    End Sub
End Class