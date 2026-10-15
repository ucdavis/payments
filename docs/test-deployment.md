# GitHub Actions test deployment pilot

GitHub Actions validates and packages Payments. `Deploy test` is manually dispatched
from `master` and targets only the existing Windows `payments-test` App Service.
Production remains in Azure Pipelines, including its build artifacts and manual release.

## Package and validation

`CI` runs .NET 6 / Node 22, restores the frontend from its lockfile, runs frontend
and backend tests, and publishes one ZIP with the website and both triggered WebJobs:

- `app_data/jobs/triggered/Payments.Jobs.MoneyMovement`, hourly.
- `app_data/jobs/triggered/Payments.Jobs.AutoApprove`, daily at 06:00 UTC.

The package includes each job's `run.cmd`, `settings.job`, dependencies and settings.
`wwwroot/release.json` identifies the source commit. The retained artifact includes
its SHA-256 checksum. The deploy workflow builds and tests its exact revision through
the same reusable CI workflow, then deploys that artifact without rebuilding it.

`/health` is anonymous, uncached, and returns only `Healthy` / HTTP 200 or
`Unhealthy` / HTTP 503 after a read-only database connection check with a five-second
cancellation deadline. It does not inspect or mutate invoices. A successful deployment
requires two consecutive checks that each report healthy and match the expected commit.

For local validation, install .NET 6 and Node 22, run `npm ci --legacy-peer-deps`
in `src/Payments.Mvc/ClientApp`, run `npm run test:run` there, then run
`python3 -m unittest discover -s tests/deployment -v`,
`dotnet test Payments.sln --configuration Release` and
`bash scripts/deployment/package.sh` from the repository root. Packaging requires
`zip`, `shasum`, a fresh `artifacts/payments.zip` path, and restored frontend dependencies.
The deployment readiness tests require Python 3, Bash and `jq`.

## Test access

The test identity and GitHub environment already exist. Keep cloud configuration
unchanged and `TEST_DEPLOYMENT_READY=false` through code validation and PR review.
Recheck the existing configuration below during the post-merge handover.

Use the user-assigned managed identity `id-payments-test-deploy` in resource group
`payments-test`, subscription `105dede4-4731-492e-8c28-5121226319b0`. Its OIDC trust
must use issuer `https://token.actions.githubusercontent.com`, audience
`api://AzureADTokenExchange`, and subject `repo:ucdavis/payments:environment:test`.
The repository currently uses that default, non-immutable OIDC subject format;
recheck it at handover. Keep Website Contributor scoped only to this App Service.
Do not grant production, SQL, shared-plan or subscription-wide access.

Verify that the existing GitHub environment named `test` is restricted to the
`master` branch, with:

| Variable                | Value                                     |
| ----------------------- | ----------------------------------------- |
| `AZURE_CLIENT_ID`       | Client ID of the test deployment identity |
| `AZURE_TENANT_ID`       | `a8046f64-66c0-4f00-9046-c8daf92ff62b`    |
| `AZURE_SUBSCRIPTION_ID` | `105dede4-4731-492e-8c28-5121226319b0`    |
| `RESOURCE_GROUP`        | `payments-test`                           |
| `WEB_APP_NAME`          | `payments-test`                           |
| `TEST_DEPLOYMENT_READY` | `false`; enable only at handover step 5   |

Application secrets and connection strings stay in Azure. Preserve Windows hosting,
32-bit worker configuration, Always On, schedules, domains, certificates and
`ASPNETCORE_ENVIRONMENT=Production`. The GitHub environment name does not replace
the ASP.NET environment setting. Do not enable run-from-package or change database
migration behavior during this pilot.

## Handover and first deployment

1. Merge the reviewed PR after CI succeeds. Coordinate other application merges so
   the first pilot does not also introduce financial logic or schema changes.
2. Recheck the live baseline. At planning time test used Release-218 / build 15073 /
   `e09c70b5d33f15f1cc3840757b3c263c2cb5ba6f`; production used Release-217.
   Retain/download the last successful test artifact and capture the release definition,
   deployed file inventory, settings metadata and both job histories. Protect any
   configuration backups containing secrets; never commit them or upload them to CI.
3. Compare the Actions package with that baseline, allowing the health endpoint and
   revision file. Investigate dependency, frontend and package layout differences.
   Check both job directories and the generated Windows `web.config`.
4. In classic release definition 1, change only Test's trigger from automatic to manual.
   Preserve the build pipeline, release artifact trigger and Production environment.
   Confirm no test release is running or queued. Existing releases can retain older
   trigger settings, so check them too.
5. Check neither scheduled job is running. Deploy between hourly MoneyMovement runs,
   outside AutoApprove's daily window. Set `TEST_DEPLOYMENT_READY=true` only after
   this handover. Dispatch `Deploy test` on `master`.
6. Verify Kudu deployment completion, application readiness and revision. Compare
   deployed files with the artifact. The old MSDeploy task removed extra files and
   renamed locked files; ZIP deployment's first run must be checked for obsolete
   files and locking differences. Do not blindly erase `wwwroot` or runtime data.

## Functional acceptance, step 5

Record evidence for each item; a green workflow alone does not satisfy this list.

- CAS sign-in, existing invoice display, frontend assets and PDF generation.
- Relevant authenticated/API-key API calls, preserving authorization boundaries.
- Controlled CyberSource sandbox checkout and callback handling, with expected invoice
  state and no duplicate payment processing.
- Controlled recharge and notification/webhook behavior, using agreed test records and
  recipients. Confirm every external destination before allowing side effects.
- Both jobs present with unchanged names, schedules and entry points; observe the next
  hourly MoneyMovement run and next daily AutoApprove run. Check job records and
  expected downstream results, not just Kudu exit status. Do not manually rerun
  financial jobs to accelerate observation.
- No new application errors, connection failures, duplicate processing or failed jobs.

Live inspection found test SQL, CyberSource, SLOTH, CAS and reporting targets.
Aggie Enterprise uses a production-named gateway with `ScopeEnv=Test`; verify the
actual effective scope before financial tests. The AutoApprove executable does not
check `Finance:RechargeAutoApprove`, so that setting is not a job-disable switch.
Email and webhook delivery require approved test recipients. Never send real
notifications merely to validate deployment.

Stop after functional acceptance. Automatic GitHub deployment, branch-protection changes
and production migration are separate follow-up decisions. If sign-in, test fixtures or a scheduled
run are unavailable, report the missing evidence instead of declaring acceptance.

## Recovery

If the pilot fails, block further GitHub deploys with `TEST_DEPLOYMENT_READY=false`
and wait for any active deployment to finish. Redeploy the retained known-good test
artifact through the existing Azure release's Test stage only. Do not select Production.
Check health using the endpoint appropriate to that artifact, login behavior, files,
and both jobs. The older artifact will not contain `/health` or `release.json`.

Restore Test's previous automatic trigger only if abandoning the handover, after
confirming GitHub deployment is disabled and no deployment is queued. A package
rollback does not undo database changes, payments, messages or job side effects.
The pilot deliberately excludes schema and financial logic changes.
