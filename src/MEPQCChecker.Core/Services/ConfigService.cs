using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MEPQCChecker.Core.Services
{
    public class QCConfig
    {
        [JsonPropertyName("RequiredParameters")]
        public Dictionary<string, List<RequiredParameterEntry>> RequiredParameters { get; set; }
            = new Dictionary<string, List<RequiredParameterEntry>>();

        [JsonPropertyName("PipeSlope")]
        public PipeSlopeConfig PipeSlope { get; set; } = new PipeSlopeConfig();

        [JsonPropertyName("SprinklerCoverage")]
        public SprinklerCoverageConfig SprinklerCoverage { get; set; } = new SprinklerCoverageConfig();

        [JsonPropertyName("GravityDrainedSystemNames")]
        public List<string> GravityDrainedSystemNames { get; set; } = new List<string>();
    }

    public class RequiredParameterEntry
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("Severity")]
        public string Severity { get; set; } = "Warning";
    }

    public class PipeSlopeConfig
    {
        [JsonPropertyName("MinSlopePctLargePipe")]
        public double MinSlopePctLargePipe { get; set; } = 1.0;

        [JsonPropertyName("MinSlopePctSmallPipe")]
        public double MinSlopePctSmallPipe { get; set; } = 2.0;

        [JsonPropertyName("MaxSlopePct")]
        public double MaxSlopePct { get; set; } = 15.0;

        [JsonPropertyName("SmallPipeThresholdMM")]
        public double SmallPipeThresholdMM { get; set; } = 50;
    }

    public class SprinklerCoverageConfig
    {
        [JsonPropertyName("DefaultCoverageRadiusM")]
        public double DefaultCoverageRadiusM { get; set; } = 2.25;

        [JsonPropertyName("CriticalUncoveredPct")]
        public double CriticalUncoveredPct { get; set; } = 5.0;

        [JsonPropertyName("WarningUncoveredPct")]
        public double WarningUncoveredPct { get; set; } = 1.0;

        [JsonPropertyName("GridSamplingResolutionM")]
        public double GridSamplingResolutionM { get; set; } = 0.5;
    }

    public static class ConfigService
    {
        public static QCConfig Load()
        {
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (assemblyDir == null)
                return GetDefaults();

            var configPath = Path.Combine(assemblyDir, "config.json");
            if (!File.Exists(configPath))
                return GetDefaults();

            var json = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<QCConfig>(json) ?? GetDefaults();
        }

        public static QCConfig LoadFromJson(string json)
        {
            return JsonSerializer.Deserialize<QCConfig>(json) ?? GetDefaults();
        }

        public static QCConfig GetDefaults()
        {
            return new QCConfig
            {
                RequiredParameters = new Dictionary<string, List<RequiredParameterEntry>>
                {
                    ["OST_DuctCurves"] = new List<RequiredParameterEntry>
                    {
                        new RequiredParameterEntry { Name = "System Name", Severity = "Warning" },
                        new RequiredParameterEntry { Name = "System Classification", Severity = "Warning" },
                        new RequiredParameterEntry { Name = "Insulation Type", Severity = "Info" },
                        new RequiredParameterEntry { Name = "Flow", Severity = "Warning" }
                    },
                    ["OST_DuctFitting"] = new List<RequiredParameterEntry>
                    {
                        new RequiredParameterEntry { Name = "System Name", Severity = "Info" }
                    },
                    ["OST_PipeCurves"] = new List<RequiredParameterEntry>
                    {
                        new RequiredParameterEntry { Name = "System Name", Severity = "Warning" },
                        new RequiredParameterEntry { Name = "System Classification", Severity = "Warning" },
                        new RequiredParameterEntry { Name = "Pipe Material", Severity = "Warning" },
                        new RequiredParameterEntry { Name = "Outside Diameter", Severity = "Warning" },
                        new RequiredParameterEntry { Name = "Insulation Type", Severity = "Info" }
                    },
                    ["OST_PipeFitting"] = new List<RequiredParameterEntry>
                    {
                        new RequiredParameterEntry { Name = "System Name", Severity = "Info" }
                    },
                    ["OST_Sprinklers"] = new List<RequiredParameterEntry>
                    {
                        new RequiredParameterEntry { Name = "System Name", Severity = "Critical" },
                        new RequiredParameterEntry { Name = "Head Type", Severity = "Critical" },
                        new RequiredParameterEntry { Name = "Coverage Radius", Severity = "Warning" },
                        new RequiredParameterEntry { Name = "Flow", Severity = "Warning" }
                    },
                    ["OST_MechanicalEquipment"] = new List<RequiredParameterEntry>
                    {
                        new RequiredParameterEntry { Name = "System Name", Severity = "Warning" },
                        new RequiredParameterEntry { Name = "Manufacturer", Severity = "Warning" },
                        new RequiredParameterEntry { Name = "Model", Severity = "Warning" }
                    },
                    ["OST_PlumbingFixtures"] = new List<RequiredParameterEntry>
                    {
                        new RequiredParameterEntry { Name = "System Name", Severity = "Warning" },
                        new RequiredParameterEntry { Name = "Fixture Type", Severity = "Warning" }
                    }
                },
                PipeSlope = new PipeSlopeConfig(),
                SprinklerCoverage = new SprinklerCoverageConfig(),
                GravityDrainedSystemNames = new List<string>
                {
                    "Sanitary", "Vent", "Storm Drain", "Domestic Cold Water Return"
                }
            };
        }
    }
}
