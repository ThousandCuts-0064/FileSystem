using System;
using System.IO;
using Text;
using CustomQuery;
using static FileSystemNS.Constants;

namespace FileSystemNS
{
    public static class FileHelper
    {
        private const string CREATE = nameof(CREATE);
        private const string HELP = nameof(HELP);
        private const string OPEN = nameof(OPEN);
        private const int PAD_COUNT = 10;
        private static readonly string _defaultDirectory;

        public static string MainExternalDirectory { get; }

        static FileHelper()
        {
            MainExternalDirectory = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            _defaultDirectory = MainExternalDirectory + "\\Files\\";
        }

        public static FileSystem Open(string input)
        {
            System.IO.Directory.CreateDirectory(_defaultDirectory);

            FileSystem fileSystem = null;
            do
            {
                string[] commands;
                if (input.IsNullOrEmpty_())
                {
                    Console.WriteLine("Chose a file or type \"help\" for more info.");
                    Console.WriteLine();
                    string str = Console.ReadLine();
                    if (str == "1") str = "create f /t 10kb /s 512";
                    commands = str.TrimEnd_(' ').Split_(' ', 2);
                    Console.WriteLine();
                }
                else
                {
                    commands = input.TrimEnd_(' ').Split_(' ', 2);
                    input = "";
                }

                if (commands.Length == 0)
                {
                    Console.WriteLine("Please enter a command");
                    Console.WriteLine();
                    continue;
                }

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

                        if (fileName.Length > NAME_MAX_LENGTH)
                        {
                            Console.WriteLine($"File name exceeds the {NAME_MAX_LENGTH} char limit.");
                            break;
                        }

                        if (fileName.ContainsAny_(NAME_FORBIDDEN_CHARS))
                        {
                            Console.WriteLine("A file name cannot contain any of the following symbolls: " + NAME_FORBIDDEN_CHARS);
                            break;
                        }

                        string error = TryParseFileInfo(fileInfo, out long totalSize, out ushort sectorSize);

                        if (error is null)
                        {
                            fileSystem = FileSystem.Create(System.IO.File.Create(_defaultDirectory + fileName), fileName, totalSize, sectorSize);
                            continue;
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
                        else HelpCommand(commands[1].ToUpperASCII_());
                        break;

                    case OPEN:
                        if (commands.Length < 2)
                        {
                            Console.WriteLine("Please specify a file name.");
                            break;
                        }

                        string fullPath = _defaultDirectory + commands[1].TrimEnd_(' ');

                        if (System.IO.File.Exists(fullPath))
                        {
                            fileSystem = FileSystem.Open(System.IO.File.Open(fullPath, FileMode.Open));
                            continue;
                        }

                        Console.WriteLine("File doesn't exist. Please enter a valid file name.");
                        break;

                    default:
                        Console.WriteLine($"\"{commands[0]}\" is not recognized. Type \"help\" for more info.");
                        break;
                }
                Console.WriteLine();
            }
            while (fileSystem is null);

            return fileSystem;
        }

        private static string PadCommand(string command) => command.PadRight_(PAD_COUNT);

        private static string TryParseFileInfo(string[] fileInfo, out long totalSize, out ushort sectorSize)
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
                var args = str.Split_(' ', 2);
                if (args.Length == 1)
                    return $"{str.TrimEnd_(' ')} is invalid. Argument and value must be separated with ' '.";

                str = args[1].TrimEnd_(' ');
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

            return found.ContainsAll_('T', 'S') ? null : "Not all mandatory arguments are present.";
        }

        private static void HelpCommand(string command)
        {
            switch (command)
            {
                case CREATE:
                    Console.WriteLine(CREATE + " <name> </T size <KB | MB | GB>> </S size>");
                    Console.WriteLine();
                    Console.WriteLine(PadCommand("name") + "Name of the file");
                    Console.WriteLine("/T size <KB | MB | GB>");
                    Console.WriteLine(new string(' ', PAD_COUNT) + "Total size of the file in KB | MB | GB.");
                    Console.WriteLine(PadCommand("/S size") + "Size of a single sector in bytes.");
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
