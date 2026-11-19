using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Proje_Hastane
{
    public partial class BransPaneli : Form
    {
        public BransPaneli()
        {
            InitializeComponent();
            Tema.Uygula(this);
            Tema.TehlikeDugme(btnSil);
            Tema.IkincilDugme(btnGuncelle);
        }

        sqlbaglanti bgl = new sqlbaglanti();
        string secilenEskiAd = "";   // güncellemede doktor ve randevulardaki branş adını da düzeltmek için

        private void BransPaneli_Load(object sender, EventArgs e)
        {
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ReadOnly = true;
            txtBransId.ReadOnly = true;   // id veritabanı tarafından verilir, elle yazılmaz
            BranslariListele();
        }

        private void BranslariListele()
        {
            DataTable dt = new DataTable();
            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlDataAdapter da = new SqlDataAdapter("Select * From Tbl_Branslar", baglanti))
            {
                da.Fill(dt);
            }
            dataGridView1.DataSource = dt;
        }

        private void Temizle()
        {
            txtBransId.Clear();
            txtBransAd.Clear();
            secilenEskiAd = "";
        }

        // Aynı adda başka bir branş var mı? (güncellemede branşın kendisi hariç)
        private bool AyniAdVar(SqlConnection baglanti)
        {
            using (SqlCommand komut = new SqlCommand(
                "Select Count(*) From Tbl_Branslar where BransAd=@p1 and Bransid<>@p2", baglanti))
            {
                komut.Parameters.AddWithValue("@p1", txtBransAd.Text.Trim());
                komut.Parameters.AddWithValue("@p2", txtBransId.Text == "" ? "0" : txtBransId.Text);
                return (int)komut.ExecuteScalar() > 0;
            }
        }

        private void btnEkle_Click(object sender, EventArgs e)
        {
            if (txtBransAd.Text.Trim() == "")
            {
                MessageBox.Show("Branş adını yazın.");
                return;
            }

            using (SqlConnection baglanti = bgl.baglanti())
            {
                txtBransId.Clear();
                if (AyniAdVar(baglanti))
                {
                    MessageBox.Show("Bu isimde bir branş zaten var.");
                    return;
                }

                using (SqlCommand komut = new SqlCommand("insert into Tbl_Branslar (BransAd) values (@p1)", baglanti))
                {
                    komut.Parameters.AddWithValue("@p1", txtBransAd.Text.Trim());
                    komut.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Branş eklendi.");
            Temizle();
            BranslariListele();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow satir = dataGridView1.Rows[e.RowIndex];
            txtBransId.Text = satir.Cells["Bransid"].Value.ToString();
            txtBransAd.Text = satir.Cells["BransAd"].Value.ToString();
            secilenEskiAd = txtBransAd.Text;
        }

        private void btnSil_Click(object sender, EventArgs e)
        {
            if (txtBransId.Text == "")
            {
                MessageBox.Show("Önce tablodan silinecek branşı seçin.");
                return;
            }

            // ESKİ HATA: new SqlCommand("delete ...") yazılmış ama bağlantı verilmemişti.
            // Bağlantısız komut çalıştırılamadığı için Sil ve Güncelle her basışta programı çökertiyordu.
            using (SqlConnection baglanti = bgl.baglanti())
            {
                // Bu branşta doktor varsa silmeyelim; yoksa o doktorlar listede olmayan bir branşa bağlı kalır.
                using (SqlCommand kontrol = new SqlCommand("Select Count(*) From Tblo_Doktorlar where DoktorBrans=@p1", baglanti))
                {
                    kontrol.Parameters.AddWithValue("@p1", secilenEskiAd);
                    int doktorSayisi = (int)kontrol.ExecuteScalar();
                    if (doktorSayisi > 0)
                    {
                        MessageBox.Show("Bu branşta " + doktorSayisi + " doktor var. Önce doktorları başka bir branşa taşıyın.");
                        return;
                    }
                }

                DialogResult cevap = MessageBox.Show(secilenEskiAd + " branşı silinsin mi?", "Branş Sil",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (cevap != DialogResult.Yes) return;

                using (SqlCommand komut = new SqlCommand("delete From Tbl_Branslar where Bransid=@p1", baglanti))
                {
                    komut.Parameters.AddWithValue("@p1", txtBransId.Text);
                    komut.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Branş silindi.");
            Temizle();
            BranslariListele();
        }

        private void btnGuncelle_Click(object sender, EventArgs e)
        {
            if (txtBransId.Text == "")
            {
                MessageBox.Show("Önce tablodan güncellenecek branşı seçin.");
                return;
            }
            if (txtBransAd.Text.Trim() == "")
            {
                MessageBox.Show("Branş adı boş olamaz.");
                return;
            }

            string yeniAd = txtBransAd.Text.Trim();

            using (SqlConnection baglanti = bgl.baglanti())
            {
                if (AyniAdVar(baglanti))
                {
                    MessageBox.Show("Bu isimde başka bir branş var.");
                    return;
                }

                // Branş adı doktorlarda ve randevularda da yazı olarak tutuluyor.
                // Üç tabloyu birlikte güncelliyoruz: ya hepsi ya hiçbiri (transaction).
                using (SqlTransaction islem = baglanti.BeginTransaction())
                {
                    string[] sorgular =
                    {
                        "update Tbl_Branslar set BransAd=@yeni where Bransid=@id",
                        "update Tblo_Doktorlar set DoktorBrans=@yeni where DoktorBrans=@eski",
                        "update Tbl_Randevular set RandevuBrans=@yeni where RandevuBrans=@eski"
                    };
                    foreach (string sorgu in sorgular)
                    {
                        using (SqlCommand komut = new SqlCommand(sorgu, baglanti, islem))
                        {
                            komut.Parameters.AddWithValue("@yeni", yeniAd);
                            komut.Parameters.AddWithValue("@eski", secilenEskiAd);
                            komut.Parameters.AddWithValue("@id", txtBransId.Text);
                            komut.ExecuteNonQuery();
                        }
                    }
                    islem.Commit();
                }
            }

            MessageBox.Show("Branş güncellendi.");
            Temizle();
            BranslariListele();
        }
    }
}
