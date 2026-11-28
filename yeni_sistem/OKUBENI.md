# Yeni sistem (sıfırdan, adım adım)

Kullanıcının sırası: 1 puan tablosu -> 2 iç-dış tablosu -> 3 İY -> 4 MS -> 5-8 gol aralıkları (0-1 / 2-3 / 4-5 / 6+) -> 9-12 aralıktan Alt / Üst bahisleri.
Kurallar: hiçbir adım OYNA vermez; her adım yazılır, sınanır, kullanıcı onaylayınca sonrakine geçilir; zip yok; mevcut programa dokunulmaz.
Kaynak: maç sonuçları arşivi. Her maç için tablo o maçtan önceki maçlarla kurulur: geçmişte sınanan ile canlıda çalışan aynı hesap.
Kod C# 5 (PowerShell 5.1 Add-Type ile derlenir).

## KULLANICININ ŞARTLARI (28.11.2026, el yazısı liste) · her adım bunlara uyacak
1. Sistem hatasız çalışacak.
2. Hiçbir yüzde kendi başına hesaplanmaz; her sütunun yüzdesi sırasına göre öncekinden destek alır (tablo -> iç/dış -> İY -> MS -> gol aralıkları -> bahis; ekler aynı zincire girer).
3. Takım karnesi önemli.
4. Takım güç karnesi önemli.
5. Maç karnesi önemli.
6. Ekranda görünen her yazı bu sistemin sonucundan üretilir.
7. Eski sistemden hiçbir şey kalmayacak.
8. xG kaydı ve kendi xG'mizi oluşturma olacak.
9. OYNA şartı: her şart bizim yüzde dilimimize girdiğinde OYNA.
10. Kesinlikle eski sistemden daha iyi olacak.
11. Hava durumu ve şartlar önemli.
12. Geçmiş maçlardan alınan kadro değerlendirmesi kullanılabilir.

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

