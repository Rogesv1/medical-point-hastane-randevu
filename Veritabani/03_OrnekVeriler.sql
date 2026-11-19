-- Uygulamayı denemek için örnek veriler: doktorlar, hastalar, sekreter, duyurular ve randevular.
-- Tekrar tekrar çalıştırılabilir: zaten var olan kayıt (aynı TC / aynı randevu / aynı duyuru) yeniden eklenmez.
-- Örnek kullanıcıların hepsinin şifresi: 1234

USE HastaneProje;
GO
SET NOCOUNT ON;

-- ---------- Branşlar ----------
INSERT INTO Tbl_Branslar (BransAd)
SELECT v.Ad FROM (VALUES (N'Dahiliye'), (N'Göz'), (N'KBB'), (N'Fizik Tedavi'), (N'Kardiyoloji')) v(Ad)
WHERE NOT EXISTS (SELECT 1 FROM Tbl_Branslar b WHERE b.BransAd = v.Ad);

-- ---------- Doktorlar ----------
INSERT INTO Tblo_Doktorlar (DoktorAd, DoktorSoyad, DoktorBrans, DoktorTc, DoktorSifre)
SELECT v.Ad, v.Soyad, v.Brans, v.Tc, '1234'
FROM (VALUES
    (N'Ayşe',  N'Kaya',   N'KBB',          '22222222201'),
    (N'Can',   N'Demir',  N'Fizik Tedavi', '22222222202'),
    (N'Elif',  N'Şahin',  N'Dahiliye',     '22222222203'),
    (N'Kemal', N'Arslan', N'Kardiyoloji',  '22222222204')
) v(Ad, Soyad, Brans, Tc)
WHERE NOT EXISTS (SELECT 1 FROM Tblo_Doktorlar d WHERE d.DoktorTc = v.Tc);

-- ---------- Hastalar ----------
INSERT INTO Tbl_Hastalar (HastaAd, HastaSoyad, HastaTC, HastaTelefon, HastaSifre, HastaCinsiyet)
SELECT v.Ad, v.Soyad, v.Tc, v.Tel, '1234', v.Cinsiyet
FROM (VALUES
    (N'Ahmet',   N'Yılmaz', '44444444401', '5321112233', N'Erkek'),
    (N'Zeynep',  N'Arslan', '44444444402', '5332223344', N'Kadın'),
    (N'Mustafa', N'Koç',    '44444444403', '5343334455', N'Erkek'),
    (N'Selin',   N'Aydın',  '44444444404', '5354445566', N'Kadın')
) v(Ad, Soyad, Tc, Tel, Cinsiyet)
WHERE NOT EXISTS (SELECT 1 FROM Tbl_Hastalar h WHERE h.HastaTC = v.Tc);

-- ---------- Sekreter ----------
INSERT INTO Tbl_Sekreter (SekreterAdSoyad, SekreterTc, SekreterSifre)
SELECT N'Hakan Yurt', '55555555501', '1234'
WHERE NOT EXISTS (SELECT 1 FROM Tbl_Sekreter WHERE SekreterTc = '55555555501');

-- ---------- Duyurular ----------
INSERT INTO Tbl_Duyurular (Duyuru)
SELECT v.Metin FROM (VALUES
    (N'Grip aşısı randevuları açıldı. Sekreterlikten bilgi alabilirsiniz.'),
    (N'Cuma günü poliklinikler 16:00''da kapanacaktır.'),
    (N'Kardiyoloji polikliniği yeni katına (2. kat) taşındı.'),
    (N'Hastane otoparkı bakım nedeniyle hafta sonu kapalıdır.')
) v(Metin)
WHERE NOT EXISTS (SELECT 1 FROM Tbl_Duyurular d WHERE d.Duyuru = v.Metin);

-- ---------- Randevular ----------
-- Durum 0 = boş (hastalar alabilir), 1 = alınmış.
-- Doktor adı, uygulamanın kullandığı biçimde: DoktorAd + ' ' + DoktorSoyad
INSERT INTO Tbl_Randevular (RandevuTarih, RandevuSaat, RandevuBrans, RandevuDoktor, RandevuDurum, HastaTc, HastaSikayet)
SELECT v.Tarih, v.Saat, v.Brans, v.Doktor, v.Durum, v.Tc, v.Sikayet
FROM (VALUES
    -- Geçmiş (alınmış) randevular
    ('04.11.2026', '09:30', N'Dahiliye',     N'Elif Şahin',     1, '44444444401', N'Mide ağrısı ve hazımsızlık'),
    ('06.11.2026', '11:00', N'Göz',          N'Tuna Çınar',     1, '44444444402', N'Uzağı bulanık görme'),
    ('10.11.2026', '14:00', N'Kardiyoloji',  N'Kemal Arslan',   1, '44444444403', N'Merdiven çıkarken nefes darlığı'),
    ('12.11.2026', '10:30', N'Dahiliye',     N'Mehmet YücedAĞ', 1, '11111111111', N'Halsizlik, kan tahlili kontrolü'),
    -- İleri tarihli, alınmış randevular
    ('24.11.2026', '09:00', N'KBB',          N'Ayşe Kaya',      1, '44444444404', N'Boğaz ağrısı ve ateş'),
    ('24.11.2026', '10:00', N'Fizik Tedavi', N'Can Demir',      1, '44444444401', N'Bel ağrısı'),
    ('25.11.2026', '11:30', N'Dahiliye',     N'Mehmet YücedAĞ', 1, '44444444402', N'Baş dönmesi'),
    ('26.11.2026', '13:30', N'Kardiyoloji',  N'Kemal Arslan',   1, '11111111111', N'Çarpıntı'),
    -- Boş randevular (hasta panelinde "Aktif Randevular" listesinde görünür)
    ('25.11.2026', '09:00', N'Dahiliye',     N'Mehmet YücedAĞ', 0, NULL, NULL),
    ('25.11.2026', '10:00', N'Dahiliye',     N'Mehmet YücedAĞ', 0, NULL, NULL),
    ('25.11.2026', '14:00', N'Dahiliye',     N'Elif Şahin',     0, NULL, NULL),
    ('26.11.2026', '09:30', N'Göz',          N'Tuna Çınar',     0, NULL, NULL),
    ('26.11.2026', '10:30', N'Göz',          N'Akın  Yıldız',   0, NULL, NULL),
    ('27.11.2026', '11:00', N'KBB',          N'Ayşe Kaya',      0, NULL, NULL),
    ('27.11.2026', '15:00', N'Fizik Tedavi', N'Can Demir',      0, NULL, NULL),
    ('28.11.2026', '10:00', N'Kardiyoloji',  N'Kemal Arslan',   0, NULL, NULL)
) v(Tarih, Saat, Brans, Doktor, Durum, Tc, Sikayet)
WHERE NOT EXISTS (
    SELECT 1 FROM Tbl_Randevular r
    WHERE r.RandevuTarih = v.Tarih AND r.RandevuSaat = v.Saat AND r.RandevuDoktor = v.Doktor);

PRINT 'Örnek veriler hazır.';
