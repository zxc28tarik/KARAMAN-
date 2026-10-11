'use strict';
// assets/assets.js içindeki resimleri yükler; arka plan katmanlarını ve karakter parçalarını hazırlar.
(function (K) {
  const CFG = window.KARAMANI_ASSETS || { backgrounds: {}, characters: {} };
  const imgs = [];
  function load(src) {
    const im = new Image();
    im.src = src;
    im.onerror = () => console.warn('Görsel yüklenemedi:', src);
    imgs.push(im);
    return im;
  }
  const ok = im => !!im && (im instanceof HTMLCanvasElement || (im.complete && im.naturalWidth > 0));

  const bg = {};
  for (const d in CFG.backgrounds || {}) bg[d] = (CFG.backgrounds[d] || []).map(l => Object.assign({}, l, { img: load(l.src) }));

  const prep = p => Object.assign({}, p, { img: load(p.src) });
  const chars = {};
  for (const name in CFG.characters || {}) {
    const c = CFG.characters[name];
    chars[name] = { parts: {}, weapons: {} };
    for (const k in c.parts || {}) chars[name].parts[k] = prep(c.parts[k]);
    for (const k in c.weapons || {}) chars[name].weapons[k] = prep(c.weapons[k]);
  }

  K.assets = {
    // Bu semtin resimli katmanları hazır mı?
    bgLayers(district) {
      const L = bg[district];
      return L && L.length && L.every(l => ok(l.img)) ? L : null;
    },
    // Karakterin parça seti (bütün parçalar yüklendiyse)
    charParts(name) {
      const c = chars[name];
      if (!c) return null;
      const all = Object.values(c.parts);
      return all.length && all.every(p => ok(p.img)) ? c : null;
    },
    // Bir parçayı iki eklem noktası arasına yerleştirip çizer
    drawPart(ctx, p, ax, ay, bx, by, stretch = true) {
      if (!p || !ok(p.img)) return false;
      const [pax, pay] = p.a, [pbx, pby] = p.b;
      const lenImg = Math.hypot(pbx - pax, pby - pay) || 1;
      const lenRig = Math.hypot(bx - ax, by - ay) || 1;
      const s = stretch ? lenRig / lenImg : p.scale || 1;
      const rot = Math.atan2(by - ay, bx - ax) - Math.atan2(pby - pay, pbx - pax);
      ctx.save();
      ctx.translate(ax, ay);
      ctx.rotate(rot);
      ctx.scale(s, s);
      ctx.drawImage(p.img, -pax, -pay);
      ctx.restore();
      return true;
    },
    // Test ve ayar için: bir karaktere çalışma anında parça ata
    setChar(name, parts, weapons = {}) {
      chars[name] = { parts, weapons };
    },
    hasUser(name) {
      return !!(CFG.characters && CFG.characters[name]);
    },
    setBg(district, layers) {
      bg[district] = layers;
    },
  };
})(window.K);
