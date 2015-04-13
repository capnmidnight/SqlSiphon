using System;
using System.Collections.Generic;
using System.Management;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace ssp
{
    class Options
    {
        [Option('l', "list", DefaultValue = false, HelpText = "List the active serial ports.")]
        public bool List { get; set; }
        [Option('p', "port", HelpText = "Port name to connect to.")]
        public string Port { get; set; }
        [Option('b', "baudrate", HelpText = "Baud rate at which to connect.")]
        public int BaudRate { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("sps - Simple Serial Port handler", "0.1"),
                Copyright = new CopyrightInfo("Sean T. McBeth", 2015),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            help.AddPreOptionsLine("");
            help.AddPreOptionsLine("Usage: sps (-l|-c COM5)");
            help.AddOptions(this);
            return help;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                if (options.List)
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM WIN32_SerialPort"))
                    {
                        foreach (var p in searcher.Get())
                        {
                            Console.WriteLine(
                                "{0} - {1}",
                                p["DeviceID"],
                                p["Description"]);
                        }
                    }
                }
                else if (options.Port != null)
                {
                    if (!SafeSerialPort.GetPortNames().Contains(options.Port))
                    {
                        Console.WriteLine("Port name '{0}' is not a serial port.", options.Port);
                    }
                    else
                    {
                        using (var port = new SafeSerialPort(options.Port, options.BaudRate))
                        {
                            port.Open();
                            using (var reader = new System.IO.StreamReader(port.BaseStream))
                            {
                                while (!reader.EndOfStream)
                                {
                                    Console.WriteLine(reader.ReadLine());
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine(options.GetUsage());
                }
            }
        }
    }
}
