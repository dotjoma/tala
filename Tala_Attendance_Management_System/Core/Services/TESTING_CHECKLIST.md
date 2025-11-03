# COM Port Manager - Testing Checklist

## Pre-Testing Setup

- [ ] RFID receiver (Silicon Labs CP210x) is connected to computer
- [ ] Database is accessible
- [ ] Application builds without errors
- [ ] Check Windows Device Manager shows COM port

## Test Scenario 1: RFIDScanMonitor Basic Operation

### Steps:
1. [ ] Launch application and login
2. [ ] Open RFIDScanMonitor form
3. [ ] Verify message shows "TAP YOUR CARD"
4. [ ] Check logs show: "✓ Successfully connected to COM[X]"
5. [ ] Scan an RFID card
6. [ ] Verify attendance card displays with teacher info
7. [ ] Wait 3 seconds, verify returns to "TAP YOUR CARD"
8. [ ] Scan same card again
9. [ ] Verify Time Out is recorded

### Expected Results:
- ✅ Auto-connects to Silicon Labs port
- ✅ Displays waiting state
- ✅ Processes RFID scans correctly
- ✅ Records attendance in database
- ✅ Shows professional attendance card

## Test Scenario 2: FormIDScanner from AddFaculty

### Steps:
1. [ ] Keep RFIDScanMonitor open
2. [ ] Navigate to Faculty → Add Faculty
3. [ ] Click "Scan RFID Card" button
4. [ ] Verify FormIDScanner opens
5. [ ] Check RFIDScanMonitor shows "PORT IN USE BY ANOTHER FORM"
6. [ ] In FormIDScanner, click "Connect"
7. [ ] Verify shows "Connected: COM[X]"
8. [ ] Scan an RFID card
9. [ ] Verify tag ID appears in textbox
10. [ ] Verify FormIDScanner closes automatically
11. [ ] Verify success dialog appears
12. [ ] Check RFIDScanMonitor returns to "TAP YOUR CARD"
13. [ ] Scan a card in RFIDScanMonitor
14. [ ] Verify it works normally

### Expected Results:
- ✅ FormIDScanner requests port access
- ✅ RFIDScanMonitor pauses automatically
- ✅ FormIDScanner connects successfully
- ✅ RFID scan works in FormIDScanner
- ✅ Tag ID is set in AddFaculty form
- ✅ RFIDScanMonitor resumes automatically
- ✅ No "Access Denied" errors

## Test Scenario 3: FormIDScanner from AddStudents

### Steps:
1. [ ] Keep RFIDScanMonitor open
2. [ ] Navigate to Students → Add Student
3. [ ] Click "Scan RFID Card" button
4. [ ] Verify FormIDScanner opens
5. [ ] Click "Connect"
6. [ ] Scan an RFID card
7. [ ] Verify tag ID is set in AddStudents form
8. [ ] Close FormIDScanner
9. [ ] Verify RFIDScanMonitor resumes

### Expected Results:
- ✅ Same smooth handoff as AddFaculty
- ✅ No conflicts or errors

## Test Scenario 4: Device Disconnection

### Steps:
1. [ ] Open RFIDScanMonitor
2. [ ] Verify connected
3. [ ] Unplug RFID receiver
4. [ ] Wait 5 seconds
5. [ ] Check RFIDScanMonitor shows "RECEIVER DISCONNECTED"
6. [ ] Plug receiver back in
7. [ ] Wait 5 seconds
8. [ ] Verify RFIDScanMonitor shows "TAP YOUR CARD"
9. [ ] Scan a card
10. [ ] Verify it works

### Expected Results:
- ✅ Detects disconnection
- ✅ Shows appropriate error message
- ✅ Auto-reconnects when plugged back in
- ✅ Resumes normal operation

## Test Scenario 5: Multiple Quick Switches

