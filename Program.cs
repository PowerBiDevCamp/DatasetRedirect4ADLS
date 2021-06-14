using System;
using DatasetRedirect4ADLS.Models;

namespace DatasetRedirect4ADLS {
  class Program {

    static void Main() {

      DatasetManager.DeleteAllWorkspaces();

      DatasetManager.OnboardNewTenant(SampleTenants.Wingtip);
      //DatasetManager.OnboardNewTenant(SampleTenants.Contoso);

    }

  }
}
