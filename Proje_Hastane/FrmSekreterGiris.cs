using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Proje_Hastane
{
    public partial class FrmSekreterGiris : Form
    {
        public FrmSekreterGiris()
        {
            InitializeComponent();
        }

        sqlbaglanti bgl = new sqlbaglanti();

        private void btnGirisYap_Click(object sender, EventArgs e)
        {
            if (!mskTc.MaskCompleted || txtSifre.Text == "")
            {
                MessageBox.Show("TC kimlik numarasını (11 hane) ve şifrenizi girin.");
                return;
            }

            bool kayitVar;
            using (SqlConnection baglanti = bgl.baglanti())
            using (SqlCommand komut = new SqlCommand(
                "Select Count(*) From Tbl_Sekreter where SekreterTc=@p1 and SekreterSifre=@p2", baglanti))
            {
                komut.Parameters.AddWithValue("@p1", mskTc.Text);
                komut.Parameters.AddWithValue("@p2", txtSifre.Text);
                kayitVar = (int)komut.ExecuteScalar() > 0;
            }

            if (kayitVar)
            {
                FrmSekreterDetay frs = new FrmSekreterDetay();
                frs.tcnumara = mskTc.Text;
                frs.FormClosed += (s, args) => this.Close();   // sekreter paneli kapanınca rol seçimine dön
                frs.Show();
                this.Hide();
            }
            else
            {
                // Eskiden şifre yanlışsa hiçbir şey olmuyordu.
                MessageBox.Show("Hatalı TC veya şifre!");
            }
        }
    }
}
