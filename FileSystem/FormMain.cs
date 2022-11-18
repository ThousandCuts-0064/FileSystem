using System;
using System.Drawing;
//using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Text;

namespace FileSystemNS
{
    public partial class FormMain : Form
    {
        private const string HELP = nameof(HELP);
        private const string HEXDUMP = nameof(HEXDUMP);
        private const string BINDUMP = nameof(BINDUMP);

        public static FormMain Get { get; private set; }

        public FormMain()
        {
            InitializeComponent();
            Get = this;
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
                            Console.WriteLine(FileSystem.ToHex());
                            Console.WriteLine();
                            break;

                        case BINDUMP:
                            Console.WriteLine(FileSystem.ToBin());
                            Console.WriteLine();
                            break;

                        default:
                            Console.WriteLine(command + " is not recognized. Type \"help\" for more info.");
                            break;
                    }
                    Console.WriteLine();
                }
            });
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            FileSystem.Close();
        }
    }
}
