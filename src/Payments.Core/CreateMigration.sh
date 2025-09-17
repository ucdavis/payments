[ "$#" -eq 1 ] || { echo "1 argument required, $# provided. Useage: sh CreateMigration <MigrationName>"; exit 1; }

dotnet ef migrations add $1 --context ApplicationDbContext --output-dir Migrations --startup-project ../Payments.Mvc --project ../Payments.Core
# usage from PM console in the Payments.Core directory: ./CreateMigration.sh <MigrationName>

echo 'All done';