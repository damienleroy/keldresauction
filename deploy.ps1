$ErrorActionPreference = 'Stop'

function Get-EnvValue {
	param(
		[Parameter(Mandatory = $true)]
		[string]$Name,
		[switch]$Required
	)

	$value = [System.Environment]::GetEnvironmentVariable($Name)
	if ($Required -and [string]::IsNullOrWhiteSpace($value)) {
		throw "Environment variable '$Name' is required."
	}

	return $value
}

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptRoot

$serverHost = Get-EnvValue -Name 'SERVER_HOST' -Required
$serverUser = Get-EnvValue -Name 'SERVER_USER' -Required
$serverPort = Get-EnvValue -Name 'SERVER_PORT'
if ([string]::IsNullOrWhiteSpace($serverPort)) { $serverPort = '22' }

$sshKeyPath = Get-EnvValue -Name 'SERVER_SSH_KEY_PATH'
if ([string]::IsNullOrWhiteSpace($sshKeyPath)) {
	$sshKeyPath = Join-Path $env:USERPROFILE '.ssh\truenas_ssh'
}
if (-not (Test-Path -Path $sshKeyPath -PathType Leaf)) {
	throw "SSH key file not found: '$sshKeyPath'."
}

$network = Get-EnvValue -Name 'DOCKER_NETWORK'
if ([string]::IsNullOrWhiteSpace($network)) { $network = 'simpleapi_network' }

$service = 'keldresauction'
$hostPort = '9093'
$containerPort = '8080'
$imageName = "keldresauction/$service"
$envFile = Join-Path $env:TEMP "keldresauction_$service.env"
$tarFile = Join-Path $env:TEMP "keldresauction_$service.tar"

$envLines = @(
	'ASPNETCORE_ENVIRONMENT=Integration'
	'DOTNET_ENVIRONMENT=Integration'
)
$envLines += "ConnectionStrings__keldresauction=$(Get-EnvValue -Name 'CONNECTIONSTRING_KELDRESAUCTION' -Required)"
$otlpEndpoint = Get-EnvValue -Name 'OTEL_EXPORTER_OTLP_ENDPOINT'
if (-not [string]::IsNullOrWhiteSpace($otlpEndpoint)) {
	$envLines += "OTEL_EXPORTER_OTLP_ENDPOINT=$otlpEndpoint"
}

Set-Content -Path $envFile -Value $envLines -Encoding UTF8

$gitSha = (git rev-parse --short HEAD 2>$null)
if ([string]::IsNullOrWhiteSpace($gitSha)) {
	$gitSha = 'local'
}

try {
	docker build `
		-t "${imageName}:latest" `
		-t "${imageName}:$gitSha" `
		-f Dockerfile `
		.
	if ($LASTEXITCODE -ne 0) { throw "docker build failed." }

	docker save -o $tarFile "${imageName}:latest"
	if ($LASTEXITCODE -ne 0) { throw "docker save failed." }

	scp -P $serverPort -i $sshKeyPath $tarFile "${serverUser}@${serverHost}:/tmp/"
	if ($LASTEXITCODE -ne 0) { throw "scp failed while uploading image archive." }

	ssh -p $serverPort -i $sshKeyPath "${serverUser}@${serverHost}" "docker load -i /tmp/$(Split-Path -Leaf $tarFile) && rm /tmp/$(Split-Path -Leaf $tarFile)"
	if ($LASTEXITCODE -ne 0) { throw "ssh failed while loading docker image on remote host." }

	scp -P $serverPort -i $sshKeyPath $envFile "${serverUser}@${serverHost}:/tmp/keldresauction_$service.env"
	if ($LASTEXITCODE -ne 0) { throw "scp failed while uploading env file." }

	$remoteRun = @"
set -e

docker stop "$service" || true
docker rm "$service" || true

docker run -d --name "$service" --network "$network" --restart unless-stopped -p ${hostPort}:${containerPort} --env-file "/tmp/keldresauction_$service.env" "${imageName}:latest"

rm "/tmp/keldresauction_$service.env"
"@

	$remoteRun | ssh -p $serverPort -i $sshKeyPath "${serverUser}@${serverHost}" bash -s
	if ($LASTEXITCODE -ne 0) { throw "ssh failed while running remote deployment script." }
}
finally {
	Remove-Item -Path $envFile -ErrorAction SilentlyContinue
	Remove-Item -Path $tarFile -ErrorAction SilentlyContinue
}
