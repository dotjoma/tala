# COM Port Manager

## Overview

The `ComPortManager` is a centralized singleton service that manages all COM port access for RFID scanners across the application. It prevents port access conflicts and provides a clean, event-driven API for forms that need to read RFID data.

## Architecture

### Singleton Pattern
- Only one instance exists throughout the application lifetime
- Thread-safe initialization using double-check locking
- Access via `ComPortManager.Instance`

### Event-Driven Communication
Forms don't directly access the COM port. Instead, they:
1. Request access using `RequestAccess()`
2. Subscribe to events like `DataReceived`
3. Release access using `ReleaseAccess()`

## Key Features

### 1. Access Control
```vb
' Request exclusive access
If ComPortManager.Instance.RequestAccess("MyForm", autoConnect:=True) Then
    ' You now have exclusive access
End If

' Release when done
ComPortManager.Instance.ReleaseAccess("MyForm")
```

### 2. Event Subscription
```vb
' Subscribe to events
AddHandler ComPortManager.Instance.DataReceived, AddressOf OnDataReceived
AddHandler ComPortManager.Instance.PortDisconnected, AddressOf OnPortDisconnected

' Handle RFID scans
Private Sub OnDataReceived(tagData As String)
    If ComPortManager.Instance.CurrentOwner = "MyForm" Then
        ' Process the tag data
    End If
End Sub
```

### 3. Auto-Discovery
The manager automatically detects Silicon Labs RFID receivers:
```vb
' Get list of available ports with details
Dim ports As List(Of ComPortInfo) = ComPortManager.Instance.GetAvailablePorts()

For Each portInfo In ports
    If portInfo.IsSiliconLabs Then
        Console.WriteLine($"RFID Reader: {portInfo.PortName}")
    End If
Next
```

### 4. Connection Management
```vb
' Check connection status
If ComPortManager.Instance.IsConnected Then
    Dim portName = ComPortManager.Instance.ConnectedPort
    Console.WriteLine($"Connected to {portName}")
End If

' Manual connect/disconnect
ComPortManager.Instance.ConnectToSiliconLabsPort()
ComPortManager.Instance.Disconnect()
```

## Events

### PortAccessRequested
Fired when another form requests access to the port.
```vb
Private Sub OnPortAccessRequested(currentOwner As String, requester As String)
    ' Current owner should pause operations
End Sub
```

### PortAccessGranted
Fired when access is granted to a form.
```vb
Private Sub OnPortAccessGranted(owner As String, portName As String)
    ' You can now use the port
End Sub
```

### PortReleased
Fired when a form releases the port.
```vb
Private Sub OnPortReleased(owner As String)
    ' Port is now available for others
End Sub
```

### PortDisconnected
Fired when the physical device is unplugged.
```vb
Private Sub OnPortDisconnected(portName As String)
    ' Handle disconnection
End Sub
```

### DataReceived
Fired when RFID tag data is received.
```vb
Private Sub OnDataReceived(tagData As String)
    ' Process RFID tag
End Sub
```

## Usage Examples

### Example 1: RFIDScanMonitor (Long-Running Monitor)

```vb
Public Class RFIDScanMonitor
    Private ReadOnly _comPortManager As ComPortManager = ComPortManager.Instance
    Private Const FORM_NAME As String = "RFIDScanMonitor"
    
    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Subscribe to events
        AddHandler _comPortManager.DataReceived, AddressOf OnDataReceived
        AddHandler _comPortManager.PortAccessRequested, AddressOf OnPortAccessRequested
        
        ' Request access
        _comPortManager.RequestAccess(FORM_NAME, autoConnect:=True)
    End Sub
    
    Private Sub OnDataReceived(tagData As String)
        If _comPortManager.CurrentOwner = FORM_NAME Then
            ProcessRFIDTag(tagData)
        End If
    End Sub
    
    Private Sub OnPortAccessRequested(currentOwner As String, requester As String)
        If currentOwner = FORM_NAME Then
            ' Pause monitoring while another form uses the port
            ShowMessage("Port temporarily in use")
        End If
    End Sub
    
    Private Sub Form_Closing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        ' Unsubscribe and release
        RemoveHandler _comPortManager.DataReceived, AddressOf OnDataReceived
        _comPortManager.ReleaseAccess(FORM_NAME)
    End Sub
End Class
```

