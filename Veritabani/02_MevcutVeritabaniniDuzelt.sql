/*  Mevcut HastaneProje veritabanını düzelten script.
    Veri silmez; tekrar tekrar çalıştırılabilir (her adım önce "zaten yapılmış mı?" diye kontrol eder).

    Çalıştırma:  SSMS'te açıp Execute (F5)
             ya da: sqlcmd -S .\SQLEXPRESS -E -C -f 65001 -i 02_MevcutVeritabaniniDuzelt.sql
*/
USE HastaneProje;
GO

/* 1) Randevuya şikayet kaydedilecek sütun.
      Hasta panelindeki "Şikayet" kutusu ve doktor panelindeki "Randevu Detay" bu sütunu kullanıyor. */
IF COL_LENGTH('Tbl_Randevular', 'HastaSikayet') IS NULL
    ALTER TABLE Tbl_Randevular ADD HastaSikayet VARCHAR(250) NULL;
GO

/* 2) RandevuDurum boş (NULL) kalmasın: 0 = boş randevu, 1 = alınmış randevu.
      Hasta paneli "RandevuDurum = 0" diye aradığı için NULL olan randevular hiç görünmüyordu. */
UPDATE Tbl_Randevular SET RandevuDurum = 0 WHERE RandevuDurum IS NULL;
IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Randevular_Durum')
    ALTER TABLE Tbl_Randevular ADD CONSTRAINT DF_Randevular_Durum DEFAULT 0 FOR RandevuDurum;
GO

/* 3) Doktor adı + soyadı (15 + 1 + 15 = 31 karakter) randevuya sığsın. Önceden 20 karakterdi. */
ALTER TABLE Tbl_Randevular ALTER COLUMN RandevuDoktor VARCHAR(31) NULL;
GO

/* 4) Her tabloya birincil anahtar (PRIMARY KEY): her satır kendi id'siyle tek olsun. */
IF OBJECT_ID('PK_Hastalar')   IS NULL ALTER TABLE Tbl_Hastalar   ADD CONSTRAINT PK_Hastalar   PRIMARY KEY (Hastaİd);
IF OBJECT_ID('PK_Doktorlar')  IS NULL ALTER TABLE Tblo_Doktorlar ADD CONSTRAINT PK_Doktorlar  PRIMARY KEY (Doktorid);
IF OBJECT_ID('PK_Sekreter')   IS NULL ALTER TABLE Tbl_Sekreter   ADD CONSTRAINT PK_Sekreter   PRIMARY KEY (Sekreterid);
IF OBJECT_ID('PK_Branslar')   IS NULL ALTER TABLE Tbl_Branslar   ADD CONSTRAINT PK_Branslar   PRIMARY KEY (Bransid);
IF OBJECT_ID('PK_Randevular') IS NULL ALTER TABLE Tbl_Randevular ADD CONSTRAINT PK_Randevular PRIMARY KEY (Randevuid);
IF OBJECT_ID('PK_Duyurular')  IS NULL ALTER TABLE Tbl_Duyurular  ADD CONSTRAINT PK_Duyurular  PRIMARY KEY (Duyuruid);
GO

/* 5) Aynı TC kimlik numarasıyla ikinci kayıt açılamasın (UNIQUE = benzersiz). */
IF OBJECT_ID('UQ_Hastalar_TC')  IS NULL ALTER TABLE Tbl_Hastalar   ADD CONSTRAINT UQ_Hastalar_TC  UNIQUE (HastaTC);
IF OBJECT_ID('UQ_Doktorlar_TC') IS NULL ALTER TABLE Tblo_Doktorlar ADD CONSTRAINT UQ_Doktorlar_TC UNIQUE (DoktorTc);
IF OBJECT_ID('UQ_Sekreter_TC')  IS NULL ALTER TABLE Tbl_Sekreter   ADD CONSTRAINT UQ_Sekreter_TC  UNIQUE (SekreterTc);
GO

/* 6) Doktorlarda kullanılıp branş listesinde olmayan branşları listeye ekle
      (yoksa hasta o doktoru hiçbir zaman seçemiyordu). */
INSERT INTO Tbl_Branslar (BransAd)
SELECT DISTINCT d.DoktorBrans
FROM Tblo_Doktorlar d
WHERE d.DoktorBrans IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM Tbl_Branslar b WHERE b.BransAd = d.DoktorBrans);
GO

PRINT 'Veritabanı düzeltmeleri tamamlandı.';
