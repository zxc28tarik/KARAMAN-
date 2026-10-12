# Karamani, 2. kısım: goblin düşmanları ve boss'lar

**Tarih:** 2026-10-12, ikinci sürüm. **Durum:** taslak tasarım, kullanıcı geri bildirimiyle yeniden yazıldı.

**Kullanıcı geri bildirimi (2026-10-12):** ilk sürüm ciddiyetsizdi ve dönemine uymuyordu.
- Fantastik olsa da dönem belli: makine yok.
- Büyücüler ve silahlar olabilir.
- Tuzakçı olmaz.
- Pusucu bir anda bir yerden üstüne atlayan bir goblin olacak; arka plan buna göre yapılacak.

**Kapsam:** Karamani'nin 2. kısmı. 6 normal düşman, 2 yarı-boss, 1 boss.
**Ortak dövüş kuralları:** `2026-10-12-enemy-roster-design.md` ile aynı (⚪/🔴, denge, saldırı sırası).

## 1. Ton, dönem ve hikâye çerçevesi
- **Ton:** karanlık ve ciddi. Goblinler komik değil, vahşi ve korkutucu yağmacılar: sessiz, kurnaz, sürü hâlinde avlanan, ölüme meydan okuyan bir savaşçı kabilesi. Gülme, kaçışma ya da sakarlık yok.
- **Dönem (1566):**
  - Yalnızca dönemin silahları ve gereçleri var: kama, kılıç, mızrak, balta, sapan, yay, deri ve kemik kalkan, ip, zincir, ateşli çömlek.
  - Makine, düzenek ve top-tüfek yapımı gibi şeyler yok. Goblinler barutu da kullanmaz.
  - Fantastik taraf büyü ve yaratıklarla sınırlı: şaman büyüsü, dev domuzlar.
- **Hikâye:** Kısmet'in kabilesi (vergi veren tebaa) ile çelişmesin diye düşman goblinler ayrı ve dönek bir oymak: **Çatlak Kaya oymağı**.
  - Antlaşmayı tanımazlar.
  - Sınırın ötesindeki ormanlarda, mağaralarda ve terk edilmiş eski maden galerilerinde yaşarlar.
  - Kervan basar, köy yağmalarlar.
  - Gizli kötülükle bağları açık bırakıldı; bu hikâye kararı kullanıcının.
- **Görünüş dili:**
  - Kül-gri ve yeşilimsi soluk ten; Kısmet'in canlı yeşilinden belirgin biçimde farklı.
  - Yüzlerde ve kollarda kırmızı-siyah savaş dövmeleri, kulaklarında kemik halkalar.
  - Kaba deri, kürk ve kemikten zırhlar. Yağmaladıkları köy ve kervanlardan alınmış kumaş parçaları ve kırık kılıçlar; komik değil, ganimet olarak.
  - Sivri kulaklı, çökük yanaklı, sarı ya da kırmızımsı gözlü, keskin dişli sert yüzler.
- **Boy (Karamani 258 px):** sıradan goblin yaklaşık 175 px (%68). Kaba yaklaşık 260 px. Yarı-boss'lar ve boss aşağıda.

## 2. Goblin dövüşünün 1. kısımdan farkı
İnsanlar düzenli ve tek tek dövüşüyordu. Goblinler **sürü, pusu ve baskınla** dövüşür. Oyuncu aynı anda birden çok rakibi yönetmeyi, çevreyi okumayı ve öncelik seçmeyi öğrenir; önce şaman, önce okçu gibi.

**Ortak kurallar:**
1. **Saldırı sırası 3:** goblinler zayıf olduğu için aynı anda en fazla 3 goblin saldırır.
2. **Sürü cesareti:** liderleri (Kaba ya da Şaman) yanlarındayken daha sık ve daha hızlı saldırırlar. Lider ölünce bir an duraksar, geri çekilip yeniden toplanırlar. Kaçıp gülmezler; sinsice yeniden gelirler.
3. **Pusu noktaları (arka planla birlikte tasarlanır):** her bölümde goblinlerin gizlendiği yerler var:
   - ağaç dalları
   - kaya çıkıntıları
   - mağara tavanındaki girintiler
   - yıkık duvar üstleri
   - galeri kirişleri
   
   Bu yerler sanatta okunur olmalı, oyuncu "orası tehlikeli" diye öğrenmeli.
