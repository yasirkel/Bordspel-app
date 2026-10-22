[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $InstanceId,
    [string] $Profile = "bordspel-assessment",
    [string] $Region = "eu-north-1"
)

$ErrorActionPreference = "Stop"
aws ec2 start-instances --instance-ids $InstanceId --profile $Profile --region $Region | Out-Null
aws ec2 wait instance-running --instance-ids $InstanceId --profile $Profile --region $Region
aws ec2 describe-instances --instance-ids $InstanceId --profile $Profile --region $Region `
    --query "Reservations[0].Instances[0].{State:State.Name,PublicIp:PublicIpAddress}" --output table

Write-Host "The EC2 host is running. Docker services restart through systemd/docker restart policy."
Write-Host "If the public IP changed, run deploy/refresh-sslip-hostnames.sh on the host and use the new sslip.io URLs."
