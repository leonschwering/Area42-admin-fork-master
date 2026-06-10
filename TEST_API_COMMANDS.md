# 🧪 Quick API Testing Commands

## Test in PowerShell (Windows)

### Test 1: Check API is Running
```powershell
$response = Invoke-WebRequest -Uri "https://localhost:7001/" -SkipCertificateCheck
Write-Host "API Status: $($response.StatusCode)"
Write-Host "Response: $($response.Content)"

# Expected output:
# API Status: 200
# Response: Area42 Reservation API is running.
```

### Test 2: Get All Accommodations
```powershell
$response = Invoke-WebRequest -Uri "https://localhost:7001/api/accommodations" -SkipCertificateCheck
$accommodations = $response.Content | ConvertFrom-Json
Write-Host "Found $($accommodations.Count) accommodations"
$accommodations | ConvertTo-Json

# Expected output:
# List of all accommodations with IDs, names, types, prices, etc.
```

### Test 3: Get Accommodations by Type (Bungalow = 0)
```powershell
$response = Invoke-WebRequest -Uri "https://localhost:7001/api/accommodations/type/0" -SkipCertificateCheck
$accommodations = $response.Content | ConvertFrom-Json
Write-Host "Found $($accommodations.Count) bungalows"
$accommodations | Format-Table Name, Price, GuestCapacity -AutoSize

# Expected output:
# List of bungalow accommodations
```

### Test 4: Register New User
```powershell
$body = @{
    email = "testuser@area42.nl"
    firstName = "John"
    lastName = "Doe"
    password = "Test123!"
    confirmPassword = "Test123!"
} | ConvertTo-Json

$response = Invoke-WebRequest -Uri "https://localhost:7001/api/auth/register" `
    -Method POST `
    -ContentType "application/json" `
    -Body $body `
    -SkipCertificateCheck

$result = $response.Content | ConvertFrom-Json
Write-Host "Registration Success: $($result.success)"
Write-Host "Token: $($result.token)"
Write-Host "User: $($result.user.email)"

# Expected output:
# Registration Success: True
# Token: eyJ0eXAiOiJKV1QiLCJhbGc... (long JWT token)
# User: testuser@area42.nl
```

### Test 5: Login User
```powershell
$body = @{
    email = "testuser@area42.nl"
    password = "Test123!"
} | ConvertTo-Json

$response = Invoke-WebRequest -Uri "https://localhost:7001/api/auth/login" `
    -Method POST `
    -ContentType "application/json" `
    -Body $body `
    -SkipCertificateCheck

$result = $response.Content | ConvertFrom-Json
Write-Host "Login Success: $($result.success)"
Write-Host "Token: $($result.token.Substring(0, 50))..."
$token = $result.token

# Expected output:
# Login Success: True
# Token: eyJ0eXAiOiJKV1QiLCJhbGc...
```

### Test 6: Get Accommodation by ID
```powershell
# First, get an accommodation to get its ID
$response = Invoke-WebRequest -Uri "https://localhost:7001/api/accommodations" -SkipCertificateCheck
$accommodations = $response.Content | ConvertFrom-Json

if ($accommodations.Count -gt 0) {
    $id = $accommodations[0].id
    $response = Invoke-WebRequest -Uri "https://localhost:7001/api/accommodations/$id" -SkipCertificateCheck
    $accommodation = $response.Content | ConvertFrom-Json
    Write-Host "Accommodation: $($accommodation.name)"
    Write-Host "Price: $($accommodation.price)"
    Write-Host "Type: $($accommodation.type)"
} else {
    Write-Host "No accommodations found"
}

# Expected output:
# Single accommodation details
```

### Test 7: Check Availability (No Auth Required)
```powershell
# Check if an accommodation is available for dates
$accommodationId = "00000000-0000-0000-0000-000000000001"  # Replace with real ID
$checkIn = [DateTime]::Now.AddDays(1).ToString("yyyy-MM-dd")
$checkOut = [DateTime]::Now.AddDays(5).ToString("yyyy-MM-dd")

$response = Invoke-WebRequest -Uri "https://localhost:7001/api/reservations/availability/check?accommodationId=$accommodationId&checkIn=$checkIn&checkOut=$checkOut" `
    -SkipCertificateCheck

