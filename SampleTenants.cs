using DatasetRedirect4ADLS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatasetRedirect4ADLS {
  class SampleTenants {

    public static PowerBiTenant Wingtip = new PowerBiTenant {
      Name = "Wingtip",
      AzureStorageAccountUrl = "https://powerbidevcamp.blob.core.windows.net/",
      ContainerName = "exceldata",
      ExcelFileName = "SalesDataProd.xlsx",
      AppliationKey = AppSettings.adlsStorageKey
    };

    public static PowerBiTenant Contoso = new PowerBiTenant {
      Name = "Contoso",
      AzureStorageAccountUrl = "https://powerbidevcamp.blob.core.windows.net/",
      ContainerName = "exceldata",
      ExcelFileName = "SalesDataProd2.xlsx",
      AppliationKey = AppSettings.adlsStorageKey
    };
  }
}
