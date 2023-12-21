# Usage

Go to the official [documentation](https://docs.cradl.ai/integrations/uipath) to see how these activities can be used in UiPath.

The code in [https://github.com/LucidtechAI/cradl-integrations/UiPath]([https://github.com/LucidtechAI/cradl-integrations/UiPath]) is used to create the activities found in [this](https://www.nuget.org/packages/CradlAI.Activities) nuget package.

# Development for Linux Users
1. Download virtualbox 
```commandline
# sudo pacman -S virtualbox  
```
2. Download Visual Studio from [Microsoft](https://visualstudio.microsoft.com/downloads/). Make sure you choose the _.NET desktop development workload_ during the installation. 
3. Download UIPath Studio by following [the official guide](https://docs.uipath.com/studio/standalone/2022.10/user-guide/install-studio).
4. Install the Activity Creator in Visual Studio, by following these [steps](https://docs.uipath.com/activities/other/latest/developer/using-activity-creator).

# Build .nupkg
Select CradlAI.Activities.Designer in the project menu and press _Build_ in the top bar.

# Testing in UIPath Studio
Open _Manage Packages_ -> Settings and add the path to your local build. You should now be able to find the local package and test it in your flow.
