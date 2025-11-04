@echo off
setlocal EnableDelayedExpansion

set BASE_URL=http://localhost:5000/api
set ADMIN_ID=00000000-0000-0000-0000-000000000001

echo Testing Activator API endpoints...
echo.

:: 1. Create clinician user
echo Creating clinician...
curl -X POST %BASE_URL%/users ^
  -H "Content-Type: application/json" ^
  -H "X-User-Id: %ADMIN_ID%" ^
  -H "X-User-Role: Admin" ^
  -d "{\"username\":\"clinician1\",\"role\":1}"
echo.

:: Wait a bit
timeout /t 2 > nul

:: 2. Create patient user
echo Creating patient...
curl -X POST %BASE_URL%/users ^
  -H "Content-Type: application/json" ^
  -H "X-User-Id: %ADMIN_ID%" ^
  -H "X-User-Role: Admin" ^
  -d "{\"username\":\"patient1\",\"role\":0}"
echo.

:: Store the IDs (in real script we'd parse the JSON responses)
set CLINICIAN_ID=test-clinician-id
set PATIENT_ID=test-patient-id

:: 3. Upload pressure frame
echo Uploading pressure frame...
curl -X POST %BASE_URL%/frames/upload ^
  -H "Content-Type: application/json" ^
  -H "X-User-Id: %PATIENT_ID%" ^
  -H "X-User-Role: User" ^
  -d "{\"userId\":\"%PATIENT_ID%\",\"timestamp\":\"2025-11-01T10:00:00Z\",\"csvPayload\":\"1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1\"}"
echo.

:: 4. Add comment
echo Adding comment...
curl -X POST %BASE_URL%/comments ^
  -H "Content-Type: application/json" ^
  -H "X-User-Id: %PATIENT_ID%" ^
  -H "X-User-Role: User" ^
  -d "{\"frameId\":\"test-frame-id\",\"userId\":\"%PATIENT_ID%\",\"text\":\"Test comment\",\"timestamp\":\"2025-11-01T10:00:00Z\"}"
echo.

:: 5. Get metrics
echo Getting metrics...
curl "%BASE_URL%/metrics/user/%PATIENT_ID%?period=1h" ^
  -H "X-User-Id: %CLINICIAN_ID%" ^
  -H "X-User-Role: Clinician"
echo.

:: 6. Get report
echo Getting report...
curl "%BASE_URL%/reports/user/%PATIENT_ID%?period=24h" ^
  -H "X-User-Id: %CLINICIAN_ID%" ^
  -H "X-User-Role: Clinician"
echo.

echo.
echo Test complete!
pause