# Karamani: Proje Aktarım Belgesi

Bu belge, Karamani projesini başka bir yapay zekâ asistanına (örneğin ChatGPT) eksiksiz aktarmak için hazırlandı. Oyunun tasarımını, alınan bütün kararları, kodun yapısını, görsel yönü ve kalan işleri içerir.

Paket içeriği:
- `AKTARIM.md`: bu belge.
- `index.html` ve `js/`: oyunun kaynak kodu.
- `karamani_tek_dosya.html`: bütün kodun içine gömüldüğü, tek başına açılan sürüm.
- `assets/`: resim dosyası klasörü, ayar dosyası (`assets.js`) ve görsel üretim rehberi (`README.md`).
- `docs/referans/`: hedeflenen görsel tarzı gösteren 4 referans ekran görüntüsü.
- `docs/ekran/`: oyunun şu anki halinden ekran görüntüleri.

---

## 1. ChatGPT'ye ilk mesaj (kopyala-yapıştır)

> Ekteki zip, Karamani adlı 2D aksiyon oyunumun bütün projesi. Önce `AKTARIM.md` dosyasını baştan sona oku; oyunun tasarımı, verilen kararlar, kodun yapısı ve görsel hedefler orada. Oyun HTML5 + saf JavaScript, kurulum gerektirmiyor; `index.html` açılınca çalışıyor. Bu aşamada senden istediğim, karakterlerin ve arka planların `docs/referans/` klasöründeki görsellerin tarzında, resimsel kalitede çizilmesi. Görselleri `assets/README.md` içindeki kurallara göre üret: karakterler tam yandan, sağa bakan, nötr duruşta, düz arka planda. Sonra onları parçalara ayırıp `assets/assets.js` dosyasına tanımla. Kod tarafında bir şeyi değiştirmeden önce bana sor. Benimle Türkçe konuş.

---

## 2. Proje özeti

- **Oyun:** Karamani (çalışma adı; ilk karakterin unvanı).
- **Tür:** 2D yan kaydırmalı aksiyon-platform; beat 'em up ve metroidvania karışımı.
- **Ayar:** 1566, fantastik bir orta çağ. Osmanlı–Kutsal Roma (Macar) sınırı.
- **Yapı:** Warcraft 3 kampanyaları gibi sıralı bölümler. Her bölüm başka bir karakterin gözünden anlatılır.
- **Platform:** HTML5, tarayıcıda. Mobil öncelik değil.
- **Şu anki durum:** Karamani'nin ilk bölümü, "İstanbul'dan Çıkış", baştan sona oynanabilir bir prototip olarak hazır: 7 dalga, 4 kıdemli yeniçeri, ağa boss'u, tam 100 yeniçeri.
- **Takılınan nokta:** Karakter çizimlerinin kalitesi. Kodla çizilen karakterler hedeflenen resimsel kaliteye ulaşamıyor. Projeyi taşıma sebebi bu: resim üretebilen bir araçla karakter ve arka plan görsellerinin üretilmesi.

---

## 3. Oyun tasarımı

### İlham kaynakları
- Dan the Man: kalabalık düşmana karşı kombo akışı ve vuruş hissi.
- Castlevania: Symphony of the Night ve Bloodstained: atmosfer, karakter gelişimi.
- Dead Cells: tempo.
- Prince of Persia: platform.
- Warcraft 3: kampanya yapısı, karakterlerin bölümler arasında kesişmesi.
- Dövüş referansları: Odin Sphere (beş karakter, sıralı bölümler; yapımıza en yakın oyun), Muramasa: The Demon Blade (hızlı kılıç, havada kombolar), Blasphemous 2 (ağır vuruşlar, savuşturma), Otherworld Legends (kombolu dövüş, görevler arası salon).

### Dünya ve ayar
- Hikâye yalnızca Osmanlı topraklarında ve Avrupa'da geçer; Orta Asya yok.
- Bütün karakterler Osmanlı–Kutsal Roma sınır bölgesine bağlı. Yalnızca Karamani İstanbul'dan başlar.
- Gerçek tarih sadece zemindir; hikâye tamamen kurgudur. Gerçek tarihi olaylarla hikâyeyi yönlendirmeye çalışma.
- Irklar: insanlar, vampirler, kurt adamlar, goblinler kesin. Diğer ırklar henüz açık.
- Goblinler Osmanlı'ya vergi veren tebaa olarak sınıra yakın yaşar.
- Ton karışık: gotik ve karanlık anlar, epik macera ve mizah bir arada. Mizahı ağırlıklı olarak Kısmet taşır.

