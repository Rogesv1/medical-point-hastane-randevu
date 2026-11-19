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
    public partial class BransPaneli : Form
    {
        public BransPaneli()
        {
            InitializeComponent();
        }
        sqlbaglanti bgl =new sqlbaglanti();
        private void BransPaneli_Load(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            SqlDataAdapter da =new SqlDataAdapter("Select * From Tbl_Branslar", bgl.baglanti());
            da.Fill(dt);
            dataGridView1.DataSource = dt;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            SqlCommand komut=new SqlCommand("insert into Tbl_Branslar (BransAd) values (@p1)", bgl.baglanti());
            komut.Parameters.AddWithValue("@p1", txtSoyad.Text);
            komut.ExecuteNonQuery();
            bgl.baglanti().Close();
            MessageBox.Show("Brans Eklendi.");
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int secilen = dataGridView1.SelectedCells[0].RowIndex;
            txtAd.Text = dataGridView1.Rows[secilen].Cells[0].Value.ToString();
            txtSoyad.Text = dataGridView1.Rows[secilen].Cells[1].Value.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SqlCommand komut = new SqlCommand("delete From Tbl_Branslar where Bransid=@p1");
            {
                komut.Parameters.AddWithValue("@p1", txtAd.Text);
                komut.ExecuteNonQuery();
                bgl.baglanti().Close();
                MessageBox.Show("Brans Silindi.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SqlCommand komut=new SqlCommand("update Tbl_Branslar set bransad=@p1 where Bransid=@p2");
            komut.Parameters.AddWithValue("@p1", txtSoyad.Text);
            komut.Parameters.AddWithValue("@p2", txtAd.Text);
            komut.ExecuteNonQuery();
            bgl.baglanti().Close();
            MessageBox.Show("Brans Güncellendi.");
        }
    }
}
