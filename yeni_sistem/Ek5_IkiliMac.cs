// YENİ SİSTEM · EK 5: İKİLİ MAÇLAR (maç karnesi) · öğren-sına ile alındı (OKUBENI.md, Ek 5)
// İki takımın maçtan ÖNCEKİ aralarındaki son 5 maçı (her turnuva), en az 2 maç:
//   gol farkı ortalaması (ev sahibinin gözünden) -> ev beklenen golü x e^(0,1 x fark x d), deplasman x e^(-0,1 x fark x d)
//   toplam gol ortalaması -> iki beklenen gol x e^(0,1 x (ikili toplam - beklenen toplam) x d)
//   d = n / (n + 3) (az ikili maç daha az sayılır)
// Kullanım: arşiv okununca bir kez Ek5.Hazirla(arsiv); Adım 4 hazırsa kendisi uygular. C# 5.
using System;
using System.Collections.Generic;

namespace YeniSistem
{
    public static class Ek5
    {
        public const double FarkAgirlik = 0.1, ToplamAgirlik = 0.1, Denge = 3.0;
        public const int EnAz = 2, Son = 5;
        static Dictionary<string, List<Mac>> cift;   // "küçükId|büyükId" -> maçlar (eskiden yeniye)

        public static bool Hazir { get { return cift != null; } }

        public static void Hazirla(Dictionary<string, List<Mac>> arsiv)
        {
            Dictionary<string, List<Mac>> d = new Dictionary<string, List<Mac>>();
            foreach (List<Mac> l in arsiv.Values)
                foreach (Mac m in l)
                {
                    string k = Anahtar(m.EvId, m.DepId);
                    List<Mac> a; if (!d.TryGetValue(k, out a)) { a = new List<Mac>(); d[k] = a; }
                    a.Add(m);
                }
            foreach (List<Mac> a in d.Values) a.Sort(delegate (Mac x, Mac y) { return x.Zaman.CompareTo(y.Zaman); });
            cift = d;
        }

        static string Anahtar(string a, string b) { return string.CompareOrdinal(a, b) < 0 ? a + "|" + b : b + "|" + a; }

        // ev sahibinin gözünden son ikili maçlar: (n, gol farkı ort., toplam gol ort.)
        public static int Ozet(string evId, string depId, DateTime kesim, out double fark, out double toplam)
        {
            fark = 0; toplam = 0;
            List<Mac> a; if (cift == null || !cift.TryGetValue(Anahtar(evId, depId), out a)) return 0;
            int n = 0;
            for (int i = a.Count - 1; i >= 0 && n < Son; i--)
            {
                Mac m = a[i]; if (m.Zaman >= kesim) continue;
                int at = m.EvId == evId ? m.GolEv : m.GolDep, ye = m.EvId == evId ? m.GolDep : m.GolEv;
                fark += at - ye; toplam += at + ye; n++;
            }
            if (n > 0) { fark /= n; toplam /= n; }
            return n;
        }

        // Adım 4'ün beklenen gollerine uygulanır; uygulandıysa true
        public static bool Uygula(string evId, string depId, DateTime kesim, ref double lamEv, ref double lamDep)
        {
            double fark, toplam; int n = Ozet(evId, depId, kesim, out fark, out toplam);
            if (n < EnAz) return false;
            double d = n / (n + Denge), t = Math.Exp(ToplamAgirlik * (toplam - (lamEv + lamDep)) * d);
            lamEv *= Math.Exp(FarkAgirlik * fark * d) * t;
            lamDep *= Math.Exp(-FarkAgirlik * fark * d) * t;
            return true;
        }
    }
}
