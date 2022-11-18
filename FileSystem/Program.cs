using System;
using System.IO;
using System.Windows.Forms;
using Text;
using CustomQuery;
using static Core.Constants;
using static FileSystemNS.Constants;

namespace FileSystemNS
{
    internal static class Program
    {
        private const string CREATE = nameof(CREATE);
        private const string HELP = nameof(HELP);
        private const string OPEN = nameof(OPEN);
        private const int PAD_COUNT = 10;
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
                string[] commands = Console.ReadLine().TrimEnd_(' ').Split_(' ', 2);
                if (commands.Length == 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Please enter a command");
                    Console.WriteLine();
                    continue;
                }
                Console.WriteLine();

                switch (commands[0].ToUpperASCII_())
                {
                    case CREATE:
                        if (commands.Length < 2)
                        {
                            Console.WriteLine("Please specify a file name.");
                            break;
                        }

                        string[] fileInfo = commands[1].Split_('/');
                        string fileName = fileInfo[0].TrimEnd_(' ');
                        for (int i = 1; i < fileInfo.Length; i++)
                            fileInfo[i] = fileInfo[i].ToUpperASCII_();
                        if (fileName.ContainsAny_(FORBIDDEN_CHARS))
                        {
                            Console.WriteLine("A file name cannot contain any of the following symbolls: " + FORBIDDEN_CHARS);
                            break;
                        }

                        string error = FileInfoToBytes(fileInfo, out byte[] bootSectorBytes);
                        if (!(error is null))
                        {
                            Console.WriteLine(error);
                            break;
                        }

                        ulong sectorCount = bootSectorBytes.ToULong(TOTAL_SIZE_INDEX) / bootSectorBytes.ToUShort(SECTOR_SIZE_INDEX);
                        for (int y = 0; y < ULONG_BYTES; y++)
                            bootSectorBytes[SECTOR_COUNT_INDEX + y] = sectorCount.GetByte(y);

                        fileStream = File.Create(_defaultDirectory + fileName);
                        fileStream.Write(bootSectorBytes, 0, BOOT_SECTOR_SIZE);
                        fileStream.Position = 0;
                        fileStream.WriteByte((byte)BootByte.All); // Finally set first bit to true to signal correct file initializationl
                        break;

                    case HELP:
                        if (commands.Length == 1)
                        {
                            Console.WriteLine(PadCommand(CREATE) + "Creates a new file and opens it.");
                            Console.WriteLine(PadCommand(HELP) + "Shows commands' discription.");
                            Console.WriteLine(PadCommand(OPEN) + "Loads an existing file.");
                        }
                        else HelpCommand(commands[1]);
                        break;

                    case OPEN:
                        if (commands.Length < 2)
                        {
                            Console.WriteLine("Please specify a file name.");
                            break;
                        }

                        string fullPath = _defaultDirectory + commands[1].TrimEnd_(' ');
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

            FileSystem.Open(fileStream);
            Application.Run(new FormMain());
            Console.WriteLine(1);
        }

        private static string PadCommand(string command) => command.PadRight_(PAD_COUNT);

        private static string FileInfoToBytes(string[] fileInfo, out byte[] bytes)
        {
            bytes = new byte[BOOT_SECTOR_SIZE];
            char[] found = new char[fileInfo.Length - 1]; // fileInfo[0] is name
            string error = null;
            int foundIndex = 0;

            for (int i = 1; error is null && i < fileInfo.Length; i++)
            {
                string str = fileInfo[i];
                char c = str[0];
                if (found.Contains_(c))
                {
                    error = $"/{c} appears more than once.";
                    break;
                }
                found[foundIndex++] = c;
                str = str.Split_(' ', 2)[1].TrimEnd_(' ');
                switch (c)
                {
                    case 'S':
                        if (ushort.TryParse(str, out ushort sectorSize))
                        {
                            for (int y = 0; y < USHORT_BYTES; y++)
                                bytes[SECTOR_SIZE_INDEX + y] = sectorSize.GetByte(y);
                        }
                        else error = "Invalid sector size.";
                        break;

                    case 'T':
                        if (!long.TryParse(str.Substring_(0, str.Length - 2).TrimEnd_(' '), out long totalSize))
                        {
                            error = "Invalid total size.";
                            break;
                        }
                        if (totalSize < 0)
                        {
                            error = "Total size cannot be negative";
                            break;
                        }
                        string unit = str.Substring_(str.Length - 2, 2);
                        switch (unit)
                        {
                            case "GB":
                                totalSize *= 1024;
                                goto case "MB";

                            case "MB":
                                totalSize *= 1024;
                                goto case "KB";

                            case "KB":
                                totalSize *= 1024;
                                for (int y = 0; y < ULONG_BYTES; y++)
                                    bytes[TOTAL_SIZE_INDEX + y] = totalSize.GetByte(y);
                                break;

                            default:
                                error = nameof(unit) + " is not recognized. Try KB | MB | GB instead.";
                                break;
                        }
                        break;

                    default:
                        error = $"/{c} is not recognized";
                        break;
                }
            }

            if (!found.ContainsAll_('T', 'S')) error = "Not all mandatory parameters are present.";
            return error;
        }

        private static void HelpCommand(string command)
        {
            switch (command)
            {
                case CREATE:
                    Console.WriteLine(CREATE + " name </T size <KB | MB | GB>> </S>");
                    Console.WriteLine();
                    Console.WriteLine("/T size <KB | MB | GB>");
                    Console.WriteLine(new string(' ', PAD_COUNT) + "Total size of the file in KB | MB | GB.");
                    Console.WriteLine(PadCommand("/S") + "Size of a single sector in bytes.");
                    break;

                case HELP:
                    Console.WriteLine(HELP + " [command]");
                    Console.WriteLine();
                    Console.WriteLine(PadCommand("command") + "Show information about the command.");
                    break;

                case OPEN:
                    Console.WriteLine(OPEN + " <path>");
                    Console.WriteLine();
                    Console.WriteLine(PadCommand("path") + "Path to the file.");
                    break;

                default:
                    Console.WriteLine(command + " is not recognized. Type \"help\" for more info.");
                    break;
            }
        }
    }
}
