Imports System
Imports System.Threading.Tasks
Imports System.Windows.Forms

Public Class UpdateManager
    Private ReadOnly _updateService As UpdateService
    Private ReadOnly _logger As ILogger

    Public Sub New(logger As ILogger)
        _logger = logger
        _updateService = New UpdateService(logger)
    End Sub

    Public Async Function CheckForUpdatesAsync(Optional parentForm As Form = Nothing) As Task(Of Boolean)
        Try
            _logger.LogInfo("Starting update check...")

            If Not ShouldCheckForUpdates() Then
                _logger.LogInfo("Update check skipped - conditions not met")
                Return False
            End If

            Dim versionInfo = Await _updateService.CheckForUpdateAsync()

            If versionInfo IsNot Nothing Then
                ShowUpdateDialog(versionInfo, parentForm)
                Return True
            Else
                _logger.LogInfo("No updates available")
                Return True
            End If

        Catch ex As Exception
            _logger.LogError($"Error during update check: {ex.Message}")
            Return False
        End Try
    End Function

    Public Async Function CheckForUpdatesSilentAsync() As Task(Of VersionInfo)
        Try
            If Not ShouldCheckForUpdates() Then
                Return Nothing
            End If

            Return Await _updateService.CheckForUpdateAsync()

        Catch ex As Exception
            _logger.LogError($"Error during silent update check: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Public Sub ShowUpdateNotification(versionInfo As VersionInfo, Optional parentForm As Form = Nothing)
        Try
            Dim message = $"A new version ({versionInfo.Version}) is available.{Environment.NewLine}Do you want to update now?"
            Dim result = MessageBox.Show(message, "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information)

            If result = DialogResult.Yes Then
                ShowUpdateDialog(versionInfo, parentForm)
            End If

        Catch ex As Exception
            _logger.LogError($"Error showing update notification: {ex.Message}")
        End Try
    End Sub

    Private Function ShouldCheckForUpdates() As Boolean
        Try
            If AppConfig.Instance.Environment <> EnvironmentType.Development Then
                _logger.LogInfo("Update check disabled - not in development environment")
                Return False
            End If

            If Not NetworkHelper.IsInternetAvailable() Then
                _logger.LogWarning("Update check disabled - no internet connection")
                Return False
            End If

            Return True

        Catch ex As Exception
            _logger.LogError($"Error checking update conditions: {ex.Message}")
            Return False
        End Try
    End Function

    Private Sub ShowUpdateDialog(versionInfo As VersionInfo, parentForm As Form)
        Try
            Using updateDialog As New UpdateDialog(_updateService, versionInfo, _logger)
                If parentForm IsNot Nothing Then
                    updateDialog.ShowDialog(parentForm)
                Else
                    updateDialog.ShowDialog()
                End If
            End Using

        Catch ex As Exception
            _logger.LogError($"Error showing update dialog: {ex.Message}")
        End Try
    End Sub

    Public Async Sub CheckForUpdatesOnStartup(Optional parentForm As Form = Nothing)
        Try
            Await Task.Delay(2000)

            Dim versionInfo = Await CheckForUpdatesSilentAsync()
            If versionInfo IsNot Nothing Then
                If parentForm IsNot Nothing AndAlso parentForm.InvokeRequired Then
                    parentForm.Invoke(New Action(Sub() ShowUpdateNotification(versionInfo, parentForm)))
                Else
                    ShowUpdateNotification(versionInfo, parentForm)
                End If
            End If

        Catch ex As Exception
            _logger.LogError($"Error during startup update check: {ex.Message}")
        End Try
    End Sub

End Class