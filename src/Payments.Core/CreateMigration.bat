@echo off
REM Check for exactly one argument
REM from package manager console .\CreateMigration.bat MigrationName
IF "%~1"=="" (
    echo 1 argument required, 0 provided. Usage: CreateMigration.bat MigrationName
    exit /b 1
)

dotnet ef migrations add %1 --context ApplicationDbContext --output-dir Migrations --startup-project ../Payments.Mvc --project ../Payments.Core

echo All done
