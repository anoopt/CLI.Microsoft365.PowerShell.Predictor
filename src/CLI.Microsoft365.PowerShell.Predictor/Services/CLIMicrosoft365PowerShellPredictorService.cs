using CLI.Microsoft365.PowerShell.Predictor.Abstractions.Interfaces;
using CLI.Microsoft365.PowerShell.Predictor.Abstractions.Models;
using System.Management.Automation.Subsystem.Prediction;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using CLI.Microsoft365.PowerShell.Predictor.Utilities;

namespace CLI.Microsoft365.PowerShell.Predictor.Services
{
    internal sealed class CLIMicrosoft365PowerShellPredictorService : ICLIMicrosoft365PowerShellPredictorService
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
        
        private void RemoveInvalidSuggestions()
        {
            //if _allPredictiveSuggestions is null, then return
            if (_allPredictiveSuggestions == null)
            {
                return;
            }
            
            //filter out suggestions where CommandName and Command are not null or empty
            _allPredictiveSuggestions = _allPredictiveSuggestions.Where(suggestion => !string.IsNullOrEmpty(suggestion.CommandName) && !string.IsNullOrEmpty(suggestion.Command)).ToList();
        }

        private async Task SetPredictiveSuggestions()
        {
            string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string fileName = Path.Combine($"{executableLocation}{CLIMicrosoft365PowerShellPredictorConstants.SuggestionsFileRelativePath}", _commandsFileName);
            string jsonString = await File.ReadAllTextAsync(fileName);
            _allPredictiveSuggestions = JsonSerializer.Deserialize<List<Suggestion>>(jsonString)!;
            
            RemoveInvalidSuggestions();
        }

        private void RequestAllPredictiveCommands(bool showWarning)
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
        
        private IEnumerable<Suggestion>? GetFilteredSuggestions(string input)
        {
            IEnumerable<Suggestion>? filteredSuggestions = null;
            
            #region Search

            switch (_commandSearchMethod)
            {
                default:
                case CommandSearchMethod.Contains:
                    filteredSuggestions = _allPredictiveSuggestions
                        ?.Where(pc => pc.CommandName != null && pc.CommandName.ToLower().Contains(input.ToLower()))
                        .OrderBy(pc => pc.Rank);
                    break;
                
                case CommandSearchMethod.StartsWith:
                    filteredSuggestions = _allPredictiveSuggestions
                        ?.Where(pc => pc.CommandName != null && pc.CommandName.ToLower().StartsWith(input.ToLower()))
                        .OrderBy(pc => pc.Rank);
                    break;
                
                //TODO: Might need improvements
                case CommandSearchMethod.Fuzzy:
                {
                    var inputWithoutSpaces = Regex.Replace(input, @"\s+", "");
                    
                    var matches = new List<Suggestion>();

                    foreach (var suggestion in CollectionsMarshal.AsSpan(_allPredictiveSuggestions))
                    {
                        FuzzyMatcher.Match(suggestion.CommandName, inputWithoutSpaces, out var score);
                        suggestion.Rank = score;
                        matches.Add(suggestion);
                    }

                    filteredSuggestions = matches.OrderByDescending(m => m.Rank);
                    break;
                }   
            }

            #endregion
            
            return filteredSuggestions;
        }

        public List<PredictiveSuggestion>? GetSuggestions(PredictionContext context)
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

            IEnumerable<Suggestion>? filteredSuggestions = GetFilteredSuggestions(input);

            var result = filteredSuggestions?.Select(fs => new PredictiveSuggestion(fs.Command)).ToList();

            return result;
        }
    }
}
