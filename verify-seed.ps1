# verify-seed.ps1
# Verifies seeded data in Azure SQL. Exit 0 = PASS, 1 = FAIL
# Reads connection string from environment variable AZURE_SQL_CONN

param()

$ErrorActionPreference = 'Stop'

function Write-Result($ok, $message) {
    if ($ok) { Write-Host "PASS: $message" -ForegroundColor Green }
    else { Write-Host "FAIL: $message" -ForegroundColor Red }
}

# Get connection string
$conn = $env:AZURE_SQL_CONN
if ([string]::IsNullOrWhiteSpace($conn)) {
    Write-Host "Environment variable AZURE_SQL_CONN is not set." -ForegroundColor Red
    exit 1
}

# Parse connection string using .NET builder
$builder = New-Object System.Data.Common.DbConnectionStringBuilder
try { $builder.ConnectionString = $conn } catch { Write-Host "Invalid connection string format." -ForegroundColor Red; exit 1 }

# Helper to get key ignoring case
function Get-Key($name) {
    foreach ($k in $builder.Keys) {
        if ($k.ToString().ToLower() -eq $name.ToLower()) { return $builder[$k] }
    }
    return $null
}

$server = Get-Key 'Server' ; if (-not $server) { $server = Get-Key 'Data Source' }
$db = Get-Key 'Initial Catalog' ; if (-not $db) { $db = Get-Key 'Database' }
$uid = Get-Key 'User ID' ; $pwd = Get-Key 'Password' ; $integrated = Get-Key 'Integrated Security'

# Normalize server for sqlcmd (remove leading tcp:)
if ($server -is [string] -and $server.StartsWith('tcp:', [System.StringComparison]::InvariantCultureIgnoreCase)) {
    $serverForSqlcmd = $server.Substring(4)
} else { $serverForSqlcmd = $server }

# If serverForSqlcmd is empty, try to extract from raw connection string
if ([string]::IsNullOrWhiteSpace($serverForSqlcmd)) {
    if ($conn -match '(?:Server|Data Source)\s*=\s*(?:tcp:)?([^;,]+)') {
        $serverForSqlcmd = $matches[1]
    }
}

# Determine execution mode
$hasSqlCmd = (Get-Command sqlcmd -ErrorAction SilentlyContinue) -ne $null
if ($hasSqlCmd) { Write-Host "Using sqlcmd for queries." }
else { Write-Host "sqlcmd not found; falling back to .NET SqlClient." }

function Exec-ScalarSqlcmd($sql) {
    # Use direct invocation of sqlcmd which returns stdout lines; avoid Start-Process redirection issues
    $args = @()
    $args += '-S'; $args += "$serverForSqlcmd"
    if ($db) { $args += '-d'; $args += "$db" }
    if ($uid -and $pwd) { $args += '-U'; $args += $uid; $args += '-P'; $args += $pwd }
    else { $args += '-E' }
    $args += '-Q'; $args += $sql; $args += '-h'; $args += '-1'; $args += '-W'

    try {
        $outLines = & sqlcmd @args 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "sqlcmd failed with exit code ${LASTEXITCODE}: $($outLines -join ' | ')"
        }
        if (-not $outLines) { return $null }
        $first = ($outLines -join "`n").Split("`n")[0].Trim()
        return $first
    }
    catch {
        throw $_
    }
}

function Exec-ScalarDotNet($sql) {
    Add-Type -AssemblyName System.Data
    $cn = New-Object System.Data.SqlClient.SqlConnection
    $cn.ConnectionString = $conn
    try {
        $cn.Open()
        $cmd = $cn.CreateCommand()
        $cmd.CommandText = $sql
        $res = $cmd.ExecuteScalar()
        return $res
    }
    finally { $cn.Close() }
}

function Exec-Scalar($sql) {
    if ($hasSqlCmd) {
        try { return Exec-ScalarSqlcmd $sql } catch { Write-Host "sqlcmd failed: $_" -ForegroundColor Yellow; return Exec-ScalarDotNet $sql }
    } else {
        return Exec-ScalarDotNet $sql
    }
}

# Run checks
$failed = $false

Write-Host "Running COUNT checks..."
$persons = [int](Exec-Scalar "SET NOCOUNT ON; SELECT COUNT(*) FROM Persons")
Write-Result ($persons -ge 10) "Persons >= 10 ($persons)"; if ($persons -lt 10) { $failed = $true }

$aspnet = [int](Exec-Scalar "SET NOCOUNT ON; SELECT COUNT(*) FROM AspNetUsers")
Write-Result ($aspnet -ge 10) "AspNetUsers >= 10 ($aspnet)"; if ($aspnet -lt 10) { $failed = $true }

$bg = [int](Exec-Scalar "SET NOCOUNT ON; SELECT COUNT(*) FROM BoardGames")
Write-Result ($bg -ge 6) "BoardGames >= 6 ($bg)"; if ($bg -lt 6) { $failed = $true }

$gn = [int](Exec-Scalar "SET NOCOUNT ON; SELECT COUNT(*) FROM GameNights")
Write-Result ($gn -ge 6) "GameNights >= 6 ($gn)"; if ($gn -lt 6) { $failed = $true }

