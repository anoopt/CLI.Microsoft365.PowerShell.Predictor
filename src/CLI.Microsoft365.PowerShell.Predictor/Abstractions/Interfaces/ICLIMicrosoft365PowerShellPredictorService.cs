using System.Management.Automation.Subsystem.Prediction;

namespace CLI.Microsoft365.PowerShell.Predictor.Abstractions.Interfaces
{
    internal interface ICLIMicrosoft365PowerShellPredictorService
    {
        public List<PredictiveSuggestion>? GetSuggestions(PredictionContext context);
    }
}
