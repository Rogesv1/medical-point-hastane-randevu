# Yapılan Düzeltmeler ve Öğrenilenler

Bu dosya, projenin ilk sürümünde bulunan hataları ve nasıl düzeltildiklerini anlatır.
Her madde **Sorun → Neden → Çözüm → Kavram** düzenindedir.
Kodda ilgili yerlerde de açıklama satırları (yorumlar) bulunur.

---

## 1. Altyapı

### 1.1 Veritabanı bağlantıları kapanmıyordu
- **Sorun:** Her sorgudan sonra `bgl.baglanti().Close()` yazılıydı ama bağlantılar açık kalıyordu.
- **Neden:** `baglanti()` metodu her çağrıldığında **yeni** bir bağlantı açar. `bgl.baglanti().Close()`
  satırı yeni bir bağlantı açıp hemen onu kapatıyordu; sorguda kullanılan bağlantıya hiç dokunmuyordu.
- **Çözüm:** Her bağlantı `using` bloğuna alındı:
  ```csharp
  using (SqlConnection baglanti = bgl.baglanti())
  using (SqlCommand komut = new SqlCommand("...", baglanti))
  {
      ...
  }   // blok bitince bağlantı ve komut otomatik kapanır — arada hata çıksa bile
  ```
- **Kavram:** `IDisposable` ve `using`. Dosya, bağlantı gibi kaynakları açan nesneler işi bitince kapatılmalıdır.

### 1.2 Bağlantı metni koda gömülüydü
- **Sorun:** `Data Source=ROG\SQLEXPRESS` doğrudan kodda yazıyordu. Proje başka bir bilgisayarda çalışmaz.
- **Çözüm:** Bağlantı metni `App.config` dosyasına taşındı ve `ConfigurationManager` ile okunuyor.
  Sunucu adı `.\SQLEXPRESS` (= "bu bilgisayardaki SQLEXPRESS") yapıldı.
- **Kavram:** Ayarları koddan ayırmak (configuration). Kodu değiştirmeden ortam değiştirilebilir.

