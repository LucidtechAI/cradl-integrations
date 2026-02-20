## Deployment instructions

As of 20.02.2026 These are the necessary steps: 

```
# Create two solutions in Power Automate. One that contains only the connector, and another that contains the connector and one or more example flows.
# Export the two solutions, lets call them ConnectorSolution.zip and FlowSolution.zip
mkdir CradlConnector
cd CradlConnector
mkdir PkgAssets
cp /path/to/readme.md intro.md
cp /path/to/ConnectorSolution.zip PkgAssets
cp /path/to/FlowSolution.zip PkgAssets
zip -r package.zip PkgAssets/              
zip -r SubmissionPackage.zip intro.md package.zip
# Upload SubmissionPackage.zip to Azure
# Rightclick and choose Generate SAS
# Make sure to set the expiry date to at least 15 days from now 
# Paste the URL into your Marketplace offer
```

