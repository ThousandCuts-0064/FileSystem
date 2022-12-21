using System;
using System.Windows.Forms;
using FileSystemNS;
using Text;

namespace UI
{
    static class Program
    {
        public static bool Reload { get; set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            do
            {
                Reload = false;
                FileSystem fileSystem = FileHelper.Open(args.Join_(" "));
                Console.Title = fileSystem.RootDirectory.Name;
                Application.Run(new FormMain(fileSystem));
                fileSystem.Close();
            } while (Reload);
        }
    }
}