4. **Karamani'nin avantajı:** ağır vuruş ve kurt atılışı birden çok goblini aynı anda vurur; sıradan goblinler ağır vuruşla yere serilir.

## 3. Normal düşmanlar
Değerler başlangıç değeridir. Karamani'nin hafif vuruşu 20, ağır vuruşu 44.

### 3.1 Bıçakçı: sürünün temeli
- **Görünüş:**
  - cılız, sırım gibi
  - geniş ve paslı bir kama, deri bez
  - yüzünde kırmızı dövme
  - göğsünde kemik ve diş kolye
- **Değerler:** can 30, denge 15, hız 2.2. 2–3'lü gruplar hâlinde gelir.
- **Saldırılar:**
  - ⚪ sıçrayarak gelen 2'li kama saplaması (erim 1.1, 2 × 8)
  - ⚪ yandan dolanma: yuvarlanıp oyuncunun arkasına geçer, oradan saplar
- **Davranış:** kurt sürüsü gibi etrafında dolanır, 1.2–2 birimde sinsice bekler. Sıra gelince dalar, vurunca hemen geri sıçrar.
- **Zayıflık:** 2 hafif vuruşta ölür; savuşturulunca yere düşer.
- **Kareler:** ana poz, bekleme 4, koşu 8, saplama 6, yuvarlanıp dolanma 6, geri sıçrama 3, darbe 3, ölüm 5.

### 3.2 Zehirli Okçu
- **Görünüş:** kısa, kirişi sinirden boynuz yay; kürk sadakta siyah uçlu oklar. Yüzünün alt yarısı bezle örtülü, saçları tek örgü.
- **Değerler:** can 30, denge 10, hız 1.8. Yüksek yerleri sever.
- **Saldırılar:**
  - ⚪ zehirli ok: hasar 8, ardından 3 saniye boyunca saniyede 3 zehir hasarı
  - 🔴 ok yağmuru: 3 ok birden, düşecekleri yer yerde belli olur
- **Davranış:** 6–10 birim mesafe tutar, yaklaşınca geri çekilip bir kayaya ya da dala sıçrar.
- **Kareler:** ana poz, bekleme 4, koşu 8, ok atma 5, yukarı atış 5, geri sıçrama 3, darbe 3, ölüm 5.

### 3.3 Pusucu: üstüne atlayan goblin
- **Görünüş:**
  - kürk ve yosunla kaplı koyu pelerin
  - yüzü kül ve siyahla boyalı
  - iki kısa, kıvrık hançer
  - çok yırtıcı bir duruş: çömelmiş, kollar uzun, dişler görünür
- **Değerler:** can 40, denge 20.
- **Pusu:** büyü yok. Arka plandaki pusu noktasında (§2.3) bekler: dalın, çıkıntının, tavan girintisinin ya da kirişin üstünde, yarı karanlıkta.
  - Oyuncu altından ya da yakınından geçerken bir an önce **gözleri karanlıkta parlar ve hafif bir hışırtı duyulur.** Bu uyarı yaklaşık 0,5 saniyedir.
  - 🔴 Sonra üstüne atlar (hasar 22). Uyarıyı gören zamanında kaçar ya da geri sıçrar.
  - Atlayış ıskalanırsa yere çakılır ve 1 saniye açık kalır.
- **Yerde:** ⚪ iki hançerle 3'lü kombo, hızlı geri sıçrayış.
- **Davranış:** yere indikten sonra bir daha pusuya dönemez; sıradan bir dövüşçü gibi savaşır. Her pusucu, bölümdeki tek bir pusu noktasına bağlıdır.
- **Kareler:** ana poz, pusuda bekleme 4 (çömelmiş, yukarıda), atlayış 6, yere çakılma 4, bekleme 4, koşu 8, hançer kombosu 8, darbe 3, ölüm 5.

