using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Proje_Hastane
{
    public partial class FrmHastaKayit : Form
    {
        public FrmHastaKayit()
        {
            InitializeComponent();
        }

        sqlbaglanti bgl = new sqlbaglanti();

        private void btnKayit_Click(object sender, EventArgs e)
        {
            // Eskiden boş form da kaydediliyordu; cinsiyet seçilmezse program çöküyordu
            // (SelectedItem null iken .ToString() çağrılıyordu).
            if (txtAd.Text.Trim() == "" || txtSoyad.Text.Trim() == "" || txtSifre.Text == ""
                || !mskTc.MaskCompleted || !mskTelno.MaskCompleted || cmbCinsiyet.SelectedItem == null)
            {
                MessageBox.Show("Lütfen tüm alanları eksiksiz doldurun.");
                return;
            }

            using (SqlConnection baglanti = bgl.baglanti())
            {
                // Bu TC ile daha önce kayıt olunmuş mu?
                using (SqlCommand kontrol = new SqlCommand("Select Count(*) From Tbl_Hastalar where HastaTC=@p1", baglanti))
                {
                    kontrol.Parameters.AddWithValue("@p1", mskTc.Text);
                    if ((int)kontrol.ExecuteScalar() > 0)
                    {
                        MessageBox.Show("Bu TC kimlik numarasıyla zaten bir kayıt var.");
                        return;
                    }
                }

                using (SqlCommand komut = new SqlCommand(
                    "insert into Tbl_Hastalar (HastaAd,HastaSoyad,HastaTC,HastaTelefon,HastaSifre,HastaCinsiyet) values (@p1,@p2,@p3,@p4,@p5,@p6)",
                    baglanti))
                {
                    komut.Parameters.AddWithValue("@p1", txtAd.Text.Trim());
                    komut.Parameters.AddWithValue("@p2", txtSoyad.Text.Trim());
                    komut.Parameters.AddWithValue("@p3", mskTc.Text);
                    komut.Parameters.AddWithValue("@p4", mskTelno.Text);
                    komut.Parameters.AddWithValue("@p5", txtSifre.Text);
                    komut.Parameters.AddWithValue("@p6", cmbCinsiyet.SelectedItem.ToString());
                    komut.ExecuteNonQuery();
                }
            }

            // Şifre artık mesajda gösterilmiyor.
            MessageBox.Show("Kaydınız oluşturuldu. Giriş ekranından giriş yapabilirsiniz.");
            this.Close();   // giriş ekranı geri gelir
        }
    }
}
