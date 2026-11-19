using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Proje_Hastane
{
    public partial class FrmBilgiDuzenle : Form
    {
        public FrmBilgiDuzenle()
        {
            InitializeComponent();
        }

        public string TCno;
        sqlbaglanti bgl = new sqlbaglanti();

        private void FrmBilgiDuzenle_Load(object sender, EventArgs e)
        {
            mskTc.Text = TCno;
            mskTc.ReadOnly = true;   // TC hastayı tanımlayan bilgi; buradan değiştirilemez

            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlCommand komut = new SqlCommand("Select * From Tbl_Hastalar where HastaTC=@p1", baglanti))
            {
                komut.Parameters.AddWithValue("@p1", TCno);
                using (SqlDataReader dr = komut.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        // Eskiden sütunlar sıra numarasıyla okunuyordu (dr[5], dr[6]) ve şifre ile cinsiyet
                        // yer değiştirmişti. Sütunu adıyla okumak hem doğru hem de okunaklı.
                        txtAd.Text = dr["HastaAd"].ToString();
                        txtSoyad.Text = dr["HastaSoyad"].ToString();
                        mskTelno.Text = dr["HastaTelefon"].ToString();
                        txtSifre.Text = dr["HastaSifre"].ToString();
                        cmbCinsiyet.Text = dr["HastaCinsiyet"].ToString();
                    }
                }
            }
        }

        private void btnGuncelle_Click(object sender, EventArgs e)
        {
            if (txtAd.Text.Trim() == "" || txtSoyad.Text.Trim() == "" || txtSifre.Text == ""
                || !mskTelno.MaskCompleted || cmbCinsiyet.Text == "")
            {
                MessageBox.Show("Lütfen tüm alanları eksiksiz doldurun.");
                return;
            }

            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlCommand komut = new SqlCommand(
                "update Tbl_Hastalar set HastaAd=@p1,HastaSoyad=@p2,HastaTelefon=@p3,HastaSifre=@p4,HastaCinsiyet=@p5 where HastaTC=@p6",
                baglanti))
            {
                komut.Parameters.AddWithValue("@p1", txtAd.Text.Trim());
                komut.Parameters.AddWithValue("@p2", txtSoyad.Text.Trim());
                komut.Parameters.AddWithValue("@p3", mskTelno.Text);
                komut.Parameters.AddWithValue("@p4", txtSifre.Text);
                komut.Parameters.AddWithValue("@p5", cmbCinsiyet.Text);
                komut.Parameters.AddWithValue("@p6", TCno);
                komut.ExecuteNonQuery();
            }

            MessageBox.Show("Bilgileriniz güncellendi.");
            this.Close();   // hasta paneline dön
        }
    }
}
