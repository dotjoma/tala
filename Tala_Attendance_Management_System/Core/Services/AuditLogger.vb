Imports System.Data.Odbc
Public Class AuditLogger
    Private Shared _instance As AuditLogger
    Private ReadOnly _logger As ILogger

    Private Sub New()
        _logger = LoggerFactory.Instance
    End Sub

    Public Shared ReadOnly Property Instance As AuditLogger
        Get
            If _instance Is Nothing Then
                _instance = New AuditLogger()
            End If
            Return _instance
        End Get
    End Property

    Public Sub LogActivity(username As String, actionType As String, moduleName As String, description As String, Optional userId As Integer? = Nothing)
        Try
            Dim dbContext As New DatabaseContext()
            Dim query As String = "INSERT INTO user_activity_logs (user_id, username, action_type, module, description) VALUES (?, ?, ?, ?, ?)"

            dbContext.ExecuteNonQuery(query, If(userId, DBNull.Value), username, actionType, moduleName, description)

            _logger.LogInfo($"[AUDIT] {username} - {actionType} - {moduleName}: {description}")
        Catch ex As Exception
            _logger.LogError($"Failed to log audit activity: {ex.Message}")
        End Try
    End Sub

    Public Sub LogLogin(username As String, userId As Integer, role As String)
        LogActivity(username, "LOGIN", "Authentication", $"User logged in with role: {role}", userId)
    End Sub
    Public Sub LogLogout(username As String, userId As Integer)
        LogActivity(username, "LOGOUT", "Authentication", "User logged out", userId)
    End Sub

    Public Sub LogCreate(username As String, moduleName As String, recordDetails As String, Optional userId As Integer? = Nothing)
        LogActivity(username, "CREATE", moduleName, $"Created: {recordDetails}", userId)
    End Sub
    Public Sub LogUpdate(username As String, moduleName As String, recordDetails As String, Optional userId As Integer? = Nothing)
        LogActivity(username, "UPDATE", moduleName, $"Updated: {recordDetails}", userId)
    End Sub

    Public Sub LogDelete(username As String, moduleName As String, recordDetails As String, Optional userId As Integer? = Nothing)
        LogActivity(username, "DELETE", moduleName, $"Deleted/Deactivated: {recordDetails}", userId)
    End Sub

    Public Sub LogPasswordChange(username As String, userId As Integer, changedBy As String)
        LogActivity(changedBy, "PASSWORD_CHANGE", "Users", $"Changed password for user: {username}", userId)
    End Sub
End Class
