using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Proje_Hastane
{
    public partial class FrmSekreterDetay : Form
    {
        public FrmSekreterDetay()
        {
            InitializeComponent();
            Tema.Uygula(this);
            Tema.IkincilDugme(btnGuncelle);
            Tema.IkincilDugme(btnRandevuPanel);
            Tema.IkincilDugme(btnBransPanel);
            Tema.IkincilDugme(btnDoktorPanel);
            Tema.IkincilDugme(btnDuyurular);
        }

        public string tcnumara;
        sqlbaglanti bgl = new sqlbaglanti();

        private void FrmSekreterDetay_Load(object sender, EventArgs e)
        {
            lblTc.Text = tcnumara;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView2.AllowUserToAddRows = false;

            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlCommand komut = new SqlCommand("Select SekreterAdSoyad From Tbl_Sekreter where SekreterTc=@p1", baglanti))
            {
                komut.Parameters.AddWithValue("@p1", tcnumara);
                using (SqlDataReader dr = komut.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        lblAdSoyad.Text = dr[0].ToString();
                    }
                }
            }

            ListeleriYukle();
        }

        // Branş ve doktor tablolarını ve branş kutusunu (yeniden) doldurur.
        // Doktor/branş panelinde değişiklik yapılınca da çağrılıyor ki liste güncel kalsın.
        private void ListeleriYukle()
        {
            DataTable branslar = new DataTable();
            DataTable doktorlar = new DataTable();
            using (SqlConnection baglanti = bgl.baglanti())
            {
                using (SqlDataAdapter da = new SqlDataAdapter("Select * From Tbl_Branslar", baglanti))
                {
                    da.Fill(branslar);
                }
                using (SqlDataAdapter da = new SqlDataAdapter(
                    "Select (DoktorAd + ' ' + DoktorSoyad) AS Doktorlar From Tblo_Doktorlar", baglanti))
                {
                    da.Fill(doktorlar);
                }
            }
            dataGridView1.DataSource = branslar;
            dataGridView2.DataSource = doktorlar;

            cmbBrans.Items.Clear();
            cmbDoktorlar.Items.Clear();
            foreach (DataRow satir in branslar.Rows)
            {
                cmbBrans.Items.Add(satir["BransAd"].ToString());
            }
        }

        // Branş seçilince doktor kutusu o branştaki doktorlarla dolar.
        // Eskiden doktor kutusu hiç dolmuyordu: kod kutunun KENDİ seçim olayına yazılmıştı
        // (boş kutuda seçim yapılamayacağı için hiç çalışmıyordu) ve olmayan dr3[1] sütununu okuyordu.
        private void cmbBrans_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbDoktorlar.Items.Clear();
            cmbDoktorlar.Text = "";
            if (cmbBrans.SelectedItem == null) return;

            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlCommand komut = new SqlCommand(
                "Select (DoktorAd + ' ' + DoktorSoyad) From Tblo_Doktorlar where DoktorBrans=@p1", baglanti))
            {
                komut.Parameters.AddWithValue("@p1", cmbBrans.SelectedItem.ToString());
                using (SqlDataReader dr = komut.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        cmbDoktorlar.Items.Add(dr[0].ToString());
                    }
                }
            }
        }

        // Kaydet ve Güncelle aynı alanları kullandığı için ortak kontrol.
        private bool RandevuAlanlariGecerli()
        {
            if (!mskTarih.MaskCompleted || !mskSaat.MaskCompleted || cmbBrans.Text == "" || cmbDoktorlar.Text == "")
            {
                MessageBox.Show("Tarih, saat, branş ve doktor alanlarını doldurun.");
                return false;
            }
            if (mskTc.Text != "" && !mskTc.MaskCompleted)
            {
                MessageBox.Show("Hasta TC kimlik numarası 11 hane olmalı (boş da bırakılabilir).");
                return false;
            }
            return true;
        }

        // TC kutusu boşsa veritabanına NULL yazılır.
        private object HastaTcDegeri()
        {
            if (mskTc.MaskCompleted) return mskTc.Text;
            return DBNull.Value;
        }

        private void btnKaydet_Click(object sender, EventArgs e)
        {
            if (!RandevuAlanlariGecerli()) return;

            // Eskiden Durum ve TC kaydedilmiyordu; RandevuDurum NULL kaldığı için randevu hastaya hiç görünmüyordu.
            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlCommand komut = new SqlCommand(
                "INSERT INTO Tbl_Randevular (RandevuTarih, RandevuSaat, RandevuBrans, RandevuDoktor, RandevuDurum, HastaTc) VALUES (@r1, @r2, @r3, @r4, @r5, @r6)",
                baglanti))
            {
                komut.Parameters.AddWithValue("@r1", mskTarih.Text);
                komut.Parameters.AddWithValue("@r2", mskSaat.Text);
                komut.Parameters.AddWithValue("@r3", cmbBrans.Text);
                komut.Parameters.AddWithValue("@r4", cmbDoktorlar.Text);
                komut.Parameters.AddWithValue("@r5", checkBox1.Checked);
                komut.Parameters.AddWithValue("@r6", HastaTcDegeri());
                komut.ExecuteNonQuery();
            }
            MessageBox.Show("Randevu oluşturuldu.");
        }

        // Eskiden Güncelle düğmesine hiçbir kod bağlı değildi.
        private void btnGuncelle_Click(object sender, EventArgs e)
        {
            if (txtİd.Text == "")
            {
                MessageBox.Show("Güncellenecek randevuyu Randevu Listesi'nden çift tıklayarak seçin.");
                return;
            }
            if (!RandevuAlanlariGecerli()) return;

            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlCommand komut = new SqlCommand(
                "update Tbl_Randevular set RandevuTarih=@r1, RandevuSaat=@r2, RandevuBrans=@r3, RandevuDoktor=@r4, RandevuDurum=@r5, HastaTc=@r6 where Randevuid=@r7",
                baglanti))
            {
                komut.Parameters.AddWithValue("@r1", mskTarih.Text);
                komut.Parameters.AddWithValue("@r2", mskSaat.Text);
                komut.Parameters.AddWithValue("@r3", cmbBrans.Text);
                komut.Parameters.AddWithValue("@r4", cmbDoktorlar.Text);
                komut.Parameters.AddWithValue("@r5", checkBox1.Checked);
                komut.Parameters.AddWithValue("@r6", HastaTcDegeri());
                komut.Parameters.AddWithValue("@r7", txtİd.Text);
                komut.ExecuteNonQuery();
            }
            MessageBox.Show("Randevu güncellendi.");
        }

        // Randevu Listesi penceresinde bir satıra çift tıklanınca o pencere bu metodu çağırır.
        public void RandevuBilgileriniDoldur(DataGridViewRow satir)
        {
            txtİd.Text = satir.Cells["Randevuid"].Value.ToString();
            mskTarih.Text = satir.Cells["RandevuTarih"].Value.ToString();
            mskSaat.Text = satir.Cells["RandevuSaat"].Value.ToString();
            cmbBrans.Text = satir.Cells["RandevuBrans"].Value.ToString();         // önce branş (doktor listesi dolsun)
            cmbDoktorlar.Text = satir.Cells["RandevuDoktor"].Value.ToString();
            object durum = satir.Cells["RandevuDurum"].Value;
            checkBox1.Checked = durum != DBNull.Value && Convert.ToBoolean(durum);
            mskTc.Text = satir.Cells["HastaTc"].Value.ToString();
        }

        private void btnOlustur_Click(object sender, EventArgs e)
        {
            if (richTextBox1.Text.Trim() == "")
            {
                MessageBox.Show("Duyuru metni boş olamaz.");
                return;
            }

            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlCommand komut = new SqlCommand("insert into Tbl_Duyurular (Duyuru) values (@d1)", baglanti))
            {
                komut.Parameters.AddWithValue("@d1", richTextBox1.Text.Trim());
                komut.ExecuteNonQuery();
            }
            richTextBox1.Clear();
            MessageBox.Show("Duyuru oluşturuldu.");
        }

        private void btnDoktorPanel_Click(object sender, EventArgs e)
        {
            FrmDoktorPaneli frmd = new FrmDoktorPaneli();
            frmd.FormClosed += (s, args) =>
            {
                ListeleriYukle();   // doktor eklenmiş/silinmiş olabilir
                this.Show();
            };
            frmd.Show();
            this.Hide();
        }

        private void btnBransPanel_Click(object sender, EventArgs e)
        {
            BransPaneli frbp = new BransPaneli();
            frbp.FormClosed += (s, args) => ListeleriYukle();   // branş eklenmiş/silinmiş olabilir
            frbp.Show();
        }

        private void btnRandevuPanel_Click(object sender, EventArgs e)
        {
            FrmRandevuListesi fr = new FrmRandevuListesi();
            fr.sekreterFormu = this;   // çift tıklanan randevu bu forma gönderilsin diye
            fr.Show();
        }

        private void btnDuyurular_Click(object sender, EventArgs e)
        {
            FrmDuyurular fr = new FrmDuyurular();
            fr.Show();
        }
    }
}
