using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace SelfEmployed.App.Secretaries;

public interface ISecretary
{
    IReadOnlyCollection<string> PoorResponseInns { get; }

    bool Handled([NotNull] string inn);
    Task CommitSelfEmployedAsync([NotNull] string inn);
    Task CommitCommonPersonAsync([NotNull] string inn);
    Task CommitPoorResponseAsync([NotNull] string inn);
}