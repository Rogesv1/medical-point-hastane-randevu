using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Proje_Hastane
{
    public partial class FrmHastaDetay : Form
    {
        public FrmHastaDetay()
        {
            InitializeComponent();
            Tema.Uygula(this);
        }

        public string tc;
        sqlbaglanti bgl = new sqlbaglanti();

        private void FrmHastaDetay_Load(object sender, EventArgs e)
        {
            lbltc.Text = tc;
            dataGridView1.AllowUserToAddRows = false;   // tablonun altındaki boş "yeni satır"ı gizle
            dataGridView2.AllowUserToAddRows = false;
            AdSoyadYukle();
            GecmisRandevulariYukle();

            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlCommand komut = new SqlCommand("Select BransAd From Tbl_Branslar", baglanti))
            using (SqlDataReader dr = komut.ExecuteReader())
            {
                while (dr.Read())
                {
                    cmbbrans.Items.Add(dr[0]);
                }
            }
        }

        private void AdSoyadYukle()
        {
            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlCommand komut = new SqlCommand("Select HastaAd,HastaSoyad From Tbl_Hastalar where HastaTC=@p1", baglanti))
            {
                komut.Parameters.AddWithValue("@p1", tc);
                using (SqlDataReader dr = komut.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        lbladsoyad.Text = dr[0] + " " + dr[1];
                    }
                }
            }
        }

        private void GecmisRandevulariYukle()
        {
            // Eskiden: "... where HastaTC=" + tc  -> SQL injection riski.
            // Değeri sorgu metnine eklemek yerine parametre (@p1) kullanıyoruz.
            DataTable dt = new DataTable();
            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlDataAdapter da = new SqlDataAdapter("Select * From Tbl_Randevular where HastaTc=@p1", baglanti))
            {
                da.SelectCommand.Parameters.AddWithValue("@p1", tc);
                da.Fill(dt);
            }
            dataGridView1.DataSource = dt;
        }

        private void AktifRandevulariYukle()
        {
            if (cmbbrans.SelectedItem == null || cmbdoktor.SelectedItem == null)
            {
                dataGridView2.DataSource = null;
                return;
            }

            // Eskiden branş ve doktor adı sorgu metnine '...' ile ekleniyordu; artık parametre.
            DataTable dt = new DataTable();
            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlDataAdapter da = new SqlDataAdapter(
                "Select * From Tbl_Randevular where RandevuBrans=@p1 and RandevuDoktor=@p2 and RandevuDurum=0", baglanti))
            {
                da.SelectCommand.Parameters.AddWithValue("@p1", cmbbrans.SelectedItem.ToString());
                da.SelectCommand.Parameters.AddWithValue("@p2", cmbdoktor.SelectedItem.ToString());
                da.Fill(dt);
            }
            dataGridView2.DataSource = dt;
        }

        private void cmbbrans_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbdoktor.Items.Clear();
            cmbdoktor.Text = "";
            TXTid.Text = "";

            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlCommand komut = new SqlCommand("Select DoktorAd,DoktorSoyad From Tblo_Doktorlar where DoktorBrans=@p1", baglanti))
            {
                komut.Parameters.AddWithValue("@p1", cmbbrans.SelectedItem.ToString());
                using (SqlDataReader dr = komut.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        cmbdoktor.Items.Add(dr[0] + " " + dr[1]);
                    }
                }
            }
            AktifRandevulariYukle();
        }

        private void cmbdoktor_SelectedIndexChanged(object sender, EventArgs e)
        {
            TXTid.Text = "";
            AktifRandevulariYukle();
        }

        // Aktif randevular tablosunda bir satıra tıklanınca randevu numarası "id" kutusuna yazılır.
        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;   // başlık satırına tıklandıysa bir şey yapma
            TXTid.Text = dataGridView2.Rows[e.RowIndex].Cells["Randevuid"].Value.ToString();
        }

        private void btnRandevuAl_Click(object sender, EventArgs e)
        {
            if (TXTid.Text == "")
            {
                MessageBox.Show("Önce aktif randevular listesinden bir randevu seçin.");
                return;
            }

            int etkilenenSatir;
            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlCommand komut = new SqlCommand(
                "update Tbl_Randevular set RandevuDurum=1, HastaTc=@p1, HastaSikayet=@p2 where Randevuid=@p3 and RandevuDurum=0",
                baglanti))
            {
                komut.Parameters.AddWithValue("@p1", tc);
                komut.Parameters.AddWithValue("@p2", rctSikayet.Text);
                komut.Parameters.AddWithValue("@p3", TXTid.Text);
                etkilenenSatir = komut.ExecuteNonQuery();   // kaç satırın güncellendiğini döndürür
            }

            // Sorgudaki "and RandevuDurum=0" sayesinde aynı randevuyu iki kişi alamaz:
            // başkası daha önce aldıysa hiçbir satır güncellenmez (etkilenenSatir = 0).
            if (etkilenenSatir == 0)
            {
                MessageBox.Show("Bu randevu az önce başkası tarafından alınmış. Lütfen başka bir randevu seçin.");
            }
            else
            {
                MessageBox.Show("Randevunuz alındı.");
                rctSikayet.Clear();
            }

            TXTid.Text = "";
            AktifRandevulariYukle();
            GecmisRandevulariYukle();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FrmBilgiDuzenle fr = new FrmBilgiDuzenle();
            fr.TCno = lbltc.Text;
            fr.FormClosed += (s, args) =>
            {
                AdSoyadYukle();   // ad soyad değiştiyse yenile
                this.Show();
            };
            fr.Show();
            this.Hide();
        }
    }
}
