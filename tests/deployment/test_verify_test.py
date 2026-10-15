"""Exercise the deployment readiness command against simulated HTTP responses."""
import os
from pathlib import Path
import subprocess
import tempfile
import unittest

SCRIPT = Path(__file__).resolve().parents[2] / 'scripts/deployment/verify-test.sh'


class ReadinessTests(unittest.TestCase):
    def run_check(self, health='Healthy', revision='expected', http_failure=False):
        with tempfile.TemporaryDirectory() as directory:
            curl = Path(directory) / 'curl'
            curl.write_text('''#!/usr/bin/env python3
import json, os, sys
if os.environ['HTTP_FAILURE'] == 'true':
    sys.exit(22)
if sys.argv[-1].endswith('/health'):
    print(os.environ['HEALTH_BODY'])
else:
    print(json.dumps({'commit': os.environ['RELEASE_SHA']}))
''')
            curl.chmod(0o755)
            sleep = Path(directory) / 'sleep'
            sleep.write_text('#!/bin/sh\nexit 0\n')
            sleep.chmod(0o755)
            env = dict(os.environ, PATH=directory + os.pathsep + os.environ['PATH'],
                       HEALTH_BODY=health, RELEASE_SHA=revision,
                       HTTP_FAILURE=str(http_failure).lower())
            return subprocess.run(['bash', str(SCRIPT), 'expected'], env=env,
                                  text=True, capture_output=True, timeout=30)

    def test_accepts_healthy_expected_revision(self):
        self.assertEqual(0, self.run_check().returncode)

    def test_rejects_previous_revision(self):
        self.assertNotEqual(0, self.run_check(revision='previous').returncode)

    def test_rejects_login_response(self):
        self.assertNotEqual(0, self.run_check(health='<html>Sign in</html>').returncode)

    def test_rejects_http_failure(self):
        self.assertNotEqual(0, self.run_check(http_failure=True).returncode)


if __name__ == '__main__':
    unittest.main()
