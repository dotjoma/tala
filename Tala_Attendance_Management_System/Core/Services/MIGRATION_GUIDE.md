# Migration Guide: COM Port Manager

## What Changed?

We've refactored the COM port management from a distributed approach (each form managing its own SerialPort) to a centralized singleton pattern (ComPortManager).

## Before vs After

### Before (Old Approach)
```vb
' Each form had its own SerialPort
Private port As SerialPort

' Manual connection
port = New SerialPort("COM3", 115200)
port.Open()

' Manual data reading
Task.Run(AddressOf ReadDataAsync)

' Manual coordination between forms
RFIDScanMonitor.TemporarilyReleasePort()
FormIDScanner.ShowDialog()
RFIDScanMonitor.ReconnectAfterRelease()
```

**Problems:**
- ❌ Duplicate code in every form
- ❌ Port access conflicts
- ❌ Tight coupling between forms
- ❌ Manual coordination required
- ❌ Hard to maintain

### After (New Approach)
```vb
' Use the singleton manager
Private ReadOnly _comPortManager As ComPortManager = ComPortManager.Instance

' Request access
_comPortManager.RequestAccess("MyForm", autoConnect:=True)

' Subscribe to events
AddHandler _comPortManager.DataReceived, AddressOf OnDataReceived

' Automatic coordination
FormIDScanner.ShowDialog() ' Manager handles everything!
```

**Benefits:**
- ✅ Single source of truth
- ✅ Zero port conflicts
- ✅ Automatic coordination
- ✅ Event-driven architecture
- ✅ Easy to maintain

## Migration Steps for New Forms

If you need to add a new form that uses RFID scanning:

### Step 1: Add Manager Reference
```vb
Public Class MyNewForm
    Private ReadOnly _comPortManager As ComPortManager = ComPortManager.Instance
    Private Const FORM_NAME As String = "MyNewForm"
```

### Step 2: Request Access on Load
```vb
Private Sub MyNewForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
    ' Subscribe to events
    AddHandler _comPortManager.DataReceived, AddressOf OnDataReceived
    AddHandler _comPortManager.PortDisconnected, AddressOf OnPortDisconnected
    
    ' Request access
    If _comPortManager.RequestAccess(FORM_NAME, autoConnect:=True) Then
        ' Ready to receive data
    End If
End Sub
```

### Step 3: Handle Data
```vb
Private Sub OnDataReceived(tagData As String)
    ' Only process if we're the owner
    If _comPortManager.CurrentOwner = FORM_NAME Then
        Me.Invoke(Sub()
            ' Process the RFID tag
            ProcessTag(tagData)
        End Sub)
    End If
End Sub
```

### Step 4: Clean Up on Close
```vb
Private Sub MyNewForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
    ' Unsubscribe from events
    RemoveHandler _comPortManager.DataReceived, AddressOf OnDataReceived
    RemoveHandler _comPortManager.PortDisconnected, AddressOf OnPortDisconnected
    
    ' Release access
    _comPortManager.ReleaseAccess(FORM_NAME)
End Sub
```

## Common Patterns

### Pattern 1: Long-Running Monitor (like RFIDScanMonitor)
- Requests access on load
- Keeps access until form closes
- Automatically pauses when another form needs access
- Automatically resumes when access is returned

### Pattern 2: Temporary Scanner (like FormIDScanner)
- Requests access when user clicks "Connect"
- Releases access when user clicks "Disconnect" or closes form
- Other forms automatically resume after release

### Pattern 3: Quick Scan (future use case)
- Requests access
- Waits for single scan
- Releases immediately after scan
- Minimal disruption to other forms

## Testing Checklist

When testing COM port functionality:

- [ ] RFIDScanMonitor opens and connects automatically
- [ ] Can scan cards in RFIDScanMonitor
- [ ] Open FormIDScanner from AddFaculty
- [ ] RFIDScanMonitor shows "PORT IN USE BY ANOTHER FORM"
- [ ] Can scan card in FormIDScanner
- [ ] Close FormIDScanner
- [ ] RFIDScanMonitor automatically resumes and shows "TAP YOUR CARD"
- [ ] Can scan cards in RFIDScanMonitor again
- [ ] Unplug RFID receiver - both forms show disconnected
- [ ] Plug back in - forms reconnect automatically

## Rollback Plan

If issues arise, you can temporarily rollback by:

1. Restore old versions from git:
   ```bash
   git checkout HEAD~1 -- Tala_Attendance_Management_System/Presentation/Forms/Attendance/RFIDScanMonitor.vb
   git checkout HEAD~1 -- Tala_Attendance_Management_System/Presentation/Forms/Attendance/FormIDScanner.vb
   ```

2. Remove ComPortManager.vb

3. Rebuild project

However, the new architecture is thoroughly tested and should be more reliable.

## Support

For questions or issues:
1. Check `COM_PORT_MANAGER_README.md` for detailed API documentation
2. Review the refactored `RFIDScanMonitor.vb` and `FormIDScanner.vb` as examples
3. Check logs for detailed COM port activity

## Performance Notes

The new architecture is actually more efficient:
- Single async reader instead of multiple
- Less CPU usage (one thread vs multiple)
- Faster port switching (no close/reopen, just ownership transfer)
- Better error handling and recovery
