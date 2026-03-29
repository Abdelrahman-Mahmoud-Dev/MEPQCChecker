using System;
using System.Collections.Generic;
using MEPQCChecker.Core.Checks;
using MEPQCChecker.Core.Models;

namespace MEPQCChecker.Core.Services
{
    public class CheckRunner
    {
        private readonly QCConfig _config;

        public CheckRunner() : this(ConfigService.GetDefaults()) { }

        public CheckRunner(QCConfig config)
        {
            _config = config;
        }

        public QCReport RunAll(RevitModelSnapshot snapshot)
        {
            var checks = new List<IQCCheck>
            {
                new ClashDetector(),
                new UnconnectedElementChecker(),
                new MissingParameterChecker(_config),
                new PipeSlopeChecker(_config),
                new SprinklerCoverageChecker(_config)
            };

            var report = new QCReport
            {
                ProjectName = snapshot.ProjectName,
                ModelPath = snapshot.ModelPath,
                RevitVersion = snapshot.RevitVersion,
                RunAt = DateTime.Now
            };

            foreach (var check in checks)
            {
                foreach (var issue in check.Run(snapshot))
                {
                    report.Issues.Add(issue);
                }
            }

            return report;
        }
    }
}
