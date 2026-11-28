# YENİ SİSTEM · PROGRAM (ekran). Windows PowerShell 5.1, WPF.
# Hesap kodu: ..\*.cs (zincir, ekler, karar) + kod\*.cs (site verisi, motor, kayıt). Veriler: veri\ (önbellek), kayit\tahminler.csv.
# Ekrandaki her yazı motorun sonuçlarından gelir (kullanıcının 6. şartı); site adı ekranda geçmez; sarı yalnız uyarı.
$ErrorActionPreference = 'Stop'
$kok = $PSScriptRoot
Add-Type -AssemblyName PresentationFramework, PresentationCore, WindowsBase
if (-not ('YeniSistem.Motor' -as [type])) {
    $kaynak = @(Get-ChildItem -LiteralPath (Split-Path $kok) -Filter '*.cs' -File | ForEach-Object FullName) + @(Get-ChildItem -LiteralPath (Join-Path $kok 'kod') -Filter '*.cs' -File | ForEach-Object FullName)
    Add-Type -Path $kaynak -IgnoreWarnings -WarningAction SilentlyContinue
}
$kayitYol = Join-Path $kok 'kayit\tahminler.csv'

[xml]$xaml = @'
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Maç Analiz · Yeni Sistem" Width="1400" Height="860" WindowStartupLocation="CenterScreen" Background="#F4F6F8" FontFamily="Segoe UI" FontSize="13">
  <Window.Resources>
    <Style TargetType="DataGridColumnHeader"><Setter Property="FontWeight" Value="SemiBold"/><Setter Property="Padding" Value="6,4"/><Setter Property="Background" Value="#E3E8EE"/></Style>
    <Style x:Key="Satir" TargetType="DataGridRow">
      <Style.Triggers>
        <DataTrigger Binding="{Binding KararYazi}" Value="OYNA"><Setter Property="Background" Value="#D9F2DF"/><Setter Property="FontWeight" Value="SemiBold"/></DataTrigger>
        <DataTrigger Binding="{Binding KararYazi}" Value=""><Setter Property="Foreground" Value="#8A949E"/></DataTrigger>
      </Style.Triggers>
    </Style>
    <Style x:Key="SonucSatir" TargetType="DataGridRow">
      <Style.Triggers>
        <DataTrigger Binding="{Binding TuttuYazi}" Value="tuttu"><Setter Property="Background" Value="#D9F2DF"/></DataTrigger>
        <DataTrigger Binding="{Binding TuttuYazi}" Value="tutmadı"><Setter Property="Background" Value="#F6DADA"/></DataTrigger>
      </Style.Triggers>
    </Style>
  </Window.Resources>
  <DockPanel Margin="10">
    <Border DockPanel.Dock="Top" Background="White" CornerRadius="4" Padding="10" Margin="0,0,0,8">
      <DockPanel>
        <Button x:Name="BtnGuncelle" Content="Güncelle" Width="110" Height="30" Margin="0,0,12,0" FontWeight="SemiBold"/>
        <CheckBox x:Name="ChkOyna" Content="Yalnız OYNA" VerticalAlignment="Center" Margin="0,0,12,0"/>
        <CheckBox x:Name="ChkHesap" Content="Yalnız hesaplanan" VerticalAlignment="Center" Margin="0,0,12,0" IsChecked="True"/>
        <TextBlock Text="Ara:" VerticalAlignment="Center" Margin="0,0,4,0"/>
        <TextBox x:Name="TxtAra" Width="180" Height="26" VerticalContentAlignment="Center" Margin="0,0,12,0"/>
        <Border x:Name="DurumKutu" CornerRadius="3" Padding="8,4"><TextBlock x:Name="TxtDurum" VerticalAlignment="Center"/></Border>
      </DockPanel>
    </Border>
    <TabControl>
      <TabItem Header="Maçlar">
        <DockPanel>
          <TextBox x:Name="TxtDetay" DockPanel.Dock="Bottom" Height="210" IsReadOnly="True" FontFamily="Consolas" FontSize="13" TextWrapping="Wrap" VerticalScrollBarVisibility="Auto" Margin="0,8,0,0" Padding="6"/>
          <DataGrid x:Name="Grid" AutoGenerateColumns="False" IsReadOnly="True" HeadersVisibility="Column" GridLinesVisibility="Horizontal" HorizontalGridLinesBrush="#E3E8EE"
                    RowStyle="{StaticResource Satir}" SelectionMode="Single" CanUserAddRows="False" Background="White">
            <DataGrid.Columns>
              <DataGridTextColumn Header="Saat" Binding="{Binding SaatYazi}" SortMemberPath="SaatS"/>
              <DataGridTextColumn Header="Lig" Binding="{Binding LigYazi}" Width="180"/>
              <DataGridTextColumn Header="Ev" Binding="{Binding EvYazi}" Width="150"/>
              <DataGridTextColumn Header="Deplasman" Binding="{Binding DepYazi}" Width="150"/>
              <DataGridTextColumn Header="Karar" Binding="{Binding KararYazi}"/>
              <DataGridTextColumn Header="Bahis" Binding="{Binding BahisYazi}"/>
              <DataGridTextColumn Header="Yüzde" Binding="{Binding YuzdeYazi}" SortMemberPath="YuzdeS"/>
              <DataGridTextColumn Header="Oran" Binding="{Binding OranYazi}" SortMemberPath="OranS"/>
              <DataGridTextColumn Header="MS 1" Binding="{Binding Ms1Yazi}" SortMemberPath="Ms1S"/>
              <DataGridTextColumn Header="X" Binding="{Binding MsXYazi}" SortMemberPath="MsXS"/>
              <DataGridTextColumn Header="MS 2" Binding="{Binding Ms2Yazi}" SortMemberPath="Ms2S"/>
              <DataGridTextColumn Header="0-1" Binding="{Binding G01Yazi}" SortMemberPath="G01S"/>
              <DataGridTextColumn Header="2-3" Binding="{Binding G23Yazi}" SortMemberPath="G23S"/>
              <DataGridTextColumn Header="4-5" Binding="{Binding G45Yazi}" SortMemberPath="G45S"/>
              <DataGridTextColumn Header="6+" Binding="{Binding G6Yazi}" SortMemberPath="G6S"/>
              <DataGridTextColumn Header="1,5 Ü" Binding="{Binding U15Yazi}" SortMemberPath="U15S"/>
              <DataGridTextColumn Header="2,5 Ü" Binding="{Binding U25Yazi}" SortMemberPath="U25S"/>
              <DataGridTextColumn Header="2,5 A" Binding="{Binding A25Yazi}" SortMemberPath="A25S"/>
              <DataGridTextColumn Header="3,5 A" Binding="{Binding A35Yazi}" SortMemberPath="A35S"/>
              <DataGridTextColumn Header="Beklenen gol" Binding="{Binding BekYazi}" SortMemberPath="BekS"/>
              <DataGridTextColumn Header="Durum" Binding="{Binding DurumYazi}" Width="*"/>
            </DataGrid.Columns>
          </DataGrid>
        </DockPanel>
      </TabItem>
      <TabItem Header="Sonuçlar">
        <DockPanel>
          <DataGrid x:Name="GridOzet" DockPanel.Dock="Top" AutoGenerateColumns="False" IsReadOnly="True" HeadersVisibility="Column" Height="120" Margin="0,0,0,8" Background="White" CanUserAddRows="False">
            <DataGrid.Columns>
              <DataGridTextColumn Header="Bahis" Binding="{Binding Bahis}" Width="120"/>
              <DataGridTextColumn Header="Sonuçlanan" Binding="{Binding MacYazi}" Width="100"/>
              <DataGridTextColumn Header="Tuttu" Binding="{Binding TuttuYazi}" Width="130"/>
              <DataGridTextColumn Header="Kazanç (1 birim)" Binding="{Binding KazancYazi}" Width="200"/>
              <DataGridTextColumn Header="Bekleyen" Binding="{Binding BekleyenYazi}" Width="90"/>
            </DataGrid.Columns>
          </DataGrid>
          <DataGrid x:Name="GridSonuc" AutoGenerateColumns="False" IsReadOnly="True" HeadersVisibility="Column" Background="White" CanUserAddRows="False" RowStyle="{StaticResource SonucSatir}">
            <DataGrid.Columns>
              <DataGridTextColumn Header="Tarih" Binding="{Binding TarihYazi}" SortMemberPath="SaatS"/>
              <DataGridTextColumn Header="Saat" Binding="{Binding SaatYazi}" SortMemberPath="SaatS"/>
              <DataGridTextColumn Header="Lig" Binding="{Binding LigYazi}" Width="180"/>
              <DataGridTextColumn Header="Ev" Binding="{Binding EvYazi}" Width="150"/>
              <DataGridTextColumn Header="Deplasman" Binding="{Binding DepYazi}" Width="150"/>
              <DataGridTextColumn Header="Bahis" Binding="{Binding BahisYazi}"/>
              <DataGridTextColumn Header="Yüzde" Binding="{Binding YuzdeYazi}" SortMemberPath="YuzdeS"/>
              <DataGridTextColumn Header="Oran" Binding="{Binding OranYazi}" SortMemberPath="OranS"/>
              <DataGridTextColumn Header="Skor" Binding="{Binding SkorYazi}"/>
              <DataGridTextColumn Header="Sonuç" Binding="{Binding TuttuYazi}" Width="*"/>
            </DataGrid.Columns>
          </DataGrid>
        </DockPanel>
      </TabItem>
      <TabItem Header="Nasıl karar veriyoruz">
        <TextBox x:Name="TxtAnlatim" IsReadOnly="True" FontFamily="Consolas" FontSize="13" TextWrapping="Wrap" VerticalScrollBarVisibility="Auto" Padding="10" Background="White"/>
      </TabItem>
    </TabControl>
  </DockPanel>
