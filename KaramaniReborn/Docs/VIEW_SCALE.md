# SOTN esinli ekran ve karakter ölçüsü

Kullanıcı tercihi: "Castlevania SOTN oyunu ölçülerini tercih edelim boyut olarak."

Önceki temel öncelik olan yüksek ayrıntılı çizimleri koruma isteği geçerlidir. Bu nedenle ekran kompozisyonu örnek alındı; düşük iç çözünürlük, piksel filtresi, SOTN'dan kopyalanmış karakter / harita / arayüz kullanılmadı.

## Karşılaştırma kaynağı

[Konami temsilcisinin PlayStation Blog'da yayımladığı resmî Requiem duyurusu ve oyun görüntüleri](https://blog.playstation.com/2018/09/26/castlevania-symphony-of-the-night-rondo-of-blood-haunt-ps4-october-26/).

[İncelenen SOTN görüntüsü](https://farm2.staticflickr.com/1971/44825447242_ed4b6b2d01_k.jpg): Maria ile Alucard'ın bulunduğu oda. Dış dekoratif çerçeve oynanış alanına dahil edilmedi. Görseldeki iç alan yaklaşık 4:3, ayakta Alucard yaklaşık alan yüksekliğinin %23'ü olarak değerlendirildi. Bunlar ekran görüntüsünden çıkarılan yaklaşık kompozisyon ölçüleridir; orijinal oyunun dahili çözünürlüğü veya bütün odalarının kesin ölçüleri olduğu iddia edilmez. Referans görüntüsü oyunun varlıklarına eklenmedi.

## Uygulanan ölçüler

| Ölçü | Tercih |
| --- | --- |
| Oynanış alanı | 4:3; önceki 16:9'dan daraltıldı |
| Yüksek çözünürlüklü render / pencere varsayılanı | 1440 × 1080 |
| Hareket önizlemesi | 960 × 720; yine 4:3 |
| Kameranın görünen dünya alanı | 14.4 × 10.8 Unity birimi |
| Karakter–ekran oranı | Ayakta yaklaşık %23 yükseklik; önceki fiziksel boy korunuyor |
| Arayüz koordinat alanı | 1920 × 1440; üst, alt ve merkez grupları ayrı yerleştiriliyor |
| Geniş ekranda görüntü | Merkezde 4:3, iki yanda siyah pay |

Tam ekran gerçek monitör çözünürlüğünü kullanır. 1440 × 1080 sabit düşük iç çözünürlük değildir; kamera her pencereye / monitöre oranı bozmadan çizilir. Kaynak PNG'ler değişmez ve sıkıştırılmaz. Çizimlerdeki kemik uzunluğu, karakter dünya ölçeği ve hareket / çarpışma sistemi değiştirilmedi.

Tek ölçü kaynağı `Assets/Scripts/GameView.cs` dosyasıdır. Kamera, arayüz, derleme varsayılanı ve inceleme çıktıları aynı profili kullanır. Ekran payları ayrı bir temizleme kamerasıyla siyah tutulur; çözünürlük / tam ekran değişiminden sonra eski kare izleri kalmaz.

## Denetim

Editör denetimi 4:3, 16:9, ultra geniş ve dikey pencere ölçülerinde yatay / dikey piksel ölçeğinin eşitliğini hesaplar. Menü, duraklatma ve oyun arayüzlerinin görünen 4:3 alanın içinde kaldığını; karakter yüksekliğinin beklenen aralıkta olduğunu denetler. Hareket, eklem, basan ayak, zıplama ve savaş denetimleri tekrar çalıştırılır. Gerçek Windows oyuncusu ayrıca kendi penceresindeki 4:3 alanın oranını doğrular.


## Güncelleme 2026-10-11
Kullanıcı karakteri ve dünyayı ekranda fazla büyük buldu ("soth gibi küçük"). Görüş alanı 4/3 genişletildi: 480×360 oyun pikseli, ortografik boyut 7,2, oyun pikseli 0,04 birim (değişmedi). Karakter artık görüş yüksekliğinin ~%18'i; fizik ve dövüş mesafeleri aynı.
Aynı gün ikinci güncelleme: kullanıcı %12 istedi. Görüş 720×540 oyun pikseli (1080 satırda tam 2×), ortografik boyut 10,8, karakter ~%11,9. Uzak manzara görüş yüksekliğine göre ölçeklenir, kemerlerin altı koyu dolguyla kapanır, zemin ekranın alttan ~%25'inde.
