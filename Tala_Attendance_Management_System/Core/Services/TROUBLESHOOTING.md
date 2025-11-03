# COM Port Manager - Troubleshooting Guide

## Issue: Scan Doesn't Work

### Step 1: Check the Logs

The application logs everything. Check the log file for these messages:

**Location:** Look for log files in your application directory (usually `Logs/` folder)

### Step 2: What to Look For in Logs

#### Connection Messages:
```
[INFO] ComPortManager instance created
[INFO] Searching for Silicon Labs RFID receiver...
[INFO] ✓ Detected Silicon Labs port: COM3 - ...
[INFO] ✓ Successfully connected to COM3
[INFO] ReadDataAsync started - waiting for RFID scans...
```

If you DON'T see these, the port isn't connecting.

#### When You Scan a Card:
```
[INFO] RFID tag scanned (raw): '12345678' (length: 8)
[INFO] Raising DataReceived event with tag: '12345678'
[INFO] DataReceived event raised successfully
[INFO] OnDataReceived called with tag: '12345678', CurrentOwner: 'RFIDScanMonitor', FORM_NAME: 'RFIDScanMonitor'
[INFO] We are the owner, invoking ProcessRFIDTag...
[INFO] ========== Processing RFID tag: 12345678 ==========
```

If you see "RFID tag scanned" but NOT "OnDataReceived called", the event isn't being subscribed to properly.

### Step 3: Common Issues

#### Issue 1: No Connection
**Symptoms:** Form shows "NO RFID RECEIVER DETECTED" or "CONNECTION FAILED"

**Checks:**
1. Is the RFID receiver plugged in?
2. Check Windows Device Manager - do you see "Silicon Labs CP210x USB to UART Bridge"?
3. What COM port is it on? (e.g., COM3, COM4)
4. Is another application using the port? (Close other apps)

**Logs to check:**
```
[WARNING] No Silicon Labs RFID receiver detected
[ERROR] Failed to connect to any COM port
```

#### Issue 2: Connected But No Scans
**Symptoms:** Form shows "TAP YOUR CARD" but scanning does nothing

**Checks:**
1. Check logs for "ReadDataAsync started" - if missing, async reader didn't start
2. Scan a card and check logs for "RFID tag scanned" - if missing, no data is coming from device
3. Check if RFID receiver has power (LED should be on)
4. Try scanning a known-good card

**Logs to check:**
```
[INFO] ReadDataAsync started - waiting for RFID scans...
```

If you see this but no "RFID tag scanned" when you scan, the hardware isn't sending data.

#### Issue 3: Scans But Doesn't Process
**Symptoms:** Logs show "RFID tag scanned" but nothing happens on screen

**Checks:**
1. Check if event is being raised: Look for "Raising DataReceived event"
2. Check if event is being received: Look for "OnDataReceived called"
3. Check ownership: Look for "CurrentOwner" in logs

**Logs to check:**
```
[INFO] RFID tag scanned (raw): '12345678'
[INFO] Raising DataReceived event with tag: '12345678'
[INFO] DataReceived event raised successfully
```

If you see these but NOT "OnDataReceived called", the form isn't subscribed to the event.

#### Issue 4: Wrong Form Processes Scan
**Symptoms:** Scan works in one form but not the other

**Checks:**
1. Check CurrentOwner in logs
2. Verify the form requested access: Look for "Requesting COM port access"
3. Verify access was granted: Look for "✓ Access granted to [FormName]"

**Logs to check:**
```
[INFO] [FormIDScanner] Requesting COM port access...
[INFO] ✓ Access granted to [FormIDScanner]
[INFO] OnDataReceived called with tag: '12345678', CurrentOwner: 'FormIDScanner'
```

### Step 4: Enable Debug Logging

The code now has extensive logging. Just run the application and check the logs after each action.

### Step 5: Manual Testing

#### Test 1: Check COM Port in Device Manager
1. Open Device Manager (Win + X, then M)
2. Expand "Ports (COM & LPT)"
3. Look for "Silicon Labs CP210x USB to UART Bridge (COM3)" or similar
4. Note the COM port number

