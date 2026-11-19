# Medical Point — Hastane Randevu Sistemi

C# Windows Forms ve SQL Server ile geliştirilmiş bir hastane randevu ve yönetim uygulaması.
Hasta, doktor ve sekreter olmak üzere üç farklı kullanıcı rolü vardır.

## Özellikler

**Hasta**
- Kayıt olma ve TC kimlik no + şifre ile giriş
- Branş ve doktor seçerek boş randevuları listeleme, şikayet yazarak randevu alma
- Geçmiş randevuları görüntüleme, kişisel bilgileri güncelleme

**Doktor**
- Kendine ait randevuları listeleme, seçilen randevudaki hasta şikayetini görme
- Bilgilerini güncelleme, duyuruları okuma

**Sekreter**
- Randevu oluşturma ve güncelleme (randevu listesinden çift tıklayarak seçme)
- Doktor ekleme / silme / güncelleme
- Branş ekleme / silme / güncelleme
- Duyuru yayınlama

## Kullanılan teknolojiler

- C# / .NET Framework 4.7.2, Windows Forms
- Microsoft SQL Server (Express), ADO.NET (`SqlConnection`, `SqlCommand`, `SqlDataAdapter`)
- Parametreli sorgular, `using` ile bağlantı yönetimi, `SqlTransaction`

## Kurulum

1. **Veritabanını oluşturun.** SQL Server Management Studio'da
   [`Veritabani/01_VeritabaniKurulum.sql`](Veritabani/01_VeritabaniKurulum.sql) dosyasını açıp çalıştırın (F5).
   Tablolar ve deneme verileri oluşur.
2. **Bağlantı ayarını kontrol edin.** [`Proje_Hastane/App.config`](Proje_Hastane/App.config) içindeki
   `Data Source=.\SQLEXPRESS` değeri, bilgisayarınızdaki SQL Server adıyla aynı olmalı.
3. `Proje_Hastane.slnx` dosyasını Visual Studio ile açıp çalıştırın (F5).

Deneme verileriyle giriş bilgileri (şifre hepsinde `1234`):

| Rol      | TC kimlik no  |
|----------|---------------|
| Doktor   | 22222222222   |
| Sekreter | 44444444444   |
| Hasta    | Kayıt ekranından yeni hesap açın |

## Veritabanı

| Tablo            | İçerik |
|------------------|--------|
| `Tbl_Hastalar`   | Hasta bilgileri (TC benzersiz) |
| `Tblo_Doktorlar` | Doktor bilgileri ve branşı |
| `Tbl_Sekreter`   | Sekreter bilgileri |
| `Tbl_Branslar`   | Branş listesi |
| `Tbl_Randevular` | Randevular: tarih, saat, branş, doktor, durum (0 = boş, 1 = alındı), hasta TC, şikayet |
| `Tbl_Duyurular`  | Sekreterin yayınladığı duyurular |

## Geliştirme notları

Projenin ilk sürümünde bulunan hatalar ve nasıl düzeltildikleri
[`DUZELTMELER.md`](DUZELTMELER.md) dosyasında tek tek anlatılmıştır.
Commit geçmişi de aynı sırayı izler.

Bilinen sınırlamalar (ileride geliştirilebilir):
- Şifreler veritabanında düz metin olarak tutuluyor; gerçek bir sistemde hash'lenmeli (ör. PBKDF2 / BCrypt).
- Randevular doktoru ve branşı id yerine ad ile tutuyor; ad değişikliklerinde ilgili kayıtlar transaction ile birlikte güncelleniyor.
- Tarih ve saat metin (`varchar`) olarak saklanıyor; `date` / `time` türleri sıralama ve filtrelemeyi kolaylaştırır.