### 3.4 Kalkanlı Mızrakçı
- **Görünüş:** kalın derili, kemik ve gergin hayvan derisinden büyük yuvarlak kalkan, ucu kemik kakmalı uzun mızrak. Omuzlarında kürk, başında boynuzlu deri başlık.
- **Değerler:** can 70, denge 60, hız 1.2.
- **Saldırılar:**
  - ⚪ kalkanın arkasından uzun mızrak dürtüşü (erim 2.2)
  - ⚪ kalkanla itme
- **Kalkan:** 1. kısımdaki insan mızrakçı gibi önden gelen hafif vuruşu keser. Ağır vuruş ya da arkadan vuruş dengesini iki kat düşürür.
- **Fark:** goblin mızrakçılar **ikili sıra** kurar. İki kalkanlı yan yana yürür, arkalarından bir okçu atar; düzenli bir duvar. Kırmanın yolu ağır vuruş ya da üstlerinden atlayıp arkaya geçmek.
- **Kareler:** ana poz, bekleme 4, yürüme 8, dürtüş 6, kalkanla itme 5, blok 2, darbe 3, ölüm 5.

### 3.5 Ateş Çömlekçisi
- **Görünüş:** sırtında içi yanıcı yağla dolu kil çömlekler, belinde paçavralar. Elleri ve kolları eski yanıklarla dolu, yüzü sert ve isli.
- **Değerler:** can 35, denge 15, hız 1.6.
- **Saldırılar:**
  - 🔴 yanan yağ çömleği: ağzındaki paçavrayı yakıp kavisli fırlatır. Yere düştüğü yerde 4 saniye yanan bir alev alanı bırakır (içinde kalan saniyede 6 hasar). Düşeceği yer belli olur.
  - ⚪ yakında bir çömlekle vurur (kırılır, kısa bir alev)
- **Fırsat:** ölünce sırtındaki çömlekler kırılır ve olduğu yer tutuşur. Kalabalık goblinlerin yanında öldürülürse onları da yakar.
- **Kareler:** ana poz, bekleme 4, yürüme 8, çömlek fırlatma 6, yakın vuruş 5, darbe 3, ölüm (çömleklerin kırılıp tutuşması) 6.

### 3.6 Kaba: dev goblin
- **Görünüş:** Karamani boyunda, iki kat geniş, kambur, korkutucu. Sırtı yara izi ve dövmeyle dolu. Elinde zincire bağlı devasa bir satır; boynunda yağmaladığı köylerden zincirler.
- **Değerler:** can 130, denge 100, hız 1.0.
- **Saldırılar:**
  - ⚪ satırla ağır iniş (hasar 24; bloklayanın dayanıklılığını çok düşürür)
  - 🔴 zincirli satırı geniş bir yayla savurma; zıplayarak ya da geri çekilerek atlatılır
  - 🔴 yakalama: dibinde saldırmadan durursan kavrayıp yere çarpar (hasar 26)
- **Davranış:** yavaş ama durmadan ilerler; hafif vuruşlarla sendelemez, ancak denge kırılınca sendeler.
- **Kareler:** ana poz, bekleme 4, yürüme 8, ağır iniş 8, zincir savuruşu 8, yakalayıp çarpma 8, darbe 3, ölüm 6.

## 4. Yarı-boss'lar

### 4.1 Kan Şamanı Kırçıl
- **Görünüş:** yaşlı, uzun ve sıska bir goblin.
  - Yüzünde kemikten bir maske, sırtında kurt postu, boynunda kemik ve tılsım dizileri.
  - Elinde ucunda bir hayvan kafatası olan çarpık asa.
  - Kolları ritüel kesikleriyle dolu.
