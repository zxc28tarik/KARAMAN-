# Karamani: Görsel Üretim Rehberi

Oyun, resim dosyası bulamadığı her şeyi kodla çizdiği geçici görünümle gösterir. Bir resmi bu klasöre koyup `assets/assets.js` içinde tanımladığın anda oyun onu kullanmaya başlar. Tanımlamayı istersen bana bırakabilirsin: dosyaları klasöre koyman yeterli, noktaları ben ölçüp ayar dosyasını doldururum.

## Klasör yapısı

```
assets/
  assets.js            hangi resmin nerede kullanılacağı
  bg/<semt>/           arka plan katmanları
  chars/<karakter>/    karakter parçaları ve silahları
```

Semtler: `mahalle`, `carsi`, `cami`, `bostan`, `sur`

## 1. Arka plan katmanları

Her semt için 4 ayrı resim. Arkadan öne doğru:

| Dosya | İçerik | Saydamlık | Kayma |
|---|---|---|---|
| `1-gokyuzu.png` | Gökyüzü, ay, bulutlar | Opak | Sabit |
| `2-uzak.png` | Uzak cami silüetleri, tepeler | Saydam arka plan | Çok yavaş |
| `3-orta.png` | Konaklar, çatılar, ağaçlar | Saydam arka plan | Orta |
| `4-yakin.png` | Sokak cephesi ve zemin | Saydam üst kısım | Karakterlerle birlikte |

Kurallar:
- **Boyut:** 16:9 oranında, en az 2560×1440. Daha geniş bir panorama da olur, örneğin 5120×1440.
- **Yatay döngü:** Resimler yatayda tekrar edecek. Sol ve sağ kenar birbirine kusursuz bağlanmalı ("seamless, tileable horizontally").
- **Zemin çizgisi:** `4-yakin.png` içinde karakterlerin bastığı zemin, resim yüksekliğinin **%85**'inde olmalı (1440 yükseklikte 1224. piksel).
- **Işık:** Sahne şafakta. Kızıl ışık sağdan, gölgeler sola. Gece karanlığı soğuk ve mor-lacivert, ateşler ve pencereler sıcak.
- **Saydam arka plan:** Araç saydam üretemiyorsa düz yeşil ya da magenta bir zeminde üret, sonra bir arka plan silme aracıyla temizle (örneğin Photopea veya remove.bg).

### Semtler

- **mahalle:** Cumbalı ahşap Osmanlı konakları, taş zemin katlar, bahçe duvarları, çeşme, sokak fenerleri.
- **carsi:** Tenteli dükkânlar, baharat ve bakır kaplar, asılı halılar, kemerli han kapıları, mangallar.
- **cami:** Pencereli uzun avlu duvarı, mukarnaslı taç kapı, serviler, arkada büyük bir kubbe ve minareler.
- **bostan:** Sur dibinde bostanlar, çitler, meyve ağaçları, kuyu, alçak evler.
- **sur:** Taş ve tuğla hatıllı İstanbul surları, kuleler, kızıl sancaklar, mangallar; ortada içinden şafak ışığı vuran büyük kapı.

### Örnek komut (İngilizce daha iyi sonuç verir)

> 2D side-scrolling game background layer, Ottoman Istanbul 1566, narrow street with wooden Ottoman houses with bay windows (cumba), dark fantasy, 90s dark anime atmosphere, blood-red dawn, moonlit night fading to sunrise, warm lit windows, painterly, highly detailed, orthographic side view, no characters, seamless tileable horizontally, 16:9

Bunu her katman için uyarla. Uzak katman için örneğin: "only distant mosque silhouettes and minarets on transparent background".

## 2. Karakter parçaları

Karakterler kemik sistemiyle hareket ediyor. Resim parçaları bu kemiklere takılınca bütün animasyonlar, kombolar ve vuruşlar kendiliğinden çalışır. Her karakter için ayrı PNG'ler gerekiyor:

| Parça | İçerik |
|---|---|
| `torso` | Gövde: kalçadan boyna (kaftan üst kısmı, kuşak) |
| `head` | Baş: börk ve sakal dahil |
| `upperArmF`, `foreArmF` | Öndeki kolun üst ve alt kısmı (el, alt kısma dahil) |
| `upperArmB`, `foreArmB` | Arkadaki kol (biraz daha koyu) |
| `thighF`, `shinF` | Öndeki bacağın uyluk ve baldır kısmı (çizme, baldıra dahil) |
| `thighB`, `shinB` | Arkadaki bacak (biraz daha koyu) |
| `skirt` | İsteğe bağlı: kaftanın etek kısmı |
| `cape` | İsteğe bağlı: pelerin |

Kurallar:
- **Görünüş:** Tam yandan, **sağa bakan**. Kollar aşağıda, bacaklar düz, nötr bir duruş.
- **Boyut:** Bütün karakter yaklaşık 800 piksel boyunda olacak şekilde, bütün parçalar aynı ölçekte.
- **Eklemler:** Parçaların eklem uçları biraz yuvarlak ve taşkın olsun; dönerken arada boşluk görünmesin.
- **Işık:** Bütün parçalarda aynı yönden. Kontur ve kenar ışığını oyun kendisi ekliyor.
- **Saydam arka plan:** Her parça kendi dosyasında, saydam zeminde.

**En pratik yol:** Karakteri tek parça olarak yandan, nötr duruşta üret. Sonra bir resim düzenleyicide (Photopea ücretsiz ve tarayıcıda çalışıyor) parçalara kes. Yapay zekâya doğrudan "parça parça" çizdirmek genelde tutarsız sonuç veriyor.

### Silahlar

Karamani: `kilic`, `mizrak`, `yay`, `tabanca`, `cirit`. Yeniçeriler: `yatagan`, `mizrak`, `tufek`. Kıdemliler: `topuz`, `bayrak`.

Her silah **yatay ve sağa doğru** çizilmeli, kabza solda. Yay ise **dikey** çizilmeli, kabza ortada.

### Tutturma noktaları

Her parçanın iki noktası var: `a` üst eklem (omuz, kalça, diz, dirsek, boyun), `b` alt eklem ya da uç. Silahta `a` kabza, `b` uç. Bu noktaları dosyalara bakarak ben ölçebilirim.

## 3. Sırası

1. Önce **Karamani** ve **mahalle** arka planı. Tek bir karakter ve tek bir semtle bütün sistem sınanmış olur.
2. Sonra sıradan yeniçeri (bir tane yeterli; renkleri kodla değiştirip kılıçlı, mızraklı, tüfekli ve kalkanlı türevlerini çıkarabilirim).
3. Sonra kıdemliler, ağa ve diğer semtler.
