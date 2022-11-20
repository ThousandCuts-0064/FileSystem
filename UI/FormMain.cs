using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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

        private readonly FileSystem _fileSystem;
        private string _currDir;
        public static FormMain Get { get; private set; }

        public FormMain(FileSystem fileSystem)
        {
            InitializeComponent();
            _fileSystem = fileSystem;
            Get = this;
            _currDir = fileSystem.RootDir.Name;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    string command = Console.ReadLine().ToUpperASCII_();
                    Console.WriteLine();

                    switch (command)
                    {
                        case HELP:
                            break;

                        case HEXDUMP:
                            Console.WriteLine(_fileSystem.GetHex());
                            break;

                        case BINDUMP:
                            Console.WriteLine(_fileSystem.GetBin());
                            break;

                        case MKDIR:

                            break;

                        default:
                            Console.WriteLine(command + " is not recognized. Type \"help\" for more info.");
                            break;
                    }
                    Console.WriteLine();
                }
            });
        }
    }
}