### 1.3 Pencere kapatılınca uygulama arka planda çalışmaya devam ediyordu
- **Sorun:** Bir pencere açılınca önceki `Hide()` ile gizleniyor, bir daha gösterilmiyordu.
  Son pencere kapatılsa bile ilk pencere gizli olarak açık kaldığı için program kapanmıyordu (Görev Yöneticisi'nde kalıyordu).
- **Çözüm:** "Açılan pencere kapanınca onu açan pencere geri gelsin" kalıbı:
  ```csharp
  fr.FormClosed += (s, args) => this.Show();
  fr.Show();
  this.Hide();
  ```
  Zincirin başı olan rol seçim ekranı kapatılınca uygulama tamamen kapanır.
- **Kavram:** Olaylar (events) ve lambda ifadeleri (`(s, args) => ...`).

### 1.4 Beklenmeyen hatada program çöküyordu
- **Çözüm:** `Program.cs` içine `Application.ThreadException` olayı eklendi. Hata olursa program kapanmak yerine
  anlaşılır bir mesaj gösterir.

---

## 2. Veritabanı (`Veritabani/02_MevcutVeritabaniniDuzelt.sql`)

| Sorun | Çözüm |
|---|---|
| Şikayet kaydedilecek sütun yoktu | `Tbl_Randevular.HastaSikayet` eklendi |
| `RandevuDurum` NULL kalıyordu, hasta paneli `RandevuDurum=0` aradığı için yeni randevular hiç görünmüyordu | Varsayılan değer `0` (DEFAULT), mevcut NULL'lar 0 yapıldı |
| `RandevuDoktor` 20 karakterdi, ad (15) + soyad (15) sığmıyordu | 31 karaktere çıkarıldı |
| Hiçbir tabloda birincil anahtar yoktu | Her tabloya `PRIMARY KEY` |
| Aynı TC ile birden fazla kayıt açılabiliyordu | TC sütunlarına `UNIQUE` kısıtı |
| Bir doktorun branşı branş listesinde yoktu, hasta o doktoru seçemiyordu | Eksik branş listeye eklendi |

- **Kavram:** Veri bütünlüğü. Kuralları sadece kodda değil veritabanında da tanımlamak, hatalı verinin
  hangi yoldan gelirse gelsin girmesini engeller.

---

## 3. Hasta tarafı

### 3.1 "Randevu Al" düğmesi çalışmıyordu
- **Neden:** Düğmeye bağlı `button1_Click` metodunun içi boştu.
- **Çözüm:** Aktif randevular tablosunda tıklanan satırın numarası seçiliyor, düğmeye basınca randevu hastaya atanıyor:
  ```sql
  update Tbl_Randevular set RandevuDurum=1, HastaTc=@p1, HastaSikayet=@p2
  where Randevuid=@p3 and RandevuDurum=0
  ```
  `and RandevuDurum=0` şartı sayesinde aynı randevuyu iki kişi alamaz. `ExecuteNonQuery()` 0 döndürürse
  "bu randevu alınmış" mesajı gösterilir.

### 3.2 Bilgi düzenlemede şifre ile cinsiyet yer değiştirmişti
- **Neden:** Sütunlar sıra numarasıyla okunuyordu: `dr[5]` aslında şifre, `dr[6]` cinsiyetti.
- **Çözüm:** Sütunlar adıyla okunuyor: `dr["HastaSifre"]`, `dr["HastaCinsiyet"]`.
- **Kavram:** Sıra numarası yerine ad kullanmak hem okunaklı hem de tabloya sütun eklendiğinde bozulmaz.

### 3.3 Kayıt formu
- Boş alanlarla kayıt yapılabiliyordu → tüm alanlar kontrol ediliyor (`MaskCompleted`: TC'nin 11 hanesi de dolu mu?).
- Cinsiyet seçilmezse program çöküyordu (`SelectedItem` null iken `.ToString()`) → önce kontrol ediliyor.
- Aynı TC ile ikinci kayıt engelleniyor.
- Kayıt mesajında şifre gösteriliyordu → kaldırıldı.

### 3.4 SQL injection riski
- **Sorun:** `"Select * From Tbl_Randevular where HastaTC=" + tc` gibi değerler sorgu metnine ekleniyordu.
- **Neden önemli:** Kullanıcıdan gelen bir değer sorgunun parçası olursa, kötü niyetli biri sorguyu değiştirebilir.
- **Çözüm:** Her yerde parametre kullanılıyor: `where HastaTc=@p1` + `Parameters.AddWithValue("@p1", tc)`.

---

## 4. Doktor tarafı

- **Doktor girişi çalışmıyordu:** "Giriş Yap" düğmesine hiçbir kod bağlı değildi → giriş kodu yazıldı.
- **Doktor paneli boştu:** Formda hiç kod yoktu, bilgiler "NUll" olarak kalıyordu → doktorun bilgileri ve
  sadece kendine ait randevular listeleniyor; satıra tıklayınca şikayet gösteriliyor.
- **Bilgi Düzenle / Duyurular / Çıkış** düğmeleri bağlandı.
- **Doktor adı değişince randevuları kayboluyordu:** Randevular doktoru "Ad Soyad" yazısıyla tutuyor.
  Ad güncellenince randevulardaki ad da güncelleniyor. İki işlem bir **transaction** içinde:
  ```csharp
  using (SqlTransaction islem = baglanti.BeginTransaction())
  {
      ... doktoru güncelle ...
      ... randevulardaki adı güncelle ...
      islem.Commit();   // ikisi de başarılıysa kalıcı yap
  }   // Commit'e ulaşılamazsa (hata) hiçbir değişiklik kaydedilmez
  ```
- **Kavram:** Transaction — "ya hepsi ya hiçbiri". Veriler yarım güncellenip tutarsız kalmaz.

---

## 5. Sekreter, doktor yönetimi ve branş yönetimi

### 5.1 Branş Sil ve Güncelle her basışta çöküyordu
- **Neden:** `new SqlCommand("delete From Tbl_Branslar where Bransid=@p1")` — komuta **bağlantı verilmemişti**.
  Bağlantısı olmayan komut çalıştırılamaz (`InvalidOperationException`).
- **Çözüm:** Komut bağlantıyla oluşturuluyor. Ayrıca branşta doktor varsa silme engelleniyor; branş adı
  değişince doktorlar ve randevulardaki branş adı da transaction ile güncelleniyor.

### 5.2 Sekreter panelinde doktor kutusu hiç dolmuyordu
- **Neden:** Doldurma kodu doktor kutusunun **kendi** seçim olayına yazılmıştı. Boş bir kutuda seçim
  yapılamayacağı için kod hiç çalışmıyordu. Ayrıca sorgu tek sütun döndürürken `dr3[1]` okunmaya çalışılıyordu.
- **Çözüm:** Branş seçilince (`cmbBrans_SelectedIndexChanged`) o branşın doktorları listeleniyor.

### 5.3 Sekreterin açtığı randevular hastaya görünmüyordu
- **Neden:** Kaydet, `RandevuDurum` ve `HastaTc` alanlarını hiç yazmıyordu; durum NULL kalıyordu.
- **Çözüm:** Formdaki Durum kutucuğu ve TC alanı da kaydediliyor (TC boşsa NULL). Veritabanında da varsayılan 0.

### 5.4 Çalışmayan düğmeler bağlandı
- Sekreter **Güncelle**: Randevu Listesi'nde bir randevuya çift tıklanınca bilgileri sekreter paneline gelir,
  değiştirip Güncelle'ye basılır.
- Doktor Paneli **Sil** ve **Güncelle**: Silmeden önce onay soruluyor.
- Sekreter girişinde yanlış şifrede mesaj gösteriliyor (eskiden hiçbir şey olmuyordu).

### 5.5 Tablodaki boş satıra tıklayınca çökme
- **Neden:** `DataGridView` en altta boş bir "yeni satır" gösterir; ona tıklanınca `Value` null olur ve
  `.ToString()` hata verir.
- **Çözüm:** `AllowUserToAddRows = false` ve başlık satırı için `if (e.RowIndex < 0) return;`.

---

## 6. Okunabilirlik

- Kontrol adları anlamlı hale getirildi: `button1` → `btnEkle`, `label2` → `lblTc`, `txtSoyad` (branş adı tutuyordu) → `txtBransAd`.
- Pencere başlıkları düzeltildi: "FrmSekreterDetay" → "Sekreter Paneli".
- Şifre kutuları doktor ve sekreter girişinde de gizleniyor.
- İçi boş, hiçbir iş yapmayan olay metotları kaldırıldı.

---

## Nasıl test edildi

- **Derleme:** MSBuild ile hatasız ve uyarısız derleniyor.
- **Veritabanı akışı:** Sekreter randevu açar → hasta görür → randevuyu alır → ikinci kişi aynı randevuyu alamaz →
  doktor şikayeti görür → doktor ve branş adı değişince randevular kaybolmaz → aynı TC ile ikinci kayıt engellenir.
  Test gerçek veritabanında bir transaction içinde çalıştırılıp sonunda geri alındı (ROLLBACK).
- **Arayüz:** Windows UI Automation ile otomatik olarak düğmelere basıldı: üç giriş penceresinin açılıp kapanması,
  boş alanla girişte uyarı mesajı ve ana pencere kapanınca uygulamanın tamamen kapanması doğrulandı.
- **Uçtan uca arayüz testi (58 kontrol):** Formlar açılıp gerçek düğmelere basıldı, çıkan mesaj kutuları
  otomatik okunup kapatıldı ve her adımın sonucu veritabanından kontrol edildi:
  sekreterin doktor/branş eklemesi-güncellemesi-silmesi, randevu oluşturup listeden seçerek güncellemesi, duyuru;
  hastanın kayıt olması, randevuyu görüp alması, aynı randevuyu ikinci kez alamaması, bilgilerini düzenlemesi;
  doktorun kendi randevularını ve şikayeti görmesi, adını değiştirince randevularının kaybolmaması;
  pencereler kapanınca rol ekranına dönülmesi. Test kayıtları sonunda silindi.

## 7. Sonradan bulunan hatalar

- **Doktor bulunamayınca "@p1 parametresi verilmedi" hatası:** Doktor kaydı bulunamazsa ad soyad `null` kalıyordu.
  ADO.NET, değeri `null` olan parametreyi "hiç verilmemiş" sayar. Artık boş metin gönderiliyor (`doktorAdSoyad ?? ""`).
  Veritabanında boş değer saklamak gerekirse `null` yerine `DBNull.Value` kullanılır.
- **Bağlantı ayarı okunamazsa anlaşılmaz hata:** `ConnectionStrings["HastaneProje"]` null dönünce
  "Nesne başvurusu bir nesnenin örneğine ayarlanmadı" hatası çıkıyordu. Artık ne yapılacağını söyleyen bir mesaj var.
  Genel hata penceresi de hatanın hangi form ve metotta çıktığını gösteriyor.
