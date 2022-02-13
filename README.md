# IDataProtectionFirebase
A .NET library for IDataProtection to store keys in a Google Firebase instance.

https://github.com/kibblewhite/IDataProtectionFirebase

## Getting Started

Follow the examples below to see how the library can be integrated into your application.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddDataProtection()
        .SetApplicationName(Assembly.GetExecutingAssembly().FullName!)
        .PersistKeysToFirebase(service_name, json_credentials, project_id);

    services.AddMvc();
}
```

### Service Name

The `service_name` is to help you identify the data entries in the [firebase database](https://console.firebase.google.com/)

### Project ID

The `project_id` is provided by google:
- https://cloud.google.com/resource-manager/docs/creating-managing-projects

#### Unit Test

You will also need to update the `ProjectId` field inside of the `appsettings.Test.json`

```json
{
  "ProjectId": "project-id-from-gcp-firebase"
}
```

### JSON Credentials

You will also need to create a service account key in the form of a JSON key file (credentials.json)

Read this into the `json_credentials` variable.

You can get this from the Google Cloud Platform console (GCP), for more information on this read the following:
- https://cloud.google.com/docs/authentication/getting-started

## Getting Help

Unfortunately it is only me, feel free to submit bug reports or feature requests, but please keep in mind that it is not a priority for me to keep this project updated or operational.


### Note

Check out the three unit tests and scan through the `appsettings.Test.json`.

You can also use the `sample-json-credentials.json` as reference.

