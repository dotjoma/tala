Imports System.IO.Ports
Imports System.Text
Imports System.Management

Public Class FormIDScanner
    Private ReadOnly _logger As ILogger = LoggerFactory.Instance
    Private ReadOnly _comPortManager As ComPortManager = ComPortManager.Instance
    Private deviceID As Integer = 0
    Private autoRefreshTimer As Timer
    Private WithEvents inactivityTimer As Timer
    Private inactivityCountdown As Integer = 10 ' 10 seconds
    Private Const FORM_NAME As String = "FormIDScanner"
    
    Private Sub FormIDScanner_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _logger.LogInfo("FormIDScanner loading...")
        txtTagID.Text = ""

        btnConnect.Text = "Connect"
        btnConnect.BackColor = Color.FromArgb(46, 204, 113) ' Green for connect
        btnConnect.Enabled = False
        cboCOMPort.Enabled = True
        btnRefresh.Enabled = True

        autoRefreshTimer = New Timer()
        autoRefreshTimer.Interval = 5000
        AddHandler autoRefreshTimer.Tick, AddressOf AutoRefreshTimer_Tick
        autoRefreshTimer.Start()

        inactivityTimer = New Timer()
        inactivityTimer.Interval = 1000 ' 1 second
        inactivityTimer.Enabled = False

        AddHandler _comPortManager.DataReceived, AddressOf OnDataReceived
        AddHandler _comPortManager.PortDisconnected, AddressOf OnPortDisconnected

        LoadAvailablePorts()
        _logger.LogInfo("FormIDScanner loaded successfully")
    End Sub

    Private Sub AutoRefreshTimer_Tick(sender As Object, e As EventArgs)
        If _comPortManager.CurrentOwner <> FORM_NAME Then
            _logger.LogDebug("Auto-refresh: Checking for Silicon Labs devices...")
            LoadAvailablePorts()
        End If
    End Sub

    Private Sub LoadAvailablePorts()
        Try
            _logger.LogInfo("Loading available COM ports...")

            cboCOMPort.Items.Clear()

            Dim ports As List(Of ComPortInfo) = _comPortManager.GetAvailablePorts()
            
            If ports.Count = 0 Then
                _logger.LogWarning("No COM ports detected on system")
                cboCOMPort.Items.Add("No ports available")
                cboCOMPort.SelectedIndex = 0
                btnConnect.Enabled = False
                txtComPort.Text = "No COM ports detected"
                txtComPort.ForeColor = Color.Red
                pbUsbImage.Image = My.Resources.usb_disconnected
                Return
            End If

            Dim siliconLabsPorts As New List(Of ComPortInfo)
            Dim otherPorts As New List(Of ComPortInfo)
            
            For Each portInfo As ComPortInfo In ports
                If portInfo.IsSiliconLabs Then
                    siliconLabsPorts.Add(portInfo)
                Else
                    otherPorts.Add(portInfo)
                End If
            Next

            For Each portInfo As ComPortInfo In siliconLabsPorts
                cboCOMPort.Items.Add(portInfo.ToString())
            Next

            For Each portInfo As ComPortInfo In otherPorts
                cboCOMPort.Items.Add(portInfo.ToString())
            Next

            If siliconLabsPorts.Count = 0 Then
                txtComPort.Text = "No RFID receiver connected"
                txtComPort.ForeColor = Color.Red
                pbUsbImage.Image = My.Resources.usb_disconnected
                _logger.LogWarning("No Silicon Labs RFID receiver detected")

                If cboCOMPort.Items.Count > 0 Then
                    cboCOMPort.SelectedIndex = 0
                End If
                btnConnect.Enabled = (cboCOMPort.Items.Count > 0)
            ElseIf siliconLabsPorts.Count = 1 Then
                cboCOMPort.SelectedIndex = 0
                txtComPort.Text = "RFID receiver detected (click Connect)"
                txtComPort.ForeColor = Color.Blue
                pbUsbImage.Image = My.Resources.usb_disconnected
                btnConnect.Enabled = True
                _logger.LogInfo($"Single Silicon Labs device detected: {siliconLabsPorts(0).PortName}")
            Else
                cboCOMPort.SelectedIndex = 0
                txtComPort.Text = $"{siliconLabsPorts.Count} RFID receivers detected"
                txtComPort.ForeColor = Color.Blue
                pbUsbImage.Image = My.Resources.usb_disconnected
                _logger.LogInfo($"Multiple Silicon Labs devices detected ({siliconLabsPorts.Count}) - User must select")
                btnConnect.Enabled = True
            End If

        Catch ex As Exception
            _logger.LogError($"Error loading COM ports: {ex.Message}")
            MessageBox.Show("Error loading COM ports: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    
    Private Sub btnConnect_Click(sender As Object, e As EventArgs) Handles btnConnect.Click
        Try
            If btnConnect.Text = "Disconnect" Then
                StopInactivityTimer()

                _comPortManager.ReleaseAccess(FORM_NAME)
                _logger.LogInfo("User manually disconnected from COM port")

                txtComPort.Text = "Disconnected"
                txtComPort.ForeColor = Color.Gray
                pbUsbImage.Image = My.Resources.usb_disconnected
                cboCOMPort.Enabled = True
                btnConnect.Text = "Connect"
                btnConnect.BackColor = Color.FromArgb(46, 204, 113)
                btnRefresh.Enabled = True

                If autoRefreshTimer IsNot Nothing Then
                    autoRefreshTimer.Start()
                    _logger.LogDebug("Auto-refresh timer restarted (manual disconnect)")
                End If

                LoadAvailablePorts()
                Return
            End If

            Dim selectedItem As String = cboCOMPort.SelectedItem?.ToString()

            If String.IsNullOrEmpty(selectedItem) OrElse selectedItem = "No ports available" Then
                MessageBox.Show("Please select a valid COM port.", "Invalid Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim selectedPort As String = selectedItem.Split(" "c)(0).Trim()

            _logger.LogInfo($"Requesting access to COM port for scanning...")

            If _comPortManager.RequestAccess(FORM_NAME, autoConnect:=True) Then
                _logger.LogInfo($"✓ Successfully connected to {_comPortManager.ConnectedPort}")
                txtComPort.Text = $"Connected: {_comPortManager.ConnectedPort}"
                txtComPort.ForeColor = Color.Green
                pbUsbImage.Image = My.Resources.usb_connected

                cboCOMPort.Enabled = False
                btnConnect.Text = "Disconnect"
                btnConnect.BackColor = Color.FromArgb(231, 76, 60)
                btnRefresh.Enabled = False

                If autoRefreshTimer IsNot Nothing Then
                    autoRefreshTimer.Stop()
                    _logger.LogDebug("Auto-refresh timer stopped (connected)")
                End If

                StartInactivityTimer()
                _logger.LogInfo("Inactivity timer started - form will auto-close in 10 seconds without scan")
            Else
                Throw New Exception("Failed to connect to COM port")
            End If

        Catch ex As Exception
            _logger.LogError($"Error connecting to COM port: {ex.Message}")
            txtComPort.Text = "Connection Failed"
            txtComPort.ForeColor = Color.Red
            pbUsbImage.Image = My.Resources.usb_disconnected
            MessageBox.Show($"Failed to connect: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error)

            cboCOMPort.Enabled = True
            btnConnect.Text = "Connect"
            btnConnect.BackColor = Color.FromArgb(46, 204, 113) ' Green for connect
            btnRefresh.Enabled = True

            If autoRefreshTimer IsNot Nothing Then
                autoRefreshTimer.Start()
            End If
        End Try
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        LoadAvailablePorts()
    End Sub

    Private Sub OnDataReceived(tagData As String)
        _logger.LogInfo($"FormIDScanner.OnDataReceived called with tag: '{tagData}', CurrentOwner: '{_comPortManager.CurrentOwner}', FORM_NAME: '{FORM_NAME}'")

        If _comPortManager.CurrentOwner = FORM_NAME Then
            _logger.LogInfo($"✓ Tag ID received: {tagData}, setting txtTagID...")
            Me.Invoke(Sub()
                          StopInactivityTimer()

                          txtTagID.Text = tagData
                          _logger.LogInfo($"txtTagID.Text set to: {txtTagID.Text}")
                      End Sub)
        Else
            _logger.LogWarning($"Not the owner, skipping. Owner is: '{_comPortManager.CurrentOwner}'")
        End If
    End Sub
    Private Sub OnPortDisconnected(portName As String)
        If _comPortManager.CurrentOwner = FORM_NAME Then
            _logger.LogWarning($"Port {portName} disconnected")

            Me.Invoke(Sub()
                          txtComPort.Text = "Device disconnected"
                          txtComPort.ForeColor = Color.Red
                          pbUsbImage.Image = My.Resources.usb_disconnected
                          cboCOMPort.Enabled = True
                          btnConnect.Text = "Connect"
                          btnConnect.BackColor = Color.FromArgb(46, 204, 113)
                          btnRefresh.Enabled = True

                          If autoRefreshTimer IsNot Nothing Then
                              autoRefreshTimer.Start()
                              _logger.LogDebug("Auto-refresh timer restarted after device disconnect")
                          End If

                          LoadAvailablePorts()
                      End Sub)
        End If
    End Sub

    Private Sub FormIDScanner_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        _logger.LogInfo("FormIDScanner closing...")

        Try
            RemoveHandler _comPortManager.DataReceived, AddressOf OnDataReceived
            RemoveHandler _comPortManager.PortDisconnected, AddressOf OnPortDisconnected

            If inactivityTimer IsNot Nothing Then
                inactivityTimer.Stop()
                inactivityTimer.Dispose()
                _logger.LogInfo("Inactivity timer stopped")
            End If

            If autoRefreshTimer IsNot Nothing Then
                autoRefreshTimer.Stop()
                autoRefreshTimer.Dispose()
                _logger.LogInfo("Auto-refresh timer stopped")
            End If

            _comPortManager.ReleaseAccess(FORM_NAME)

        Catch ex As Exception
            _logger.LogError($"Error closing FormIDScanner: {ex.Message}")
        End Try

        _logger.LogInfo("FormIDScanner closed")
    End Sub

    Private Sub cboCOMPort_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboCOMPort.SelectedIndexChanged
        If cboCOMPort.SelectedItem IsNot Nothing AndAlso cboCOMPort.SelectedItem.ToString() <> "No ports available" Then
            btnConnect.Enabled = True
        End If
    End Sub

    Private Sub txtTagID_TextChanged(sender As Object, e As EventArgs) Handles txtTagID.TextChanged
        If txtTagID.TextLength > 7 Then
            _logger.LogDebug($"Tag ID length valid ({txtTagID.TextLength} chars): {txtTagID.Text}")
            
            If Val(txtFlag.Text) = 1 Then
                _logger.LogInfo($"Setting tag ID for Student: {txtTagID.Text}")
                AddStudents.txtTagID.Text = txtTagID.Text
            ElseIf Val(txtFlag.Text) = 2 Then
                _logger.LogInfo($"Setting tag ID for Faculty: {txtTagID.Text}")
                AddFaculty.txtTagID.Text = txtTagID.Text
            End If

            _logger.LogDebug("Closing FormIDScanner after successful scan")
            Dim tagId As String = txtTagID.Text
            Me.Close()

            Task.Run(Sub()
                         System.Threading.Thread.Sleep(100)
                         Application.OpenForms(0)?.Invoke(Sub()
                                                              SuccessDialog.ShowSuccess($"RFID scanning has been completed!", 1)
                                                          End Sub)
                     End Sub)
        Else
            If txtTagID.TextLength > 0 Then
                _logger.LogDebug($"Tag ID too short ({txtTagID.TextLength} chars): {txtTagID.Text}")
            End If
        End If
    End Sub

    Private Sub labelButtonClose_Click(sender As Object, e As EventArgs) Handles labelButtonClose.Click
        _logger.LogInfo("User clicked close button on FormIDScanner")
        
        Me.Close()
        
        If Val(txtFlag.Text) = 1 Then
            If AddStudents.txtTagID.TextLength > 0 Then
                _logger.LogDebug($"Keeping existing student tag ID: {AddStudents.txtTagID.Text}")
                AddStudents.txtTagID.Text = AddStudents.txtTagID.Text
            Else
                _logger.LogDebug("Setting default student tag ID: abcdefg")
                AddStudents.txtTagID.Text = "abcdefg"
            End If
        ElseIf Val(txtFlag.Text) = 2 Then
            If AddFaculty.txtTagID.TextLength > 0 Then
                _logger.LogDebug($"Keeping existing faculty tag ID: {AddFaculty.txtTagID.Text}")
                AddFaculty.txtTagID.Text = AddFaculty.txtTagID.Text
            Else
                _logger.LogDebug("Setting default faculty tag ID: abcdefg")
                AddFaculty.txtTagID.Text = "abcdefg"
            End If
        End If
    End Sub

    Private Sub StartInactivityTimer()
        inactivityCountdown = 10
        UpdateCountdownDisplay()
        inactivityTimer.Start()
        _logger.LogDebug("Inactivity timer started")
    End Sub

    Private Sub ResetInactivityTimer()
        inactivityCountdown = 10
        UpdateCountdownDisplay()
        _logger.LogDebug("Inactivity timer reset")
    End Sub

    Private Sub StopInactivityTimer()
        If inactivityTimer IsNot Nothing Then
            inactivityTimer.Stop()
            txtComPort.Text = If(_comPortManager.IsConnected, $"Connected: {_comPortManager.ConnectedPort}", "Disconnected")
            _logger.LogDebug("Inactivity timer stopped")
        End If
    End Sub
    Private Sub UpdateCountdownDisplay()
        If inactivityCountdown > 0 Then
            txtComPort.Text = $"Auto-close in {inactivityCountdown}s (scan to cancel)"
            txtComPort.ForeColor = If(inactivityCountdown <= 3, Color.Red, Color.Orange)
        End If
    End Sub
    Private Sub inactivityTimer_Tick(sender As Object, e As EventArgs) Handles inactivityTimer.Tick
        inactivityCountdown -= 1
        
        If inactivityCountdown <= 0 Then
            _logger.LogInfo("Inactivity timeout reached - auto-closing FormIDScanner")
            inactivityTimer.Stop()
            Me.Close()
        Else
            UpdateCountdownDisplay()
        End If
    End Sub
End Class
