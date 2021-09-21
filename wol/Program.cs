using System;
using System.Collections.Generic;
using Mono.Options;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace wol
{
    class Program
    {

        static void Main(string[] args)
        {
            IPAddress ip = null;
            PhysicalAddress mac = null;
            int port = 7;
            int verbosity = 1;
            bool shouldShowHelp = false;

            string ipString = null;
            string macString = null;
            string portString = null;
            string verbosityString = null;

            var options = new OptionSet
            {
                { "a|address=", "the address to reach the device (can be IPv4, IPv6 or an hostname), default=255.255.255.255", a => ipString = a },
                { "m|mac=", "the mac address of the device used for creating the magic packet", m => macString = m },
                { "p|port=", "port to use to send the magic packet, default=7", p => portString = p},
                { "v|verbosity=", "sets the debug message verbosity, 0=silent, 1=normal, 2=debug, default=1", v =>
                {
                    if(v != null)
                        verbosityString = v;
                } },
                { "s|silent", "run silently without prompt except for errors, equivalent to verbosity level 0. This option overrides verbosity", s => {
                    if(s != null)
                        verbosity = 0;
                } },
                { "h|help", "show this message and exit", h =>  shouldShowHelp = h != null},
            };

            List<string> extra;

            try
            {
                extra = options.Parse(args);
            }
            catch (OptionException e)
            {
                Error(e.Message);
            }

            if (shouldShowHelp || args.Length == 0)
            {
                Help(options);
            }

            //set the verbosity level if needed
            if (!string.IsNullOrEmpty(verbosityString))
            {
                try
                {
                    verbosity = int.Parse(verbosityString);
                }
                catch (Exception e)
                {
                    Error(e.Message);
                }
            }

            //try parsing the address
            if (!string.IsNullOrEmpty(ipString))
            {
                try
                {
                    ip = IPAddress.Parse(ipString);

                    if (verbosity >= 2)
                    {
                        Console.WriteLine("ip address set to {0}", ip.ToString());
                    }
                }
                catch (FormatException)
                {
                    try
                    {

                        ip = Dns.GetHostEntry(ipString).AddressList[0];
                        bool check = ip is null;

                        if (verbosity >= 2)
                        {
                            Console.WriteLine("Hostname {0} resolved to ip {1}", ipString, ip.ToString());
                        }
                    }
                    catch (SocketException e)
                    {
                        Error(string.Format("{0}, {1}", ipString, e.Message));
                    }
                    catch (Exception e)
                    {
                        Error(e.Message);
                    }
                }
                catch (Exception e)
                {
                    Error(e.Message);
                }
            }
            else
            {
                ip = IPAddress.Broadcast;

                if (verbosity >= 2)
                {
                    Console.WriteLine("ip address set to broadcast ({0})", ip.ToString());
                }
            }

            //try parsing the physical address
            if (!string.IsNullOrEmpty(macString))
            {
                try
                {
                    mac = PhysicalAddress.Parse(macString);

                    if (verbosity >= 2)
                    {
                        Console.WriteLine("Physical address set to {0}", mac);
                    }
                }
                catch (Exception e)
                {
                    Error(e.Message);
                }
            }
            else
            {
                Error("No Physical Address provided");
            }

            if (!string.IsNullOrEmpty(portString))
            {
                try
                {
                    port = int.Parse(portString);

                    if (verbosity >= 2)
                    {
                        Console.WriteLine("Port set to {0}", port);
                    }
                }
                catch (Exception e)
                {
                    Error(e.Message);
                }
            }
            else
            {
                if (verbosity >= 2)
                {
                    Console.WriteLine("port set to default ({0})", port);
                }
            }

            try
            {
                SendMP(ip, mac, port);
            }
            catch (Exception e)
            {
                Error(e.Message);
            }

            if (verbosity >= 1)
            {
                Console.WriteLine("Magic packet sent successfully");
            }
            
        }

        static void Error(string e)
        {
            Console.WriteLine("wol: {0}", e);
            Console.WriteLine("Try \'wol --help\'");
            
            Environment.Exit(1);
        }
        static void Help(OptionSet options)
        {
            Console.WriteLine("Usage: wol.exe [OPTIONS]");
            Console.WriteLine("send a wake on lan command with a magic packet to the ip address provided");
            Console.WriteLine("Shows this help message if no options are provided");
            Console.WriteLine();

            Console.WriteLine("Options:");
            options.WriteOptionDescriptions(Console.Out);

            Environment.Exit(0);
        }

        static void SendMP(IPAddress ip, PhysicalAddress mac, int port)
        {

            byte[] magicPacket = new byte[102];

            for (int i = 0; i < 6; i++)
            {
                magicPacket[i] = 0xFF;
            }

            for (int i = 6; i < 102; i++)
            {
                magicPacket[i] = mac.GetAddressBytes()[i % 6];
            }

            UdpClient udp;

            if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                udp = new UdpClient(AddressFamily.InterNetworkV6);
            }
            else
            {
                udp = new UdpClient(AddressFamily.InterNetwork);
            }

            udp.Connect(ip, port);
            udp.Send(magicPacket, magicPacket.Length);
            udp.Close();
            

        }
    }
}
