// YENİ SİSTEM · EK 7: ÇAPRAZ (kullanıcının yöntemi, sıfırdan) · öğren-sına ile alındı (OKUBENI.md, Ek 7)
// Her takımın maçtan ÖNCEKİ son 15 maçı (her turnuva, genel) ve evde / deplasmanda son 7 maçı (iç-dış):
//   çapraz ev golü = (ev attı + dep yedi) / 2 ; çapraz dep golü = (dep attı + ev yedi) / 2   (genel ve iç-dış ayrı, yarı yarıya)
//   beklenen gol = zincirin beklenen golü^(1 - 0,2) x çapraz^0,2
// Şart: iki takımın en az 8 genel ve 4 iç-dış maçı. Arşiv okununca bir kez Ek7.Hazirla(arsiv). C# 5.
using System;
using System.Collections.Generic;

namespace YeniSistem
{
    public static class Ek7
    {
        public const double Pay = 0.2, IcDisPay = 0.5;
        public const int Genel = 15, IcDis = 7, EnAzGenel = 8, EnAzIcDis = 4;
        sealed class TM { public DateTime Z; public bool Ev; public int At, Ye; }
        static Dictionary<string, List<TM>> takim;

        public static bool Hazir { get { return takim != null; } }

        public static void Hazirla(Dictionary<string, List<Mac>> arsiv)
        {
            Dictionary<string, List<TM>> d = new Dictionary<string, List<TM>>();
            foreach (List<Mac> l in arsiv.Values)
                foreach (Mac m in l)
                {
                    Ekle(d, m.EvId, new TM { Z = m.Zaman, Ev = true, At = m.GolEv, Ye = m.GolDep });
                    Ekle(d, m.DepId, new TM { Z = m.Zaman, Ev = false, At = m.GolDep, Ye = m.GolEv });
                }
            foreach (List<TM> a in d.Values) a.Sort(delegate (TM x, TM y) { return x.Z.CompareTo(y.Z); });
            takim = d;
        }

        static void Ekle(Dictionary<string, List<TM>> d, string id, TM t)
        {
            List<TM> a; if (!d.TryGetValue(id, out a)) { a = new List<TM>(); d[id] = a; }
            a.Add(t);
        }

        // takımın kesimden önceki son n maçının attığı / yediği ortalaması (ev: null = hepsi, true = iç saha, false = dış saha)
        static int Son(string id, DateTime kesim, int n, bool? ev, out double at, out double ye)
        {
            at = 0; ye = 0; int k = 0; List<TM> a;
            if (takim == null || !takim.TryGetValue(id, out a)) return 0;
            for (int i = a.Count - 1; i >= 0 && k < n; i--)
            {
                TM t = a[i]; if (t.Z >= kesim) continue;
                if (ev.HasValue && t.Ev != ev.Value) continue;
                at += t.At; ye += t.Ye; k++;
            }
            if (k > 0) { at /= k; ye /= k; }
            return k;
        }

        public static bool Uygula(string evId, string depId, DateTime kesim, ref double lamEv, ref double lamDep)
        {
            double eA, eY, dA, dY, ieA, ieY, ddA, ddY;
            int n1 = Son(evId, kesim, Genel, null, out eA, out eY), n2 = Son(depId, kesim, Genel, null, out dA, out dY);
            int n3 = Son(evId, kesim, IcDis, true, out ieA, out ieY), n4 = Son(depId, kesim, IcDis, false, out ddA, out ddY);
            if (n1 < EnAzGenel || n2 < EnAzGenel || n3 < EnAzIcDis || n4 < EnAzIcDis) return false;
            double cEv = (1 - IcDisPay) * (eA + dY) / 2.0 + IcDisPay * (ieA + ddY) / 2.0;
            double cDep = (1 - IcDisPay) * (dA + eY) / 2.0 + IcDisPay * (ddA + ieY) / 2.0;
            cEv = Math.Max(cEv, 0.1); cDep = Math.Max(cDep, 0.1);
            lamEv = Math.Pow(lamEv, 1 - Pay) * Math.Pow(cEv, Pay);
            lamDep = Math.Pow(lamDep, 1 - Pay) * Math.Pow(cDep, Pay);
            return true;
        }
    }
}
