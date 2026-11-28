// YENİ SİSTEM · ADIM 3: İLK YARI TABLOSU
// Adım 1'in ligi ve sezonuyla, maçtan ÖNCEKİ maçların ilk yarı skorlarından: genel, iç saha (takımın evindeki maçlar), dış saha.
// Her takım için: ilk yarısı bilinen maç sayısı, ilk yarıda önde / berabere / geride, ilk yarıda attığı / yediği gol,
// ilk yarısı gollü geçen maç (en az 1 gol) ve en az 2 gollü maç sayısı.
// Denetim: önde + berabere + geride = ilk yarısı bilinen maç; ilk yarı golü maç sonu golünü geçmez; iç + dış = genel.
// Bu adım karar vermez (OYNA yok). C# 5.
using System;
using System.Collections.Generic;

namespace YeniSistem
{
    public sealed class IySatir
    {
        public string TakimId, Ad;
        public int O;        // bütün maçlar (Adım 1 ile aynı)
        public int OIy;      // ilk yarı skoru bilinen maçlar
        public int Onde, Berabere, Geride, A, Y, Gol1, Gol2;
    }

    public sealed class IyTablo
    {
        public Dictionary<string, IySatir> Genel = new Dictionary<string, IySatir>();
        public Dictionary<string, IySatir> Ic = new Dictionary<string, IySatir>();
        public Dictionary<string, IySatir> Dis = new Dictionary<string, IySatir>();
        public int Mac, MacIy;   // sezonda kesime kadar maç; ilk yarısı bilinen
        public bool Tutarli;
    }

    public static class Adim3
    {
        public static IyTablo Kur(List<Mac> l, DateTime kesim) { return Kur(l, kesim, null); }
        public static IyTablo Kur(List<Mac> l, DateTime kesim, string takim)   // takim: gruplu ligde o takımın grubu
        {
            IyTablo t = new IyTablo(); t.Tutarli = true;
            int son = 0; while (son < l.Count && l[son].Zaman < kesim) son++;
            if (son == 0) return t;
            int bas = Adim1.SezonBasi(l, son);
            HashSet<string> grup = takim == null ? null : Adim1.Grup(l, bas, son, takim);
            for (int i = bas; i < son; i++)
            {
                Mac m = l[i];
                if (grup != null && !grup.Contains(m.EvId)) continue;
                t.Mac++;
                bool iy = m.IyEv >= 0 && m.IyDep >= 0; if (iy) t.MacIy++;
                Ekle(Al(t.Genel, m.EvId, m.Ev), m, true, iy); Ekle(Al(t.Ic, m.EvId, m.Ev), m, true, iy);
                Ekle(Al(t.Genel, m.DepId, m.Dep), m, false, iy); Ekle(Al(t.Dis, m.DepId, m.Dep), m, false, iy);
                if (iy && (m.IyEv > m.GolEv || m.IyDep > m.GolDep)) t.Tutarli = false;
            }
            foreach (IySatir g in t.Genel.Values)
            {
                if (g.Onde + g.Berabere + g.Geride != g.OIy || g.OIy > g.O) t.Tutarli = false;
                IySatir a, b; t.Ic.TryGetValue(g.TakimId, out a); t.Dis.TryGetValue(g.TakimId, out b);
                int o = (a == null ? 0 : a.OIy) + (b == null ? 0 : b.OIy), aa = (a == null ? 0 : a.A) + (b == null ? 0 : b.A), on = (a == null ? 0 : a.Onde) + (b == null ? 0 : b.Onde);
                if (o != g.OIy || aa != g.A || on != g.Onde) t.Tutarli = false;
            }
            return t;
        }

        static void Ekle(IySatir s, Mac m, bool ev, bool iy)
        {
            s.O++;
            if (!iy) return;
            int at = ev ? m.IyEv : m.IyDep, ye = ev ? m.IyDep : m.IyEv;
            s.OIy++; s.A += at; s.Y += ye;
            if (at > ye) s.Onde++; else if (at < ye) s.Geride++; else s.Berabere++;
            if (at + ye >= 1) s.Gol1++;
            if (at + ye >= 2) s.Gol2++;
        }

        static IySatir Al(Dictionary<string, IySatir> d, string id, string ad)
        {
            IySatir s; if (d.TryGetValue(id, out s)) return s;
            s = new IySatir(); s.TakimId = id; s.Ad = ad; d[id] = s; return s;
        }
    }
}
