# Dataset Redirect for ADLS
This repository holds a developer sample as a simple C# console application named **DatasetRedirect4ADLS** that demonstrates uploading a PBIX template file for a dataset that is parameterized to redirect to an Excel file in ADLS storage.

You can download the PBIX template file from [here](https://github.com/PowerBiDevCamp/DatasetRedirect4ADLS/raw/main/PBIX/DatasetRedirect4ADLS.pbix). If you open this PBIX file, you will see it creates these dataset parameters named **AzureStorageAccountUrl**, **ContainerName** and **ExcelFileName**.

![Dataset Parameters](images/imgage1.jpg?raw=true "Adding dataset parameters to a PBIX project file.")
 

Here is the M code behind the query which uses these three dataset parameters to import data from an Excel file in ADLS.

```M
let
    Source = AzureStorage.Blobs(AzureStorageAccountUrl),
    StorageContainer = Source{[Name=ContainerName]}[Data],
    FolderPath = AzureStorageAccountUrl & ContainerName & "/",
    ExcelFileBlob = StorageContainer{[#"Folder Path"=FolderPath, Name=ExcelFileName]}[Content],
    ImportExcelFile = Excel.Workbook(ExcelFileBlob),
    OpenExcelTable = ImportExcelFile{[Item="Sales",Kind="Table"]}[Data],
    Output = Table.TransformColumnTypes(OpenExcelTable,{{"Sales", Currency.Type}})
in
    Output
```

After creating the parameterized PBIX file, I wrote C# code in a C# console application named DatasetRedirect4ADLS. This developer sample uses the Power BI .NET SDK to demonstrate how to implement the following onboarding logic.

(1)	Create a new tenant workspace
(2)	Import the PBIX file template file to create a new dataset
(3)	Update the dataset parameters to redirect datasource to a specific Excel file in ADLS storage
(4)	Set the datasource credentials using the ADLS storage key
(5)	Start a refresh operation to populate the dataset with data from the Excel file in ADFS storage

You can examine the code I wrote in this C# source file named DatasetManager.cs. Inside this C# source file, you will find a function named PatchAdlsCredentials which demonstrates how to set datasource credentials using the Azure storage key.

```C#
public static void PatchAdlsCredentials(Guid WorkspaceId, string DatasetId) {
  PowerBIClient pbiClient = TokenManager.GetPowerBiClient(PowerBiPermissionScopes.TenantProvisioning);
  pbiClient.Datasets.TakeOverInGroup(WorkspaceId, DatasetId);
  var datasources = (pbiClient.Datasets.GetDatasourcesInGroup(WorkspaceId, DatasetId)).Value;
  foreach (var datasource in datasources) {
    if (datasource.DatasourceType.ToLower() == "azureblobs") {
     // get the datasourceId and the gatewayId
     var datasourceId = datasource.DatasourceId;
      var gatewayId = datasource.GatewayId;
     // Create UpdateDatasourceRequest to update access key
     UpdateDatasourceRequest req = new UpdateDatasourceRequest {
        CredentialDetails = new CredentialDetails(
          new KeyCredentials(AppSettings.adlsStorageKey),
          PrivacyLevel.None,
          EncryptedConnection.NotEncrypted)
      };
     // Execute Patch command to update credentials
     pbiClient.Gateways.UpdateDatasource((Guid)gatewayId, (Guid)datasourceId, req);
    }
  };
}
```

There is also another function named OnboardNewTenant which contains the top-level logic for the entire onboarding process.


public static void OnboardNewTenant(PowerBiTenant Tenant) {

  Console.WriteLine("Starting tenant onboarding process for " + Tenant.Name);
  PowerBIClient pbiClient = TokenManager.GetPowerBiClient(PowerBiPermissionScopes.TenantProvisioning);

  Console.WriteLine(" - Creating workspace for " + Tenant.Name + "...");
  GroupCreationRequest request = new GroupCreationRequest(Tenant.Name);
  Group workspace = pbiClient.Groups.CreateGroup(request);
      
  Console.WriteLine(" - Importing PBIX teplate file...");
  string importName = "Sales";

  var import = DatasetManager.ImportPBIX(workspace.Id, Properties.Resources.DatasetRedirect4ADLS_pbix, importName);

  Dataset dataset = GetDataset(workspace.Id, importName);

  Console.WriteLine(" - Updating dataset parameters...");
  UpdateMashupParametersRequest req =
    new UpdateMashupParametersRequest(new List<UpdateMashupParameterDetails>() {
      new UpdateMashupParameterDetails { Name = "AzureStorageAccountUrl", NewValue = Tenant.AzureStorageAccountUrl },
      new UpdateMashupParameterDetails { Name = "ContainerName", NewValue = Tenant.ContainerName },
      new UpdateMashupParameterDetails { Name = "ExcelFileName", NewValue = Tenant.ExcelFileName }
  });

  pbiClient.Datasets.UpdateParametersInGroup(workspace.Id, dataset.Id, req);

  Console.WriteLine(" - Patching datasourcre credentials...");
  PatchAdlsCredentials(workspace.Id, dataset.Id);

  Console.WriteLine(" - Starting dataset refresh operation...");
  pbiClient.Datasets.RefreshDatasetInGroup(workspace.Id, dataset.Id);

  Console.WriteLine(" - Tenant onboarding processing completed");
  Console.WriteLine();
}

Let me know if you have any questions. I would be happy to demo this in our next meeting if you'd like me to. If you are not using the Power BI .NET SDK, I can also help to translate this code into what you will need if you are executing HTTP requests directly against the Power BI Service API.

