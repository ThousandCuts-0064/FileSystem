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
        private const string HELP = nameof(HELP);
        private const string HEX = nameof(HEX);
        private const string BIN = nameof(BIN);

        private const string MKDIR = nameof(MKDIR);
        private const string RMDIR = nameof(RMDIR);
        private const string LS = nameof(LS);
        private const string TREE = nameof(TREE);
        private const string CD = nameof(CD);

        private readonly FileSystem _fileSystem;
        private Directory _currDir;
        public static FormMain Get { get; private set; }

        public FormMain(FileSystem fileSystem)
        {
            InitializeComponent();
            _fileSystem = fileSystem;
            Get = this;
            _currDir = _fileSystem.RootDirectory;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Console.Write(_currDir.FullName + '>');
                    string[] commands = Console.ReadLine().Split_(' ', 2);
                    Console.WriteLine();

                    switch (commands[0].ToUpperASCII_())
                    {
                        case HELP:
                            break;

                        case HEX:
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

                        case BIN:
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

                        case MKDIR:
                            if (commands.Length == 1)
                            {
                                Console.WriteLine("Please specify a name.");
                                break;
                            }

                            if (_currDir.CreateSubdirectory(commands[1], out _) == FSResult.NameTaken)
                            {
                                Console.WriteLine("Name is already taken");
                                break;
                            }
                            continue;

                        case RMDIR:
                            if (commands.Length == 1)
                            {
                                Console.WriteLine("Please specify a name.");
                                break;
                            }

                            _currDir.TryRemoveSubdirectory(commands[1]);
                            continue;

                        case LS:
                            Console.WriteLine(_currDir.Name);
                            if (_currDir.SubDirectories.Count == 0) break;

                            for (int i = 0; i < _currDir.SubDirectories.Count - 1; i++)
                                Console.WriteLine('├' + _currDir.SubDirectories[i].Name);
                            Console.WriteLine('└' + _currDir.SubDirectories.Last_().Name);
                            break;

                        case TREE:
                            List_<char> chars = new List_<char>();
                            Console.WriteLine(_currDir.Name);
                            foreach (var item in _currDir.SubDirectories.SelectTree_(dir => dir.SubDirectories, (dir, depth) =>
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
                                            {
                                                chs[i * level + y] = ' ';
                                            }
                                        }
                                        int last = str.Length - 1;
                                        chs[last * level] = str[last];
                                        for (int y = 1; y < level; y++)
                                        {
                                            chs[last * level + y] = '─';
                                        }
                                        return new string(chs);
                                    }
                                }))
                                Console.WriteLine(item);
                            break;

                        case CD:
                            if (commands.Length == 1)
                            {
                                Console.WriteLine("Please specify a name.");
                                break;
                            }

                            int newDirIndex = _currDir.SubDirectories.IndexOf_(dir => dir.Name == commands[1]);
                            if (newDirIndex == -1)
                            {
                                Console.WriteLine($"\"{commands[1]}\" was not found.");
                                break;
                            }

                            _currDir = _currDir.SubDirectories[newDirIndex];
                            break;

                        default:
                            Console.WriteLine(commands[0] + " is not recognized. Type \"help\" for more info.");
                            break;
                    }
                    Console.WriteLine();
                }
            });
        }
    }
}
