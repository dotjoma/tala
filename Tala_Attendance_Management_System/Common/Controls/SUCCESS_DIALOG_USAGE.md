# SuccessDialog Control - Usage Guide

Animated success dialog with customizable options.

## Features
- Animated checkmark drawing
- Customizable message
- Auto-close with timer
- Optional buttons (Yes/No, OK, or none)

## Usage Examples

### 1. Simple Success Message (Auto-close after 3 seconds)
```vb
SuccessDialog.ShowSuccess("Record saved successfully!", 3)
```

### 2. Success Message (No auto-close, no buttons - click anywhere to close)
```vb
SuccessDialog.ShowSuccess("Data exported successfully!")
```

### 3. Success with OK Button
```vb
Dim result = SuccessDialog.ShowWithOk("Faculty member added successfully!")
If result = DialogResult.OK Then
    ' User clicked OK
End If
```

### 4. Success with Yes/No Buttons
```vb
Dim result = SuccessDialog.ShowWithYesNo("Record updated! Do you want to add another?")
If result = DialogResult.Yes Then
    ' User clicked Yes
    AddAnotherRecord()
ElseIf result = DialogResult.No Then
    ' User clicked No
    CloseForm()
End If
```

### 5. Custom Configuration
```vb
Using dialog As New SuccessDialog()
    dialog.Message = "Attendance recorded successfully!" & vbCrLf & "Time: " & DateTime.Now.ToString("hh:mm tt")
    dialog.AutoCloseSeconds = 5 ' Auto-close after 5 seconds
    dialog.ShowOk = True ' Show OK button
    Dim result = dialog.ShowDialog()
End Using
```

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Message` | String | "Your action has been completed successfully." | The message to display below "Success!" |
| `AutoCloseSeconds` | Integer | 0 | Seconds before auto-close (0 = no auto-close) |
| `ShowYesNo` | Boolean | False | Show Yes/No buttons |
| `ShowOk` | Boolean | False | Show OK button |
| `DialogResultValue` | DialogResult | None | The result when dialog closes |

## Static Methods

### `ShowSuccess(message, autoCloseSeconds)`
Shows a simple success message with optional auto-close.

**Parameters:**
- `message` (String): The message to display
- `autoCloseSeconds` (Integer, Optional): Seconds before auto-close (default: 0)

**Returns:** DialogResult

### `ShowWithYesNo(message)`
Shows success dialog with Yes/No buttons.

**Parameters:**
- `message` (String): The message to display

**Returns:** DialogResult (Yes or No)

### `ShowWithOk(message)`
Shows success dialog with OK button.

**Parameters:**
- `message` (String): The message to display

**Returns:** DialogResult (OK)

## Design Specifications

- **Size:** 450 x 400 pixels
- **Checkmark Circle:** 120px diameter, green (#2ECC71)
- **Animation:** Smooth checkmark drawing animation (1 second)
- **Colors:**
  - Success text: Dark gray (#34495E)
  - Message text: Light gray (#7F8C8D)
  - Yes button: Green (#2ECC71)
  - No button: Red (#E74C3C)
  - OK button: Blue (#3498DB)

## Common Use Cases

### After Saving Data
```vb
Try
    SaveData()
    SuccessDialog.ShowSuccess("Data saved successfully!", 2)
Catch ex As Exception
    MessageBox.Show("Error: " & ex.Message)
End Try
```

### After Deleting with Confirmation
```vb
Dim result = SuccessDialog.ShowWithYesNo("Record deleted! Delete another?")
If result = DialogResult.Yes Then
    ' Continue deleting
End If
```

### RFID Scan Success
```vb
SuccessDialog.ShowSuccess("Attendance recorded!" & vbCrLf & "Welcome, " & studentName, 2)
```

### Batch Operation Complete
```vb
SuccessDialog.ShowWithOk($"{recordCount} records processed successfully!")
```

## Notes

- The dialog is modal and centers on the parent form
- The checkmark animates smoothly when the dialog appears
- If no buttons are shown and no auto-close is set, clicking anywhere closes the dialog
- The dialog has rounded corners for a modern look
- All buttons have hover effects and proper cursor changes
