// YENİ SİSTEM · KAYIT: kayit/tahminler.csv
// Her güncellemede: başlamamış maçın satırı yenilenir; başlamış maçın satırı donar (sonradan değiştirilmez).
// Sonuç: maçın skoru arşive (takım listelerine) girince satıra yazılır; OYNA satırında bahis tuttu mu yazılır.
// Ayrıca: bütün OYNA kayıtlarının özeti (Sonuçlar sekmesi). C# 5.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace YeniSistem
{
    public sealed class KayitOzet
    {
        public string Bahis = ""; public int Mac, Tuttu, Bekleyen; public double Kazanc;   // Kazanc: 1 birim yatırımla net
        public string MacYazi { get { return Mac.ToString(); } }
        public string TuttuYazi { get { return Mac > 0 ? Tuttu + " (%" + (100.0 * Tuttu / Mac).ToString("0.0", CultureInfo.GetCultureInfo("tr-TR")) + ")" : "-"; } }
        public string KazancYazi { get { return Mac > 0 ? (Kazanc >= 0 ? "+" : "") + Kazanc.ToString("0.00", CultureInfo.GetCultureInfo("tr-TR")) + " birim (%" + (100.0 * Kazanc / Mac).ToString("0.0", CultureInfo.GetCultureInfo("tr-TR")) + ")" : "-"; } }
        public string BekleyenYazi { get { return Bekleyen.ToString(); } }
    }

    public static class Kayit
    {
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        static readonly string[] Bas = new string[] { "kod", "mid", "saat", "lig", "ev", "dep", "durum", "karar", "bahis", "yuzde", "oran", "karar_neden",
            "ms1", "msx", "ms2", "g01", "g23", "g45", "g6", "u15", "u25", "u35", "u45", "bek_ev", "bek_dep", "aralik", "mac_ev", "mac_dep", "site_mac_ev", "site_mac_dep",
            "ek_sut", "ek_ikili", "ek_capraz", "oranlar", "kayit", "skor_ev", "skor_dep", "tuttu" };

        static string D(double v) { return v.ToString("0.#####", Inv); }
        static string T(string s) { return (s ?? "").Replace(';', ',').Replace('\r', ' ').Replace('\n', ' '); }

        static string Satir(Tahmin t)
        {
            List<string> o = new List<string>();
            foreach (KeyValuePair<string, double> kv in t.Oranlar) o.Add(kv.Key + "=" + D(kv.Value));
            string[] c = new string[] { t.Kod, t.Mid, t.Saat.ToString("yyyy-MM-dd HH:mm", Inv), T(t.Lig), T(t.Ev), T(t.Dep), T(t.Durum), t.KararYazi, t.Bahis,
                t.Oyna ? D(t.Yuzde) : "", t.Oyna ? D(t.Oran) : "", T(t.KararNeden),
                D(t.Ms1), D(t.MsX), D(t.Ms2), D(t.G01), D(t.G23), D(t.G45), D(t.G6), D(t.U15), D(t.U25), D(t.U35), D(t.U45), D(t.BekEv), D(t.BekDep), t.Aralik,
                t.MacEv.ToString(), t.MacDep.ToString(), t.SiteMacEv.ToString(), t.SiteMacDep.ToString(), t.Sut ? "1" : "0", t.Ikili ? "1" : "0", t.Capraz ? "1" : "0",
                string.Join("|", o.ToArray()), t.Kayit, t.SkorEv >= 0 ? t.SkorEv.ToString() : "", t.SkorDep >= 0 ? t.SkorDep.ToString() : "", t.Tuttu };
            return string.Join(";", c);
        }

        static double P(string s) { double v; return double.TryParse(s, NumberStyles.Float, Inv, out v) ? v : 0; }
        static int I(string s) { int v; return int.TryParse(s, out v) ? v : -1; }

        public static List<Tahmin> Oku(string yol)
        {
            List<Tahmin> l = new List<Tahmin>();
            if (!File.Exists(yol)) return l;
            string[] sat = File.ReadAllLines(yol, Encoding.UTF8);
            if (sat.Length == 0) return l;
            string[] h = sat[0].TrimStart('\uFEFF').Split(';');
            Dictionary<string, int> ix = new Dictionary<string, int>(); for (int i = 0; i < h.Length; i++) ix[h[i]] = i;
            for (int r = 1; r < sat.Length; r++)
            {
                string[] c = sat[r].Split(';'); if (c.Length < h.Length) continue;
                Func<string, string> g = delegate (string ad) { int i; return ix.TryGetValue(ad, out i) && i < c.Length ? c[i] : ""; };
                Tahmin t = new Tahmin();
                t.Kod = g("kod"); t.Mid = g("mid"); DateTime.TryParseExact(g("saat"), "yyyy-MM-dd HH:mm", Inv, DateTimeStyles.None, out t.Saat);
                t.Lig = g("lig"); t.Ev = g("ev"); t.Dep = g("dep"); t.Durum = g("durum"); t.Oyna = g("karar") == "OYNA"; t.Bahis = g("bahis");
                t.Yuzde = P(g("yuzde")); t.Oran = P(g("oran")); t.KararNeden = g("karar_neden"); t.Hesaplandi = t.Durum == "tamam";
                t.Ms1 = P(g("ms1")); t.MsX = P(g("msx")); t.Ms2 = P(g("ms2")); t.G01 = P(g("g01")); t.G23 = P(g("g23")); t.G45 = P(g("g45")); t.G6 = P(g("g6"));
                t.U15 = P(g("u15")); t.U25 = P(g("u25")); t.U35 = P(g("u35")); t.U45 = P(g("u45")); t.BekEv = P(g("bek_ev")); t.BekDep = P(g("bek_dep")); t.Aralik = g("aralik");
                t.MacEv = Math.Max(0, I(g("mac_ev"))); t.MacDep = Math.Max(0, I(g("mac_dep"))); t.SiteMacEv = Math.Max(0, I(g("site_mac_ev"))); t.SiteMacDep = Math.Max(0, I(g("site_mac_dep")));
                t.Sut = g("ek_sut") == "1"; t.Ikili = g("ek_ikili") == "1"; t.Capraz = g("ek_capraz") == "1";
                foreach (string o in g("oranlar").Split('|')) { string[] q = o.Split('='); if (q.Length == 2) t.Oranlar[q[0]] = P(q[1]); }
                t.Kayit = g("kayit"); t.SkorEv = I(g("skor_ev")); t.SkorDep = I(g("skor_dep")); t.Tuttu = g("tuttu");
                l.Add(t);
            }
            return l;
        }

        public static void Yaz(string yol, List<Tahmin> l)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Join(";", Bas));
            foreach (Tahmin t in l) sb.AppendLine(Satir(t));
            string gecici = yol + ".yeni";
            File.WriteAllText(gecici, sb.ToString(), new UTF8Encoding(true));
            // ekran / telefon o an okuyor olabilir: üç kez denenir
            for (int d = 0; ; d++)
            {
                try { if (File.Exists(yol)) File.Replace(gecici, yol, null); else File.Move(gecici, yol); return; }
                catch (IOException) { if (d >= 2) throw; System.Threading.Thread.Sleep(500 * (d + 1)); }
                catch (UnauthorizedAccessException) { if (d >= 2) throw; System.Threading.Thread.Sleep(500 * (d + 1)); }
            }
        }

        public static bool BahisTuttu(string bahis, int ev, int dep)
        {
            int top = ev + dep;
            switch (bahis)
            {
                case "2,5 Üst": return top >= 3;
                case "2,5 Alt": return top <= 2;
                case "1,5 Üst": return top >= 2;
                case "3,5 Alt": return top <= 3;
            }
            return false;
        }

        // yeni tahminleri kayda işler (başlamamış maçlar), sonuçları yazar
        public static void Isle(string yol, List<Tahmin> yeni, Dictionary<string, List<Mac>> arsiv, DateTime simdi)
        {
            List<Tahmin> eski = Oku(yol);
            Dictionary<string, int> sira = new Dictionary<string, int>();
            for (int i = 0; i < eski.Count; i++) sira[eski[i].Kod] = i;
            foreach (Tahmin t in yeni)
            {
                int i;
                if (sira.TryGetValue(t.Kod, out i)) { if (eski[i].Saat > simdi) eski[i] = t; }   // başlamış maçın satırı donar
                else { sira[t.Kod] = eski.Count; eski.Add(t); }
            }
            // sonuçlar: arşivde maç kimliğiyle
            Dictionary<string, Mac> mid = new Dictionary<string, Mac>();
            foreach (List<Mac> l in arsiv.Values) foreach (Mac m in l) if (!string.IsNullOrEmpty(m.Id)) mid[m.Id] = m;
            foreach (Tahmin t in eski)
            {
                Mac m;
                if (t.SkorEv >= 0 || t.Mid.Length == 0 || !mid.TryGetValue(t.Mid, out m)) continue;
                t.SkorEv = m.GolEv; t.SkorDep = m.GolDep;
                if (t.Oyna) t.Tuttu = BahisTuttu(t.Bahis, t.SkorEv, t.SkorDep) ? "evet" : "hayır";
            }
            eski.Sort(delegate (Tahmin a, Tahmin b) { return a.Saat.CompareTo(b.Saat); });
            Yaz(yol, eski);
        }

        // OYNA kayıtlarının özeti: bahis başına ve toplam
        public static List<KayitOzet> Ozet(List<Tahmin> l)
        {
            Dictionary<string, KayitOzet> d = new Dictionary<string, KayitOzet>();
            KayitOzet top = new KayitOzet(); top.Bahis = "Toplam";
            foreach (Tahmin t in l)
            {
                if (!t.Oyna) continue;
                KayitOzet o; if (!d.TryGetValue(t.Bahis, out o)) { o = new KayitOzet(); o.Bahis = t.Bahis; d[t.Bahis] = o; }
                foreach (KayitOzet x in new KayitOzet[] { o, top })
                {
                    if (t.Tuttu.Length == 0) { x.Bekleyen++; continue; }
                    x.Mac++;
                    if (t.Tuttu == "evet") { x.Tuttu++; x.Kazanc += t.Oran - 1; } else x.Kazanc -= 1;
                }
            }
            List<KayitOzet> r = new List<KayitOzet>(d.Values);
            r.Sort(delegate (KayitOzet a, KayitOzet b) { return string.CompareOrdinal(a.Bahis, b.Bahis); });
            r.Add(top);
            return r;
        }
    }
}
