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
        bool noMessage = false;
        if (Config.macAddr.Length == 17)
        {
            if (!Config.macAddr.Contains(':') && !Config.macAddr.Contains('-'))
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
        if (string.IsNullOrWhiteSpace(Config.shutdownMessage))
        {
            Console.WriteLine($"[{DateTime.Now}] WARNING: shutdownMessage in Config.cs is not specified/consists of only whitespace characters, not using it!");
            noMessage = true;
        }

        string formattedMac = Config.macAddr.ToUpper().Replace('-', ':');

        try
        {
            using (UdpClient listener = new UdpClient(Config.wolPort))
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, Config.wolPort);
                Console.WriteLine($"[{DateTime.Now}] Listening for WoL packets on port {Config.wolPort}...");
                while (true)
                {
                    byte[] bytes = listener.Receive(ref ip);
                    if (IsMagicPacket(bytes))
                    {
                        string mac = GetMacFromPacket(bytes);
                        if (mac == formattedMac)
                        {
                            Console.WriteLine($"[{DateTime.Now}] Received WoL packet from {ip}, sending shutdown request.");
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                            {
                                if (Config.instantShutdown == 0)
                                {
                                    if (noMessage == false)
                                    {
                                        Process.Start("shutdown", $"+1 \"{Config.shutdownMessage}\"");
                                    }
                                    else
                                    {
                                        Process.Start("shutdown", "+1");
                                    }
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
                                    if (noMessage == false)
                                    {
                                        Process.Start("shutdown", $"/s /t 60 /c \"{Config.shutdownMessage}\"");
                                    }
                                    else
                                    {
                                        Process.Start("shutdown", "/s /t 60");
                                    }
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
                                Console.WriteLine($"[{DateTime.Now}] Ignoring WoL packet sent to {mac} from {ip}");
                            }
                        }
                    }
                    else
                    {
                        if (Config.showInvalid == 1)
                        {
                            Console.WriteLine($"[{DateTime.Now}] Invalid UDP packet sent from {ip}");
                        }
                    }
                }
            }
        }
        catch (SocketException e)
        {
            if (e.SocketErrorCode == SocketError.AccessDenied)
            {
                Console.WriteLine($"Failed to bind to {Config.wolPort} port: are you root? (Permission denied)");
            }
            else if (e.SocketErrorCode == SocketError.AddressAlreadyInUse)
            {
                Console.WriteLine($"Failed to bind to {Config.wolPort} port: another program is already using it (Address already in use)");
            }
            else
            {
                Console.WriteLine($"Failed to bind to {Config.wolPort} port: {e.Message}");
                Console.WriteLine("Stack trace:");
                Console.WriteLine(e.StackTrace);
                Console.WriteLine();
                Console.WriteLine("It's highly likely that you caught a bug, please report it in repository issues with steps to reproduce.");
            }
            Environment.Exit(1);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Something seriously gone wrong (Fatal exception): {e.Message}");
            Console.WriteLine("Stack trace:");
            Console.WriteLine(e.StackTrace);
            Console.WriteLine();
            Console.WriteLine("It's highly likely that you caught a bug, please report it in repository issues with steps to reproduce.");
            Environment.Exit(1);
        }
    }

    static bool IsMagicPacket(byte[] packet)
    {
        if (packet.Length < 102)
        {
            return false;
        }
        for (int i = 0; i < 6; i++)
        {
            if (packet[i] != 0xFF)
            {
                return false;
            }
        }
        return true;
    }

    static string GetMacFromPacket(byte[] packet)
    {
        var macBytes = packet.Skip(6).Take(6);
        return string.Join(":", macBytes.Select(b => b.ToString("X2")));
    }
}
