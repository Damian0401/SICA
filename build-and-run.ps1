param (
    [Alias("f")]
    [string]$File = "./compose.yaml",

    [Alias("e")]
    [string]$EnvFile = "./.env.sample",

    [Alias("s")]
    [string]$Service
)

if (-not $Service) {
    docker compose --env-file .env.sample up -d --build
    Write-Host "All services have been rebuilt and started successfully."
    exit $LASTEXITCODE
}

docker compose --env-file $EnvFile -f $File build --no-cache $Service

if ($?) {
    docker compose --env-file $EnvFile -f $File up -d $Service
}

if ($?) {
    Write-Host "Service $Service has been rebuilt and started successfully."
} else {
    Write-Host "Failed to rebuild or start the service $Service." -ForegroundColor Red
}