### Steps:
1. [ ] Open RFIDScanMonitor
2. [ ] Open FormIDScanner from AddFaculty
3. [ ] Close FormIDScanner immediately (don't scan)
4. [ ] Open FormIDScanner again
5. [ ] Close it again
6. [ ] Repeat 3-4 times quickly
7. [ ] Verify RFIDScanMonitor still works
8. [ ] Open FormIDScanner one more time
9. [ ] Connect and scan a card
10. [ ] Verify everything works

### Expected Results:
- ✅ No crashes or hangs
- ✅ Port access handled gracefully
- ✅ Both forms continue to work

## Test Scenario 6: No RFID Receiver Connected

### Steps:
1. [ ] Unplug RFID receiver
2. [ ] Launch application
3. [ ] Open RFIDScanMonitor
4. [ ] Verify shows "NO RFID RECEIVER DETECTED"
5. [ ] Open FormIDScanner
6. [ ] Verify shows "No RFID receiver connected"
7. [ ] Plug in receiver
8. [ ] Wait 5 seconds
9. [ ] Verify both forms detect it
10. [ ] Test scanning in both forms

### Expected Results:
- ✅ Graceful handling of missing device
- ✅ Clear error messages
- ✅ Auto-detection when plugged in

## Test Scenario 7: Concurrent Access Attempt

### Steps:
1. [ ] Open RFIDScanMonitor
2. [ ] Open FormIDScanner from AddFaculty
3. [ ] Connect in FormIDScanner
4. [ ] While FormIDScanner is open, try to scan in RFIDScanMonitor
5. [ ] Verify RFIDScanMonitor doesn't process the scan
6. [ ] Close FormIDScanner
7. [ ] Scan in RFIDScanMonitor
8. [ ] Verify it processes correctly

### Expected Results:
- ✅ Only the current owner processes scans
- ✅ No duplicate processing
- ✅ Clean ownership transfer

## Log Verification

Check the log file for these key messages:

### Successful Connection:
```
[INFO] ComPortManager instance created
[INFO] ✓ Detected Silicon Labs port: COM3 - ...
[INFO] ✓ Successfully connected to COM3
[INFO] ReadDataAsync started - waiting for RFID scans...
```

### Port Access Request:
```
[INFO] [FormIDScanner] Requesting COM port access...
[INFO] Port currently owned by [RFIDScanMonitor], requesting release...
[INFO] ✓ Access granted to [FormIDScanner]
```

### Port Release:
```
[INFO] [FormIDScanner] Releasing COM port access
[INFO] Port released by [FormIDScanner], reclaiming access...
[INFO] ✓ Access granted to [RFIDScanMonitor]
```

### RFID Scan:
```
[INFO] RFID tag scanned: '12345678'
[INFO] ========== Processing RFID tag: 12345678 ==========
[INFO] ✓ Teacher found: DOE, J., Employee ID: EMP001
```

## Performance Checks

- [ ] Port switching happens in < 500ms
- [ ] No noticeable lag when opening FormIDScanner
- [ ] RFID scans are processed immediately
- [ ] No memory leaks (check Task Manager over time)
- [ ] CPU usage stays low (< 5% when idle)

## Error Conditions to Test

- [ ] Scan unregistered RFID card → Shows "RFID not registered"
- [ ] Scan same card within 60 seconds → Shows "PLEASE WAIT X SECONDS"
- [ ] Database connection fails → Graceful error handling
- [ ] COM port in use by another app → Shows appropriate error

## Regression Testing

Verify existing functionality still works:

- [ ] Manual attendance entry
- [ ] Attendance reports
- [ ] Faculty management
- [ ] Student management
- [ ] All other forms open/close normally

## Sign-Off

- [ ] All test scenarios passed
- [ ] No errors in logs
- [ ] Performance is acceptable
- [ ] User experience is smooth
- [ ] Ready for production

**Tested By:** _______________  
**Date:** _______________  
**Version:** 2.3.0  
**Notes:** _______________________________________________
