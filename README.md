# IDataProtectionFirebase
A .NET library for IDataProtection to store keys in a Google Firebase instance.

GitHub Repository URL: https://github.com/kibblewhite/IDataProtectionFirebase

NuGet Package URL: https://www.nuget.org/packages/IDataProtectionFirebase/

## Getting Started

Follow the examples below to see how the library can be integrated into your application.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddDataProtection()
        .PersistKeysToFirebase(service_name, json_credentials);

    services.AddMvc();
}
```


If the FirestoreDb instance is already in the running assembly's service collection ready for DI, then the following code can be used:
The library will throw an ArgumentNullException if the FirestoreDb can not be located.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    service_container.AddDataProtection()
        .PersistKeysToFirebase(service_name);

    services.AddMvc();
}
```

To see a examples of this code, please visit the `UnitTests.cs` within the Tests project. If you wish to run the unit tests, you will need to have a valid `credentials.json` in the unit test project directory that has access to your Firebase DB instance from GCP.

### Service Name

The `service_name` can be any "reasonable" string value that you wish. It is to help you identify the data entries in the [firebase database](https://console.firebase.google.com/)


### JSON Credentials

You will also need to create a service account key (which has access to the Firebase DB) in the form of a JSON key file (credentials.json)

Read this into the `json_credentials` variable.

You can get this from the Google Cloud Platform console (GCP), for more information on this read the following:
- https://cloud.google.com/docs/authentication/getting-started


## Getting Help

Unfortunately it is only me, feel free to submit bug reports or feature requests, but please keep in mind that it is not a priority for me to keep this project updated or operational.


### Note

Check out the unit tests and you can also use the `credentials.sample.json` as reference.


This is just a note more for me than for you, to remind me of the nuget publish command:
```bash
dotnet nuget push IDataProtectionFirebase\bin\Release\net6.0\publish\IDataProtectionFirebase.*.nupkg -k [api-key-here /] -s https://api.nuget.org/v3/index.json
```
