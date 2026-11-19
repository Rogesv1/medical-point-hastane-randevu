using System;
using System.Threading;
using System.Windows.Forms;

namespace Proje_Hastane
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Beklenmeyen bir hata olursa (ör. veritabanına ulaşılamazsa) uygulama kapanıp gitmesin,
            // kullanıcıya anlaşılır bir mesaj gösterilsin.
            Application.ThreadException += BeklenmeyenHata;

            Application.Run(new FrmGirisler());
        }

        static void BeklenmeyenHata(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show("İşlem sırasında bir hata oluştu:\n\n" + e.Exception.Message,
                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
