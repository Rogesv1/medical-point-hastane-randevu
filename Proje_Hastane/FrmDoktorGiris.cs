using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Proje_Hastane
{
    public partial class FrmDoktorGiris : Form
    {
        public FrmDoktorGiris()
        {
            InitializeComponent();
            Tema.Uygula(this);
            Tema.Baslik(label1);
        }

        sqlbaglanti bgl = new sqlbaglanti();

        // Eskiden "Giriş Yap" düğmesine hiçbir kod bağlı değildi; doktor sisteme giremiyordu.
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
                "Select Count(*) From Tblo_Doktorlar where DoktorTc=@p1 and DoktorSifre=@p2", baglanti))
            {
                komut.Parameters.AddWithValue("@p1", mskTc.Text);
                komut.Parameters.AddWithValue("@p2", txtSifre.Text);
                kayitVar = (int)komut.ExecuteScalar() > 0;
            }

            if (kayitVar)
            {
                FrmDoktorDetay fr = new FrmDoktorDetay();
                fr.TC = mskTc.Text;
                fr.FormClosed += (s, args) => this.Close();   // doktor paneli kapanınca rol seçimine dön
                fr.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Hatalı TC veya şifre!");
            }
        }
    }
}