### Example 2: FormIDScanner (Temporary Access)

```vb
Public Class FormIDScanner
    Private ReadOnly _comPortManager As ComPortManager = ComPortManager.Instance
    Private Const FORM_NAME As String = "FormIDScanner"
    
    Private Sub btnConnect_Click(sender As Object, e As EventArgs) Handles btnConnect.Click
        ' Request access (will notify current owner)
        If _comPortManager.RequestAccess(FORM_NAME, autoConnect:=True) Then
            ' Subscribe to data
            AddHandler _comPortManager.DataReceived, AddressOf OnDataReceived
            UpdateUI("Connected")
        End If
    End Sub
    
    Private Sub OnDataReceived(tagData As String)
        If _comPortManager.CurrentOwner = FORM_NAME Then
            txtTagID.Text = tagData
        End If
    End Sub
    
    Private Sub Form_Closing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        ' Clean up
        RemoveHandler _comPortManager.DataReceived, AddressOf OnDataReceived
        _comPortManager.ReleaseAccess(FORM_NAME)
    End Sub
End Class
```

## Thread Safety

The manager is fully thread-safe:
- All port operations are protected by `SyncLock`
- Events are raised on the appropriate thread
- Safe to call from any thread

## Best Practices

### 1. Always Use Unique Form Names
```vb
Private Const FORM_NAME As String = "MyUniqueFormName"
```

### 2. Always Unsubscribe from Events
```vb
Private Sub Form_Closing(...)
    RemoveHandler _comPortManager.DataReceived, AddressOf OnDataReceived
    RemoveHandler _comPortManager.PortDisconnected, AddressOf OnPortDisconnected
End Sub
```

### 3. Always Release Access
```vb
Private Sub Form_Closing(...)
    _comPortManager.ReleaseAccess(FORM_NAME)
End Sub
```

### 4. Check Ownership Before Processing
```vb
Private Sub OnDataReceived(tagData As String)
    If _comPortManager.CurrentOwner = FORM_NAME Then
        ' Only process if we're the owner
        ProcessData(tagData)
    End If
End Sub
```

### 5. Handle Disconnections Gracefully
```vb
Private Sub OnPortDisconnected(portName As String)
    UpdateUI("Device unplugged - please reconnect")
    ' Optionally try to reconnect
    _comPortManager.ConnectToSiliconLabsPort()
End Sub
```

## Troubleshooting

### Port Access Denied
**Symptom:** "Access to the port 'COM3' is denied"
**Cause:** Another application has the port open
**Solution:** Close other applications or use the manager's access control

### No Data Received
**Symptom:** Events not firing
**Cause:** Not subscribed to events or not the current owner
**Solution:** 
1. Check event subscription
2. Verify `CurrentOwner` matches your form name
3. Check `IsConnected` property

### Multiple Forms Fighting for Access
**Symptom:** Port keeps disconnecting/reconnecting
**Cause:** Forms not properly releasing access
**Solution:** Always call `ReleaseAccess()` in `FormClosing`

## Future Enhancements

Potential improvements:
- Priority-based access (emergency scans override monitoring)
- Queue system for multiple requesters
- Automatic reconnection with exponential backoff
- Port usage statistics and logging
- Support for multiple simultaneous ports

## Related Files

- `Core/Services/ComPortManager.vb` - Main implementation
- `Presentation/Forms/Attendance/RFIDScanMonitor.vb` - Example usage
- `Presentation/Forms/Attendance/FormIDScanner.vb` - Example usage
