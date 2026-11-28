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

## Kaynak değişikliği: site maç arşivi (SiteArsivi.ps1)
Adım 3 için ilk yarı skoru gerekiyor; Flashscore arşivinde yok. Sitenin takım maç listeleri (Excel motorunun da kullandığı kaynak) her maç için
maç sonu ve ilk yarı skoru, lig numarası ve sezon adı veriyor. SiteArsivi.ps1 bu listeleri site_maclari.csv'ye çevirir. Adım 1-3 iki arşivi de okur;
site arşivinde sezon etiketten alınır (tahmin edilmez), gruplu liglerde tablo takımın kendi grubundan kurulur (birbiriyle oynayan takımlar).
**Kural:** bir maçın tablosu için o ligdeki BÜTÜN takımların listesi gerekir (yalnız iki takımın listesiyle tablo eksik kalır).

## Adım 3: İlk yarı tablosu (Adim3_IlkYari.cs) · sınandı 28.11.2026
- Genel, iç saha, dış saha: ilk yarısı bilinen maç, ilk yarıda önde / berabere / geride, ilk yarıda atılan / yenilen, ilk yarısı gollü (1+) ve 2+ gollü maç.
- Kendi denetimi (önde + berabere + geride = maç; ilk yarı golü maç sonunu geçmez; iç + dış = genel): 1.146 lig-sezonun hepsinde tuttu.
- İlk yarı skoru maçların %78'inde var (120.622 maçın 94.047'si).
- Site arşiviyle Adım 1-2 yeniden sınandı (180 site tablosu, 2.895 takımın listesi, takım numarasıyla birebir karşılaştırma):
  genel %84,6, iç saha %90,8, dış saha %90,8 birebir. Kalan fark iki türlü:
  - sitenin takım listelerinde bazı maçlar hiç yok (ör. Cymru Güney: site tablosu 112 maç, 16 takımın listeleri birlikte 107);
  - sezon içinde ikinci aşama olan liglerde (Kadınlar Premier Lig, Ulusal Lig) site yalnız güncel aşamanın tablosunu gösteriyor.

## Adım 4: Maç sonucu (Adim4_MacSonucu.cs) · sınandı 28.11.2026
- Excel motorunun çekirdeği (kadro çarpanı ve gerçek skor karışımı yok): 12 maçlık lig ortalamasıyla dengeli hücum / savunma,
  %50 genel + %50 iç / dış, iki bağımsız Poisson -> skor dağılımı (Adım 5-8 de bunu kullanacak) -> MS 1 / X / 2. Takımların en az 3 maçı olmalı.
- Sınama: tablosu tam 180 lig-sezonun 15.144 maçı (Şubat - Kasım 2026), her maç kendinden önceki maçlarla:
  - log-kayıp 1,0313 (yarılar 1,0307 / 1,0319); basit kıyas (ligin o güne kadarki ev / beraberlik / deplasman oranı) 1,0767. Açıkça daha iyi.
  - en olası sonuç %48,3 tuttu.
  - **Yüzdeler fazla temkinli:** ortalamada MS 1 %40,8 diyor, gerçek %43,8 (ev avantajı az veriliyor); MS 2 %34,4 diyor, gerçek %31,1.
    MS 1 %54 dediğinde gerçek %63, %64 dediğinde %77; MS 2 %17 dediğinde %10. Uçlar yeterince açılmıyor (12 maçlık dengeleme ağır).
- İlk yarı (Adım 3) MS'ye bir şey katıyor mu: aynı MS 1 bandında ilk yarı güçlü / zayıf ayrımı tutarlı bir fark vermedi (fark temkinlilikten). Katmıyor.
