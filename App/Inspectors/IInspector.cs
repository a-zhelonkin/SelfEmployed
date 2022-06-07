using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace SelfEmployed.App.Inspectors
{
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
}