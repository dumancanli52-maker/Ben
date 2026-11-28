# Excel motoru (56 / 57) nasıl çalışıyor

İnceleme: 28 Kasım 2026, kullanıcının gönderdiği Nesine_Flashscore_Mac_Oncesi56.xlsx ve Nesine_Flashscore_Gecmis_Mac_Analizi56.xlsx
(paketteki 56'larla birebir aynı). Hücre adresleri "Maç Analizi" sayfasındandır.

## 1. Veri (Power Query, "Tümünü Yenile")
Sitenin maç numarasından (Baglanti sayfası) şunlar çekilir:
- **Puan tablosu** (genel, iç saha, dış saha; ligdeki bütün takımlar, en çok 40): InternetVerisi, Lig Verileri.
- **Ev sahibinin son 5 iç saha maçı** ve **deplasmanın son 5 dış saha maçı**, ilk yarı skorlarıyla birlikte: TakimFormVerisi.
- **İki takımın aralarındaki son 5 maç**, ilk yarı skorlarıyla: IkiliMacVerisi.
- **Kadro gücü** (Flashscore): muhtemel 11'in hücum / savunma puanı ve takımın son maçlardaki ortalaması.

Kontroller (K2 = 1 olmazsa hesap yapılmaz): tablo toplamları tutmalı, iki takım tabloda olmalı, iç / dış saha sayıları genelden büyük olmamalı,
takımların maç geçmişi tabloyla aynı olmalı ("takım geçmişi ile puan tablosu uyuşmuyor"), maç başlamamış olmalı.

## 2. Gol tabanı (her takım için beklenen gol)
Lig ortalaması: takım başı maç başı gol (Lig Verileri G51).

Ev sahibinin beklenen golü (K23):
- **Genel taban (CJ):** (ev sahibinin attığı gol / maç) x (deplasmanın yediği gol / maç) / lig ortalaması.
  Her iki oran da **12 maçlık lig ortalamasıyla dengelenir**: (gol + 12 x lig ort.) / (maç + 12). Az maçlı takım lig ortalamasına çekilir.
- **İç / dış saha tabanı (CK):** aynısı, ev sahibinin iç saha ve deplasmanın dış saha rakamlarıyla, ligin iç saha ortalamasına göre.
- **Karışım:** %50 genel + %50 iç / dış saha (CG = 0,5).
- **Kadro çarpanı:** x (ev sahibinin kadro hücumu, CB) / (deplasmanın kadro savunması, CE).
  Kadro çarpanı = 1 + (bugünkü 11 puanı / takımın ortalaması - 1) x n / (n + 5); n = kadro verisi olan maç sayısı (3-5). Kadro yoksa 1.

Deplasmanın beklenen golü (L23) aynı şekilde, tersten (CL, CM, CD / CC).

## 3. Skor dağılımı (M-T sütunları, 0-0'dan 60-60'a 3.721 skor)
- **Model:** iki bağımsız Poisson; ev K23, deplasman L23 ile (AT sütunu).
- **Gerçek skorlar:** son 5 iç saha + son 5 dış saha + son 5 ikili maçın kendi skorları (aynı maç iki kez sayılmaz).
- **Birleşim (T):** skor olasılığı = (250 x model olasılığı + o skorun gerçek maçlarda kaç kez çıktığı) / (250 + gerçek maç sayısı).
  15 gerçek maç varsa gerçek skorların payı 15 / 265 = %5,7; modelin payı %94,3.
- Ortak gol beklentisi (K5, L5) bu birleşik dağılımın ortalamasıdır.

## 4. Sonuçlar (hepsi aynı skor dağılımından toplanır)
- MS 1 = ev sahibinin önde bittiği skorların toplamı; MS X beraberlikler; MS 2 tersi.
- 2,5 Üst = toplam 3 ve üstü gol; KG Var = iki takım da en az 1 gol; gol aralıkları (0-1, 2-3, 4-6, 7+), 3,5 Alt / Üst, birlikte koşullar (MS1 + KG Var gibi).
- Hücum / savunma olasılığı: ev sahibinin bu maçta gol atma olasılığı (ev golü > 0), gol yememe olasılığı (dep golü = 0).

## 5. İlk yarı
- İlk yarı gol payı (AY65, AY66): gerçek maçların ilk yarı golü / toplam golü (yine 250'lik model ağırlığıyla dengeli; veri yoksa %50).
- İlk yarı dağılımı (AQ): Poisson (K23 x ilk yarı payı) ile gerçek ilk yarı skorlarının aynı 250'ye karşı karışımı.
  İlk yarı skoru bilinmeyen maçta maç sonu golleri o paya göre bölünerek (binom) sayılır.

## 6. Bilgi bölümleri (karara girmez)
- **Maç tipi:** 6.431 maçlık arşivde bu güç / gol tipindeki maçların 100'de kaçı o sonuçla bitti.
- **Oran karşılaştırma:** site ve dünya oranlarının yüzdesi, bizim yüzde, tip yüzdesi; dördü %55+ ise "olur".
- **"100 maçta":** renge göre iki sezonluk denetimden gelen gerçek tutma sayısı (ör. MS 1'de %36 / 54 / 64 / 80).
- Renkler: %45 altı kırmızı, %45-55 sarı, %55-60 açık yeşil, üstü koyu yeşil.

## 7. Geçmiş dosyası farkı
Aynı hesap; yalnız "Geçmiş maç denemesi" modunda çalışır ve altta **GERÇEK SONUÇ VE DENEME** bölümü vardır
(gerçek 90 dakika ve ilk yarı skoru, her sonucun gerçekleşip gerçekleşmediği). Bitmiş maçın tablosu ve geçmişi maç öncesine göre kesilir.

## 8. Program bunun üstüne ne yapıyor
Excel'in yüzdeleri programa gelir; program hücre denetiminin eklerini koyar (şut üstünlüğü, toplam şut puanı, çapraz, maç tipi ...),
1,30 altı oranı atar, %59 sınırı (çapraz aynı yöndeyse %57), KG Var şut onayı, maç başı tek bahis ve 146'nın gol aralığı şartı (KULLANIM 150).

## Gözlemler (değişiklik önerisi değil, bilgi)
- Son maç formu skora yalnız ~%5 ağırlıkla giriyor (250'ye karşı en çok 15 maç); tahmin büyük ölçüde sezonluk tablodan geliyor.
- İki takımın golleri bağımsız Poisson: beraberlik (özellikle 0-0, 1-1) olasılığı genelde biraz düşük çıkar.
- 12 maçlık dengeleme sezon başında güçlü: ilk haftalarda yüzdeler lig ortalamasına yakın kalır.
- İkili maçlar eski sezonlardan da gelebilir; aynı 250'ye karşı sayıldığı için etkisi küçük.

## Ölçüm: Excel'in ham yüzdeleri gerçekte ne yaptı (MAM 28.11 10:23, 3.869 sonuçlu maç öncesi kaydı, 2-28 Kasım)
Kayıttaki biz_ sütunları = hücre eklerinden önceki ham Excel yüzdesi.

| | Excel ort. | Gerçek |
|---|---|---|
| MS 1 | %44,1 | %44,6 |
| MS X | %25,3 | %23,1 |
| MS 2 | %30,6 | %32,3 |
| 2,5 Üst | %53,3 | %53,4 |
| KG Var | %55,4 | %53,3 |
| 0-1 / 2-3 / 4-6 / 7+ gol | 23,8 / 44,1 / 28,2 / 3,8 | 24,0 / 44,2 / 28,4 / 3,4 |

- Ortalamada dürüst: yüzdeler gerçeğe 0-2 puan yakın. Yüksek yüzdelerde güçlü: 2,5 Üst %70-79 -> 314 maç %74; MS 1 %70-79 -> 171 maç %71, %80-89 -> 45 maç %89.
- Ayırma gücü orta: yüzdesi yüksek tarafı seçince 2,5 Alt/Üst %56,9, KG %53,9, MS %51,0 tutuyor. KG Var %60-69 dediğinde gerçek %56.
- **Gol aralığı hücreleri:** programdaki "45" hücresi Excel'de **4-6 gol**, "6" hücresi **7+ gol** (ortalamalar ancak böyle tutuyor; Excel etiketi de böyle).
  146'nın MS 2 şartı ekranda "4-5 gol" diye yazıyor ama ölçülen ve kullanılan hücre 4-6 gol: kural sınamada bu hücreyle geçti, yalnız adı yanlış.

### Üç gözlemin sınaması
1. "Son maç formu az etkili, zarar mı?" -> **Hayır.** Excel'in 2,5 Üst yüzdesi sabitken son 5 maçta çok Üst gören takımlar beklenenden AZ Üst yaptı
   (7-10 Üst: Excel %61,2, gerçek %57,1; 0-3 Üst: Excel %41,8, gerçek %49,2). Form geri dönüyor; forma daha çok ağırlık vermek bozardı.
2. "Beraberlik düşük çıkar" -> **Yanlış.** Excel beraberliği fazla veriyor (%25,3 / gerçek %23,1), en çok gollü maçlarda (%23,1 / %19,6). Kısır maçlarda tam (%27,5 / %27,5).
3. "Sezon başı ortalamaya yakın" -> **Sınanamadı:** kayıtlarda tabloda 6 maçtan az oynamış lig yok. 21+ maçlık liglerde isabet daha yüksek (2,5 taraf %60,4, KG %60,4).
