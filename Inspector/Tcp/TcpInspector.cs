using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SelfEmployed.Common.Extensions;

namespace SelfEmployed.Inspector.Tcp;

public sealed class TcpInspector : IInspector
{
    private const int Port = 443;
    private const string Host = "npd.nalog.ru";

    private const string Boundary = ".";
    private static readonly byte[] BoundaryBytes = Encoding.ASCII.GetBytes($"\r\n--{Boundary}\r\n");
    private static readonly byte[] TrailerBytes = Encoding.ASCII.GetBytes($"\r\n--{Boundary}--\r\n");
    private static readonly byte[] DefaultData;

    static TcpInspector()
    {
        using var stream = new MemoryStream();
        stream.WriteBytes(BoundaryBytes);
        stream.WriteUtf8("Content-Disposition:form-data;name=__EVENTTARGET\r\n\r\n");
        stream.WriteBytes(BoundaryBytes);
        stream.WriteUtf8("Content-Disposition:form-data;name=ctl00$ctl00$btSend\r\n\r\nНайти");
        DefaultData = stream.ToArray();
    }

    private readonly ConcurrentBag<Tunnel> _tunnels = new();

    public async Task<(string Inn, InspectionStatus Status)> InspectAsync(string inn, string date)
    {
        if (!_tunnels.TryTake(out var tunnel))
        {
            Console.WriteLine("New tunnel");
            tunnel = await Tunnel.CreateAsync();
        }

        var sslStream = tunnel.SslStream;

        try
        {
            using var stream = new MemoryStream();
            await stream.WriteBytesAsync(BoundaryBytes);
            await stream.WriteUtf8Async($"Content-Disposition:form-data;name=ctl00$ctl00$tbINN\r\n\r\n{inn}");
            await stream.WriteBytesAsync(BoundaryBytes);
            await stream.WriteUtf8Async($"Content-Disposition:form-data;name=ctl00$ctl00$tbDate\r\n\r\n{date}");
            await stream.WriteBytesAsync(TrailerBytes);

            await sslStream.WriteUtf8Async("POST /check-status/ HTTP/1.1\r\n");
            await sslStream.WriteUtf8Async($"Host: {Host}\r\n");
            await sslStream.WriteUtf8Async("Connection: Keep-Alive\r\n");
            await sslStream.WriteUtf8Async($"Content-Type: multipart/form-data;boundary={Boundary}\r\n");
            await sslStream.WriteUtf8Async($"Content-Length: {DefaultData.Length + stream.Length}\r\n\r\n");
            await sslStream.WriteBytesAsync(DefaultData);
            await sslStream.WriteBytesAsync(stream.ToArray());
            await sslStream.FlushAsync();
            // await Task.Delay(500);

            int bytes;
            var buffer = new byte[24576];
            var builder = new StringBuilder();
            do
            {
                bytes = await sslStream.ReadAsync(buffer, 0, buffer.Length);

                var decoder = Encoding.UTF8.GetDecoder();
                var chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);

                builder.Append(chars);
            } while (bytes == buffer.Length);

            var responseContent = builder.ToString();
            if (responseContent.Contains($"{inn} явля"))
                return (inn, InspectionStatus.SelfEmployed);

            if (responseContent.Contains($"{inn} не я"))
                return (inn, InspectionStatus.CommonPerson);
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync($"Error while requesting tcp: {e.Message}");
        }
        finally
        {
            _tunnels.Add(tunnel);
        }

        return (inn, InspectionStatus.PoorResponse);
    }

    private sealed class Tunnel : IDisposable
    {
        [NotNull] public TcpClient Client { get; }
        [NotNull] public SslStream SslStream { get; }

        private Tunnel(TcpClient client, SslStream sslStream)
        {
            Client = client;
            SslStream = sslStream;
        }

        [Pure]
        [ItemNotNull]
        public static async Task<Tunnel> CreateAsync()
        {
            var client = new TcpClient();
            await client.ConnectAsync(Host, Port);

            var networkStream = client.GetStream();
            // networkStream.ReadTimeout = 2000;

            var sslStream = new SslStream(networkStream);
            await sslStream.AuthenticateAsClientAsync(Host);

            return new Tunnel(client, sslStream);
        }

        public void Dispose()
        {
            Client.Dispose();
            SslStream.Dispose();
        }
    }
}