using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Proje_Hastane
{
    // Uygulamanın ortak görünümü.
    // Her form, kurucusunda InitializeComponent()'ten hemen sonra Tema.Uygula(this) çağırır.
    // Renkler ve yazı tipleri tek yerde durduğu için görünümü değiştirmek isteyen
    // sadece bu dosyayı düzenler; 14 formun tasarım dosyasına tek tek dokunmak gerekmez.
    public static class Tema
    {
        // ---- Renkler ----
        public static readonly Color Arkaplan     = ColorTranslator.FromHtml("#F3F6F5");
        public static readonly Color Kart         = Color.White;
        public static readonly Color Birincil     = ColorTranslator.FromHtml("#0F6E56");
        public static readonly Color BirincilKoyu = ColorTranslator.FromHtml("#085041");
        public static readonly Color BirincilAcik = ColorTranslator.FromHtml("#E1F5EE");
        public static readonly Color Secim        = ColorTranslator.FromHtml("#BFEBDC");
        public static readonly Color Kenar        = ColorTranslator.FromHtml("#D5DFDB");
        public static readonly Color KoyuKenar    = ColorTranslator.FromHtml("#B4C4BE");
        public static readonly Color Metin        = ColorTranslator.FromHtml("#1F2A27");
        public static readonly Color SoluMetin    = ColorTranslator.FromHtml("#5F6B67");
        public static readonly Color Tehlike      = ColorTranslator.FromHtml("#A32D2D");
        public static readonly Color TehlikeAcik  = ColorTranslator.FromHtml("#FCEBEB");
        public static readonly Color AlternatifSatir = ColorTranslator.FromHtml("#F7FAF9");

        // ---- Yazı tipleri ----
        public static readonly Font Yazi       = new Font("Segoe UI", 10F);
        public static readonly Font KalinYazi  = new Font("Segoe UI Semibold", 10F);
        public static readonly Font BaslikYazi = new Font("Segoe UI Semibold", 18F);

        public static void Uygula(Form form)
        {
            // Form, yerleşimi yazı tipine göre ölçekleyecek şekilde (AutoScaleMode.Font) tasarlandı.
            // Yazı tipini değiştirince WinForms bütün kontrolleri yeniden boyutlandırmasın diye
            // ölçeklemeyi kapatıyoruz. Konumlar ve boyutlar tasarımdaki gibi kalır.
            form.AutoScaleMode = AutoScaleMode.None;
            form.BackColor = Arkaplan;
            form.ForeColor = Metin;
            form.Font = Yazi;
            Stillendir(form.Controls);
        }

        // Kontrolleri ve içlerindeki kontrolleri (ör. GroupBox içindekiler) tek tek dolaşır.
        static void Stillendir(Control.ControlCollection kontroller)
        {
            foreach (Control k in kontroller)
            {
                // Sıralama önemli: LinkLabel bir Label, MaskedTextBox ve RichTextBox birer TextBoxBase'dir.
                if (k is GroupBox g) KartYap(g);
                else if (k is Button b) DugmeYap(b);
                else if (k is DataGridView d) TabloYap(d);
                else if (k is LinkLabel ll) BaglantiYap(ll);
                else if (k is Label l) EtiketYap(l);
                else if (k is TextBoxBase t) KutuYap(t);
                else if (k is ComboBox c) AcilirKutuYap(c);
                else if (k is CheckBox cb) { cb.Font = Yazi; cb.ForeColor = Metin; }

                if (k.HasChildren) Stillendir(k.Controls);
            }
        }

        // ---- Etiketler ----

        static void EtiketYap(Label l)
        {
            l.BackColor = Color.Transparent;
            if (l.Name.ToLower().StartsWith("lbl"))
            {
                // lblTc, lblAdSoyad gibi bilgi gösteren etiketler: koyu ve belirgin
                l.Font = KalinYazi;
                l.ForeColor = Metin;
            }
            else
            {
                // "Ad:", "TC Kimlik No:" gibi alan adları: daha soluk
                l.Font = Yazi;
                l.ForeColor = SoluMetin;
            }
        }

        // Pencerenin ana başlığı (ör. "Medical Point", "Hasta Giriş")
        public static void Baslik(Label l)
        {
            l.Font = BaslikYazi;
            l.ForeColor = BirincilKoyu;
        }

        static void BaglantiYap(LinkLabel ll)
        {
            ll.BackColor = Color.Transparent;
            ll.Font = KalinYazi;
            ll.LinkColor = Birincil;
            ll.ActiveLinkColor = BirincilKoyu;
            ll.VisitedLinkColor = Birincil;
            ll.LinkBehavior = LinkBehavior.HoverUnderline;
        }

        // ---- Giriş kutuları ----

        static void KutuYap(TextBoxBase t)
        {
            t.BorderStyle = BorderStyle.FixedSingle;
            t.Font = Yazi;
            t.ForeColor = Metin;
            // Salt okunur kutular (ör. otomatik verilen Id) hafif gri görünsün
            t.BackColor = t.ReadOnly ? AlternatifSatir : Kart;
        }

        static void AcilirKutuYap(ComboBox c)
        {
            // FlatStyle.Flat beyaz kartın üzerinde kenarlıksız görünüyor (kutu kayboluyor);
            // bu yüzden Windows'un standart açılır kutu çizimini koruyoruz.
            c.Font = Yazi;
            c.ForeColor = Metin;
            c.BackColor = Kart;
        }

        // ---- Düğmeler ----

        static void DugmeYap(Button b)
        {
            if (b.BackgroundImage != null)
            {
                // Rol seçim ekranındaki resimli düğmeler: beyaz kart, resim oranı bozulmadan
                b.FlatStyle = FlatStyle.Flat;
                b.UseVisualStyleBackColor = false;
                b.BackColor = Kart;
                b.BackgroundImageLayout = ImageLayout.Zoom;
                b.FlatAppearance.BorderColor = Kenar;
                b.FlatAppearance.BorderSize = 1;
                b.FlatAppearance.MouseOverBackColor = BirincilAcik;
                b.FlatAppearance.MouseDownBackColor = Secim;
                b.Cursor = Cursors.Hand;
                return;
            }
            BirincilDugme(b);   // varsayılan: ana işlem düğmesi
        }

        static void DugmeStili(Button b, Color arka, Color yazi, Color kenar, Color uzerinde, Color basili)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.UseVisualStyleBackColor = false;   // tasarımdaki eski renkleri (ör. MediumSpringGreen) devre dışı bırakır
            b.BackColor = arka;
            b.ForeColor = yazi;
            b.Font = KalinYazi;
            b.FlatAppearance.BorderColor = kenar;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.MouseOverBackColor = uzerinde;
            b.FlatAppearance.MouseDownBackColor = basili;
            b.Cursor = Cursors.Hand;
        }

        // Ana işlem: Giriş Yap, Kaydet, Ekle, Randevu Al...
        public static void BirincilDugme(Button b) =>
            DugmeStili(b, Birincil, Color.White, Birincil, BirincilKoyu, BirincilKoyu);

        // Yan işlem: Güncelle, Duyurular, Hızlı Erişim düğmeleri...
        public static void IkincilDugme(Button b) =>
            DugmeStili(b, Kart, Metin, KoyuKenar, BirincilAcik, Secim);

        // Geri alınamayan işlem: Sil, Çıkış
        public static void TehlikeDugme(Button b) =>
            DugmeStili(b, Kart, Tehlike, Tehlike, TehlikeAcik, TehlikeAcik);

        // ---- Kart görünümlü GroupBox ----

        static void KartYap(GroupBox g)
        {
            g.BackColor = Kart;          // içindeki etiketler de beyaz zemini alır
            g.ForeColor = BirincilKoyu;
            g.Font = KalinYazi;
            // Üst boşluk: kutuyu dolduran (Dock = Fill) tablolar başlık yazısının altında başlasın
            g.Padding = new Padding(6, 10, 6, 8);
            g.Paint += KartCiz;
        }

        // GroupBox'ın kendi çizdiği gri çerçevenin üzerine yuvarlak köşeli beyaz bir kart çizer.
        static void KartCiz(object sender, PaintEventArgs e)
        {
            GroupBox g = (GroupBox)sender;
            Graphics gr = e.Graphics;
            gr.Clear(g.Parent != null ? g.Parent.BackColor : Arkaplan);
            gr.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle alan = new Rectangle(0, 0, g.Width - 1, g.Height - 1);
            using (GraphicsPath yol = YuvarlakDikdortgen(alan, 10))
            using (SolidBrush firca = new SolidBrush(Kart))
            using (Pen kalem = new Pen(Kenar))
            {
                gr.FillPath(firca, yol);
                gr.DrawPath(kalem, yol);
            }
            TextRenderer.DrawText(gr, g.Text, g.Font, new Point(12, 4), BirincilKoyu);
        }

        static GraphicsPath YuvarlakDikdortgen(Rectangle r, int yaricap)
        {
            int cap = yaricap * 2;
            GraphicsPath yol = new GraphicsPath();
            yol.AddArc(r.X, r.Y, cap, cap, 180, 90);
            yol.AddArc(r.Right - cap, r.Y, cap, cap, 270, 90);
            yol.AddArc(r.Right - cap, r.Bottom - cap, cap, cap, 0, 90);
            yol.AddArc(r.X, r.Bottom - cap, cap, cap, 90, 90);
            yol.CloseFigure();
            return yol;
        }

        // ---- Tablolar ----

        static void TabloYap(DataGridView d)
        {
            d.BorderStyle = BorderStyle.None;
            d.BackgroundColor = Kart;
            d.GridColor = Kenar;
            d.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            d.RowHeadersVisible = false;   // soldaki boş gri sütun
            d.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            d.RowTemplate.Height = 30;
            d.DataBindingComplete += SutunGenislikleriniAyarla;

            // Başlık satırı: kendi renklerimizi kullanabilmek için Windows'un başlık stilini kapatıyoruz
            d.EnableHeadersVisualStyles = false;
            d.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            d.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            d.ColumnHeadersHeight = 34;
            d.ColumnHeadersDefaultCellStyle.BackColor = BirincilAcik;
            d.ColumnHeadersDefaultCellStyle.ForeColor = BirincilKoyu;
            d.ColumnHeadersDefaultCellStyle.SelectionBackColor = BirincilAcik;
            d.ColumnHeadersDefaultCellStyle.SelectionForeColor = BirincilKoyu;
            d.ColumnHeadersDefaultCellStyle.Font = KalinYazi;
            d.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);

            d.DefaultCellStyle.BackColor = Kart;
            d.DefaultCellStyle.ForeColor = Metin;
            d.DefaultCellStyle.SelectionBackColor = Secim;
            d.DefaultCellStyle.SelectionForeColor = Metin;
            d.DefaultCellStyle.Font = Yazi;
            d.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            d.AlternatingRowsDefaultCellStyle.BackColor = AlternatifSatir;
        }

        // Tabloya veri bağlandıktan sonra sütunlar belli olur. Sütunlar tabloya sığıyorsa genişliği
        // paylaştırırız (Fill); sığmıyorsa her sütun içeriği kadar genişler ve yatay kaydırma çubuğu çıkar.
        // Böylece çok sütunlu tablolarda başlıklar kesilmez.
        static void SutunGenislikleriniAyarla(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            DataGridView d = (DataGridView)sender;
            int toplam = 0;
            foreach (DataGridViewColumn s in d.Columns)
            {
                toplam += s.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
            }
            d.AutoSizeColumnsMode = toplam < d.ClientSize.Width
                ? DataGridViewAutoSizeColumnsMode.Fill
                : DataGridViewAutoSizeColumnsMode.AllCells;
        }
    }
}
