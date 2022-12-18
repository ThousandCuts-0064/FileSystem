using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CustomCollections;
using CustomQuery;
using FileSystemNS;
using Text;

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

        private const string DIR = nameof(DIR);
        private const string CHDIR = nameof(CHDIR);
        private const string CAT = nameof(CAT);
        private const string WRITE = nameof(WRITE);
        private const string RENAME = nameof(RENAME);
        private const string COPY = nameof(COPY);
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
                    string[] commands = Console.ReadLine().Split_(' ', 2);
                    Console.WriteLine();

                    switch (commands[0].ToUpperASCII_())
                    {

                        case RELOAD:
                            Program.Reload = true;
                            goto case EXIT;

                        case EXIT:
                            if (!IsDisposed)
                                Invoke(new Action(Close));
                            return;

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
                            foreach (var item in directory.SubDirectories.SelectTree_(dir => dir.SubDirectories, (dir, depth) =>
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

                                bool IsFirst(Directory d) => d.Parent.SubDirectories[0] == d;
                                bool IsLast(Directory d) => d.Parent.SubDirectories.Last_() == d;
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

                            if (directory.SubDirectories.Count > 0)
                            {
                                Console.WriteLine("Directories:");
                                Console.WriteLine();
                                for (int i = 0; i < directory.SubDirectories.Count - 1; i++)
                                    Console.WriteLine(directory.SubDirectories[i].Name);
                                Console.WriteLine(directory.SubDirectories.Last_().Name);
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

                            file.Load();
                            if (file.Object is null)
                                continue;

                            switch (file.Format)
                            {
                                case FileFormat.None: throw new InvalidOperationException($"The {nameof(FileFormat)} should have been set.");

                                case FileFormat.Txt:
                                    Console.WriteLine(file.Object);
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

                            file.Load();
                            file.TrySetObject((string)file.Object + (args.Length == 2 ? args[1] : "") + '\n');
                            file.Save();

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

                            string[] args = commands[1].Split(' ');
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

                            if (_currDir.TryCreateDirectory(commands[1], out _, out string faultedName)
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

                            if (_currDir.TryCreateFile(commands[1], out _, out string faultedName)
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

                            break;
                        }

                        case EX:
                        case EXPORT:
                        {

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
            });
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            SetForegroundWindow(Handle); // Allows the method bellow to work more than 1 times. ¯\_(ツ)_/¯
            SetForegroundWindow(_consolePtr);
        }
    }
}
