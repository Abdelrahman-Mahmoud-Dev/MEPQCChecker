using System.Collections.Generic;
using System.Linq;
using MEPQCChecker.Core.Models;

namespace MEPQCChecker.Core.Checks
{
    public class UnconnectedElementChecker : IQCCheck
    {
        private static readonly HashSet<string> TerminalCategories = new HashSet<string>
        {
            "OST_PlumbingFixtures",
            "OST_Sprinklers",
            "OST_AirTerminals",
            "OST_MechanicalEquipment"
        };

        public string CheckName => "UnconnectedElement";
        public string Discipline => "All";

        public IEnumerable<QCIssue> Run(RevitModelSnapshot snapshot)
        {
            foreach (var element in snapshot.Elements)
            {
                if (TerminalCategories.Contains(element.Category))
                    continue;

                if (element.Connectors == null || element.Connectors.Count == 0)
                    continue;

                var openConnectors = element.Connectors
                    .Where(c => !c.IsConnected && !c.IsEndCap)
                    .ToList();

                foreach (var connector in openConnectors)
                {
                    yield return new QCIssue
                    {
                        Severity = QCSeverity.Warning,
                        CheckType = CheckName,
                        Discipline = GetDiscipline(element.Category),
                        Description = $"{element.Category} (ID: {element.Id}) has an open connector at {connector.Description} on Level {element.Level}",
                        ElementId = element.Id,
                        ElementCategory = element.Category,
                        Level = element.Level
                    };
                }
            }
        }

        private static string GetDiscipline(string category)
        {
            return category switch
            {
                "OST_DuctCurves" or "OST_DuctFitting" or "OST_DuctAccessory" => "Mechanical",
                "OST_PipeCurves" or "OST_PipeFitting" or "OST_PipeAccessory" => "Plumbing",
                _ => "All"
            };
        }
    }
}
