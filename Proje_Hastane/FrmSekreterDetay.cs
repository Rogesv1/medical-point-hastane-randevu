using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Proje_Hastane
{
    public partial class FrmSekreterDetay : Form
    {
        public FrmSekreterDetay()
        {
            InitializeComponent();
        }
        sqlbaglanti bgl=new sqlbaglanti();
        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }
        public string tcnumara;
        private void FrmSekreterDetay_Load(object sender, EventArgs e)
        {
            lblTc.Text = tcnumara;
            SqlCommand komut1 = new SqlCommand("Select SekreterAdSoyad From Tbl_Sekreter where SekreterTc=@p1", bgl.baglanti());
            komut1.Parameters.AddWithValue("@p1", lblTc.Text);
            SqlDataReader dr1 =komut1.ExecuteReader();
            while (dr1.Read())
            {
                lblAdSoyad.Text = dr1[0].ToString();
            }
            bgl.baglanti().Close();


            DataTable dt1 = new DataTable();
            SqlDataAdapter da1 = new SqlDataAdapter("Select * From Tbl_Branslar", bgl.baglanti());
            da1.Fill(dt1);
            dataGridView1.DataSource = dt1;

            DataTable dt2 = new DataTable();
            SqlDataAdapter da2 = new SqlDataAdapter("Select (DoktorAd + ' ' + DoktorSoyad) AS Doktorlar From Tblo_Doktorlar", bgl.baglanti());
            da2.Fill(dt2);
            dataGridView2.DataSource = dt2;

            SqlCommand komut2= new SqlCommand("Select BransAd From Tbl_Branslar", bgl.baglanti());
            SqlDataReader dr2 = komut2.ExecuteReader();
            while (dr2.Read())
            {
                cmbBrans.Items.Add(dr2[0]);
            }
            bgl.baglanti().Close();

            


        }

        private void btnKaydet_Click(object sender, EventArgs e)
        {
            SqlCommand eklemekomut = new SqlCommand(
                "INSERT INTO Tbl_Randevular (RandevuTarih, RandevuSaat, RandevuDoktor, RandevuBrans) VALUES (@r1, @r2, @r3, @r4)",
                    bgl.baglanti());
            eklemekomut.Parameters.AddWithValue("@r1", mskTarih.Text);
            eklemekomut.Parameters.AddWithValue("@r2", mskSaat.Text);
            eklemekomut.Parameters.AddWithValue("@r3", cmbDoktorlar.Text);
            eklemekomut.Parameters.AddWithValue("@r4", cmbBrans.Text);
            eklemekomut.ExecuteNonQuery();
            bgl.baglanti().Close();
            MessageBox.Show("Randevu Oluşturuldu.");
        }

        private void cmbDoktorlar_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbDoktorlar.Items.Clear();
            SqlCommand komut3 = new SqlCommand("Select (DoktorAd + ' ' + DoktorSoyad) AS Doktorlar From Tblo_Doktorlar", bgl.baglanti());
            SqlDataReader dr3 = komut3.ExecuteReader();
            while (dr3.Read())
            {
                cmbDoktorlar.Items.Add(dr3[0]+" " + dr3[1]);
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnOlustur_Click(object sender, EventArgs e)
        {
            SqlCommand komut= new SqlCommand("insert into Tbl_Duyurular (Duyuru) values (@d1)", bgl.baglanti());
            komut.Parameters.AddWithValue("@d1", richTextBox1.Text);
            komut.ExecuteNonQuery();
            bgl.baglanti().Close();
            MessageBox.Show("Duyuru Oluşturuldu.");
        }

        private void btnDoktorPanel_Click(object sender, EventArgs e)
        {
            FrmDoktorPaneli frmd= new FrmDoktorPaneli();
            frmd.Show();
            this.Hide();
        }

        private void btnBransPanel_Click(object sender, EventArgs e)
        {
            BransPaneli frbp = new BransPaneli();
            frbp.Show();
            
        }

        private void btnRandevuPanel_Click(object sender, EventArgs e)
        {
            FrmRandevuListesi fr=new FrmRandevuListesi();
            fr.Show();
        }

        private void btnDuyurular_Click(object sender, EventArgs e)
        {
            FrmDuyurular fr=new FrmDuyurular();
            fr.Show();
        }
    }
}
