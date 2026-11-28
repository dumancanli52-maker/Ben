// YENİ SİSTEM · ADIM 4: MAÇ SONUCU (MS 1 / X / 2)
// Adım 1-2'nin tablolarından (maçtan ÖNCEKİ maçlar, takımın grubu) iki takımın beklenen golü, oradan skor dağılımı ve MS yüzdeleri.
// Hesap kullanıcının Excel motorunun çekirdeği (MOTOR_MANTIGI.md, bölüm 2-3), kadro çarpanı ve gerçek skor karışımı olmadan, Adım 4b ayarıyla:
//   lig ortalaması: takım başı maç başı gol (genel), iç saha maç başı ev golü, dış saha maç başı deplasman golü
//   oran = (gol + 12 x lig ort.) / (maç + 12)            (az maçlı takım lig ortalamasına çekilir)
//   genel taban (ev) = oran(ev attı) x oran(dep yedi) / lig ort.     iç/dış taban (ev) = oran(ev iç attı) x oran(dep dış yedi) / iç saha ort.
//   ev beklenen golü = %50 genel + %50 iç/dış ; deplasman tersi (dış saha ortalamasıyla)
//   skor dağılımı: iki bağımsız Poisson (0-10 gol); MS 1 = ev önde biten skorların toplamı
// Bu adım karar vermez (OYNA yok). Geçerlilik: iki takımın da en az EnAzMac maçı olmalı. C# 5.
using System;
using System.Collections.Generic;

namespace YeniSistem
{
    public sealed class MsTahmin
    {
        public bool Gecerli; public string Neden = "";
        public double LamEv, LamDep;          // beklenen gol
        public double P1, PX, P2;             // MS yüzdeleri (0-1)
        public double[,] Skor;                // skor dağılımı [ev, dep] 0-10 (Adım 5-8 için)
        public int MacEv, MacDep;             // takımların tablodaki maç sayısı
    }

    public static class Adim4
    {
        // Ayar (Adım 4b, 28.11.2026): tek günlerden öğrenilip çift günlerde, çift günlerden öğrenilip tek günlerde sınandı.
        // Excel'deki 12 maç / %50 / çarpansız hâl fazla temkinliydi (MS 1 %64 dediğinde gerçek %77). İki yarıda da MS ve gol hatası düştü.
        public const double Denge = 6.0;       // Excel CF2 = 12
        public const double IcDisPay = 0.25;   // Excel CG2 = 0,5
        public const double EvCarpan = 1.05;   // ev sahibinin beklenen golü
        public const double DepCarpan = 0.925; // deplasmanın beklenen golü
        // Adım 5 ayarı (28.11.2026, aynı öğren-sına): maçın toplam gol tahmini lig ortalamasına yaklaştırılır ve hafifçe artırılır
        // (Excel hâli toplamda fazla emindi: 0-1 gol %44 dediğinde gerçek %33). Ev / deplasman oranı korunur; MS hatası değişmedi.
        public const double ToplamCarpan = 1.02;
        public const double ToplamYakinlik = 0.4;   // 1 = değişmez, 0 = her maç lig ortalaması
        public const int EnAzMac = 3;
        public const int MaxGol = 10;

        public static MsTahmin Hesapla(List<Mac> l, DateTime kesim, string evId, string depId)
        {
            MsTahmin t = new MsTahmin();
            IcDis x = Adim2.Kur(l, kesim, evId);
            Satir eg, dg, ei, dd;
            x.Genel.Takim.TryGetValue(evId, out eg); x.Genel.Takim.TryGetValue(depId, out dg);
            x.Ic.Takim.TryGetValue(evId, out ei); x.Dis.Takim.TryGetValue(depId, out dd);
            if (eg == null || dg == null) { t.Neden = "takım tabloda yok"; return t; }
            t.MacEv = eg.O; t.MacDep = dg.O;
            if (eg.O < EnAzMac || dg.O < EnAzMac) { t.Neden = "az maç"; return t; }
            if (!x.Tutarli) { t.Neden = "tablo tutarsız"; return t; }
            // lig ortalamaları
            int gm = 0, ga = 0, icM = 0, icG = 0, disM = 0, disG = 0;
            foreach (Satir s in x.Genel.Satirlar) { gm += s.O; ga += s.A; }
            foreach (Satir s in x.Ic.Satirlar) { icM += s.O; icG += s.A; }
            foreach (Satir s in x.Dis.Satirlar) { disM += s.O; disG += s.A; }
            if (gm == 0 || icM == 0 || disM == 0 || ga == 0 || icG == 0 || disG == 0) { t.Neden = "lig ortalaması yok"; return t; }
            double ort = (double)ga / gm, icOrt = (double)icG / icM, disOrt = (double)disG / disM;
            // genel taban
            double evGenel = Oran(eg.A, eg.O, ort) * Oran(dg.Y, dg.O, ort) / ort;
            double depGenel = Oran(dg.A, dg.O, ort) * Oran(eg.Y, eg.O, ort) / ort;
            // iç / dış taban (iç saha maçı yoksa 0 maçla: lig ortalamasına eşit)
            double evIc = Oran(ei == null ? 0 : ei.A, ei == null ? 0 : ei.O, icOrt) * Oran(dd == null ? 0 : dd.Y, dd == null ? 0 : dd.O, icOrt) / icOrt;
            double depDis = Oran(dd == null ? 0 : dd.A, dd == null ? 0 : dd.O, disOrt) * Oran(ei == null ? 0 : ei.Y, ei == null ? 0 : ei.O, disOrt) / disOrt;
            t.LamEv = EvCarpan * ((1 - IcDisPay) * evGenel + IcDisPay * evIc);
            t.LamDep = DepCarpan * ((1 - IcDisPay) * depGenel + IcDisPay * depDis);
            double ligTop = 2.0 * ort, top = t.LamEv + t.LamDep;
            if (top > 0) { double r = ToplamCarpan * (ligTop + ToplamYakinlik * (top - ligTop)) / top; t.LamEv *= r; t.LamDep *= r; }
            t.Skor = Dagilim(t.LamEv, t.LamDep);
            for (int i = 0; i <= MaxGol; i++) for (int j = 0; j <= MaxGol; j++) { double p = t.Skor[i, j]; if (i > j) t.P1 += p; else if (i == j) t.PX += p; else t.P2 += p; }
            t.Gecerli = true;
            return t;
        }

        static double Oran(int gol, int mac, double ort) { return (gol + Denge * ort) / (mac + Denge); }

        public static double[,] Dagilim(double le, double ld)
        {
            double[] pe = Poisson(le), pd = Poisson(ld);
            double[,] s = new double[MaxGol + 1, MaxGol + 1]; double top = 0;
            for (int i = 0; i <= MaxGol; i++) for (int j = 0; j <= MaxGol; j++) { s[i, j] = pe[i] * pd[j]; top += s[i, j]; }
            for (int i = 0; i <= MaxGol; i++) for (int j = 0; j <= MaxGol; j++) s[i, j] /= top;
            return s;
        }

        static double[] Poisson(double l)
        {
            double[] p = new double[MaxGol + 1]; p[0] = Math.Exp(-l);
            for (int k = 1; k <= MaxGol; k++) p[k] = p[k - 1] * l / k;
            return p;
        }
    }
}