$result = $response.Content | ConvertFrom-Json
Write-Host "Available: $($result.available)"

# Expected output:
# Available: true or false
```

### Test 8: Get User Reservations (Requires Auth Token)
```powershell
# You need the JWT token from login
# Replace with your actual token from Test 5
$token = "eyJ0eXAiOiJKV1QiLCJhbGc..."

$headers = @{
    "Authorization" = "Bearer $token"
}

$response = Invoke-WebRequest -Uri "https://localhost:7001/api/reservations/user/00000000-0000-0000-0000-000000000001" `
    -Headers $headers `
    -SkipCertificateCheck

$reservations = $response.Content | ConvertFrom-Json
Write-Host "User has $($reservations.Count) reservations"
$reservations | Format-Table CheckinDate, CheckoutDate, AccommodationId -AutoSize

# Expected output:
# User reservations list (might be empty if no bookings)
```

---

## Full Automated Test Script

Save this as `test-api.ps1`:

```powershell
#!/usr/bin/env pwsh

# Color output
function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Error { Write-Host $args -ForegroundColor Red }
function Write-Info { Write-Host $args -ForegroundColor Cyan }

Write-Info "================================"
Write-Info "Area42 API Test Suite"
Write-Info "================================`n"

$baseUrl = "https://localhost:7001"
$token = $null

# Test 1: API Health
Write-Info "[1/8] Testing API health..."
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/" -SkipCertificateCheck
    Write-Success "✓ API is running (Status: $($response.StatusCode))"
} catch {
    Write-Error "✗ API is not responding"
    exit 1
}

# Test 2: Get Accommodations
Write-Info "`n[2/8] Getting all accommodations..."
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/accommodations" -SkipCertificateCheck
    $accommodations = $response.Content | ConvertFrom-Json
    Write-Success "✓ Found $($accommodations.Count) accommodations"
} catch {
    Write-Error "✗ Failed to get accommodations: $_"
}

# Test 3: Get Bungalows
Write-Info "`n[3/8] Getting bungalows (type=0)..."
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/accommodations/type/0" -SkipCertificateCheck
    $bungalows = $response.Content | ConvertFrom-Json
    Write-Success "✓ Found $($bungalows.Count) bungalows"
} catch {
    Write-Error "✗ Failed to get bungalows: $_"
}