- **Değerler:** can 200, denge 50; can çubuğu ekranın altında.
- **Büyüler:**
  - 🔴 kara lanet: yerden yükselen kara diken halkası (yarıçap 1.5); işaretten 0.8 saniye sonra çıkar
  - ruh kurtları: 2 yarı saydam kara kurt çağırır, kısa süre saldırıp dağılırlar (her evrede bir kez)
  - kan bağı: yakındaki goblinlerin canını doldurur; önce şaman öldürülmeli
  - kemik sisi ışınlanması: sisle kaybolup uzakta belirir
- **Yakın dövüş:** ⚪ asayla 2'li vuruş; 🔴 köşeye sıkışınca asayı yere vurup itme dalgası
- **İkinci evre (canı yarıdan az):** kendi kolunu kesip kan ritüeli yapar. Etrafında kızıl bir hale oluşur; lanetleri ikişer gelir, kurtlar daha hızlı olur.
- **Kareler:**
  - hareketler: ana poz, bekleme 4, yürüme 8, asa kombosu 8, lanet büyüsü 6, kurt çağırma 6, iyileştirme 6, ışınlanma 6, kan ritüeli (evre geçişi) 6, darbe 3, ölüm 6
  - efektler: diken halkası 4, ruh kurdu 6

### 4.2 Domuz Binicisi Çatal
- **Görünüş:** demirle değil kemik plakalarla zırhlanmış, kocaman dişli bir mağara yaban domuzuna binmiş, tek kulağı kesik, sert yüzlü bir goblin savaşçı. Elinde uzun çatal mızrak, sırtında oymağın kuyruk sancağı.
- **Değerler:** binekliyken can 160, denge 80; binici yalnız 90.
- **Birinci evre (binekli):**
  - 🔴 arenayı boydan boya geçen hücum; dönüşte bir kez daha. Kaçışla ya da zıplayarak atlatılır.
  - ⚪ binekten mızrak dürtüşü (uzun erim)
  - 🔴 domuz şahlanıp ön ayaklarıyla yere vurur, çevresinde şok çıkar
- **İkinci evre:** domuzun canı biterse domuz yaralanıp kaçar. Binici yere düşer ve mızrakla hızlı bir yaya dövüşçüye dönüşür.
- **Kareler:**
  - binekli: bekleme 4, koşu 8, hücum 6, mızrak 6, şahlanma 6
  - yaya: ana poz, bekleme 4, koşu 8, mızrak kombosu 8, binekten düşüş 5, darbe 3, ölüm 6
  - domuz tek başına: kaçış 6

## 5. Boss: Oymak Beyi Kara Diş
**Kullanıcı isteği (2026-10-12):**
- yapılı, yaşlı, çift palalı, büyü yapabilen bir goblin
- atlaması ve zıplaması bol, çok hareketli, pala saldırıları çok çeşitli
- güçlü, hızlı, mekaniği iyi
- kısa mesafe arkaya ışınlanma
- havada üçgen çizen zıplamalı saldırı
- çift palayla yaklaştırmayan yatay kesik
- büyü: düşmanı kendine çekme ve havadan karakterin üstüne aralıklı düşen 3 ateş topu

Bu boss bir **düellocu**: sürü çağırmaz, yalnız ve hızlı dövüşür. Tasarım ilkesi şu: saldırıları çok çeşitli ama her birinin okunur bir hazırlığı ve bir ceza penceresi var. Ritmi öğrenen oyuncu onu yenebilir.

- **Kim:** Çatlak Kaya oymağının beyi. Yaşlı, ama yılların dövüşüyle sırım gibi kaslı, geniş omuzlu.
  - Boy: Karamani'den yaklaşık %10 uzun.
  - Uzun, kirli gri saçlar ve sakal örgüleri; çökük, keskin bir yüz; kara demirle kaplı ön dişler (adı buradan gelir).
  - Çıplak, dövmeli kollar, göğsünde kızıl ritüel dövmesi. Bel ve omuzlarda kurt postu, kemerde kemik tılsımlar.
  - Silah: her elinde bir **pala**, kıvrık ve ağır, ağızları kara.
  - Büyüsünü oymağın şamanlarından öğrenmiştir; büyü yaparken gözleri ve göğsündeki dövme kızıl parlar.
