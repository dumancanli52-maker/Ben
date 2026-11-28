// YENİ SİSTEM · ADIM 5-8: GOL ARALIKLARI (0-1 / 2-3 / 4-5 / 6+)
// Ayrı model değil: Adım 4'ün skor dağılımından okunur. MS ile gol aralıkları hep aynı hesaptan çıkar, birbirini tutar.
// Toplam gol ayarı (lig ortalamasına yaklaştırma, x 1,02) Adım 4'te yapılır (sınaması OKUBENI.md, Adım 5).
// Bu adım karar vermez (OYNA yok). C# 5.
using System;
using System.Collections.Generic;

namespace YeniSistem
{
    public sealed class GolAraligi
    {
        public bool Gecerli; public string Neden = "";
        public double G01, G23, G45, G6;   // 0-1, 2-3, 4-5, 6 ve üzeri gol (0-1)
        public double BeklenenToplam;       // maçın beklenen toplam golü
    }

    public static class Adim5
    {
        public static GolAraligi Hesapla(MsTahmin ms)
        {
            GolAraligi g = new GolAraligi();
            if (ms == null || !ms.Gecerli || ms.Skor == null) { g.Neden = ms == null ? "maç sonucu yok" : ms.Neden; return g; }
            int n = ms.Skor.GetLength(0);
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    double p = ms.Skor[i, j]; int t = i + j;
                    if (t <= 1) g.G01 += p; else if (t <= 3) g.G23 += p; else if (t <= 5) g.G45 += p; else g.G6 += p;
                }
            g.BeklenenToplam = ms.LamEv + ms.LamDep;
            g.Gecerli = true;
            return g;
        }
    }
}