## Ek 1: İsabetli şut (Ek1_Sut.cs, Adım 4'e bağlı) · sınandı ve alındı 28.11.2026
- Kaynak: Flashscore maç istatistiği (MAM mac_istatistik.csv). Site maçlarıyla eşleştirme: aynı gün, aynı skor, takım adı benzerliği, tek aday
  (Aralık'tan beri 61.538 site maçının 51.749'u eşleşti, 110 çok adaylı atıldı; 11.533'ünde şut var). Arşive isb_ev / isb_dep sütunu olarak girer.
- Hesap: sezon içinde maçtan önceki şutlu maçlardan takımın maç başı attığı / yediği isabetli şut, 8 maçlık lig ortalamasıyla dengeli;
  ev beklenen golü x (ev şut hücumu x dep şut savunması)^0,35, deplasman tersi. İki takımın da en az 3 şutlu maçı yoksa uygulanmaz.
- Öğren-sına (iki takımın da 3+ şutlu maçı olan 5.895 maç): öğrenilen ağırlık 0,4 / 0,3, dengeleme 10 / 6 -> ortak 0,35 / 8.
| | MS hatası | Gol aralığı hatası | Gol hatası |
|---|---|---|---|
| tek gün (2.656) | 1,0162 -> 1,0130 | 1,2393 -> 1,2375 | 3,0204 -> 3,0157 |
| çift gün (3.239) | 1,0229 -> 1,0207 | 1,2202 -> 1,2186 | 2,9588 -> 2,9542 |
- C# koduyla bütün 15.144 maçta: MS 1,0218 -> 1,0206 (tek), 1,0215 -> 1,0206 (çift); gol aralığı 1,2301 -> 1,2294, 1,2226 -> 1,2220.
- Canlıda şut bilgisi, programın Flashscore'dan çektiği maç istatistiğine bağlı.

## Ek 2: Kendi xG'miz (Ek2_xG.cs) · 28.11.2026 · KAYIT VE GÖSTERİM, ZİNCİRE GİRMEDİ
- Oluşturma (23.078 takım-maç, öğren-sına; iki yarıda katsayılar aynı):
  büyük fırsat varsa xG = 0,246 x isabetli - 0,045 x isabetsiz + 0,25 x büyük fırsat (ceza sahası içi şut katkısız); yoksa xG = 0,315 x isabetli.
- Maç içinde golle ilişki: zengin xG 0,665, Flashscore xG 0,58 (ama bu aynı maçın goluyla, kısmen kendiliğinden yüksek).
- Asıl sınav (takımın geçmiş xG'si sonraki maçın golünü söylüyor mu, 5.959 maç; Flashscore xG'li 2.821):
  - kendi xG ≈ isabetli şut (Ek 1): ek bilgi yok;
  - Flashscore xG tek başına şuttan biraz iyi, ama şut zincirdeyken eklenince: tek gün MS 1,0138 -> 1,0141 (kötü), çift gün 1,0248 -> 1,0243 (iyi): tutarsız.
- Karar: kendi xG'miz her maç için hesaplanır ve kaydedilir (arşive isz_ / bf_ sütunları, Mac.IszEv / BfEv), ekranda gösterilebilir; yüzdelere karışmaz.

## Ek 3: Takım karnesi · form ve seri · 28.11.2026 · ALINMADI
Veri: iki takımın da en az 8 lig maçı olan 8.316 maç; taban = zincirin bugünkü hâli (tablolar + isabetli şut). Ağırlık eksi de olabilir (form geri dönebilir).
- Gol formu (son 5 maçta atılan / yenilen, takımın sezon ortalamasına göre; lig ortalamasıyla dengeli): öğrenilen ağırlık 0,1 ve 0.
  0,1 sınanan yarıda MS 1,0111 -> 1,0124 / 1,0121 ve gol 2,9848 -> 2,9857'ye bozdu; öbür yarıda 0 (etkisiz).
- Puan formu ve seri (son 5 maçta alınan puan, sezon ortalamasına göre; MS dengesine): öğrenilen ağırlık +0,03 / -0,03 (ters işaret),
  iki sınanan yarıda da MS bozuldu (1,0111 -> 1,0122; 1,0047 -> 1,0054).
- Ev / deplasman başarısı zaten Adım 2 (iç / dış tablo) olarak zincirde.
- Sonuç: tablo ve şut zincirdeyken son maç formu bir şey katmıyor (eski sistemde de form "geri dönüyordu").

## Ek 4: Takım güç karnesi · kadro gücü ve eksik oyuncu · 28.11.2026 · ŞİMDİLİK ALINMADI (veri az)
Kaynak: MAM kadro_gecmis.csv (programın Kasım'da analiz ettiği maçlar); yeni arşivle eşleşen 2.676 maçta kadro gücü (ilk 11 / düzenli 11) 784, eksik golü 543 maçta.
- Kadro gücü (güçlü 11 çıkaranın beklenen golü artar, rakibinki azalır): öğrenilen ağırlık 0,25 / 0. 0,25 sınanan 371 maçta MS 0,9965 -> 0,9992, gol 2,8401 -> 2,8445 (bozdu).
- Eksiklerin golü (golcüsü eksik takımın beklenen golü azalır): öğrenilen 0 / 0,02. 0,02 sınanan 282 maçta MS 0,9977 -> 0,9997, aralık 1,2855 -> 1,2913 (bozdu).
- Sonuç: yarı başına 260-410 maçla katkı görünmüyor. "İşe yaramaz" denmedi: kadro kaydı biriktikçe (en az birkaç bin maç) aynı yöntemle yeniden sınanacak.

## Ek 5: Maç karnesi · ikili maçlar (Ek5_IkiliMac.cs, Adım 4'e bağlı) · sınandı ve alındı 28.11.2026
- İki takımın maçtan önceki aralarındaki son 5 maçı (her turnuva), en az 2 maç; az maç d = n / (n + 3) ile daha az sayılır.
  Gol farkı ortalaması -> ev / deplasman beklenen golü e^(+/-0,1 x fark x d); toplam gol ortalaması -> iki beklenen gol e^(0,1 x (ikili toplam - beklenen toplam) x d).
- Öğren-sına (15.144 maç, 9.528'inde 2+ ikili maç): iki öğrenmede de fark 0,1 / toplam 0,1 (dengeleme 3 / 5) -> ortak 0,1 / 0,1 / 3.
- C# koduyla bütün maçlarda: tek gün MS 1,0206 -> 1,0186, gol aralığı 1,2294 -> 1,2285; çift gün MS 1,0206 -> 1,0188, gol aralığı 1,2220 -> 1,2201.
## Maç karnesinin öbür parçaları · 28.11.2026
- Dinlenme farkı (iki takımın son maçından geçen gün, +/-4 ile sınırlı): iki yarıda aynı yön ama etki binde 0,2-0,3: alınmadı (çok küçük).
- Yorgunluk (son maçtan 3,5 günden az): tutmadı.
- Maçın önemi (sezonun son 8 maçında şampiyonluk / düşme hesabı, Adım 1 tablosundan): yalnız 800 maçta hesaplanabildi, bir tarafın önemli öbürünün önemsiz olduğu 99 maç: veri az, sınanmadı.

## Ek 6: Hava durumu · 28.11.2026 · ŞİMDİLİK ALINMADI (tutarsız)
Kaynak: eski sistemin hava kayıtları (MAM arsiv_hava.csv + kayıtların hava sütunları, 6.919 maç); tarih / takım / skorla yeni arşive eşleşen 4.061,
tablosu tam liglerde 3.343 maç (571 yağmurlu, 189 rüzgarlı 20+ km/s, 263 aşırı sıcak 28+ / soğuk 3- derece). Taban: zincirin bugünkü hâli (tablo + şut + ikili maç).
- Yağmur: yağmurlu maçta gerçek gol beklenenden +0,10 (yağmursuzda -0,06). Öğrenilen ağırlık iki yarıda +0,05 (toplam gol artar):
  gol aralığı iki yarıda iyileşti (1,2283 -> 1,2273; 1,2318 -> 1,2314) ama MS iki yarıda biraz bozuldu (+0,0002) ve gol hatası bir yarıda bozuldu: tutarlı değil.
- Rüzgar: iki yarı ters yön (+0,02 / -0,02), ikisi de bozdu. Aşırı sıcak / soğuk: -0,05 / 0, katkı yok.
- Hava kayıtları biriktikçe (özellikle yağmurlu maç sayısı) yeniden sınanacak.

## Karar adımı (kullanıcının 9. şartı: "her şart bizim yüzde dilimimize girince OYNA") · geniş veriyle sınandı 28.11.2026 · SİSTEME KONMADI
Veri genişletildi: MAM'daki sonuçlu ve site oranlı 5.278 maçın ligleri için 4.460 takımın maç listesi indirildi (arşiv 164.709 maç, 15.942'si istatistikli);
4.610 maç yeni sistemle hesaplandı (her maç kendinden önceki maçlarla). Bahisler: MS 1 / 2, 1,5 Üst, 2,5 Alt / Üst, 3,5 Alt / Üst, KG Var / Yok (KG aynı skor dağılımından).
Şart: bahsin yüzdesi kendi diliminde + site oranı 1,30+; dilim bir yarıda öğrenilir (öğrenilen yarıda %62 altı kapalı), öbür yarıda denenir; maç başı tek bahis.
| Dilim şekli | Bahis | Tuttu | Yarılar | Kâr | Eski programın maçlarında |
|---|---|---|---|---|---|
| dar (6 puan, 30+ maç) | 564 | %59,8 | %63,2 / %56,0 | -%15,8 | 457 bahis %61,3 |
| alt sınırlı (X ve üstü, 60+ maç) | 668 | %59,7 | %61,7 / %58,5 | -%15,9 | 543 %60,8 |
| geniş (12+ puan, 60+ maç) | 732 | %59,8 | %60,9 / %58,9 | -%15,6 | 581 %61,3 |
| geniş (16+ puan, 100+ maç) | 638 | %58,5 | %65,2 / %56,2 | -%16,4 | 545 %58,7 |
| Eski program, aynı maçlar (kayıttaki karar) | 1.714 | %59,3 | %60,4 / %58,2 | -%12,8 | |
- Tutma oranında yeni sistem eski programla aynı yerde (aynı maçlarda 1-2 puan üstte), kârda 3 puan geride; daha az bahis veriyor.
- Dar dilimler iki yarıda farklı seçiliyor (şansa göre öğrenme); geniş dilimler daha dengeli.
- Şart 10 ("kesinlikle eski sistemden iyi") henüz sağlanmadı: karar kuralı sisteme konmadı.

## Ek 7: Çapraz (kullanıcının yöntemi, sıfırdan; Ek7_Capraz.cs, Adım 4'e bağlı) · sınandı ve alındı 28.11.2026
- Her takımın maçtan önceki son 15 maçı (her turnuva) ve evde / deplasmanda son 7 maçı: çapraz ev golü = (ev attı + dep yedi) / 2, dep tersi;
  genel ve iç-dış yarı yarıya; beklenen gol = zincirin beklenen golü^0,8 x çapraz^0,2. Şart: 8 genel ve 4 iç-dış maç.
- Öğren-sına (14.555 maç; taban: tablo + şut + ikili maç): iki öğrenmede de çapraz payı 0,2, iç-dış payı 0,5.
  Sınanan: tek gün MS 1,0200 -> 1,0189, aralık 1,2198 -> 1,2186, gol 2,9813 -> 2,9790; çift gün MS 1,0190 -> 1,0178, aralık 1,2276 -> 1,2260, gol 3,0202 -> 3,0170.
- C# koduyla 15.855 maçta: tek gün MS 1,0191 -> 1,0180, aralık 1,2278 -> 1,2261; çift gün MS 1,0168 -> 1,0157, aralık 1,2203 -> 1,2194.

## Karar adımı, çaprazlı zincirle (4.610 oranlı maç) · 28.11.2026
| | Bahis | Tuttu | Yarılar (tuttu / kâr) | Kâr |
|---|---|---|---|---|
| Yeni: dilim kuralı (12+ puan, 60+ maç, 1,30+) | 822 | %61,7 | %64,1 / -%10,6 · %59,0 / -%15,7 | -%13,0 |
| Yeni, yalnız eski programın da baktığı maçlarda | 638 | %62,9 | | -%11,2 |
| Yeni: dilim + değer şartı (yüzde x oran) | 582 | %51,7 | | -%13,6 (değer şartı tutmayı düşürdü: alınmadı) |
| Eski program, aynı maçlar (kayıttaki karar, kendi öğrendiği maçlar) | 1.714 | %59,3 | %60,4 / %58,2 | -%12,8 |
- Aynı maçlarda yeni sistem tutmada +3,6 puan, kârda +1,6 puan önde; iki yarıda da eski programın üstünde.
- Dilimler: 2,5 Üst %66-86 (iki yarıda aynı), 2,5 Alt %60-72 / %60-80, 3,5 Alt %62-74 / %66-78; KG Var, MS 2, 1,5 Üst tek yarıda.
- Not: zincirin 6 ayarı aynı dönemin maçlarından öğrenildi (bu maçların bir kısmı dahil); dilimler ise öbür yarıdan.

## KARAR SİSTEME KONDU (Karar.cs, Tahmin.ps1) · 28.11.2026 17:15
Dilimler bütün 4.610 oranlı maçtan öğrenildi (en az 60 maç, en az %62 tutmuş, 12+ puan genişlik):
1,5 Üst %60-72 (79 maç %68,4) · 2,5 Üst %66-86 (178 maç %64,0) · 2,5 Alt %60-72 (290 maç %62,8) · 3,5 Alt %62-74 (263 maç %64,6).
MS 1 / 2 ve KG Var / Yok bu çıtayı geçemedi, kurala girmiyor. OYNA: yüzde dilimde + site oranı 1,30+ + tablo kontrolü; maç başı tek bahis.
Kullanıcının kararıyla (28.11 17:25) 3,5 Alt oynanan bahis değil, yalnız bilgi (oranı çoğunlukla 1,30 altı; destek şartı olarak da ek bilgi vermedi). Liste: 46 OYNA (2,5 Üst 27, 2,5 Alt 17, 1,5 Üst 2).
Kullanıcının kararıyla 1,5 Üst de oynanmaz (site oranı ortalama 1,11; dilimdeyken bile yalnız %31i 1,30+). Oynanan: 2,5 Üst ve 2,5 Alt. Liste: 44 OYNA (2,5 Üst 27, 2,5 Alt 17).
İlk uygulama: 28-29 Kasım'ın başlamamış 726 maçı -> 136 OYNA (3,5 Alt 90, 2,5 Üst 27, 2,5 Alt 17, 1,5 Üst 2); tahmin/YeniSistem_OYNA_2026-11-28.xlsx.

## PROGRAM (program\Baslat.bat) · 28.11.2026
Kullanıcının isteği: "excel istemiyorum, sistem istiyorum". Eski programdan hiçbir parça yok (şart 7); ekrandaki her yazı motorun sonucundan (şart 6).
- Açılış: program\Baslat.bat (Windows PowerShell 5.1, WPF). Hesap kodu: yeni_sistem\*.cs (zincir, ekler, karar) + program\kod\*.cs.
- Güncelle (açılışta kendiliğinden):
  1. Bülten: önümüzdeki 36 saatin futbol maçları ve oranları.
  2. Her maçın bilgisi (lig, sezon, takımlar) ve lig tablosu (3 saatte bir yenilenir).
  3. O liglerin bütün takımlarının maç listeleri (12 saatte bir yenilenir; tablo eksiksiz kurulsun diye).
  4. Arşiv: program\veri içindeki bütün takım listelerinden (SiteArsivi.ps1 ile aynı kurallar; aynı önbellekte 164.698 maç, iki yol aynı).
  5. Tablo kontrolü -> zincir -> karar.
  6. Kayıt: program\kayit\tahminler.csv.
- Kayıt: başlamamış maçın satırı her güncellemede yenilenir; başlamış maçın satırı donar. Skor takım listelerine girince satıra yazılır, OYNA satırında "tuttu / tutmadı".
- Sekmeler: Maçlar (yüzdeler, OYNA yeşil; satıra tıklayınca kararın nasıl çıktığı), Sonuçlar (OYNA kayıtları, tuttu %, 1 birim kazanç), Nasıl karar veriyoruz (Karar.cs sabitlerinden üretilir).
- İsabetli şut: site listelerinde şut yok, bu yüzden programda Ek 1 uygulanmaz. Sınama (4.610 oranlı maç, aynı dilimler):
  şutlu 468 bahis %63,2 (yarılar %63,0 / %63,5); şutsuz 429 bahis %62,5 (%62,1 / %62,8). Dilimler şutsuz da tutuyor.
- Sınama (28.11.2026 14:30, gerçek site): bültende 814 maç, 730 hesaplandı, 48 OYNA (2,5 Üst 25, 2,5 Alt 23); süre 40 sn (önbellek doluyken).
  Hesaplanmayanlar: 64 sitede tablo yok (kupa vb.), 13 takım tabloda yok, 6 tablo eksik, 1 maç bilgisi yok.
- İlk açılışta önbellek boş: binlerce takım listesi indirilir, uzun sürer (sonraki açılışlar dakikalar).
- C# 5 / .NET 4.8 derleme denetimi: 0 hata. Ekran (WPF) burada (Linux) açılamadı; XAML ve PowerShell sözdizimi denetlendi.
