using System.Threading.Tasks;
using JetBrains.Annotations;

namespace SelfEmployed.Inspector;

public interface IInspector
{
    Task<(string Inn, InspectionStatus Status)> InspectAsync([NotNull] string inn, [NotNull] string date);
}

public enum InspectionStatus
{
    SelfEmployed,
    CommonPerson,
    PoorResponse,
}