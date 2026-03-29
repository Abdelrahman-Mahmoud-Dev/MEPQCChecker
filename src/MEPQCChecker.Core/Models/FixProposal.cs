using System;

namespace MEPQCChecker.Core.Models
{
    public enum FixActionType
    {
        MoveElement,
        ConnectElements,
        SetParameter,
        AdjustElevation,
        PlaceSprinklerHead
    }

    public enum FixConfidence
    {
        High,
        Medium,
        Low
    }

    public class FixProposal
    {
        public string FixId { get; set; } = Guid.NewGuid().ToString();
        public string IssueId { get; set; } = string.Empty;
        public FixActionType ActionType { get; set; }
        public FixConfidence Confidence { get; set; }
        public string Description { get; set; } = string.Empty;
        public long ElementId { get; set; }
        public long? ElementId2 { get; set; }
        public bool IsSelected { get; set; } = true;

        // MoveElement fields (metres)
        public double? MoveX { get; set; }
        public double? MoveY { get; set; }
        public double? MoveZ { get; set; }

        // SetParameter fields
        public string? ParameterName { get; set; }
        public string? ParameterValue { get; set; }

        // AdjustElevation fields
        public double? NewEndZ { get; set; }

        // ConnectElements fields
        public int? ConnectorIndex1 { get; set; }
        public int? ConnectorIndex2 { get; set; }

        // PlaceSprinklerHead fields
        public double? PlaceX { get; set; }
        public double? PlaceY { get; set; }
        public double? PlaceZ { get; set; }
        public string? FamilyTypeName { get; set; }
    }
}
