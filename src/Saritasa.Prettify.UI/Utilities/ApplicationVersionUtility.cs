using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Prettify.UI.Utilities
{
    public class ApplicationVersionUtility
    {
        public static string GetVersion()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                return ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }

            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
