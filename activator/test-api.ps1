# Test script for Activator API endpoints
$baseUrl = "http://localhost:5002/api"
$adminId = "00000000-0000-0000-0000-000000000001"

# Helper function to make HTTP requests
function Invoke-ApiRequest {
    param (
        [string]$Method,
        [string]$Endpoint,
        [string]$UserId,
        [string]$Role,
        $Body
    )
    
    $headers = @{
        'Content-Type' = 'application/json'
        'X-User-Id' = if ($UserId) { $UserId } else { $adminId }
        'X-User-Role' = $Role
    }
    
    $params = @{
        Method = $Method
        Uri = "$baseUrl/$endpoint"
        Headers = $headers
        ContentType = 'application/json'
    }
    
    if ($Body) {
        $params.Body = ($Body | ConvertTo-Json)
    }
    
    try {
        $response = Invoke-RestMethod @params
        Write-Host "Success: $Method $Endpoint"
        return $response
    }
    catch {
        Write-Host "Error: $Method $Endpoint - $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# 1. Create a clinician user
Write-Host "`nCreating clinician..." -ForegroundColor Yellow
$clinician = Invoke-ApiRequest -Method 'POST' -Endpoint 'users' `
    -UserId $adminId -Role 'Admin' `
    -Body @{
        username = "clinician1"
        role = 1  # Clinician
    }
$clinicianId = $clinician.id

# 2. Create a patient user
Write-Host "`nCreating patient..." -ForegroundColor Yellow
$patient = Invoke-ApiRequest -Method 'POST' -Endpoint 'users' `
    -UserId $adminId -Role 'Admin' `
    -Body @{
        username = "patient1"
        role = 0  # User/Patient
    }
$patientId = $patient.id

# 3. Generate a sample pressure frame CSV (16x16 high pressure in center)
$csv = (1..32 | ForEach-Object {
    $row = $_
    (1..32 | ForEach-Object {
        $col = $_
        if (($row -ge 8 -and $row -lt 24) -and ($col -ge 8 -and $col -lt 24)) { 220 } else { 1 }
    }) -join ','
}) -join "`n"

# 4. Upload pressure frame
Write-Host "`nUploading pressure frame..." -ForegroundColor Yellow
$frame = Invoke-ApiRequest -Method 'POST' -Endpoint 'frames/upload' `
    -UserId $patientId -Role 'User' `
    -Body @{
        userId = $patientId
        timestamp = (Get-Date).ToString('o')
        csvPayload = $csv
    }
$frameId = $frame.frameId

# 5. Add a comment to the frame
Write-Host "`nAdding comment..." -ForegroundColor Yellow
$comment = Invoke-ApiRequest -Method 'POST' -Endpoint 'comments' `
    -UserId $patientId -Role 'User' `
    -Body @{
        frameId = $frameId
        userId = $patientId
        text = "I feel some discomfort in this region"
        timestamp = (Get-Date).ToString('o')
    }
Write-Host "Comment added:" -ForegroundColor Cyan
$comment | Format-List

# 6. Get comments for the frame
Write-Host "`nGetting comments..." -ForegroundColor Yellow
$comments = Invoke-ApiRequest -Method 'GET' -Endpoint "comments/frame/$frameId" `
    -UserId $clinicianId -Role 'Clinician'
Write-Host "Comments retrieved:" -ForegroundColor Cyan
$comments | Format-Table -AutoSize

# 7. Get metrics for last hour
Write-Host "`nGetting metrics..." -ForegroundColor Yellow
Write-Host "PatientId before metrics call: $patientId" -ForegroundColor Cyan
$metricsEndpoint = "metrics/user/$patientId`?period=1h"
$metrics = Invoke-ApiRequest -Method 'GET' -Endpoint $metricsEndpoint `
    -UserId $clinicianId -Role 'Clinician'
Write-Host "Metrics for last hour:" -ForegroundColor Cyan
$metrics | Format-Table -AutoSize

# 8. Get a simple report
Write-Host "`nGetting report..." -ForegroundColor Yellow
$reportEndpoint = "reports/user/$patientId`?period=24h"
$report = Invoke-ApiRequest -Method 'GET' -Endpoint $reportEndpoint `
    -UserId $clinicianId -Role 'Clinician'
Write-Host "24-hour report:" -ForegroundColor Cyan
$report | Format-List

Write-Host "`nTest run complete!" -ForegroundColor Green
Write-Host "Created: Clinician ID: $clinicianId"
Write-Host "Created: Patient ID: $patientId"
Write-Host "Created: Frame ID: $frameId"