</Window>
'@
$pen = [Windows.Markup.XamlReader]::Load((New-Object System.Xml.XmlNodeReader $xaml))
$el = @{}
foreach ($ad in 'BtnGuncelle', 'ChkOyna', 'ChkHesap', 'TxtAra', 'DurumKutu', 'TxtDurum', 'TxtDetay', 'Grid', 'GridOzet', 'GridSonuc', 'TxtAnlatim') { $el[$ad] = $pen.FindName($ad) }
$el.TxtAnlatim.Text = [YeniSistem.Karar]::Anlatim()

$script:liste = New-Object 'System.Collections.Generic.List[YeniSistem.Tahmin]'
$sari = [Windows.Media.BrushConverter]::new().ConvertFromString('#FFF2B3')

function Durum([string]$yazi, [bool]$uyari) {
    $el.TxtDurum.Text = $yazi
    $el.DurumKutu.Background = $(if ($uyari) { $sari } else { [Windows.Media.Brushes]::Transparent })
}

function Suz {
    $ara = $el.TxtAra.Text.Trim()
    $g = New-Object 'System.Collections.Generic.List[YeniSistem.Tahmin]'
    foreach ($t in $script:liste) {
        if ($el.ChkOyna.IsChecked -and -not $t.Oyna) { continue }
        if ($el.ChkHesap.IsChecked -and -not $t.Hesaplandi) { continue }
        if ($ara -and ($t.Ev + ' ' + $t.Dep + ' ' + $t.Lig).IndexOf($ara, [StringComparison]::CurrentCultureIgnoreCase) -lt 0) { continue }
        $g.Add($t)
    }
    $el.Grid.ItemsSource = $g
}

