# Yeni sistem (sıfırdan, adım adım)

Kullanıcının sırası: 1 puan tablosu -> 2 iç-dış tablosu -> 3 İY -> 4 MS -> 5-8 gol aralıkları (0-1 / 2-3 / 4-5 / 6+) -> 9-12 aralıktan Alt / Üst bahisleri.
Kurallar: hiçbir adım OYNA vermez; her adım yazılır, sınanır, kullanıcı onaylayınca sonrakine geçilir; zip yok; mevcut programa dokunulmaz.
Kaynak: maç sonuçları arşivi. Her maç için tablo o maçtan önceki maçlarla kurulur: geçmişte sınanan ile canlıda çalışan aynı hesap.
Kod C# 5 (PowerShell 5.1 Add-Type ile derlenir).

## Adım 1: Puan tablosu (Adim1_PuanTablosu.cs) · sınandı 28.11.2026
- Arşiv okunur (aynı dakikada aynı iki takımın maçı bir kez), lig başına eskiden yeniye sıralanır.
- Sezon: geriye doğru okunur; 25 günden uzun aradan ancak aradan önceki ve sonraki takımlar aynıysa geçilir (kış arası, Dünya Kupası arası aynı sezon).
- Sıralama: puan, averaj, atılan gol.
- Sınama (MAM 28.11 arşivi, 27.11 00:00 kesim; sitenin 28.11 sabahki tabloları, arşivde ligi güvenle bulunan 34 tablo):
  - 551 takım satırının 447'si sitedekiyle birebir aynı (O, G, B, M, A, Y, P);
  - 79'unda sitede 1 maç fazla: o takımın son maçı arşive henüz yazılmamış (26-28 Kasım);
  - 25 satır açıklanamadı (arşivde eski bir maç eksik);
  - 4 tabloda takım sayısı tutmadı: Allsvenskan (eleme maçları aynı lig adında), Premiyer Lig (grup ayrımı), CAF Şampiyonlar Ligi (gruplar), Liga Portugal (yanlış eşleşme).
  - Brezilya Serie A: Dünya Kupası arasına rağmen sezon doğru bulundu (350 / 363 maç; kalanı arşive yazılmamış son maçlar).
- Açık konular: grup / eleme / ikiye bölünen ligler; arşivin güncelliği (canlıda son maçlar sitenin tablosuyla doğrulanmalı).

## Adım 2: İç saha / dış saha tablosu (Adim2_IcDisTablosu.cs) · sınandı 28.11.2026
- Adım 1'in sezonuyla: her takımın yalnız iç saha maçlarından iç saha tablosu, yalnız deplasman maçlarından dış saha tablosu; maçtan önceki maçlarla.
- Kendi denetimi: her takımda iç saha + dış saha = genel tablo (tutmazsa Tutarli = false).
- Sınama (aynı veri, takım sayısı tutan 30 lig):
  - iç + dış = genel: 30 / 30 ligde tuttu; arşivdeki bütün 1.400 ligde de tuttu (0 bozuk).
  - İç saha: 551 satırın 491'i sitedekiyle birebir, 54'ünde sitede 1 maç fazla (son maç arşivde yok), 6 açıklanamayan.
  - Dış saha: 549 satırın 494'ü birebir, 49'unda 1 maç fazla, 6 açıklanamayan.
