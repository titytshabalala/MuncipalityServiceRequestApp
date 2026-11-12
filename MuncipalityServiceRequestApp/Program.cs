using System;
using System.Windows.Forms;

namespace MuncipalityServiceRequestApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Last line of defence: show a friendly message instead of crashing.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => ShowFatal(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => ShowFatal(e.ExceptionObject as Exception);

            try
            {
                AppDataStore.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Saved data unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            Application.ApplicationExit += (s, e) =>
            {
                try { AppDataStore.Save(); }
                catch (Exception ex) { ShowFatal(ex); }
            };

            Application.Run(new Form1());
        }

        private static void ShowFatal(Exception ex)
        {
            MessageBox.Show(
                "Something unexpected went wrong:\n\n" + (ex != null ? ex.Message : "Unknown error"),
                "Municipal Services Application",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}