#### Test 2: Test with Another Application
Use a serial terminal (like PuTTY or Arduino Serial Monitor):
1. Connect to the COM port at 115200 baud
2. Scan a card
3. You should see the tag ID appear

If this doesn't work, the hardware/driver has an issue.

#### Test 3: Check Application Logs
1. Run the application
2. Open RFIDScanMonitor
3. Check logs for connection messages
4. Scan a card
5. Check logs for scan messages

### Step 6: Quick Fixes

#### Fix 1: Restart Application
Sometimes the port gets stuck. Close and reopen the application.

#### Fix 2: Replug Device
Unplug the RFID receiver, wait 5 seconds, plug it back in.

#### Fix 3: Check Driver
Update the Silicon Labs CP210x driver from their website.

#### Fix 4: Try Different USB Port
Some USB ports have power issues. Try a different port.

### Step 7: Report Issue

If none of the above works, provide:

1. **Log file** - The complete log from application start to when you scanned
2. **COM port number** - From Device Manager
3. **What form** - RFIDScanMonitor or FormIDScanner?
4. **What happens** - Exactly what you see on screen
5. **What you expected** - What should happen

### Debug Checklist

Run through this checklist:

- [ ] RFID receiver is plugged in
- [ ] Device Manager shows Silicon Labs device
- [ ] Application starts without errors
- [ ] Form opens (RFIDScanMonitor or FormIDScanner)
- [ ] Logs show "ComPortManager instance created"
- [ ] Logs show "✓ Successfully connected to COM[X]"
- [ ] Logs show "ReadDataAsync started"
- [ ] When I scan, logs show "RFID tag scanned"
- [ ] Logs show "Raising DataReceived event"
- [ ] Logs show "OnDataReceived called"
- [ ] Logs show correct CurrentOwner
- [ ] Screen updates with scan result

**Where did it fail?** That's where the problem is!

### Common Log Patterns

#### Successful Scan in RFIDScanMonitor:
```
[INFO] RFID tag scanned (raw): '12345678' (length: 8)
[INFO] Raising DataReceived event with tag: '12345678'
[INFO] DataReceived event raised successfully
[INFO] OnDataReceived called with tag: '12345678', CurrentOwner: 'RFIDScanMonitor', FORM_NAME: 'RFIDScanMonitor'
[INFO] We are the owner, invoking ProcessRFIDTag...
[INFO] ========== Processing RFID tag: 12345678 ==========
[INFO] Database connected
[INFO] ✓ Teacher found: DOE, J., Employee ID: EMP001
```

#### Successful Scan in FormIDScanner:
```
[INFO] RFID tag scanned (raw): '12345678' (length: 8)
[INFO] Raising DataReceived event with tag: '12345678'
[INFO] DataReceived event raised successfully
[INFO] FormIDScanner.OnDataReceived called with tag: '12345678', CurrentOwner: 'FormIDScanner', FORM_NAME: 'FormIDScanner'
[INFO] ✓ Tag ID received: 12345678, setting txtTagID...
[INFO] txtTagID.Text set to: 12345678
```

#### Port Handoff (RFIDScanMonitor → FormIDScanner):
```
[INFO] [FormIDScanner] Requesting COM port access...
[INFO] Port currently owned by [RFIDScanMonitor], requesting release...
[INFO] ✓ Access granted to [FormIDScanner]
```

### Still Not Working?

Check these advanced issues:

1. **Antivirus blocking COM port access** - Temporarily disable and test
2. **Windows permissions** - Run as Administrator
3. **Multiple instances** - Only run one instance of the application
4. **Port already open** - Check Task Manager for other processes using the port
5. **Corrupted driver** - Uninstall and reinstall Silicon Labs driver

### Contact Support

If you've tried everything and it still doesn't work, provide:
- Complete log file
- Screenshot of Device Manager showing COM ports
- Screenshot of the form when you try to scan
- Windows version
- Application version
