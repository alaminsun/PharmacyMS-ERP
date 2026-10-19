# Smoke test for a LOCAL DEVELOPMENT PharmacyMS database only.
# Requirements: running API, migrated SQL Server LocalDB, an existing Admin account.
# Creates three uniquely named TEST users and one TEST role. Does not delete records.
param([string]$BaseUrl = 'http://127.0.0.1:5080')

$ErrorActionPreference = 'Stop'
$uri = [Uri]$BaseUrl
if ($uri.Host -notin @('localhost','127.0.0.1','::1')) {
    throw 'Safety check: only localhost URLs are allowed for this data-writing smoke test.'
}
$BaseUrl = $BaseUrl.TrimEnd('/')
$script:Passed = 0
$script:Failed = 0

function Call-Api {
    param([string]$Method,[string]$Path,[object]$Body=$null,[string]$Token='')
    $headers = @{}
    if ($Token) { $headers['Authorization'] = "Bearer $Token" }
    $args = @{Method=$Method; Uri="$BaseUrl$Path"; Headers=$headers; UseBasicParsing=$true; ErrorAction='Stop'}
    if ($null -ne $Body) {
        $args.ContentType = 'application/json'
        $args.Body = ConvertTo-Json -InputObject $Body -Depth 12 -Compress
    }
    try {
        $response = Invoke-WebRequest @args
        $data = $null
        if ($response.Content) { $data = $response.Content | ConvertFrom-Json }
        return @{Status=[int]$response.StatusCode; Data=$data}
    }
    catch {
        $status = 0
        if ($_.Exception.Response) { $status = [int]$_.Exception.Response.StatusCode }
        if (-not $status) { throw "Could not connect to $BaseUrl$Path : $($_.Exception.Message)" }
        return @{Status=$status; Data=$null}
    }
}
function Expect {
    param([string]$Label,[int]$Actual,[int]$Expected)
    if ($Actual -eq $Expected) { $script:Passed++; Write-Host "PASS [$Actual] $Label" -ForegroundColor Green }
    else { $script:Failed++; Write-Host "FAIL [$Actual / expected $Expected] $Label" -ForegroundColor Red }
}
function Check {
    param([string]$Label,[bool]$Condition)
    if ($Condition) { $script:Passed++; Write-Host "PASS $Label" -ForegroundColor Green }
    else { $script:Failed++; Write-Host "FAIL $Label" -ForegroundColor Red }
}
function Get-PlainPassword {
    param([string]$Caption)
    $secure = Read-Host -Prompt $Caption -AsSecureString
    $ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try { return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr) }
}
function Login {
    param([string]$Email,[string]$Password)
    Call-Api 'POST' '/api/auth/login' @{email=$Email;password=$Password}
}

Write-Host 'This writes TEST users and a TEST role to your local development database.' -ForegroundColor Yellow
$adminEmail = Read-Host 'Existing Admin email'
$adminPassword = Get-PlainPassword 'Existing Admin password'
$testPassword = Get-PlainPassword 'TEST account password (12+ chars, upper/lower/digit/special)'
$admin = Login $adminEmail $adminPassword
Expect 'Admin login' $admin.Status 200
if ($admin.Status -ne 200) { throw 'Cannot continue without an Admin token.' }
$adminToken = $admin.Data.accessToken

Expect 'Anonymous GET medicines blocked' (Call-Api 'GET' '/api/medicines').Status 401
Expect 'Admin can list roles' (Call-Api 'GET' '/api/admin/roles' -Token $adminToken).Status 200
$access = Call-Api 'GET' '/api/auth/me/permissions' -Token $adminToken
Expect 'Admin effective permissions endpoint' $access.Status 200
if ($access.Status -eq 200) {
    Check 'Admin receives Users.Manage and Roles.Manage' (('Users.Manage' -in @($access.Data.permissions)) -and ('Roles.Manage' -in @($access.Data.permissions)))
}

$seed = Call-Api 'POST' '/api/admin/roles/seed-defaults' -Token $adminToken
Expect 'Seed only missing default roles' $seed.Status 200
$nonce = [Guid]::NewGuid().ToString('N').Substring(0,10)
$cashierEmail = "smoke.cashier.$nonce@example.test"
$pharmacistEmail = "smoke.pharmacist.$nonce@example.test"
$probeEmail = "smoke.probe.$nonce@example.test"
$probeRole = "RBAC_Probe_$nonce"

