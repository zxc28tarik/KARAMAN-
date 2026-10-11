# Karamani Reborn

Dört görsel referansın sanat yönünü temel alan, sıfırdan kurulmuş yerel Unity oyunu. Önceki HTML uygulaması ve eski `KaramaniUnity` projesi değiştirilmedi.

## Oyunu aç

`Build/KaramaniReborn.exe` dosyasına çift tıkla. Ana menüden bir bölge seç ve **Yolculuğa Başla** düğmesine bas. Oyuncunun bilgisayarında Unity'nin kurulu olması gerekmez.

EXE'yi tek başına başka yere taşıma. `KaramaniReborn_Data`, `UnityPlayer.dll`, `MonoBleedingEdge` ve diğer derleme dosyaları aynı klasörde kalmalı. Windows sürümü 64 bit, Direct3D 11 kullanır. Menüden pencere / tam ekran değiştirilebilir.

## SOTN boyut tercihi

Oynanış alanı **4:3** oranındadır; yüksek çözünürlüklü inceleme çıktısı 1440 × 1080'dir. Geniş ekranda yanlarda siyah pay bırakılır; görüntü yatayda esnetilmez. Dar / dikey pencerede aynı oran üst ve alt paylarla korunur.

Symphony of the Night'ın karakter–mekân oranı örnek alındı, düşük çözünürlüklü piksel görünümü uygulanmadı. Karakterin boyu yaklaşık ekran yüksekliğinin %23'üdür. Kamera yüksekliği, karakterlerin fiziksel ölçüleri, adım mesafesi, zıplama, darbe mesafesi ve kaynak çizimler korunmuştur. Menü ve oyun göstergeleri 4:3 alanın içine yeniden yerleştirilmiştir. Ölçü kararı ve referans `Docs/VIEW_SCALE.md` dosyasındadır.

## Bu sürümde

- Dört oynanabilir bölüm: Kızıl Surlar, Yanmış Mahalle, Kurt Geçidi ve Karanlık Kale. Her bölümde beş muhafız ve bir kapı komutanı bulunur.
- Ayrı çizilmiş arka plan, kemerli sur, ahşap köprü, eşya ve karakter katmanları; kamera takibi, uzak plan hareketi, sis, köz, kar ve sıcak ışıklar.
- Yürüme / koşma, değişken yüksekliğe sahip zıplama, platformlara iniş ve aşağı atlama.
- Tamponlanabilen üçlü kılıç kombosu, ağır darbe, blok, zamanlamalı savuşturma, kaçış, kurt atılışı, tabanca ve iyileştirme.
- Yakın dövüş, kalkan ve okçu davranışları; savunma kırma, sersemleme, dayanıklılık ve kapı komutanları.
- Yol taşında dinlenme, kayıt, ölümden sonra yol taşında yeniden doğma; komutan yenildikten sonra kapıdan sonraki bölüme geçiş.
- Yerel Unity menüsü, can / dayanıklılık göstergeleri, düşman göstergeleri, mini harita, bölüm seçimi ve fotoğraf görünümü.

## Kontroller

| Tuş | İşlev |
| --- | --- |
| A / D veya sol / sağ ok | Hareket |
| Ctrl | Yavaş yürüme |
| Boşluk | Zıplama; basılı tutmak daha yüksek sıçratır; havada tekrar basınca taklalı ikinci zıplama |
| S + Boşluk | Yükseltilmiş platformdan aşağı atlama |
| W | Kenara asılıyken yukarı tırmanma |
| Havada S + sol tık | Kılıçla aşağı dalış; düşmana isabet edince geri seker, ikinci zıplama yenilenir |
| Sol tık veya J | Hafif saldırı; saldırı sırasında tekrar basarak kombo |
| F | Ağır saldırı; kalkan savunmasını kırar |
| Sağ tık veya K | Blok; darbeden hemen önce basmak savuşturur. Savuşturmadan sonra saldırı: sersemleyen rakibe infaz |
| Shift + yön | Kaçış / yuvarlanma |
| Yalnız Shift | Hızlı geri adım; ortasında saldırıya geçilebilir |
| Q | Kurt atılışı |
| R | Tabanca |
| H | İksir |
| E | Yol taşında dinlenme / açılan kapıdan geçme |
| Esc | Duraklatma |
| F2 | Oyun arayüzünü gizle / göster |

