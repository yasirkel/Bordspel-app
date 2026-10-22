param(
    [Parameter(Mandatory = $true)]
    [string]$PemPath,

    [string]$Target = "ubuntu@13.60.219.216",

    [string]$CredentialsPath = "$env:USERPROFILE\Downloads\BordspelApp-assessment-credentials.txt"
)

$ErrorActionPreference = "Stop"
$temporaryEnvPath = Join-Path $env:TEMP "bordspel-assessment-seed.env"
$characters = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789"

function New-AssessmentPassword {
    $randomPart = -join (1..18 | ForEach-Object {
        [char]$characters[[System.Security.Cryptography.RandomNumberGenerator]::GetInt32($characters.Length)]
    })

    return "$randomPart`!aA1"
}

$accounts = @(
    @{ Name = "Docent"; Email = "assessment.docent@example.com"; Variable = "ASSESSMENT_DOCENT_PASSWORD" },
    @{ Name = "Organisator"; Email = "assessment.organizer@example.com"; Variable = "ASSESSMENT_ORGANIZER_PASSWORD" },
    @{ Name = "Speler 1"; Email = "assessment.player1@example.com"; Variable = "ASSESSMENT_PLAYER1_PASSWORD" },
    @{ Name = "Speler 2"; Email = "assessment.player2@example.com"; Variable = "ASSESSMENT_PLAYER2_PASSWORD" },
    @{ Name = "Speler 3"; Email = "assessment.player3@example.com"; Variable = "ASSESSMENT_PLAYER3_PASSWORD" },
    @{ Name = "Speler 16/17 jaar"; Email = "assessment.minor@example.com"; Variable = "ASSESSMENT_MINOR_PASSWORD" }
)

$credentialLines = @(
    "BordspelApp assessment testaccounts",
    "Deze fictieve wachtwoorden staan alleen lokaal. Niet committen of delen via Git."
)
$environmentLines = @("ASSESSMENT_SEED=true")

foreach ($account in $accounts) {
    $password = New-AssessmentPassword
    $account.Password = $password
    $credentialLines += "$($account.Name): $($account.Email) / $password"
    $environmentLines += "$($account.Variable)=$password"
}

[System.IO.File]::WriteAllLines($CredentialsPath, $credentialLines)
[System.IO.File]::WriteAllLines($temporaryEnvPath, $environmentLines)
icacls $CredentialsPath /inheritance:r /grant:r "$($env:USERNAME):(R,W)" | Out-Null
icacls $temporaryEnvPath /inheritance:r /grant:r "$($env:USERNAME):(R,W)" | Out-Null

try {
    scp -q -o StrictHostKeyChecking=no -i $PemPath $temporaryEnvPath "$Target`:/tmp/bordspel-assessment-seed.env"
    $remoteCommand = 'cd /opt/bordspel-app; sudo docker compose --env-file .env run --rm --no-deps --env-from-file /tmp/bordspel-assessment-seed.env migrator; status=$?; sudo rm -f /tmp/bordspel-assessment-seed.env; exit $status'
    ssh -o StrictHostKeyChecking=no -o ConnectTimeout=15 -i $PemPath $Target $remoteCommand
    if ($LASTEXITCODE -ne 0) {
        throw "The assessment seed command failed on EC2."
    }
}
finally {
    Remove-Item -LiteralPath $temporaryEnvPath -Force -ErrorAction SilentlyContinue
}

Write-Output "Assessment seed completed. Credentials are stored locally at $CredentialsPath"
