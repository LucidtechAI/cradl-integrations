# Usage

Go to the official [documentation](https://docs.cradl.ai/integrations/uipath) to see how these activities can be used in UiPath.

The code in [cradl-integrations/UiPath](https://github.com/LucidtechAI/cradl-integrations/tree/main/UiPath) is used to create the activities found in [this](https://www.nuget.org/packages/CradlAI.UIPath.Activities) nuget package.


# Development
1. Download virtualbox.
```commandline
sudo pacman -S virtualbox  
```
2. Download Visual Studio from [Microsoft](https://visualstudio.microsoft.com/downloads/). Make sure you choose the _.NET desktop development workload_ during the installation. 
3. Download UIPath Studio by following [the official guide](https://docs.uipath.com/studio/standalone/2022.10/user-guide/install-studio).
4. Install the Activity Creator in Visual Studio, by following these [steps](https://docs.uipath.com/activities/other/latest/developer/using-activity-creator).


# Build .nupkg
Select CradlAI.Activities.Designer in the project menu and press _Build_ in the top bar.


# Testing in UIPath Studio
Open _Manage Packages_ -> Settings and add the path to your local build. You should now be able to find the local package and test it in your flow.


# Listing information

## Title
Cradl AI - Automated Data Extraction Demo

## Asset Pitch 
Boost UiPath with Crad AI: State of the art document parsing powered by the latest within machine learning.

## Features
Two illustrative examples of how to use Cradl AI in UiPath
1. Send your document to Cradl AI for data extraction and optional human verification. 
2. Retrieve extracted data from Cradl AI.
A feedback loop is automatically created so your model gets better for every document you parse with Cradl AI.

## About Us

Build cutting edge AI models for document parsing and seamlessly integrate into your existing workflows in UiPath. 
Instead of relying on pre-trained models that almost fit your case, Cradl AI enables you to train your own model on your own documents. 
Our ML models are language agnostic and designed to be easily trained on any document type across industries. 
With as little as 15 examples our deep neural network is able to adapt to your documents and provide you with the output that you need to automate your document process. 
Cradl AI offers an all-in-one AI platform that enables automated document parsing as well as an optional human-in-the-loop approach for superior accuracy and adaptability.

