// YENİ SİSTEM · SİTE VERİSİ: bülten (günün maçları ve oranları), maç bilgisi, lig tablosu, takım maç listeleri -> arşiv (Adım 1 maçları).
// Saatler Türkiye saati. C# 5.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace YeniSistem
{
    public sealed class BultenMac
    {
        public string Kod, Ev, Dep;
        public DateTime Saat;   // TR
        public Dictionary<string, double> Oran = new Dictionary<string, double>();
    }

    public sealed class MacBilgi
    {
        public string Mid, LigTid, LigAd, Sezon, EvTid, EvAd, DepTid, DepAd;
    }

    public static class SiteVeri
    {
        static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime TrSaat(double ms) { return Epoch.AddMilliseconds(ms).AddHours(3); }

        // bülten: yalnız futbol (TYPE 1); oranlar: MS1 MSX MS2, 15A 15U, 25A 25U, 35A 35U, KGV KGY
        public static List<BultenMac> Bulten(string json)
        {
            List<BultenMac> l = new List<BultenMac>();
            object sg = Json.Al(Json.Oku(json), "sg");
            foreach (object e in Json.Liste(sg, "EA"))
            {
                if (Json.Sayi(e, "TYPE") != 1) continue;
                BultenMac b = new BultenMac();
                b.Kod = Json.Yazi(e, "C"); b.Ev = Json.Yazi(e, "HN"); b.Dep = Json.Yazi(e, "AN");
                double esd = Json.Sayi(e, "ESD"); if (double.IsNaN(esd) || b.Kod.Length == 0) continue;
                b.Saat = TrSaat(esd);
                foreach (object m in Json.Liste(e, "MA"))
                {
                    string mt = Json.Yazi(m, "MTID"); double sov = Json.Sayi(m, "SOV"); string[] ad = null;
                    if (mt == "1") ad = new string[] { "MS1", "MSX", "MS2" };
                    else if (mt == "11" && Math.Abs(sov - 1.5) < 0.01) ad = new string[] { "15A", "15U" };
                    else if (mt == "12" && Math.Abs(sov - 2.5) < 0.01) ad = new string[] { "25A", "25U" };
                    else if (mt == "13" && Math.Abs(sov - 3.5) < 0.01) ad = new string[] { "35A", "35U" };
                    else if (mt == "38") ad = new string[] { "KGV", "KGY" };
                    if (ad == null) continue;
                    foreach (object o in Json.Liste(m, "OCA"))
                    {
                        int n = (int)Json.Sayi(o, "N"); double v = Json.Sayi(o, "O");
                        if (n >= 1 && n <= ad.Length && v > 1.0) b.Oran[ad[n - 1]] = v;
                    }
                }
                l.Add(b);
            }
            return l;
        }

        public static MacBilgi Bilgi(string json)
        {
            object d = Json.Al(Json.Oku(json), "d"); if (d == null) return null;
            object tour = Json.Al(d, "TOUR"); List<object> ht = Json.Liste(d, "HT"), at = Json.Liste(d, "AT");
            if (tour == null || ht.Count == 0 || at.Count == 0) return null;
            MacBilgi b = new MacBilgi();
            b.Mid = Json.Yazi(d, "MID"); b.LigTid = Json.Yazi(tour, "TID"); b.LigAd = Json.Yazi(tour, "N"); b.Sezon = Json.Yazi(tour, "SEA");
            b.EvTid = Json.Yazi(ht[0], "TID"); b.EvAd = Json.Yazi(ht[0], "N"); b.DepTid = Json.Yazi(at[0], "TID"); b.DepAd = Json.Yazi(at[0], "N");
            return b;
        }

        // lig tablosu: takım -> oynadığı maç (genel tablo, bütün gruplar); site tablo vermezse null
        public static Dictionary<string, int> Tablo(string json)
        {
            object j = Json.Oku(json); if (Json.Sayi(j, "sc") != 200) return null;
            Dictionary<string, int> t = new Dictionary<string, int>();
            foreach (object g in Json.Liste(Json.Al(j, "d"), "LTL"))
                foreach (object s in Json.Liste(g, "LTSL"))
                    foreach (object tt in Json.Liste(s, "LTTL"))
                    {
                        if (Json.Sayi(tt, "FT") != 8) continue;
                        foreach (object r in Json.Liste(tt, "LTR")) t[Json.Yazi(r, "TID")] = (int)Json.Sayi(r, "MC");
                    }
            return t.Count > 0 ? t : null;
        }

        // takım maç listesi -> skorlu maçlar arşive. SiteArsivi.ps1 + Adim1.ArsivOku ile aynı kurallar (sınamalar bu kurallarla yapıldı):
        // yalnız FT 5 bölümü, tam bir maç sonu skoru, saat sitenin yazdığı gibi (ilk 16 karakter), aynı maç (kimlik ya da saat+takımlar) bir kez.
        public static int ArsiveEkle(string json, Dictionary<string, List<Mac>> arsiv, HashSet<string> gorulen)
        {
            object j = Json.Oku(json); if (Json.Sayi(j, "sc") != 200) return 0;
            object d = Json.Al(j, "d"); if (d == null) return 0;
            int n = 0;
            foreach (object tl in Json.Liste(d, "TML"))
                foreach (object b in Json.Liste(tl, "TT"))
                {
                    if (Json.Sayi(b, "FT") != 5) continue;
                    foreach (object tm in Json.Liste(b, "TMS"))
                        foreach (object m in Json.Liste(tm, "ML"))
                        {
                            string mid = Json.Yazi(m, "MID"), md = Json.Yazi(m, "MD"); object ht = Json.Al(m, "HT"), at = Json.Al(m, "AT"), lg = Json.Al(m, "LG");
                            if (mid.Length == 0 || ht == null || at == null || lg == null || md.Length < 16 || gorulen.Contains("#" + mid)) continue;
                            int ge = -1, gd = -1, ie = -1, id = -1, nMs = 0, nIy = 0;
                            foreach (object sc in Json.Liste(m, "SC"))
                            {
                                double ty = Json.Sayi(sc, "TY"), obi = Json.Sayi(sc, "OBI"), h = Json.Sayi(sc, "HTS"), a = Json.Sayi(sc, "ATS");
                                if (ty == 2 && obi == 99) { nMs++; if (!double.IsNaN(h) && !double.IsNaN(a)) { ge = (int)h; gd = (int)a; } }
                                if (ty == 1 && obi == 1) { nIy++; if (!double.IsNaN(h) && !double.IsNaN(a)) { ie = (int)h; id = (int)a; } }
                            }
                            if (nMs != 1 || ge < 0 || gd < 0) continue;
                            DateTime z;
                            if (!DateTime.TryParseExact(md.Substring(0, 16).Replace('T', ' '), "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out z)) continue;
                            string evId = Json.Yazi(ht, "TID"), depId = Json.Yazi(at, "TID");
                            if (evId.Length == 0 || depId.Length == 0 || evId == depId) continue;
                            gorulen.Add("#" + mid);
                            if (!gorulen.Add(z.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + "|" + evId + "|" + depId)) continue;
                            Mac x = new Mac();
                            x.Id = mid; x.Zaman = z; x.EvId = evId; x.DepId = depId; x.Ev = Temiz(Json.Yazi(ht, "N")); x.Dep = Temiz(Json.Yazi(at, "N"));
                            x.GolEv = ge; x.GolDep = gd; x.SezonEtiketli = true;
                            if (nIy == 1 && ie >= 0 && id >= 0 && ie <= ge && id <= gd) { x.IyEv = ie; x.IyDep = id; }
                            x.Lig = Json.Yazi(lg, "TID") + " | " + Temiz(Json.Yazi(lg, "N")) + " " + Json.Yazi(m, "SEA");
                            List<Mac> l; if (!arsiv.TryGetValue(x.Lig, out l)) { l = new List<Mac>(); arsiv[x.Lig] = l; }
                            l.Add(x); n++;
                        }
                }
            return n;
        }

        static string Temiz(string s) { return s.Replace(';', ' ').Replace('\r', ' ').Replace('\n', ' '); }

        public static void Sirala(Dictionary<string, List<Mac>> arsiv)
        {
            foreach (List<Mac> l in arsiv.Values) l.Sort(delegate (Mac a, Mac b) { return a.Zaman.CompareTo(b.Zaman); });
        }

        // lig anahtarı: "ligNo | ad sezon"; ad sitede değişebileceği için numara + sezonla bulunur
        public static string LigAnahtari(Dictionary<string, List<Mac>> arsiv, string ligTid, string sezon)
        {
            foreach (string k in arsiv.Keys) if (k.StartsWith(ligTid + " | ", StringComparison.Ordinal) && k.EndsWith(" " + sezon, StringComparison.Ordinal)) return k;
            return null;
        }
    }
}
