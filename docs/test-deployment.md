# GitHub Actions deployment

The manual `Deploy` workflow follows the [web-app-template deployment workflow](https://github.com/ucdavis/web-app-template/blob/main/.github/workflows/deploy-azure-appservice.yml).
Choose a GitHub environment when running it from `master`. Only `test` is enabled
for this migration. Production continues through the existing Azure pipeline.

## Build and publish

CI builds the frontend and .NET projects and runs their tests using .NET 6 and
Node 22. Deployment repeats those steps, then publishes directly to `publish/web`.
Payments' existing publish target includes the frontend in the web output.

The deploy workflow also publishes both WebJobs into the web directory before
zipping it to `publish/webapp.zip`:

- `app_data/jobs/triggered/Payments.Jobs.MoneyMovement`, hourly.
- `app_data/jobs/triggered/Payments.Jobs.AutoApprove`, daily at 06:00 UTC.

Their existing `run.cmd`, `settings.job`, dependencies and settings are preserved.
The same job logs into Azure with OIDC, checks SCM readiness, deploys the ZIP using
`azure/webapps-deploy`, and checks `/health` for two consecutive HTTP 200 responses.
The health endpoint performs a read-only database connection check and returns
`Healthy` or `Unhealthy`, without connection details.

## GitHub environment

The `test` environment and deployment identity already exist. The environment
restricts deployment to `master` and supplies these variables:

| Variable                     | Purpose                                       |
| ---------------------------- | --------------------------------------------- |
| `AZURE_CLIENT_ID`            | Deployment managed identity client ID         |
| `AZURE_TENANT_ID`            | Azure tenant ID                               |
| `AZURE_SUBSCRIPTION_ID`      | Azure subscription ID                         |
| `RESOURCE_GROUP`             | Existing App Service resource group           |
| `WEB_APP_NAME`               | Existing App Service name                     |
| `APP_HEALTH_TIMEOUT_SECONDS` | Optional health-check timeout, defaults to 60 |

The test identity is `id-payments-test-deploy`, with Website Contributor scoped to
`payments-test` only. Its federated subject is
`repo:ucdavis/payments:environment:test`. No production identity or permissions are
part of this change.

Application secrets and connection strings remain in Azure. Preserve Windows
hosting, 32-bit workers, Always On, schedules, domains, certificates, and
`ASPNETCORE_ENVIRONMENT=Production`. Do not change database migration behavior or
enable run-from-package during this migration.

The earlier draft's `TEST_DEPLOYMENT_READY` variable is no longer used. Deployment
requires an explicit manual run; do not dispatch it until the handover below.

## Test handover

1. Review and merge the PR after CI passes. Coordinate other application changes so
   the first deployment does not also introduce schema or financial logic changes.
2. Recheck the live test release and retain its artifact for rollback. The initial
   baseline was Release-218 / build 15073 / commit
   `e09c70b5d33f15f1cc3840757b3c263c2cb5ba6f`. Inspect both WebJobs and their histories.
3. In Azure classic release definition 1, change only Test's automatic trigger to
   manual. Preserve the build pipeline, release artifact trigger, and Production
   stage. Confirm no test release is running or queued, including older releases.
4. Check neither scheduled job is running. Between hourly runs and outside the
   daily AutoApprove window, run `Deploy` on `master` with environment `test`.
5. Confirm deployment and health checks pass. Inspect deployed files and both
   WebJobs, especially their schedules and Windows entry points. The old MSDeploy
   task removed extra files and renamed locked files; inspect the first ZIP deploy
   for obsolete files or locking issues without deleting runtime data.

## Read-only acceptance

For this first pass, inspect login, existing invoice pages, frontend assets, and
safe authenticated reads where access is available. Observe the next scheduled
MoneyMovement and AutoApprove runs and inspect application errors and job results.
Do not manually run jobs or initiate payments, recharge, email, or webhooks.

A green build or deployment does not prove authenticated workflows or scheduled
job success. Record any checks that still need credentials or the next scheduled
run. The AutoApprove executable does not check `Finance:RechargeAutoApprove`, so
that setting is not a job-disable switch.

Automatic GitHub deployment and production migration need a separate decision.

## Recovery

Stop further manual GitHub runs and let any active deployment finish. Redeploy
the retained known-good artifact through the existing Azure release's Test stage
only. Check login, deployed files, and both jobs. The older artifact does not have
`/health`, so use its existing application routes for verification.

If abandoning the handover, restore Test's previous automatic trigger after
confirming no GitHub deployment is running or queued. Package rollback does not
undo database changes or job side effects.
