using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Proje_Hastane
{
    public partial class FrmHastaGiris : Form
    {
        public FrmHastaGiris()
        {
            InitializeComponent();
            Tema.Uygula(this);
            Tema.Baslik(label1);
        }

        sqlbaglanti bgl = new sqlbaglanti();

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FrmHastaKayit fr = new FrmHastaKayit();
            fr.FormClosed += (s, args) => this.Show();   // kayıt penceresi kapanınca giriş ekranına dön
            fr.Show();
            this.Hide();
        }

        private void btnGirisYap_Click(object sender, EventArgs e)
        {
            // Önce alanlar dolu mu? (MaskCompleted: 11 hanenin hepsi girildiyse true olur)
            if (!mskTc.MaskCompleted || txtSifre.Text == "")
            {
                MessageBox.Show("TC kimlik numarasını (11 hane) ve şifrenizi girin.");
                return;
            }

            bool kayitVar;
            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlCommand komut = new SqlCommand(
                "Select Count(*) From Tbl_Hastalar Where HastaTC=@p1 and HastaSifre=@p2", baglanti))
            {
                komut.Parameters.AddWithValue("@p1", mskTc.Text);
                komut.Parameters.AddWithValue("@p2", txtSifre.Text);
                kayitVar = (int)komut.ExecuteScalar() > 0;   // ExecuteScalar: tek bir değer (burada sayı) döndürür
            }

            if (kayitVar)
            {
                FrmHastaDetay fr = new FrmHastaDetay();
                fr.tc = mskTc.Text;
                fr.FormClosed += (s, args) => this.Close();   // hasta paneli kapanınca rol seçimine dön
                fr.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Hatalı TC veya şifre!");
            }
        }
    }
}