### Oyun yapısı
- Her bölüm yaklaşık 30 dakikalık kendi görev haritası.
- Bölümler tek yönde akar. Anahtar ya da kanca gibi küçük döngüler sadece bazı bölümlerde, nadiren.
- **Backtracking yok:** Bitmiş bir bölüme geri dönülmez.
- Ana görevin yanında yan görevler var.
- Tamamen at üstünde geçen bölümler olacak.
- Ara sahneler ileride yapılacak, şu anki kapsamın dışında.

### İlerleme ve eşyalar
- Karakterin kendisi, ilerleme ağaçları ve eşyalar eşit derecede önemli.
- **Slotlar:** Her karakterin farklı sayıda slotu var. Karamani ve Ludwig silah, Álmos yetenek slotu kullanır.
- **Ağaçlar:** Her karakterin farklı dövüş tarzı ve içinde seçilebilecek birden fazla ağaç var.
- Taşınan silah ya da eşya hareket setini değiştirir.
- Eşya kaynakları (dördü de var): rastgele ganimet; haritada sabit, el yapımı özel eşyalar; üretim (crafting); yaratık güçleri (yeteneklere bağlı).
- Seviye ve eşyalar bölümden bölüme taşınır; bir karakter sonra geri döndüğünde gücünü korur.
- **Tabanca kuralı:** Tabanca hızlı değildir. Doldurma süresi arkebüzle aynı, hasarı ve menzili daha düşük; tek avantajı iki tane taşınabilmesi.
- Demirci, simyacı ve üretim sistemini kullanıcı kendisi anlatacak; bunları kendiliğinden tasarlama.

### Karakterler

| Karakter | Irk / köken | Yaş | Mizaç | Sınıf | Başlangıç |
|---|---|---|---|---|---|
| Karamani | İnsan, Karaman / Osmanlı | 40'larında | Sert, tecrübeli | Dövüşçü | Sultan onu sınırdaki olayları araştırmaya yollar |
| Ludwig von Rosenstein | İnsan, Alman | 21 | Şımarık, aklı havada | Tank | Hocası Ernst'i kaybeder |
| Álmos de genere Kán | Vampir, Macar | 30 görünümlü, 700 yaşında | Temiz, iyi niyetli | Büyü + saldırı | Kasabası yok edilir |
| Kutluca | İnsan, kadim Türk soyu | 12-13 | Korkmuş, masum | Büyücü / yaratık formunda suikastçı | Ailesi öldürülür |
| Kısmet | Goblin, Osmanlı tebaası | — | Savsak ama şanslı | Dövüşçü-suikastçı | Klanı suçlanır, araştırmaya yollanır |

**Akıncı Çavuşu Tarık Karamani** (konuşurken "Karamani" denir, "Tarık" denmez)
- Eski Karaman sultanının torunu. Babası, annesi Osmanlı soyundan gelen üvey kardeşine taht savaşını kaybeder; tahta amcası geçer. Babası ölünce büyük dayısı olan Osmanlı padişahı onu Enderun'a, sonra Akıncı Ocağı'na gönderir.
- Zeki, itibarlı, sevilen bir savaş beyi; sert bir karakter.
- Yay, kılıç, tabanca, mızrak ve cirit ustası. Yakın dövüşte pala değil kılıç kullanır. Çok sayıda silah slotu var.
- Büyü kullanmaz. Yaşı gereği zaten gelişmiş: başta güçlüdür, asıl silahlarının kalitesiyle güçlenir; ilerleme ağacı yine de kısa olmaz.
- Çevik, güçlü, hızlı. Zırhı sağlam ama Ludwig'inki kadar değil.
- **At:** Atını yetenek olarak çağırabilir (örneğin 3 dakikada bir 15 saniye). Kısa sürelidir, çünkü atı tehlikeye atmak istemez ve at bazı yerlere giremez.
- Fantastik bir özel eşyası olacak; ne olduğu henüz belirlenmedi.
- **Açılış:** İstanbul'dan çıkarken ona düşman bir paşanın gönderdiği yüz yeniçeriyle savaşır. Yeniçeriler gelince atını uzaklaştırır. 6-7 dalga, 3-4 kıdemli yeniçeri, sonunda güçlü bir ağa. Savaşın sonunda bütün silahları yıpranıp kaybolur, yanında yalnızca özel eşyası kalır.

