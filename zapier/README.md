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

```bash
# Install dependencies
npm install  # or you can use yarn

# Run tests
zapier test

# Or you can link to an existing integration on Zapier
zapier link

# Push it to Zapier
zapier push
```

# Updating version

Update the version in package.json. 
Update CHANGELOG.md. 
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