Expect 'Create Cashier test user' (Call-Api 'POST' '/api/admin/users' @{email=$cashierEmail;password=$testPassword;role='Cashier'} -Token $adminToken).Status 201
Expect 'Create Pharmacist test user' (Call-Api 'POST' '/api/admin/users' @{email=$pharmacistEmail;password=$testPassword;role='Pharmacist'} -Token $adminToken).Status 201
Expect 'Create isolated probe role' (Call-Api 'POST' '/api/admin/roles' @{name=$probeRole} -Token $adminToken).Status 200
Expect 'Non-Admin role cannot be granted Users.Manage' (Call-Api 'POST' '/api/admin/roles/grant-permission' @{role=$probeRole;permission='Users.Manage'} -Token $adminToken).Status 400
Expect 'Grant probe Medicines.Read' (Call-Api 'POST' '/api/admin/roles/grant-permission' @{role=$probeRole;permission='Medicines.Read'} -Token $adminToken).Status 204
Expect 'Create probe test user' (Call-Api 'POST' '/api/admin/users' @{email=$probeEmail;password=$testPassword;role=$probeRole} -Token $adminToken).Status 201

$cashier = Login $cashierEmail $testPassword
$pharmacist = Login $pharmacistEmail $testPassword
$probe = Login $probeEmail $testPassword
Expect 'Cashier login' $cashier.Status 200
Expect 'Pharmacist login' $pharmacist.Status 200
Expect 'Probe login' $probe.Status 200
if ($cashier.Status -eq 200) {
    $token = $cashier.Data.accessToken
    Expect 'Cashier can read medicines' (Call-Api 'GET' '/api/medicines' -Token $token).Status 200
    Expect 'Cashier can read sales' (Call-Api 'GET' '/api/sales' -Token $token).Status 200
    Expect 'Cashier denied purchasing' (Call-Api 'GET' '/api/purchases' -Token $token).Status 403
    Expect 'Cashier denied reports' (Call-Api 'GET' '/api/reports?fromUtc=2026-10-01T00:00:00Z&toUtc=2026-11-01T00:00:00Z' -Token $token).Status 403
    Expect 'Cashier denied user management' (Call-Api 'GET' '/api/admin/users' -Token $token).Status 403
    Expect 'Cashier denied medicine write' (Call-Api 'POST' '/api/medicines' @{} -Token $token).Status 403
    Expect 'Deactivate test Cashier' (Call-Api 'PUT' '/api/admin/users/status' @{email=$cashierEmail;isActive=$false} -Token $adminToken).Status 204
    Expect 'Deactivated Cashier old JWT is rejected' (Call-Api 'GET' '/api/medicines' -Token $token).Status 401
    Expect 'Deactivated Cashier cannot log in' (Login $cashierEmail $testPassword).Status 401
    Expect 'Reactivate test Cashier' (Call-Api 'PUT' '/api/admin/users/status' @{email=$cashierEmail;isActive=$true} -Token $adminToken).Status 204
    $reactivated = Login $cashierEmail $testPassword
    Expect 'Reactivated Cashier can log in again' $reactivated.Status 200
    if ($reactivated.Status -eq 200) { Expect 'Reactivated Cashier can read medicines' (Call-Api 'GET' '/api/medicines' -Token $reactivated.Data.accessToken).Status 200 }
}
if ($pharmacist.Status -eq 200) {
    $token = $pharmacist.Data.accessToken
    Expect 'Pharmacist can read purchases' (Call-Api 'GET' '/api/purchases' -Token $token).Status 200
    Expect 'Pharmacist can read dashboard' (Call-Api 'GET' '/api/dashboard' -Token $token).Status 200
    Expect 'Pharmacist denied roles admin' (Call-Api 'GET' '/api/admin/roles' -Token $token).Status 403
}
if ($probe.Status -eq 200) {
    $token = $probe.Data.accessToken
    Expect 'Probe can read medicines before revocation' (Call-Api 'GET' '/api/medicines' -Token $token).Status 200
    Expect 'Revoke probe permission' (Call-Api 'POST' '/api/admin/roles/revoke-permission' @{role=$probeRole;permission='Medicines.Read'} -Token $adminToken).Status 204
    Expect 'Same JWT now denied (live role-claim check)' (Call-Api 'GET' '/api/medicines' -Token $token).Status 403
    Expect 'Regrant probe permission' (Call-Api 'POST' '/api/admin/roles/grant-permission' @{role=$probeRole;permission='Medicines.Read'} -Token $adminToken).Status 204
    Expect 'Same JWT now allowed' (Call-Api 'GET' '/api/medicines' -Token $token).Status 200
    Expect 'Remove probe role' (Call-Api 'POST' '/api/admin/users/remove-role' @{email=$probeEmail;role=$probeRole} -Token $adminToken).Status 204
    Expect 'Old JWT invalid after user role removal' (Call-Api 'GET' '/api/medicines' -Token $token).Status 401
    $fresh = Login $probeEmail $testPassword
    Expect 'Probe can still log in without roles' $fresh.Status 200
    if ($fresh.Status -eq 200) { Expect 'Roleless probe denied medicines' (Call-Api 'GET' '/api/medicines' -Token $fresh.Data.accessToken).Status 403 }
}

