using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Proje_Hastane
{
    public partial class FrmDoktorBilgiDuzenle : Form
    {
        public FrmDoktorBilgiDuzenle()
        {
            InitializeComponent();
            Tema.Uygula(this);
        }

        public string TCNO;
        string eskiAdSoyad;   // güncellemeden önceki ad soyad (randevuları da güncellemek için)
        sqlbaglanti bgl = new sqlbaglanti();

        private void FrmDoktorBilgiDuzenle_Load(object sender, EventArgs e)
        {
            mskTc.Text = TCNO;
            mskTc.ReadOnly = true;

            using (SqlConnection baglanti = bgl.baglanti())
            {
                using (SqlCommand komut = new SqlCommand("Select BransAd From Tbl_Branslar", baglanti))
                using (SqlDataReader dr = komut.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        cmbBrans.Items.Add(dr[0]);
                    }
                }

                using (SqlCommand komut = new SqlCommand("Select * From Tblo_Doktorlar where DoktorTc=@p1", baglanti))
                {
                    komut.Parameters.AddWithValue("@p1", TCNO);
                    using (SqlDataReader dr = komut.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            txtAd.Text = dr["DoktorAd"].ToString();
                            txtSoyad.Text = dr["DoktorSoyad"].ToString();
                            cmbBrans.Text = dr["DoktorBrans"].ToString();
                            txtSifre.Text = dr["DoktorSifre"].ToString();
                            eskiAdSoyad = txtAd.Text + " " + txtSoyad.Text;
                        }
                    }
                }
            }
        }

        private void btnGuncelle_Click(object sender, EventArgs e)
        {
            if (txtAd.Text.Trim() == "" || txtSoyad.Text.Trim() == "" || cmbBrans.Text == "" || txtSifre.Text == "")
            {
                MessageBox.Show("Lütfen tüm alanları eksiksiz doldurun.");
                return;
            }

            string yeniAdSoyad = txtAd.Text.Trim() + " " + txtSoyad.Text.Trim();

            using (SqlConnection baglanti = bgl.baglanti())
            // Transaction: içindeki komutların YA HEPSİ kaydedilir YA HİÇBİRİ.
            // Doktor güncellenip randevular güncellenemezse veriler tutarsız kalmasın diye.
            using (SqlTransaction islem = baglanti.BeginTransaction())
            {
                using (SqlCommand komut = new SqlCommand(
                    "update Tblo_Doktorlar set DoktorAd=@p1, DoktorSoyad=@p2, DoktorBrans=@p3, DoktorSifre=@p4 where DoktorTc=@p5",
                    baglanti, islem))
                {
                    komut.Parameters.AddWithValue("@p1", txtAd.Text.Trim());
                    komut.Parameters.AddWithValue("@p2", txtSoyad.Text.Trim());
                    komut.Parameters.AddWithValue("@p3", cmbBrans.Text);
                    komut.Parameters.AddWithValue("@p4", txtSifre.Text);
                    komut.Parameters.AddWithValue("@p5", TCNO);
                    komut.ExecuteNonQuery();
                }

                // Randevular doktoru "Ad Soyad" yazısıyla tutuyor; ad değişirse eski randevular da yeni adı almalı.
                using (SqlCommand komut = new SqlCommand(
                    "update Tbl_Randevular set RandevuDoktor=@yeni where RandevuDoktor=@eski", baglanti, islem))
                {
                    komut.Parameters.AddWithValue("@yeni", yeniAdSoyad);
                    komut.Parameters.AddWithValue("@eski", eskiAdSoyad);
                    komut.ExecuteNonQuery();
                }

                islem.Commit();   // her şey yolunda: değişiklikleri kalıcı yap
            }

            MessageBox.Show("Bilgileriniz güncellendi.");
            this.Close();
        }
    }
}
