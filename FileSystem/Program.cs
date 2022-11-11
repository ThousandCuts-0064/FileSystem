using System;
using System.IO;
using System.Windows.Forms;
using Text;

namespace FileSystem
{
    internal static class Program
    {
        private const string CREATE = nameof(CREATE);
        private const string HELP = nameof(HELP);
        private const string OPEN = nameof(OPEN);
        private const int COMMAND_COLUMN_WIDTH = 10;
        private static readonly string _defaultDirectory = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + "\\Files\\";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            FileStream fileStream = null;
            do
            {
                Console.WriteLine("Chose a file or type \"help\" for more info.");

                Console.WriteLine();
                string[] commands = Console.ReadLine().Split_(' ', 2);
                if (commands.Length == 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Please enter a command");
                    Console.WriteLine();
                    continue;
                }
                Console.WriteLine();


                switch (commands[0].ToUpperASCII())
                {
                    case CREATE:
                        if (commands.Length < 2)
                        {
                            Console.WriteLine("Please specify a file name.");
                            break;
                        }

                        fileStream = File.Create(_defaultDirectory + commands[1]);
                        break;

                    case HELP:
                        Console.Write(PadCommand(CREATE));
                        Console.WriteLine("Creates a new file and opens it.");
                        Console.Write(PadCommand(HELP));
                        Console.WriteLine("Shows commands' discription.");
                        Console.Write(PadCommand(OPEN));
                        Console.WriteLine("Loads an existing file.");
                        break;

                    case OPEN:
                        if (commands.Length < 2)
                        {
                            Console.WriteLine("Please specify a file name.");
                            break;
                        }

                        string fullPath = _defaultDirectory + commands[1];
                        if (!File.Exists(fullPath))
                        {
                            Console.WriteLine("File doesn't exist. Please enter a valid file name.");
                            break;
                        }

                        fileStream = File.Open(fullPath, FileMode.Open);
                        break;

                    default:
                        Console.WriteLine("Unrecognized command. Type \"help\" for more info.");
                        break;
                }
                Console.WriteLine();
            }
            while (fileStream is null);

            Application.Run(new FormMain(fileStream));
        }

        private static string PadCommand(string command) => command.PadRight_(COMMAND_COLUMN_WIDTH);
    }
}
