# Get started
Set up your local .env file with the following fields:
```bash
client_id=<Your app client ID>
client_secret=<Your app client secret>
API_BASE_URL=<base URL to the Cradl API>
API_AUTH_URL=<auth URL for the Cradl API>
TEST_MODEL_ID=<ID of the model you want to test with>
TEST_WORKFLOW_ID=<ID of the workflow you want to test with>
```

# Install dependencies
```bash
npm install  # or you can use yarn
npm install -g zapier-platform-cli
```

# Login to zapier user and check that you have necessary authorization
```bash
zapier login
zapier integrations
```

# Run tests
```bash
zapier test
```

# Make updates to the integration
Docs here: https://github.com/zapier/zapier-platform/blob/main/packages/cli/README.md

# Push update to Zapier
Note: This it is only possible to push to a non-public version. You'll probably want to update the version in package.json.
```bash
zapier push
```

# Updating version
Update the version in package.json. 
Update CHANGELOG.md. 
Check that environment variables are set correctly by comparing with old version:
```bash
zapier env:get <old version>
zapier env:get <new version>
```
Set the API_BASE_URL and API_AUTH_URL values with 
```bash
zapier env:set x.y.z API_BASE_URL=<url>
zapier env:set x.y.z API_AUTH_URL=<url>
```
Promote your version with 
```bash
zapier promote x.y.z
```
Consider migrating users.
```bash
zapier migrate --help
```