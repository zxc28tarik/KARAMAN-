# Karamani Reborn — doğrulama kaydı

Tarih: 8 Ekim 2026. Unity: 6000.6.0f1, Windows 64 bit, Direct3D 11. Gerçek oyuncu denetiminin ekran kartı: NVIDIA GeForce MX130. Bu kayıt bir performans / FPS kıyaslaması değildir.

## Çizim koruması

Üretilen on PNG'nin SHA-256 değerleri proje içindeki kopyalarıyla karşılaştırıldı: **10 / 10 değişmemiş**. Unity'ye alınan doku boyutları kaynak PNG başlıklarıyla otomatik olarak karşılaştırıldı: **10 / 10 birebir aynı**.

| Dosya ailesi | Kaynak / Unity boyutu |
| --- | --- |
| Dört background PNG | Her biri 1672 × 941 |
| Hero, guard, props, equipment atlasları | Her biri 1254 × 1254 |
| Kemerli sur | 1942 × 809 |
| Taş platform | 2089 × 753 |

İçe aktarmada güç-ikiye yuvarlama kapalıdır (`NPOTScale.None`); mipmap ve doku sıkıştırması kapalıdır. Sprite'lar doğrudan kaynak doku dikdörtgenlerini kullanır. Animasyon parçalarının ölçekleri sabittir. Dünya ölçeği de her karakter için tek ve eşittir; yalnızca sağ / sol ayna işareti değişir.

## Oyun ve hareket denetimi

- SOTN esinli 4:3 sunum: 1440 × 1080, 1920 × 1080, 2560 × 1080 ve 1080 × 1920 pencere ölçülerinde eşit yatay / dikey piksel ölçeği ve merkezlenmiş oyun alanı denetlendi. Menü, duraklatma ve oyun arayüzünün 4:3 görünümden taşmadığı kontrol edildi. Ayrıntılar ve referans `VIEW_SCALE.md` dosyasındadır.
- Görünen ayak pivotlarıyla 563 basma örneği: kayma < 0.001 Unity birimi. Görünen kol / bacak uçlarıyla sabit kemik uzunluğu: hata < 0.001 birim.
- Zıplama: kalkış, 1.8561 birim tepe yüksekliği ve yere iniş doğrulandı.
- Simülasyon adımı olmayan yüksek yenileme hızı karelerinde ve darbe duraklamasında tuş kenarlarının tutulması / bir kez tüketilmesi doğrulandı.
- Kılıç doğrusu / beden kesişimi, tamponlu üçlü kombo, zamanlı savuşturma ve kaçış dokunulmazlığı doğrulandı.
- Tabancanın eldeki silah konumundan ateş etmesi, cephane tüketimi, mermi teması ve yüksek konumdaki okçunun aşağıdaki oyuncuya nişan alması doğrulandı.
- Yol taşı etkileşimi, ölüm / yeniden doğma, komutan ölümüyle kapının açılması, dört kapı geçişi ve yolculuk sonu doğrulandı.
- Boyalı sur üst yüzeyi ile fiziksel zemin arasındaki hata < 0.001 birim. Dört bölüm gerçek Unity kamerasıyla kontrol edildi.

## EXE denetimi

Derlenmiş Windows oyuncusunda üretim başlangıcı, dinamik çizimler ve yerel arayüz oluşturuldu. Başla düğmesinin gerçek olay işleyicisi çağrıldı; hareket, zıplama, kombo, duraklatma / devam, yol taşı / yeniden doğma ve dört bölüm açılışı çalıştırıldı. Oyuncu başarı koduyla kapandı; günlükte `KARAMANI_PLAYER_VERIFY_SUCCESS` kaydı bulunuyor.

16:9 test penceresinde ortada 4:3 alan bulunur. `Review/player-window.png`, bu pencerenin ölçüsünde üretim kameralarının açıkça çizdirildiği görüntüdür; otomatik / gizli testte son ekran tamponu yerine Unity RenderTexture kullanılır. İç alanın boş olmadığı ve iki yandaki payların siyah olduğu birlikte denetlenir. Bu dosya işletim sistemi ekran görüntüsü değildir.

Bu denetim otomatik bir simülasyon sürücüsüdür; baştan sona uzun süreli elle oynanmış kampanya veya bütün bilgisayarlarda uyumluluk testi değildir. Normal oyunda doğrulama sürücüsü etkinleşmez ve klavye / fare kontrolü kullanılır. Denetim kullanıcı kaydına yazmaz.

## İnceleme dosyaları

`Review/verification.txt`, `Review/player-verification.txt`, `Review/player.log` ve proje kökündeki `verify.log` / `build.log` denetim sonuçlarını içerir.

`Review/player-*.png` derlenmiş EXE'nin kamerasından alınmıştır; yeni bölüm / menü çıktıları 1440 × 1080 (4:3) boyutundadır. `Review/chapter-*.png`, `Review/poses` ve `Review/animation` editörün gerçek Unity renderlarıdır. `Review/motion-preview.gif` bu hareket karelerinin 960 × 720, 38 karelik oynatılabilir dışa aktarımıdır. GIF renk paleti videolu önizleme içindir; oyunun orijinal PNG dokularının yerine kullanılmaz.

Kapsam ve henüz tamamlanmayan üretim işleri için proje kökündeki `README.md` dosyasına bak.