function SonuclariGoster {
    $kayit = [YeniSistem.Kayit]::Oku($kayitYol)
    $el.GridOzet.ItemsSource = [YeniSistem.Kayit]::Ozet($kayit)
    $oy = New-Object 'System.Collections.Generic.List[YeniSistem.Tahmin]'
    for ($i = $kayit.Count - 1; $i -ge 0; $i--) { if ($kayit[$i].Oyna) { $oy.Add($kayit[$i]) } }
    $el.GridSonuc.ItemsSource = $oy
    return $kayit
}

# açılışta: kayıttaki başlamamış maçlar (ağ beklemeden)
$simdi = [DateTime]::UtcNow.AddHours(3)
foreach ($t in (SonuclariGoster)) { if ($t.Saat -gt $simdi) { $script:liste.Add($t) } }
Suz

$zaman = New-Object Windows.Threading.DispatcherTimer
$zaman.Interval = [TimeSpan]::FromMilliseconds(500)
$zaman.Add_Tick({
    if ([YeniSistem.Motor]::Calisiyor) { Durum ([YeniSistem.Motor]::Durum) $false; return }
    $zaman.Stop(); $el.BtnGuncelle.IsEnabled = $true
    if ([YeniSistem.Motor]::Hata) { Durum ([YeniSistem.Motor]::Durum) $true; return }
    $script:liste = [YeniSistem.Motor]::Sonuc
    Suz; [void](SonuclariGoster)
    Durum ([YeniSistem.Motor]::Durum) $false
})
function Guncelle {
    $el.BtnGuncelle.IsEnabled = $false
    [YeniSistem.Motor]::Baslat($kok)
    $zaman.Start()
}

$el.BtnGuncelle.Add_Click({ Guncelle })
$el.ChkOyna.Add_Click({ Suz })
$el.ChkHesap.Add_Click({ Suz })
$el.TxtAra.Add_TextChanged({ Suz })
$el.Grid.Add_SelectionChanged({ $s = $el.Grid.SelectedItem; $el.TxtDetay.Text = $(if ($s) { $s.Aciklama() } else { '' }) })
$el.GridSonuc.Add_SelectionChanged({ $s = $el.GridSonuc.SelectedItem; if ($s) { $el.TxtDetay.Text = $s.Aciklama() } })
$pen.Add_ContentRendered({ Guncelle })
[void]$pen.ShowDialog()
