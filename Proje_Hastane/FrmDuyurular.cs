using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Proje_Hastane
{
    public partial class FrmDuyurular : Form
    {
        public FrmDuyurular()
        {
            InitializeComponent();
            Tema.Uygula(this);
        }

        sqlbaglanti bgl = new sqlbaglanti();

        private void FrmDuyurular_Load(object sender, EventArgs e)
        {
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ReadOnly = true;

            DataTable dt = new DataTable();
            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlDataAdapter da = new SqlDataAdapter("Select * From Tbl_Duyurular", baglanti))
            {
                da.Fill(dt);
            }
            dataGridView1.DataSource = dt;
        }
    }
}
