[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter(Mandatory = $true)]
    [string] $InstanceId,
    [string] $Profile = "bordspel-assessment",
    [string] $Region = "eu-north-1"
)

$ErrorActionPreference = "Stop"
# Stop de bestaande EC2-host alleen na bevestiging via ShouldProcess.
if ($PSCmdlet.ShouldProcess($InstanceId, "Stop EC2 instance")) {
    aws ec2 stop-instances --instance-ids $InstanceId --profile $Profile --region $Region
}
