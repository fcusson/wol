using System;
using System.Collections.Generic;
using Mono.Options;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;

namespace wol
{
    class Program
    {

        //version control variable
        const string VERSION = "1.0.1";

        /// <summary>
        /// entry point of the program
        /// </summary>
        /// <param name="args">list of argument provided at runtime</param>
        static void Main(string[] args)
        {
            IPAddress ip = null;
            PhysicalAddress mac = null;
            int port = 7;
            int debug = 1;
            bool shouldShowHelp = false;
            bool shouldShowVersion = false;

            string ipString = null;
            string macString = null;
            string portString = null;
            string debugString = null;

            var options = new OptionSet
            {
                { "a|address=", "the address to reach the device (can be IPv4, IPv6 or an hostname), default=255.255.255.255", a => ipString = a },
                { "m|mac=", "the mac address of the device used for creating the magic packet", m => macString = m },
                { "p|port=", "port to use to send the magic packet, default=7", p => portString = p},
                { "d|debug-level=", "sets the debug message level, 0=silent, 1=normal, 2=debug, default=1", d =>
                {
                    if( d != null)
                        debugString = d;
                } },
                { "s|silent", "run silently without prompt except for errors, equivalent to debug level 0. This option overrides debug", s => {
                    if(s != null)
                        debug = 0;
                } },
                { "h|help", "show this message and exit", h =>  shouldShowHelp = h != null},
                { "v|version", "show the version information", v => shouldShowVersion = v != null},
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

            if (shouldShowVersion)
            {
                Version();
            }

            if (shouldShowHelp || args.Length == 0)
            {
                Help(options);
            }

            //set the debug level if needed
            if (!string.IsNullOrEmpty(debugString))
            {
                try
                {
                    debug = int.Parse(debugString);
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

                    if (debug >= 2)
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

                        if (debug >= 2)
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

                if (debug >= 2)
                {
                    Console.WriteLine("IP address set to broadcast ({0})", ip.ToString());
                }
            }

            //try parsing the physical address
            if (!string.IsNullOrEmpty(macString))
            {
                try
                {
                    mac = PhysicalAddress.Parse(macString);

                    if (debug >= 2)
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

                    if (debug >= 2)
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
                if (debug >= 2)
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

            if (debug >= 1)
            {
                Console.WriteLine("Magic packet sent successfully");
            }
            
        }

        /// <summary>
        /// error management methods, prints the error message e
        /// </summary>
        /// <param name="e">error message to print</param>
        static void Error(string e)
        {
            Console.WriteLine("wol: {0}", e);
            Console.WriteLine("Try \'wol --help\'");
            
            Environment.Exit(1);
        }

        /// <summary>
        /// prints the help menu and quits the program
        /// </summary>
        /// <param name="options">list of possible options</param>
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

        /// <summary>
        /// sends a magic packet built with the physical address ma to [ip]:[port]
        /// </summary>
        /// <param name="ip">internet protocol address</param>
        /// <param name="mac">physical address</param>
        /// <param name="port">network port to use</param>
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

        /// <summary>
        /// prints the version information
        /// </summary>
        static void Version()
        {
            Console.WriteLine("wol - {0}", VERSION);
            Console.WriteLine("Copyright(C) 2021 Felix Cusson");
            Console.WriteLine("MIT License: <https://github.com/Darkfull-Dante/wol/blob/master/LICENSE>");
            Console.WriteLine("Thisi is free software: you are free to change and redistribute it.");
            Console.WriteLine("There is NO WARANTY, to the extent permitted by law.");

            Environment.Exit(0);
        }
    }
}
