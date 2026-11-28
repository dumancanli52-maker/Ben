// YENİ SİSTEM · MOTOR: siteden günün maçlarını ve verileri indirir, zinciri (Adım 1-12 + Ek 1/5/7) hesaplar, kararı verir, kaydeder, sonuçları işler.
// Ekrandaki her yazı buradaki sonuçlardan gelir (kullanıcının 6. şartı). C# 5, .NET 4.8 (PowerShell 5.1 Add-Type).
//
// Akış (Guncelle):
//   1. bülten: önümüzdeki maçlar ve oranları (cdnbulten)
//   2. her maç: maç bilgisi (lig, sezon, takımlar) + lig tablosu (apistats)
//   3. o liglerin bütün takımlarının maç listeleri (tablonun eksiksiz kurulması için; 12 saatten eskiyse yenilenir)
//   4. arşiv: önbellekteki bütün takım listelerinden (eski günlerin listeleri de; sınamalar böyle yapıldı)
//   5. her maç: tablo kontrolü (iki takımın maç sayısı sitenin tablosuyla en çok 1 farklı) -> zincir -> karar
//   6. kayıt: kayit/tahminler.csv (maç başlayana kadar güncellenir, sonra donar); skoru gelen maçın sonucu yazılır
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace YeniSistem
{
    public sealed class Tahmin
    {
        public string Kod = "", Mid = "", Lig = "", Ev = "", Dep = "", Durum = "";
        public DateTime Saat;
        public bool Oyna; public string Bahis = "", KararNeden = "";
        public double Yuzde, Oran;
        public double Ms1, MsX, Ms2, G01, G23, G45, G6, U15, U25, U35, U45, BekEv, BekDep;
        public bool Hesaplandi;
        public string Aralik = "";
        public int MacEv, MacDep, SiteMacEv, SiteMacDep;
        public bool Ikili, Capraz, Sut;
        public Dictionary<string, double> Oranlar = new Dictionary<string, double>();
        public string Kayit = "";
        public int SkorEv = -1, SkorDep = -1;   // sonuç (gelince)
        public string Tuttu = "";               // OYNA'da: "evet" / "hayır"; değilse boş

        // ekran (WPF yalnız özelliklere bağlanır; S ile bitenler sıralama için)
        public DateTime SaatS { get { return Saat; } }
        public double YuzdeS { get { return Oyna ? Yuzde : -1; } }
        public double OranS { get { return Oyna ? Oran : -1; } }
        public double Ms1S { get { return Hesaplandi ? Ms1 : -1; } }
        public double MsXS { get { return Hesaplandi ? MsX : -1; } }
        public double Ms2S { get { return Hesaplandi ? Ms2 : -1; } }
        public double G01S { get { return Hesaplandi ? G01 : -1; } }
        public double G23S { get { return Hesaplandi ? G23 : -1; } }
        public double G45S { get { return Hesaplandi ? G45 : -1; } }
        public double G6S { get { return Hesaplandi ? G6 : -1; } }
        public double U15S { get { return Hesaplandi ? U15 : -1; } }
        public double U25S { get { return Hesaplandi ? U25 : -1; } }
        public double A25S { get { return Hesaplandi ? 1 - U25 : -1; } }
        public double A35S { get { return Hesaplandi ? 1 - U35 : -1; } }
        public double BekS { get { return Hesaplandi ? BekEv + BekDep : -1; } }
        public string LigYazi { get { return Lig; } }
        public string EvYazi { get { return Ev; } }
        public string DepYazi { get { return Dep; } }
        public string BahisYazi { get { return Oyna ? Bahis : ""; } }
        public string DurumYazi { get { return Hesaplandi ? "" : Durum; } }
        public string NedenYazi { get { return Hesaplandi ? KararNeden : Durum; } }
        public string TuttuYazi { get { return Tuttu == "evet" ? "tuttu" : Tuttu == "hayır" ? "tutmadı" : (SkorEv >= 0 ? "" : "bekliyor"); } }
        public string TarihYazi { get { return Saat.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture); } }
        public string SaatYazi { get { return Saat.ToString("dd.MM HH:mm", CultureInfo.InvariantCulture); } }
        public string KararYazi { get { return Oyna ? "OYNA" : (Hesaplandi ? "OYNAMA" : ""); } }
        public string SkorYazi { get { return SkorEv >= 0 ? SkorEv + "-" + SkorDep : ""; } }
        public string YuzdeYazi { get { return Oyna ? Y(Yuzde) : ""; } }
        public string OranYazi { get { return Oyna ? Oran.ToString("0.00", CultureInfo.GetCultureInfo("tr-TR")) : ""; } }
        public string Ms1Yazi { get { return Hesaplandi ? Y(Ms1) : ""; } }
        public string MsXYazi { get { return Hesaplandi ? Y(MsX) : ""; } }
        public string Ms2Yazi { get { return Hesaplandi ? Y(Ms2) : ""; } }
        public string G01Yazi { get { return Hesaplandi ? Y(G01) : ""; } }
        public string G23Yazi { get { return Hesaplandi ? Y(G23) : ""; } }
        public string G45Yazi { get { return Hesaplandi ? Y(G45) : ""; } }
        public string G6Yazi { get { return Hesaplandi ? Y(G6) : ""; } }
        public string U15Yazi { get { return Hesaplandi ? Y(U15) : ""; } }
        public string U25Yazi { get { return Hesaplandi ? Y(U25) : ""; } }
        public string A25Yazi { get { return Hesaplandi ? Y(1 - U25) : ""; } }
        public string A35Yazi { get { return Hesaplandi ? Y(1 - U35) : ""; } }
        public string BekYazi { get { return Hesaplandi ? (BekEv.ToString("0.00", CultureInfo.GetCultureInfo("tr-TR")) + " - " + BekDep.ToString("0.00", CultureInfo.GetCultureInfo("tr-TR"))) : ""; } }
        static string Y(double p) { return "%" + (100 * p).ToString("0", CultureInfo.InvariantCulture); }

        // seçili maçın açıklaması: kararın nasıl çıktığı, hepsi hesaptan
        public string Aciklama()
        {
            StringBuilder s = new StringBuilder();
            CultureInfo tr = CultureInfo.GetCultureInfo("tr-TR");
            s.AppendLine(Ev + " - " + Dep + "   (" + Lig + ", " + Saat.ToString("dd.MM.yyyy HH:mm", tr) + ")");
            if (!Hesaplandi) { s.AppendLine("Hesap yapılmadı: " + Durum); return s.ToString(); }
            s.AppendLine();
            s.AppendLine("1) Puan tablosu ve iç-dış tablo: " + Ev + " " + MacEv + " maç, " + Dep + " " + MacDep + " maç (site tablosu: " + SiteMacEv + " / " + SiteMacDep + ")");
            s.AppendLine("2) Beklenen gol: " + Ev + " " + BekEv.ToString("0.00", tr) + ", " + Dep + " " + BekDep.ToString("0.00", tr) + " (toplam " + (BekEv + BekDep).ToString("0.00", tr) + ")");
            List<string> ek = new List<string>();
            if (Sut) ek.Add("isabetli şut");
            if (Ikili) ek.Add("ikili maçlar");
            if (Capraz) ek.Add("çapraz (son 15 / son 7 maç)");
            s.AppendLine("   Beklenen gole destek: " + (ek.Count > 0 ? string.Join(", ", ek.ToArray()) : "yok (veri yetmedi)"));
            s.AppendLine("3) Maç sonucu: 1 " + Y(Ms1) + ", X " + Y(MsX) + ", 2 " + Y(Ms2));
            s.AppendLine("4) Gol aralığı: 0-1 " + Y(G01) + ", 2-3 " + Y(G23) + ", 4-5 " + Y(G45) + ", 6+ " + Y(G6) + "  (en olası " + Aralik + ")");
            s.AppendLine("5) Alt / Üst: 1,5 Üst " + Y(U15) + ", 2,5 Üst " + Y(U25) + ", 2,5 Alt " + Y(1 - U25) + ", 3,5 Alt " + Y(1 - U35));
            s.AppendLine();
            if (Oyna) s.AppendLine("KARAR: OYNA " + Bahis + " " + Y(Yuzde) + ", oran " + Oran.ToString("0.00", tr) + " · " + KararNeden);
            else s.AppendLine("KARAR: OYNAMA · " + KararNeden);
            if (SkorEv >= 0) s.AppendLine("SONUÇ: " + SkorYazi + (Tuttu.Length > 0 ? "  · bahis " + (Tuttu == "evet" ? "tuttu" : "tutmadı") : ""));
            return s.ToString();
        }
    }

    public static class Motor
    {
        public const string Api = "https://apistats.nesine.com/api/v3/";
        public const string BultenAdres = "https://cdnbulten.nesine.com/api/bulten/getprebultenfull";
        public static int SaatIleri = 36;          // bültenden bu kadar saat sonrasına kadar başlayacak maçlar
        public static int ListeYasSaat = 12;       // takım listesi bundan eskiyse yenilenir
        public static int TabloYasSaat = 3;        // lig tablosu bundan eskiyse yenilenir
        public static int Paralel = 4;

        // durum (ekran okur; başka iş parçacığından yazılır)
        static readonly object kilit = new object();
        static string durum = "";
        static volatile bool calisiyor;
        public static string Durum { get { lock (kilit) return durum; } }
        public static bool Calisiyor { get { return calisiyor; } }
        public static string Hata = "";
        public static List<Tahmin> Sonuc = new List<Tahmin>();
        static void Yaz(string s) { lock (kilit) durum = s; }

        public static void Baslat(string klasor)
        {
            if (calisiyor) return;
            calisiyor = true; Hata = "";
            Thread t = new Thread(delegate ()
            {
                try { Sonuc = Guncelle(klasor); Yaz("Hazır: " + Sonuc.Count + " maç, " + Say(Sonuc, true) + " OYNA · " + DateTime.Now.ToString("HH:mm")); }
                catch (Exception e) { Hata = e.Message; Yaz("Hata: " + e.Message); }
                finally { calisiyor = false; }
            });
            t.IsBackground = true; t.Start();
        }

        static int Say(List<Tahmin> l, bool oyna) { int n = 0; foreach (Tahmin t in l) if (t.Oyna == oyna) n++; return n; }

        // ---------- indirme ----------
        static string Indir(string adres)
        {
            Exception son = null;
            for (int d = 0; d < 3; d++)
            {
                try
                {
                    HttpWebRequest r = (HttpWebRequest)WebRequest.Create(adres);
                    r.Accept = "application/json"; r.Timeout = 30000; r.ReadWriteTimeout = 60000;
                    r.UserAgent = "Mozilla/5.0";
                    r.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    using (HttpWebResponse c = (HttpWebResponse)r.GetResponse())
                    using (StreamReader sr = new StreamReader(c.GetResponseStream(), Encoding.UTF8))
                        return sr.ReadToEnd();
                }
                catch (Exception e) { son = e; Thread.Sleep(1000 + 2000 * d); }
            }
            throw new Exception("indirilemedi: " + adres + " (" + (son == null ? "" : son.Message) + ")");
        }

        // önbellekli: dosya yoksa ya da yasSaat'ten eskiyse indirir; indirilemezse eldeki dosya kullanılır
        static string Getir(string onbellek, string yol, double yasSaat)
        {
            string f = Path.Combine(onbellek, yol.Replace('/', '_') + ".json");
            if (File.Exists(f) && (yasSaat < 0 || (DateTime.Now - File.GetLastWriteTime(f)).TotalHours < yasSaat)) return File.ReadAllText(f, Encoding.UTF8);
            try
            {
                string s = Indir(Api + yol);
                if (Json.Oku(s) == null) throw new Exception("bozuk cevap");
                File.WriteAllText(f, s, new UTF8Encoding(false));
                return s;
            }
            catch { return File.Exists(f) ? File.ReadAllText(f, Encoding.UTF8) : null; }
        }

        static void HepsiniGetir(string onbellek, List<string> yollar, double yasSaat, string ne)
        {
            int i = -1, bitti = 0, n = yollar.Count;
            Thread[] th = new Thread[Math.Max(1, Paralel)];
            for (int k = 0; k < th.Length; k++)
            {
                th[k] = new Thread(delegate ()
                {
                    while (true)
                    {
                        int j = Interlocked.Increment(ref i); if (j >= n) return;
                        Getir(onbellek, yollar[j], yasSaat);
                        int b = Interlocked.Increment(ref bitti);
                        if (b % 20 == 0 || b == n) Yaz(ne + ": " + b + " / " + n);
                    }
                });
                th[k].IsBackground = true; th[k].Start();
            }
            foreach (Thread t in th) t.Join();
        }

        // ---------- ana iş ----------
        public static List<Tahmin> Guncelle(string klasor)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 16;
            string onbellek = Path.Combine(klasor, "veri"), kayitKl = Path.Combine(klasor, "kayit");
            Directory.CreateDirectory(onbellek); Directory.CreateDirectory(kayitKl);
            DateTime simdi = DateTime.UtcNow.AddHours(3);   // Türkiye saati

            // 1. bülten
            Yaz("Bülten indiriliyor...");
            string bj; try { bj = Indir(BultenAdres); File.WriteAllText(Path.Combine(onbellek, "bulten.json"), bj, new UTF8Encoding(false)); }
            catch { string f = Path.Combine(onbellek, "bulten.json"); if (!File.Exists(f)) throw; bj = File.ReadAllText(f, Encoding.UTF8); }
            List<BultenMac> bulten = new List<BultenMac>();
            foreach (BultenMac b in SiteVeri.Bulten(bj)) if (b.Saat > simdi && b.Saat <= simdi.AddHours(SaatIleri)) bulten.Add(b);
            bulten.Sort(delegate (BultenMac a, BultenMac b) { return a.Saat.CompareTo(b.Saat); });

            // 2. maç bilgisi + lig tablosu
            List<string> yol = new List<string>();
            foreach (BultenMac b in bulten) yol.Add("HeadToHead/" + b.Kod + "/Header");
            HepsiniGetir(onbellek, yol, -1, "Maç bilgisi");
            yol.Clear();
            foreach (BultenMac b in bulten) yol.Add("HeadToHead/" + b.Kod + "/LeagueTable");
            HepsiniGetir(onbellek, yol, TabloYasSaat, "Lig tablosu");

            // 3. liglerin bütün takımlarının maç listeleri
            Dictionary<string, MacBilgi> bilgi = new Dictionary<string, MacBilgi>();
            Dictionary<string, Dictionary<string, int>> tablo = new Dictionary<string, Dictionary<string, int>>();
            HashSet<string> takim = new HashSet<string>();
            foreach (BultenMac b in bulten)
            {
                MacBilgi mb = SiteVeri.Bilgi(Oku(onbellek, "HeadToHead/" + b.Kod + "/Header"));
                if (mb == null) continue;
                bilgi[b.Kod] = mb; takim.Add(mb.EvTid); takim.Add(mb.DepTid);
                Dictionary<string, int> t = SiteVeri.Tablo(Oku(onbellek, "HeadToHead/" + b.Kod + "/LeagueTable"));
                if (t == null) continue;
                tablo[b.Kod] = t; foreach (string k in t.Keys) takim.Add(k);
            }
            yol.Clear(); foreach (string t in takim) yol.Add("Team/" + t + "/Matches");
            HepsiniGetir(onbellek, yol, ListeYasSaat, "Takım listesi");

            // 4. arşiv
            Yaz("Arşiv kuruluyor...");
            Dictionary<string, List<Mac>> arsiv = new Dictionary<string, List<Mac>>();
            HashSet<string> gorulen = new HashSet<string>();
            foreach (string f in Directory.GetFiles(onbellek, "Team_*_Matches.json"))
            {
                try { SiteVeri.ArsiveEkle(File.ReadAllText(f, Encoding.UTF8), arsiv, gorulen); } catch { }
            }
            SiteVeri.Sirala(arsiv);
            Ek5.Hazirla(arsiv); Ek7.Hazirla(arsiv);

            // 5. hesap
            Yaz("Hesaplanıyor...");
            List<Tahmin> sonuc = new List<Tahmin>();
            string kayitZaman = simdi.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            foreach (BultenMac b in bulten)
            {
                Tahmin t = new Tahmin(); t.Kod = b.Kod; t.Saat = b.Saat; t.Ev = b.Ev; t.Dep = b.Dep; t.Oranlar = b.Oran; t.Kayit = kayitZaman;
                Hesapla(t, b, bilgi, tablo, arsiv, simdi);
                sonuc.Add(t);
            }

            // 6. kayıt + sonuçlar
            Yaz("Kaydediliyor...");
            Kayit.Isle(Path.Combine(kayitKl, "tahminler.csv"), sonuc, arsiv, simdi);
            return sonuc;
        }

        static string Oku(string onbellek, string yol)
        {
            string f = Path.Combine(onbellek, yol.Replace('/', '_') + ".json");
            return File.Exists(f) ? File.ReadAllText(f, Encoding.UTF8) : null;
        }

        public static void Hesapla(Tahmin t, BultenMac b, Dictionary<string, MacBilgi> bilgi, Dictionary<string, Dictionary<string, int>> tablo, Dictionary<string, List<Mac>> arsiv, DateTime simdi)
        {
            MacBilgi mb; if (!bilgi.TryGetValue(b.Kod, out mb)) { t.Durum = "site maç bilgisi yok"; return; }
            t.Mid = mb.Mid; t.Ev = mb.EvAd; t.Dep = mb.DepAd; t.Lig = mb.LigAd + " " + mb.Sezon;
            string k = SiteVeri.LigAnahtari(arsiv, mb.LigTid, mb.Sezon);
            if (k == null) { t.Durum = "lig arşivde yok"; return; }
            Dictionary<string, int> so; if (!tablo.TryGetValue(b.Kod, out so)) { t.Durum = "sitede tablo yok"; return; }
            MsTahmin ms = Adim4.Hesapla(arsiv[k], b.Saat, mb.EvTid, mb.DepTid);   // tablo: maç saatinden önceki maçlar
            if (!ms.Gecerli) { t.Durum = ms.Neden; return; }
            t.MacEv = ms.MacEv; t.MacDep = ms.MacDep;
            if (!so.ContainsKey(mb.EvTid) || !so.ContainsKey(mb.DepTid)) { t.Durum = "takım site tablosunda yok"; return; }
            t.SiteMacEv = so[mb.EvTid]; t.SiteMacDep = so[mb.DepTid];
            if (Math.Abs(ms.MacEv - t.SiteMacEv) > 1 || Math.Abs(ms.MacDep - t.SiteMacDep) > 1) { t.Durum = "tablo eksik (arşiv " + ms.MacEv + "/" + ms.MacDep + ", site " + t.SiteMacEv + "/" + t.SiteMacDep + ")"; return; }
            GolAraligi g = Adim5.Hesapla(ms); AltUst a = Adim9.Hesapla(ms, g);
            t.Ms1 = ms.P1; t.MsX = ms.PX; t.Ms2 = ms.P2; t.BekEv = ms.LamEv; t.BekDep = ms.LamDep;
            t.G01 = g.G01; t.G23 = g.G23; t.G45 = g.G45; t.G6 = g.G6;
            t.U15 = a.Ust15; t.U25 = a.Ust25; t.U35 = a.Ust35; t.U45 = a.Ust45; t.Aralik = a.EnOlasiAralik;
            t.Sut = ms.SutKullanildi; t.Ikili = ms.IkiliKullanildi; t.Capraz = ms.CaprazKullanildi;
            KararSonuc kr = Karar.Ver(ms, a, b.Oran);
            t.Oyna = kr.Oyna; t.Bahis = kr.Bahis; t.Yuzde = kr.Yuzde; t.Oran = kr.Oran; t.KararNeden = kr.Neden;
            t.Hesaplandi = true; t.Durum = "tamam";
        }
    }
}
