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
using static FileSystemNS.Constants;

namespace UI
{
    public partial class FormMain : Form
    {
        private const string HELP = nameof(HELP);
        private const string HEX = nameof(HEX);
        private const string BIN = nameof(BIN);
        private const string TREE = nameof(TREE);

        private const string DIR = nameof(DIR);
        private const string CHDIR = nameof(CHDIR);
        private const string CAT = nameof(CAT);
        private const string RENAME = nameof(RENAME);
        private const string COPY = nameof(COPY);
        private const string MKDIR = nameof(MKDIR);
        private const string MKFILE = nameof(MKFILE);
        private const string RMDIR = nameof(RMDIR);
        private const string RMFILE = nameof(RMFILE);

        private const string LS = nameof(LS);
        private const string CD = nameof(CD);
        private const string GC = nameof(GC);
        private const string RN = nameof(RN);
        private const string CP = nameof(CP);
        private const string MD = nameof(MD);
        private const string MF = nameof(MF);
        private const string RD = nameof(RD);
        private const string RF = nameof(RF);

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
                            if (_currDir.SubDirectories.Count > 0)
                            {
                                for (int i = 0; i < _currDir.SubDirectories.Count - 1; i++)
                                    Console.WriteLine(_currDir.SubDirectories[i].Name);
                                Console.WriteLine(_currDir.SubDirectories.Last_().Name);
                                Console.WriteLine();
                            }

                            if (_currDir.Files.Count == 0)
                                continue;

                            for (int i = 0; i < _currDir.Files.Count - 1; i++)
                                Console.WriteLine(_currDir.Files[i].Name);
                            Console.WriteLine(_currDir.Files.Last_().Name);

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

                            if (commands[1] == CUR_DIR)
                                continue;

                            if (commands[1] == PAR_DIR)
                            {
                                if (_currDir.Parent is null)
                                {
                                    Console.WriteLine("You are already at root directory");
                                    break;
                                }

                                _currDir = _currDir.Parent;
                                continue;
                            }

                            var dirCD = _currDir.SubDirectories.FirstOrDefault_(dir => dir.Name == commands[1]);
                            if (dirCD is null)
                            {
                                Console.WriteLine($"\"{commands[1]}\" was not found.");
                                break;
                            }

                            _currDir = dirCD;
                            continue;
                        }

                        case GC:
                        case CAT:
                        {
                            break;
                        }

                        case RN:
                        case RENAME:
                        {
                            if (commands.Length == 1)
                            {
                                Console.WriteLine("Please specify an object.");
                                break;
                            }

                            string[] names = commands[1].Split(' ');
                            if (names.Length == 1)
                            {
                                Console.WriteLine("Please specify a new name.");
                                break;
                            }

                            FSResult resRen;

                            if (names[0] == CUR_DIR)
                                resRen = _currDir.TrySetName(names[1]);

                            else if (names[0] == PAR_DIR)
                            {
                                if (_currDir.Parent is null)
                                {
                                    Console.WriteLine("You are already at root directory");
                                    break;
                                }

                                resRen = _currDir.Parent.TrySetName(names[1]);
                            }
                            else
                            {
                                var objRen = _currDir.EnumerateObjects().FirstOrDefault_(dir => dir.Name == names[0]);
                                if (objRen is null)
                                {
                                    Console.WriteLine($"\"{names[0]}\" was not found.");
                                    break;
                                }

                                if (names[0] == names[1])
                                    continue;

                                resRen = objRen.TrySetName(names[1]);
                            }

                            if (resRen == FSResult.Success)
                                continue;

                            Console.WriteLine(resRen.ToMessage(names[1]));
                            break;
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

                            FSResult resMD = _currDir.TryCreateSubdirectory(commands[1], out _);
                            if (resMD != FSResult.Success)
                            {
                                Console.WriteLine(resMD.ToMessage(commands[1]));
                                break;
                            }
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

                            FSResult resMF = _currDir.TryCreateFile(commands[1], out _);
                            if (resMF != FSResult.Success)
                            {
                                Console.WriteLine(resMF.ToMessage(commands[1]));
                                break;
                            }
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

                            FSResult resRD;

                            if (commands[1] == CUR_DIR)
                            {
                                if (_currDir.Parent is null)
                                {
                                    Console.WriteLine("Cannot remove the root directory");
                                    break;
                                }

                                resRD = _currDir.Parent.TryRemoveSubdirectory(_currDir.Name);
                            }
                            else if (commands[1] == PAR_DIR)
                            {
                                if (_currDir.Parent is null)
                                {
                                    Console.WriteLine("The root directory has no parent");
                                    break;
                                }

                                if (_currDir.Parent.Parent is null)
                                {
                                    Console.WriteLine("Cannot remove the root directory");
                                    break;
                                }

                                resRD = _currDir.Parent.Parent.TryRemoveSubdirectory(_currDir.Parent.Name);
                            }
                            else
                                resRD = _currDir.TryRemoveSubdirectory(commands[1]);

                            if (resRD != FSResult.Success)
                            {
                                Console.WriteLine(resRD.ToMessage(commands[1]));
                                break;
                            }
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

                            FSResult resRF = _currDir.TryRemoveFile(commands[1]);

                            if (resRF != FSResult.Success)
                            {
                                Console.WriteLine(resRF.ToMessage(commands[1]));
                                break;
                            }
                            continue;
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
    }
}
