using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SelfEmployed.App.Secretaries
{
    public sealed class FileSecretary : ISecretary
    {
        private const string SelfEmployedPath = "inns.self-employed.txt";
        private const string CommonPersonPath = "inns.common-person.txt";
        private const string PoorResponsePath = "inns.poor-response.txt";

        private readonly HashSet<string> _selfEmployedInns;
        private readonly HashSet<string> _commonPersonInns;
        private readonly HashSet<string> _poorResponseInns;

        private readonly StreamWriter _selfEmployedWriter;
        private readonly StreamWriter _commonPersonWriter;

        public FileSecretary()
        {
            _selfEmployedInns = new HashSet<string>(ReadLines(SelfEmployedPath));
            _commonPersonInns = new HashSet<string>(ReadLines(CommonPersonPath));
            _poorResponseInns = new HashSet<string>(ReadLines(PoorResponsePath));

            _selfEmployedWriter = AppendOrCreateText(SelfEmployedPath);
            _commonPersonWriter = AppendOrCreateText(CommonPersonPath);
        }

        public IReadOnlyCollection<string> PoorResponseInns => _poorResponseInns;

        public bool Handled(string inn) => _selfEmployedInns.Contains(inn) || _commonPersonInns.Contains(inn);

        public Task CommitSelfEmployedAsync(string inn)
        {
            _selfEmployedInns.Add(inn);
            _poorResponseInns.Remove(inn);

            return Task.WhenAll(
                _selfEmployedWriter.WriteLineAsync(inn),
                Console.Out.WriteLineAsync($"Self employed inn: {inn}")
            );
        }

        public Task CommitCommonPersonAsync(string inn)
        {
            _commonPersonInns.Add(inn);
            _poorResponseInns.Remove(inn);

            return Task.WhenAll(
                _commonPersonWriter.WriteLineAsync(inn),
                Console.Out.WriteLineAsync($"Common person inn: {inn}")
            );
        }

        public Task CommitPoorResponseAsync(string inn)
        {
            _poorResponseInns.Add(inn);

            return Console.Out.WriteLineAsync($"Poor response inn: {inn}");
        }

        private static IEnumerable<string> ReadLines([NotNull] string path) =>
            File.Exists(path)
                ? File.ReadLines(path)
                : Enumerable.Empty<string>();

        private static StreamWriter AppendOrCreateText([NotNull] string path)
        {
            var writer = File.Exists(path)
                ? File.AppendText(path)
                : File.CreateText(path);

            writer.AutoFlush = true;

            return writer;
        }
    }
}