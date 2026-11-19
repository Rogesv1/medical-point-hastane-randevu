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
            // Veritabanı "metin sütuna sığmıyor" hatası (2628 / 8152): kutulardaki MaxLength bunu normalde
            // engeller; yine de olursa kullanıcıya İngilizce teknik mesaj yerine anlaşılır bir açıklama gösterilir.
            if (e.Exception is System.Data.SqlClient.SqlException sqlHatasi &&
                (sqlHatasi.Number == 2628 || sqlHatasi.Number == 8152))
            {
                MessageBox.Show("Girdiğiniz bilgilerden biri izin verilen karakter sayısından uzun. Lütfen kısaltıp tekrar deneyin.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Hatanın hangi formda ve hangi metotta çıktığını da gösteriyoruz; hata bildiren kişi
            // bu bilgiyi verirse sorunun yeri hemen bulunur.
            string yer = e.Exception.TargetSite != null
                ? e.Exception.TargetSite.DeclaringType.Name + "." + e.Exception.TargetSite.Name
                : "bilinmiyor";
            MessageBox.Show("İşlem sırasında bir hata oluştu:\n\n" + e.Exception.Message +
                "\n\nYer: " + yer,
                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