**Ludwig von Rosenstein**
- Zengin bir Alman soylu ailesinin oğlu, yeni şövalye olmuş. Ailesi onu "adam olsun" diye sınır ucunda yaşayan akrabası Ernst von Rosenstein'ın yanına göndermiş; bir süredir onunla görevlere çıkıyor.
- Açılış: Bir uyarıyı dinlemiyor; Ernst onu korurken ölüyor. Kampanyası şımarık bir gençten gerçek bir şövalyeye dönüşümünü anlatır.
- Silahlar: kılıç; çift tabanca ya da arkebüz; süvari çekici. Karamani'den daha az slot.
- Karamani'den yavaş ve daha az çevik; gücü aynı, zırhı çok daha sağlam.

**Álmos de genere Kán**
- Balkanlar ile Orta Avrupa arasında bir kasabada yıllardır insan gibi yaşayan bir vampir hekim. Kimseye zarar vermiyor.
- Açılış: İş için köye gidiyor; döndüğünde kasabadaki herkes ölü. Cesetler vahşice parçalanmış ve yakılmış, çıkarım yapmak çok zor. İntikam için yola çıkıyor; katil hikâye boyunca çözülüyor.
- İnsan kanı içmediği için normal vampirlerden zayıf; bu eksiği güçlü soy yetenekleriyle kapatıyor. Soyu şimdilik gizemli.
- Büyü ve saldırı karışık ağaçlar; slotları silah değil yetenek taşır.

**Kutluca**
- Çok soylu, kadim bir Türk ailesinin oğlu. Adı anlamını taşır: Tanrı armağanı bir kutla kutsanmış. Hikâyenin en önemli karakterlerinden biri.
- Açılış: Gizli bir kötü güç ailesini öldürür; asıl hedef Kutluca'dır. Saldırı içindeki kadim kurt kanını uyandırır, uyanan canavar kötü gücün adamlarını öldürür. Kutluca o geceyi hatırlamıyor.
- Yaratık formunun kökü kadim kurt kanında, ama oyuncu için ne olduğu tam belli değil; sıradan bir kurt adama benzemez.
- İnsan formunda babaannesinin öğrettiği büyülerle büyücü, yaratık formunda suikastçı. Dönüşüm başta kendiliğinden olur, ilerleme ağacıyla kontrol etmeyi öğrenir. Kampanyasında yalnızdır.
- Eşyaları: babaannesinin bastonu, babasının hançeri, annesinin tesbihi. Baston ve tesbihin özelliklerini kullanıcı anlatacak.

**Kısmet**
- Sınıra yakın yaşayan, vergi veren bir goblin klanından. Türkler yok olan kasabalardan goblinleri sorumlu tutunca klan lideri olayları araştırması için onu yollar.
- Savsak, sakar ama çok şanslı; kadronun mizahi karakteri.
- Silahlar: geniş ağızlı kısa pala / kama; uzak mesafe için dart.
- Suikastçı tarzında dövüşçü; gizlenme ve görünmezlik. Sakarlığı yüzünden yetenekleri ara sıra bozulur (örneğin ilk seviyelerde %20 ihtimal).

### Ana gizem
- Sınırdaki saldırıların arkasında gizli bir kötü güç var; asıl hedefi Kutluca.
- Kutluca'nın ailesinin katili, Álmos'un kasabasını yok edenle aynı. Bu bir sürprizdir, sona saklanır.
- Álmos katili arar, Kısmet goblinlerin suçlandığı saldırıları araştırır, Karamani sultanın emriyle olayları araştırır. Ludwig'in bağlantısı henüz belirlenmedi.
- Karamani ile Ludwig birbirini tanımıyor.

---

