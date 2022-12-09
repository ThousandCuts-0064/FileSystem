using System;
using System.Windows.Forms;
using FileSystemNS;

using System.IO;
using CustomQuery;

namespace UI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            FileSystem fileSystem = FileHelper.Open(string.Join(" ", args));
            Application.Run(new FormMain(fileSystem));
            fileSystem.Close();
        }
    }
}
