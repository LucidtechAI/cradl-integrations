## Cradl AI

Cradl AI is a no-code AI platform for automating internal document workflows. Cradl AI enables you to:

🚀️ Create customized AI models for any document type in any Latin-based language. <br />
👍 Deploy a fully fledged *human-in-the-loop* validation UI with one click <br />
🎉️ Automatically re-train and improve your model based on feedback from end users.<br />

## Publisher: Cradl AI

## Prerequisites

A free Cradl AI account. If you don't already have one, you can sign up for free [here](https://app.cradl.ai/signup).

## Supported Operations

This connector supports the following operations:

### Extract Data From Document
Extract data from documents like invoices, receipts, order confirmations.

### Extracted Data From Document
Trigger that runs when data is extracted from a document. 

### Validate Trigger Output
Validate output from trigger, to make sure that the output originates from Cradl.

### Get Document 
Get the content of a document.

### Get Document Metadata
Get metadata like name and content-type from a document.

### Create Document (deprecated)
Create a new document.

### Parse Document with Human-in-the-Loop (deprecated)

Parse a document with *Flows*. This operation runs asynchronous.

### Parse Document (deprecated)

Parse a document by calling the model directly. This operation runs synchronously.

## Obtaining Credentials

Log into Cradl AI, and in your *Flow* select either a Power Automate trigger or export and copy `Credentials` to the *Client Credentials* field.

## Getting Started

This quick start guide aims to provide a basic overview how Cradl AI can be integrated in a Power Automate workflow. Please refer to the [official documentation](https://docs.cradl.ai/) for up-to-date documentation.

#### 1. Set up a flow

*Cradl AI Flows* enables you to automate internal document processes in a simple, effective and unified way. 
It reduces the risk of using AI models in production since you can send uncertain documents to a *human-in-the-loop* when necessary. 
By adjusting your flow's *confidence thresholds*, you can decide when a document should be sent to manual verification and when it's allowed to pass straight through. 
To create a new flow, go to [Flows -> New flow](https://app.cradl.ai/agents).

### 2. Configure a Power Automate Trigger

Open your Cradl AI Flow and, in the *Trigger* section, select *Power Automate* from the list of available integrations. 
Create a new Flow in Power Automate, and select the action "Extract Data from Document" from Cradl AI. 
Open the dropdown menu for the parameter *Agent* and choose the Agent you just modified in Cradl AI.
Make sure you have a Trigger in your Power Automate Flow that has a valid file as output.

### 3. Configure a Power Automate Export 

Open your Cradl AI Flow and, in the *Export* section, select *Power Automate* from the list of available integrations. 
Create a new Flow in Power Automate, and select the trigger "Extracted Data from Document" from Cradl AI. 
Open the dropdown menu for the parameter *Export Action* and choose the Action you just created in Cradl AI.

## Known Issues and Limitations

See [API Limits](https://docs.cradl.ai/reference/quotas).

## Frequently Asked Questions

### Which document formats are supported?

JPEG, PNG, PDF, WEBP and TIFF.

### How many models do I need?

One per _document process_. For example, if you want to automate an expense approval process where you process receipts, invoices and airline tickets, we recommend using one model even if you process multiple document types with different layouts.

### Where is my data stored?

Please refer to our to our [Data Processing Agreement](https://docs.cradl.ai/legal/dpa) and [Privacy Policy](https://docs.cradl.ai/legal/privacy-policy) for more information about how personal data is processed.

## Deployment Instructions

Refer the documentation [here](https://learn.microsoft.com/en-us/connectors/custom-connectors/paconn-cli) to deploy this connector as a custom connector in Microsoft Power Automate and Power Apps.