- **Değerler (başlangıç):** can 420, denge 80, koşu hızı 3.2. Karamani'den hızlı.

### 5.1 Pala saldırıları
1. ⚪ **Çift pala kombosu:** sağ-sol çapraz kesikler. 4 vuruş, her biri ayrı savuşturulabilir; dördüncüsü savuşturulursa sendeler.
2. 🔴 **Yaklaştırmayan yatay kesik:** iki palayı yana açıp kendi çevresinde yatay bir dönüşle keser (yarıçap 2.2).
   - Oyuncu dibine girdiğinde ya da arka arkaya iki vuruş yediğinde yapar.
   - Kısa bir çömelişle haber verir.
   - Zıplayarak ya da geri çekilerek atlatılır.
3. ⚪ **X atılışı:** iki palayı göğsünde çaprazlayıp öne atılır, oyuncunun içinden geçerek arkasında durur (4 birim). Savuşturulursa yere sürçer.
4. ⚪ **Yükselen kesik (havaya karşı):** oyuncu üstündeyse ya da üstünden atlamaya çalışıyorsa iki palayla yukarı doğru keser. Üstünden atlamak her zaman güvenli değildir.
5. ⚪ **Pala fırtınası (yalnız ikinci evrede):** 6 vuruşluk, giderek hızlanan çapraz kesikler. Son vuruş 🔴 aşağı doğru ağır iniş.

### 5.2 Hareket
6. **Kısa mesafe geri ışınlanma:**
   - kızıl bir kor tozu bırakarak 3 birim geriye ışınlanır
   - oyuncu yaklaştığında ya da bir saldırısı savuşturulduğunda kullanır
   - bekleme süresi 3 saniye (ikinci evrede 2)
   - ışınlanmadan sonra çoğunlukla ateş topu ya da X atılışı gelir
7. 🔴 **Üçgen hava saldırısı:** havada bir üçgen çizer.
   - (a) geriye ve yukarı sıçrar
   - (b) oyuncunun bulunduğu yere çapraz dalar ve vurur
   - (c) yerden sekip yeniden havaya fırlar
   - (d) üçgenin üçüncü köşesinden ikinci kez aşağı çakılır; çakıldığı yerde küçük bir şok çıkar
   
   Her dalıştan önce havada bir an durur ve paları parlar; iki dalış için ayrı kaçış gerekir. Son çakılıştan sonra 1,2 saniye yerde kalır; asıl ceza penceresi budur.
8. **Duvar ve kaya sekmesi:** arenadaki kaya çıkıntılarına ve kirişlere sıçrayıp oradan saldırıya geçebilir. Arena bu sıçrama noktalarıyla çizilir.

### 5.3 Büyüler
9. 🔴 **Çekme büyüsü (kara zincir):**
   - Bir elini uzatır, gözleri kızıl yanar; 0,6 saniyelik hazırlık.
   - Ardından görünmez bir güçle oyuncuyu kendine doğru çeker (en fazla 6 birim).
   - Arkasından hemen bir yatay kesik ya da kombo gelir.
   - Hazırlık sırasında kaçış yapılırsa (dokunulmazlık) çekme boşa çıkar ve 0,8 saniye açık kalır.
10. 🔴 **Gök ateşi:**
    - Palalarını göğe kaldırır; havada **3 ateş topu** belirir.
    - Oyuncunun bulunduğu yere **aralıklı** düşerler: her biri 0,5 saniye arayla ve oyuncunun o anki yerine.
    - Her birinin düşeceği yer yerde kızıl bir halka olarak 0,7 saniye önceden görünür.
    - Durmadan hareket eden oyuncu üçünden de kurtulur.
    - Büyüyü yaparken 1 saniye yerinde durur; uzaktan tabancayla vurulabilir.

### 5.4 Evreler
- **Birinci evre (can %100–50):**
  - pala kombosu, yatay kesik, X atılışı, yükselen kesik
  - üçgen saldırı (tek tur), geri ışınlanma
  - çekme büyüsü, gök ateşi (3 top)
