﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Core;
using CustomCollections;
using CustomQuery;
using ExceptionsNS;
using FileSystemNS;
using Text;
using static FileSystemNS.Constants;

namespace UI
{
    public partial class FormMain : Form
    {
        #region Commands
        private const string RELOAD = nameof(RELOAD);
        private const string EXIT = nameof(EXIT);
        private const string HELP = nameof(HELP);
        private const string HEX = nameof(HEX);
        private const string BIN = nameof(BIN);
        private const string TREE = nameof(TREE);

        private const string R = nameof(R);
        private const string E = nameof(E);

        private const string DIR = nameof(DIR);
        private const string CHDIR = nameof(CHDIR);
        private const string CAT = nameof(CAT);
        private const string WRITE = nameof(WRITE);
        private const string RENAME = nameof(RENAME);
        private const string COPY = nameof(COPY);
        private const string CLEAR = nameof(CLEAR);
        private const string MKDIR = nameof(MKDIR);
        private const string MKFILE = nameof(MKFILE);
        private const string RMDIR = nameof(RMDIR);
        private const string RMFILE = nameof(RMFILE);
        private const string IMPORT = nameof(IMPORT);
        private const string EXPORT = nameof(EXPORT);

        private const string LS = nameof(LS);
        private const string CD = nameof(CD);
        private const string GC = nameof(GC);
        private const string WR = nameof(WR);
        private const string RN = nameof(RN);
        private const string CP = nameof(CP);
        private const string CL = nameof(CL);
        private const string MD = nameof(MD);
        private const string MF = nameof(MF);
        private const string RD = nameof(RD);
        private const string RF = nameof(RF);
        private const string IM = nameof(IM);
        private const string EX = nameof(EX);
        #endregion
        private readonly FileSystem _fileSystem;
        private readonly IntPtr _consolePtr;
        private Directory _currDir;
        public static FormMain Get { get; private set; }

        public FormMain(FileSystem fileSystem)
        {
            _consolePtr = GetConsoleWindow();
            InitializeComponent();
            //WindowState = FormWindowState.Minimized;
            _fileSystem = fileSystem;
            Get = this;
            _currDir = _fileSystem.RootDirectory;
        }

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern IntPtr GetConsoleWindow();