$from = [DateTime]::UtcNow.Date.AddDays(-30).ToString('yyyy-MM-ddTHH:mm:ssZ')
$to = [DateTime]::UtcNow.Date.AddDays(1).ToString('yyyy-MM-ddTHH:mm:ssZ')
$query = "?fromUtc=$from&toUtc=$to"
$report = Call-Api 'GET' "/api/reports$query" -Token $adminToken
$details = Call-Api 'GET' "/api/reports/sales-details$query" -Token $adminToken
$dashboard = Call-Api 'GET' '/api/dashboard' -Token $adminToken
$lowStock = Call-Api 'GET' '/api/stock/low-stock' -Token $adminToken
Expect 'Summary report endpoint' $report.Status 200
Expect 'Sales details endpoint' $details.Status 200
Expect 'Dashboard endpoint' $dashboard.Status 200
Expect 'Low-stock endpoint' $lowStock.Status 200
if ($report.Status -eq 200) {
    $r = $report.Data
    Check 'Sales subtotal - discount = net sales' ([decimal]$r.salesSubtotal - [decimal]$r.salesDiscount -eq [decimal]$r.salesTotal)
    Check 'Purchases subtotal - discount = net purchases' ([decimal]$r.purchasesSubtotal - [decimal]$r.purchasesDiscount -eq [decimal]$r.purchasesTotal)
    if ($details.Status -eq 200) {
        $sales = @($details.Data | Where-Object { $null -ne $_ })
        Check 'Sale count matches line-detail invoice count' ($sales.Count -eq [int]$r.saleCount)
        $total = [decimal]0
        foreach ($sale in $sales) {
            $total += [decimal]$sale.grandTotal
            $lineSum = [decimal]0
            foreach ($line in @($sale.items)) {
                Check "Line total = quantity x historical unit price (invoice $($sale.invoiceNumber))" ([decimal]$line.lineTotal -eq [decimal]$line.quantity * [decimal]$line.unitPrice)
                $lineSum += [decimal]$line.lineTotal
            }
            Check "Invoice subtotal - discount = grand total ($($sale.invoiceNumber))" ($lineSum - [decimal]$sale.discount -eq [decimal]$sale.grandTotal)
        }
        Check 'Summed detail invoices match summary net sales' ($total -eq [decimal]$r.salesTotal)
    }
}
if ($dashboard.Status -eq 200 -and $lowStock.Status -eq 200) {
    $alerts = @($lowStock.Data | Where-Object { $null -ne $_ })
    $lowCount = @($alerts | Where-Object { $_.status -eq 'LowStock' }).Count
    $outCount = @($alerts | Where-Object { $_.status -eq 'OutOfStock' }).Count
    Check 'Low-stock detail count matches Dashboard' ($lowCount -eq [int]$dashboard.Data.lowStockMedicines)
    Check 'Out-of-stock detail count matches Dashboard' ($outCount -eq [int]$dashboard.Data.outOfStockMedicines)
}

Write-Host "`nResults: $script:Passed passed; $script:Failed failed." -ForegroundColor Cyan
Write-Host "Dev-only test accounts created: $cashierEmail, $pharmacistEmail, $probeEmail" -ForegroundColor Yellow
Write-Host "Dev-only test role created: $probeRole (left in DB for manual cleanup)." -ForegroundColor Yellow
if ($script:Failed -gt 0) { exit 1 }
