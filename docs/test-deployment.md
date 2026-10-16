# GitHub Actions deployment

The `CI/CD` workflow follows the [web-app-template workflow pattern](https://github.com/ucdavis/web-app-template/blob/main/.github/workflows/ci-cd.yml)
and calls the shared `Deploy` workflow.

| Trigger                                            | Result                                                        |
| -------------------------------------------------- | ------------------------------------------------------------- |
| Pull request to `master`                           | Build and test, without deployment                            |
| Push to `master`, including a merge                | Build, test, and deploy to `test`                             |
| Manual `CI/CD` run on `master`, environment `test` | Build, test, and deploy to test                               |
| Manual `CI/CD` run on `master`, environment `prod` | Wait for approval, then build, test, and deploy to production |

Production is requested separately; a successful test deployment does not start
a production deployment. Both environments allow only the `master` branch.
Deployments are serialized per environment, without cancelling an active run.

## Build and publish

Pull request validation and deployment both build the frontend and .NET projects
and run their tests using .NET 6 and Node 22. On pushes and manual requests, the
deployment job performs these checks before publishing directly to `publish/web`.
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

## GitHub environments

Each GitHub environment supplies its own values for these variables:

| Variable                     | Purpose                                       |
| ---------------------------- | --------------------------------------------- |
| `AZURE_CLIENT_ID`            | Deployment managed identity client ID         |
| `AZURE_TENANT_ID`            | Azure tenant ID                               |
| `AZURE_SUBSCRIPTION_ID`      | Azure subscription ID                         |
| `RESOURCE_GROUP`             | Existing App Service resource group           |
| `WEB_APP_NAME`               | Existing App Service name                     |
| `APP_HEALTH_TIMEOUT_SECONDS` | Optional health-check timeout, defaults to 60 |

| Environment | App Service and resource group | Deployment identity       | Subscription               |
| ----------- | ------------------------------ | ------------------------- | -------------------------- |
| `test`      | `payments-test`                | `id-payments-test-deploy` | Existing test subscription |
| `prod`      | `payments-production`          | `id-payments-prod-deploy` | UC Davis CAES Production   |

Each identity has Website Contributor scoped only to its App Service. The
federated subjects are `repo:ucdavis/payments:environment:test` and
`repo:ucdavis/payments:environment:prod`; production does not reuse test access.

The `prod` environment requires approval from `srkirkland`. Self-approval is
allowed, but administrator bypass is disabled. The shared deployment job binds
to this environment, so it cannot start or obtain its Azure OIDC token before
approval. A manual workflow request alone does not approve deployment.

Application secrets and connection strings remain in Azure. Preserve Windows
hosting, 32-bit workers, Always On, schedules, domains, certificates, and
`ASPNETCORE_ENVIRONMENT=Production`. Do not change database migration behavior or
enable run-from-package during this migration.

The earlier draft's `TEST_DEPLOYMENT_READY` variable is unused and does not block
deployment. Azure classic release definition 1 keeps both Test and Production
manual. Leave Azure Test automatic deployment disabled to avoid competing
deployers. The existing Azure build and production release remain available for
recovery; GitHub's approval gate does not govern that separate Azure release path.

## Test deployment

Merging to `master` starts test deployment automatically. Review the resulting
commit and workflow run, confirm deployment and health, and inspect both
WebJobs' deployed entrypoints and schedules. After enabling this workflow, the
next test deployment includes merged application changes such as PR #564.

For a retry, run `CI/CD` from `master` and choose `test`. Check for active Azure
releases before any manual retry or recovery. Prefer a merge or manual run
between hourly job runs and outside the daily AutoApprove window.

## Production release

1. Recheck the live production revision and retain its Azure build artifact and
   deployment metadata for rollback. On October 16, 2026, production was
   Release-217 / build 14794 / commit
   `13cc9bb29a14551299022b58655b04cc909ce53e`. Build 15073 is a test rollback
   artifact, not the production rollback artifact.
2. Review the full delta from production to the intended `master` commit,
   including PR #556's API/auth changes and PR #564's receipt/AutoApprove changes.
   Check configuration and migrations; application startup runs database
   migrations. Verify test evidence for the intended revision and agree a release
   window outside scheduled job activity.
3. Request a manual `CI/CD` run from `master` with environment `prod`. Confirm
   the run's commit is the intended release and that it is waiting for approval.
   The reviewer must approve that deployment explicitly. If the wrong revision
   was selected, cancel the request and select the intended release again.
4. After approval, watch deployment and health checks to completion. Inspect
   deployed files and WebJob contracts, then observe natural scheduled runs.
   ZIP deployment differs from the old MSDeploy task's extra-file removal and
   locked-file handling, so check for obsolete files or locking errors without
   deleting runtime data.

## Read-only acceptance

Inspect login, existing invoice pages, frontend assets, and
safe authenticated reads where access is available. Observe the next scheduled
MoneyMovement and AutoApprove runs and inspect application errors and job results.
Do not manually run jobs or initiate payments, recharge, email, or webhooks.

A green build or deployment does not prove authenticated workflows or scheduled
job success. Record any checks that still need credentials or the next scheduled
run. The AutoApprove executable does not check `Finance:RechargeAutoApprove`, so
that setting is not a job-disable switch.

Workflow success and health checks do not establish the exact live revision.
Record the workflow commit separately from deployment, health, scheduled-job,
and authenticated functional evidence.

## Recovery

For test recovery, disable the `CI/CD` workflow to prevent new automatic runs,
cancel queued deployments, and let any active deployment finish before using
Azure. Redeploy the retained known-good artifact through the existing Azure
release's Test stage. Keep GitHub deployment disabled until recovery is verified
and the deployment owner is agreed.

For production recovery, obtain explicit release approval, cancel pending GitHub
production requests, and let any active deployment finish. Use the retained
current production artifact through the existing Azure Production stage.

Check login, deployed files, and both jobs. Older artifacts may not have `/health`,
so use their existing application routes for verification. Restore Azure Test's
automatic trigger only if handing deployment ownership back to Azure and after
confirming GitHub cannot deploy concurrently. Package rollback does not undo
database changes or job side effects.
