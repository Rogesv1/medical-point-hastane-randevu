using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Proje_Hastane
{
    // Uygulamanın ortak görünümü.
    // Her form, kurucusunda InitializeComponent()'ten hemen sonra Tema.Uygula(this) çağırır.
    // Renkler ve yazı tipleri tek yerde durduğu için görünümü değiştirmek isteyen
    // sadece bu dosyayı düzenler; 14 formun tasarım dosyasına tek tek dokunmak gerekmez.
    //
    // Formlara yeni alan veya düğme EKLENMEZ; yalnızca var olan kontroller farklı çizilir:
    //  - Düğmeler ve giriş kutuları yuvarlak köşeli çizilir (Paint olayı ile "owner-draw").
    //  - GroupBox'lar gölgeli beyaz kart olur.
    //  - GroupBox'ı olmayan formlarda (giriş, kayıt, yönetim ekranları) içerik beyaz bir kartın üzerinde durur.
    public static class Tema
    {
        // ---- Renkler ----
        public static readonly Color Arkaplan        = ColorTranslator.FromHtml("#EEF3F1");
        public static readonly Color Kart            = Color.White;
        public static readonly Color Birincil        = ColorTranslator.FromHtml("#0F6E56");
        public static readonly Color BirincilKoyu    = ColorTranslator.FromHtml("#085041");
        public static readonly Color BirincilAcik    = ColorTranslator.FromHtml("#E1F5EE");
        public static readonly Color Secim           = ColorTranslator.FromHtml("#BFEBDC");
        public static readonly Color Kenar           = ColorTranslator.FromHtml("#D5DFDB");
        public static readonly Color KoyuKenar       = ColorTranslator.FromHtml("#B4C4BE");
        public static readonly Color Metin           = ColorTranslator.FromHtml("#1F2A27");
        public static readonly Color SoluMetin       = ColorTranslator.FromHtml("#5F6B67");
        public static readonly Color Tehlike         = ColorTranslator.FromHtml("#B3261E");
        public static readonly Color TehlikeAcik     = ColorTranslator.FromHtml("#FCEBEA");
        public static readonly Color AlternatifSatir = ColorTranslator.FromHtml("#F7FAF9");
        public static readonly Color PasifDugme      = ColorTranslator.FromHtml("#C9D3CF");

        // ---- Yazı tipleri ----
        public static readonly Font Yazi       = new Font("Segoe UI", 10F);
        public static readonly Font KalinYazi  = new Font("Segoe UI Semibold", 10F);
        public static readonly Font BaslikYazi = new Font("Segoe UI Semibold", 18F);
        public static readonly Font KucukKalinYazi = new Font("Segoe UI Semibold", 8.5F);

        const int KoseYaricapi = 8;

        // Hangi düğme hangi türde çizilecek (birincil / ikincil / tehlike)
        enum DugmeTuru { Birincil, Ikincil, Tehlike }
        static readonly Dictionary<Button, DugmeTuru> dugmeTurleri = new Dictionary<Button, DugmeTuru>();

        // Kenarlığını biz çizdiğimiz giriş kutularının çerçeve alanı (tasarımdaki eski konum ve boyut)
        static readonly Dictionary<Control, Rectangle> kutuCerceveleri = new Dictionary<Control, Rectangle>();

        // İçeriği beyaz kartın üzerinde duran formlar
        static readonly HashSet<Form> kartliFormlar = new HashSet<Form>();

        public static void Uygula(Form form)
        {
            // Form, yerleşimi yazı tipine göre ölçekleyecek şekilde (AutoScaleMode.Font) tasarlandı.
            // Yazı tipini değiştirince WinForms bütün kontrolleri yeniden boyutlandırmasın diye
            // ölçeklemeyi kapatıyoruz. Konumlar ve boyutlar tasarımdaki gibi kalır.
            form.AutoScaleMode = AutoScaleMode.None;
            form.BackColor = Arkaplan;
            form.ForeColor = Metin;
            form.Font = Yazi;
            form.CiftTamponAc();

            // GroupBox'ı ya da formu dolduran (Dock) bir kontrolü olmayan formlarda içerik bir kartın üzerinde durur
            bool grupVar = form.Controls.OfType<GroupBox>().Any();
            bool dolduranVar = form.Controls.Cast<Control>().Any(k => k.Dock != DockStyle.None);
            if (!grupVar && !dolduranVar) kartliFormlar.Add(form);

            Stillendir(form.Controls);
            form.Paint += FormuCiz;
            form.FormClosed += (s, e) => Temizle(form);
        }

        // Form kapanınca sözlüklerde tuttuğumuz kayıtları silelim (bellekte birikmesin)
        static void Temizle(Form form)
        {
            kartliFormlar.Remove(form);
            foreach (Button b in dugmeTurleri.Keys.Where(b => b.FindForm() == form || b.IsDisposed).ToList())
                dugmeTurleri.Remove(b);
            foreach (Control k in kutuCerceveleri.Keys.Where(k => k.FindForm() == form || k.IsDisposed).ToList())
                kutuCerceveleri.Remove(k);
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
            EtiketleriHizala(kontroller);
        }

        // "Ad:", "Şifre:" gibi alan adlarını sağındaki giriş kutusuna hizalar:
        // dikeyde kutunun ortasına, yatayda sağ kenarları kutunun 12 piksel soluna gelecek şekilde.
        // Böylece alan adları düzgün bir sütun oluşturur. Yeni kontrol eklenmez, sadece konum düzeltilir.
        static void EtiketleriHizala(Control.ControlCollection kontroller)
        {
            List<Control> kutular = kontroller.Cast<Control>()
                .Where(k => k is TextBoxBase || k is ComboBox).ToList();

            List<Label> etiketler = kontroller.OfType<Label>()
                .Where(l => !(l is LinkLabel) && !l.Name.ToLower().StartsWith("lbl") && l.Font.Size <= 12)
                .ToList();

            // Önce olası bütün (etiket, kutu) çiftlerini dikey uzaklıklarıyla topla:
            // kutu etiketin sağında olmalı, etiketin ortası kutunun ortasına 24 pikselden yakın olmalı
            // (çok satırlı kutularda kutunun üst kısmına).
            var ciftler = new List<(Label etiket, Control kutu, Rectangle alan, int uzaklik)>();
            foreach (Label l in etiketler)
            {
                int etiketOrtasi = l.Top + l.Height / 2;
                foreach (Control k in kutular)
                {
                    Rectangle r = Cerceve(k);
                    if (r.Left < l.Left) continue;
                    bool cokSatirli = k is TextBoxBase t && t.Multiline;
                    int hedef = cokSatirli ? r.Top + 14 : r.Top + r.Height / 2;
                    int uzaklik = System.Math.Abs(etiketOrtasi - hedef);
                    if (uzaklik < 24) ciftler.Add((l, k, r, uzaklik));
                }
            }

            // En yakın çiftten başlayarak eşleştir; her etiket ve her kutu yalnızca bir kez kullanılır.
            var kullanilanEtiket = new HashSet<Label>();
            var kullanilanKutu = new HashSet<Control>();
            foreach (var c in ciftler.OrderBy(c => c.uzaklik))
            {
                if (kullanilanEtiket.Contains(c.etiket) || kullanilanKutu.Contains(c.kutu)) continue;
                kullanilanEtiket.Add(c.etiket);
                kullanilanKutu.Add(c.kutu);

                bool cokSatirli = c.kutu is TextBoxBase t && t.Multiline;
                c.etiket.AutoSize = true;
                int ust = cokSatirli ? c.alan.Top + 4 : c.alan.Top + (c.alan.Height - c.etiket.PreferredHeight) / 2;
                c.etiket.Location = new Point(c.alan.Left - 12 - c.etiket.PreferredWidth, ust);
            }

            // Eşleşmeyen ama bir kutunun üstüne binen etiket (ör. doktor panelindeki "Şikayet:")
            // kutunun hemen üstüne alınır.
            foreach (Label l in etiketler.Where(l => !kullanilanEtiket.Contains(l)))
            {
                foreach (Control k in kutular)
                {
                    Rectangle r = Cerceve(k);
                    if (r.IntersectsWith(l.Bounds))
                    {
                        l.Location = new Point(r.Left, r.Top - l.Height - 4);
                        break;
                    }
                }
            }
        }

        // Giriş kutusunun görünen çerçevesi (kenarlığını biz çiziyorsak eski alanı)
        static Rectangle Cerceve(Control k) =>
            kutuCerceveleri.ContainsKey(k) ? kutuCerceveleri[k] : k.Bounds;

        // Bir kontrolün arkasında görünen zemin rengi (yuvarlak köşelerin dışını bu renkle boyarız)
        static Color Zemin(Control k)
        {
            Control ebeveyn = k.Parent;
            if (ebeveyn is GroupBox) return Kart;
            if (ebeveyn is Form f && kartliFormlar.Contains(f)) return Kart;
            return ebeveyn != null ? ebeveyn.BackColor : Arkaplan;
        }

        // ================= Form ve kart çizimi =================

        // Formun zeminine: kartın gölgesi + beyaz kart, GroupBox gölgeleri ve giriş kutusu kenarlıkları
        static void FormuCiz(object sender, PaintEventArgs e)
        {
            Form form = (Form)sender;
            Graphics gr = e.Graphics;
            gr.SmoothingMode = SmoothingMode.AntiAlias;

            if (kartliFormlar.Contains(form))
            {
                Rectangle alan = IcerikAlani(form);
                GolgeCiz(gr, alan, 14);
                using (GraphicsPath yol = YuvarlakDikdortgen(alan, 14))
                using (SolidBrush firca = new SolidBrush(Kart))
                using (Pen kalem = new Pen(Kenar))
                {
                    gr.FillPath(firca, yol);
                    gr.DrawPath(kalem, yol);
                }
            }

            foreach (GroupBox g in form.Controls.OfType<GroupBox>())
            {
                GolgeCiz(gr, g.Bounds, 10);
            }

            KutuKenarlariniCiz(form, gr);
        }

        // Formdaki bütün kontrolleri içine alan dikdörtgen + kenar boşluğu
        static Rectangle IcerikAlani(Form form)
        {
            Rectangle alan = Rectangle.Empty;
            foreach (Control k in form.Controls)
            {
                if (!k.Visible) continue;
                Rectangle r = kutuCerceveleri.ContainsKey(k) ? kutuCerceveleri[k] : k.Bounds;
                alan = alan.IsEmpty ? r : Rectangle.Union(alan, r);
            }
            alan.Inflate(36, 30);
            // Formun dışına taşmasın
            return Rectangle.Intersect(alan, new Rectangle(12, 12, form.ClientSize.Width - 24, form.ClientSize.Height - 24));
        }

        // Yumuşak gölge: birkaç kat yarı saydam, aşağı kaydırılmış yuvarlak dikdörtgen
        static void GolgeCiz(Graphics gr, Rectangle r, int yaricap)
        {
            for (int i = 1; i <= 4; i++)
            {
                Rectangle g = new Rectangle(r.X - i + 1, r.Y + i, r.Width + 2 * i - 2, r.Height + i);
                using (GraphicsPath yol = YuvarlakDikdortgen(g, yaricap + i))
                using (SolidBrush firca = new SolidBrush(Color.FromArgb(10, 8, 60, 45)))
                {
                    gr.FillPath(firca, yol);
                }
            }
        }

        // ---- Kart görünümlü GroupBox ----

        static void KartYap(GroupBox g)
        {
            g.BackColor = Kart;          // içindeki etiketler de beyaz zemini alır
            g.ForeColor = BirincilKoyu;
            g.Font = KalinYazi;
            // Üst boşluk: kutuyu dolduran (Dock = Fill) tablolar başlık yazısının altında başlasın.
            // DİKKAT: Padding'i sadece dolduran bir kontrol varsa değiştiriyoruz. WinForms, üste sabitlenmiş
            // (varsayılan) kontrolleri Padding'e göre konumlar; her GroupBox'ta değiştirince içindeki
            // bütün etiket ve düğmeler 11 piksel aşağı kayıyordu.
            if (g.Controls.Cast<Control>().Any(k => k.Dock != DockStyle.None))
                g.Padding = new Padding(8, 14, 8, 10);
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

            // Başlık ve altında ince yeşil vurgu çizgisi.
            // İçerik kutunun üstüne çok yakınsa (ör. "Hızlı Erişim") başlığı yukarı alır, çizgiyi çizmeyiz.
            Size yaziBoyutu = TextRenderer.MeasureText(g.Text, g.Font);
            int enUstKontrol = g.Controls.Count > 0 ? g.Controls.Cast<Control>().Min(k => k.Top) : int.MaxValue;
            bool sikisik = enUstKontrol < 12 + yaziBoyutu.Height;
            TextRenderer.DrawText(gr, g.Text, g.Font, new Point(14, sikisik ? 1 : 6), BirincilKoyu);
            if (!sikisik)
            {
                using (Pen vurgu = new Pen(Birincil, 2))
                {
                    gr.DrawLine(vurgu, 16, 8 + yaziBoyutu.Height, 16 + 24, 8 + yaziBoyutu.Height);
                }
            }

            KutuKenarlariniCiz(g, gr);
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

        // ================= Etiketler =================

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

        // ================= Giriş kutuları =================

        // Kutunun kendi kenarlığını kaldırıp, tasarımdaki eski alanına yuvarlak köşeli bir kenarlık çiziyoruz.
        // Kenarlığı kutunun ebeveyni (form ya da GroupBox) çizer; kutu odaklanınca kenarlık yeşil olur.
        static void KutuYap(TextBoxBase t)
        {
            Rectangle cerceve = t.Bounds;
            kutuCerceveleri[t] = cerceve;

            t.BorderStyle = BorderStyle.None;
            t.Font = Yazi;
            t.ForeColor = Metin;
            t.BackColor = t.ReadOnly ? AlternatifSatir : Kart;   // salt okunur kutular hafif gri

            if (t.Multiline)
            {
                t.Bounds = new Rectangle(cerceve.X + 8, cerceve.Y + 6, cerceve.Width - 16, cerceve.Height - 12);
            }
            else
            {
                // Kenarlıksız tek satırlık kutu yazı yüksekliği kadardır; eski alanın dikey ortasına yerleştir
                int y = cerceve.Y + (cerceve.Height - t.Height) / 2;
                t.Bounds = new Rectangle(cerceve.X + 8, y, cerceve.Width - 16, t.Height);
            }

            t.Enter += (s, e) => t.Parent?.Invalidate(kutuCerceveleri[t]);
            t.Leave += (s, e) => t.Parent?.Invalidate(kutuCerceveleri[t]);
        }

        // Ebeveynin içindeki her giriş kutusu için yuvarlak kenarlık çizer
        static void KutuKenarlariniCiz(Control ebeveyn, Graphics gr)
        {
            gr.SmoothingMode = SmoothingMode.AntiAlias;
            foreach (Control k in ebeveyn.Controls)
            {
                if (!k.Visible || !kutuCerceveleri.TryGetValue(k, out Rectangle r)) continue;
                Rectangle alan = new Rectangle(r.X, r.Y, r.Width - 1, r.Height - 1);
                using (GraphicsPath yol = YuvarlakDikdortgen(alan, 6))
                using (SolidBrush firca = new SolidBrush(k.BackColor))
                using (Pen kalem = new Pen(k.Focused ? Birincil : KoyuKenar, k.Focused ? 2 : 1))
                {
                    gr.FillPath(firca, yol);
                    gr.DrawPath(kalem, yol);
                }
            }
        }

        static void AcilirKutuYap(ComboBox c)
        {
            // FlatStyle.Flat beyaz kartın üzerinde kenarlıksız görünüyor (kutu kayboluyor);
            // bu yüzden Windows'un standart açılır kutu çizimini koruyoruz.
            c.Font = Yazi;
            c.ForeColor = Metin;
            c.BackColor = Kart;
        }

        // ================= Düğmeler =================

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

        // Ana işlem: Giriş Yap, Kaydet, Ekle, Randevu Al...
        public static void BirincilDugme(Button b) => DugmeTuruAyarla(b, DugmeTuru.Birincil);

        // Yan işlem: Güncelle, Duyurular, Hızlı Erişim düğmeleri...
        public static void IkincilDugme(Button b) => DugmeTuruAyarla(b, DugmeTuru.Ikincil);

        // Geri alınamayan işlem: Sil, Çıkış
        public static void TehlikeDugme(Button b) => DugmeTuruAyarla(b, DugmeTuru.Tehlike);

        static void DugmeTuruAyarla(Button b, DugmeTuru tur)
        {
            if (!dugmeTurleri.ContainsKey(b))
            {
                b.FlatStyle = FlatStyle.Flat;
                b.UseVisualStyleBackColor = false;   // tasarımdaki eski renkleri (ör. MediumSpringGreen) devre dışı bırakır
                b.FlatAppearance.BorderSize = 0;
                b.Font = KalinYazi;
                b.Cursor = Cursors.Hand;
                b.Paint += DugmeCiz;
            }
            dugmeTurleri[b] = tur;
            b.Invalidate();
        }

        // Düğmeyi baştan çizer: zemin, yuvarlak dolgu, kenarlık ve ortalanmış yazı.
        // Fare üzerindeyse ya da basılıysa renk koyulaşır.
        static void DugmeCiz(object sender, PaintEventArgs e)
        {
            Button b = (Button)sender;
            Graphics gr = e.Graphics;
            gr.SmoothingMode = SmoothingMode.AntiAlias;
            gr.Clear(Zemin(b));

            bool uzerinde = b.ClientRectangle.Contains(b.PointToClient(Control.MousePosition));
            bool basili = uzerinde && Control.MouseButtons == MouseButtons.Left;

            Color dolgu, kenar, yazi;
            switch (dugmeTurleri[b])
            {
                case DugmeTuru.Ikincil:
                    dolgu = basili ? Secim : uzerinde ? BirincilAcik : Kart;
                    kenar = uzerinde ? Birincil : KoyuKenar;
                    yazi = uzerinde ? BirincilKoyu : Metin;
                    break;
                case DugmeTuru.Tehlike:
                    dolgu = basili ? ColorTranslator.FromHtml("#F6D5D3") : uzerinde ? TehlikeAcik : Kart;
                    kenar = Tehlike;
                    yazi = Tehlike;
                    break;
                default:
                    dolgu = basili ? ColorTranslator.FromHtml("#063D32") : uzerinde ? BirincilKoyu : Birincil;
                    kenar = dolgu;
                    yazi = Color.White;
                    break;
            }
            if (!b.Enabled) { dolgu = PasifDugme; kenar = PasifDugme; yazi = Color.White; }

            Rectangle alan = new Rectangle(0, 0, b.Width - 1, b.Height - 1);
            using (GraphicsPath yol = YuvarlakDikdortgen(alan, KoseYaricapi))
            using (SolidBrush firca = new SolidBrush(dolgu))
            using (Pen kalem = new Pen(kenar))
            {
                gr.FillPath(firca, yol);
                gr.DrawPath(kalem, yol);
            }
            // Yazı düğmeye sığmıyorsa (küçük düğmeler) bir punto küçük yazı tipiyle çiz
            Font yaziTipi = b.Font;
            if (TextRenderer.MeasureText(b.Text, yaziTipi).Width > b.Width - 10) yaziTipi = KucukKalinYazi;
            TextRenderer.DrawText(gr, b.Text, yaziTipi, b.ClientRectangle, yazi,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        // ================= Tablolar =================

        static void TabloYap(DataGridView d)
        {
            d.BorderStyle = BorderStyle.None;
            d.BackgroundColor = Kart;
            d.GridColor = Kenar;
            d.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            d.RowHeadersVisible = false;   // soldaki boş gri sütun
            d.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            d.RowTemplate.Height = 32;
            d.DataBindingComplete += SutunGenislikleriniAyarla;

            // Başlık satırı: kendi renklerimizi kullanabilmek için Windows'un başlık stilini kapatıyoruz
            d.EnableHeadersVisualStyles = false;
            d.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            d.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            d.ColumnHeadersHeight = 36;
            d.ColumnHeadersDefaultCellStyle.BackColor = BirincilAcik;
            d.ColumnHeadersDefaultCellStyle.ForeColor = BirincilKoyu;
            d.ColumnHeadersDefaultCellStyle.SelectionBackColor = BirincilAcik;
            d.ColumnHeadersDefaultCellStyle.SelectionForeColor = BirincilKoyu;
            d.ColumnHeadersDefaultCellStyle.Font = KalinYazi;
            d.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);

            d.DefaultCellStyle.BackColor = Kart;
            d.DefaultCellStyle.ForeColor = Metin;
            d.DefaultCellStyle.SelectionBackColor = Secim;
            d.DefaultCellStyle.SelectionForeColor = Metin;
            d.DefaultCellStyle.Font = Yazi;
            d.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
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

        // Form.DoubleBuffered "protected" olduğu için dışarıdan yansıma (reflection) ile açıyoruz.
        // Çift tamponlama, kendi çizdiğimiz kart ve gölgelerin titremesini önler.
        static void CiftTamponAc(this Control k)
        {
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(k, true, null);
        }
    }
}
