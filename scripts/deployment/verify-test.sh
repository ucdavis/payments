#!/usr/bin/env bash
set -euo pipefail
revision="${1:?Expected commit is required}"
base=https://payments-test.azurewebsites.net
ready=0
for attempt in {1..36}; do
  echo "Readiness attempt $attempt"
  health="$(curl --silent --show-error --fail --max-time 10 "$base/health" || true)"
  deployed="$(curl --silent --show-error --fail --max-time 10 -H 'Cache-Control: no-cache' "$base/release.json" | jq -r .commit || true)"
  if [[ "$health" == Healthy && "$deployed" == "$revision" ]]; then
    ready=$((ready + 1))
    if ((ready >= 2)); then
      echo "Test is healthy and serves commit $revision."
      exit 0
    fi
  else
    ready=0
  fi
  sleep 5
done
echo "Test did not become healthy at the expected revision." >&2
exit 1
