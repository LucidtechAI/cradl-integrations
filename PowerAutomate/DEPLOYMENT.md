## Deployment instructions

The deployment pipeline normally takes around 14 days. 
1. First Microsoft verifies the package (1-2 days)
2. Testing in a preview environment that Microsoft creates for us
    a. Make sure you login to the preview account. Username and password will be sent to you in two separate emails
    b. Select the right environment to test the new connector. It is not the one called "Microsoft (default)", but the other one.
    c. Start a free trial to give permission to test the new connector since it is a premium feature. [https://learn.microsoft.com/en-us/power-platform/admin/power-automate-licensing/deep-dive-on-specific-license#power-automate-trial-license](docs).
    d. Test that your connector works in a flow or two.
3. Press the "Go live" button in the partner center (10-14 days before the connector is rolled out to all regions)

As of 20.02.2026 These are the necessary steps to create the package:
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

