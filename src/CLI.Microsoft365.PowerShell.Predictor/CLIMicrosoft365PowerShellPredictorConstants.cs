namespace CLI.Microsoft365.PowerShell.Predictor
{
    internal static class CLIMicrosoft365PowerShellPredictorConstants
    {
        public const string SuggestionsFileName = "CLI.Microsoft365.PowerShell.Suggestions.{0}.json";
        public const string SuggestionsFileRelativePath = "\\Data";
        public const string DefaultVersion = "0.0.0";
        public const string EnvironmentVariableCommandSearchMethod = "CLIM365PredictorCommandSearchMethod";
        public const string EnvironmentVariableShowWarning = "CLIM365PredictorShowWarning";
        public const string LibraryName = "CLI.Microsoft365.PowerShell.Predictor.dll";
        public const string WarningMessageOnLoad = "WARNING: Unable to load latest predictions. Loading suggestions from local file. Hence some commands from the predictions might not work. Press enter to continue.";
        public const string GenericErrorMessage = "Unable to load predictions. Press enter to continue.";
    }
}
