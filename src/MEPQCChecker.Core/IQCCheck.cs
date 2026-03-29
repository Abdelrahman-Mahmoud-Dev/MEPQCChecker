using System.Collections.Generic;
using MEPQCChecker.Core.Models;

namespace MEPQCChecker.Core
{
    public interface IQCCheck
    {
        string CheckName { get; }
        string Discipline { get; }
        IEnumerable<QCIssue> Run(RevitModelSnapshot snapshot);
    }
}
