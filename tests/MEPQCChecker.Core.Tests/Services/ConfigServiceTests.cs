using System.IO;
using System.Linq;
using MEPQCChecker.Core.Services;

namespace MEPQCChecker.Core.Tests.Services
{
    public class ConfigServiceTests
    {
        [Fact]
        public void LoadFromJson_ValidJson_ReturnsConfig()
        {
            var json = File.ReadAllText(
                Path.Combine(FindProjectRoot(), "src", "MEPQCChecker.Core", "config.json"));

            var config = ConfigService.LoadFromJson(json);

            Assert.NotNull(config);
            Assert.True(config.RequiredParameters.Count > 0);
            Assert.Contains("OST_DuctCurves", config.RequiredParameters.Keys);
            Assert.Contains("OST_Sprinklers", config.RequiredParameters.Keys);
        }

        [Fact]
        public void LoadFromJson_ParsesPipeSlopeConfig()
        {
            var json = File.ReadAllText(
                Path.Combine(FindProjectRoot(), "src", "MEPQCChecker.Core", "config.json"));

            var config = ConfigService.LoadFromJson(json);

            Assert.Equal(1.0, config.PipeSlope.MinSlopePctLargePipe);
            Assert.Equal(2.0, config.PipeSlope.MinSlopePctSmallPipe);
            Assert.Equal(15.0, config.PipeSlope.MaxSlopePct);
            Assert.Equal(50, config.PipeSlope.SmallPipeThresholdMM);
        }

        [Fact]
        public void LoadFromJson_ParsesSprinklerCoverageConfig()
        {
            var json = File.ReadAllText(
                Path.Combine(FindProjectRoot(), "src", "MEPQCChecker.Core", "config.json"));

            var config = ConfigService.LoadFromJson(json);

            Assert.Equal(2.25, config.SprinklerCoverage.DefaultCoverageRadiusM);
            Assert.Equal(5.0, config.SprinklerCoverage.CriticalUncoveredPct);
            Assert.Equal(0.5, config.SprinklerCoverage.GridSamplingResolutionM);
        }

        [Fact]
        public void LoadFromJson_ParsesGravityDrainedSystems()
        {
            var json = File.ReadAllText(
                Path.Combine(FindProjectRoot(), "src", "MEPQCChecker.Core", "config.json"));

            var config = ConfigService.LoadFromJson(json);

            Assert.Contains("Sanitary", config.GravityDrainedSystemNames);
            Assert.Contains("Vent", config.GravityDrainedSystemNames);
            Assert.Contains("Storm Drain", config.GravityDrainedSystemNames);
            Assert.Equal(4, config.GravityDrainedSystemNames.Count);
        }

        [Fact]
        public void GetDefaults_ReturnsNonEmptyConfig()
        {
            var config = ConfigService.GetDefaults();

            Assert.True(config.RequiredParameters.Count >= 7);
            Assert.True(config.GravityDrainedSystemNames.Count >= 4);
            Assert.True(config.PipeSlope.MinSlopePctLargePipe > 0);
            Assert.True(config.SprinklerCoverage.DefaultCoverageRadiusM > 0);
        }

        [Fact]
        public void LoadFromJson_EmptyObject_ReturnsFallbackDefaults()
        {
            var config = ConfigService.LoadFromJson("{}");

            Assert.NotNull(config);
            // Empty config should have default empty collections
            Assert.Empty(config.RequiredParameters);
            // But PipeSlope/SprinklerCoverage defaults are from the class initializer
            Assert.Equal(1.0, config.PipeSlope.MinSlopePctLargePipe);
        }

        private static string FindProjectRoot()
        {
            var dir = Directory.GetCurrentDirectory();
            while (dir != null && !File.Exists(Path.Combine(dir, "MEPQCChecker.sln")))
                dir = Directory.GetParent(dir)?.FullName;
            return dir ?? Directory.GetCurrentDirectory();
        }
    }
}
