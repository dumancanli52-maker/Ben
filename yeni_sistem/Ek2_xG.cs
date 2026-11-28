// YENİ SİSTEM · EK 2: KENDİ xG'MİZ (kayıt ve gösterim; yüzde zincirine GİRMEZ)
// Kullanıcının 8. şartı: xG kaydı ve kendi xG'mizi oluşturma.
// Formül (28.11.2026, 23.078 takım-maç, tek / çift gün öğren-sına; iki yarıda katsayılar aynı çıktı):
//   büyük fırsat bilgisi varsa: xG = 0,246 x isabetli şut - 0,045 x isabetsiz şut + 0,25 x büyük fırsat (ceza sahası içi şutun katkısı yok)
//   yoksa:                       xG = 0,315 x isabetli şut (isabetsiz şutun katkısı yok)
// Zincire neden girmiyor: takımın geçmiş xG'si sonraki maçın golünü isabetli şuttan (Ek 1) daha iyi söylemedi;
// Flashscore'un xG'si de şutun üstüne iki yarıda tutarlı katkı vermedi (OKUBENI.md, Ek 2). C# 5.
using System;
using System.Collections.Generic;

namespace YeniSistem
{
    public sealed class XgOzet
    {
        public int EvN, DepN;                 // xG'si hesaplanabilen maç sayısı
        public double EvUretti, EvVerdi, DepUretti, DepVerdi;   // maç başı ortalama
    }

    public static class Ek2
    {
        // isabetsiz / büyük fırsat bilinmiyorsa -1
        public static double Hesapla(int isabetli, int isabetsiz, int buyukFirsat)
        {
            if (isabetli < 0) return double.NaN;
            if (buyukFirsat >= 0) return Math.Max(0.0, 0.246 * isabetli - 0.045 * Math.Max(0, isabetsiz) + 0.25 * buyukFirsat);
            return 0.315 * isabetli;
        }

        // takımın sezon içinde maçtan önceki maçlarındaki kendi xG'si (gösterim için)
        public static XgOzet Ozet(List<Mac> l, DateTime kesim, string evId, string depId)
        {
            XgOzet o = new XgOzet();
            int son = 0; while (son < l.Count && l[son].Zaman < kesim) son++;
            if (son == 0) return o;
            int bas = Adim1.SezonBasi(l, son);
            for (int i = bas; i < son; i++)
            {
                Mac m = l[i];
                double xe = Hesapla(m.IsbEv, m.IszEv, m.BfEv), xd = Hesapla(m.IsbDep, m.IszDep, m.BfDep);
                if (double.IsNaN(xe) || double.IsNaN(xd)) continue;
                if (m.EvId == evId) { o.EvN++; o.EvUretti += xe; o.EvVerdi += xd; }
                if (m.DepId == evId) { o.EvN++; o.EvUretti += xd; o.EvVerdi += xe; }
                if (m.EvId == depId) { o.DepN++; o.DepUretti += xe; o.DepVerdi += xd; }
                if (m.DepId == depId) { o.DepN++; o.DepUretti += xd; o.DepVerdi += xe; }
            }
            if (o.EvN > 0) { o.EvUretti /= o.EvN; o.EvVerdi /= o.EvN; }
            if (o.DepN > 0) { o.DepUretti /= o.DepN; o.DepVerdi /= o.DepN; }
            return o;
        }
    }
}
