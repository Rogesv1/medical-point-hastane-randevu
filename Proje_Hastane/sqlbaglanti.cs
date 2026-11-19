using System.Configuration;
using System.Data.SqlClient;

namespace Proje_Hastane
{
    // Veritabanı bağlantısını tek bir yerden üretir.
    // Bağlantı metni artık koda gömülü değil, App.config dosyasındaki "HastaneProje" kaydından okunuyor.
    //
    // ÖNEMLİ: Bu metot her çağrıldığında YENİ bir bağlantı açar. Bu yüzden formlarda şöyle kullanılır:
    //
    //     using (SqlConnection baglanti = bgl.baglanti())
    //     {
    //         ... sorgular ...
    //     }   // <- blok bitince bağlantı otomatik kapanır (hata çıksa bile)
    //
    // Eski koddaki "bgl.baglanti().Close()" satırı yeni bir bağlantı açıp onu kapatıyordu;
    // asıl kullanılan bağlantı açık kalıyordu.
    class sqlbaglanti
    {
        public SqlConnection baglanti()
        {
            string baglantiMetni = ConfigurationManager.ConnectionStrings["HastaneProje"].ConnectionString;
            SqlConnection baglan = new SqlConnection(baglantiMetni);
            baglan.Open();
            return baglan;
        }
    }
}
