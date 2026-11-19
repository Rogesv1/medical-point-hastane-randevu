using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Proje_Hastane
{
    public partial class FrmDoktorDetay : Form
    {
        public FrmDoktorDetay()
        {
            InitializeComponent();
            Tema.Uygula(this);
            Tema.IkincilDugme(btnBilgiDuzenle);
            Tema.IkincilDugme(btnDuyurular);
            Tema.TehlikeDugme(btnCikis);
        }

        public string TC;          // giriş ekranından gelen doktor TC'si
        string doktorAdSoyad;      // randevular tablosunda doktor "Ad Soyad" olarak tutuluyor
        sqlbaglanti bgl = new sqlbaglanti();

        private void FrmDoktorDetay_Load(object sender, EventArgs e)
        {
            lblTc.Text = TC;
            dataGridView1.AllowUserToAddRows = false;
            rchSikayet.ReadOnly = true;   // doktor şikayeti sadece okur
            BilgileriYukle();
        }

        private void BilgileriYukle()
        {
            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlCommand komut = new SqlCommand("Select DoktorAd, DoktorSoyad From Tblo_Doktorlar where DoktorTc=@p1", baglanti))
            {
                komut.Parameters.AddWithValue("@p1", TC);
                using (SqlDataReader dr = komut.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        doktorAdSoyad = dr[0] + " " + dr[1];
                        lblAdSoyad.Text = doktorAdSoyad;
                    }
                }
            }

            // Sadece bu doktora ait randevular
            DataTable dt = new DataTable();
            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlDataAdapter da = new SqlDataAdapter("Select * From Tbl_Randevular where RandevuDoktor=@p1", baglanti))
            {
                // Doktor bulunamazsa doktorAdSoyad null kalır. Değeri null olan parametre ADO.NET'e
                // "hiç verilmemiş" sayılır ve "expects the parameter '@p1'" hatası çıkar; boş metin gönderiyoruz.
                da.SelectCommand.Parameters.AddWithValue("@p1", doktorAdSoyad ?? "");
                da.Fill(dt);
            }
            dataGridView1.DataSource = dt;
            rchSikayet.Clear();
        }

        // Randevu listesinde bir satıra tıklanınca hastanın şikayeti "Randevu Detay" kutusunda gösterilir.
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            rchSikayet.Text = dataGridView1.Rows[e.RowIndex].Cells["HastaSikayet"].Value.ToString();
        }

        private void btnBilgiDuzenle_Click(object sender, EventArgs e)
        {
            FrmDoktorBilgiDuzenle fr = new FrmDoktorBilgiDuzenle();
            fr.TCNO = TC;
            fr.FormClosed += (s, args) =>
            {
                BilgileriYukle();   // ad soyad değişmiş olabilir
                this.Show();
            };
            fr.Show();
            this.Hide();
        }

        private void btnDuyurular_Click(object sender, EventArgs e)
        {
            FrmDuyurular fr = new FrmDuyurular();
            fr.Show();
        }

        private void btnCikis_Click(object sender, EventArgs e)
        {
            this.Close();   // oturumu kapatır, rol seçim ekranına dönülür
        }
    }
}
