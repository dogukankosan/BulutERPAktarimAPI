using System;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using BulutERPAktarim.Forms;

namespace BulutERPAktarim
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }
}