## 4. Görsel yön

Kullanıcının hedefi `docs/referans/` klasöründeki 4 görsel: **yüksek çözünürlüklü, resimsel, karanlık fantastik**.

- 90'lar ve 2000'lerin karanlık animelerinden (Vampire Hunter D, Berserk 1997, Hellsing) ilham alır, ama çok da karanlık değil: "sonuçta fantastik bir oyun".
- Kızıl şafak ya da ay ışıklı gece, sis, meşaleler ve mangallar, hilalli kızıl sancaklar, uçuşan korlar.
- Karakterler: kasvetli ve soluk palet. Yeniçeriler koyu kızıl dolama, deri lamel zırh, bronz işleme, kirli beyaz börk. Gövdeler aşağı doğru karanlığa gömülür; ateş tarafından sıcak kenar ışığı alırlar. Duruş alçak, geniş, saldırıya hazır. Kan bol.
- Arayüz: elmas çerçeveli portre, uçları sivri altın kenarlı barlar, sağ üstte yer adı ve görev satırları, alt ortada boss barı, altta tuş etiketli silah yuvaları, serif yazı tipi. Bu arayüz şu an oyunda uygulanmış durumda.

### Bu konuda yaşanan süreç
1. Önce kodla çizilen basit vektör karakterler ve arka plan yapıldı.
2. Kullanıcı önce piksel sanatını seçti; arka plan ve karakterler piksel sanatına çevrildi.
3. Kullanıcı referans görselleri gösterip **yüksek çözünürlüklü, resimsel** yöne geçilmesine karar verdi. Karakterler tam çözünürlüğe döndü.
4. Karakterler kodla "resimsel" çizilmeye çalışıldı (`js/painter.js`): gölge, doku, zırh, yüz detayları. Kullanıcı sonucu yeterli bulmadı: kodla çizim, referanslardaki resimsel kaliteye ulaşamıyor.
5. Bu yüzden görsellerin resim üreten bir yapay zekâ ile üretilmesi gerekiyor. Altyapı hazır (bölüm 6.4 ve `assets/README.md`).

Arka plan şu an hâlâ kodla üretilen geçici bir piksel sanatı (`js/pixelbg.js`). Resimli katmanlar eklenince otomatik olarak onların yerine geçer.

---

## 5. Prototip: Bölüm I, "İstanbul'dan Çıkış"

### Akış
1. Açılış: İstanbul, 1566, şafak. Karamani atının yanında; yeniçeriler gelince atını uzaklaştırır. Enter ile atlanabilir.
2. Sıralı 12 çarpışma; her biri kilitli bir arena. Bitince "İlerle" oku çıkar, oyuncu sağa yürüyünce sonraki başlar. Her çarpışma sonunda canın %30'u yenilenir.
3. Yol boyunca semtler: mahalle → çarşı → cami avlusu → bostanlar → Edirnekapı surları.
4. Ağa yenilince silahlar tek tek kırılır, "Yanında yalnızca özel eşyası kaldı." yazar, istatistik ekranı gelir.

| # | Çarpışma | İçerik |
|---|---|---|
| 1 | Dalga 1 | 8 kılıçlı |
| 2 | Dalga 2 | 12 (kılıçlı, tüfekli) |
| 3 | Kıdemli | Odabaşı Deli Hüseyin: çift yatağan, hızlı kombo, atılma |
| 4 | Dalga 3 | 12 (kılıçlı, mızraklı, tüfekli) |
| 5 | Dalga 4 | 14 |
| 6 | Kıdemli | Tüfekçibaşı Sarı Ahmed: nişanlı atış, bomba, tekme; 4 muhafız |
| 7 | Dalga 5 | 14 (kalkanlılar dahil) |
| 8 | Kıdemli | Topuzcu Koca Mehmed: kalkan önden vuruşları keser, yere vurunca şok dalgası |
| 9 | Dalga 6 | 14 |
| 10 | Kıdemli | Bayraktar Arslan: mızrak ve sancak, iki kez 2'şer yeniçeri çağırır |
| 11 | Dalga 7 | 12 |
| 12 | Ağa | Yeniçeri Ağası Kara Murad: 3'lü kombo, hücum, sıçrayıp çakılma, tabanca; canı %60 ve %30'a inince 3'er yeniçeri çağırır, %30 altında hızlanır |

