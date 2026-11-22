# Futbol tahmin / veri repoları: inceleme (22.11.2026)

12 repo klonlandı ve okundu. Kod Ben reposuna kopyalanmadı (lisanslar farklı, toplam ~410 MB); burada yalnız değerlendirme var.

## Özet tablo

| Repo | Tür | Lisans | Son güncelleme | Süper Lig | Maç Analiz Merkezi için |
|---|---|---|---|---|---|
| L1nu-X/open-matchday | Veri (CSV, her gün yenileniyor) | Ticari olmayan kullanım serbest | 21.11.2026 | **Var** (`TUR_1_super-lig`) | **En yararlı.** İY/MS skor, şut, isabetli şut, korner (1. ve 2. yarı ayrı), kart, gol dakikaları. API anahtarı yok, düz CSV. |
| serayd61/football-predictions-open-data | Model tahmini + sonuç (CSV) | CC BY 4.0 | 22.11.2026 | **Var** (90 maç, %54,4 tuttu) | Karşılaştırma için: Dixon-Coles 1/X/2 olasılıkları ve beklenen goller (lambda). Bizim olasılıklarımızı bununla kıyaslamak mümkün. |
| v-eatpizzanot/soccer-dataset | Veri gölü (674 bin maç, 271 lig) | CC BY 4.0 | 05.07.2026 | Var (Hugging Face'te tam veri) | KG Var/Yok ve gol modeli için geçmiş veri. Repoda yalnız ~1 bin satır örnek; tam veri Hugging Face'te. Not: xG'si basit formül, güvenilmez. |
| Reymes/football-match-prediction | Model (Python) | MIT | 12.07.2026 | Ligler football-data.co.uk'tan | **Yöntem olarak en dürüst.** Elo + zaman ağırlıklı Dixon-Coles + LightGBM + kalibrasyon. Sonuç: hiçbir model marjı çıkarılmış oranı geçemiyor (log-loss 1,0055 vs 1,0026). |
| Samatuly/football-predictor | Model (Claude Code eklentisi, Python/NumPy) | MIT | 15.11.2026 | Yok (milli maçlar) | Dixon-Coles + Elo-Poisson, oran marjı çıkarma, 2,5 Üst / KG / İY pazarları. Formüller PowerShell'e taşınabilecek kadar sade. |
| Genofldn/football-prediction | Model (XGBoost + Poisson) | Belirtilmemiş | 26.06.2026 | 24 lig | 1X2 %53,2, 2,5 Üst %61,6, KG %59,8. API-Football ve Odds API anahtarı ister. Lisans yok → kod alınamaz, yalnız fikir. |
| Luis-ntonio/Match_Prediction | Model (MCMC + XGBoost) | CC0 | 22.06.2026 | Yok (milli maçlar) | Ana bulgu: sinyalin neredeyse tamamı hücum/savunma gücünden geliyor; form ve H2H az katkı veriyor. |
| joel662/Football-Match-Prediction | Akademik çalışma (Elo + ML) | README'de MIT | 09.05.2026 | Yok (İspanya ağırlıklı) | Beraberlik yakalama ile doğruluk arasındaki ödünleşme üzerine. Sınırlı yarar. |
| TacticsBadger/MonteCarloFootballMatchSim | xG'den Monte Carlo simülasyon | GPL-2.0 | 21.01.2024 | Yok | Basit: xG ver, 20.000 maç simüle et, skor matrisi çıkar. Eski. |
| Ryzellx/football-live-api | Canlı skor API (Node/TypeScript) | MIT | 19.09.2026 | FotMob üzerinden | FotMob'u kazıyor; kurulum ister (Vercel). Siteye otomatik erişim kuralı açısından dikkat. |
| salimt/football-datasets | Transfermarkt veri (oyuncu, değer, sakatlık) | Lisans dosyası yok | 29.09.2026 | Oyuncu düzeyinde | Sakatlık geçmişi ilginç ama maç tahmini için dolaylı; 129 MB. |
| florentfougeres/OpenFootballBase-Data | Kadro/kariyer veri (Wikipedia) | Kod MIT, veri CC BY-SA 4.0 | 22.11.2026 | 18 lig | Kadro bilgisi; maç tahmini için dolaylı. |

## Öne çıkan sonuçlar

1. **Veri için:** `open-matchday` Süper Lig dahil 75 lig, her gün güncel, korner ve şut yarı yarı ayrı. Doğrudan indirilebilir:
   `https://raw.githubusercontent.com/oladeeayo/open-matchday/master/data/TUR_1_super-lig/results_2627.csv`
2. **Kıyas için:** `football-predictions-open-data` her maçın maç öncesi olasılıklarını kaydediyor ve kaybedenleri silmiyor. Süper Lig'de 90 maçta %54,4 tuttu (hep ev sahibi seçmek: %45,0).
3. **Yöntem:** Ciddi repoların hepsi aynı sonuca varıyor: marjı çıkarılmış bahis oranı en güçlü tahmin. Modeller ancak yakın geçebiliyor (Reymes: 1,0055 vs 1,0026 log-loss). Dixon-Coles + zaman ağırlığı + kalibrasyon standart yapı.
4. **Lisans:** Genofldn ve salimt'in lisansı yok, kodu kullanılamaz. open-matchday ticari olmayan kullanım için.

## Hatırlatma

Bu repolardan herhangi biri Maç Analiz Merkezi'ne eklenecekse kuralın geçerli: her hücrede arka planda denenir, katkı yaptığı yere eklenir, bozduğu yere eklenmez.