$gnp = [int](Exec-Scalar "SET NOCOUNT ON; SELECT COUNT(*) FROM GameNightPlayers")
Write-Result ($gnp -ge 18) "GameNightPlayers >= 18 ($gnp)"; if ($gnp -lt 18) { $failed = $true }

$gnbg = [int](Exec-Scalar "SET NOCOUNT ON; SELECT COUNT(*) FROM GameNightBoardGames")
Write-Result ($gnbg -ge 12) "GameNightBoardGames >= 12 ($gnbg)"; if ($gnbg -lt 12) { $failed = $true }

# Identity <-> Person
Write-Host "\nRunning Identity <-> Person checks..."
$nullPerson = [int](Exec-Scalar "SET NOCOUNT ON; SELECT COUNT(*) FROM AspNetUsers WHERE PersonId IS NULL")
Write-Result ($nullPerson -eq 0) "AspNetUsers with PersonId IS NULL = $nullPerson"; if ($nullPerson -ne 0) { $failed = $true }

$orphanPerson = [int](Exec-Scalar "SET NOCOUNT ON; SELECT COUNT(*) FROM AspNetUsers a WHERE a.PersonId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Persons p WHERE p.Id = a.PersonId)")
Write-Result ($orphanPerson -eq 0) "AspNetUsers with PersonId not matching Persons = $orphanPerson"; if ($orphanPerson -ne 0) { $failed = $true }

$dupEmails = [int](Exec-Scalar "SET NOCOUNT ON; SELECT COUNT(*) FROM (SELECT Email FROM Persons GROUP BY Email HAVING COUNT(*) > 1) AS dup")
Write-Result ($dupEmails -eq 0) "Duplicate emails in Persons = $dupEmails"; if ($dupEmails -ne 0) { $failed = $true }

# Organizers
Write-Host "\nRunning Organizers checks..."
$badOrganizers = [int](Exec-Scalar "SET NOCOUNT ON; SELECT COUNT(*) FROM GameNights gn WHERE NOT EXISTS (SELECT 1 FROM Persons p WHERE p.Id = gn.OrganizerId)")
Write-Result ($badOrganizers -eq 0) "GameNights with OrganizerId not matching Persons = $badOrganizers"; if ($badOrganizers -ne 0) { $failed = $true }

# Per GameNight relations and summary (print 6 rows)
Write-Host "\nPer-GameNight summary (Title, PlayerCount, BoardGameCount):"
$rowsSql = @"
SELECT TOP(6) gn.Id, gn.Title,
 (SELECT COUNT(*) FROM GameNightPlayers gnp WHERE gnp.GameNightId = gn.Id) AS PlayerCount,
 (SELECT COUNT(*) FROM GameNightBoardGames gnb WHERE gnb.GameNightId = gn.Id) AS BoardGameCount
FROM GameNights gn
ORDER BY gn.DateTime ASC
"@

# Execute query and format output
function Exec-QueryRows($sql) {
    Add-Type -AssemblyName System.Data
    $list = @()
    $cn = New-Object System.Data.SqlClient.SqlConnection
    $cn.ConnectionString = $conn
    try {
        $cn.Open()
        $cmd = $cn.CreateCommand()
        $cmd.CommandText = $sql
        $r = $cmd.ExecuteReader()
        while ($r.Read()) {
            $list += [pscustomobject]@{
                Id = $r.GetInt32(0)
                Title = $r.GetString(1)
                PlayerCount = $r.GetInt32(2)
                BoardGameCount = $r.GetInt32(3)
            }
        }
        $r.Close()
    }
    finally { $cn.Close() }
    return $list
}

$rows = Exec-QueryRows $rowsSql
foreach ($row in $rows) {
    $ok = ($row.PlayerCount -ge 3) -and ($row.BoardGameCount -ge 2)
    $status = if ($ok) { "OK" } else { "BAD" }
    Write-Host "- $($row.Title) | Players: $($row.PlayerCount) | BoardGames: $($row.BoardGameCount) => $status"
    if (-not $ok) { $failed = $true }
}

# Check any GameNight failing the rule
$badGNCount = [int](Exec-Scalar "SET NOCOUNT ON; SELECT COUNT(*) FROM (SELECT gn.Id, (SELECT COUNT(*) FROM GameNightPlayers gnp WHERE gnp.GameNightId = gn.Id) AS PlayerCount, (SELECT COUNT(*) FROM GameNightBoardGames gnb WHERE gnb.GameNightId = gn.Id) AS BoardGameCount FROM GameNights gn) t WHERE PlayerCount < 3 OR BoardGameCount < 2")
Write-Result ($badGNCount -eq 0) "GameNights with PlayerCount <3 or BoardGameCount <2 = $badGNCount"; if ($badGNCount -ne 0) { $failed = $true }

# Final result
if ($failed) {
    Write-Host "\nVERIFY-SEED: FAILED" -ForegroundColor Red
    exit 1
} else {
    Write-Host "\nVERIFY-SEED: PASS" -ForegroundColor Green
    exit 0
}
