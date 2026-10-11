'use strict';
// Piksel sanatı arka plan. Her şey düşük çözünürlükte (sanat pikseli) üretilir, PX katıyla büyütülür.
(function (K) {
  const PX = 3;
  const AW = Math.ceil(K.W / PX); // 427
  const AH = Math.ceil(K.H / PX); // 240
  const GY = Math.round(K.GROUND / PX); // 203: sokak zemininin üst çizgisi
  K.PX = PX;
  const FAR_DY = -80, ROOF_DY = -58;

  // ---------- Yardımcılar ----------
  const BAYER = [0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5].map(v => (v + 0.5) / 16);
  const bayer = (x, y) => BAYER[(y & 3) * 4 + (x & 3)];
  const hash = (x, y) => {
    const s = Math.sin(x * 12.9898 + y * 78.233) * 43758.5453;
    return s - Math.floor(s);
  };
  const rgb = h => {
    const n = parseInt(h.slice(1), 16);
    return [(n >> 16) & 255, (n >> 8) & 255, n & 255];
  };
  const hex = ([r, g, b]) => '#' + ((1 << 24) | (r << 16) | (g << 8) | b).toString(16).slice(1);
  const mix = (a, b, t) => {
    const A = rgb(a), B = rgb(b);
    return hex(A.map((v, i) => Math.round(v + (B[i] - v) * t)));
  };
  const lt = (c, t) => mix(c, '#fff4dc', t);
  const dk = (c, t) => mix(c, '#100810', t);

  function mk(w, h) {
    const c = document.createElement('canvas');
    c.width = Math.max(1, Math.ceil(w));
    c.height = Math.max(1, Math.ceil(h));
    const g = c.getContext('2d');
    return { c, g, w: c.width, h: c.height };
  }
  function R(g, x, y, w, h, col) {
    x = Math.round(x);
    y = Math.round(y);
    w = Math.round(w);
    h = Math.round(h);
    if (w <= 0 || h <= 0) return;
    g.fillStyle = col;
    g.fillRect(x, y, w, h);
  }
  function P(g, x, y, col) {
    g.fillStyle = col;
    g.fillRect(Math.round(x), Math.round(y), 1, 1);
  }
  // Satır satır, aynı renk dizilerini tek dikdörtgende çizer. fn(x,y) renk ya da null döndürür.
  function paint(g, x0, y0, w, h, fn) {
    for (let y = y0; y < y0 + h; y++) {
      let run = null, rs = x0;
      for (let x = x0; x <= x0 + w; x++) {
        const c = x < x0 + w ? fn(x, y) : null;
        if (c !== run) {
          if (run) R(g, rs, y, x - rs, 1, run);
          run = c;
          rs = x;
        }
      }
    }
  }
  // Işığı sağ üstten alan, kenarları gürültülü bir küme (ağaç tacı, bitki, saman)
  function blob(g, cx, cy, rx, ry, cols, seed = 0) {
    paint(g, Math.floor(cx - rx - 2), Math.floor(cy - ry - 2), Math.ceil(rx * 2 + 4), Math.ceil(ry * 2 + 4), (x, y) => {
      const dx = (x - cx) / rx, dy = (y - cy) / ry;
      const n = hash(x + seed, y) * 0.35;
      if (dx * dx + dy * dy > 1 + n - 0.15) return null;
      const l = dx * 0.6 - dy * 0.8 + (hash(y, x + seed) - 0.5) * 0.5;
      if (l > 0.45 + bayer(x, y) * 0.3) return cols[2];
      if (l < -0.35 - bayer(x, y) * 0.3) return cols[0];
      return cols[1];
    });
  }
  // Yarım küre kubbe; sağ tarafı aydınlık
  function dome(g, cx, by, r, col, lit, dark, rim) {
    for (let dy = 0; dy <= r; dy++) {
      const hw = Math.round(Math.sqrt(r * r - dy * dy));
      const y = by - dy;
      for (let x = cx - hw; x <= cx + hw; x++) {
        const k = (x - cx) / (hw + 0.01);
        let c = col;
        if (k > 0.35 + bayer(x, y) * 0.3) c = lit;
        else if (k < -0.45 - bayer(x, y) * 0.3) c = dark;
        P(g, x, y, c);
      }
      if (rim) P(g, cx + hw, y, rim);
    }
    if (rim) R(g, cx - 1, by - r, 3, 1, rim);
  }
  // Görüntü verisinde renkleri bir sis rengine doğru karıştırır
  const emissive = (r, g, b) => r >= 195 && r - b > 90;
  function haze(L, col, amt, fromY = 0, keepLit = false) {
    const id = L.g.getImageData(0, 0, L.w, L.h);
    const d = id.data;
    const H = rgb(col);
    for (let y = fromY; y < L.h; y++) {
      for (let x = 0; x < L.w; x++) {
        const i = (y * L.w + x) * 4;
        if (!d[i + 3]) continue;
        if (keepLit && emissive(d[i], d[i + 1], d[i + 2])) continue;
        const a = typeof amt === 'function' ? amt(y) : amt;
        d[i] += (H[0] - d[i]) * a;
        d[i + 1] += (H[1] - d[i + 1]) * a;
        d[i + 2] += (H[2] - d[i + 2]) * a;
      }
    }
    L.g.putImageData(id, 0, 0);
  }

  // Dünya x'ine göre semt
  function district(worldX) {
    if (worldX < 4700) return 'mahalle';
    if (worldX < 9500) return 'carsi';
    if (worldX < 14300) return 'cami';
    if (worldX < K.GATE_X - 1150) return 'bostan';
    return 'sur';
  }
  K.district = district;

  // ---------- Renkler ----------
  const SKY = ['#06060e', '#0c0b19', '#141125', '#1f1530', '#2c1936', '#3c1c39', '#521f3a', '#6c2338', '#882a35', '#a53632', '#c04d33', '#d8703c', '#e99a55', '#f6c886'];
  const WOOD = ['#9a6a40', '#7e3a2c', '#b89a70', '#5e3e2c', '#6a6e80', '#a8624c', '#8a5a3a'];
  const STONE = { base: '#6e6068', light: '#867880', dark: '#564a54', mortar: '#463a44' };
  const ROOF = ['#8e362a', '#a8453a', '#7a2e26'];

  // ---------- Gökyüzü ----------
  function genSky(top, bottom, sunY, glow, stars, seed, moon) {
    const L = mk(AW, AH);
    const id = L.g.createImageData(AW, AH);
    const d = id.data;
    const PAL = SKY.map(rgb);
    const sx = 330, horizon = 112;
    for (let y = 0; y < AH; y++) {
      for (let x = 0; x < AW; x++) {
        const t = Math.min(1, y / horizon);
        let f = top + (bottom - top) * Math.pow(t, 1.35);
        const dist = Math.hypot((x - sx) * 0.8, y - sunY);
        f += Math.max(0, glow * (1 - dist / 120)) + Math.max(0, glow * 0.6 * (1 - dist / 40));
        let c;
        if (dist < 11 && sunY < horizon + 6) c = dist < 7 ? [255, 226, 190] : [246, 168, 118];
        else {
          f = K.clamp(f, 0, PAL.length - 1);
          const i = Math.floor(f);
          c = PAL[Math.min(PAL.length - 1, i + ((f - i) > bayer(x, y) ? 1 : 0))];
        }
        const k = (y * AW + x) * 4;
        d[k] = c[0];
        d[k + 1] = c[1];
        d[k + 2] = c[2];
        d[k + 3] = 255;
      }
    }
    L.g.putImageData(id, 0, 0);
    const r = K.seeded(seed);
    if (moon) {
      drawMoon(L.g, moon.x, moon.y, moon.r, moon.a);
      cloudBank(L.g, moon.x + 30, moon.y + 14, 96, 15, moon, r);
      cloudBank(L.g, moon.x - 6, moon.y - 24, 46, 7, moon, r);
    }
    for (let i = 0; i < stars; i++) {
      const x = Math.floor(r() * AW), y = Math.floor(r() * 90);
      P(L.g, x, y, r() < 0.3 ? '#e8e2ff' : '#9c94c4');
    }
    return L;
  }

  // Ayın önünden geçen koyu bulut kümesi; aya yakın üst kenarları gümüş ışık alır
  function cloudBank(g, cx, cy, w, h, moon, r) {
    const parts = [];
    const n = 5 + Math.floor(r() * 4);
    for (let i = 0; i < n; i++) parts.push({ x: cx + (r() - 0.5) * w, y: cy + (r() - 0.5) * h * 0.6, rx: w * (0.15 + r() * 0.2), ry: h * (0.4 + r() * 0.4) });
    const inside = (x, y) => parts.some(q => ((x - q.x) / q.rx) ** 2 + ((y - q.y) / q.ry) ** 2 <= 1);
    paint(g, Math.floor(cx - w), Math.floor(cy - h * 1.5), Math.ceil(w * 2), Math.ceil(h * 3), (x, y) => {
      if (!inside(x, y)) return null;
      const dm = Math.hypot(x - moon.x, y - moon.y);
      const near = dm < moon.r * 3.6;
      if (!inside(x, y - 1)) return near ? '#c8c2dc' : '#4a2e44';
      if (!inside(x, y - 2)) return near ? '#7a7494' : '#2e1e30';
      if (!inside(x, y + 1) && !near) return '#5a1e2a';
      const v = (y - cy) / h;
      return v < -0.1 && bayer(x, y) > 0.5 ? '#1a1424' : '#0f0b17';
    });
  }

  // Batan iri ay: solgun, kraterli, hafif hale
  function drawMoon(g, cx, cy, rad, alpha) {
    paint(g, cx - rad * 3, cy - rad * 3, rad * 6, rad * 6, (x, y) => {
      const d = Math.hypot(x - cx, y - cy);
      if (d <= rad) {
        const l = ((x - cx) * -0.6 + (cy - y) * 0.5) / rad;
        const crater = hash(Math.floor(x / 3), Math.floor(y / 3)) < 0.18 && d < rad - 2;
        if (crater) return '#a8a0b8';
        if (l > 0.35 + bayer(x, y) * 0.3) return '#f0ecf4';
        if (l < -0.45 - bayer(x, y) * 0.3) return '#9c94ac';
        return '#d6d0e0';
      }
      const h = (1 - (d - rad) / (rad * 2)) * alpha;
      return h > bayer(x, y) + 0.25 ? '#3a3050' : null;
    });
  }

  function genCloud(r) {
    const w = 50 + Math.floor(r() * 90), lines = 3 + Math.floor(r() * 3);
    const L = mk(w + 20, lines * 3 + 4);
    let x0 = 10, len = w * 0.4;
    for (let i = 0; i < lines; i++) {
      const y = 2 + i * 3;
      const last = i === lines - 1;
      const lx = Math.round(x0 + (r() - 0.5) * 16), ll = Math.round(len);
      R(L.g, lx, y, ll, last ? 1 : 2, last ? '#a8343a' : i === 0 ? '#3a2242' : '#2a1832');
      if (!last) R(L.g, lx + 2, y + 2, ll - 4, 1, '#6a2838');
      x0 = lx - 6 + r() * 10;
      len = Math.min(w, len + 10 + r() * 30);
    }
    return L;
  }

  // ---------- Uzak silüet: camiler, Galata Kulesi, servi ----------
  function silMosque(g, cx, by, s, base, rim) {
    const bw = Math.round(64 * s), bh = Math.round(20 * s);
    R(g, cx - bw / 2, by - bh, bw, bh + 20, base);
    const dr = Math.round(18 * s);
    R(g, cx - dr - 2, by - bh - 4, dr * 2 + 5, 5, base);
    dome(g, cx, by - bh - 4, dr, base, base, base, rim);
    R(g, cx, by - bh - 4 - dr - 5, 1, 5, rim);
    for (const sgn of [-1, 1]) {
      dome(g, cx + sgn * Math.round(17 * s), by - bh, Math.round(10 * s), base, base, base, rim);
      dome(g, cx + sgn * Math.round(28 * s), by - bh, Math.round(5 * s), base, base, base, rim);
    }
    const mins = s > 1 ? [-1, -0.8, 0.8, 1] : [-1, 1];
    for (const m of mins) {
      const mx = Math.round(cx + m * (bw / 2 + 6 * s));
      const mh = Math.round((Math.abs(m) === 1 ? 74 : 60) * s);
      R(g, mx - 1, by - mh, 3, mh + 20, base);
      R(g, mx - 2, by - Math.round(mh * 0.72), 5, 1, base);
      if (s > 0.9) R(g, mx - 2, by - Math.round(mh * 0.86), 5, 1, base);
      for (let i = 0; i < 9; i++) R(g, mx - (i < 3 ? 1 : 0), by - mh - i, i < 3 ? 3 : 1, 1, base);
      R(g, mx + 1, by - mh, 1, Math.round(mh * 0.3), rim);
    }
  }
  function galata(g, x, by, base, rim) {
    R(g, x - 6, by - 64, 13, 70, base);
    R(g, x - 7, by - 50, 15, 2, base);
    for (let i = 0; i < 16; i++) R(g, x - 7 + Math.floor(i / 2.2), by - 64 - i, 15 - Math.floor(i / 1.1), 1, base);
    R(g, x + 6, by - 64, 1, 64, rim);
    R(g, x, by - 82, 1, 3, rim);
  }
  function cypress(g, x, by, h, w, cols) {
    for (let i = 0; i < h; i++) {
      const t = i / h;
      const hw = Math.max(0.5, w * Math.sin(Math.PI * Math.min(1, (1 - t) * 1.15)) * (t < 0.15 ? t / 0.15 * 0.6 + 0.4 : 1));
      const half = Math.round(hw);
      for (let x = x - half; x <= x + half; x++) {
        const k = (x - (x - half)) / (half * 2 + 0.01);
        P(g, x, by - i, k > 0.7 + bayer(x, by - i) * 0.25 ? cols[2] : k < 0.25 ? cols[0] : cols[1]);
      }
    }
  }
  function cypressAt(g, x, by, h, w, cols) {
    for (let i = 0; i < h; i++) {
      const t = i / h;
      const hw = w * Math.sin(Math.PI * Math.min(1, (1 - t) * 1.1 + 0.02)) * (t < 0.12 ? 0.55 + t * 3.7 : 1);
      const half = Math.max(0, Math.round(hw));
      for (let xx = x - half; xx <= x + half; xx++) {
        const k = half ? (xx - x) / half : 0;
        const y = by - i;
        P(g, xx, y, k > 0.35 + bayer(xx, y) * 0.4 ? cols[2] : k < -0.45 ? cols[0] : cols[1]);
      }
    }
  }

  function genFar(w) {
    const L = mk(w, AH), g = L.g, r = K.seeded(7);
    const base = '#170f1f', rim = '#7a2434';
    let y = 186;
    for (let x = 0; x < w; x++) {
      y = K.clamp(y + (r() - 0.5) * 1.3, 181, 192);
      R(g, x, Math.round(y), 1, AH, base);
      if (r() < 0.04) P(g, x, Math.round(y) - 1, base);
    }
    let x = 20, gal = false;
    while (x < w - 30) {
      if (!gal && x > w * 0.25) {
        galata(g, x, 188, base, rim);
        gal = true;
        x += 40;
        continue;
      }
      const k = r();
      if (k < 0.5) {
        const s = 0.65 + r() * 0.65;
        silMosque(g, x + 40 * s, 189, s, base, rim);
        x += 90 * s + 20 + r() * 110;
      } else {
        const n = 1 + Math.floor(r() * 4);
        for (let i = 0; i < n; i++) cypressAt(g, x + i * 5, 190, 14 + Math.floor(r() * 12), 2, [base, base, rim]);
        x += n * 5 + 15 + r() * 40;
      }
    }
    haze(L, '#3a1a34', y => K.clamp((y - 150) / 60, 0, 0.5), 150);
    return L;
  }

  // ---------- Şehir çatıları ----------
  function genRoofs(w, smoke) {
    const L = mk(w, AH), g = L.g, r = K.seeded(11);
    const rows = [{ by: 191, hz: 0.5 }, { by: 199, hz: 0.25 }];
    for (const row of rows) {
      let x = -10;
      while (x < w) {
        const k = r();
        if (k < 0.07) {
          // hamam / küçük cami kubbesi
          const rr = 6 + Math.floor(r() * 6);
          R(g, x, row.by - rr - 4, rr * 2 + 4, rr + 30, '#2a2034');
          dome(g, x + rr + 2, row.by - rr - 4, rr, '#34324a', '#4c4864', '#221e30', '#8a3a44');
          for (let i = 0; i < 3; i++) P(g, x + rr - 2 + i * 2, row.by - rr - 4 - Math.floor(rr * 0.6), '#f0c070');
          x += rr * 2 + 6;
        } else if (k < 0.12) {
          cypressAt(g, x + 3, row.by + 2, 22 + Math.floor(r() * 16), 3, ['#0c0e14', '#14181a', '#3a2028']);
          x += 8;
        } else if (k < 0.14) {
          // yakın minare
          const mh = 70 + Math.floor(r() * 30);
          R(g, x, row.by - mh, 4, mh + 10, '#2e2638');
          R(g, x + 3, row.by - mh, 1, Math.round(mh * 0.3), '#8a3040');
          R(g, x - 1, row.by - Math.round(mh * 0.7), 6, 2, '#241e2e');
          for (let i = 0; i < 12; i++) R(g, x + (i < 3 ? 0 : Math.floor(i / 6)), row.by - mh - i, Math.max(1, 4 - Math.floor(i / 3)), 1, '#262838');
          x += 10;
        } else {
          const bw = 9 + Math.floor(r() * 16), wh = 7 + Math.floor(r() * 12), rh = 3 + Math.floor(r() * 3);
          const wall = ['#2e2236', '#34263a', '#2a2032', '#3a2a3a'][Math.floor(r() * 4)];
          const roof = ['#4a1e2a', '#56222c', '#3e1a26'][Math.floor(r() * 3)];
          const top = row.by - wh;
          R(g, x, top, bw, wh + 20, wall);
          R(g, x + bw - 1, top, 1, wh, lt(wall, 0.18));
          for (let i = 0; i < rh; i++) R(g, x - 1 + i, top - i - 1, bw + 2 - i * 2, 1, i === rh - 1 ? lt(roof, 0.15) : roof);
          for (let wy = top + 2; wy < row.by - 2; wy += 4) {
            for (let wx = x + 2; wx < x + bw - 2; wx += 3 + Math.floor(r() * 2)) {
              if (r() < 0.22) P(g, wx, wy, r() < 0.6 ? '#f2b45e' : '#ffd890');
              else if (r() < 0.5) P(g, wx, wy, dk(wall, 0.35));
            }
          }
          if (r() < 0.25) {
            const cx = x + 2 + Math.floor(r() * (bw - 4));
            R(g, cx, top - rh - 3, 2, 4, '#5a4250');
            if (row.hz < 0.4) smoke.push({ x: cx, y: top - rh - 4, s: r() * 10 });
          }
          x += bw - 1 + (r() < 0.2 ? 2 + Math.floor(r() * 4) : 0);
        }
      }
      haze(L, '#2a1630', row.hz * 0.7, 0, true);
    }
    R(g, 0, 198, w, AH - 198, '#1e1528');
    return L;
  }

  // ---------- Bina parçaları ----------
  function stoneWall(g, x, y, w, h, pal = STONE, course = 5, seed = 0) {
    R(g, x, y, w, h, pal.mortar);
    let row = 0;
    for (let yy = y; yy < y + h; yy += course, row++) {
      const ch = Math.min(course, y + h - yy);
      let xx = x - Math.floor(hash(row + seed, x) * 9);
      while (xx < x + w) {
        const sw = 6 + Math.floor(hash(xx, yy + seed) * 7);
        const a = Math.max(x, xx), b = Math.min(x + w, xx + sw - 1);
        const v = hash(xx + 3, yy + seed * 7);
        const c = v < 0.25 ? pal.dark : v > 0.8 ? pal.light : pal.base;
        R(g, a, yy, b - a, ch - 1, c);
        if (b - a > 1) R(g, a, yy, b - a, 1, lt(c, 0.12));
        xx += sw;
      }
    }
  }
  function window_(g, x, y, w, h, kind, frame = '#c8b490') {
    R(g, x - 1, y - 1, w + 2, h + 2, frame);
    if (kind === 'lit') {
      R(g, x, y, w, h, '#ffcf6e');
      R(g, x, y + h - 2, w, 2, '#ee9a4c');
      R(g, x + Math.floor(w / 2), y, 1, h, '#c8783e');
    } else {
      R(g, x, y, w, h, '#24161c');
      R(g, x + w - 1, y, 1, h, '#3a2630');
    }
    if (kind === 'kafes') {
      for (let j = 0; j < h; j++) for (let i = 0; i < w; i++) if ((i + j) % 2 === 0) P(g, x + i, y + j, '#5a3a28');
    }
    R(g, x - 1, y + h + 1, w + 2, 1, dk(frame, 0.3));
  }
  function pickWin(r) {
    const v = r();
    return v < 0.3 ? 'lit' : v < 0.65 ? 'kafes' : 'dark';
  }
  function roof(g, x, w, eaveY, h, pal) {
    for (let i = 0; i < h; i++) {
      const y = eaveY - i;
      const inset = Math.round(i * 1.6);
      for (let xx = x - 4 + inset; xx < x + w + 4 - inset; xx++) {
        let c = i % 2 ? pal[0] : pal[1];
        if ((xx + (i % 2) * 2) % 4 === 0) c = dk(c, 0.18);
        if (xx > x + w + 4 - inset - 3) c = lt(c, 0.15);
        P(g, xx, y, c);
      }
    }
    R(g, x - 4, eaveY + 1, w + 8, 1, '#3a1c1a');
    R(g, x - 3, eaveY + 2, w + 6, 1, 'rgba(20,8,12,0.55)');
  }
  // Ahşap Osmanlı evi. opts: base (alt çizgi), top (çatı saçağı), gf (zemin kat üstü)
  function house(g, x, w, opts, r) {
    const base = opts.base, top = opts.top, gf = opts.gf;
    const col = opts.col || WOOD[Math.floor(r() * WOOD.length)];
    const pal = ROOF.slice(Math.floor(r() * 2));
    // zemin kat: taş
    stoneWall(g, x + 2, gf, w - 4, base - gf, STONE, 5, Math.floor(r() * 99));
    // kapı
    const dx = x + 4 + Math.floor(r() * (w - 20));
    R(g, dx - 1, base - 23, 13, 23, STONE.light);
    R(g, dx, base - 21, 11, 21, '#4a2c20');
    for (let i = 2; i < 11; i += 3) R(g, dx + i, base - 20, 1, 20, '#3a2018');
    R(g, dx, base - 22, 11, 1, '#4a2c20');
    P(g, dx + 8, base - 11, '#d8b060');
    if (w > 40) {
      const wx = dx > x + w / 2 ? x + 6 : x + w - 14;
      window_(g, wx, gf + 6, 7, 9, 'dark', STONE.light);
      for (let i = 1; i < 7; i += 2) R(g, wx + i, gf + 6, 1, 9, '#2a2026');
    }
    // üst katlar: çıkma
    const ux = x - 1, uw = w + 2;
    R(g, ux, top, uw, gf - top, col);
    for (let y = top + 2; y < gf; y += 3) R(g, ux, y, uw, 1, dk(col, 0.12));
    R(g, ux + uw - 1, top, 1, gf - top, lt(col, 0.25));
    R(g, ux, top, 1, gf - top, dk(col, 0.3));
    R(g, ux - 1, gf - 2, uw + 2, 2, '#3a2420');
    for (let bx = ux + 3; bx < ux + uw - 2; bx += 9) {
      P(g, bx, gf, '#3a2420');
      P(g, bx + 1, gf + 1, '#3a2420');
      P(g, bx + 2, gf + 2, '#3a2420');
    }
    const floors = Math.max(1, Math.round((gf - top) / 22));
    const fh = (gf - top) / floors;
    for (let f = 0; f < floors; f++) {
      const fy = Math.round(top + f * fh);
      if (f > 0) R(g, ux, fy, uw, 1, dk(col, 0.4));
      const n = Math.max(1, Math.floor((uw - 6) / 11));
      const sp = (uw - 6) / n;
      for (let i = 0; i < n; i++) window_(g, Math.round(ux + 4 + i * sp + (sp - 7) / 2), fy + 5, 6, Math.min(10, fh - 9), pickWin(r));
    }
    // cumba
    if (w > 50 && r() < 0.6) {
      const cw = 22, cx = Math.round(x + w / 2 - cw / 2), cy = top + 3;
      R(g, cx - 2, cy, cw + 4, gf - cy - 3, lt(col, 0.08));
      R(g, cx + cw + 1, cy, 1, gf - cy - 3, lt(col, 0.3));
      R(g, cx - 3, cy - 2, cw + 6, 2, '#3a2420');
      for (let i = 0; i < 3; i++) window_(g, cx + 1 + i * 7, cy + 4, 5, Math.min(12, gf - cy - 12), pickWin(r));
      P(g, cx, gf - 2, '#3a2420');
      P(g, cx + 1, gf - 1, '#3a2420');
      P(g, cx + cw, gf - 2, '#3a2420');
    }
    roof(g, ux, uw, top - 1, opts.roofH || 9, pal);
    if (r() < 0.6) {
      const cx = ux + 6 + Math.floor(r() * (uw - 12));
      R(g, cx, top - 12, 3, 8, '#6a5a60');
      R(g, cx - 1, top - 13, 5, 1, '#4a3e44');
      if (opts.smoke) opts.smoke.push({ x: cx + 1, y: top - 14, s: r() * 10 });
    }
  }
  function tree(g, x, by, h, r, cols = ['#24361f', '#3a5a32', '#62874a']) {
    R(g, x - 1, by - h * 0.45, 3, h * 0.45, '#5a4a40');
    P(g, x + 1, by - h * 0.3, '#7a6a5e');
    const cr = h * 0.33;
    blob(g, x - cr * 0.5, by - h * 0.62, cr * 0.8, cr * 0.65, cols, x);
    blob(g, x + cr * 0.6, by - h * 0.6, cr * 0.75, cr * 0.6, cols, x + 5);
    blob(g, x, by - h * 0.78, cr, cr * 0.7, cols, x + 9);
  }

  // ---------- Orta katman: konaklar, büyük cami ----------
  function bigMosque(g, cx, by) {
    const st = '#8a7e8c', sd = '#6a5e70', sl = '#a498a6', lead = '#5c6074', leadL = '#8c90a8', leadD = '#3c4054', rim = '#d0605a';
    R(g, cx - 70, by - 50, 140, 60, st);
    R(g, cx + 68, by - 50, 2, 50, sl);
    for (let i = 0; i < 6; i++) {
      const wx = cx - 60 + i * 22;
      R(g, wx, by - 40, 6, 12, '#3a2e3e');
      dome(g, wx + 3, by - 40, 3, '#3a2e3e', '#3a2e3e', '#3a2e3e');
      R(g, wx, by - 20, 6, 10, '#3a2e3e');
    }
    R(g, cx - 34, by - 62, 68, 12, sd);
    for (let i = 0; i < 8; i++) R(g, cx - 30 + i * 8, by - 60, 3, 5, '#3a2e3e');
    dome(g, cx, by - 62, 32, lead, leadL, leadD, rim);
    R(g, cx, by - 104, 1, 10, '#d8b060');
    P(g, cx + 1, by - 105, '#d8b060');
    for (const s of [-1, 1]) {
      dome(g, cx + s * 40, by - 50, 18, lead, leadL, leadD, rim);
      dome(g, cx + s * 62, by - 50, 8, lead, leadL, leadD, rim);
      const mx = cx + s * 84;
      R(g, mx - 3, by - 150, 7, 160, st);
      R(g, mx + 3, by - 150, 1, 150, sl);
      for (const k of [0.45, 0.68]) {
        R(g, mx - 5, by - Math.round(150 * k), 11, 2, sd);
        R(g, mx - 5, by - Math.round(150 * k) + 2, 11, 1, '#5a4e5e');
      }
      for (let i = 0; i < 18; i++) R(g, mx - 3 + Math.floor(i / 6), by - 150 - i, Math.max(1, 7 - Math.floor(i / 3)), 1, i % 4 === 0 ? leadL : lead);
      R(g, mx, by - 172, 1, 4, '#d8b060');
    }
  }

  function genMid(w, smoke) {
    const L = mk(w, AH), g = L.g, r = K.seeded(23);
    const toWorld = mx => (mx * PX) / 0.55;
    let mosqueDone = false;
    let x = -20;
    while (x < w) {
      const d = district(toWorld(x));
      if (d === 'cami' && !mosqueDone && toWorld(x) > 11000) {
        bigMosque(g, x + 90, 196);
        mosqueDone = true;
        x += 190;
        continue;
      }
      const k = r();
      if (d === 'bostan' || d === 'sur') {
        if (k < 0.5) {
          tree(g, x + 12, 200, 46 + r() * 24, r);
          x += 22 + r() * 16;
        } else if (k < 0.7) {
          cypressAt(g, x + 4, 200, 50 + Math.floor(r() * 25), 4, ['#1a2622', '#26382c', '#4e6040']);
          x += 12;
        } else {
          const bw = 34 + Math.floor(r() * 20);
          house(g, x, bw, { base: 200, top: 150 - Math.floor(r() * 12), gf: 178, smoke }, r);
          x += bw + 8;
        }
        continue;
      }
      if (k < 0.12) {
        tree(g, x + 14, 200, 60 + r() * 30, r);
        x += 26;
      } else if (k < 0.18) {
        cypressAt(g, x + 4, 200, 60 + Math.floor(r() * 30), 4, ['#1a2622', '#26382c', '#4e6040']);
        x += 10;
      } else {
        const bw = 40 + Math.floor(r() * 34);
        const top = 126 + Math.floor(r() * 26);
        house(g, x, bw, { base: 200, top, gf: 178, smoke, roofH: 7 + Math.floor(r() * 4) }, r);
        x += bw + (r() < 0.25 ? 4 + Math.floor(r() * 8) : 1);
      }
    }
    haze(L, '#160e20', 0.6, 0, true);
    return L;
  }

  // ---------- Yakın katman: sokak ----------
  const near = {};
  near.lamp = function (g, x, S) {
    R(g, x, GY - 54, 2, 54, '#2a2026');
    R(g, x - 1, GY - 2, 4, 2, '#2a2026');
    R(g, x, GY - 54, 9, 1, '#2a2026');
    R(g, x + 6, GY - 53, 1, 2, '#2a2026');
    R(g, x + 4, GY - 51, 5, 7, '#3a2a22');
    R(g, x + 5, GY - 50, 3, 5, '#ffd27a');
    S.lamps.push({ x: x + 6, y: GY - 48 });
  };
  near.gardenWall = function (g, x, w, r, S) {
    const h = 34 + Math.floor(r() * 12);
    if (r() < 0.5) tree(g, x + w / 2, GY - h + 6, 60 + r() * 20, r, ['#1e2e1c', '#33502c', '#5a7a40']);
    stoneWall(g, x, GY - h, w, h, STONE, 5, x);
    R(g, x - 1, GY - h - 2, w + 2, 2, '#8c7e84');
    R(g, x - 1, GY - h, w + 2, 1, '#4a3e48');
    for (let i = 0; i < w; i += 6 + Math.floor(r() * 8)) {
      if (r() < 0.6) blob(g, x + i, GY - h - 3, 3 + r() * 4, 2 + r() * 3, ['#24361f', '#3a5a32', '#62874a'], i);
      if (r() < 0.3) P(g, x + i + 1, GY - h - 5, r() < 0.5 ? '#e0708a' : '#f0d070');
    }
    if (r() < 0.35) {
      // duvarda kedi
      const cx = x + 6 + Math.floor(r() * (w - 12)), cy = GY - h - 2;
      R(g, cx, cy - 4, 4, 4, '#2a2024');
      P(g, cx, cy - 5, '#2a2024');
      P(g, cx + 3, cy - 5, '#2a2024');
      R(g, cx + 4, cy - 1, 3, 1, '#2a2024');
      P(g, cx + 1, cy - 3, '#e0c060');
    }
  };
  near.cesme = function (g, x) {
    const m = '#b8b0b4', ml = '#d6cfd0', md = '#8c8488';
    R(g, x, GY - 46, 36, 46, m);
    R(g, x + 35, GY - 46, 1, 46, ml);
    R(g, x - 2, GY - 50, 40, 4, ml);
    R(g, x - 2, GY - 46, 40, 1, md);
    for (let i = 0; i < 9; i++) R(g, x + 2 + i, GY - 54 + Math.floor(i / 3), 32 - i * 2, 1, i % 2 ? '#8e362a' : '#a8453a');
    R(g, x + 9, GY - 34, 18, 26, md);
    for (let i = 0; i < 6; i++) R(g, x + 9 + i, GY - 40 + i, 18 - i * 2, 1, md);
    R(g, x + 11, GY - 30, 14, 22, '#5e565a');
    R(g, x + 8, GY - 44, 20, 5, '#4a3a2c');
    for (let i = 0; i < 18; i += 3) R(g, x + 9 + i, GY - 42, 2, 1, '#c9a24a');
    R(g, x + 17, GY - 20, 2, 3, '#c9a24a');
    R(g, x + 3, GY - 7, 30, 7, md);
    R(g, x + 4, GY - 6, 28, 2, '#7aa0b8');
    R(g, x + 4, GY - 6, 10, 1, '#bfe0f0');
  };
  near.alley = function (g, x, w, r) {
    R(g, x, GY - 90, w, 90, '#24181e');
    R(g, x + 2, GY - 70, w - 4, 70, '#1c1218');
    for (let i = 0; i < 4; i++) R(g, x + 1, GY - 8 + i * 2, w - 2, 1, '#3a2c30');
    if (r() < 0.6) {
      R(g, x - 2, GY - 92, w + 4, 6, '#5e4a52');
      for (let i = 0; i < w / 2; i++) R(g, x + i, GY - 86 + Math.round(Math.sqrt(i) * 0.8), w - i * 2, 1, '#5e4a52');
    }
    // çamaşır ipi
    if (r() < 0.6) {
      for (let i = 0; i < w + 8; i++) P(g, x - 4 + i, GY - 104 + Math.round(Math.sin((i / (w + 8)) * Math.PI) * 4), '#2a2024');
      const cols = ['#d8d0c0', '#b04a3a', '#4a6a9a', '#e0c070'];
      for (let i = 0; i < 3; i++) R(g, x + i * 6, GY - 102 + Math.round(Math.sin(((i * 6 + 4) / (w + 8)) * Math.PI) * 4), 4, 6, cols[Math.floor(r() * 4)]);
    }
  };
  near.houseFront = function (g, x, w, r, S) {
    house(g, x, w, { base: GY, top: 104 + Math.floor(r() * 22), gf: 152 }, r);
  };
  near.shop = function (g, x, w, r, S) {
    const top = 98 + Math.floor(r() * 14), gf = 136;
    house(g, x, w, { base: gf, top, gf: gf - 1 }, r);
    // dükkân cephesi
    R(g, x, gf - 2, w, GY - gf + 2, '#2a1a1c');
    stoneWall(g, x, gf, 4, GY - gf, STONE, 4, x);
    stoneWall(g, x + w - 4, gf, 4, GY - gf, STONE, 4, x + 1);
    R(g, x, gf - 3, w, 3, '#4a2c20');
    for (const sy of [gf + 16, gf + 34, gf + 52]) {
      R(g, x + 4, sy, w - 8, 1, '#4a3020');
      for (let i = x + 6; i < x + w - 6; i += 3) if (r() < 0.7) R(g, i, sy - 3, 2, 3, ['#c9783a', '#6a8a9a', '#b04a3a', '#d8c090', '#4a6a9a', '#8a5a3a'][Math.floor(r() * 6)]);
    }
    // tente
    const pair = [['#b0302a', '#e8dcc0'], ['#2f6a4a', '#e8dcc0'], ['#2a4a7a', '#e0c890'], ['#a8602a', '#f0e0b0']][Math.floor(r() * 4)];
    for (let i = 0; i < w + 4; i++) {
      const c = Math.floor(i / 4) % 2 ? pair[1] : pair[0];
      R(g, x - 2 + i, gf - 2, 1, 8, c);
      R(g, x - 2 + i, gf - 2, 1, 2, dk(c, 0.25));
      if (i % 4 !== 3) P(g, x - 2 + i, gf + 6, c);
    }
    R(g, x - 2, gf + 7, w + 4, 1, 'rgba(10,4,8,0.5)');
    // asılı bakır kaplar
    for (let i = x + 6; i < x + w - 6; i += 7) {
      if (r() < 0.5) {
        P(g, i + 1, gf + 7, '#3a2a22');
        R(g, i, gf + 8, 3, 3, '#c87838');
        P(g, i + 2, gf + 8, '#f0b070');
      }
    }
    // halı
    if (r() < 0.5) {
      const hx = r() < 0.5 ? x + 4 : x + w - 12;
      R(g, hx, gf + 8, 8, 30, '#8e2a2a');
      R(g, hx + 1, gf + 9, 6, 28, '#a83434');
      for (let j = 0; j < 4; j++) {
        P(g, hx + 3, gf + 13 + j * 7, '#2a4a7a');
        P(g, hx + 4, gf + 13 + j * 7, '#2a4a7a');
        R(g, hx + 2, gf + 14 + j * 7, 4, 1, '#d8b070');
      }
    }
    // önde baharat sepetleri
    const spices = ['#c8402a', '#e0a830', '#7a4a2a', '#5a7a3a', '#d86a2a'];
    for (let i = x + 6; i < x + w - 10; i += 11) {
      if (r() < 0.75) {
        const c = spices[Math.floor(r() * spices.length)];
        R(g, i, GY - 5, 9, 5, '#7a5a30');
        R(g, i, GY - 5, 9, 1, '#9a7a48');
        for (let j = 0; j < 5; j++) R(g, i + j, GY - 6 - j, 9 - j * 2, 1, j === 4 ? lt(c, 0.3) : c);
      }
    }
  };
  near.hanGate = function (g, x, r, S) {
    const w = 70;
    stoneWall(g, x, 84, w, GY - 84, { base: '#8a7a70', light: '#a2928a', dark: '#6e6058', mortar: '#5a4e48' }, 6, x);
    R(g, x - 2, 80, w + 4, 4, '#a2928a');
    const ax = x + 20, aw = 30, at = 120;
    for (let i = 0; i < aw / 2 + 4; i++) {
      for (let a = 0; a < 40; a++) {
        const ang = Math.PI + (a / 40) * Math.PI;
        const rr = aw / 2 + 2 + (i % 4);
        const px = ax + aw / 2 + Math.cos(ang) * rr, py = at + Math.sin(ang) * rr;
        if (i < 4) P(g, px, py, Math.floor(a / 5) % 2 ? '#b8a890' : '#7a4a3a');
      }
    }
    R(g, ax, at, aw, GY - at, '#1c1216');
    for (let dy = 0; dy < aw / 2; dy++) {
      const hw = Math.round(Math.sqrt((aw / 2) ** 2 - dy * dy));
      R(g, ax + aw / 2 - hw, at - dy, hw * 2, 1, '#1c1216');
    }
    R(g, ax + 10, GY - 24, 10, 24, '#3a2a26');
    R(g, ax + 12, GY - 20, 6, 20, '#c88a58');
    S.lamps.push({ x: ax + aw / 2, y: at - aw / 2 - 8 });
    R(g, ax + aw / 2 - 2, at - aw / 2 - 12, 4, 6, '#3a2a22');
    R(g, ax + aw / 2 - 1, at - aw / 2 - 11, 2, 4, '#ffd27a');
  };
  near.courtyard = function (g, x, w, r, S) {
    const top = 116;
    const pal = { base: '#8a7e84', light: '#a09498', dark: '#766a72', mortar: '#5e5258' };
    if (r() < 0.5) tree(g, x + w / 2, top + 8, 70 + r() * 20, r, ['#1e2e1c', '#33502c', '#5a7a40']);
    stoneWall(g, x, top, w, GY - top, pal, 6, x);
    for (let i = 0; i < w; i += 6) {
      R(g, x + i, top - 3, 4, 3, '#a09498');
      P(g, x + i + 1, top - 4, '#a09498');
      P(g, x + i + 2, top - 4, '#a09498');
    }
    R(g, x, top, w, 1, '#5e5258');
    for (let i = 10; i < w - 10; i += 30) {
      const wx = x + i, wy = 140;
      R(g, wx - 2, wy - 2, 16, 26, '#a89ea2');
      for (let k = 0; k < 7; k++) R(g, wx - 2 + k, wy - 3 - k, 16 - k * 2, 1, '#a89ea2');
      R(g, wx, wy, 12, 22, '#2a2028');
      for (let k = 0; k < 5; k++) R(g, wx + k + 1, wy - 1 - k, 10 - k * 2, 1, '#2a2028');
      for (let yy = wy; yy < wy + 22; yy += 2) for (let xx = wx; xx < wx + 12; xx += 2) P(g, xx + ((yy / 2) % 2), yy, '#5a5058');
    }
  };
  near.portal = function (g, x, r, S) {
    const w = 56, top = 70;
    const pal = { base: '#9a8e92', light: '#b0a4a8', dark: '#867a80', mortar: '#6e6268' };
    stoneWall(g, x, top, w, GY - top, pal, 6, x);
    R(g, x - 2, top - 4, w + 4, 4, '#b0a4a8');
    R(g, x + 6, top + 6, w - 12, GY - top - 6, '#6e6268');
    R(g, x + 8, top + 8, w - 16, GY - top - 8, '#8a7e84');
    // mukarnas
    for (let row = 0; row < 8; row++) {
      for (let i = 0; i < (w - 18) / 4 - row * 0.5; i++) {
        const px = x + 9 + row * 2 + i * 4, py = top + 10 + row * 4;
        if (px > x + w - 12 - row * 2) continue;
        R(g, px, py, 3, 3, row % 2 ? '#a09498' : '#766a72');
        P(g, px + 1, py + 2, '#4e4248');
      }
    }
    R(g, x + 14, top + 48, w - 28, 6, '#2e3a5a');
    for (let i = 0; i < w - 30; i += 3) P(g, x + 15 + i, top + 50, '#d8b060');
    R(g, x + 16, GY - 40, w - 32, 40, '#4a2c20');
    R(g, x + w / 2, GY - 40, 1, 40, '#2a1810');
    for (let yy = GY - 36; yy < GY - 2; yy += 6) for (let xx = x + 18; xx < x + w - 18; xx += 6) P(g, xx, yy, '#c9a24a');
  };
  near.bostan = function (g, x, r, S) {
    const k = r();
    if (k < 0.3) {
      // çit
      const w = 40 + Math.floor(r() * 30);
      for (let i = 0; i < w; i += 7) R(g, x + i, GY - 22, 2, 22, '#6a4a32');
      R(g, x, GY - 18, w, 2, '#7a5a3a');
      R(g, x, GY - 10, w, 2, '#7a5a3a');
      if (r() < 0.7) tree(g, x + w / 2, GY - 4, 56 + r() * 20, r, ['#1e2e1c', '#3a5a2c', '#6a8a44']);
      return w + 6;
    }
    if (k < 0.5) {
      // meyve ağacı
      tree(g, x + 16, GY, 60 + r() * 20, r, ['#20301c', '#3a5a2c', '#6a8a44']);
      for (let i = 0; i < 8; i++) P(g, x + 4 + r() * 24, GY - 40 - r() * 20, '#e0702a');
      return 34;
    }
    if (k < 0.65) {
      // saman yığını
      blob(g, x + 14, GY - 9, 14, 10, ['#9a7a34', '#c8a050', '#e8c870'], x);
      for (let i = 0; i < 6; i++) R(g, x + 4 + i * 4, GY - 14 + (i % 2) * 3, 1, 5, '#8a6a2a');
      return 32;
    }
    if (k < 0.8) {
      // kuyu
      stoneWall(g, x, GY - 14, 22, 14, STONE, 4, x);
      R(g, x + 2, GY - 34, 2, 20, '#5a3a28');
      R(g, x + 18, GY - 34, 2, 20, '#5a3a28');
      for (let i = 0; i < 6; i++) R(g, x - 2 + i, GY - 36 - i, 26 - i * 2, 1, i % 2 ? '#8e362a' : '#a8453a');
      R(g, x + 10, GY - 30, 1, 8, '#3a2a22');
      R(g, x + 8, GY - 22, 5, 4, '#6a4a32');
      return 30;
    }
    // alçak taş duvar
    const w = 30 + Math.floor(r() * 30);
    stoneWall(g, x, GY - 16, w, 16, STONE, 4, x);
    R(g, x - 1, GY - 17, w + 2, 1, '#8c7e84');
    return w + 4;
  };

  function line(g, x0, y0, x1, y1, col) {
    const n = Math.max(Math.abs(x1 - x0), Math.abs(y1 - y0), 1);
    for (let i = 0; i <= n; i++) P(g, Math.round(x0 + ((x1 - x0) * i) / n), Math.round(y0 + ((y1 - y0) * i) / n), col);
  }
  // Mangal: ayaklı demir sepet, üstünde canlı ateş
  near.brazier = function (g, x, S) {
    const iron = '#1c1418', ironL = '#4a3a34';
    line(g, x - 7, GY, x - 4, GY - 20, iron);
    line(g, x + 7, GY, x + 4, GY - 20, iron);
    line(g, x, GY, x, GY - 20, iron);
    R(g, x - 8, GY - 27, 17, 2, ironL);
    R(g, x - 7, GY - 25, 15, 5, iron);
    for (let i = -6; i <= 6; i += 2) P(g, x + i, GY - 24, ironL);
    for (let i = -6; i <= 6; i++) P(g, x + i, GY - 28, i % 2 ? '#ff8a2a' : '#d84a1a');
    S.fires.push({ x, y: GY - 28, w: 13, h: 18, s: hash(x, 1) * 10 });
  };
  near.wallTorch = function (g, x, y, S) {
    R(g, x - 2, y + 2, 5, 2, '#1c1418');
    R(g, x, y - 6, 2, 9, '#4a3020');
    R(g, x - 1, y - 8, 4, 2, '#1c1418');
    P(g, x, y - 9, '#ff8a2a');
    S.fires.push({ x: x + 1, y: y - 9, w: 6, h: 10, s: hash(x, y) * 10 });
  };
  near.campfire = function (g, x, S) {
    for (let i = -7; i <= 7; i += 3) R(g, x + i, GY - 3, 3, 3, '#4a4048');
    line(g, x - 6, GY - 2, x + 5, GY - 5, '#3a2618');
    line(g, x + 6, GY - 2, x - 5, GY - 5, '#3a2618');
    S.fires.push({ x, y: GY - 5, w: 12, h: 16, s: hash(x, 2) * 10 });
  };
  function crescent(g, cx, cy, rad, col) {
    for (let y = -rad; y <= rad; y++) {
      for (let x = -rad; x <= rad; x++) {
        if (x * x + y * y > rad * rad) continue;
        const ox = x - rad * 0.45, oy = y + rad * 0.15;
        if (ox * ox + oy * oy <= (rad * 0.82) ** 2) continue;
        P(g, cx + x, cy + y, col);
      }
    }
  }
  // Hilalli, ucu yırtık kızıl sancak (x sol kenar, y asılı olduğu çubuk)
  near.banner = function (g, x, y, h) {
    const w = 15;
    R(g, x - 3, y, w + 6, 2, '#2a1c14');
    P(g, x - 4, y, '#c9a24a');
    P(g, x + w + 3, y, '#c9a24a');
    for (let i = 0; i < w; i++) {
      const len = h - Math.floor(hash(x + i, y) * 7) - (i % 5 === 2 ? 4 : 0);
      for (let j = 0; j < len; j++) {
        let c = i % 5 === 0 ? '#4e0e14' : i % 5 === 4 ? '#8a1c22' : '#6e161c';
        if (j < 2) c = '#c9a24a';
        if (hash(i * 7 + x, j) < 0.04) c = '#3a0a10';
        P(g, x + i, y + 2 + j, c);
      }
    }
    crescent(g, x + 7, y + 4 + Math.round(h * 0.35), 5, '#d8b060');
  };

  // İstanbul surları: taş ve tuğla hatıllı; kapının ortası gx
  function cityWalls(g, x0, x1, gx, S) {
    const top = 72;
    const stone = { base: '#7a6c70', light: '#8e8084', dark: '#665a60', mortar: '#4e4248' };
    const bandRow = y => ((y - top) % 22 + 22) % 22 >= 15;
    const masonry = (x, y, w, h, seed) => {
      stoneWall(g, x, y, w, h, stone, 4, seed);
      for (let yy = y; yy < y + h; yy++) {
        if (!bandRow(yy)) continue;
        const brick = (yy % 2 ? (Math.floor((yy - x) / 1) % 1) : 0);
        R(g, x, yy, w, 1, yy % 2 ? '#7e4034' : '#8e4a3a');
        for (let xx = x + (yy % 2) * 3; xx < x + w; xx += 6) P(g, xx, yy, '#5a3a32');
        void brick;
      }
    };
    const crenel = (x, w, y) => {
      for (let i = 0; i < w; i += 9) {
        R(g, x + i, y - 6, 5, 6, '#7a6c70');
        R(g, x + i + 4, y - 6, 1, 6, '#5e5258');
        R(g, x + i, y - 6, 5, 1, '#9a8c90');
      }
    };
    const tower = (tx, tw, ty) => {
      masonry(tx, ty, tw, GY - ty, tx);
      R(g, tx, ty, 1, GY - ty, '#5a4e54');
      R(g, tx + tw - 1, ty, 1, GY - ty, '#9a8c90');
      crenel(tx, tw, ty);
      for (let yy = ty + 16; yy < GY - 40; yy += 34) {
        R(g, tx + tw / 2 - 1, yy, 2, 7, '#2a2026');
      }
      S.flags.push({ x: tx + Math.floor(tw / 2), y: ty - 6 });
    };
    // gövde
    masonry(x0, top, x1 - x0, GY - top, 3);
    crenel(x0, x1 - x0, top);
    // kuleler
    tower(x0, 44, 60);
    for (let tx = x0 + 150; tx < x1; tx += 150) {
      if (Math.abs(tx + 22 - gx) < 130) continue;
      tower(tx, 44, 60);
    }
    tower(gx - 92, 50, 52);
    tower(gx + 42, 50, 52);
    // kapı
    const aw = 56, at = 118;
    for (let dy = 0; dy < aw / 2 + 5; dy++) {
      const hw = Math.round(Math.sqrt((aw / 2 + 5) ** 2 - dy * dy));
      for (let xx = gx - hw; xx < gx + hw; xx++) P(g, xx, at - dy, Math.floor((Math.atan2(at - dy - at, xx - gx) + 4) * 4) % 2 ? '#8e4a3a' : '#9a8c90');
    }
    R(g, gx - aw / 2 - 5, at, aw + 10, GY - at, '#9a8c90');
    paint(g, gx - aw / 2, at - aw / 2, aw, GY - at + aw / 2, (x, y) => {
      if (y < at) {
        const dy = at - y, hw = Math.sqrt((aw / 2) ** 2 - dy * dy);
        if (!(Math.abs(x - gx) < hw)) return null;
      }
      const t = (y - (at - aw / 2)) / (GY - at + aw / 2);
      if (y > GY - 18) return t * 1.2 > bayer(x, y) + 0.4 ? '#c88a58' : '#d89a62';
      if (y > GY - 26 && Math.abs(x - gx) > 6 + hash(x, 3) * 10) return '#d9776a';
      return t > bayer(x, y) * 0.5 + 0.35 ? '#f6b866' : '#fde0a0';
    });
    for (const s of [-1, 1]) {
      R(g, gx + s * (aw / 2 - 3) - (s > 0 ? 3 : 0), at - 6, 6, GY - at + 6, '#4a2c20');
      for (let yy = at; yy < GY; yy += 6) P(g, gx + s * (aw / 2 - 2) - (s > 0 ? 1 : 0), yy, '#c9a24a');
    }
    near.wallTorch(g, gx - 44, 132, S);
    near.wallTorch(g, gx + 44, 132, S);
    S.gate = { x: gx, y: at, w: aw };
    for (let tx = x0 + 150; tx < x1; tx += 150) {
      if (Math.abs(tx + 22 - gx) < 130) continue;
      near.wallTorch(g, tx + 22, GY - 70, S);
      if (tx + 100 < x1 && Math.abs(tx + 100 - gx) > 120) near.banner(g, tx + 90, top + 2, 46);
    }
    near.banner(g, gx - 86, 54, 54);
    near.banner(g, gx + 70, 54, 54);
    for (let bx = x0 + 60; bx < x1 - 20; bx += 115) {
      if (Math.abs(bx - gx) < 75) continue;
      near.brazier(g, bx, S);
    }
    near.brazier(g, gx - 52, S);
    near.brazier(g, gx + 52, S);
  }

  // ---------- Zemin ----------
  function ground(g, w) {
    R(g, 0, GY - 2, w, AH - GY + 2, '#2a2026');
    R(g, 0, GY - 2, w, 1, '#4e3e40');
    const rows = [3, 3, 4, 4, 5, 5, 6, 7];
    let y = GY;
    rows.forEach((h, ri) => {
      let x = -Math.floor(hash(ri, 1) * 8);
      while (x < w) {
        const d = district(x * PX);
        let sw, c;
        const v = hash(x, y);
        if (d === 'cami') {
          sw = 14 + Math.floor(v * 10);
          c = v < 0.3 ? '#6e6266' : v < 0.7 ? '#7a6e72' : '#86797c';
        } else if (d === 'bostan' || d === 'sur') {
          sw = 4 + Math.floor(v * 5);
          c = v < 0.4 ? '#5e4838' : v < 0.8 ? '#664e3c' : '#76604c';
        } else {
          sw = 4 + Math.floor(v * 4) + Math.floor(ri / 2);
          c = v < 0.3 ? '#463638' : v < 0.7 ? '#504040' : '#5c4a46';
        }
        R(g, x, y, sw - 1, h - 1, c);
        R(g, x, y, Math.max(1, sw - 3), 1, lt(c, 0.14));
        R(g, x + sw - 2, y + 1, 1, h - 2, dk(c, 0.18));
        x += sw;
      }
      y += h;
    });
    // su birikintileri ve tekerlek izleri
    for (let x = 30; x < w; x += 90 + Math.floor(hash(x, 9) * 140)) {
      const d = district(x * PX);
      if (d === 'mahalle' || d === 'carsi') {
        const py = GY + 10 + Math.floor(hash(x, 4) * 18), pw = 10 + Math.floor(hash(x, 5) * 10);
        for (let j = 0; j < 2; j++) R(g, x + j * 2, py + j, pw - j * 4, 1, j === 0 ? '#6a2a34' : '#3a1c26');
        R(g, x + 3, py, 3, 1, '#c84a3a');
      } else if (d === 'bostan' || d === 'sur') {
        R(g, x, GY + 12, 60, 1, '#4a382c');
        R(g, x + 6, GY + 24, 60, 1, '#4a382c');
      }
    }
  }

  // Sokak süsleri (fıçı, sandık, çuval)
  function props(g, w, S) {
    for (let x = 40; x < w; x += 70 + Math.floor(hash(x, 2) * 120)) {
      const d = district(x * PX);
      if (d === 'sur' || d === 'cami') continue;
      const k = hash(x, 7);
      if (k < 0.3) {
        R(g, x, GY - 11, 8, 11, '#6a4428');
        R(g, x + 6, GY - 11, 2, 11, '#8a5e38');
        R(g, x, GY - 9, 8, 1, '#3a2a22');
        R(g, x, GY - 3, 8, 1, '#3a2a22');
        R(g, x + 1, GY - 12, 6, 1, '#4a3020');
      } else if (k < 0.55) {
        R(g, x, GY - 8, 12, 8, '#7a5634');
        R(g, x, GY - 8, 12, 1, '#9a7448');
        R(g, x + 5, GY - 8, 1, 8, '#5a3e26');
        if (d === 'carsi') for (let i = 0; i < 4; i++) P(g, x + 2 + i * 3, GY - 9, '#e8802a');
      } else if (k < 0.75) {
        blob(g, x + 5, GY - 5, 5, 5, ['#8a7254', '#a88c66', '#c8ac80'], x);
        R(g, x + 4, GY - 11, 2, 2, '#6a5434');
      }
    }
  }

  function genNear(w, S) {
    const L = mk(w, AH), g = L.g, r = K.seeded(1453);
    const gx = Math.round(K.GATE_X / PX);
    const wallX = Math.floor((K.GATE_X - 1150) / PX);
    let x = -10, sinceLamp = 0, portalDone = false;
    const fronts = [];
    const later = [];
    while (x < wallX) {
      const d = district(x * PX);
      const k = r();
      let used = 0;
      if (d === 'mahalle') {
        if (k < 0.4) {
          used = 50 + Math.floor(r() * 26);
          near.houseFront(g, x, used, r, S);
          if (r() < 0.3) later.push(['torch', x + 6, GY - 32]);
        } else if (k < 0.7) {
          used = 40 + Math.floor(r() * 30);
          near.gardenWall(g, x, used, r, S);
        } else if (k < 0.82) {
          near.cesme(g, x + 4);
          used = 44;
        } else {
          used = 14 + Math.floor(r() * 8);
          near.alley(g, x, used, r);
        }
      } else if (d === 'carsi') {
        if (k < 0.15) {
          near.hanGate(g, x, r, S);
          used = 72;
          later.push(['banner', x + 6, 90, 34], ['banner', x + 50, 90, 34]);
        } else {
          used = 44 + Math.floor(r() * 14);
          near.shop(g, x, used, r, S);
          if (r() < 0.3) later.push(['brazier', x + used]);
        }
      } else if (d === 'cami') {
        if (!portalDone && x * PX > 11600) {
          near.portal(g, x, r, S);
          portalDone = true;
          used = 58;
          later.push(['torch', x + 3, GY - 50], ['torch', x + 54, GY - 50]);
        } else {
          used = 64;
          near.courtyard(g, x, used, r, S);
          if (r() < 0.6) fronts.push([x + 64, 74 + Math.floor(r() * 20)]);
        }
      } else {
        used = near.bostan(g, x, r, S);
        if (r() < 0.12) later.push(['camp', x + used + 6]);
      }
      sinceLamp += used;
      if (sinceLamp > 110 && d !== 'cami' && d !== 'bostan') {
        near.lamp(g, x + used - 3, S);
        sinceLamp = 0;
      }
      x += used;
    }
    for (const [cx, ch] of fronts) cypressAt(g, cx, GY, ch, 7, ['#1a2c22', '#2c4634', '#5a7a4c']);
    for (const [k, a, b, c] of later) {
      if (k === 'torch') near.wallTorch(g, a, b, S);
      else if (k === 'brazier') near.brazier(g, a, S);
      else if (k === 'camp') near.campfire(g, a, S);
      else if (k === 'banner') near.banner(g, a, b, c);
    }
    cityWalls(g, wallX, w, gx, S);
    ground(g, w);
    props(g, wallX, S);
    haze(L, '#0e0a1a', y => (y < GY ? 0.58 : 0.4), 0, true);
    // kapıdan sokağa düşen ışık
    if (S.gate) {
      paint(g, gx - 140, GY - 1, 280, AH - GY + 1, (x, y) => {
        const spread = 28 + (y - GY) * 3.2;
        const dx = Math.abs(x - gx);
        if (dx > spread) return null;
        const t = (1 - dx / spread) * (1 - (y - GY) / 60);
        return t > bayer(x, y) + 0.15 ? '#c8885a' : null;
      });
    }
    return L;
  }

  // ---------- Canlı efekt hareketli parçaları ----------
  function glowSprite(rad, col) {
    const L = mk(rad * 2 + 1, rad * 2 + 1);
    paint(L.g, 0, 0, L.w, L.h, (x, y) => {
      const d = Math.hypot(x - rad, y - rad) / rad;
      if (d > 1) return null;
      const v = d < 0.28 ? 0.85 : Math.pow(1 - d, 1.8) * 0.7;
      return v > bayer(x, y) ? col : null;
    });
    return L;
  }

  function genFog(w, h, col, dens, seed) {
    const L = mk(w, h);
    const r = K.seeded(seed);
    const bumps = [];
    for (let i = 0; i < 14; i++) bumps.push({ x: r() * w, rx: 30 + r() * 70, a: 0.4 + r() * 0.6 });
    paint(L.g, 0, 0, w, h, (x, y) => {
      let v = 0;
      for (const b of bumps) {
        for (const o of [-w, 0, w]) {
          const dx = (x - b.x - o) / b.rx;
          if (Math.abs(dx) < 1) v = Math.max(v, (1 - dx * dx) * b.a);
        }
      }
      const vy = Math.sin((y / h) * Math.PI);
      return v * vy * dens > bayer(x, y) + 0.05 ? col : null;
    });
    return L;
  }

  // En önde, karakterlerin önünde kalan koyu silüetler
  function genFore(w) {
    const L = mk(w, AH), g = L.g, r = K.seeded(77);
    const C = '#06040a', C2 = '#120c16', rim = '#3a1820';
    let x = 160;
    while (x < w) {
      const k = r();
      if (k < 0.35) {
        const n = 3 + Math.floor(r() * 4);
        for (let i = 0; i < n; i++) {
          const px = x + i * 10, h = 22 + Math.floor(r() * 18);
          R(g, px, AH - h, 4, h, C);
          R(g, px + 1, AH - h - 2, 2, 2, C);
          R(g, px + 3, AH - h, 1, Math.round(h * 0.5), rim);
        }
        R(g, x - 3, AH - 15, n * 10 + 4, 3, C);
        x += n * 10;
      } else if (k < 0.6) {
        blob(g, x + 22, AH + 6, 28, 20, [C, C2, rim], x);
        x += 50;
      } else if (k < 0.82) {
        for (let i = 0; i < 16; i++) {
          const h = 6 + Math.floor(r() * 14);
          for (let j = 0; j < h; j++) P(g, x + i * 2 + Math.round((j / h) * (r() < 0.5 ? -2 : 2)), AH - j, j > h - 3 ? rim : C);
        }
        x += 34;
      } else {
        const cx = x + 14, cy = AH - 4;
        for (let a = 0; a < 64; a++) {
          const ang = (a / 64) * Math.PI * 2;
          P(g, cx + Math.cos(ang) * 13, cy + Math.sin(ang) * 13, C);
          P(g, cx + Math.cos(ang) * 12, cy + Math.sin(ang) * 12, C);
        }
        for (let s2 = 0; s2 < 6; s2++) line(g, cx, cy, cx + Math.round(Math.cos(s2) * 12), cy + Math.round(Math.sin(s2) * 12), C);
        x += 30;
      }
      x += 260 + Math.floor(r() * 380);
    }
    return L;
  }

  K.bg = {
    init(worldW) {
      this.worldW = worldW;
      this.lamps = [];
      this.fires = [];
      this.embers = [];
      this.lastT = 0;
      this.flags = [];
      this.smokeRoof = [];
      this.smokeMid = [];
      this.skies = [
        genSky(0, 6.2, 150, 1.6, 160, 1, { x: 80, y: 52, r: 22, a: 0.9 }),
        genSky(0.4, 8.6, 122, 2.6, 70, 2, { x: 70, y: 78, r: 22, a: 0.6 }),
        genSky(1.4, 10.8, 96, 3.4, 10, 3, { x: 60, y: 108, r: 22, a: 0.3 }),
      ];
      this.fogBack = genFog(640, 34, '#3a2a48', 0.8, 5);
      this.fogFront = genFog(640, 26, '#4a3858', 0.55, 6);
      const r = K.seeded(99);
      this.clouds = [];
      for (let i = 0; i < 9; i++) this.clouds.push({ L: genCloud(r), x: r() * 900, y: 24 + r() * 100, p: 0.02 + r() * 0.04, v: 1 + r() * 2 });
      this.far = genFar(Math.ceil((worldW * 0.1) / PX) + AW + 40);
      this.roofs = genRoofs(Math.ceil((worldW * 0.25) / PX) + AW + 40, this.smokeRoof);
      this.mid = genMid(Math.ceil((worldW * 0.55) / PX) + AW + 40, this.smokeMid);
      this.near = genNear(Math.ceil(worldW / PX) + AW, this);
      this.glow = glowSprite(16, 'rgba(255,200,120,0.5)');
      this.glowSmall = glowSprite(9, 'rgba(255,214,140,0.6)');
      this.fireGlow = glowSprite(30, 'rgba(255,128,50,0.42)');
      this.fore = genFore(Math.ceil((worldW * 1.25) / PX) + AW + 40);
      this.ash = [];
      for (let i = 0; i < 40; i++) this.ash.push({ x: r() * AW, y: r() * AH, v: 2 + r() * 5, ph: r() * 6 });
      this.art = mk(AW, AH);
      this.birds = [];
      for (let i = 0; i < 6; i++) this.birds.push({ x: r() * AW, y: 40 + r() * 80, v: 8 + r() * 10, ph: r() * 6 });
    },
    // Resimli katmanlar: semt sınırına yakınken iki semt yumuşakça karışır
    paintedSets(camX) {
      if (!K.assets) return null;
      const c = camX + K.W / 2;
      const B = [4700, 9500, 14300, K.GATE_X - 1150];
      const span = 450;
      let sets = [{ d: district(c), a: 1 }];
      for (const b of B) {
        if (c > b - span && c < b + span) sets = [{ d: district(b - 1), a: 1 }, { d: district(b + 1), a: (c - (b - span)) / (span * 2) }];
      }
      for (const s of sets) {
        s.layers = K.assets.bgLayers(s.d);
        if (!s.layers) return null;
      }
      return sets;
    },
    drawPainted(ctx, camX, sets) {
      ctx.imageSmoothingEnabled = true;
      for (const s of sets) {
        ctx.globalAlpha = s.a;
        for (const l of s.layers) {
          const im = l.img;
          const ih = im.naturalHeight || im.height, iw = im.naturalWidth || im.width;
          const sc = K.H / ih, w = iw * sc;
          const off = (((camX * (l.parallax || 0)) % w) + w) % w;
          for (let x = -off; x < K.W; x += w) ctx.drawImage(im, x, 0, w + 1, K.H);
        }
      }
      ctx.globalAlpha = 1;
      const t = performance.now() / 1000;
      ctx.fillStyle = 'rgba(170,140,150,0.45)';
      for (const a of this.ash) {
        const ax = (((a.x + t * a.v * 0.7 - (camX / PX) * 0.3) % AW) + AW) % AW;
        const ay = (((a.y + t * a.v * 0.5 + Math.sin(t + a.ph) * 3) % AH) + AH) % AH;
        ctx.fillRect(ax * PX, ay * PX, 2, 2);
      }
    },
    draw(ctx, camX) {
      const sets = this.paintedSets(camX);
      this.paintedActive = !!sets;
      if (sets) return this.drawPainted(ctx, camX, sets);
      const g = this.art.g;
      const t = performance.now() / 1000;
      const prog = K.clamp(camX / Math.max(1, this.worldW - 2000 - K.W), 0, 1);
      // gökyüzü geçişi: şafak öncesi -> şafak -> gün doğumu
      const k = prog * 2;
      const a = Math.min(1, Math.floor(k)), b = Math.min(2, a + 1);
      g.globalAlpha = 1;
      g.drawImage(this.skies[a].c, 0, 0);
      g.globalAlpha = K.clamp(k - a, 0, 1);
      g.drawImage(this.skies[b].c, 0, 0);
      g.globalAlpha = 1;
      // yıldız pırıltısı
      if (prog < 0.5) {
        for (let i = 0; i < 12; i++) {
          if (Math.sin(t * 3 + i * 7.3) > 0.85) P(g, (i * 97) % AW, (i * 41) % 80, '#ffffff');
        }
      }
      // bulutlar
      for (const c of this.clouds) {
        const span = AW + c.L.w + 40;
        const cx = ((c.x - camX * c.p / PX + t * c.v) % span + span) % span - c.L.w - 20;
        g.drawImage(c.L.c, Math.round(cx), Math.round(c.y));
      }
      // kuşlar
      for (const bd of this.birds) {
        const x = Math.round(((bd.x + t * bd.v) % (AW + 40)) - 20), y = Math.round(bd.y + Math.sin(t * 0.7 + bd.ph) * 6);
        const up = Math.sin(t * 9 + bd.ph) > 0;
        P(g, x, y, '#2a1c2e');
        P(g, x - 1, y + (up ? -1 : 1), '#2a1c2e');
        P(g, x + 1, y + (up ? -1 : 1), '#2a1c2e');
        P(g, x - 2, y + (up ? -1 : 1), '#2a1c2e');
        P(g, x + 2, y + (up ? -1 : 1), '#2a1c2e');
      }
      const strip = (L, par, dy = 0) => {
        const off = Math.round((camX * par) / PX);
        g.drawImage(L.c, off, 0, AW, AH, 0, dy, AW, AH);
        if (dy < 0) {
          g.fillStyle = '#1e1528';
          g.fillRect(0, AH + dy, AW, -dy);
        }
        return off;
      };
      strip(this.far, 0.1, FAR_DY);
      const ro = strip(this.roofs, 0.25, ROOF_DY);
      this.drawSmoke(g, this.smokeRoof, ro, t, '#4a3a52', ROOF_DY);
      const mo = strip(this.mid, 0.55);
      this.drawSmoke(g, this.smokeMid, mo, t, '#5a4a5e');
      const fb = this.fogBack;
      const fo = (((camX * 0.7) / PX - t * 3) % fb.w + fb.w) % fb.w;
      g.globalAlpha = 0.85;
      g.drawImage(fb.c, -fo, 128);
      g.drawImage(fb.c, fb.w - fo, 128);
      g.globalAlpha = 1;

      ctx.imageSmoothingEnabled = false;
      ctx.drawImage(this.art.c, 0, 0, AW, AH, 0, 0, AW * PX, AH * PX);
      // yakın katman: tam dünya hizasında (karakterlerle kaymasın)
      const sx = Math.floor(camX / PX);
      ctx.drawImage(this.near.c, sx, 0, AW + 2, AH, sx * PX - camX, 0, (AW + 2) * PX, AH * PX);
      const ff = this.fogFront;
      const ffo = ((camX / PX - t * 5) % ff.w + ff.w) % ff.w;
      ctx.globalAlpha = 0.5;
      for (let i = -1; i < 2; i++) ctx.drawImage(ff.c, (i * ff.w - ffo) * PX, (GY - 22) * PX, ff.w * PX, ff.h * PX);
      ctx.globalAlpha = 1;
      // ateşler ve korlar
      const dt = Math.min(0.05, Math.max(0, t - this.lastT));
      this.lastT = t;
      for (const f of this.fires) {
        const fx = f.x * PX - camX;
        if (fx < -120 || fx > K.W + 120) continue;
        const fl = 0.75 + Math.sin(t * 11 + f.s) * 0.12 + Math.sin(t * 17.3 + f.s * 2) * 0.1;
        ctx.globalAlpha = fl;
        const gs = f.w > 8 ? 1 : 0.6;
        const gw = this.fireGlow.w * PX * gs;
        ctx.drawImage(this.fireGlow.c, fx - gw / 2, (f.y - f.h * 0.4) * PX - gw / 2, gw, gw);
        ctx.globalAlpha = 1;
        this.drawFire(ctx, f, t, camX);
        if (Math.random() < dt * (f.w > 8 ? 7 : 3)) {
          this.embers.push({ x: f.x * PX + K.rand(-f.w, f.w), y: (f.y - f.h * 0.6) * PX, vx: K.rand(-20, 20), vy: K.rand(-110, -50), life: K.rand(1, 2.4), max: 2.4, s: Math.random() * 6 });
        }
      }
      for (const e of this.embers) {
        e.life -= dt;
        e.vx += Math.sin(t * 3 + e.s) * 40 * dt;
        e.x += e.vx * dt;
        e.y += e.vy * dt;
      }
      this.embers = this.embers.filter(e => e.life > 0);
      for (const e of this.embers) {
        const k = e.life / e.max;
        ctx.fillStyle = k > 0.6 ? '#ffd27a' : k > 0.3 ? '#ff7a2a' : '#9a2a1a';
        ctx.fillRect(Math.round((e.x - camX) / PX) * PX - ((camX % PX) + PX) % PX, Math.round(e.y / PX) * PX, PX, PX);
      }
      // havada süzülen kül
      ctx.fillStyle = 'rgba(150,120,140,0.5)';
      for (const a of this.ash) {
        const ax = (((a.x + t * a.v * 0.7 - camX / PX * 0.3) % AW) + AW) % AW;
        const ay = (((a.y + t * a.v * 0.5 + Math.sin(t + a.ph) * 3) % AH) + AH) % AH;
        ctx.fillRect(Math.round(ax) * PX, Math.round(ay) * PX, PX, PX);
      }
      // fener parıltıları
      for (const l of this.lamps) {
        const lx = l.x * PX - camX;
        if (lx < -80 || lx > K.W + 80) continue;
        const f = 0.75 + Math.sin(t * 13 + l.x) * 0.08 + Math.sin(t * 7.7 + l.x * 3) * 0.08;
        ctx.globalAlpha = f;
        ctx.drawImage(this.glow.c, lx - 16 * PX, l.y * PX - 16 * PX, this.glow.w * PX, this.glow.h * PX);
        ctx.globalAlpha = 1;
      }
      // sancaklar
      for (const fl of this.flags) {
        const fx = fl.x * PX - camX;
        if (fx < -60 || fx > K.W + 60) continue;
        ctx.fillStyle = '#2a2026';
        ctx.fillRect(fx, (fl.y - 16) * PX, PX, 16 * PX);
        for (let i = 0; i < 14; i++) {
          const wy = Math.round(Math.sin(t * 5 - i * 0.6) * (i / 6));
          ctx.fillStyle = i === 13 ? '#7a1a1e' : '#b0232a';
          ctx.fillRect(fx + (i + 1) * PX, (fl.y - 16 + wy) * PX, PX, 8 * PX);
          ctx.fillStyle = '#d8b060';
          ctx.fillRect(fx + (i + 1) * PX, (fl.y - 16 + wy) * PX, PX, PX);
        }
      }
    },
    // Piksel ateş: alttan üste dar, beyaz-sarı çekirdek, turuncu, kızıl uçlar
    drawFire(ctx, f, t, camX) {
      const tq = Math.floor(t * 12);
      const bx = Math.round(f.x * PX - camX), by = f.y * PX;
      const rect = (ax, w, row, col) => {
        ctx.fillStyle = col;
        ctx.fillRect(bx + ax * PX, by - row * PX, w * PX, PX);
      };
      for (let i = 0; i < f.h; i++) {
        const k = i / f.h;
        const wob = Math.round(Math.sin(tq * 0.9 + i * 0.6 + f.s) * k * 2);
        const half = Math.round((f.w / 2) * Math.pow(1 - k, 0.8) * (0.8 + 0.4 * hash(tq + f.s, i)));
        if (half < 0) continue;
        rect(wob - half, half * 2 + 1, i, k > 0.7 ? '#8a2216' : '#d84a1c');
        const m = Math.round(half * 0.62);
        if (k < 0.85) rect(wob - m, m * 2 + 1, i, '#ff9a2e');
        const c = Math.round(half * 0.28);
        if (k < 0.55) rect(wob - c, c * 2 + 1, i, '#ffeaa8');
      }
      for (let j = 0; j < 2; j++) {
        if (hash(tq + j, f.s) > 0.45) rect(Math.round((hash(tq, j + f.s) - 0.5) * f.w * 0.6), 1, f.h + 1 + Math.floor(hash(j, tq) * 4), '#d84a1c');
      }
    },
    // Bir noktadaki en güçlü ışık (karakter aydınlatması için)
    lightAt(x, y) {
      if (this.paintedActive) return null;
      let best = null, bk = 0;
      for (const f of this.fires) {
        const d = Math.hypot(f.x * PX - x, (f.y * PX - y) * 0.6);
        const k = 1 - d / (f.w > 8 ? 330 : 220);
        if (k > bk) {
          bk = k;
          best = { dir: K.sign(f.x * PX - x), k, col: '#ffa040' };
        }
      }
      for (const l of this.lamps) {
        const d = Math.hypot(l.x * PX - x, (l.y * PX - y) * 0.6);
        const k = (1 - d / 200) * 0.6;
        if (k > bk) {
          bk = k;
          best = { dir: K.sign(l.x * PX - x), k, col: '#ffcf7a' };
        }
      }
      return best;
    },
    drawFore(ctx, camX) {
      const off = (camX * 1.25) / PX;
      const sx = Math.floor(off);
      ctx.imageSmoothingEnabled = false;
      ctx.drawImage(this.fore.c, sx, 0, AW + 2, AH, Math.round((sx - off) * PX), 0, (AW + 2) * PX, AH * PX);
    },
    drawSmoke(g, list, off, t, col, dy = 0) {
      g.fillStyle = col;
      for (const s of list) {
        const x0 = s.x - off;
        if (x0 < -10 || x0 > AW + 10) continue;
        for (let i = 0; i < 5; i++) {
          const ph = (t * 0.25 + i / 5 + s.s) % 1;
          g.globalAlpha = (1 - ph) * 0.55;
          const sz = ph > 0.5 ? 2 : 1;
          g.fillRect(Math.round(x0 + Math.sin(ph * 4 + s.s) * 1.5 + ph * 8), Math.round(s.y + dy - ph * 20), sz, sz);
        }
      }
      g.globalAlpha = 1;
    },
  };
})(window.K);
