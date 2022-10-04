using CLI.Microsoft365.PowerShell.Predictor.Abstractions.Interfaces;
using CLI.Microsoft365.PowerShell.Predictor.Services;
using System.Management.Automation.Subsystem.Prediction;
using CLI.Microsoft365.PowerShell.Predictor.Utilities;

namespace CLI.Microsoft365.PowerShell.Predictor
{
    internal sealed class CLIMicrosoft365PowerShellPredictor : ICommandPredictor, IDisposable
    {
        private readonly Guid _guid;

        /// <summary>
        /// Gets the unique identifier for a subsystem implementation.
        /// </summary>
        public Guid Id => _guid;

        /// <summary>
        /// Gets the name of a subsystem implementation.
        /// </summary>
        public string Name => "CLI for M365";

        /// <summary>
        /// Gets the description of a subsystem implementation.
        /// </summary>
        public string Description => "CLI for Microsoft 365 PowerShell predictor";
        
        private Settings _settings;

        private ICLIMicrosoft365PowerShellPredictorService _cliMicrosoft365PowerShellPredictorService;

        private PowerShellRuntime? _powerShellRuntime;

        private ICLIMicrosoft365PowerShellContext _cliMicrosoft365PowerShellContext;

        public CLIMicrosoft365PowerShellPredictor(string guid)
        {
            _guid = new Guid(guid);
            _powerShellRuntime = new PowerShellRuntime();
            Task.Run(() =>
            {
                _settings = Settings.GetSettings();
                _cliMicrosoft365PowerShellContext = new CLIMicrosoft365PowerShellContext(_powerShellRuntime);
                _cliMicrosoft365PowerShellContext.UpdateContext();
                _cliMicrosoft365PowerShellPredictorService = new CLIMicrosoft365PowerShellPredictorService(_cliMicrosoft365PowerShellContext, _settings);
            });

        }

        /// <summary>
        /// Get the predictive suggestions. It indicates the start of a suggestion rendering session.
        /// </summary>
        /// <param name="client">Represents the client that initiates the call.</param>
        /// <param name="context">The <see cref="PredictionContext"/> object to be used for prediction.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the prediction.</param>
        /// <returns>An instance of <see cref="SuggestionPackage"/>.</returns>
        public SuggestionPackage GetSuggestion(PredictionClient client, PredictionContext context, CancellationToken cancellationToken)
        {
            var result = _cliMicrosoft365PowerShellPredictorService.GetSuggestions(context);

            if (result is null || cancellationToken.IsCancellationRequested)
            {
                return default;
            }

            return new SuggestionPackage(result);
        }

        public void Dispose()
        {
            if (_powerShellRuntime != null)
            {
                _powerShellRuntime.Dispose();
                _powerShellRuntime = null;
            }
        }

        #region "interface methods for processing feedback"

        /// <summary>
        /// Gets a value indicating whether the predictor accepts a specific kind of feedback.
        /// </summary>
        /// <param name="client">Represents the client that initiates the call.</param>
        /// <param name="feedback">A specific type of feedback.</param>
        /// <returns>True or false, to indicate whether the specific feedback is accepted.</returns>
        public bool CanAcceptFeedback(PredictionClient client, PredictorFeedbackKind feedback) => false;

        /// <summary>
        /// One or more suggestions provided by the predictor were displayed to the user.
        /// </summary>
        /// <param name="client">Represents the client that initiates the call.</param>
        /// <param name="session">The mini-session where the displayed suggestions came from.</param>
        /// <param name="countOrIndex">
        /// When the value is greater than 0, it's the number of displayed suggestions from the list
        /// returned in <paramref name="session"/>, starting from the index 0. When the value is
        /// less than or equal to 0, it means a single suggestion from the list got displayed, and
        /// the index is the absolute value.
        /// </param>
        public void OnSuggestionDisplayed(PredictionClient client, uint session, int countOrIndex) { }

        /// <summary>
        /// The suggestion provided by the predictor was accepted.
        /// </summary>
        /// <param name="client">Represents the client that initiates the call.</param>
        /// <param name="session">Represents the mini-session where the accepted suggestion came from.</param>
        /// <param name="acceptedSuggestion">The accepted suggestion text.</param>
        public void OnSuggestionAccepted(PredictionClient client, uint session, string acceptedSuggestion) { }

        /// <summary>
        /// A command line was accepted to execute.
        /// The predictor can start processing early as needed with the latest history.
        /// </summary>
        /// <param name="client">Represents the client that initiates the call.</param>
        /// <param name="history">History command lines provided as references for prediction.</param>
        public void OnCommandLineAccepted(PredictionClient client, IReadOnlyList<string> history) { }

        /// <summary>
        /// A command line was done execution.
        /// </summary>
        /// <param name="client">Represents the client that initiates the call.</param>
        /// <param name="commandLine">The last accepted command line.</param>
        /// <param name="success">Shows whether the execution was successful.</param>
        public void OnCommandLineExecuted(PredictionClient client, string commandLine, bool success) { }

        #endregion;
    }
}