# Test 4: Register User
Write-Info "`n[4/8] Registering test user..."
try {
    $uniqueEmail = "test-$(Get-Random)@area42.nl"
    $body = @{
        email = $uniqueEmail
        firstName = "Test"
        lastName = "User"
        password = "Test123!"
        confirmPassword = "Test123!"
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "$baseUrl/api/auth/register" `
        -Method POST `
        -ContentType "application/json" `
        -Body $body `
        -SkipCertificateCheck

    $result = $response.Content | ConvertFrom-Json
    if ($result.success) {
        Write-Success "✓ User registered: $uniqueEmail"
        Write-Info "  Token length: $($result.token.Length) chars"
    } else {
        Write-Error "✗ Registration failed: $($result.message)"
    }
} catch {
    Write-Error "✗ Registration error: $_"
}

# Test 5: Login User
Write-Info "`n[5/8] Logging in user..."
try {
    $body = @{
        email = $uniqueEmail
        password = "Test123!"
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "$baseUrl/api/auth/login" `
        -Method POST `
        -ContentType "application/json" `
        -Body $body `
        -SkipCertificateCheck

    $result = $response.Content | ConvertFrom-Json
    if ($result.success) {
        Write-Success "✓ Login successful"
        $token = $result.token
        Write-Info "  User: $($result.user.email)"
        Write-Info "  Role: $($result.user.role)"
    } else {
        Write-Error "✗ Login failed: $($result.message)"
    }
} catch {
    Write-Error "✗ Login error: $_"
}

# Test 6: Get Specific Accommodation
Write-Info "`n[6/8] Getting specific accommodation..."
try {
    if ($accommodations.Count -gt 0) {
        $id = $accommodations[0].id
        $response = Invoke-WebRequest -Uri "$baseUrl/api/accommodations/$id" -SkipCertificateCheck
        $accommodation = $response.Content | ConvertFrom-Json
        Write-Success "✓ Retrieved: $($accommodation.name)"
        Write-Info "  Price: €$($accommodation.basePrice)"
        Write-Info "  Type: $($accommodation.type)"
    } else {
        Write-Error "✗ No accommodations available to test"
    }
} catch {
    Write-Error "✗ Failed to get accommodation: $_"
}

# Test 7: Check Availability
Write-Info "`n[7/8] Checking availability..."
try {
    if ($accommodations.Count -gt 0) {
        $id = $accommodations[0].id
        $checkIn = [DateTime]::Now.AddDays(1).ToString("yyyy-MM-dd")
        $checkOut = [DateTime]::Now.AddDays(5).ToString("yyyy-MM-dd")

        $response = Invoke-WebRequest -Uri "$baseUrl/api/reservations/availability/check?accommodationId=$id&checkIn=$checkIn&checkOut=$checkOut" `
            -SkipCertificateCheck

        $result = $response.Content | ConvertFrom-Json
        Write-Success "✓ Availability checked: $($result.available)"
    }
} catch {
    Write-Error "✗ Availability check failed: $_"
}

# Test 8: Authenticated Request
Write-Info "`n[8/8] Testing authenticated request..."
try {
    if ($token) {
        $headers = @{
            "Authorization" = "Bearer $token"
        }
        $userId = $result.user.id
        $response = Invoke-WebRequest -Uri "$baseUrl/api/reservations/user/$userId" `
            -Headers $headers `
            -SkipCertificateCheck

        $reservations = $response.Content | ConvertFrom-Json
        Write-Success "✓ Authenticated request successful"
        Write-Info "  User reservations: $($reservations.Count)"
    }
} catch {
    Write-Error "✗ Authenticated request failed: $_"
}

Write-Info "`n================================"
Write-Success "All tests completed!"
Write-Info "================================"
```

### Run the script:
```powershell
# Give it execute permission (first time only)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Run it
.\test-api.ps1
```

---

## Test Results You Should See

```
================================
Area42 API Test Suite
================================

[1/8] Testing API health...
✓ API is running (Status: 200)

[2/8] Getting all accommodations...
✓ Found 6 accommodations

[3/8] Getting bungalows (type=0)...
✓ Found 2 bungalows

[4/8] Registering test user...
✓ User registered: test-12345@area42.nl
  Token length: 500 chars

[5/8] Logging in user...
✓ Login successful
  User: test-12345@area42.nl
  Role: Customer

[6/8] Getting specific accommodation...
✓ Retrieved: Modern Family Bungalow
  Price: €150
  Type: 0

[7/8] Checking availability...
✓ Availability checked: true

[8/8] Testing authenticated request...
✓ Authenticated request successful
  User reservations: 0

================================
All tests completed!
================================
```

---

## What Each Test Verifies

| Test | Verifies |
|------|----------|
| 1 | API is running and responding |
| 2 | Can fetch all accommodations |
| 3 | Can filter accommodations by type |
| 4 | User registration endpoint works |
| 5 | Login and JWT token generation works |
| 6 | Can get specific accommodation by ID |
| 7 | Availability checking works |
| 8 | JWT authentication/authorization works |

---

## 🎯 Quick Start

1. **Start services:**
   ```powershell
   .\START_AREA42.bat
   ```

2. **Run tests:**
   ```powershell
   # Quick test
   $response = Invoke-WebRequest -Uri "https://localhost:7001/api/accommodations" -SkipCertificateCheck
   $response.Content | ConvertFrom-Json | ConvertTo-Json

   # Or run full test suite
   .\test-api.ps1
   ```

3. **Check frontend:**
   ```
   https://localhost:7000/accommodations
   ```

All endpoints should now return 200 status codes! ✅
