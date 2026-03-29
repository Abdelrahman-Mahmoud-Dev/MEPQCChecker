using System.Collections.Generic;

namespace MEPQCChecker.Core.Models
{
    public class RevitModelSnapshot
    {
        public string ProjectName { get; set; } = string.Empty;
        public string ModelPath { get; set; } = string.Empty;
        public string RevitVersion { get; set; } = string.Empty;
        public List<MEPElement> Elements { get; set; } = new List<MEPElement>();
        public List<RoomData> Rooms { get; set; } = new List<RoomData>();
        public List<LevelData> Levels { get; set; } = new List<LevelData>();
        public List<LinkedModelInfo> LinkedModels { get; set; } = new List<LinkedModelInfo>();
    }
}
