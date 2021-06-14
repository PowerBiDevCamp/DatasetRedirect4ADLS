using System;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.PowerBI.Api.Models.Credentials;

namespace DatasetRedirect4ADLS.Models {
  class DatasetManager {

    public static void DisplayWorkspaces() {
      Console.WriteLine("Running DisplayWorkspaces");
      PowerBIClient pbiClient = TokenManager.GetPowerBiClient(PowerBiPermissionScopes.ReadWorkspaces);
      var workspaces = pbiClient.Groups.GetGroups().Value;
      if (workspaces.Count == 0) {
        Console.WriteLine("There are no workspaces for this user");
      }
      else {
        foreach (var workspace in workspaces) {
          Console.WriteLine("  " + workspace.Name + " - [" + workspace.Id + "]");
        }
      }
      Console.WriteLine();
    }


    public static Group CreatWorkspace(string Name) {
      PowerBIClient pbiClient = TokenManager.GetPowerBiClient(PowerBiPermissionScopes.TenantProvisioning);

      GroupCreationRequest request = new GroupCreationRequest(Name);
      Group workspace = pbiClient.Groups.CreateGroup(request);
      return workspace;
    }

    public void DeleteWorkspace(Guid WorkspaceId) {
      PowerBIClient pbiClient = TokenManager.GetPowerBiClient(PowerBiPermissionScopes.TenantProvisioning);
      pbiClient.Groups.DeleteGroup(WorkspaceId);
    }

    public static Dataset GetDataset(Guid WorkspaceId, string DatasetName) {
      PowerBIClient pbiClient = TokenManager.GetPowerBiClient(PowerBiPermissionScopes.ReadWorkspaceAssets);
      var datasets = pbiClient.Datasets.GetDatasetsInGroup(WorkspaceId).Value;
      foreach (var dataset in datasets) {
        if (dataset.Name.Equals(DatasetName)) {
          return dataset;
        }
      }
      return null;
    }

    public static void DisplayDatasetInfo(Guid WorkspaceId, string DatasetId) {

      PowerBIClient pbiClient = TokenManager.GetPowerBiClient(PowerBiPermissionScopes.ReadWorkspaceAssets);
      IList<Dataset> datasets = pbiClient.Datasets.GetDatasetsInGroup(WorkspaceId).Value;

      var dataset = datasets.Where(ds => ds.Id.Equals(DatasetId)).Single();

      var newline = Environment.NewLine;

      Console.WriteLine("Name: " + dataset.Name);
      Console.WriteLine("ConfiguredBy: " + dataset.ConfiguredBy);
      Console.WriteLine();
      Console.WriteLine("IsEffectiveIdentityRequired: " + dataset.IsEffectiveIdentityRequired);
      Console.WriteLine("IsEffectiveIdentityRolesRequired: " + dataset.IsEffectiveIdentityRolesRequired);
      Console.WriteLine("IsOnPremGatewayRequired: " + dataset.IsOnPremGatewayRequired);
      Console.WriteLine();

      Console.WriteLine("Datasources:");
      IList<Datasource> datasources = pbiClient.Datasets.GetDatasourcesInGroup(WorkspaceId, DatasetId).Value;
      foreach (var datasource in datasources) {
        Console.WriteLine("  [" + datasource.DatasourceType + "] server=" +
                          datasource.ConnectionDetails.Server + " database=" +
                          datasource.ConnectionDetails.Database);
      }
      Console.WriteLine();

      IList<Refresh> refreshes = null;
      if (dataset.IsRefreshable == true) {
        Console.WriteLine("Refresh History:");
        refreshes = pbiClient.Datasets.GetRefreshHistoryInGroup(WorkspaceId, DatasetId).Value;
        if (refreshes.Count == 0) {
          Console.WriteLine("  This dataset has never been refreshed.");
        }
        else {
          foreach (var refresh in refreshes) {
            Console.WriteLine("  " + refresh.RefreshType.Value +
                              " refresh on " + refresh.StartTime.Value.ToShortDateString() +
                              " | Started: " + refresh.StartTime.Value.ToLocalTime().ToLongTimeString() +
                              " | Completed:  " + refresh.EndTime.Value.ToLocalTime().ToLongTimeString());
          }
        }
      }

      Console.WriteLine();
    }

    public static Import ImportPBIX(Guid WorkspaceId, string PbixFilePath, string ImportName) {
      PowerBIClient pbiClient = TokenManager.GetPowerBiClient(PowerBiPermissionScopes.TenantProvisioning);

      // open PBIX in file stream
      FileStream stream = new FileStream(PbixFilePath, FileMode.Open, FileAccess.Read);

      // post import to start import process
      var import = pbiClient.Imports.PostImportWithFileInGroup(WorkspaceId, stream, ImportName, ImportConflictHandlerMode.CreateOrOverwrite);

      // poll to determine when import operation has complete
      do { import = pbiClient.Imports.GetImportInGroup(WorkspaceId, import.Id); }
      while (import.ImportState.Equals("Publishing"));

      // return Import object to caller
      return import;
    }

    public static Import ImportPBIX(Guid WorkspaceId, byte[] PbixContent, string ImportName) {
      PowerBIClient pbiClient = TokenManager.GetPowerBiClient(PowerBiPermissionScopes.TenantProvisioning);

      MemoryStream stream = new MemoryStream(PbixContent);
      var import = pbiClient.Imports.PostImportWithFileInGroup(WorkspaceId, stream, ImportName, ImportConflictHandlerMode.CreateOrOverwrite);

      do { import = pbiClient.Imports.GetImportInGroup(WorkspaceId, import.Id); }
      while (import.ImportState.Equals("Publishing"));

      return import;
    }

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

    public static void UpdateParameter(Guid WorkspaceId, string DatasetId, string ParameterName, string ParameterValue) {
    
      PowerBIClient pbiClient = TokenManager.GetPowerBiClient(PowerBiPermissionScopes.TenantProvisioning);

      IList<Dataset> datasets = pbiClient.Datasets.GetDatasetsInGroup(WorkspaceId).Value;
      var dataset = datasets.Where(ds => ds.Id.Equals(DatasetId)).Single();
      UpdateMashupParametersRequest req =
        new UpdateMashupParametersRequest(
          new UpdateMashupParameterDetails {
            Name = ParameterName,
            NewValue = ParameterValue
          });
      
      pbiClient.Datasets.UpdateParametersInGroup(WorkspaceId, DatasetId, req);
    }

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

    public static void DeleteAllWorkspaces() {
      PowerBIClient pbiClient = TokenManager.GetPowerBiClient(PowerBiPermissionScopes.TenantProvisioning);
      var workspaces = pbiClient.Groups.GetGroups().Value;
      foreach (var workspace in workspaces) {
        Console.WriteLine("Deleting " + workspace.Name);
        pbiClient.Groups.DeleteGroup(workspace.Id);
      }
      Console.WriteLine();
    }



  }
}