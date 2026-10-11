// Karamani görsel dosyaları ayarı.
// Bir resim eklemek için dosyayı assets/ altına koy ve burada tanımla. Boş bırakılan her şey
// oyunun kodla çizdiği geçici görünümle gösterilir. Ayrıntılar: assets/README.md
window.KARAMANI_ASSETS = {
  // Semt arka planları. Katmanlar arkadan öne sıralanır.
  // parallax: 0 = sabit (gökyüzü), 1 = sokakla birlikte kayar. Resim yüksekliği ekrana sığdırılır,
  // yatayda tekrar eder.
  // Semtler: mahalle, carsi, cami, bostan, sur
  backgrounds: {
    // mahalle: [
    //   { src: 'assets/bg/mahalle/1-gokyuzu.png', parallax: 0 },
    //   { src: 'assets/bg/mahalle/2-uzak.png', parallax: 0.1 },
    //   { src: 'assets/bg/mahalle/3-orta.png', parallax: 0.45 },
    //   { src: 'assets/bg/mahalle/4-yakin.png', parallax: 1 },
    // ],
  },

  // Parçalı karakterler. Her parça için iki tutturma noktası (resim pikseli olarak):
  //   a = üst eklem (omuz, kalça, diz, dirsek, boyun...), b = alt eklem ya da uç.
  // Silahlarda a = kabza (elin tuttuğu yer), b = uç.
  characters: {
    // karamani: {
    //   parts: {
    //     torso:     { src: 'assets/chars/karamani/torso.png',     a: [60, 180], b: [62, 20] },
    //     head:      { src: 'assets/chars/karamani/head.png',      a: [40, 90],  b: [40, 10] },
    //     upperArmF: { src: 'assets/chars/karamani/upperArmF.png', a: [20, 10],  b: [20, 80] },
    //     ...
    //   },
    //   weapons: {
    //     kilic: { src: 'assets/chars/karamani/kilic.png', a: [30, 20], b: [280, 20] },
    //   },
    // },
  },
};
