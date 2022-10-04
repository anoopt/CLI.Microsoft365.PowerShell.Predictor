namespace CLI.Microsoft365.PowerShell.Predictor.Abstractions.Models
{
    internal class Suggestion
    {
        public string? Command { get; set; }
        public int Rank { get; set; }
    }
}
