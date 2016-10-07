using System.Deployment.Application;
using System.Reflection;

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
