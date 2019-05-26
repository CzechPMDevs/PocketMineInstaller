using System;
using System.Linq;
using System.Net;
using System.IO.Compression;
using System.Diagnostics;

namespace PocketMineInstaller
{
    class Program
    {

        static string windowsPhpURL = "https://jenkins.pmmp.io/job/PHP-7.2-Aggregate/lastBuild/artifact/PHP-7.2-Windows-x64.zip";

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Out.WriteLine("-> PocketMine console installator by CzechPMDevs");
            Console.Out.WriteLine("-> Type 'help' to display installator commands.");
            Console.Out.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.Title = "PocketMine-MP installer by CzechPMDevs";

            string output = "";
            while(null != (output = Console.ReadLine())) {
                HandleCommand(output);
            }
        }

        static void HandleCommand(string command)
        {
            string[] args = command.Split(' ');
            int argsCount = args.Count();
            if(argsCount < 1) {
                Console.Out.WriteLine("Type 'help' to display PocketMine installer commands.");
                Console.Out.WriteLine(argsCount);
                return;
            }

            switch(args[0]) {
                case "help":
                    Console.Out.WriteLine("--- PocketMine Installator commands ---");
                    Console.Out.WriteLine("help : Displays help pages");
                    Console.Out.WriteLine("stop : Stops installer");
                    Console.Out.WriteLine("install : Installs PocketMine server");
                    Console.Out.WriteLine("start : Starts PocketMine server");
                    Console.Out.WriteLine("remove : Removes PocketMine server");
                    Console.Out.WriteLine("list : Displays list of available servers");
                    break;
                case "stop":
                    Console.Out.WriteLine("Installer stopped!");
                    Environment.Exit(0);
                    break;
                case "install":
                    if(argsCount < 2) {
                        Console.Out.WriteLine("Use 'install <serverName>'");
                        break;
                    }

                    string name = "";
                    string path = getDataPath() + (name = string.Join(" ", args.Skip(1).ToArray()));
                    Install(name, path);
                    break;
                case "start":
                    if (argsCount < 2)
                    {
                        Console.Out.WriteLine("Use 'start <serverName>'");
                        break;
                    }
                
                    Start(args[1]);
                    break;
                case "list":
                    string[] dirs = System.IO.Directory.GetDirectories(getDataPath(), "*", System.IO.SearchOption.TopDirectoryOnly);
                    string[] server = { };
                    foreach(string dir in dirs)
                    {
                        System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(dir);
                        if (info.Name != "bin")
                        {
                            server = server.Concat(new string[] { info.Name }).ToArray();

                        }
                    }

                    Console.Out.WriteLine("Available servers: " + string.Join(", ", server));

                    break;
                case "remove":
                    if (argsCount < 2)
                    {
                        Console.Out.WriteLine("Use 'start <serverName>'");
                        break;
                    }

                    Remove(args[1]);
                    break;
                default:
                    Console.Out.WriteLine("Type 'help' to display PocketMine installer commands.");
                    break;
            }
        }

        static void Remove(string name)
        {
            if (!System.IO.Directory.Exists(getDataPath() + name))
            {
                Console.Out.WriteLine("Server " + name + " was not found!");
                return;
            }

            try
            {
                System.IO.Directory.Delete(getDataPath() + name, true);

                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.Out.WriteLine("Server "+name+" removed!");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;

            }
            catch(Exception e)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.Out.WriteLine(e.Message);
                Console.BackgroundColor = ConsoleColor.Black;
            }

            
        }

        
        static void Start(string name)
        {
            if(!System.IO.Directory.Exists(getDataPath() + name))
            {
                Console.Out.WriteLine("Server " + name + " was not found!");
                return;
            }

            ProcessStartInfo cmd = new ProcessStartInfo();
            cmd.FileName = getDataPath() + name + "/start.cmd";

            Process process = new Process();
            process.StartInfo = cmd;
            process.Start();
            process.WaitForExit();

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.Out.WriteLine("Server '"+ name +"' started!");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;

        }

        static void Install(string name, string path)
        {
            if(System.IO.Directory.Exists(path)) {
                Console.Out.WriteLine("Server " + name + " is already installed!");
                return;
            }

            InitDirectories();
            InstallPhp();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Installing into directory '" + path + "'");
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.Out.WriteLine("Creating directory for the server...");
            System.IO.Directory.CreateDirectory(path);

            Console.Out.WriteLine("Downloading latest PocketMine-MP.phar from github...");
            WebClient webClient = new WebClient();
            webClient.DownloadFile("https://jenkins.pmmp.io/job/PocketMine-MP/lastSuccessfulBuild/artifact/PocketMine-MP.phar", path + "/PocketMine-MP.phar");
            Console.Out.WriteLine("PocketMine-MP downloaded!");

            Console.Out.WriteLine("Generating starting script...");
            System.IO.File.WriteAllText(path + "/start.cmd", "@echo off\r\n" +
                "CD " + path + "/\r\n" +
                "TITLE PocketMine-MP server software for Minecraft: Pocket Edition\r\n" +
                "REM pause on exitcode != 0 so the user can see what went wrong\r\n" +
                getDataPath() + "bin/php/php.exe " + path + "/PocketMine-MP.phar");
                
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.Out.WriteLine("Server " + name + " installed!");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        static void InitDirectories()
        {
            if (!System.IO.Directory.Exists(getDataPath())) {
                System.IO.Directory.CreateDirectory(getDataPath());
                Console.Out.WriteLine("Created directory '" + getDataPath() + "'!");
            }
        }

        static void InstallPhp()
        {
            if(System.IO.Directory.Exists(getDataPath() + "bin")) {
                return;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Out.WriteLine("PHP not found, installing php ...");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Out.WriteLine("Downloading php.zip ...");

            WebClient webClient = new WebClient();
            webClient.DownloadFile(windowsPhpURL, getDataPath() + "php.zip");
            Console.Out.WriteLine("PHP downloaded!");

            Console.Out.WriteLine("Extracting php...");
            ZipFile.ExtractToDirectory(getDataPath() + "php.zip", getDataPath());
            Console.Out.WriteLine("PHP extracted!");

            Console.Out.WriteLine("Removing unuseful files...");
            if(System.IO.File.Exists(getDataPath() + "vc_redist.x64.exe")) {
                System.IO.File.Delete(getDataPath() + "vc_redist.x64.exe");
            }
            if (System.IO.File.Exists(getDataPath() + "vc_redist.x86.exe")) {
                System.IO.File.Delete(getDataPath() + "vc_redist.x86.exe");
            }
            if (System.IO.File.Exists(getDataPath() + "php.zip")) {
                System.IO.File.Delete(getDataPath() + "php.zip");
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.Out.WriteLine("PocketMine php installed!");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        static string getDataPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/PocketMine/";
        }
    }
}
