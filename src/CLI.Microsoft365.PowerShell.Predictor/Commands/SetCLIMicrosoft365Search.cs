using System.Management.Automation;
using System.Reflection;
using System.Text;
using CLI.Microsoft365.PowerShell.Predictor.Abstractions.Models;

namespace CLI.Microsoft365.PowerShell.Predictor.Commands;

[Cmdlet(VerbsCommon.Set, "CLIMicrosoft365PredictorSearch")]
public sealed class SetCLIMicrosoft365Search : PSCmdlet
{
    private static readonly string[] ReloadModuleStatements =
    {
#if DEBUG
        $"Remove-Module {Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), CLIMicrosoft365PowerShellPredictorConstants.LibraryName)} -Force",
        $"Import-Module {Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), CLIMicrosoft365PowerShellPredictorConstants.LibraryName)} -Force"
#else
        "Remove-Module -Name CLI.Microsoft365.PowerShell.Predictor -Force",
        "Import-Module -Name CLI.Microsoft365.PowerShell.Predictor -Force"
#endif
    };

    [Parameter(Mandatory = true, Position = 0)]
    public CommandSearchMethod Method { get; set; }

    protected override void ProcessRecord()
    {
        var scriptToRun = new StringBuilder();
        var _ = scriptToRun.Append(string.Join(";", ReloadModuleStatements));

        if (Method.GetType() == typeof(CommandSearchMethod))
        {
            Environment
                .SetEnvironmentVariable(
                    CLIMicrosoft365PowerShellPredictorConstants.EnvironmentVariableCommandSearchMethod,
                    Method.ToString()
                );
            Environment
                .SetEnvironmentVariable(
                    CLIMicrosoft365PowerShellPredictorConstants.EnvironmentVariableShowWarning,
                    "false"
                );
            InvokeCommand.InvokeScript(scriptToRun.ToString());
        }
    }
}