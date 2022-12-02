using System;
using System.Windows.Forms;
using FileSystemNS;

namespace UI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            FileSystem fileSystem = FileHelper.Open();
            Application.Run(new FormMain(fileSystem));
            fileSystem.Close();
        }
    }
}
