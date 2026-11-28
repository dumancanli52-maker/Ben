// YENİ SİSTEM · ADIM 2: İÇ SAHA / DIŞ SAHA TABLOSU
// Adım 1'in sezonu ve maçlarıyla: her takımın yalnız evinde oynadığı maçlardan iç saha tablosu, yalnız deplasmanda oynadıklarından dış saha tablosu.
// Her ikisi de o maçtan ÖNCEKİ maçlarla kurulur. Denetim: her takımda iç saha + dış saha = genel tablo (Adım 1).
// Bu adım karar vermez (OYNA yok). C# 5.
using System;
using System.Collections.Generic;

namespace YeniSistem
{
    public sealed class IcDis
    {
        public Tablo Genel;                 // Adım 1
        public Tablo Ic = new Tablo();      // yalnız iç saha maçları
        public Tablo Dis = new Tablo();     // yalnız dış saha maçları
        public bool Tutarli;                // her takımda iç + dış = genel
    }

    public static class Adim2
    {
        public static IcDis Kur(List<Mac> l, DateTime kesim)
        {
            IcDis s = new IcDis();
            s.Genel = Adim1.Kur(l, kesim);
            s.Ic.Lig = s.Dis.Lig = s.Genel.Lig; s.Ic.Kesim = s.Dis.Kesim = kesim;
            s.Ic.Baslangic = s.Dis.Baslangic = s.Genel.Baslangic;
            int son = 0; while (son < l.Count && l[son].Zaman < kesim) son++;
            if (son == 0) { s.Tutarli = true; return s; }
            int bas = Adim1.SezonBasi(l, son);
            for (int i = bas; i < son; i++)
            {
                Mac m = l[i];
                Satir e = Al(s.Ic, m.EvId, m.Ev), d = Al(s.Dis, m.DepId, m.Dep);
                e.O++; d.O++; e.A += m.GolEv; e.Y += m.GolDep; d.A += m.GolDep; d.Y += m.GolEv;
                if (m.GolEv > m.GolDep) { e.G++; d.M++; } else if (m.GolEv < m.GolDep) { d.G++; e.M++; } else { e.B++; d.B++; }
            }
            s.Ic.MacSayisi = s.Dis.MacSayisi = son - bas;
            Sirala(s.Ic); Sirala(s.Dis);
            // denetim: iç + dış = genel (her takım)
            s.Tutarli = true;
            foreach (Satir g in s.Genel.Satirlar)
            {
                Satir a, b; s.Ic.Takim.TryGetValue(g.TakimId, out a); s.Dis.Takim.TryGetValue(g.TakimId, out b);
                int o = (a == null ? 0 : a.O) + (b == null ? 0 : b.O), gg = (a == null ? 0 : a.G) + (b == null ? 0 : b.G), bb = (a == null ? 0 : a.B) + (b == null ? 0 : b.B);
                int aa = (a == null ? 0 : a.A) + (b == null ? 0 : b.A), yy = (a == null ? 0 : a.Y) + (b == null ? 0 : b.Y);
                if (o != g.O || gg != g.G || bb != g.B || aa != g.A || yy != g.Y) { s.Tutarli = false; break; }
            }
            return s;
        }

        static Satir Al(Tablo t, string id, string ad)
        {
            Satir s; if (t.Takim.TryGetValue(id, out s)) return s;
            s = new Satir(); s.TakimId = id; s.Ad = ad; t.Takim[id] = s; t.Satirlar.Add(s); return s;
        }

        static void Sirala(Tablo t)
        {
            t.Satirlar.Sort(delegate (Satir a, Satir b)
            {
                int c = b.P.CompareTo(a.P); if (c != 0) return c;
                c = b.Av.CompareTo(a.Av); if (c != 0) return c;
                c = b.A.CompareTo(a.A); if (c != 0) return c;
                return string.CompareOrdinal(a.Ad, b.Ad);
            });
            int mn = int.MaxValue, mx = 0;
            for (int i = 0; i < t.Satirlar.Count; i++) { t.Satirlar[i].Sira = i + 1; mn = Math.Min(mn, t.Satirlar[i].O); mx = Math.Max(mx, t.Satirlar[i].O); }
            t.MacFarki = t.Satirlar.Count > 0 ? mx - mn : 0;
        }
    }
}
