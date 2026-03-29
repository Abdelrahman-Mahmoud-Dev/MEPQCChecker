namespace MEPQCChecker.Core.Models
{
    public class LinkedModelInfo
    {
        public string ModelId { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public string ModelPath { get; set; } = string.Empty;
        public bool IsIncluded { get; set; } = true;
        public int ElementCount { get; set; }
    }
}
