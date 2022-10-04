using CLI.Microsoft365.PowerShell.Predictor.Abstractions.Interfaces;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace CLI.Microsoft365.PowerShell.Predictor
{
    internal sealed class CLIMicrosoft365PowerShellContext : ICLIMicrosoft365PowerShellContext
    {
        private static readonly Version DefaultVersion = new Version(CLIMicrosoft365PowerShellPredictorConstants.DefaultVersion);
        private IPowerShellRuntime _powerShellRuntime;

        public Version CLIMicrosoft365Version { get; private set; } = DefaultVersion;
        public CLIMicrosoft365PowerShellContext(IPowerShellRuntime powerShellRuntime) => _powerShellRuntime = powerShellRuntime;
        public Runspace DefaultRunspace => _powerShellRuntime.DefaultRunspace;

        public void UpdateContext()
        {
            CLIMicrosoft365Version = GetCLIMicrosoft365Version();
        }

        private Version GetCLIMicrosoft365Version()
        {
            Version latestVersion = DefaultVersion;

            try
            {
                var script =
                    $@"
                    
                    $details = [pscustomobject]@{{
                        Name = ""@pnp/cli-microsoft365""
                        Version = ""{CLIMicrosoft365PowerShellPredictorConstants.DefaultVersion}""
                    }};

                    try {{
                        $details.version = $(m365 version).replace('v', '').replace('""', '');
                    }} 
                    catch {{
                        Write-Warning ""Can't get the installed M365 CLI version"";
                    }}
                    finally {{
                        $details;
                    }}";

                var outputs = _powerShellRuntime.ExecuteScript<PSObject>(script);

                if (outputs?.Any() == true)
                {
                    ExtractAndSetLatestCLIMicrosoft365Version(outputs);
                }
            }
            catch (Exception)
            {

            }

            return latestVersion;

            void ExtractAndSetLatestCLIMicrosoft365Version(IEnumerable<PSObject> outputs)
            {
                foreach (var psObject in outputs)
                {
                    string versionOutput = psObject.Properties["Version"].Value.ToString();
                    Version currentVersion = new Version(versionOutput);
                    if (currentVersion > latestVersion)
                    {
                        latestVersion = currentVersion;
                    }
                }
            }
        }
    }
}
