using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EchoServer;
public class Program
{
    static void Main(string[] args)
    {
        bool isUdp = args.Contains("-udp", StringComparer.OrdinalIgnoreCase);
        if (isUdp)
            RunUdpServer();
        else RunTcpServer();
    }

    public static void RunUdpServer()
    {
        const int port = 7;

        using UdpClient udpServer = new(port);
        Console.WriteLine($"UDP Echo server listening on : {port}");

        while (true)
        {
            IPEndPoint remoteEP = new(IPAddress.Any, 0);
            try
            {
                byte[] data = udpServer.Receive(ref  remoteEP);
                string message = Encoding.UTF8.GetString(data);
                Console.WriteLine($"UDP [{remoteEP}]: {message}");

                byte[] reply = Encoding.UTF8.GetBytes($"echo: {message}");
                udpServer.Send(reply, reply.Length, remoteEP);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Udp Error: {ex.Message}");
            }
        }
        
    }
    public static void RunTcpServer()
    {
        const int port = 7;
        IPAddress echoServerIP = IPAddress.Parse("127.0.0.1");

        TcpListener server = new(echoServerIP, port);
        server.Start();
        Console.WriteLine("Server waiting for connections...");


        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine($"Client with {client.Client.RemoteEndPoint} is connected");

            Task.Run(() => HandleClient(client));
        }
    }
    static void HandleClient(TcpClient client)
    {
        string? clientInfo = client.Client.RemoteEndPoint?.ToString();
        using (client)
        using (NetworkStream stream = client.GetStream())
        using (StreamReader reader = new(stream, Encoding.ASCII, false, 1024, leaveOpen: true))
        using (StreamWriter writer = new(stream, Encoding.ASCII, 1024, leaveOpen: true) { AutoFlush = true })
        {
            try
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    Console.WriteLine($"[{clientInfo}]: {line}");
                    writer.WriteLine($"Echo: {line}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with client {clientInfo}: {ex.Message}");
            }
        }
        Console.WriteLine("Client disconnected.");
    }
}

