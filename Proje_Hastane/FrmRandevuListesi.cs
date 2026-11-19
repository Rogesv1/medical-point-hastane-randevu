using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Proje_Hastane
{
    public partial class FrmRandevuListesi : Form
    {
        public FrmRandevuListesi()
        {
            InitializeComponent();
        }

        sqlbaglanti bgl = new sqlbaglanti();

        // Bu pencereyi açan sekreter paneli. Çift tıklanan randevu bilgileri oraya gönderilir.
        public FrmSekreterDetay sekreterFormu;

        private void FrmRandevuListesi_Load(object sender, EventArgs e)
        {
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ReadOnly = true;

            DataTable dt = new DataTable();
            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlDataAdapter da = new SqlDataAdapter("Select * From Tbl_Randevular", baglanti))
            {
                da.Fill(dt);
            }
            dataGridView1.DataSource = dt;
        }

        // Eskiden bu olayın içi boştu. Artık çift tıklanan randevu sekreter panelindeki
        // Randevu Paneli alanlarına doldurulur; sekreter oradan Güncelle'ye basabilir.
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || sekreterFormu == null) return;
            sekreterFormu.RandevuBilgileriniDoldur(dataGridView1.Rows[e.RowIndex]);
            this.Close();
        }
    }
}
