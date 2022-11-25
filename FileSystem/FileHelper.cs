using System;
using System.IO;
using System.Windows.Forms;
using Text;
using CustomQuery;
using static Core.Constants;
using static FileSystemNS.Constants;

namespace FileSystemNS
{
    public static class FileHelper
    {
        private const string CREATE = nameof(CREATE);
        private const string HELP = nameof(HELP);
        private const string OPEN = nameof(OPEN);
        private const int PAD_COUNT = 10;
        public static readonly string _defaultDirectory = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + "\\Files\\";

        public static FileSystem Open()
        {
            FileSystem fileSystem = null;
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

                        string error = FileInfoToBytes(fileInfo, out long totalSize, out ushort sectorSize);

                        if (error is null)
                        {
                            fileSystem = FileSystem.Create(File.Create(_defaultDirectory + fileName), totalSize, sectorSize);
                            break;
                        }

                        Console.WriteLine(error);
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

                        if (File.Exists(fullPath))
                        {
                            fileSystem = FileSystem.Open(File.Open(fullPath, FileMode.Open));
                            break;
                        }

                        Console.WriteLine("File doesn't exist. Please enter a valid file name.");
                        break;

                    default:
                        Console.WriteLine("Unrecognized command. Type \"help\" for more info.");
                        break;
                }
                Console.WriteLine();
            }
            while (fileSystem is null);

            return fileSystem;
        }

        private static string PadCommand(string command) => command.PadRight_(PAD_COUNT);

        private static string FileInfoToBytes(string[] fileInfo, out long totalSize, out ushort sectorSize)
        {
            totalSize = 0;
            sectorSize = 0;
            char[] found = new char[fileInfo.Length - 1]; // fileInfo[0] is name
            int foundIndex = 0;

            for (int i = 1; i < fileInfo.Length; i++)
            {
                string str = fileInfo[i];
                char c = str[0];
                if (found.Contains_(c)) return $"/{c} appears more than once.";

                found[foundIndex++] = c;
                str = str.Split_(' ', 2)[1].TrimEnd_(' ');
                switch (c)
                {
                    case 'S':
                        if (!ushort.TryParse(str, out sectorSize))
                            return "Invalid sector size.";

                        break;

                    case 'T':
                        if (!long.TryParse(str.Substring_(0, str.Length - 2).TrimEnd_(' '), out totalSize))
                            return "Invalid total size.";

                        if (totalSize < 0)
                            return "Total size cannot be negative";

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
                                break;

                            default:
                                return unit + " is not recognized. Try KB | MB | GB instead.";
                        }
                        break;

                    default:
                        return $"/{c} is not recognized";
                }
            }

            return found.ContainsAll_('T', 'S') ? null : "Not all mandatory parameters are present.";
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