Yol taşları yaklaşık x=2, 21 ve 39 konumlarında bulunur. Kapıya yaklaşırken komutanı yen; açılan kapıda E'ye bas. Kayıt, `%USERPROFILE%/AppData/LocalLow/Karamani/KaramaniReborn/journey.json` içinde yereldir. Yeni yolculuğa başlama seçili bölgenin kaydını yeniler.

## Çizimler ve tutarlı hareket

Karakterler tek bir resmin esnetilmesiyle hareket etmez. Baş, gövde, kalça, kumaş, üst / alt kollar, üst / alt bacaklar, ayakkabılar ve silahlar ayrı yüksek ayrıntılı boyalı parçalardır. Her parça yalnızca çevrilir ve döndürülür; animasyonda en / boy oranı değişmez. İki parçalı eklem çözümü kemik uzunluklarını sabit tutar. Basma evresindeki ayaklar dünya koordinatına kilitlenir; adım ritmi yürünmüş mesafeye bağlıdır. Silah hareketi el eklemine bağlıdır ve hasar çizilen kılıcın darbe zamanına göre hesaplanır.

Kaynak PNG'ler `Assets/Resources/Art` içindedir. Unity bunları sıkıştırmasız, orijinal çözünürlükte kullanır. Yalnızca sprite dikdörtgenleri / pivotları oluşturulur; kaynak resimler üzerinde sonradan piksel değişikliği yapılmaz. Üretim yöntemi ve normalize edilmiş görsel yönergeler `Docs/ART_DIRECTION.md` dosyasındadır.

## Unity'de geliştirme

Unity Hub'a bu klasörü proje olarak ekle. Kullanılan sürüm **Unity 6000.6.0f1**. `Assets/Scenes/Karamani.unity` sahnesini açıp Play'e bas. Başlangıç sahnesi dünyayı çalışma zamanında kurar; editörde katmanları görmek için `Karamani > Sahneyi önizle` komutu kullanılabilir.

Menüdeki `Karamani > Hareket ve savaş doğrulaması` komutu sayısal testleri çalıştırır ve Unity kameralarından `Review` klasörüne görüntüler üretir. `Karamani > Windows sürümünü derle` komutu Windows oyununu yeniden derler. Aynı işlemler `BUILD_WINDOWS.ps1` ve `VERIFY_WINDOWS.ps1` ile yapılabilir. Normal EXE açılışında doğrulama sürücüsü çalışmaz.

## Doğrulama ve kapsam

`Review/verification.txt`: ekranda çizilen ayak pivotları, kemik uçları, sabit parça ölçekleri, zıplama, kombo, kılıç teması, savuşturma, kaçış, kayıt noktası ve dört kapı geçişi için editör denetimi.

`Review/player-verification.txt`: derlenmiş gerçek Windows oyuncusunun ana menü, hareket, zıplama, kombo, duraklatma, yeniden doğma ve dört bölüm açılış denetimi. `Review/player-*.png` dosyaları EXE'nin kendi kamerasından alınan görüntülerdir. `Review/animation` ve `Review/poses` Unity'deki hareket / duruş incelemelerini içerir. `Review/motion-preview.gif` hareketin oynatılabilir önizlemesidir. Ayrıntılı doğrulama kaydı `Docs/QA.md` dosyasındadır.

Bu, görsel bütünlüğe odaklanan ilk oynanabilir sürümdür; bitmiş AAA oyun veya referanslarla birebir kalite eşitliği iddiası değildir. Dört bölümün yerleşim ve düşman görsel ailesi ortak bir üretim dilimini kullanır. Hareketler eklem tabanlıdır; elle çizilmiş kare-kare animasyon paketi, her bölgeye özgü düşman modelleri, profesyonel ses / müzik, hikâye sinematikleri ve geniş içerik üretimi henüz tamamlanmış değildir. Merdivenler bu sürümde dekoratiftir; yüksek platformlara zıplanır.
