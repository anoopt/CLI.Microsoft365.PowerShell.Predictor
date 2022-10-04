using CLI.Microsoft365.PowerShell.Predictor.Abstractions.Models;

namespace CLI.Microsoft365.PowerShell.Predictor;

internal class Settings
{
    public CommandSearchMethod CommandSearchMethod { get; set; }
    public bool ShowWarning { get; set; }

    private static CommandSearchMethod GetCommandSearchMethod()
    {
        var cliM365PredictorCommandSearchMethod = Environment.GetEnvironmentVariable(CLIMicrosoft365PowerShellPredictorConstants.EnvironmentVariableCommandSearchMethod);
        
        if (cliM365PredictorCommandSearchMethod == null)
        {
            return CommandSearchMethod.Contains;
        }

        switch (cliM365PredictorCommandSearchMethod)
        {
            case "Contains":
                return CommandSearchMethod.Contains;
            case "StartsWith":
                return CommandSearchMethod.StartsWith;
            default:
                return CommandSearchMethod.StartsWith;
        }
    }
    
    private static bool GetShowWarning()
    {
        var cliM365PredictorShowWarning = Environment.GetEnvironmentVariable(CLIMicrosoft365PowerShellPredictorConstants.EnvironmentVariableShowWarning);
        
        if (cliM365PredictorShowWarning == null)
        {
            return true;
        }

        return bool.Parse(cliM365PredictorShowWarning);
    }

    public static Settings GetSettings()
    {
        return new Settings()
        {
            CommandSearchMethod = GetCommandSearchMethod(),
            ShowWarning = GetShowWarning()
        };
    }
}