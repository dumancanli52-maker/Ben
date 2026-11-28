// YENİ SİSTEM · ADIM 1: PUAN TABLOSU
// Kaynak: maç sonuçları arşivi (mac_sonuclari.csv). Her maç için tablo, o maçtan ÖNCEKİ maçlarla kurulur (geleceğe bakmaz).
// Sezon: ligin maçları geriye doğru okunur; 25 günden uzun bir aradan geriye ancak aradan önceki ve sonraki takım listesi aynıysa geçilir
// (kış arası, Dünya Kupası arası aynı sezon; düşen / çıkan takım değiştiyse yeni sezon).
// Bu adım karar vermez (OYNA yok); yalnız tabloyu kurar.
// C# 5 (PowerShell 5.1 Add-Type ile derlenir).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace YeniSistem
{
    public sealed class Mac
    {
        public string Id, EvId, DepId, Ev, Dep, Lig;
        public DateTime Zaman;
        public int GolEv, GolDep;
    }

    public sealed class Satir
    {
        public string TakimId, Ad;
        public int O, G, B, M, A, Y;
        public int P { get { return 3 * G + B; } }
        public int Av { get { return A - Y; } }
        public int Sira;
    }

    public sealed class Tablo
    {
        public string Lig;
        public DateTime Baslangic, Kesim;
        public int MacSayisi;
        public List<Satir> Satirlar = new List<Satir>();
        public Dictionary<string, Satir> Takim = new Dictionary<string, Satir>();
        // tablonun güvenilirliği: takımların oynadığı maç sayıları birbirine yakın mı (en çok - en az)
        public int MacFarki;
    }

    public static class Adim1
    {
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        public const int UzunAraGun = 25;

        // Arşiv: lig -> maçlar (eskiden yeniye). Aynı maç iki kez yazılmışsa (aynı dakika, aynı iki takım) bir kez sayılır.
        public static Dictionary<string, List<Mac>> ArsivOku(string yol)
        {
            Dictionary<string, List<Mac>> lig = new Dictionary<string, List<Mac>>();
            HashSet<string> gorulen = new HashSet<string>();
            string[] satirlar = File.ReadAllLines(yol, Encoding.UTF8);
            if (satirlar.Length == 0) return lig;
            string[] bas = satirlar[0].TrimStart('﻿').Split(';');
            int iId = Array.IndexOf(bas, "mac_id"), iZ = Array.IndexOf(bas, "zaman"), iE = Array.IndexOf(bas, "ev_id"), iD = Array.IndexOf(bas, "dep_id");
            int iEa = Array.IndexOf(bas, "ev"), iDa = Array.IndexOf(bas, "dep"), iGe = Array.IndexOf(bas, "gol_ev"), iGd = Array.IndexOf(bas, "gol_dep");
            int iL = Array.IndexOf(bas, "lig"), iU = Array.IndexOf(bas, "ulke");
            for (int k = 1; k < satirlar.Length; k++)
            {
                string[] c = satirlar[k].Split(';');
                if (c.Length < bas.Length) continue;
                int ge, gd; DateTime z;
                if (!int.TryParse(c[iGe], out ge) || !int.TryParse(c[iGd], out gd)) continue;
                if (!DateTime.TryParseExact(c[iZ].Length >= 16 ? c[iZ].Substring(0, 16) : c[iZ], "yyyy-MM-dd HH:mm", Inv, DateTimeStyles.None, out z)) continue;
                if (c[iE].Length == 0 || c[iD].Length == 0 || c[iE] == c[iD]) continue;
                string anahtar = c[iZ].Substring(0, Math.Min(16, c[iZ].Length)) + "|" + c[iE] + "|" + c[iD];
                if (!gorulen.Add(anahtar)) continue;
                Mac m = new Mac();
                m.Id = c[iId]; m.Zaman = z; m.EvId = c[iE]; m.DepId = c[iD]; m.Ev = c[iEa]; m.Dep = c[iDa];
                m.GolEv = ge; m.GolDep = gd; m.Lig = c[iU] + " | " + c[iL];
                List<Mac> l; if (!lig.TryGetValue(m.Lig, out l)) { l = new List<Mac>(); lig[m.Lig] = l; }
                l.Add(m);
            }
            foreach (List<Mac> l in lig.Values) l.Sort(delegate (Mac a, Mac b) { return a.Zaman.CompareTo(b.Zaman); });
            return lig;
        }

        static HashSet<string> Takimlar(List<Mac> l, int bas, int son)
        {
            HashSet<string> t = new HashSet<string>();
            for (int i = bas; i < son; i++) { t.Add(l[i].EvId); t.Add(l[i].DepId); }
            return t;
        }

        // kesim anından önceki maçlardan bu sezonun ilk maçının sırası (l eskiden yeniye sıralı; son = kesimden önceki maç sayısı)
        public static int SezonBasi(List<Mac> l, int son)
        {
            if (son <= 0) return 0;
            int bas = son - 1;
            while (bas > 0)
            {
                double ara = (l[bas].Zaman - l[bas - 1].Zaman).TotalDays;
                if (ara > UzunAraGun)
                {
                    // aradan sonraki ilk 40 maçın takımları ile aradan önceki son 40 maçın takımları aynı mı
                    HashSet<string> sonra = Takimlar(l, bas, Math.Min(son, bas + 40));
                    HashSet<string> once = Takimlar(l, Math.Max(0, bas - 40), bas);
                    if (!sonra.SetEquals(once)) break;
                }
                bas--;
            }
            return bas;
        }

        // ligin kesim anından ÖNCEKİ maçlarıyla bu sezonun tablosu
        public static Tablo Kur(List<Mac> l, DateTime kesim)
        {
            int son = 0; while (son < l.Count && l[son].Zaman < kesim) son++;
            Tablo t = new Tablo(); t.Kesim = kesim; if (l.Count > 0) t.Lig = l[0].Lig;
            if (son == 0) return t;
            int bas = SezonBasi(l, son);
            t.Baslangic = l[bas].Zaman; t.MacSayisi = son - bas;
            for (int i = bas; i < son; i++)
            {
                Mac m = l[i];
                Satir e = Al(t, m.EvId, m.Ev), d = Al(t, m.DepId, m.Dep);
                e.O++; d.O++; e.A += m.GolEv; e.Y += m.GolDep; d.A += m.GolDep; d.Y += m.GolEv;
                if (m.GolEv > m.GolDep) { e.G++; d.M++; } else if (m.GolEv < m.GolDep) { d.G++; e.M++; } else { e.B++; d.B++; }
            }
            t.Satirlar.Sort(delegate (Satir a, Satir b)
            {
                int c = b.P.CompareTo(a.P); if (c != 0) return c;
                c = b.Av.CompareTo(a.Av); if (c != 0) return c;
                c = b.A.CompareTo(a.A); if (c != 0) return c;
                return string.CompareOrdinal(a.Ad, b.Ad);
            });
            int mn = int.MaxValue, mx = 0;
            for (int i = 0; i < t.Satirlar.Count; i++) { t.Satirlar[i].Sira = i + 1; mn = Math.Min(mn, t.Satirlar[i].O); mx = Math.Max(mx, t.Satirlar[i].O); }
            t.MacFarki = mx - mn;
            return t;
        }

        static Satir Al(Tablo t, string id, string ad)
        {
            Satir s; if (t.Takim.TryGetValue(id, out s)) return s;
            s = new Satir(); s.TakimId = id; s.Ad = ad; t.Takim[id] = s; t.Satirlar.Add(s); return s;
        }

        // denetim için: tablonun satırlarının (O, G, B, M, A, Y, P) sıralı listesi (takım adından bağımsız karşılaştırma)
        public static string Imza(IEnumerable<int[]> satirlar)
        {
            List<string> l = new List<string>();
            foreach (int[] s in satirlar) l.Add(string.Join(",", Array.ConvertAll(s, delegate (int x) { return x.ToString(Inv); })));
            l.Sort(string.CompareOrdinal);
            return string.Join(" ", l.ToArray());
        }
        public static string Imza(Tablo t)
        {
            List<int[]> l = new List<int[]>();
            foreach (Satir s in t.Satirlar) l.Add(new int[] { s.O, s.G, s.B, s.M, s.A, s.Y, s.P });
            return Imza(l);
        }
    }
}
