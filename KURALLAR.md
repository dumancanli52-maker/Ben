# Maç Analiz Merkezi: bekleyen kurallar

## GOL ARALIĞI ŞARTI (sınandı, programa eklenecek: kullanıcı MAM_gonder gönderince)

Kayıt tarihi: 27 Kasım 2026. Veri: MAM_gonder 27.11 22:27, 6.995 sonuçlu kayıt.

### Kural
| Bahis | OYNA için şart | Şart sağlanmazsa |
|---|---|---|
| MS 1 | 0-1 gol hücresi **en az %22** | maç OYNAMA (başka bahse geçmez) |
| MS 2 | 4-5 gol hücresi **en az %28** | maç OYNAMA |
| KG Var | 0-1 gol hücresi **en çok %18** | maç OYNAMA |
| 2,5 Üst, 2,5 Alt, KG Yok | değişmez | |

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
