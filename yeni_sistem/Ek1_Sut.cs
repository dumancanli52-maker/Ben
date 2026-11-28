// YENİ SİSTEM · EK 1: İSABETLİ ŞUT (aday, sınanıyor)
// Sezon içinde maçtan ÖNCEKİ ve şut bilgisi olan maçlardan: her takımın maç başı attığı / yediği isabetli şut, lig ortalaması.
// Kullanım (sınamada ağırlık bulunur): ev beklenen golü x (ev şut hücumu x dep şut savunması)^w, deplasman tersi.
// C# 5.
using System;
using System.Collections.Generic;

namespace YeniSistem
{
    public sealed class SutOzet
    {
        public int EvN, DepN;              // şut bilgisi olan maç sayısı
        public int EvAt, EvYe, DepAt, DepYe; // isabetli şut toplamları
        public int LigN, LigIsb;           // ligde şutlu takım-maç sayısı ve isabetli şut toplamı
    }

    public static class Ek1
    {
        public static SutOzet Ozet(List<Mac> l, DateTime kesim, string evId, string depId)
        {
            SutOzet o = new SutOzet();
            int son = 0; while (son < l.Count && l[son].Zaman < kesim) son++;
            if (son == 0) return o;
            int bas = Adim1.SezonBasi(l, son);
            HashSet<string> grup = Adim1.Grup(l, bas, son, evId);
            for (int i = bas; i < son; i++)
            {
                Mac m = l[i];
                if (m.IsbEv < 0 || m.IsbDep < 0) continue;
                if (grup.Count > 0 && !grup.Contains(m.EvId)) continue;
                o.LigN += 2; o.LigIsb += m.IsbEv + m.IsbDep;
                if (m.EvId == evId) { o.EvN++; o.EvAt += m.IsbEv; o.EvYe += m.IsbDep; }
                if (m.DepId == evId) { o.EvN++; o.EvAt += m.IsbDep; o.EvYe += m.IsbEv; }
                if (m.EvId == depId) { o.DepN++; o.DepAt += m.IsbEv; o.DepYe += m.IsbDep; }
                if (m.DepId == depId) { o.DepN++; o.DepAt += m.IsbDep; o.DepYe += m.IsbEv; }
            }
            return o;
        }
    }
}
