using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
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

        // Doğrulamadan geçen tarih ve saat, her zaman aynı biçimde ("25.11.2026", "09:30") kaydedilir.
        string tarihMetni, saatMetni;

        // Kaydet ve Güncelle aynı alanları kullandığı için ortak kontrol.
        // yeniRandevu: Kaydet için true (geçmiş tarih yasak), Güncelle için false.
        private bool RandevuAlanlariGecerli(bool yeniRandevu)
        {
            if (!mskTarih.MaskCompleted || !mskSaat.MaskCompleted || cmbBrans.SelectedItem == null || cmbDoktorlar.SelectedItem == null)
            {
                MessageBox.Show("Tarih, saat, branş ve doktor alanlarını doldurun.");
                return false;
            }

            // Eskiden sadece "tüm haneler dolu mu" bakılıyordu; 31.02.2026 veya 14.31.2432 gibi
            // olmayan tarihler, 25:99 gibi olmayan saatler kaydediliyordu.
            // TryParseExact metni verilen kalıba göre gerçek bir tarihe çevirmeyi dener; çeviremezse false döner.
            string tarihRakamlari = new string(mskTarih.Text.Where(char.IsDigit).ToArray());
            if (!DateTime.TryParseExact(tarihRakamlari, "ddMMyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime tarih))
            {
                MessageBox.Show("Geçersiz tarih. Gerçek bir tarih girin (ör. 25.11.2026).");
                return false;
            }
            if (yeniRandevu && tarih < DateTime.Today)
            {
                MessageBox.Show("Geçmiş bir tarihe randevu açılamaz.");
                return false;
            }

            string saatRakamlari = new string(mskSaat.Text.Where(char.IsDigit).ToArray());
            if (!DateTime.TryParseExact(saatRakamlari, "HHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime saat))
            {
                MessageBox.Show("Geçersiz saat. 00:00 ile 23:59 arasında bir saat girin.");
                return false;
            }

            if (mskTc.Text != "" && !mskTc.MaskCompleted)
            {
                MessageBox.Show("Hasta TC kimlik numarası 11 hane olmalı (boş da bırakılabilir).");
                return false;
            }

            tarihMetni = tarih.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            saatMetni = saat.ToString("HH:mm", CultureInfo.InvariantCulture);

            // Aynı doktora aynı tarih ve saatte ikinci randevu açılmasın.
            // Güncellemede randevunun kendisi sayılmasın diye kendi numarası hariç tutulur.
            int kendiNumarasi = int.TryParse(txtİd.Text, out int no) && !yeniRandevu ? no : -1;
            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlCommand komut = new SqlCommand(
                "Select Count(*) From Tbl_Randevular where RandevuDoktor=@p1 and RandevuTarih=@p2 and RandevuSaat=@p3 and Randevuid<>@p4",
                baglanti))
            {
                komut.Parameters.AddWithValue("@p1", cmbDoktorlar.Text);
                komut.Parameters.AddWithValue("@p2", tarihMetni);
                komut.Parameters.AddWithValue("@p3", saatMetni);
                komut.Parameters.AddWithValue("@p4", kendiNumarasi);
                if ((int)komut.ExecuteScalar() > 0)
                {
                    MessageBox.Show(cmbDoktorlar.Text + " için " + tarihMetni + " " + saatMetni + " saatinde zaten bir randevu var.");
                    return false;
                }
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
            if (!RandevuAlanlariGecerli(true)) return;

            // Eskiden Durum ve TC kaydedilmiyordu; RandevuDurum NULL kaldığı için randevu hastaya hiç görünmüyordu.
            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlCommand komut = new SqlCommand(
                "INSERT INTO Tbl_Randevular (RandevuTarih, RandevuSaat, RandevuBrans, RandevuDoktor, RandevuDurum, HastaTc) VALUES (@r1, @r2, @r3, @r4, @r5, @r6)",
                baglanti))
            {
                komut.Parameters.AddWithValue("@r1", tarihMetni);
                komut.Parameters.AddWithValue("@r2", saatMetni);
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
            if (!int.TryParse(txtİd.Text, out int randevuNo))
            {
                MessageBox.Show("Güncellenecek randevuyu Randevu Listesi'nden çift tıklayarak seçin.");
                return;
            }
            if (!RandevuAlanlariGecerli(false)) return;

            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlCommand komut = new SqlCommand(
                "update Tbl_Randevular set RandevuTarih=@r1, RandevuSaat=@r2, RandevuBrans=@r3, RandevuDoktor=@r4, RandevuDurum=@r5, HastaTc=@r6 where Randevuid=@r7",
                baglanti))
            {
                komut.Parameters.AddWithValue("@r1", tarihMetni);
                komut.Parameters.AddWithValue("@r2", saatMetni);
                komut.Parameters.AddWithValue("@r3", cmbBrans.Text);
                komut.Parameters.AddWithValue("@r4", cmbDoktorlar.Text);
                komut.Parameters.AddWithValue("@r5", checkBox1.Checked);
                komut.Parameters.AddWithValue("@r6", HastaTcDegeri());
                komut.Parameters.AddWithValue("@r7", randevuNo);
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
