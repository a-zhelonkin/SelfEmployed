using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace SelfEmployed.App.Secretaries
{
    public interface ISecretary
    {
        IReadOnlyCollection<string> PoorResponseInns { get; }

        bool Handled([NotNull] string inn);
        Task CommitSelfEmployedAsync([NotNull] string inn);
        Task CommitCommonPersonAsync([NotNull] string inn);
        Task CommitPoorResponseAsync([NotNull] string inn);
    }
}