using System.Management.Automation;
using System.Management.Automation.Subsystem;

namespace CLI.Microsoft365.PowerShell.Predictor
{
    /// <summary>
    /// Register the predictor on module loading and unregister it on module un-loading.
    /// </summary>
    public class Init : IModuleAssemblyInitializer, IModuleAssemblyCleanup
    {
        private const string Identifier = "86bcce54-299e-460f-a4f3-7388da8e8d58";

        /// <summary>
        /// Gets called when assembly is loaded.
        /// </summary>
        public void OnImport()
        {
            var predictor = new CLIMicrosoft365PowerShellPredictor(Identifier);
            SubsystemManager.RegisterSubsystem(SubsystemKind.CommandPredictor, predictor);
        }

        /// <summary>
        /// Gets called when the binary module is unloaded.
        /// </summary>
        public void OnRemove(PSModuleInfo psModuleInfo)
        {
            SubsystemManager.UnregisterSubsystem(SubsystemKind.CommandPredictor, new Guid(Identifier));
        }
    }
}