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

## Adım 4b: Maç sonucu ayarı · sınandı 28.11.2026
Ayarlanan: dengeleme maç sayısı, iç / dış saha payı, ev ve deplasman beklenen gol çarpanı. Tek günlerden öğrenilip çift günlerde, çift günlerden öğrenilip tek günlerde sınandı.
İki öğrenmede de dengeleme 6 maç, iç / dış payı %25 çıktı; ev çarpanı 1,05 / 1,00, deplasman 0,925 / 0,90. Seçilen: **6 maç, %25, ev 1,05, deplasman 0,925.**
| | MS hatası (tek / çift) | Gol hatası (tek / çift) | MS 1 ort. dedi / gerçek |
|---|---|---|---|
| Excel hâli (12, %50, 1, 1) | 1,0307 / 1,0319 | 3,0496 / 3,0059 | %40,8 / %43,8 |
| Ayarlı | 1,0222 / 1,0208 | 3,0459 / 2,9987 | %42,8 / %43,8 |
Kalibrasyon (ayarlı): MS 1 %50+ dedi %55 gerçek %56 · %60+ %64 / %64 · %70+ %76 / %79; MS 2 %50+ %54 / %54 · %60+ %64 / %64.
En olası sonuç %48,3 -> %48,9. Ortalama gol 2,80 diyor, gerçek 2,88 (gol aralıklarında ayrıca bakılacak). C# koduyla yeniden ölçüldü: aynı.

## Adım 5-8: Gol aralıkları (Adim5_GolAraligi.cs) · sınandı 28.11.2026
- Ayrı model yok: Adım 4'ün skor dağılımından 0-1 / 2-3 / 4-5 / 6+ okunur; MS ile aynı hesaptan çıkar.
- Ayarsız hâl toplam golde fazla emindi: 0-1 gol %44 dediğinde gerçek %33, 6+ %24 dediğinde %15; ortalama 2,80 gol diyor, gerçek 2,88.
- Ayar (Adım 4'e konuldu, öğren-sına): toplam gol tahmini lig ortalamasına yaklaştırılır (yakınlık 0,4) ve x 1,02. İki öğrenmede x 1,03 / 0,4 ve x 1,01 / 0,3 çıktı.
  Düşük skor (Dixon-Coles) düzeltmesi iki yarıda ters yön gösterdi (-0,05 / +0,05): alınmadı.
- Sonuç (15.144 maç, C# koduyla): gol aralığı hatası tek gün 1,2439 -> 1,2301, çift gün 1,2355 -> 1,2226; MS hatası değişmedi (1,0218 / 1,0215).
  Ortalama: 0-1 %22,8 (gerçek %22,6) · 2-3 %44,4 (%44,8) · 4-5 %24,8 (%24,9) · 6+ %8,0 (%7,8); beklenen toplam gol 2,88 (gerçek 2,88).
  Kalan kusur: en uçtaki "kısır maç" tahmini (0-1 gol %43+ dediği 366 maç) hâlâ iyimser (%43 -> %36, %54 -> %38).

## Adım 9-12: Aralıktan çıkan Alt / Üst (Adim9_AltUst.cs) · sınandı 28.11.2026
- Skor dağılımından 1,5 / 2,5 / 3,5 / 4,5 Üst (Alt = 1 - Üst); en olası aralık ve şemadaki bahisleri.
- Sınama (15.144 maç; "sabit oran" = öbür yarının ortalama Üst oranı, öğren-sına):
| | Dedi ort. / gerçek | Hata tek (sabit oran) | Hata çift (sabit oran) |
|---|---|---|---|
| 1,5 Üst | %77,2 / %77,4 | 0,5176 (0,5271) | 0,5373 (0,5409) |
| 2,5 Üst | %54,3 / %54,6 | 0,6802 (0,6875) | 0,6782 (0,6907) |
| 3,5 Üst | %32,8 / %32,7 | 0,6238 (0,6328) | 0,6192 (0,6310) |
| 4,5 Üst | %17,2 / %17,4 | 0,4695 (0,4747) | 0,4394 (0,4512) |
  Dört çizgi de iki yarıda sabit orandan iyi; ortalamalar gerçeğe oturuyor. Uçlarda hafif iyimserlik (2,5 Üst %74 -> %70, %26 -> %39).
- Şemanın "en olası aralık -> bahis" bağlantısı: en olası aralık 15.144 maçın 14.485'inde 2-3 gol (seçici değil).
  4-5 en olası (471 maç): 2,5 Üst %74,9, 3,5 Üst %55,0 · 0-1 en olası (183): 1,5 Alt %38,3, 2,5 Alt %63,9 · 6+ (5 maç).

## İleriye dönük sınama başladı (28.11.2026 15:55, ilk maçlardan önce)
Tahmin.ps1 ile 28-29 Kasım'ın başlamamış 818 maçı: 745'i hesaplandı (57 sitede tablo yok, 11 takım tabloda yok, 5 tablo eksik).
Kayıt: tahmin/yeni_sistem_tahmin_2026-11-28.csv (depoya 15:55 TR'de gönderildi). Maçlar bitince gerçekle, eski programla ve Claude'un sabahki tahminleriyle karşılaştırılacak.

## Karar sınaması (geçmiş maçlar, oranlı) · 28.11.2026 · SİSTEME KONMADI
MAM'ın sonuçlu maçlarından 2.677'si yeni arşivle eşleşti (tablosu tam liglerde). Bahisler: MS 1 / 2, 1,5 / 2,5 / 3,5 Alt-Üst; site oranı 1,30+;
her bahsin sınırı bir yarıda öğrenildi (öğrenilen yarıda %58 altı kapalı), öbür yarıda denendi; maç başı tek bahis.
- Yeni sistem: 414 bahis %58,7 (yarılar %67,1 / %53,5), ort. oran 1,42, kâr -%17,3. Öğrenilen sınırlar iki yarıda çok farklı (tutarsız).
  Bahisler: 3,5 Alt 173 %63,0 · 2,5 Üst 134 %59,0 · 2,5 Alt 48 %54,2 · MS 1 28 %50,0 · 1,5 Üst 16 %56,2 · MS 2 15 %40,0.
- Eski program, aynı maçlar (kayıttaki karar): 1.240 OYNA %59,7; oranı bilinen 546 MS / 2,5 bahsinde kâr -%11,8.
- Sonuç: yüzde zinciri doğru (kalibre, kıyaslardan iyi) ama yalnız tablolardan gelen bilgiyle bugün karar kuralı eski programı geçmiyor; veri bir ay (2.677 maç), sınırlar oynak.
