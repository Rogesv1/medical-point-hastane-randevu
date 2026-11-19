using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Proje_Hastane
{
    public partial class FrmDoktorPaneli : Form
    {
        public FrmDoktorPaneli()
        {
            InitializeComponent();
            Tema.Uygula(this);
            Tema.TehlikeDugme(btnSil);
            Tema.IkincilDugme(btnGuncelle);
        }

        sqlbaglanti bgl = new sqlbaglanti();
        string secilenDoktorId = "";     // tablodan seçilen doktorun id'si (Sil/Güncelle için)
        string secilenEskiAdSoyad = "";  // güncellemede randevuları da düzeltmek için

        private void FrmDoktorPaneli_Load(object sender, EventArgs e)
        {
            // Tablonun altındaki boş "yeni satır"a tıklanınca program çöküyordu; o satırı kaldırıyoruz.
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ReadOnly = true;
            DoktorlariListele();

            // Branş kutusu eskiden veritabanından doldurulmuyordu.
            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlCommand komut = new SqlCommand("Select BransAd From Tbl_Branslar", baglanti))
            using (SqlDataReader dr = komut.ExecuteReader())
            {
                while (dr.Read())
                {
                    cmbBrans.Items.Add(dr[0]);
                }
            }
        }

        private void DoktorlariListele()
        {
            DataTable dt = new DataTable();
            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlDataAdapter da = new SqlDataAdapter("Select * From Tblo_Doktorlar", baglanti))
            {
                da.Fill(dt);
            }
            dataGridView1.DataSource = dt;
        }

        private void Temizle()
        {
            txtAd.Clear();
            txtSoyad.Clear();
            cmbBrans.Text = "";
            mskTc.Clear();
            txtSifre.Clear();
            secilenDoktorId = "";
            secilenEskiAdSoyad = "";
        }

        private bool AlanlarGecerli()
        {
            if (txtAd.Text.Trim() == "" || txtSoyad.Text.Trim() == "" || cmbBrans.Text == ""
                || !mskTc.MaskCompleted || txtSifre.Text == "")
            {
                MessageBox.Show("Lütfen tüm alanları eksiksiz doldurun.");
                return false;
            }
            return true;
        }

        // Aynı TC başka bir doktorda var mı? (güncellemede doktorun kendisi hariç tutulur)
        private bool TcBaskasindaVar(SqlConnection baglanti)
        {
            using (SqlCommand komut = new SqlCommand(
                "Select Count(*) From Tblo_Doktorlar where DoktorTc=@p1 and Doktorid<>@p2", baglanti))
            {
                komut.Parameters.AddWithValue("@p1", mskTc.Text);
                komut.Parameters.AddWithValue("@p2", secilenDoktorId == "" ? "0" : secilenDoktorId);
                return (int)komut.ExecuteScalar() > 0;
            }
        }

        private void btnEkle_Click(object sender, EventArgs e)
        {
            if (!AlanlarGecerli()) return;

            using (SqlConnection baglanti = bgl.baglanti())
            {
                secilenDoktorId = "";
                if (TcBaskasindaVar(baglanti))
                {
                    MessageBox.Show("Bu TC kimlik numarasıyla kayıtlı bir doktor zaten var.");
                    return;
                }

                using (SqlCommand komut = new SqlCommand(
                    "insert into Tblo_Doktorlar (DoktorAd, DoktorSoyad, DoktorBrans, DoktorTc, DoktorSifre) values (@d1, @d2, @d3, @d4, @d5)",
                    baglanti))
                {
                    komut.Parameters.AddWithValue("@d1", txtAd.Text.Trim());
                    komut.Parameters.AddWithValue("@d2", txtSoyad.Text.Trim());
                    komut.Parameters.AddWithValue("@d3", cmbBrans.Text);
                    komut.Parameters.AddWithValue("@d4", mskTc.Text);
                    komut.Parameters.AddWithValue("@d5", txtSifre.Text);
                    komut.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Doktor eklendi.");
            Temizle();
            DoktorlariListele();   // eskiden tablo yenilenmiyordu
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;   // başlığa tıklandı
            DataGridViewRow satir = dataGridView1.Rows[e.RowIndex];

            // Sütunlara sıra numarası yerine adıyla erişmek, tablo yapısı değişse bile doğru çalışır.
            secilenDoktorId = satir.Cells["Doktorid"].Value.ToString();
            txtAd.Text = satir.Cells["DoktorAd"].Value.ToString();
            txtSoyad.Text = satir.Cells["DoktorSoyad"].Value.ToString();
            cmbBrans.Text = satir.Cells["DoktorBrans"].Value.ToString();
            mskTc.Text = satir.Cells["DoktorTc"].Value.ToString();
            txtSifre.Text = satir.Cells["DoktorSifre"].Value.ToString();
            secilenEskiAdSoyad = txtAd.Text + " " + txtSoyad.Text;
        }

        // Eskiden Sil düğmesine kod bağlı değildi.
        private void btnSil_Click(object sender, EventArgs e)
        {
            if (secilenDoktorId == "")
            {
                MessageBox.Show("Önce tablodan silinecek doktoru seçin.");
                return;
            }

            DialogResult cevap = MessageBox.Show(secilenEskiAdSoyad + " silinsin mi?", "Doktor Sil",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (cevap != DialogResult.Yes) return;

            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlCommand komut = new SqlCommand("delete From Tblo_Doktorlar where Doktorid=@p1", baglanti))
            {
                komut.Parameters.AddWithValue("@p1", secilenDoktorId);
                komut.ExecuteNonQuery();
            }

            MessageBox.Show("Doktor silindi.");
            Temizle();
            DoktorlariListele();
        }

        // Eskiden Güncelle düğmesine kod bağlı değildi.
        private void btnGuncelle_Click(object sender, EventArgs e)
        {
            if (secilenDoktorId == "")
            {
                MessageBox.Show("Önce tablodan güncellenecek doktoru seçin.");
                return;
            }
            if (!AlanlarGecerli()) return;

            string yeniAdSoyad = txtAd.Text.Trim() + " " + txtSoyad.Text.Trim();

            using (SqlConnection baglanti = bgl.baglanti())
            {
                if (TcBaskasindaVar(baglanti))
                {
                    MessageBox.Show("Bu TC kimlik numarası başka bir doktorda kayıtlı.");
                    return;
                }

                // Doktor ve randevulardaki adı birlikte güncelle: ya ikisi de olur ya hiçbiri.
                using (SqlTransaction islem = baglanti.BeginTransaction())
                {
                    using (SqlCommand komut = new SqlCommand(
                        "update Tblo_Doktorlar set DoktorAd=@d1, DoktorSoyad=@d2, DoktorBrans=@d3, DoktorTc=@d4, DoktorSifre=@d5 where Doktorid=@d6",
                        baglanti, islem))
                    {
                        komut.Parameters.AddWithValue("@d1", txtAd.Text.Trim());
                        komut.Parameters.AddWithValue("@d2", txtSoyad.Text.Trim());
                        komut.Parameters.AddWithValue("@d3", cmbBrans.Text);
                        komut.Parameters.AddWithValue("@d4", mskTc.Text);
                        komut.Parameters.AddWithValue("@d5", txtSifre.Text);
                        komut.Parameters.AddWithValue("@d6", secilenDoktorId);
                        komut.ExecuteNonQuery();
                    }

                    using (SqlCommand komut = new SqlCommand(
                        "update Tbl_Randevular set RandevuDoktor=@yeni where RandevuDoktor=@eski", baglanti, islem))
                    {
                        komut.Parameters.AddWithValue("@yeni", yeniAdSoyad);
                        komut.Parameters.AddWithValue("@eski", secilenEskiAdSoyad);
                        komut.ExecuteNonQuery();
                    }

                    islem.Commit();
                }
            }

            MessageBox.Show("Doktor güncellendi.");
            Temizle();
            DoktorlariListele();
        }
    }
}
