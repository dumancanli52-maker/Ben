# YENİ SİSTEM · TAHMİN (Adım 1-12 birlikte, OYNA YOK)
# Girdi: maç listesi (JSON: kod, saat) + sitenin o maçlar için Header / LeagueTable cevapları (klasörde) + site_maclari.csv.
# Her maç için: lig ve sezon sitenin Header'ından; tablolar arşivden (kesim anından önceki maçlar, takımın grubu);
# tablo kontrolü: iki takımın arşivdeki maç sayısı sitenin tablosuyla en çok 1 farklı olmalı, değilse "tablo eksik" (hesap yapılmaz).
# Çıktı: CSV (MS, gol aralıkları, Alt / Üst, beklenen gol). PowerShell 5.1.
param([string]$Kod, [string]$Arsiv, [string]$Maclar, [string]$Cache, [string]$Kesim, [string]$Cikti)

Add-Type -Path @("$Kod/Adim1_PuanTablosu.cs", "$Kod/Adim2_IcDisTablosu.cs", "$Kod/Adim3_IlkYari.cs", "$Kod/Adim4_MacSonucu.cs", "$Kod/Adim5_GolAraligi.cs", "$Kod/Adim9_AltUst.cs", "$Kod/Ek1_Sut.cs", "$Kod/Ek2_xG.cs", "$Kod/Ek5_IkiliMac.cs", "$Kod/Ek7_Capraz.cs") -WarningAction SilentlyContinue
$inv = [Globalization.CultureInfo]::InvariantCulture
$lig = [YeniSistem.Adim1]::ArsivOku($Arsiv); [YeniSistem.Ek5]::Hazirla($lig); [YeniSistem.Ek7]::Hazirla($lig)
$byKey = @{}; foreach ($k in $lig.Keys) { $p = $k.Split('|')[0].Trim(); $sea = $k.Substring($k.LastIndexOf(' ') + 1); $byKey[$p + '#' + $sea] = $k }
$kesimAn = [DateTime]::ParseExact($Kesim, 'yyyy-MM-dd HH:mm', $inv)
$liste = [IO.File]::ReadAllText($Maclar) | ConvertFrom-Json
$f = { param($v) ([double]$v).ToString('0.0', $inv) }
$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine('kod;saat;lig;ev;dep;durum;ms1;msx;ms2;g01;g23;g45;g6;ust15;ust25;ust35;ust45;bek_ev;bek_dep;en_olasi_aralik;mac_ev;mac_dep;kayit')
$say = @{}
foreach ($m in $liste) {
    $durum = ''; $t = $null; $g = $null; $a = $null; $ad = ''; $ev = ''; $dep = ''
    $hp = Join-Path $Cache ('HeadToHead_' + $m.kod + '_Header.json'); $lp = Join-Path $Cache ('HeadToHead_' + $m.kod + '_LeagueTable.json')
    $h = $null; try { $h = ([IO.File]::ReadAllText($hp) | ConvertFrom-Json).d } catch {}
    if ($null -eq $h) { $durum = 'site bilgisi yok' }
    else {
        $ev = [string]$h.HT[0].N; $dep = [string]$h.AT[0].N; $ad = [string]$h.TOUR.N + ' ' + [string]$h.TOUR.SEA
        $evId = [string]$h.HT[0].TID; $depId = [string]$h.AT[0].TID
        $k = $byKey[[string]$h.TOUR.TID + '#' + [string]$h.TOUR.SEA]
        $lt = $null; try { $lt = [IO.File]::ReadAllText($lp) | ConvertFrom-Json } catch {}
        if (-not $k) { $durum = 'lig arşivde yok' }
        elseif ($null -eq $lt -or $lt.sc -ne 200) { $durum = 'sitede tablo yok' }
        else {
            # sitenin tablosunda iki takımın maç sayısı
            $so = @{}
            foreach ($gr in @($lt.d.LTL)) { foreach ($s in @($gr.LTSL)) { foreach ($tt in @($s.LTTL)) { if ($tt.FT -eq 8) { foreach ($r in @($tt.LTR)) { $so[[string]$r.TID] = [int]$r.MC } } } } }
            $an = [Math]::Min($kesimAn.Ticks, [DateTime]::ParseExact($m.saat, 'yyyy-MM-dd HH:mm', $inv).Ticks)
            $t = [YeniSistem.Adim4]::Hesapla($lig[$k], [DateTime]::new($an), $evId, $depId)
            if (-not $t.Gecerli) { $durum = $t.Neden }
            elseif (-not $so.ContainsKey($evId) -or -not $so.ContainsKey($depId)) { $durum = 'takım site tablosunda yok' }
            elseif ([Math]::Abs($t.MacEv - $so[$evId]) -gt 1 -or [Math]::Abs($t.MacDep - $so[$depId]) -gt 1) { $durum = 'tablo eksik' }
            else { $g = [YeniSistem.Adim5]::Hesapla($t); $a = [YeniSistem.Adim9]::Hesapla($t, $g); $durum = 'tamam' }
        }
    }
    $say[$durum] = 1 + [int]$say[$durum]
    $sat = @($m.kod, $m.saat, ($ad -replace ';', ' '), ($ev -replace ';', ' '), ($dep -replace ';', ' '), $durum)
    if ($durum -eq 'tamam') {
        $sat += @((& $f (100 * $t.P1)), (& $f (100 * $t.PX)), (& $f (100 * $t.P2)), (& $f (100 * $g.G01)), (& $f (100 * $g.G23)), (& $f (100 * $g.G45)), (& $f (100 * $g.G6)),
            (& $f (100 * $a.Ust15)), (& $f (100 * $a.Ust25)), (& $f (100 * $a.Ust35)), (& $f (100 * $a.Ust45)), $t.LamEv.ToString('0.00', $inv), $t.LamDep.ToString('0.00', $inv), $a.EnOlasiAralik, $t.MacEv, $t.MacDep)
    } else { $sat += @('', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '') }
    $sat += (Get-Date -Format 'yyyy-MM-dd HH:mm')
    [void]$sb.AppendLine(($sat -join ';'))
}
[IO.File]::WriteAllText($Cikti, $sb.ToString(), (New-Object System.Text.UTF8Encoding $true))
($say.GetEnumerator() | Sort-Object Value -Descending | ForEach-Object { $_.Name + ': ' + $_.Value }) -join ' · '
