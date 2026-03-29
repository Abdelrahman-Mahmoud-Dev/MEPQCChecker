namespace MEPQCChecker.Core.Models
{
    public enum FixOutcome
    {
        Applied,
        Skipped,
        Failed
    }

    public class FixResult
    {
        public string FixId { get; set; } = string.Empty;
        public FixOutcome Outcome { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
