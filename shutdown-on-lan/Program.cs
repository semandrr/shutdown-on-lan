using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;
namespace shutdown_on_lan;
class Program
{
    static void Main()
    {
        // not supported, i never used these systems before
        if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Console.WriteLine("Operating system not supported");
            Environment.Exit(1);
        }

        // check config validity
        if (Config.macAddr.Length == 17)
        {
            if (!Config.macAddr.Contains(":"))
            {
                Console.WriteLine("Invalid MAC-address specified in Config.cs");
                Environment.Exit(1);
            }
        }
        else
        {
            if (Config.macAddr == "edit_me")
            {
                Console.WriteLine("You forgot to edit Config.cs :/");
                Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("Invalid MAC-address specified in Config.cs");
                Environment.Exit(1);
            }
        }
        if (Config.instantShutdown != 0 && Config.instantShutdown != 1)
        {
            Console.WriteLine("instantShutdown setting in Config.cs should be 1 or 0, please check it for any mistakes!");
            Environment.Exit(1);
        }
        if (Config.silent != 0 && Config.silent != 1)
        {
            Console.WriteLine("silent setting in Config.cs should be 1 or 0, please check it for any mistakes!");
            Environment.Exit(1);
        }

        using (UdpClient listener = new UdpClient(Config.wolPort))
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, Config.wolPort);
            Console.WriteLine($"[{DateTime.Now}] Listening for WoL packets on port {Config.wolPort}...");
            try
            {
                while (true)
                {
                    byte[] bytes = listener.Receive(ref ip);
                    if (IsMagicPacket(bytes))
                    {
                        string mac = GetMacFromPacket(bytes);
                        if (mac == Config.macAddr.ToUpper())
                        {
                            Console.WriteLine($"[{DateTime.Now}] Received WoL packet from {ip}, sending shutdown request.");
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                            {
                                if (Config.instantShutdown == 0)
                                {
                                    Process.Start("shutdown", "+1 \"Shutdown request on WoL\"");
                                }
                                else
                                {
                                    Process.Start("shutdown", "now");
                                }
                            }
                            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            {
                                if (Config.instantShutdown == 0)
                                {
                                    Process.Start("shutdown", "/s /t 60 /c \"Shutdown request on WoL\"");
                                }
                                else
                                {
                                    Process.Start("shutdown", "/s /t 0");
                                }
                            }
                            Environment.Exit(0);
                        }
                        else
                        {
                            if (Config.silent == 0)
                            {
                                Console.WriteLine($"[{DateTime.Now}] Ignoring WoL packet received on MAC {mac} from {ip}");
                            }
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine($"Socket error: {e.Message}");
            }
        }
    }

    static bool IsMagicPacket(byte[] packet)
    {
        if (packet.Length < 102) return false;
        for (int i = 0; i < 6; i++)
            if (packet[i] != 0xFF) return false;
        return true;
    }

    static string GetMacFromPacket(byte[] packet)
    {
        var macBytes = packet.Skip(6).Take(6);
        return string.Join(":", macBytes.Select(b => b.ToString("X2")));
    }
}
