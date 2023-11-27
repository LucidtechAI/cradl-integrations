# Get started

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

Remember to set the API_BASE_URL and API_AUTH_URL values with 

```bash
zapier env:set x.y.z API_BASE_URL=<url>
zapier env:set x.y.z API_AUTH_URL=<url>
```
