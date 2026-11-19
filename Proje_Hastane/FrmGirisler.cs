using System;
using System.Windows.Forms;

namespace Proje_Hastane
{
    public partial class FrmGirisler : Form
    {
        public FrmGirisler()
        {
            InitializeComponent();
        }

        // Açılan giriş penceresi kapanınca bu ekran (rol seçimi) geri gelir.
        // Eskiden pencere gizlenip bir daha gösterilmiyordu; son pencere kapatılsa bile
        // uygulama arka planda açık kalıyordu.
        private void GirisFormunuAc(Form girisFormu)
        {
            girisFormu.FormClosed += (s, args) => this.Show();
            girisFormu.Show();
            this.Hide();
        }

        private void btnHastaGiris_Click(object sender, EventArgs e)
        {
            GirisFormunuAc(new FrmHastaGiris());
        }

        private void btnDoktorGiris_Click(object sender, EventArgs e)
        {
            GirisFormunuAc(new FrmDoktorGiris());
        }

        private void btnSekreterGrisi_Click(object sender, EventArgs e)
        {
            GirisFormunuAc(new FrmSekreterGiris());
        }
    }
}
