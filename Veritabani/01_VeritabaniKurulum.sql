/*  HastaneProje veritabanını SIFIRDAN kurar (projeyi ilk kez çalıştıracaklar için).
    Mevcut bir HastaneProje veritabanınız varsa bunun yerine 02_MevcutVeritabaniniDuzelt.sql'i çalıştırın.

    Çalıştırma:  SSMS'te açıp Execute (F5)
             ya da: sqlcmd -S .\SQLEXPRESS -E -C -f 65001 -i 01_VeritabaniKurulum.sql
*/
IF DB_ID('HastaneProje') IS NULL
    CREATE DATABASE HastaneProje;
GO
USE HastaneProje;
GO

CREATE TABLE Tbl_Branslar (
    Bransid  TINYINT IDENTITY(1,1) CONSTRAINT PK_Branslar PRIMARY KEY,
    BransAd  VARCHAR(30)
);

CREATE TABLE Tblo_Doktorlar (
    Doktorid     TINYINT IDENTITY(1,1) CONSTRAINT PK_Doktorlar PRIMARY KEY,
    DoktorAd     VARCHAR(15),
    DoktorSoyad  VARCHAR(15),
    DoktorBrans  VARCHAR(30),
    DoktorTc     CHAR(11) CONSTRAINT UQ_Doktorlar_TC UNIQUE,
    DoktorSifre  VARCHAR(15)
);

CREATE TABLE Tbl_Hastalar (
    Hastaİd        SMALLINT IDENTITY(1,1) CONSTRAINT PK_Hastalar PRIMARY KEY,
    HastaAd        VARCHAR(15),
    HastaSoyad     VARCHAR(15),
    HastaTC        CHAR(11) CONSTRAINT UQ_Hastalar_TC UNIQUE,
    HastaTelefon   VARCHAR(15),
    HastaSifre     VARCHAR(15),
    HastaCinsiyet  VARCHAR(5)
);

CREATE TABLE Tbl_Sekreter (
    Sekreterid       TINYINT IDENTITY(1,1) CONSTRAINT PK_Sekreter PRIMARY KEY,
    SekreterAdSoyad  VARCHAR(20),
    SekreterTc       CHAR(11) CONSTRAINT UQ_Sekreter_TC UNIQUE,
    SekreterSifre    VARCHAR(15)
);

CREATE TABLE Tbl_Randevular (
    Randevuid      INT IDENTITY(1,1) CONSTRAINT PK_Randevular PRIMARY KEY,
    RandevuTarih   VARCHAR(10),
    RandevuSaat    VARCHAR(5),
    RandevuBrans   VARCHAR(30),
    RandevuDoktor  VARCHAR(31),
    RandevuDurum   BIT CONSTRAINT DF_Randevular_Durum DEFAULT 0,
    HastaTc        CHAR(11),
    HastaSikayet   VARCHAR(250)
);

CREATE TABLE Tbl_Duyurular (
    Duyuruid  SMALLINT IDENTITY(1,1) CONSTRAINT PK_Duyurular PRIMARY KEY,
    Duyuru    VARCHAR(200)
);
GO

/* Deneme için örnek veriler. Şifreler sadece örnektir. */
INSERT INTO Tbl_Branslar (BransAd) VALUES ('Dahiliye'), ('Göz'), ('KBB'), ('Kardiyoloji');
INSERT INTO Tblo_Doktorlar (DoktorAd, DoktorSoyad, DoktorBrans, DoktorTc, DoktorSifre)
    VALUES ('Ali', 'Demir', 'Dahiliye', '22222222222', '1234'),
           ('Ayşe', 'Kaya', 'Göz', '33333333333', '1234');
INSERT INTO Tbl_Sekreter (SekreterAdSoyad, SekreterTc, SekreterSifre)
    VALUES ('Zeynep Şahin', '44444444444', '1234');
INSERT INTO Tbl_Randevular (RandevuTarih, RandevuSaat, RandevuBrans, RandevuDoktor)
    VALUES ('25.11.2026', '10:00', 'Dahiliye', 'Ali Demir'),
           ('25.11.2026', '10:30', 'Dahiliye', 'Ali Demir'),
           ('26.11.2026', '09:00', 'Göz', 'Ayşe Kaya');
INSERT INTO Tbl_Duyurular (Duyuru) VALUES ('Sisteme hoş geldiniz.');
GO

PRINT 'HastaneProje veritabanı kuruldu.';
