# YENİ SİSTEM · SİTE MAÇ ARŞİVİ
# Sitenin takım maç listelerinden (Team/<takım>/Matches cevapları, klasörde JSON) tek maç arşivi yazar:
# site_maclari.csv: mac_id;zaman;ev_id;dep_id;ev;dep;gol_ev;gol_dep;iy_ev;iy_dep;lig;ulke;sezon;asama
#   ulke sütununa ligin site kimliği, lig sütununa "lig adı sezon" yazılır: Adım 1 ligi ve sezonu buradan ayırır (sezon tahmini gerekmez).
#   iy_ev / iy_dep: ilk yarı skoru (sitede yoksa boş). Skoru olmayan (oynanmamış) maç yazılmaz. Aynı maç bir kez yazılır.
# PowerShell 5.1 ile çalışır.
param([string]$Klasor, [string]$Cikti)

$maclar = @{}
foreach ($f in Get-ChildItem -LiteralPath $Klasor -Filter 'Team_*_Matches.json' -File) {
    $j = $null
    try { $j = [IO.File]::ReadAllText($f.FullName) | ConvertFrom-Json } catch { continue }
    if ($null -eq $j -or $j.sc -ne 200 -or $null -eq $j.d) { continue }
    foreach ($tl in @($j.d.TML)) {
        foreach ($b in @($tl.TT)) {
            if ($b.FT -ne 5) { continue }
            foreach ($tm in @($b.TMS)) {
                foreach ($m in @($tm.ML)) {
                    if ($null -eq $m.HT -or $null -eq $m.AT -or $null -eq $m.LG -or -not $m.MD) { continue }
                    $k = [string]$m.MID; if ($maclar.ContainsKey($k)) { continue }
                    $ms = @(@($m.SC) | Where-Object { $null -ne $_ -and $_.TY -eq 2 -and $_.OBI -eq 99 })
                    if ($ms.Count -ne 1 -or $null -eq $ms[0].HTS -or $null -eq $ms[0].ATS) { continue }
                    $iy = @(@($m.SC) | Where-Object { $null -ne $_ -and $_.TY -eq 1 -and $_.OBI -eq 1 })
                    $ie = ''; $id = ''
                    if ($iy.Count -eq 1 -and $null -ne $iy[0].HTS -and $null -ne $iy[0].ATS -and $iy[0].HTS -le $ms[0].HTS -and $iy[0].ATS -le $ms[0].ATS) { $ie = [string]$iy[0].HTS; $id = [string]$iy[0].ATS }
                    $asama = ([string]$m.RD -split ',')[0].Trim()
                    # PowerShell 7 tarihi kendisi DateTime yapar, 5.1 metin bırakır: ikisi de yyyy-MM-dd HH:mm
                    $zaman = $(if ($m.MD -is [datetime]) { $m.MD.ToString('yyyy-MM-dd HH:mm', [Globalization.CultureInfo]::InvariantCulture) } else { ([string]$m.MD).Substring(0, 16).Replace('T', ' ') })
                    $temiz = { param($s) ([string]$s) -replace '[;\r\n]', ' ' }
                    $maclar[$k] = (@($k, $zaman, [string]$m.HT.TID, [string]$m.AT.TID, (& $temiz $m.HT.N), (& $temiz $m.AT.N), [string]$ms[0].HTS, [string]$ms[0].ATS, $ie, $id,
                        ((& $temiz $m.LG.N) + ' ' + [string]$m.SEA), [string]$m.LG.TID, [string]$m.SEA, (& $temiz $asama)) -join ';')
                }
            }
        }
    }
}
$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine('mac_id;zaman;ev_id;dep_id;ev;dep;gol_ev;gol_dep;iy_ev;iy_dep;lig;ulke;sezon;asama')
foreach ($v in $maclar.Values) { [void]$sb.AppendLine($v) }
[IO.File]::WriteAllText($Cikti, $sb.ToString(), (New-Object System.Text.UTF8Encoding $true))
'site arşivi: ' + $maclar.Count + ' maç -> ' + $Cikti
