using System.Management.Automation.Runspaces;

namespace CLI.Microsoft365.PowerShell.Predictor.Abstractions.Interfaces
{
    internal interface ICLIMicrosoft365PowerShellContext
    {
        public Version CLIMicrosoft365Version { get; }
        public void UpdateContext();
        public Runspace DefaultRunspace { get; }
    }
}
