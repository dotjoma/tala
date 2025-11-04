Imports System.IO.Ports
Imports System.Management
Imports System.Threading
Public Class ComPortManager
    Private Shared _instance As ComPortManager
    Private Shared ReadOnly _lockObject As New Object()

    Private port As SerialPort
    Private _currentOwner As String
    Private ReadOnly _logger As ILogger = LoggerFactory.Instance
    Private ReadOnly portLock As New Object()
    Private connectedPortName As String
    Private isInitialized As Boolean = False

    ' Events for port state changes
    Public Event PortAccessRequested(currentOwnerName As String, requesterName As String)
    Public Event PortAccessGranted(ownerName As String, portName As String)
    Public Event PortReleased(ownerName As String)
    Public Event PortDisconnected(portName As String)
    Public Event DataReceived(data As String)

    Private Sub New()
        _logger.LogInfo("ComPortManager instance created")
    End Sub

    Public Shared ReadOnly Property Instance As ComPortManager
        Get
            If _instance Is Nothing Then
                SyncLock _lockObject
                    If _instance Is Nothing Then
                        _instance = New ComPortManager()
                    End If
                End SyncLock
            End If
            Return _instance
        End Get
    End Property

    Public ReadOnly Property IsConnected As Boolean
        Get
            SyncLock portLock
                Return port IsNot Nothing AndAlso port.IsOpen
            End SyncLock
        End Get
    End Property

    Public ReadOnly Property ConnectedPort As String
        Get
            SyncLock portLock
                Return connectedPortName
            End SyncLock
        End Get
    End Property

    Public ReadOnly Property CurrentOwner As String
        Get
            SyncLock portLock
                Return _currentOwner
            End SyncLock
        End Get
    End Property

    Public Function RequestAccess(requester As String, Optional autoConnect As Boolean = True) As Boolean
        SyncLock portLock
            Try
                _logger.LogInfo($"[{requester}] Requesting COM port access...")
                If Not String.IsNullOrEmpty(_currentOwner) AndAlso _currentOwner <> requester Then
                    _logger.LogInfo($"Port currently owned by [{_currentOwner}], requesting release...")
                    RaiseEvent PortAccessRequested(_currentOwner, requester)
                    Thread.Sleep(100)
                End If
                _currentOwner = requester
                _logger.LogInfo($"✓ Access granted to [{requester}]")
                If autoConnect AndAlso Not IsConnected Then
                    Return ConnectToSiliconLabsPort()
                End If

                RaiseEvent PortAccessGranted(requester, connectedPortName)
                Return True

            Catch ex As Exception
                _logger.LogError($"Error granting access to [{requester}]: {ex.Message}")
                Return False
            End Try
        End SyncLock
    End Function

    Public Sub ReleaseAccess(requester As String)
        SyncLock portLock
            Try
                If _currentOwner = requester Then
                    _logger.LogInfo($"[{requester}] Releasing COM port access")
                    _currentOwner = Nothing
                    RaiseEvent PortReleased(requester)
                Else
                    _logger.LogWarning($"[{requester}] attempted to release port but is not the owner (owner: {_currentOwner})")
                End If
            Catch ex As Exception
                _logger.LogError($"Error releasing access from [{requester}]: {ex.Message}")
            End Try
        End SyncLock
    End Sub
    Public Function ConnectToSiliconLabsPort() As Boolean
        SyncLock portLock
            Try
                _logger.LogInfo("Searching for Silicon Labs RFID receiver...")

                If port IsNot Nothing AndAlso port.IsOpen Then
                    port.Close()
                    port.Dispose()
                    port = Nothing
                End If

                Dim baudRate As Integer = 115200
                Dim availablePorts As String() = SerialPort.GetPortNames()

                _logger.LogInfo($"Found {availablePorts.Length} available COM ports: {String.Join(", ", availablePorts)}")

                If availablePorts.Length = 0 Then
                    _logger.LogWarning("No COM ports detected on system")
                    Return False
                End If

                Dim siliconLabsPorts As New List(Of String)

                Try
                    Dim searcher As New ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%(COM%'")
                    For Each obj As ManagementObject In searcher.Get()
                        Dim caption As String = obj("Caption")?.ToString()
                        If Not String.IsNullOrEmpty(caption) Then
                            Dim match = System.Text.RegularExpressions.Regex.Match(caption, "\(COM(\d+)\)")
                            If match.Success Then
                                Dim portName As String = "COM" & match.Groups(1).Value
                                If caption.ToLower().Contains("silicon labs") OrElse caption.ToLower().Contains("cp210") Then
                                    siliconLabsPorts.Add(portName)
                                    _logger.LogInfo($"✓ Detected Silicon Labs port: {portName} - {caption}")
                                End If
                            End If
                        End If
                    Next
                Catch ex As Exception
                    _logger.LogWarning($"Could not query WMI for port details: {ex.Message}")
                End Try

                If siliconLabsPorts.Count > 0 Then
                    For Each comPort As String In siliconLabsPorts
                        If TryConnectToPort(comPort, baudRate) Then
                            Return True
                        End If
                    Next
                Else
                    _logger.LogWarning("No Silicon Labs RFID receiver detected")
                End If

                _logger.LogInfo("Trying all available ports as fallback...")
                For Each comPort As String In availablePorts
                    If Not siliconLabsPorts.Contains(comPort) Then
                        If TryConnectToPort(comPort, baudRate) Then
                            Return True
                        End If
                    End If
                Next

                _logger.LogError("Failed to connect to any COM port")
                Return False

            Catch ex As Exception
                _logger.LogError($"Error in ConnectToSiliconLabsPort: {ex.Message}")
                Return False
            End Try
        End SyncLock
    End Function

    Private Function TryConnectToPort(comPort As String, baudRate As Integer) As Boolean
        Try
            _logger.LogInfo($"Attempting to connect to {comPort} at {baudRate} baud...")

            Dim tempPort As New SerialPort(comPort, baudRate)
            tempPort.Open()

            port = tempPort
            connectedPortName = comPort
            isInitialized = True

            _logger.LogInfo($"✓ Successfully connected to {comPort}")

            Task.Run(AddressOf ReadDataAsync)

            Return True

        Catch ex As Exception
            _logger.LogDebug($"Failed to connect to {comPort}: {ex.Message}")
            Return False
        End Try
    End Function

    Public Sub Disconnect()
        SyncLock portLock
            Try
                If port IsNot Nothing AndAlso port.IsOpen Then
                    Dim portName As String = port.PortName
                    port.Close()
                    port.Dispose()
                    port = Nothing
                    connectedPortName = Nothing
                    isInitialized = False

                    _logger.LogInfo($"Disconnected from {portName}")
                    RaiseEvent PortDisconnected(portName)
                End If
            Catch ex As Exception
                _logger.LogError($"Error disconnecting: {ex.Message}")
            End Try
        End SyncLock
    End Sub
    Private Sub ReadDataAsync()
        Try
            _logger.LogInfo("ReadDataAsync started - waiting for RFID scans...")

            While True
                SyncLock portLock
                    If port Is Nothing OrElse Not port.IsOpen Then
                        Exit While
                    End If
                End SyncLock

                Dim buffer As New Text.StringBuilder()

                While True
                    SyncLock portLock
                        If port Is Nothing OrElse Not port.IsOpen Then
                            Exit While
                        End If
                        If port.BytesToRead > 0 Then
                            Exit While
                        End If
                    End SyncLock
                    Application.DoEvents()
                    Thread.Sleep(10)
                End While

                SyncLock portLock
                    If port Is Nothing OrElse Not port.IsOpen Then
                        Exit While
                    End If

                    Do While port.BytesToRead > 0
                        Dim data As Integer = port.ReadByte()
                        If data = 10 Then
                            Exit Do
                        ElseIf data <> 13 Then
                            buffer.Append(Convert.ToChar(data))
                        End If
                    Loop
                End SyncLock

                Dim tagData As String = buffer.ToString().Trim()

                If Not String.IsNullOrEmpty(tagData) Then
                    _logger.LogInfo($"RFID tag scanned (raw): '{tagData}' (length: {tagData.Length})")

                    If tagData.Length > 8 AndAlso Char.IsDigit(tagData(0)) Then
                        Dim originalTag As String = tagData
                        tagData = tagData.Substring(1)
                        _logger.LogInfo($"Removed device ID prefix: '{originalTag}' -> '{tagData}'")
                    End If

                    _logger.LogInfo($"Raising DataReceived event with tag: '{tagData}'")
                    RaiseEvent DataReceived(tagData)
                    _logger.LogInfo("DataReceived event raised successfully")
                End If
            End While

        Catch ex As IO.IOException When ex.Message.Contains("port is closed")
            _logger.LogDebug("Port closed during read operation (expected)")
        Catch ex As Exception
            _logger.LogError($"Error in ReadDataAsync: {ex.Message}")

            SyncLock portLock
                If connectedPortName IsNot Nothing Then
                    Dim portName As String = connectedPortName
                    connectedPortName = Nothing
                    RaiseEvent PortDisconnected(portName)
                End If
            End SyncLock
        End Try
    End Sub

    Public Function GetAvailablePorts() As List(Of ComPortInfo)
        Dim portList As New List(Of ComPortInfo)

        Try
            Dim availablePorts As String() = SerialPort.GetPortNames()
            Dim portDetails As New Dictionary(Of String, String)

            Try
                Dim searcher As New ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%(COM%'")
                For Each obj As ManagementObject In searcher.Get()
                    Dim caption As String = obj("Caption")?.ToString()
                    If Not String.IsNullOrEmpty(caption) Then
                        Dim match = System.Text.RegularExpressions.Regex.Match(caption, "\(COM(\d+)\)")
                        If match.Success Then
                            Dim portName As String = "COM" & match.Groups(1).Value
                            portDetails(portName) = caption
                        End If
                    End If
                Next
            Catch ex As Exception
                _logger.LogWarning($"Could not query WMI: {ex.Message}")
            End Try

            For Each portName As String In availablePorts
                Dim info As New ComPortInfo With {
                    .PortName = portName,
                    .IsConnected = (portName = connectedPortName)
                }

                If portDetails.ContainsKey(portName) Then
                    Dim description As String = portDetails(portName)
                    info.Description = description
                    info.IsSiliconLabs = description.ToLower().Contains("silicon labs") OrElse description.ToLower().Contains("cp210")
                End If

                portList.Add(info)
            Next

        Catch ex As Exception
            _logger.LogError($"Error getting available ports: {ex.Message}")
        End Try

        Return portList
    End Function

End Class
Public Class ComPortInfo
    Public Property PortName As String
    Public Property Description As String
    Public Property IsSiliconLabs As Boolean
    Public Property IsConnected As Boolean
    
    Public Overrides Function ToString() As String
        If IsSiliconLabs Then
            Return $"{PortName} - Silicon Labs RFID"
        ElseIf Not String.IsNullOrEmpty(Description) Then
            Return $"{PortName} - {Description.Split("("c)(0).Trim()}"
        Else
            Return PortName
        End If
    End Function
End Class