- **İkinci evre (can %50'nin altı):**
  - Göğsündeki dövme alevlenir; bir an diz çöküp kükrer (evre geçişi, yaklaşık 1 saniye). Sonrasında daha hızlıdır, ama yaşlı bedeni bazen yorulur.
  - pala fırtınası eklenir
  - üçgen saldırı iki tur arka arkaya gelir
  - gök ateşi 5 topa çıkar
  - çekme büyüsünün ardından kesin bir kombo gelir
  - geri ışınlanma 2 saniyede bir
  - Her 4–5 saldırıdan sonra 1,5 saniye nefeslenir, palalarını yere dayayıp soluklanır. Bu ikinci evrenin güvenli ceza penceresidir.
- **Örnek ritim:** çekme → yatay kesik → geri ışınlanma → gök ateşi → X atılışı → kombo → üçgen saldırı → (yerde 1,2 sn açık).
- **Sınadığı beceri:** savuşturma ritmi, zamanlı kaçış, hareket hâlinde kalmak, ceza penceresini tanımak.

### 5.5 Kareler
- **Hareketler:** ana poz, bekleme 4, koşu 8, çift pala kombosu 12, yatay dönüş kesiği 8, X atılışı 6, yükselen kesik 6, üçgen hava saldırısı 12 (sıçrama, dalış, sekme, ikinci dalış, çakılma), geri ışınlanma 6, kaya sekmesi 5, çekme büyüsü 6, gök ateşi büyüsü 6, pala fırtınası 12, evre geçişi (kükreme) 6, nefeslenme 4, darbe 3, ölüm 6.
- **Efektler:** ateş topu 4 (düşerken) + yere çarpma 5, yer halkası 3, kara zincir (çekme) 4, kor tozu (ışınlanma) 4.

## 6. Bölgeler ve dağılım (öneri)
| Bölge | Pusu noktaları (arka plan) | Düşmanlar | Yarı-boss | Boss |
|---|---|---|---|---|
| Sisli Koru (sınır ormanı) | ağaç dalları, devrik kütükler | Bıçakçı ×4, Okçu ×2, Pusucu ×2 | – | – |
| Derin Mağaralar | tavan girintileri, kaya çıkıntıları | Bıçakçı ×3, Pusucu ×2, Kalkanlı ×2, Ateş Çömlekçisi ×1, Kaba ×1 | Kan Şamanı Kırçıl | – |
| Eski Maden Galerileri ve Kemik Tahtı | galeri kirişleri, yıkık iskeleler; boss arenasında sıçrama kayaları | Bıçakçı ×3, Kalkanlı ×2, Okçu ×1, Ateş Çömlekçisi ×2, Kaba ×1 | Domuz Binicisi Çatal | Oymak Beyi Kara Diş |

"Eski maden" burada yalnızca kazılmış galeriler ve ahşap iskele anlamında; dönemle uyumlu, makine yok. Bölge sanatı ayrı bir iş. Pusu noktaları düşman yerleşimiyle birlikte çizilmeli.

## 7. Teknik not
1. kısmın sistemi (EnemyProfiles + EnemyBrain) bu düşmanların çoğunu karşılıyor. Eklenmesi gerekenler:
- pusu noktasına bağlı atlayış ve göz parlaması uyarısı
- zehir (süreli hasar)
- yerde yanan alan
- çok hedefe vuran oyuncu saldırısı
- sürü cesareti ve yeniden toplanma
- ikili kalkan sırası
- binek (iki evreli can)
- çağrılan geçici yaratık (ruh kurdu)
- boss: oyuncuyu çekme, havada yol izleyen (üçgen) saldırı, kaya sekmesi; geri ışınlanma ve düşeceği yer işaretli ateş topları 1. kısmın büyücü ve ok yağmuru sistemlerinden uyarlanır

Bunlar 1. kısım (insan düşmanları ve 4 ağa) bittikten sonra ayrı bir uygulama planıyla yapılır.
