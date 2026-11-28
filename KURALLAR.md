# Maç Analiz Merkezi: bekleyen kurallar

## GOL ARALIĞI ŞARTI (146. düzeltmede programa eklendi, 27.11.2026)

Kayıt tarihi: 27 Kasım 2026. Veri: MAM_gonder 27.11 22:27, 6.995 sonuçlu kayıt.

### Kural
| Bahis | OYNA için şart | Şart sağlanmazsa |
|---|---|---|
| MS 1 | 0-1 gol hücresi **en az %22** | maç OYNAMA (başka bahse geçmez) |
| MS 2 | 4-5 gol hücresi **en az %28** | maç OYNAMA |
| KG Var | 0-1 gol hücresi **en çok %18** | maç OYNAMA |
| **2,5 Üst** | **İY 1,5 Üst hücresi en çok %48** | maç OYNAMA |
| 2,5 Alt, KG Yok | değişmez (aranan şartlar sınavı geçmedi) | |

- Gol aralığı yüzdesi ekrandaki gibi hesaplanır: Excel'in ham yüzdesi + hücre denetiminin ekleri, sonra 4 gol aralığı toplamı korunarak ölçeklenir (Set-HucreEkleri).
- Ekranda sebep yazar: ör. "MS 1 %64 ama 0-1 gol %19 (şart en az %22)".
- Her yerde aynı: tek maç, toplu analiz, Bugün, telefon, Telegram, kayıtların kararı, hücre denetimi.
- Kayıtların yeniden yazılması (ConvertTo-DecisionInput) gol aralığı hücrelerini de (biz_01 / 23 / 45 / 6) ve ekleri taşımalı; şu an yalnız 7 bahis taşıyor.

### Sınama (programın kendi hücre denetimi, yalnız görmediği günler)
| | OYNA | Tuttu | Yarı A / B | İlk / son günler | Kâr (site oranı) |
|---|---|---|---|---|---|
| Kuralsız | 2.761 | %59,8 | %60,0 / %59,5 | %59,6 / %59,9 | -%11,4 |
| Kural (elenen başka bahse geçer) | 2.142 | %60,8 | %61,5 / %60,2 | %60,6 / %61,1 | -%9,5 |
| **Kural (elenen maç OYNAMA)** | **1.865** | **%61,8** | **%62,9 / %60,6** | **%61,4 / %62,2** | **-%8,1** |

Bahis türüne göre (OYNAMA şekli): MS 1 435 %59,1 -> 218 %66,5 (kâr -%7,7 -> +%2,7);
MS 2 189 %62,4 -> 111 %64,9 (-%3,5 -> +%2,0); KG Var 1.133 %60,5 -> 491 %64,4 (-%13,2 -> -%6,9).

Kural oranı düşürmüyor: kalan ve elenen bahislerin ortalama oranı aynı (MS 1 1,53 / 1,52; MS 2 1,59 / 1,56; KG Var 1,43 / 1,44).

### 4. kural: 2,5 Üst (27 Kasım, ikinci sınama)
Arama: programın bütün hücreleri aday şart; şart ve eşik yalnız ilk günlerden (7 Kasım öncesi) öğrenildi, son günlerde ve iki yarıda sınandı.
- 2,5 Üst OYNA, İY 1,5 Üst en çok %48: ilk günler 125 bahis %69,6 -> son günler 78 bahis %64,1 (elenen 111 %57,7); yarılar kalan %70 / %65, elenen %54 / %51.
- KG Var için (KG Yok en az %36) ve 2,5 Alt için (İY 0,5 Üst en çok %67) bulunanlar zayıf (son günlerde fark 3 puan ve altı): alınmadı.

Programın kendi hücre denetiminde (görmediği günler, maç OYNAMA şekli):
| | OYNA | Tuttu | Yarı A / B | İlk / son günler | Kâr |
|---|---|---|---|---|---|
| Kuralsız | 2.761 | %59,8 | %60,0 / %59,5 | %59,6 / %59,9 | -%11,4 |
| 3 kural (MS 1, MS 2, KG Var) | 1.865 | %61,8 | %62,9 / %60,6 | %61,4 / %62,2 | -%8,1 |
| **4 kural (+ 2,5 Üst)** | **1.636** | **%63,1** | **%64,0 / %62,3** | **%63,4 / %62,8** | **-%5,8** |

4 kuralla bahis türleri: MS 1 217 %67,3 (kâr +%5,3) · MS 2 115 %65,2 (+%3,9) · 2,5 Üst 203 %67,5 (-%2,6) · KG Var 491 %64,4 (-%6,9) · 2,5 Alt 556 %58,3 (-%11,1) · KG Yok 54 %64,8.

## 149. düzeltme: gol aralığı 4-5 / 6+ (28 Kasım 2026)
- Excel 58'den önce "4-5 gol" hücresi aslında 4-6 gol hesaplıyordu. MS 2 şartı (4-5 gol en az %28) artık gerçek 4-5 gol yüzdesine bakar.
- Eski kayıtlar çevrilerek kullanılır. Aynı veriyle görmediği günlerde MS 2 şartı: kalan 34 %61,8 / elenen 129 %55,0 (148: kalan 108 %60,2 / elenen 55 %49,1).
- Sınır %28 eski ölçekte bulundu; yeni kayıtlar birikince yeniden sınanacak.
