using CLI.Microsoft365.PowerShell.Predictor.Abstractions.Interfaces;
using CLI.Microsoft365.PowerShell.Predictor.Abstractions.Models;
using System.Management.Automation.Subsystem.Prediction;
using System.Reflection;
using System.Text.Json;

namespace CLI.Microsoft365.PowerShell.Predictor.Services
{
    internal class CLIMicrosoft365PowerShellPredictorService : ICLIMicrosoft365PowerShellPredictorService
    {
        private List<Suggestion>? _allPredictiveSuggestions;
        private readonly CommandSearchMethod _commandSearchMethod;
        private string _commandsFileName;

        public CLIMicrosoft365PowerShellPredictorService(ICLIMicrosoft365PowerShellContext cliMicrosoft365PowerShellContext, Settings settings)
        {
            _commandsFileName = string.Format(CLIMicrosoft365PowerShellPredictorConstants.SuggestionsFileName,
                cliMicrosoft365PowerShellContext.CLIMicrosoft365Version);
            _commandSearchMethod = settings.CommandSearchMethod;
            RequestAllPredictiveCommands(settings.ShowWarning);
        }

        private async Task SetPredictiveSuggestions()
        {
            string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string fileName = Path.Combine($"{executableLocation}{CLIMicrosoft365PowerShellPredictorConstants.SuggestionsFileRelativePath}", _commandsFileName);
            string jsonString = await File.ReadAllTextAsync(fileName);
            _allPredictiveSuggestions = JsonSerializer.Deserialize<List<Suggestion>>(jsonString)!;
        }

        protected virtual void RequestAllPredictiveCommands(bool showWarning)
        {
            //TODO: Decide if we need to make an http request here to get all the commands
            //TODO: if the http request fails then fallback to local JSON file?
            Task.Run(async () =>
            {
                try
                {
                    await SetPredictiveSuggestions();
                }
                catch (Exception e)
                {
                    _allPredictiveSuggestions = null;
                }

                if (_allPredictiveSuggestions == null)
                {
                    try
                    {
                        _commandsFileName = string.Format(CLIMicrosoft365PowerShellPredictorConstants.SuggestionsFileName,
                                CLIMicrosoft365PowerShellPredictorConstants.DefaultVersion);
                        await SetPredictiveSuggestions();
                        
                        if (showWarning)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write(CLIMicrosoft365PowerShellPredictorConstants.WarningMessageOnLoad);
                            Console.ResetColor();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write(CLIMicrosoft365PowerShellPredictorConstants.GenericErrorMessage);
                        Console.ResetColor();
                        _allPredictiveSuggestions = null;
                    }
                }
            });
        }

        public virtual List<PredictiveSuggestion>? GetSuggestions(PredictionContext context)
        {
            var input = context.InputAst.Extent.Text;
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (_allPredictiveSuggestions == null)
            {
                return null;
            }

            IEnumerable<Suggestion>? filteredSuggestions = _commandSearchMethod switch
            {
                CommandSearchMethod.StartsWith => _allPredictiveSuggestions
                    ?.Where(pc => pc.Command != null && pc.Command.ToLower().StartsWith(input.ToLower()))
                    .OrderBy(pc => pc.Rank),
                CommandSearchMethod.Contains => _allPredictiveSuggestions
                    ?.Where(pc => pc.Command != null && pc.Command.ToLower().Contains(input.ToLower()))
                    .OrderBy(pc => pc.Rank),
                _ => null
            };

            var result = filteredSuggestions?.Select(fs => new PredictiveSuggestion(fs.Command)).ToList();

            return result;
        }
    }
}
