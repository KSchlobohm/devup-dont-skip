# Configuration Demo script

This demo capture changes shown in the Pull Request https://github.com/KSchlobohm/devup-dont-skip/pull/1/files

## Pre-requisite

1. Create an environment variable to be used with the External Configuration code sample.

![#image of environment var used in demo](../docs/UserVariables.png)

|Name|Value|
|--|--|
|AppSetting_message|Have a great environment|


## Step 1: Load Configuration from Environment vars

- How can we load configuration from different sources without changing code?

**Demo**

1. Add a package reference to the *OdeToFood.WebUI* project.

    > Microsoft.Configuration.ConfigurationBuilders.Environment

1. View **Warnings** and use VisualStudio to fix bindingRedirects as recommended

    ...insert image...


1. Modify the `web.config` of OdeToFood.WebUI to use Configuration Builders

    1. Add configSections

    ```xml
          <configSections>
            <section name="configBuilders" type="System.Configuration.ConfigurationBuildersSection,
			         System.Configuration, Version=4.0.0.0, Culture=neutral,
			         PublicKeyToken=b03f5f7f11d50a3a" restartOnExternalChanges="false" requirePermission="false" />
          </configSections>
          <configBuilders>
            <builders>
              <add name="AS_Environment" mode="Strict" prefix="AppSetting_" stripPrefix="true" type="Microsoft.Configuration.ConfigurationBuilders.EnvironmentConfigBuilder,
		           Microsoft.Configuration.ConfigurationBuilders.Environment" />
            </builders>
          </configBuilders>
    ```

    1. Modify the app settings to include new values
    
    > **Note**<br>
    > Observe the new attribute used on the appSettings block which correlates to config builder sources.
    
    > **Note**<br>
    > One of the *gotchas* of this approach is that the alue must exist (even if blank) in the web.config file before it can be overriden with a config builder.

    ```xml
      <appSettings configBuilders="AS_ENVIRONMENT">
        <add key="message" value="Have a great day" />

        <add key="webpages:Version" value="3.0.0.0" />
        <add key="webpages:Enabled" value="false" />
        <add key="ClientValidationEnabled" value="true" />
        <add key="UnobtrusiveJavaScriptEnabled" value="true" />
      </appSettings>
    ```

## Step 2: Override Environment Configuration with a config file
Let's explore layered configuration by viewing 2 configuration sources and 2 different messages.

- What happens if we only put a configuration into one of the two sources?
- What happens if we define a configuration value in two sources?

**Demo**

1. Add a package reference to the *OdeToFood.WebUI* project.

    > Microsoft.Configuration.ConfigurationBuilders.Json

1. View **Warnings** and use VisualStudio to fix bindingRedirects as recommended

    ...insert image...

1. Update the Home/Index controller action to display a new message.

    ```cs
    //todo: get greeting from configuration
    var greeting = ConfigurationManager.AppSettings["greeting"];
    ViewBag.Greeting = greeting ?? "Default Greetings";
    ```

1. Update the Home/Index view to display a new message.

    ```cshtml
        @* //todo: display greeting *@
        <div>
            @ViewBag.Greeting
        </div>
    ```

1. Create a new file named config.json

    ```json
    {
        "appSettings": {
            "message": "Have a great json",
            "greeting": "Hello dev up 2023!"
        }
    }
    ```

1. Configure the file as copy to output directory

1. Modify the `web.config` of OdeToFood.WebUI to use Configuration Builders

    1. Add the new Config Builder definition
    
    ```xml
            <builders>
              <add name="AS_Environment" mode="Strict" prefix="AppSetting_" stripPrefix="true" type="Microsoft.Configuration.ConfigurationBuilders.EnvironmentConfigBuilder,
		           Microsoft.Configuration.ConfigurationBuilders.Environment" />
	          <add name="AS_Json" mode="Strict" jsonFile="~\config.json" jsonMode="Sectional" type="Microsoft.Configuration.ConfigurationBuilders.SimpleJsonConfigBuilder,
		           Microsoft.Configuration.ConfigurationBuilders.Json" />
            </builders>
    ```

    1. Override the attribute in AppSettings and define the new *greeting* configuration
    
    ```xml
      <appSettings configBuilders="AS_ENVIRONMENT">
        <add key="message" value="Have a great day" />
        <add key="greeting" value="must exist to be overriden"/>

        <add key="webpages:Version" value="3.0.0.0" />
        <add key="webpages:Enabled" value="false" />
        <add key="ClientValidationEnabled" value="true" />
        <add key="UnobtrusiveJavaScriptEnabled" value="true" />
      </appSettings>
    ```