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
        public static readonly string[] Anahtar = new string[] { "15U", "25U", "25A", "35A" };
        public static readonly string[] Ad = new string[] { "1,5 Üst", "2,5 Üst", "2,5 Alt", "3,5 Alt" };
        public static readonly double[] Alt = new double[] { 0.60, 0.66, 0.60, 0.62 };
        public static readonly double[] Ust = new double[] { 0.72, 0.86, 0.72, 0.74 };
        public static readonly int[] GecmisMac = new int[] { 79, 178, 290, 263 };
        public static readonly double[] GecmisTuttu = new double[] { 0.684, 0.640, 0.628, 0.646 };

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