Yeniçeri sayısı tam 100: dalgalar 86 + 4 muhafız + Bayraktar'ın 4 çağrısı + Ağa'nın 6 çağrısı. Çağrılar can "eşikleri" ile garanti: boss'un canı, çağrı yapılmadan bir sonraki eşiğin altına inemez.

### Kontroller
| Tuş | İşlev |
|---|---|
| A / D | Hareket |
| Boşluk | Zıpla (havada bir kez daha) |
| J | Saldırı; art arda basınca kombo. W+J havaya kaldırma, havada J hava kombosu, havada S+J dalış |
| K | Ağır saldırı (kalkan kırar) |
| L | Menzilli silah; yay basılı tutulup gerilir |
| Q / E | Yakın / menzilli silah değiştir |
| Shift | Kaçınma (kısa dokunulmazlık), havada bir kez |
| F | Akın: bar dolunca ekranı 4 kez boydan boya kesen hücum |
| Esc | Duraklat |

### Karamani'nin silahları
- **Kılıç:** 4 vuruşluk kombo, havaya kaldırma, 3 vuruşluk hava kombosu, ağır saldırı.
- **Mızrak:** uzun menzilli dürtmeler, süpürme, ağır saldırıda delip geçen atılma.
- **Yay:** gerdikçe hasar artar; tam gerilen ok 3 düşmanı deler.
- **Tabanca:** tek atış, yüksek hasar, 2,4 sn doldurma.
- **Cirit:** 3 hak, kendiliğinden yenilenir, 3 düşmanı deler.
- Silahlar kullanıldıkça yıpranır ama bölüm sonuna kadar kırılmaz (%12'de durur).

---

## 6. Teknik yapı

### Çalıştırma
- `index.html` dosyasına çift tıklamak yeterli. Kurulum, derleme ya da sunucu gerekmez.
- Ya da `karamani_tek_dosya.html` açılabilir. Bütün kod içine gömülüdür; ancak `assets/assets.js` içindeki ayar da gömülü olduğu için, o dosyada sonradan yapılan değişiklikleri görmez. Geliştirme için `index.html` kullanılmalı.
- Hata ayıklama için adres sonuna eklenebilir: `?enc=11` doğrudan 12. çarpışmadan (ağa) başlar; sayı 0-11 arası. `?god` hasar almazsın.
- Tarayıcı konsolunda `K.debug.step(n)` oyunu n kare ilerletir (sekme görünmezken test için).

### Kod düzeni
Saf JavaScript, modül sistemi yok. Bütün kod tek bir global nesneye, `window.K`'ya bağlanır. Dosyalar `index.html` içinde şu sırayla yüklenir:

| Dosya | Görevi |
|---|---|
| `js/core.js` | Sabitler (1280×720, zemin 610), yardımcılar, klavye girdisi, WebAudio ile üretilen sesler, parçacık efektleri (kıvılcım, kan, toz, yazı, kan izleri), kamera ve sarsıntı, `K.strike` çarpışma fonksiyonu |
| `assets/assets.js` | Kullanıcının resim dosyası ayarı (`window.KARAMANI_ASSETS`) |
| `js/assets.js` | Resimleri yükler. `bgLayers(semt)`, `charParts(isim)`, `drawPart(...)` (bir parçayı iki eklem arasına yerleştirir) |
| `js/draw.js` | Karakter iskeleti (`K.drawRig`), poz sistemi, silah çizimleri, at. Parçalı karakter varsa onu çizer |
| `js/pixelbg.js` | Geçici arka plan: piksel sanatı, 427×240'ta üretilip 3 kat büyütülür. Katmanlar, semtler, ateş ve korlar, sancaklar, sis, ışık sorgusu (`lightAt`), resimli katman desteği ve semtler arası geçiş |
| `js/render.js` | Karakterleri tam çözünürlükte işler: ince dış hat, alt karanlık, doku, ışık kaynağı yönünde kenar ışığı, ay ışığı, vurulma parlaması |
| `js/player.js` | Karamani: durum makinesi, saldırı tablosu (kombolar ve vuruş kutuları), menzilli silahlar, Akın, hasar alma, pozlar |
| `js/enemies.js` | Yeniçeri türleri, mermiler (ok, mermi, cirit, bomba, şok dalgası), düşman yapay zekâsı (saldırı jetonu sistemi), hamle yürütücü, 4 kıdemli ve ağa |
| `js/painter.js` | Karakter parçalarını kodla, yüksek çözünürlükte (4 kat) bir kez çizer ve parça sistemine kaydeder. Kullanıcının resim dosyası varsa o karakter için devreye girmez |
| `js/level.js` | Bölüm I'in 12 çarpışması, dalga içerikleri, arena konumları |
| `js/main.js` | Oyun döngüsü, durumlar (başlık, açılış, oyun, duraklatma, ölüm ve tekrar, bitiş, zafer), arayüz çizimi, `K.debug` |

### 6.1 İskelet
- Her karakter aynı iskeleti kullanır. Ölçüler (`K.RIG`, birim): uyluk 30, baldır 31, gövde 44, üst kol 22, alt kol 21; baş merkezi omuzdan 12 birim yukarıda, baş uzunluğu 20.
- Pozlar açılarla tanımlanır (`K.pose`): `lean` gövde eğimi, `ft/fs` ön uyluk/baldır, `bt/bs` arka bacak, `au/af` ön kol, `bu/bf` arka kol, `wa` silah açısı, `bob` kalça yüksekliği, `rot` bütün bedenin dönüşü. Uzuv açıları dikey aşağıdan ölçülür, pozitif değer ileri demektir.
- Karakter sağa bakar; sola bakarken yatayda aynalanır.
- Görünüşler `K.defineLook(isim, {...})` ile tanımlanır: renkler, şapka türü (`janissary`, `wrap`, `aga`, `kavuk`, `akinci`), sakal, zırh (`mail`, `armor: 'leather'`, `pauldron`), pelerin, kalkan ve benzeri.

### 6.2 Karakter görselleri (parça sistemi)
`assets/assets.js` içinde bir karakter tanımlanırsa oyun kod çizimi yerine o resimleri kullanır:

```js
characters: {
  karamani: {
    parts: {
      torso: { src: 'assets/chars/karamani/torso.png', a: [x, y], b: [x, y] },
      head: { ... }, upperArmF: { ... }, foreArmF: { ... }, upperArmB: { ... }, foreArmB: { ... },
      thighF: { ... }, shinF: { ... }, thighB: { ... }, shinB: { ... },
      skirt: { ... },  // isteğe bağlı
      cape: { ... },   // isteğe bağlı
      shield: { ... }, // isteğe bağlı
    },
    weapons: { kilic: { src: '...', a: [x, y], b: [x, y] } },
  },
}
```

- `a` ve `b` resim pikseli cinsinden iki tutturma noktası. Oyun parçayı, `a` iskeletteki üst ekleme, `b` alt ekleme denk gelecek şekilde döndürüp ölçekler.
- Karşılıklar: `torso` a = kalça ortası, b = boyun; `head` a = çene/boyun, b = kafa tepesi (şapka hariç); kollarda a = omuz/dirsek, b = dirsek/yumruk ortası; bacaklarda a = kalça/diz, b = diz/ayak bileği; `skirt` a = bel, b = etek ucu ortası; `cape` a = omuz, b = pelerin ucu.
- Silahlar yatay ve sağa doğru çizilir: a = kabza, b = uç. Yay dikey çizilir: a = kabza, b = üst uç.
- Karakter isimleri: `karamani`, `kilicli`, `mizrakli`, `tufekli`, `kalkanli`, `odabasi`, `tufekcibasi`, `topuzcu`, `bayraktar`, `aga`.
- Parçalar iskelete takıldığı için bütün animasyonlar, kombolar ve efektler (kontur, kenar ışığı, vurulma parlaması) resimlerle de çalışır.

### 6.3 Arka plan katmanları
```js
backgrounds: {
  mahalle: [
    { src: 'assets/bg/mahalle/1-gokyuzu.png', parallax: 0 },
    { src: 'assets/bg/mahalle/2-uzak.png', parallax: 0.1 },
    { src: 'assets/bg/mahalle/3-orta.png', parallax: 0.45 },
    { src: 'assets/bg/mahalle/4-yakin.png', parallax: 1 },
  ],
}
```
- Semtler: `mahalle`, `carsi`, `cami`, `bostan`, `sur`.
- Resim yüksekliği ekrana sığdırılır, yatayda tekrar eder. Zemin çizgisi resim yüksekliğinin %85'inde olmalı.
- Semt sınırına yaklaşınca iki semtin katmanları yumuşakça karışır.

### 6.4 Görsel üretim kuralları
Ayrıntılar `assets/README.md` içinde. Özet:
- **Karakter:** tam yandan, sağa bakan, nötr duruş (kollar gövdeden biraz ayrık, bacaklar hafif açık), bütün beden görünür, düz tek renk arka plan, elde silah yok, yaklaşık 800 piksel boy. En pratik yol: karakteri tek parça üretip parçalara kesmek. Eklem uçları biraz taşkın kesilmeli ki dönerken boşluk görünmesin.
- **Silah:** ayrı görsel, yatay, kabza solda.
- **Arka plan:** semt başına 4 katman, 16:9, en az 2560×1440, yatayda kusursuz tekrar eden; 2-4 numaralı katmanlar saydam arka planlı.
- **Tarz:** `docs/referans/` içindeki görsellerle aynı.

### 6.5 Önemli sistemler
- **Düşman yapay zekâsı:** Aynı anda en fazla 2-3 yeniçeri saldırabilir (`G.maxTokens`); diğerleri çevrede dolaşır. Saldırıdan önce silah parlar ve gözleri kızıl yanar.
- **Vuruş hissi:** vuruşta kısa donma (hitstop), ekran sarsıntısı, geri itme, kıvılcım, kan fışkırması, hasar sayıları.
- **Boss dayanıklılığı (poise):** Kıdemliler ve ağa hafif vuruşlarda sendelemez; dayanıklılıkları bitince sarsılırlar.
- **Ölüm ve tekrar:** Oyuncu ölürse o çarpışma, başındaki duruma dönülerek yeniden başlar.
- **Performans:** Kare başına yaklaşık 4 milisaniye.

---

## 7. Kullanıcıyla çalışma tercihleri
- Türkçe konuş.
- İlk karaktere "Karamani" de, "Tarık" deme.
- Gerçek tarihi olaylarla hikâyeyi yönlendirme; 1566 yalnızca zemin.
- HTML'de en hızlı ve en iyi sonucu veren yolu seç; ileride başka bir motora (Unity gibi) geçiş ihtimali bugünkü kararları etkilemesin.
- Kullanıcının kendisinin anlatacağını söylediği konuları (demirci, simyacı, üretim; Kutluca'nın bastonu ve tesbihi; Karamani'nin özel eşyası; atın hikâyeye yedirilmesi) kendiliğinden tasarlama, sor.
- Kullanıcı gerçekçi, sağlam motivasyonlu hikâyeler istiyor; klişe fantastik senaryoları beğenmiyor.

---

## 8. Açık konular ve sıradaki adımlar

### Hemen sırada
1. Karamani'nin resimsel görselini üretip parça sistemine bağlamak.
2. Bir yeniçeri görseli üretmek; diğer yeniçeri türleri renk ve teçhizat değiştirilerek türetilebilir.
3. Kıdemliler ve ağa.
4. Beş semtin arka plan katmanları.
5. Kullanıcının daha önce istediği bir öneri: kamerayı yakınlaştırıp karakterleri büyütmek.

### Tasarımda karar bekleyenler
- Silah sistemi ayrıntıları ve her karakterin slot sayısı.
- Kutluca ile Kısmet'in slot yapısı.
- Kutluca'nın bastonu ile tesbihinin özellikleri.
- Demirci, simyacı ve üretim sistemi.
- Ludwig'in ana gizemle bağlantısı ve Ernst'le akrabalığı.
- Álmos'un soyu.
- Karamani'nin fantastik özel eşyası (prototipin sonunda yer tutucu bir satır var).
- Vampir, kurt adam ve goblin dışındaki ırklar.
- Atın hikâyeye nasıl yedirileceği.
- New Game+ olacak mı.
