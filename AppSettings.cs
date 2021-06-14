using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatasetRedirect4ADLS {
  class AppSettings {

    // metadata from public Azure AD application
    public const string ApplicationId = "APP_ID_FOR_PUBLIC_APPLICATION";
    public const string RedirectUri = "http://localhost";

    // metadata from confidential Azure AD application for service principal
    public const string tenantId = "TENANT_ID";
    public const string confidentialApplicationId = "APP_ID_FOR_CONFIDENTIAL_APPLICATION";
    public const string confidentialApplicationSecret = "APP_SECRET_FOR_CONFIDENTIAL_APPLICATION";
    public const string tenantSpecificAuthority = "https://login.microsoftonline.com/" + tenantId;
    public const string servicePrincipalObjectId = "SERVICE_PRINIPAL_OBJECT_ID";
    public const string adminUser = "YOUR_DEV_ACCOUNT@YOUR_TENANT.onMicrosoft.com";


    // info for PBIX import operation
    public const string datasetName = "SalesDemo";
    public const string targetWorkspaceId = "9dd30f74-aed6-472d-95eb-8e90bf3294e5";

    // info for query overwrite operation
    public const string tableName = "Sales"; // must match table name defined in PBIX file

    // info required to u[pdate query to redirect to ADLS
    public const string adlsFilePath = "https://powerbidevcamp.blob.core.windows.net/exceldata/";
    public const string adlsBlobAccount = "https://powerbidevcamp.blob.core.windows.net/";
    public const string adlsBlobContainer = "exceldata/";
    public const string adlsFileName = "SalesDataProd2.xlsx";

    // key required to configure credentials
    public const string adlsStorageKey = "YOUR_ADLS_STORAGE_KEY";

  }
}
