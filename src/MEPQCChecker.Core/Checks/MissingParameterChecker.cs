using System;
using System.Collections.Generic;
using MEPQCChecker.Core.Models;
using MEPQCChecker.Core.Services;

namespace MEPQCChecker.Core.Checks
{
    public class MissingParameterChecker : IQCCheck
    {
        private readonly QCConfig _config;

        public string CheckName => "MissingParameter";
        public string Discipline => "All";

        public MissingParameterChecker(QCConfig config)
        {
            _config = config;
        }

        public IEnumerable<QCIssue> Run(RevitModelSnapshot snapshot)
        {
            foreach (var element in snapshot.Elements)
            {
                if (!_config.RequiredParameters.TryGetValue(element.Category, out var requiredParams))
                    continue;

                foreach (var param in requiredParams)
                {
                    if (IsMissing(element, param.Name))
                    {
                        yield return new QCIssue
                        {
                            Severity = ParseSeverity(param.Severity),
                            CheckType = CheckName,
                            Discipline = GetDiscipline(element.Category),
                            Description = $"{element.Category} (ID: {element.Id}) is missing required parameter: {param.Name}",
                            ElementId = element.Id,
                            ElementCategory = element.Category,
                            Level = element.Level,
                            ParameterName = param.Name
                        };
                    }
                }
            }
        }

        private static bool IsMissing(MEPElement element, string parameterName)
        {
            if (!element.Parameters.TryGetValue(parameterName, out var value))
                return true;

            if (string.IsNullOrEmpty(value))
                return true;

            if (string.Equals(value, "<none>", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        private static QCSeverity ParseSeverity(string severity)
        {
            return severity switch
            {
                "Critical" => QCSeverity.Critical,
                "Warning" => QCSeverity.Warning,
                "Info" => QCSeverity.Info,
                _ => QCSeverity.Warning
            };
        }

        private static string GetDiscipline(string category)
        {
            return category switch
            {
                "OST_DuctCurves" or "OST_DuctFitting" or "OST_MechanicalEquipment" => "Mechanical",
                "OST_PipeCurves" or "OST_PipeFitting" or "OST_PlumbingFixtures" => "Plumbing",
                "OST_Sprinklers" => "FireProtection",
                _ => "All"
            };
        }
    }
}
