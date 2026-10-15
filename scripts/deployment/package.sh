#!/usr/bin/env bash
set -euo pipefail
cd "$(git rev-parse --show-toplevel)"
# Use a fresh directory so old publish output cannot enter a new package.
mkdir -p artifacts
archive="$PWD/artifacts/payments.zip"
test ! -e "$archive" || { echo "Package already exists; use a fresh checkout." >&2; exit 1; }
output="$(mktemp -d "$PWD/artifacts/package.XXXXXX")"
web="$output/web"
dotnet publish src/Payments.Mvc/Payments.Mvc.csproj --configuration Release \
  -p:FrontendDependenciesRestored=true --output "$web"
for job in Payments.Jobs.MoneyMovement Payments.Jobs.AutoApprove; do
  dotnet publish "src/$job/$job.csproj" --configuration Release \
    --output "$web/app_data/jobs/triggered/$job"
done
for file in web.config Payments.Mvc.dll ClientApp/build/index.html; do
  test -s "$web/$file" || { echo "Missing package file: $file" >&2; exit 1; }
done
shopt -s nullglob
frontend_assets=("$web"/ClientApp/build/assets/*.js)
((${#frontend_assets[@]} > 0)) || { echo "Missing built frontend JavaScript." >&2; exit 1; }
for job in Payments.Jobs.MoneyMovement Payments.Jobs.AutoApprove; do
  for file in "$job.dll" "$job.runtimeconfig.json" run.cmd settings.job appsettings.json; do
    test -s "$web/app_data/jobs/triggered/$job/$file" || { echo "Missing $job/$file" >&2; exit 1; }
  done
done
revision="$(git rev-parse HEAD)"
printf '{"commit":"%s"}\n' "$revision" > "$web/wwwroot/release.json"
(cd "$web" && zip -qr "$archive" .)
(cd artifacts && shasum -a 256 payments.zip > payments.zip.sha256)
