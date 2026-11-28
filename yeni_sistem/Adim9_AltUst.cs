// YENİ SİSTEM · ADIM 9-12: ARALIKTAN ÇIKAN ALT / ÜST BAHİSLERİ
// Adım 4'ün skor dağılımından 1,5 / 2,5 / 3,5 / 4,5 Üst yüzdeleri (Alt = 1 - Üst). Kullanıcının şeması:
//   0-1 gol -> 1,5 Alt, 2,5 Alt     2-3 gol -> 1,5 Üst, 2,5 Üst     4-5 gol -> 2,5 Üst, 3,5 Üst     6+ gol -> 3,5 Üst, 4,5 Üst
// En olası aralık (Adım 5) ve o aralığın bahisleri de verilir. Bu adım karar vermez (OYNA yok). C# 5.
using System;
using System.Collections.Generic;

namespace YeniSistem
{
    public sealed class AltUst
    {
        public bool Gecerli; public string Neden = "";
        public double Ust15, Ust25, Ust35, Ust45;   // Üst yüzdeleri (0-1); Alt = 1 - Üst
        public string EnOlasiAralik = "";           // "0-1", "2-3", "4-5", "6+"
        public string[] AralikBahisleri = new string[0];
    }

    public static class Adim9
    {
        public static AltUst Hesapla(MsTahmin ms, GolAraligi g)
        {
            AltUst a = new AltUst();
            if (ms == null || !ms.Gecerli || ms.Skor == null || g == null || !g.Gecerli) { a.Neden = ms == null ? "maç sonucu yok" : ms.Neden; return a; }
            int n = ms.Skor.GetLength(0);
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    double p = ms.Skor[i, j]; int t = i + j;
                    if (t >= 2) a.Ust15 += p; if (t >= 3) a.Ust25 += p; if (t >= 4) a.Ust35 += p; if (t >= 5) a.Ust45 += p;
                }
            double[] ar = new double[] { g.G01, g.G23, g.G45, g.G6 };
            int k = 0; for (int i = 1; i < 4; i++) if (ar[i] > ar[k]) k = i;
            a.EnOlasiAralik = new string[] { "0-1", "2-3", "4-5", "6+" }[k];
            a.AralikBahisleri = k == 0 ? new string[] { "1,5 Alt", "2,5 Alt" } : k == 1 ? new string[] { "1,5 Üst", "2,5 Üst" } : k == 2 ? new string[] { "2,5 Üst", "3,5 Üst" } : new string[] { "3,5 Üst", "4,5 Üst" };
            a.Gecerli = true;
            return a;
        }
    }
}
