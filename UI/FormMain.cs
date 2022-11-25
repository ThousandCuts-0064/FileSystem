using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileSystemNS;
using Text;

namespace UI
{
    public partial class FormMain : Form
    {
        private const string HELP = nameof(HELP);
        private const string HEXDUMP = nameof(HEXDUMP);
        private const string BINDUMP = nameof(BINDUMP);

        private const string MKDIR = nameof(MKDIR);
        private const string RMDIR = nameof(RMDIR);

        private readonly FileSystem _fileSystem;
        private Directory _currDir;
        public static FormMain Get { get; private set; }

        public FormMain(FileSystem fileSystem)
        {
            InitializeComponent();
            _fileSystem = fileSystem;
            Get = this;
            _currDir = _fileSystem.RootDir;
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

                        case HEXDUMP:
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

                        case BINDUMP:
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
                            _currDir.CreateSubdirectory(commands[1]);
                            break;

                        case RMDIR:
                            _currDir.CreateSubdirectory(commands[1]);
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
