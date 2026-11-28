// YENİ SİSTEM · KARAR (kullanıcının 9. şartı: her şart bizim yüzde dilimimize girince OYNA)
// Şartlar: bahsin yüzdesi kendi diliminde + sitenin oranı 1,30 ve üstü + Adım 4 geçerli (tablo kontrolü Tahmin.ps1'de).
// Maç başı tek bahis: birden çok bahis uyarsa yüzdesi kendi dilim başına en uzak olan.
// Dilimler (28.11.2026): 4.610 oranlı geçmiş maçta öğrenildi (en az 60 maç, en az %62 tutmuş dilim; 12+ puan genişlik).
// Kural önce tek / çift gün öğren-sına ile sınandı: aynı maçlarda eski program %59,3, yeni %62,9 (OKUBENI.md, Karar adımı). C# 5.
using System;
using System.Collections.Generic;

namespace YeniSistem
{
    public sealed class KararSonuc
    {
        public bool Oyna; public string Bahis = "", Neden = "";
        public double Yuzde, Oran;
    }

    public static class Karar
    {
        public const double OranEnAz = 1.30;
        // bahis anahtarı, ekranda adı, dilim alt, dilim üst (yüzde, 0-1), geçmişte maç, geçmişte tuttu
        public static readonly string[] Anahtar = new string[] { "25U", "25A" };
        public static readonly string[] Ad = new string[] { "2,5 Üst", "2,5 Alt" };
        public static readonly double[] Alt = new double[] { 0.66, 0.60 };
        public static readonly double[] Ust = new double[] { 0.86, 0.72 };
        public static readonly int[] GecmisMac = new int[] { 178, 290 };
        public static readonly double[] GecmisTuttu = new double[] { 0.640, 0.628 };
        // 1,5 Üst (dilim %60-72, 79 maç %68,4) de kullanıcının kararıyla oynanmaz: site oranı ortalama 1,11, dilimdeyken bile yalnız %31'i 1,30+; yüzdesi yalnız bilgi.
        // 3,5 Alt (dilim %62-74, 263 maç %64,6) kullanıcının kararıyla oynanacak bahis değil (oranı çoğunlukla 1,30 altı); yüzdesi yalnız bilgi.
        // Destek şartı olarak da sınandı: 2,5 Alt dilimindeki 290 maçın hepsinde 3,5 Alt zaten %78+ (aynı dağılım): ek bilgi yok.

        // ekrandaki "Nasıl karar veriyoruz" yazısı: buradaki sabitlerden üretilir (kullanıcının 6. şartı)
        public static string Anlatim()
        {
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            System.Globalization.CultureInfo tr = System.Globalization.CultureInfo.GetCultureInfo("tr-TR");
            s.AppendLine("Zincir (her adım bir öncekinden destek alır):");
            s.AppendLine("  1. Puan tablosu: maçtan önceki bu sezon maçlarından (takımın grubu).");
            s.AppendLine("  2. İç-dış tablo: yalnız iç saha ve yalnız dış saha maçları.");
            s.AppendLine("  3. İlk yarı tablosu.");
            s.AppendLine("  4. Beklenen gol ve maç sonucu (1 / X / 2): tablo atma-yeme oranları x lig ortalaması; az maçlı takım lig ortalamasına çekilir;");
            s.AppendLine("     destekler: isabetli şut (veri varsa), ikili maçlar (son " + Ek5.Son + "), çapraz (son " + Ek7.Genel + " maç ve iç-dış son " + Ek7.IcDis + " maç).");
            s.AppendLine("  5. Gol aralıkları (0-1 / 2-3 / 4-5 / 6+): aynı skor dağılımından.");
            s.AppendLine("  6. Alt / Üst yüzdeleri: aynı skor dağılımından.");
            s.AppendLine("  Tablo kontrolü: iki takımın arşivdeki maç sayısı sitenin tablosuyla en çok 1 farklı olmalı; değilse hesap yapılmaz.");
            s.AppendLine();
            s.AppendLine("OYNA şartı (hepsi birden):");
            for (int i = 0; i < Anahtar.Length; i++)
                s.AppendLine("  " + Ad[i] + ": yüzde %" + Math.Round(Alt[i] * 100) + " ile %" + Math.Round(Ust[i] * 100) + " arasında (geçmişte bu dilimde " + GecmisMac[i] + " maç, %" + (GecmisTuttu[i] * 100).ToString("0.0", tr) + " tuttu)");
            s.AppendLine("  Sitenin oranı " + OranEnAz.ToString("0.00", tr) + " ve üstü (oran yalnız bu süzgeçte kullanılır, yüzdeye karışmaz).");
            s.AppendLine("  Maç başına tek bahis: birden çok bahis uyarsa yüzdesi kendi diliminin alt sınırından en uzak olan.");
            s.AppendLine();
            s.AppendLine("Yalnız bilgi (oynanmaz): MS 1 / X / 2, gol aralıkları, 1,5 Üst, 3,5 Alt.");
            return s.ToString();
        }

        public static double Yuzde(string anahtar, AltUst a)
        {
            switch (anahtar)
            {
                case "15U": return a.Ust15;
                case "25U": return a.Ust25;
                case "25A": return 1.0 - a.Ust25;
                case "35A": return 1.0 - a.Ust35;
            }
            return double.NaN;
        }

        // oran: bahis anahtarı -> sitenin oranı (yoksa anahtar yok)
        public static KararSonuc Ver(MsTahmin ms, AltUst a, Dictionary<string, double> oran)
        {
            KararSonuc k = new KararSonuc();
            if (ms == null || !ms.Gecerli || a == null || !a.Gecerli) { k.Neden = ms == null ? "hesap yok" : ms.Neden; return k; }
            int en = -1; double enPay = -1; bool dilimde = false;
            for (int i = 0; i < Anahtar.Length; i++)
            {
                double p = Yuzde(Anahtar[i], a);
                if (p < Alt[i] || p >= Ust[i]) continue;
                dilimde = true;
                double o; if (oran == null || !oran.TryGetValue(Anahtar[i], out o) || o < OranEnAz) continue;
                double pay = p - Alt[i];
                if (pay > enPay) { enPay = pay; en = i; }
            }
            if (en < 0) { k.Neden = dilimde ? "dilimdeki bahsin oranı 1,30 altı ya da yok" : "hiçbir bahis dilimde değil"; return k; }
            k.Oyna = true; k.Bahis = Ad[en]; k.Yuzde = Yuzde(Anahtar[en], a); k.Oran = oran[Anahtar[en]];
            k.Neden = "dilim %" + Math.Round(Alt[en] * 100) + "-" + Math.Round(Ust[en] * 100) + " (geçmişte " + GecmisMac[en] + " maç, %" + Math.Round(GecmisTuttu[en] * 100) + " tuttu)";
            return k;
        }
    }
}