        private void FormMain_Load(object sender, EventArgs e)
        {
            Task console = Task.Run(() =>
            {
                while (true)
                {
                    Console.Write(_currDir.FullName + '>');

                    if (_fileSystem.IsBusy)
                    {
                        double progress = 0;
                        Console.CursorVisible = false;
                        int cursorLeft = Console.CursorLeft;
                        while (_fileSystem.IsBusy)
                        {
                            if (progress == _fileSystem.Progress.Value)
                                System.Threading.Thread.Sleep(1);

                            Console.Write($"{progress * 100}%");
                            Console.CursorLeft = cursorLeft;
                            progress = _fileSystem.Progress.Value;
                        }

                        Console.WriteLine("100%");
                        Console.WriteLine();
                        Console.CursorVisible = true;
                        continue;
                    }

                    string input = Console.ReadLine();
                    lock (_fileSystem)
                    {
                        string[] commands = input.Split_(' ', 2);
                        if (commands.Length == 0)
                            continue;
                        Console.WriteLine();

                        switch (commands[0].ToUpperASCII_())
                        {
                            case R:
                            case RELOAD:
                            {
                                Program.Reload = true;
                                goto case EXIT;
                            }

                            case E:
                            case EXIT:
                            {
                                if (!IsDisposed)
                                    Invoke(new Action(Close));
                                return;
                            }

                            case HELP:
                            {
                                break;
                            }

                            case HEX:
                            {
                                if (commands.Length == 1)
                                {
                                    Console.WriteLine(_fileSystem.GetHex());
                                    break;
                                }

                                if (!Enum.TryParse(commands[1], true, out FS fsHex))
                                {
                                    Console.WriteLine("Flags not recognized");
                                    break;
                                }

                                var listHex = _fileSystem.GetHex(fsHex);

                                Console.ForegroundColor = (ConsoleColor)1;
                                Console.Write(listHex[0]);
                                for (int i = 1; i < listHex.Count; i++)
                                {
                                    Console.Write(" ");
                                    Console.ForegroundColor = (ConsoleColor)(i % 16 + 1);
                                    Console.Write(listHex[i]);
                                }
                                Console.WriteLine();
                                Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                            }

                            case BIN:
                            {
                                if (commands.Length == 1)
                                {
                                    Console.WriteLine(_fileSystem.GetBin());
                                    break;
                                }

                                if (!Enum.TryParse(commands[1], true, out FS fsBin))
                                {
                                    Console.WriteLine("Flags not recognized");
                                    break;
                                }

                                var listBin = _fileSystem.GetBin(fsBin);

                                Console.ForegroundColor = (ConsoleColor)1;
                                Console.Write(listBin[0]);
                                for (int i = 1; i < listBin.Count; i++)
                                {
                                    Console.Write(" ");
                                    Console.ForegroundColor = (ConsoleColor)(i % 16 + 1);
                                    Console.Write(listBin[i]);
                                }
                                Console.WriteLine();
                                Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                            }

                            case TREE:
                            {
                                Directory directory = _currDir;

                                if (commands.Length > 1 &&
                                        _currDir.TryFindDirectory(commands[1], out directory, out string faultedName)
                                        .IsError(Console.WriteLine, faultedName))
                                    break;

                                List_<char> chars = new List_<char>();
                                Console.WriteLine(directory.Name);
                                foreach (var item in directory.Directories.SelectTree_(dir => dir.Directories, (dir, depth) =>
                                    {
                                        int excess = chars.Count - 1 - depth;
                                        if (excess > 0)
                                        {
                                            chars.RemoveLast(excess);
                                            chars[chars.Count - 1] = '├';
                                        }
                                        if (IsFirst(dir))
                                        {
                                            if (chars.Count > 0)
                                                chars[chars.Count - 1] = IsLast(dir.Parent) ? ' ' : '│';
                                            chars.Add(IsLast(dir) ? '└' : '├');
                                        }
                                        else if (IsLast(dir))
                                        {
                                            chars[chars.Count - 1] = '└';
                                        }

                                        return Indet(string.Concat(chars), 4) + dir.Name;

                                        bool IsFirst(Directory d) => d.Parent.Directories[0] == d;
                                        bool IsLast(Directory d) => d.Parent.Directories.Last_() == d;
                                        string Indet(string str, int level)
                                        {
                                            char[] chs = new char[str.Length * level];
                                            for (int i = 0; i < str.Length - 1; i++)
                                            {
                                                chs[i * level] = str[i];
                                                for (int y = 1; y < level; y++)
                                                    chs[i * level + y] = ' ';
                                            }
                                            int last = str.Length - 1;
                                            chs[last * level] = str[last];
                                            for (int y = 1; y < level; y++)
                                                chs[last * level + y] = '─';

                                            return new string(chs);
                                        }
                                    }))
                                    Console.WriteLine(item);
                                break;
                            }

                            case LS:
                            case DIR:
                            {
                                Directory directory = _currDir;

                                if (commands.Length > 1 &&
                                        _currDir.TryFindDirectory(commands[1], out directory, out string faultedName)
                                        .IsError(Console.WriteLine, faultedName))
                                    break;

                                if (directory.Directories.Count > 0)
                                {
                                    Console.WriteLine("Directories:");
                                    Console.WriteLine();
                                    for (int i = 0; i < directory.Directories.Count - 1; i++)
                                        Console.WriteLine(directory.Directories[i].Name);
                                    Console.WriteLine(directory.Directories.Last_().Name);
                                    Console.WriteLine();
                                }

                                if (directory.Files.Count == 0)
                                    continue;

                                Console.WriteLine("Files:");
                                Console.WriteLine();
                                for (int i = 0; i < directory.Files.Count - 1; i++)
                                    Console.WriteLine(directory.Files[i].Name);
                                Console.WriteLine(directory.Files.Last_().Name);

                                break;
                            }

                            case CD:
                            case CHDIR:
                            {
                                if (commands.Length == 1)
                                {
                                    Console.WriteLine("Please specify a name.");
                                    break;
                                }

                                if (_currDir.TryFindDirectory(commands[1], out Directory directory, out string faultedName)
                                        .IsError(Console.WriteLine, faultedName))
                                    break;

                                _currDir = directory;
                                continue;
                            }

                            case GC:
                            case CAT:
                            {
                                if (commands.Length == 1)
                                {
                                    Console.WriteLine("Please specify a name.");
                                    break;
                                }

                                if (_currDir.TryFindFile(commands[1], out File file, out string faultedName)
                                        .IsError(Console.WriteLine, faultedName))
                                    break;

                                file.TryLoad();
                                if (file.Object is null)
                                    continue;

                                switch (file.Format)
                                {
                                    case FileFormat.None: throw new InvalidOperationException($"The {nameof(FileFormat)} should have been set.");

                                    case FileFormat.Txt:
                                        Console.WriteLine(file.Object);
                                        break;

                                    case FileFormat.Rtf:
                                        Console.WriteLine(((RichTextBox)file.Object).Text);
                                        break;

                                    case FileFormat.Bmp: throw new NotImplementedException();

                                    case FileFormat.Wav: throw new NotImplementedException();

                                    default: throw new NotImplementedException();
                                }

                                break;
                            }

                            case WR:
                            case WRITE:
                            {
                                if (commands.Length == 1)
                                {
                                    Console.WriteLine("Please specify a text file.");
                                    break;
                                }

                                string[] args = commands[1].Split_(' ', 2);

                                if (_currDir.TryFindFile(args[0], out File file, out string faultedName)
                                        .IsError(Console.WriteLine, faultedName))
                                    break;

                                if (file.Format != FileFormat.Txt)
                                {
                                    Console.WriteLine("The file must have txt format.");
                                    break;
                                }

                                file.TryLoad();
                                file.TrySetObject((string)file.Object + (args.Length == 2 ? args[1] : "") + '\n');
                                file.TrySave();

                                continue;
                            }

                            case RN:
                            case RENAME:
                            {
                                if (commands.Length == 1)
                                {
                                    Console.WriteLine("Please specify an object.");
                                    break;
                                }

                                string[] args = commands[1].Split_(' ');
                                if (args.Length == 1)
                                {
                                    Console.WriteLine("Please specify a new name.");
                                    break;
                                }

                                if (_currDir.TryFindObject(args[0], out FileSystemNS.Object obj, out string faultedName)
                                        .IsError(Console.WriteLine, faultedName))
                                    break;

                                if (obj.TrySetName(args[1])
                                        .IsError(Console.WriteLine, args[1]))
                                    break;

                                continue;
                            }

                            case CP:
                            case COPY:
                            {
                                if (commands.Length == 1)
                                {
                                    Console.WriteLine("Please specify an object.");
                                    break;
                                }

                                string[] args = commands[1].Split_(' ', 3);
                                string dirTargetName = args.Length == 1 ? "." : args[1];

                                if (_currDir.TryFindFile(args[0], out File file, out string faultedFileName)
                                        .IsError(Console.WriteLine, faultedFileName))
                                    break;

                                if (_currDir.TryFindDirectory(dirTargetName, out Directory directory, out string faultedDirName)
                                        .IsError(Console.WriteLine, faultedDirName))
                                    break;

                                if (directory.TryCopyFile(file, args.Length == 3 ? args[2] : null)
                                        .IsError(Console.WriteLine))
                                    break;

                                continue;
                            }

                            case CL:
                            case CLEAR:
                            {
                                if (commands.Length == 1)
                                {
                                    Console.Clear();
                                    continue;
                                }

                                if (_currDir.TryFindObject(commands[1], out FileSystemNS.Object obj, out string faultedName)
                                        .IsError(Console.WriteLine, faultedName))
                                    break;

                                obj.Clear();

                                continue;
                            }

                            case MD:
                            case MKDIR:
                            {
                                if (commands.Length == 1)
                                {
                                    Console.WriteLine("Please specify a name.");
                                    break;
                                }

                                if (_currDir.TryCreateDirectory(commands[1].Split_(' ')[0], out _, out string faultedName)
                                        .IsError(Console.WriteLine, faultedName))
                                    break;

                                continue;
                            }

                            case MF:
                            case MKFILE:
                            {
                                if (commands.Length == 1)
                                {
                                    Console.WriteLine("Please specify a name.");
                                    break;
                                }

                                if (_currDir.TryCreateFile(commands[1].Split_(' ')[0], out _, out string faultedName)
                                        .IsError(Console.WriteLine, faultedName))
                                    break;

                                continue;
                            }

                            case RD:
                            case RMDIR:
                            {
                                if (commands.Length == 1)
                                {
                                    Console.WriteLine("Please specify a name.");
                                    break;
                                }

                                if (_currDir.TryRemoveDirectory(commands[1], out string faultedName)
                                        .IsError(Console.WriteLine, faultedName))
                                    break;

                                continue;
                            }

                            case RF:
                            case RMFILE:
                            {
                                if (commands.Length == 1)
                                {
                                    Console.WriteLine("Please specify a name.");
                                    break;
                                }

                                if (_currDir.TryRemoveFile(commands[1], out string faultedName)
                                        .IsError(Console.WriteLine, faultedName))
                                    break;

                                continue;
                            }

                            case IM:
                            case IMPORT:
                            {
                                if (commands.Length == 1)
                                {
                                    Console.WriteLine("Please enter arguments.");
                                    break;
                                }

                                string[] args = commands[1].Split(' ');
                                string externalFileName;

                                if (args[0].ContainsAny_('\\', ':'))
                                {
                                    externalFileName = args[0];
                                    goto FileNameChosen;
                                }

                                if (System.IO.Directory.Exists(FileHelper.MainExternalDirectory + "\\Import"))
                                {
                                    externalFileName = FileHelper.MainExternalDirectory + "\\Import\\" + args[0];
                                    goto FileNameChosen;
                                }

                                Console.WriteLine("External directory \"Import\" is not present. Create one or enter a full path.");
                                break;

                                FileNameChosen:
                                if (!System.IO.File.Exists(externalFileName))
                                {
                                    Console.WriteLine("External file not found.");
                                    break;
                                }

                                string extFileExtension = FileFormatExt.ResolveAliases(File.GetExtension(externalFileName).ToLowerASCII_());
                                string internalFileName = (args.Length == 1
                                            ? File.GetPureName(args[0])
                                            : args[1]) +
                                        '.' + extFileExtension;

                                if (_currDir.TryCreateFile(internalFileName, out File file, out string faultedName)
                                        .IsError(Console.WriteLine, faultedName))
                                    break;

                                object obj = null;
                                switch (extFileExtension)
                                {
                                    case "txt":
                                        obj = System.IO.File.ReadAllText(externalFileName);
                                        break;

                                    case "rtf":
                                        var temp = new RichTextBox();
                                        temp.LoadFile(externalFileName);
                                        obj = temp;
                                        break;

                                    case "bmp":
                                    case "emf":
                                    case "wmf":
                                    case "gif":
                                    case "jpeg":
                                    case "png":
                                    case "tiff":
                                    case "exif":
                                    case "icon":
                                        obj = Image.FromFile(externalFileName); // All supported formats of Image are used
                                        break;

                                    case "wav":
                                        obj = new System.Media.SoundPlayer(externalFileName);
                                        break;

                                    default: throw new UnreachableException("File format should have been checked on internal file creation.");
                                }

                                if (file.TrySetObject(obj)
                                        .IsError(Console.WriteLine))
                                    throw new UnreachableException("File format should have been the same.");
                                file.TrySave();

                                continue;
                            }

                            case EX:
                            case EXPORT:
                            {
                                if (commands.Length == 1)
                                {
                                    Console.WriteLine("Please enter arguments.");
                                    break;
                                }

                                string[] args = commands[1].Split(' ');

                                if (_currDir.TryFindFile(args[0], out File file, out string faultedName)
                                        .IsError(Console.WriteLine, faultedName))
                                    break;

                                string externalFileName = args.Length == 1
                                        ? args[0]
                                        : File.AttachExtension(args[1], File.GetExtension(args[0]));

                                if (!externalFileName.ContainsAny_('\\', ':'))
                                {
                                    System.IO.Directory.CreateDirectory(FileHelper.MainExternalDirectory + "\\Export");
                                    externalFileName = FileHelper.MainExternalDirectory + "\\Export\\" + externalFileName;
                                }

                                if (System.IO.File.Exists(externalFileName))
                                {
                                    Console.WriteLine("External file already exists.");
                                    break;
                                }

                                file.TryLoad();
                                switch (file.Object)
                                {
                                    case null:
                                        Console.WriteLine("Selected file was empty.");
                                        break;

                                    case string str:
                                        System.IO.File.WriteAllText(externalFileName, str);
                                        continue;

                                    case RichTextBox rtb:
                                        rtb.SaveFile(externalFileName);
                                        continue;

                                    case Image img:
                                        img.Save(externalFileName);
                                        continue;

                                    case System.Media.SoundPlayer sp:
                                        using (var stream = System.IO.File.OpenWrite(externalFileName))
                                            sp.Stream.CopyTo(stream);
                                        continue;
                                    default: throw new UnreachableException("File format should have been set correctly on file creation.");
                                }
                                break;
                            }

                            default:
                            {
                                Console.WriteLine(commands[0] + " is not recognized. Type \"help\" for more info.");
                                break;
                            }
                        }
                        Console.WriteLine();
                    }
                }
            });
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            SetForegroundWindow(Handle); // Allows the method bellow to work more than 1 times. ¯\_(ツ)_/¯
            SetForegroundWindow(_consolePtr);
        }
    }
}
