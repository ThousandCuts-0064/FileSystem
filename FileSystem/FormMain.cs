using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileSystem
{
    public partial class FormMain : Form
    {
        private readonly FileStream _fileStream;

        public static FormMain Get { get; private set; }

        public FormMain(FileStream fileStream)
        {
            //if (!(Get is null)) throw new exc

            InitializeComponent();
            _fileStream = fileStream;
            Get = this;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (!Console.KeyAvailable) continue;

                    Console.ReadLine();
                    Console.WriteLine(1);
                }
            });
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            _fileStream.Close();
        }
    }
}
