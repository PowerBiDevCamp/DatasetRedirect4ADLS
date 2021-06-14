using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatasetRedirect4ADLS.Models {
  
  class PowerBiTenant {
    public string Name { get; set; }
    public string AzureStorageAccountUrl { get; set; }
    public string ContainerName { get; set; }
    public string ExcelFileName { get; set; }
    public string AppliationKey { get; set; }
  }

}
