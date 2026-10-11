'use strict';
// Karakter illüstrasyonları: her parça yüksek çözünürlükte (S kat) bir kez çizilir, sonra iskelete takılır.
// Kullanıcının resim dosyaları (assets/assets.js) varsa onlar önceliklidir.
(function (K) {
  const S = 4; // iskelet birimi başına piksel
  const PI = Math.PI;
  const sh = (c, f) => K.shade(c, f);

  function canvas(w, h) {
    const c = document.createElement('canvas');
    c.width = Math.max(1, Math.ceil(w));
    c.height = Math.max(1, Math.ceil(h));
    return c;
  }
  // Parça tuvali: a eklemi (0,0); b ucu verilen noktada. Sınırlar iskelet biriminde.
  function part(bounds, bEnd, draw) {
    const [wl, wr, top, bottom] = bounds, pad = 2;
    const c = canvas((wl + wr + pad * 2) * S, (top + bottom + pad * 2) * S);
    const g = c.getContext('2d');
    const ax = (wl + pad) * S, ay = (top + pad) * S;
    g.translate(ax, ay);
    g.scale(S, S);
    g.lineJoin = 'round';
    g.lineCap = 'round';
    draw(g);
    return { img: c, a: [ax, ay], b: [ax + bEnd[0] * S, ay + bEnd[1] * S], scale: 1 / S };
  }
  function darkened(p, amt) {
    const c = canvas(p.img.width, p.img.height);
    const g = c.getContext('2d');
    g.drawImage(p.img, 0, 0);
    g.globalCompositeOperation = 'source-atop';
    g.fillStyle = `rgba(10,6,22,${amt})`;
    g.fillRect(0, 0, c.width, c.height);
    return Object.assign({}, p, { img: c });
  }

  // ---------- Boyama yardımcıları ----------
  function cyl(g, x0, x1, base, lift = 1.18) {
    const gr = g.createLinearGradient(x0, 0, x1, 0);
    gr.addColorStop(0, sh(base, 0.4));
    gr.addColorStop(0.3, sh(base, 0.78));
    gr.addColorStop(0.64, sh(base, lift));
    gr.addColorStop(0.86, sh(base, 0.95));
    gr.addColorStop(1, sh(base, 0.55));
    return gr;
  }
  function vgr(g, y0, y1, a, b) {
    const gr = g.createLinearGradient(0, y0, 0, y1);
    gr.addColorStop(0, a);
    gr.addColorStop(1, b);
    return gr;
  }
  function limb(g, L, w0, w1, bulge = 0.6) {
    g.beginPath();
    g.moveTo(-w0 / 2, 0);
    g.quadraticCurveTo(-w0 / 2 - bulge, L * 0.45, -w1 / 2, L);
    g.arc(0, L, w1 / 2, PI, 0, true);
    g.quadraticCurveTo(w0 / 2 + bulge, L * 0.45, w0 / 2, 0);
    g.arc(0, 0, w0 / 2, 0, PI, true);
    g.closePath();
  }
  function ink(g, w = 0.45, col = '#0e0806') {
    w *= 0.75;
    g.lineWidth = w;
    g.strokeStyle = col;
    g.stroke();
  }
  function strokes(g, list, col, w) {
    g.strokeStyle = col;
    g.lineWidth = w;
    for (const s of list) {
      g.beginPath();
      g.moveTo(s[0], s[1]);
      if (s.length === 6) g.quadraticCurveTo(s[2], s[3], s[4], s[5]);
      else g.lineTo(s[2], s[3]);
      g.stroke();
    }
  }
  // Boya dokusu: şeklin içine ince ışık-gölge benekleri
  function grain(g, x, y, w, h, n = 120, seed = 1) {
    const r = K.seeded(seed);
    g.save();
    g.globalCompositeOperation = 'source-atop';
    for (let i = 0; i < n * 1.6; i++) {
      g.fillStyle = r() < 0.6 ? 'rgba(10,4,4,0.13)' : 'rgba(255,236,214,0.07)';
      g.fillRect(x + r() * w, y + r() * h, 0.35 + r() * 0.7, 0.25 + r() * 0.4);
    }
    // fırça darbeleri: kısa, eğik, düzensiz
    for (let i = 0; i < n * 0.5; i++) {
      const bx = x + r() * w, by = y + r() * h, a = -0.9 + r() * 0.5, l = 0.8 + r() * 2.4;
      g.strokeStyle = r() < 0.65 ? 'rgba(12,6,6,0.14)' : 'rgba(255,230,200,0.08)';
      g.lineWidth = 0.3 + r() * 0.4;
      g.beginPath();
      g.moveTo(bx, by);
      g.lineTo(bx + Math.cos(a) * l, by + Math.sin(a) * l);
      g.stroke();
    }
    // alt kısım kirli ve koyu
    const dg = g.createLinearGradient(0, y + h * 0.55, 0, y + h);
    dg.addColorStop(0, 'rgba(14,6,6,0)');
    dg.addColorStop(1, 'rgba(14,6,6,0.32)');
    g.fillStyle = dg;
    g.fillRect(x, y, w, h);
    g.restore();
  }
  // Altın işleme: bir çizgi boyunca küçük motifler
  function embroidery(g, x0, y0, x1, y1, col, w = 1.1) {
    g.strokeStyle = sh(col, 0.65);
    g.lineWidth = w + 0.5;
    g.beginPath();
    g.moveTo(x0, y0);
    g.lineTo(x1, y1);
    g.stroke();
    g.strokeStyle = col;
    g.lineWidth = w;
    g.stroke();
    const n = Math.max(2, Math.round(Math.hypot(x1 - x0, y1 - y0) / 2.2));
    g.fillStyle = sh(col, 1.35);
    for (let i = 0; i <= n; i++) {
      const t = i / n;
      g.beginPath();
      g.arc(x0 + (x1 - x0) * t, y0 + (y1 - y0) * t, w * 0.32, 0, PI * 2);
      g.fill();
    }
  }
  function furStrokes(g, x0, y0, w, h, base, seed) {
    const r = K.seeded(seed);
    for (let i = 0; i < 70; i++) {
      const x = x0 + r() * w, y = y0 + r() * h, l = 0.8 + r() * 1.6;
      g.strokeStyle = r() < 0.5 ? sh(base, 1.35) : sh(base, 0.6);
      g.lineWidth = 0.35;
      g.beginPath();
      g.moveTo(x, y);
      g.lineTo(x - l * 0.4, y - l);
      g.stroke();
    }
  }

  // ---------- Parça çizerleri ----------
  const R = () => K.RIG;
  // Yumuşak gölge lekesi (şeklin içinde kalır)
  function soft(g, x, y, rx, ry, col, a) {
    g.save();
    g.globalCompositeOperation = 'source-atop';
    const gr = g.createRadialGradient(x, y, 0, x, y, Math.max(rx, ry));
    gr.addColorStop(0, col.replace('A', a));
    gr.addColorStop(1, col.replace('A', 0));
    g.fillStyle = gr;
    g.translate(x, y);
    g.scale(rx / Math.max(rx, ry), ry / Math.max(rx, ry));
    g.translate(-x, -y);
    g.fillRect(x - Math.max(rx, ry), y - Math.max(rx, ry), Math.max(rx, ry) * 2, Math.max(rx, ry) * 2);
    g.restore();
  }
  const DARK = 'rgba(16,6,14,A)', LIGHT = 'rgba(255,236,214,A)';

  function thigh(L) {
    const T = R().TH;
    return part([8, 8, 4, T + 5], [0, T], g => {
      limb(g, T, 12, 9, 1);
      g.fillStyle = cyl(g, -6.5, 6.5, L.pants, 1.25);
      g.fill();
      grain(g, -7, -5, 14, T + 8, 110, 3);
      soft(g, -4, T * 0.5, 3, T * 0.5, DARK, 0.45);
      strokes(g, [[-3.6, T * 0.25, 0, T * 0.36, 3.6, T * 0.32], [-2.6, T * 0.55, 1, T * 0.66, 3.8, T * 0.6], [-4, T * 0.8, -1, T * 0.88, 2.4, T * 0.84]], sh(L.pants, 0.55), 0.4);
      strokes(g, [[1.6, T * 0.3, 0.4, T * 0.62, 2, T * 0.86]], 'rgba(255,236,214,0.12)', 0.9);
      limb(g, T, 12, 9, 1);
      ink(g, 0.38);
    });
  }
  function shin(L) {
    const N = R().SN;
    return part([7, 14, 4, N + 7], [0, N], g => {
      limb(g, N, 9, 6.8, 0.6);
      g.fillStyle = cyl(g, -5, 5, L.pants);
      g.fill();
      ink(g, 0.38);
      const t = 9.5;
      g.beginPath();
      g.moveTo(-5, t);
      g.lineTo(5, t);
      g.quadraticCurveTo(5.4, N * 0.6, 4.4, N - 1.5);
      g.quadraticCurveTo(12, N - 1.2, 12.6, N + 2.8);
      g.lineTo(-4.6, N + 3.3);
      g.quadraticCurveTo(-5.8, N * 0.6, -5, t);
      g.closePath();
      g.fillStyle = cyl(g, -5.5, 6, L.boots, 1.38);
      g.fill();
      grain(g, -6, t, 19, N - t + 4, 80, 5);
      soft(g, -3.5, N * 0.62, 2.4, N * 0.35, DARK, 0.5);
      soft(g, 2, N * 0.45, 1.6, N * 0.3, LIGHT, 0.35);
      ink(g, 0.38);
      g.fillStyle = '#160c08';
      g.fillRect(-4.6, N + 2.2, 17.2, 1.4);
      // konç
      g.beginPath();
      g.moveTo(-5.8, t - 1.4);
      g.quadraticCurveTo(0, t - 2.6, 5.8, t - 1.4);
      g.lineTo(5.5, t + 2.4);
      g.quadraticCurveTo(0, t + 1.6, -5.5, t + 2.4);
      g.closePath();
      g.fillStyle = cyl(g, -6, 6, sh(L.boots, 1.1), 1.4);
      g.fill();
      ink(g, 0.32);
      g.setLineDash([0.55, 0.55]);
      strokes(g, [[-4.8, t + 1.4, 4.8, t + 1.4], [3.7, t + 3.4, 3.4, N - 1.6]], sh(L.trim, 0.85), 0.28);
      g.setLineDash([]);
    });
  }
  function upperArm(L) {
    const U = R().UA;
    return part([7, 7, 6, U + 5], [0, U], g => {
      limb(g, U, 11, 8.4, 0.8);
      g.fillStyle = cyl(g, -6, 6, L.coat, 1.25);
      g.fill();
      grain(g, -6, -6, 12, U + 10, 80, 7);
      soft(g, -3.8, U * 0.5, 2.6, U * 0.55, DARK, 0.45);
      strokes(g, [[-3.2, U * 0.42, 0, U * 0.56, 3.2, U * 0.48], [-2.6, U * 0.7, 0, U * 0.8, 3, U * 0.74]], sh(L.coat, 0.55), 0.42);
      limb(g, U, 11, 8.4, 0.8);
      ink(g, 0.38);
      g.beginPath();
      g.arc(0, 1, 5.6, PI * 1.05, PI * 1.95);
      g.strokeStyle = L.trim;
      g.lineWidth = 0.9;
      g.stroke();
      if (L.pauldron) {
        for (let i = 2; i >= 0; i--) {
          const y = -1.5 + i * 3.2;
          g.beginPath();
          g.moveTo(-6.4, y + 1);
          g.quadraticCurveTo(0, y - 2.6, 6.6, y + 0.6);
          g.lineTo(6.2, y + 3.6);
          g.quadraticCurveTo(0, y + 1.4, -6, y + 4);
          g.closePath();
          g.fillStyle = cyl(g, -6.5, 6.6, i === 0 ? '#5a3a24' : '#4a2e1c', 1.45);
          g.fill();
          ink(g, 0.32);
          g.fillStyle = '#b8945a';
          for (const x of [-3.6, 0.4, 4]) {
            g.beginPath();
            g.arc(x, y + 1.6 - (x === 0.4 ? 1 : 0.3), 0.42, 0, PI * 2);
            g.fill();
          }
        }
      }
    });
  }
  function foreArm(L) {
    const F = R().FA;
    const cuffEnd = F - 5;
    return part([6, 7, 5, F + 6], [0, F], g => {
      limb(g, cuffEnd, 8.4, 7.2, 0.5);
      g.fillStyle = cyl(g, -5, 5, L.coat, 1.25);
      g.fill();
      soft(g, -3, cuffEnd * 0.5, 2, cuffEnd * 0.5, DARK, 0.4);
      ink(g, 0.38);
      if (L.bracer) {
        g.beginPath();
        g.moveTo(-4.1, cuffEnd - 9);
        g.lineTo(4.3, cuffEnd - 9.5);
        g.lineTo(4.1, cuffEnd + 1.2);
        g.lineTo(-3.9, cuffEnd + 1.6);
        g.closePath();
        g.fillStyle = cyl(g, -4.5, 4.5, '#5a3820', 1.4);
        g.fill();
        grain(g, -4.5, cuffEnd - 10, 9, 12, 40, 31);
        ink(g, 0.32);
        strokes(g, [[-4, cuffEnd - 6, 4.2, cuffEnd - 6.4], [-3.9, cuffEnd - 2.4, 4.1, cuffEnd - 2.8]], '#2e1c10', 0.3);
        g.fillStyle = '#d8d0c8';
        for (const [x, y] of [[-1.6, cuffEnd - 7.6], [1.8, cuffEnd - 7.8], [-1.5, cuffEnd - 0.8], [1.9, cuffEnd - 1]]) {
          g.beginPath();
          g.arc(x, y, 0.5, 0, PI * 2);
          g.fill();
        }
      } else {
        g.fillStyle = vgr(g, cuffEnd - 3, cuffEnd, sh(L.trim, 1.15), sh(L.trim, 0.65));
        g.fillRect(-3.8, cuffEnd - 2.6, 7.8, 2.6);
      }
      // yumruk
      g.beginPath();
      g.moveTo(-3, F - 3.4);
      g.quadraticCurveTo(-3.6, F + 1.8, 0, F + 3.2);
      g.quadraticCurveTo(4.6, F + 3, 4.4, F - 0.6);
      g.quadraticCurveTo(4.6, F - 3.8, 1.2, F - 4);
      g.closePath();
      const hg = g.createRadialGradient(2, F - 1.6, 0.4, 0.6, F, 4.6);
      hg.addColorStop(0, sh(L.skin, 1.15));
      hg.addColorStop(1, sh(L.skin, 0.62));
      g.fillStyle = L.glove ? cyl(g, -3, 4, L.glove, 1.3) : hg;
      g.fill();
      ink(g, 0.36);
      strokes(g, [[2.4, F - 2.4, 4, F - 1.6], [2.6, F - 0.4, 4.2, F + 0.4], [2.2, F + 1.4, 3.6, F + 2], [-2.2, F - 1.2, -0.4, F - 3]], 'rgba(40,20,14,0.6)', 0.28);
    });
  }

  function torso(L) {
    const TR = R().TR, k = TR / 40;
    return part([22, 22, TR + 8, 8], [0, -TR], g => {
      g.scale(1, k);
      // sırttaki sadak (gövdenin arkasında)
      if (L.quiver) {
        g.save();
        g.translate(-9, -20);
        g.rotate(-0.5);
        for (let i = 0; i < 5; i++) {
          strokes(g, [[-2 + i * 1.1, -20, -2 + i * 1.1, -14]], '#7a5a38', 0.45);
          g.fillStyle = i % 2 ? '#e8e2d8' : '#a8262b';
          g.fillRect(-2.6 + i * 1.1, -22.6, 1, 2.8);
        }
        g.beginPath();
        g.roundRect(-3.6, -15, 7.4, 26, 1.6);
        g.fillStyle = cyl(g, -4, 4, '#6a3a20', 1.4);
        g.fill();
        g.fillStyle = '#c9a24a';
        g.fillRect(-3.6, -13.4, 7.4, 1.2);
        g.fillRect(-3.6, 7, 7.4, 1.2);
        for (let y = -10; y < 6; y += 3.6) strokes(g, [[-2.4, y, 2.4, y + 1.6]], 'rgba(216,176,96,0.6)', 0.35);
        g.beginPath();
        g.roundRect(-3.6, -15, 7.4, 26, 1.6);
        ink(g, 0.35);
        g.restore();
      }
      const body = () => {
        g.beginPath();
        g.moveTo(-11.5, 2);
        g.lineTo(12, 2);
        g.quadraticCurveTo(15.6, -16, 17, -33);
        g.quadraticCurveTo(13.5, -40.6, 4.4, -41.4);
        g.lineTo(-5.4, -41.4);
        g.quadraticCurveTo(-15.6, -39.6, -16.6, -33);
        g.quadraticCurveTo(-14, -16, -11.5, 2);
        g.closePath();
      };
      body();
      g.fillStyle = cyl(g, -17, 17, L.coat, 1.26);
      g.fill();
      grain(g, -17, -42, 35, 45, 240, 11);
      soft(g, -10, -18, 6, 18, DARK, 0.5);
      soft(g, 9, -30, 5, 8, LIGHT, 0.22);
      // zincir zırh (önde V açıklık)
      if (L.mail) {
        g.save();
        g.beginPath();
        g.moveTo(2, -40);
        g.lineTo(14.4, -34);
        g.lineTo(12.2, -6);
        g.lineTo(3.2, -6);
        g.closePath();
        g.fillStyle = cyl(g, 2, 15, '#8a9098', 1.32);
        g.fill();
        g.clip();
        g.strokeStyle = 'rgba(26,28,36,0.55)';
        g.lineWidth = 0.26;
        for (let y = -40, row = 0; y < -5; y += 1.2, row++) {
          for (let x = 2 + (row % 2 ? 0.65 : 0); x < 15; x += 1.3) {
            g.beginPath();
            g.arc(x, y, 0.65, 0, PI);
            g.stroke();
          }
        }
        soft(g, 4, -20, 3, 14, DARK, 0.45);
        g.restore();
        embroidery(g, 2, -40, 3.2, -6, L.trim, 1.0);
        embroidery(g, 14.4, -34, 12.2, -6, L.trim, 1.0);
      } else {
        g.strokeStyle = sh(L.coat, 0.5);
        g.lineWidth = 0.5;
        g.beginPath();
        g.moveTo(6.4, -40);
        g.lineTo(7.4, -6);
        g.stroke();
        for (let y = -35; y < -10; y += 4.6) {
          strokes(g, [[3, y, 12, y + 0.5]], sh(L.trim, 0.65), 1.4);
          strokes(g, [[3, y, 12, y + 0.5]], L.trim, 0.8);
          g.fillStyle = sh(L.trim, 1.3);
          g.beginPath();
          g.arc(7.4, y + 0.3, 0.9, 0, PI * 2);
          g.fill();
        }
      }
      // deri lamel zırh
      if (L.armor === 'leather') {
        g.save();
        g.beginPath();
        g.moveTo(-13, -36);
        g.quadraticCurveTo(2, -40, 15.6, -34);
        g.lineTo(13.4, -11);
        g.quadraticCurveTo(0, -12.4, -12.4, -11);
        g.closePath();
        g.clip();
        for (let y = -38, row = 0; y < -10; y += 2.6, row++) {
          for (let x = -14 + (row % 2) * 1.6; x < 16; x += 3.2) {
            g.fillStyle = cyl(g, x, x + 3, row % 3 === 0 ? '#5e3e26' : '#4c301e', 1.4);
            g.fillRect(x, y, 3, 2.4);
            g.fillStyle = 'rgba(8,4,2,0.55)';
            g.fillRect(x, y + 2.1, 3, 0.4);
            g.fillStyle = '#a08050';
            g.fillRect(x + 1.3, y + 0.5, 0.45, 0.45);
          }
        }
        soft(g, -8, -24, 7, 14, DARK, 0.5);
        g.restore();
        g.beginPath();
        g.moveTo(-13, -36);
        g.quadraticCurveTo(2, -40, 15.6, -34);
        g.lineTo(13.4, -11);
        g.quadraticCurveTo(0, -12.4, -12.4, -11);
        g.closePath();
        ink(g, 0.35);
      }
      // yaka
      g.beginPath();
      g.moveTo(-5.4, -41.4);
      g.quadraticCurveTo(0, -43.8, 5.2, -41.4);
      g.lineTo(4.6, -39);
      g.quadraticCurveTo(0, -40.8, -4.8, -39);
      g.closePath();
      g.fillStyle = vgr(g, -43, -39, sh(L.trim, 1.2), sh(L.trim, 0.7));
      g.fill();
      // kuşak
      g.beginPath();
      g.moveTo(-12.2, -9.4);
      g.quadraticCurveTo(1, -11, 14.4, -9.8);
      g.lineTo(13.2, 0.6);
      g.quadraticCurveTo(0, -0.4, -11.8, 0.7);
      g.closePath();
      g.fillStyle = cyl(g, -12, 14, L.sash, 1.32);
      g.fill();
      soft(g, -7, -4, 5, 5, DARK, 0.4);
      strokes(g, [[-10.6, -6.4, 1, -7.4, 13.2, -6.4], [-11, -3, 1, -3.8, 13.4, -3]], sh(L.sash, 0.55), 0.42);
      ink(g, 0.38);
      // kuşakta hançer
      if (L.dagger) {
        g.save();
        g.translate(8, -7);
        g.rotate(-1.15);
        g.fillStyle = cyl(g, -1.4, 1.4, '#3a2418');
        g.fillRect(-1.3, -6, 2.6, 6);
        g.fillStyle = cyl(g, -2, 2, '#c9a24a', 1.4);
        g.fillRect(-2.2, -0.4, 4.4, 1.4);
        g.beginPath();
        g.moveTo(-1.4, 1);
        g.lineTo(1.4, 1);
        g.quadraticCurveTo(2.4, 6, 0, 10);
        g.quadraticCurveTo(-1.6, 6, -1.4, 1);
        g.closePath();
        g.fillStyle = cyl(g, -1.6, 2.2, '#7a2a20', 1.4);
        g.fill();
        g.fillStyle = '#d8b060';
        g.fillRect(-1.4, 7, 2.6, 0.8);
        ink(g, 0.3);
        g.restore();
      }
      // kuşak düğümü
      g.fillStyle = sh(L.sash, 1.1);
      g.beginPath();
      g.ellipse(12.4, -4.8, 2.3, 1.9, 0, 0, PI * 2);
      g.fill();
      g.beginPath();
      g.moveTo(11.4, -3.4);
      g.quadraticCurveTo(14.6, 2.6, 12.8, 8);
      g.lineTo(11.2, 7.4);
      g.quadraticCurveTo(12.2, 2, 10.6, -3);
      g.closePath();
      g.fillStyle = cyl(g, 10.4, 14.6, L.sash, 1.25);
      g.fill();
      ink(g, 0.32);
      // çapraz kayış
      if (L.baldric) {
        g.beginPath();
        g.moveTo(14.2, -36.4);
        g.lineTo(16, -33.8);
        g.lineTo(-10, -8.8);
        g.lineTo(-12, -11);
        g.closePath();
        g.fillStyle = cyl(g, -12, 16, '#4a2c1a', 1.45);
        g.fill();
        ink(g, 0.32);
        g.fillStyle = cyl(g, 1, 4.6, '#d8b060', 1.4);
        g.fillRect(1.2, -23, 3, 3);
        g.fillStyle = '#4a2c1a';
        g.fillRect(1.95, -22.25, 1.5, 1.5);
      }
      // kürk omuzluk
      if (L.furMantle) {
        g.beginPath();
        g.ellipse(-0.6, -38.6, 17.4, 5.8, 0, 0, PI * 2);
        g.fillStyle = cyl(g, -18, 17, L.furMantle, 1.35);
        g.fill();
        furStrokes(g, -17, -43.6, 34, 9.6, L.furMantle, 13);
        soft(g, -10, -37, 6, 4, DARK, 0.35);
        g.beginPath();
        g.ellipse(-0.6, -38.6, 17.4, 5.8, 0, 0, PI * 2);
        ink(g, 0.32);
      }
      body();
      ink(g, 0.45);
    });
  }

  function skirt(L) {
    const Lh = 34;
    return part([22, 22, 2, Lh + 4], [0, Lh], g => {
      g.scale(1, Lh / 30);
      const rr = K.seeded(L.coat.length * 7 + 3);
      const hem = [];
      for (let x = 18.6; x >= -18; x -= 2.2) hem.push([x, 30 + (L.tattered ? (rr() < 0.5 ? -2.6 * rr() : 1.2 * rr()) : 0.6)]);
      const path = () => {
        g.beginPath();
        g.moveTo(-12, 0);
        g.lineTo(12, 0);
        g.quadraticCurveTo(16.4, 14, 18.6, 30);
        for (const [x, y] of hem) g.lineTo(x, y);
        g.quadraticCurveTo(-15.2, 14, -12, 0);
        g.closePath();
      };
      // kılıç kını (eteğin arkasında, kalçadan sarkar)
      if (L.scabbard) {
        g.save();
        g.translate(-4, 2);
        g.rotate(0.95);
        g.beginPath();
        g.moveTo(0, -1.6);
        g.quadraticCurveTo(22, -2.4, 36, -7.6);
        g.lineTo(37, -4.6);
        g.quadraticCurveTo(22, 1.6, 0, 1.6);
        g.closePath();
        g.fillStyle = cyl(g, -2, 2, '#3a1a12', 1.5);
        g.fill();
        g.fillStyle = '#c9a24a';
        for (const x of [2, 14, 33]) g.fillRect(x, -2.2, 1.6, 4);
        ink(g, 0.3);
        g.restore();
      }
      path();
      const gr = g.createLinearGradient(-18, 0, 18, 0);
      gr.addColorStop(0, sh(L.coat, 0.45));
      gr.addColorStop(0.55, sh(L.coat, 1.15));
      gr.addColorStop(1, sh(L.coat, 0.6));
      g.fillStyle = gr;
      g.fill();
      grain(g, -18, 0, 37, 32, 160, 17);
      soft(g, -10, 18, 6, 14, DARK, 0.45);
      strokes(g, [[-6, 4, -8.4, 18, -10.6, 30], [0, 4, 0, 18, -0.6, 30], [7.4, 4, 9.6, 18, 11.6, 30]], sh(L.coat, 0.5), 0.55);
      strokes(g, [[3.4, 5, 5, 18, 6, 29]], 'rgba(255,236,214,0.1)', 1.2);
      strokes(g, [[1.6, 2, 2.6, 16, 2.2, 30.6]], sh(L.coat, 0.3), 0.8);
      g.save();
      path();
      g.clip();
      g.fillStyle = vgr(g, 27, 31, sh(L.trim, 1.12), sh(L.trim, 0.62));
      g.fillRect(-20, 27.2, 40, 4.5);
      g.fillStyle = sh(L.trim, 0.5);
      for (let x = -17; x < 18; x += 2.4) g.fillRect(x, 28.6, 1, 1);
      g.restore();
      path();
      ink(g, 0.45);
    });
  }

  function head(L) {
    return part([20, 15, 66, 6], [0, -20], g => {
      // boyun
      g.beginPath();
      g.moveTo(-4.8, 0.6);
      g.lineTo(2.6, 1.6);
      g.lineTo(2.2, 5);
      g.lineTo(-4, 5);
      g.closePath();
      g.fillStyle = cyl(g, -5, 3, sh(L.skin, 0.85));
      g.fill();
      // kafa ve yüz profili
      const face = () => {
        g.beginPath();
        g.moveTo(-5.6, 0.6);
        g.quadraticCurveTo(-10.2, -5, -9.6, -12.6);
        g.quadraticCurveTo(-8, -20.6, 0.6, -20.8);
        g.quadraticCurveTo(7, -20.4, 8.2, -15.6);
        g.quadraticCurveTo(8.9, -14.2, 8.7, -13.4);
        g.lineTo(9.2, -12.6);
        g.quadraticCurveTo(10.4, -10.6, 11.4, -9);
        g.quadraticCurveTo(10.8, -8.2, 9.7, -8.3);
        g.lineTo(9.6, -7.2);
        g.lineTo(8.2, -6.3);
        g.lineTo(9.3, -5.6);
        g.quadraticCurveTo(9.4, -3.4, 8.4, -2.6);
        g.quadraticCurveTo(6, -1.2, 3.4, -0.4);
        g.quadraticCurveTo(2.4, 0.6, 2.4, 1.6);
        g.closePath();
      };
      face();
      g.fillStyle = cyl(g, -10, 11, L.skin, 1.15);
      g.fill();
      grain(g, -10, -21, 22, 22, 90, 41);
      // ışık ve gölge: göz çukuru, elmacık altı, çene altı, alın ışığı
      soft(g, 6.2, -12.2, 2.6, 1.8, DARK, 0.55);
      soft(g, 4.2, -7.6, 3.6, 2, DARK, 0.42);
      soft(g, -2, -6, 5, 5, DARK, 0.4);
      soft(g, 4, -2, 4, 2, DARK, 0.45);
      soft(g, 5.4, -17.2, 3.4, 2, LIGHT, 0.4);
      soft(g, 9.6, -10.6, 1.2, 2, LIGHT, 0.35);
      if (!L.beard) soft(g, 5, -3.6, 4, 2.4, 'rgba(40,30,40,A)', 0.35);
      face();
      ink(g, 0.38);
      // kulak
      g.beginPath();
      g.ellipse(-3.4, -10.4, 1.25, 2, -0.15, 0, PI * 2);
      g.fillStyle = sh(L.skin, 0.85);
      g.fill();
      ink(g, 0.26);
      strokes(g, [[-3.2, -11.6, -2.6, -10.2, -3.4, -9]], 'rgba(60,30,24,0.6)', 0.24);
      // göz: badem göz, üst kapak çizgisi, kaş
      g.fillStyle = '#e8dccc';
      g.beginPath();
      g.moveTo(5.3, -12.1);
      g.quadraticCurveTo(6.6, -12.9, 7.9, -12.2);
      g.quadraticCurveTo(6.6, -11.4, 5.3, -12.1);
      g.fill();
      g.fillStyle = '#20140c';
      g.beginPath();
      g.arc(7.05, -12.15, 0.55, 0, PI * 2);
      g.fill();
      strokes(g, [[5.1, -12.3, 6.6, -13.1, 8.1, -12.4]], '#1a0e0a', 0.42);
      strokes(g, [[3.8, -14.2, 6.4, -15, 9, -13.9]], L.brow || '#1c120c', 1.15);
      strokes(g, [[8.7, -13.2, 9.2, -12.4]], 'rgba(40,20,14,0.5)', 0.3);
      if (L.age) strokes(g, [[2.8, -16.8, 5.6, -17.5], [3.2, -17.9, 6, -18.5], [7.6, -11.5, 8.3, -10.4], [6.4, -8.6, 7.4, -7.2]], 'rgba(70,40,30,0.42)', 0.28);
      if (L.scar) {
        strokes(g, [[4.4, -14.4, 6.2, -10.6, 7.2, -8]], '#7a3a34', 0.6);
        strokes(g, [[4.4, -14.4, 6.2, -10.6, 7.2, -8]], '#d08a7a', 0.25);
      }
      // sakal
      if (L.beard) {
        const beard = () => {
          g.beginPath();
          g.moveTo(-2.6, -13);
          g.quadraticCurveTo(-3.2, -6.4, -1.2, -1.8);
          g.quadraticCurveTo(1.4, 2.6, 6.8, 2.6);
          g.quadraticCurveTo(9.8, 1.2, 9.8, -2.8);
          g.lineTo(9.6, -6.6);
          g.lineTo(7.8, -6.4);
          g.quadraticCurveTo(5.8, -7.6, 4.4, -8.4);
          g.quadraticCurveTo(1.6, -9.4, -0.6, -12.6);
          g.closePath();
        };
        beard();
        g.fillStyle = cyl(g, -3, 10, L.beard, 1.7);
        g.fill();
        const r = K.seeded(21);
        for (let i = 0; i < 46; i++) {
          const x = -2 + r() * 11, y = -11 + r() * 13;
          g.strokeStyle = L.beardGray && r() < 0.3 ? '#b0aaa2' : r() < 0.5 ? sh(L.beard, 1.9) : sh(L.beard, 0.6);
          g.lineWidth = 0.22;
          g.beginPath();
          g.moveTo(x, y);
          g.quadraticCurveTo(x + 0.6, y + 0.8, x + 0.3, y + 1.8);
          g.stroke();
        }
        beard();
        ink(g, 0.3);
      }
      // bıyık
      if (L.mustache) {
        g.beginPath();
        g.moveTo(9.9, -7.6);
        g.quadraticCurveTo(7.4, -8.6, 5, -6.8);
        g.quadraticCurveTo(3.8, -4.6, 4.6, -2.2);
        g.quadraticCurveTo(5.6, -5, 7.8, -6.2);
        g.quadraticCurveTo(9.2, -6.6, 9.9, -6.6);
        g.closePath();
        g.fillStyle = cyl(g, 4, 10, L.mustache, 1.6);
        g.fill();
        ink(g, 0.22);
      }
      // ense saçı
      g.fillStyle = L.beard || L.mustache || '#20140e';
      g.beginPath();
      g.moveTo(-9.4, -13.2);
      g.quadraticCurveTo(-10.4, -9, -7.6, -6);
      g.lineTo(-6.4, -11.4);
      g.closePath();
      g.fill();
      hat(g, L);
    });
  }

  function hat(g, L) {
    switch (L.hat) {
      case 'wrap': {
        // dağınık koyu saç
        g.beginPath();
        g.moveTo(-9.8, -12.6);
        g.quadraticCurveTo(-11.6, -19.4, -5.6, -22.6);
        g.lineTo(-4, -25.4);
        g.lineTo(-2, -22.6);
        g.lineTo(1, -25);
        g.lineTo(2.4, -22);
        g.lineTo(5.6, -23.2);
        g.quadraticCurveTo(8.6, -19, 8.2, -15.6);
        g.lineTo(-9.8, -12.6);
        g.closePath();
        g.fillStyle = cyl(g, -11, 9, '#1c1412', 1.8);
        g.fill();
        strokes(g, [[-6, -16, -4.6, -22], [-2, -16, -1.4, -22.4], [2, -16, 2.6, -21.6], [5, -16.4, 5.8, -21]], 'rgba(120,100,90,0.35)', 0.3);
        ink(g, 0.35);
        // kumaş sargı ve arkaya savrulan uçları
        for (const [ex, ey, w] of [[-22, -9, 2.4], [-19, -3.4, 2]]) {
          g.beginPath();
          g.moveTo(-8.6, -16.4);
          g.quadraticCurveTo((-8.6 + ex) / 2, -16 + (ey + 16) * 0.2 - 2, ex, ey);
          g.lineTo(ex + 1.2, ey + w);
          g.quadraticCurveTo((-8.6 + ex) / 2 + 1, -14 + (ey + 16) * 0.3, -8.4, -13.4);
          g.closePath();
          g.fillStyle = cyl(g, ex, -8, sh(L.hatColor, 0.9), 1.15);
          g.fill();
          ink(g, 0.3);
        }
        g.beginPath();
        g.moveTo(-10, -16.6);
        g.quadraticCurveTo(-1, -19.6, 8.6, -16.2);
        g.lineTo(9, -12.8);
        g.quadraticCurveTo(-1, -15.6, -10, -13);
        g.closePath();
        g.fillStyle = cyl(g, -10, 9, L.hatColor, 1.12);
        g.fill();
        strokes(g, [[-8, -15, -1, -17.6, 7, -15], [-8.6, -13.8, -1, -16, 7.6, -13.6]], sh(L.hatColor, 0.7), 0.35);
        soft(g, -6, -14, 4, 2, DARK, 0.4);
        ink(g, 0.35);
        g.beginPath();
        g.ellipse(-9.4, -15, 1.8, 2.2, 0, 0, PI * 2);
        g.fillStyle = sh(L.hatColor, 0.85);
        g.fill();
        ink(g, 0.3);
        break;
      }
      case 'akinci': {
        // turna tüyü
        g.beginPath();
        g.moveTo(-4, -23);
        g.bezierCurveTo(-8, -32, -14, -40, -24, -46);
        g.strokeStyle = '#f2ede4';
        g.lineWidth = 1.3;
        g.stroke();
        const r = K.seeded(5);
        for (let i = 0; i < 18; i++) {
          const t = i / 18;
          const x = -4 - t * 20 + Math.sin(t * 3) * 1.5, y = -23 - t * 23;
          g.strokeStyle = t > 0.82 ? '#1a1414' : r() < 0.5 ? '#e8e2d8' : '#b8b2a8';
          g.lineWidth = 0.35;
          g.beginPath();
          g.moveTo(x, y);
          g.lineTo(x + 2.2, y - 0.6);
          g.moveTo(x, y);
          g.lineTo(x - 0.8, y + 2);
          g.stroke();
        }
        // kürk börk
        g.beginPath();
        g.moveTo(-10.6, -15);
        g.quadraticCurveTo(-11, -26.5, -0.5, -27.2);
        g.quadraticCurveTo(10.4, -26.5, 10, -15);
        g.closePath();
        g.fillStyle = cyl(g, -11, 10, '#5a3e2c', 1.4);
        g.fill();
        furStrokes(g, -10.4, -27, 20.4, 11.5, '#5a3e2c', 9);
        ink(g, 0.4);
        g.beginPath();
        g.moveTo(-11, -17.4);
        g.quadraticCurveTo(-0.5, -18.6, 10.4, -17.4);
        g.lineTo(10.6, -14.6);
        g.quadraticCurveTo(-0.5, -15.8, -11.2, -14.6);
        g.closePath();
        g.fillStyle = cyl(g, -11, 11, L.sash, 1.3);
        g.fill();
        ink(g, 0.35);
        g.fillStyle = '#d8b060';
        g.beginPath();
        g.arc(7.8, -16.2, 1, 0, PI * 2);
        g.fill();
        break;
      }
      case 'janissary': {
        const hc = L.hatColor;
        // arkaya sarkan yatırtma ve dik keçe gövde
        g.beginPath();
        g.moveTo(-9.6, -16);
        g.lineTo(8.6, -16);
        g.quadraticCurveTo(7, -32, 3.6, -44);
        g.quadraticCurveTo(-2.4, -50.6, -11, -44.6);
        g.quadraticCurveTo(-19.6, -30, -21, -6);
        g.quadraticCurveTo(-18.4, -2.8, -15.2, -4);
        g.quadraticCurveTo(-14.6, -12, -9.6, -16);
        g.closePath();
        g.fillStyle = cyl(g, -21, 9, hc, 1.08);
        g.fill();
        grain(g, -21, -51, 30, 47, 110, 19);
        strokes(g, [[-12, -40, -15, -26, -16.6, -8], [-4, -44, -3, -32, -2, -18], [2.6, -40, 3.6, -30, 5, -18]], sh(hc, 0.72), 0.5);
        ink(g, 0.45);
        // altın bant ve kaşıklık
        g.beginPath();
        g.moveTo(-10.6, -18.6);
        g.quadraticCurveTo(-1, -19.8, 9.6, -18.6);
        g.lineTo(9.8, -14.6);
        g.quadraticCurveTo(-1, -15.6, -10.8, -14.6);
        g.closePath();
        g.fillStyle = vgr(g, -19, -14.6, sh(L.trim, 1.25), sh(L.trim, 0.65));
        g.fill();
        ink(g, 0.35);
        g.beginPath();
        g.moveTo(1, -18.6);
        g.lineTo(5.6, -18.6);
        g.lineTo(5, -33);
        g.quadraticCurveTo(3.3, -35.4, 1.6, -33);
        g.closePath();
        g.fillStyle = cyl(g, 1, 5.6, L.trim, 1.35);
        g.fill();
        ink(g, 0.35);
        g.fillStyle = '#b02a2a';
        g.beginPath();
        g.arc(3.3, -26, 0.9, 0, PI * 2);
        g.fill();
        break;
      }
      case 'aga': {
        // sorguç tüyleri
        for (const [dx, h, c] of [[-5, -66, '#e8e2d8'], [0, -72, '#f6f2ea'], [5, -64, '#d8d2c8']]) {
          g.beginPath();
          g.moveTo(0, -30);
          g.bezierCurveTo(dx * 1.2, -42, dx * 3, h + 12, dx * 3.4, h);
          g.strokeStyle = c;
          g.lineWidth = 2.2;
          g.stroke();
          g.strokeStyle = sh(c, 0.75);
          g.lineWidth = 0.4;
          g.stroke();
        }
        g.beginPath();
        g.moveTo(-14.6, -15);
        g.quadraticCurveTo(-16.6, -34, -1, -36);
        g.quadraticCurveTo(15, -34, 13.4, -15);
        g.closePath();
        g.fillStyle = cyl(g, -16, 14, L.hatColor, 1.1);
        g.fill();
        strokes(g, [[-11, -16, -8, -26, -4, -34], [-4, -16, -2, -26, 2, -35], [3, -16, 5, -26, 8, -33]], sh(L.hatColor, 0.75), 0.6);
        ink(g, 0.45);
        g.fillStyle = vgr(g, -20, -14, sh(L.trim, 1.25), sh(L.trim, 0.6));
        g.fillRect(-15, -19.4, 28.6, 4.6);
        g.fillStyle = '#c0182a';
        g.beginPath();
        g.ellipse(0, -27, 2.2, 3, 0, 0, PI * 2);
        g.fill();
        g.strokeStyle = sh(L.trim, 1.2);
        g.lineWidth = 0.6;
        g.stroke();
        break;
      }
      case 'kavuk': {
        g.beginPath();
        g.moveTo(-10, -16);
        g.quadraticCurveTo(-10, -30, -0.5, -31);
        g.quadraticCurveTo(9, -30, 9, -16);
        g.closePath();
        g.fillStyle = cyl(g, -10, 9, L.hatColor, 1.3);
        g.fill();
        ink(g, 0.4);
        g.beginPath();
        g.ellipse(-0.5, -16.8, 11.6, 3.4, 0, 0, PI * 2);
        g.fillStyle = cyl(g, -12, 11, '#ece4d4', 1.08);
        g.fill();
        strokes(g, [[-10, -17, -0.5, -15, 10, -17.6]], '#a89c88', 0.4);
        ink(g, 0.4);
        break;
      }
    }
  }

  function cape(L) {
    const len = L.capeLen || 62;
    return part([22, 12, 4, len + 6], [0, len], g => {
      const path = () => {
        g.beginPath();
        g.moveTo(-3, -2);
        g.lineTo(8, -1);
        g.quadraticCurveTo(10, len * 0.5, 8, len);
        for (let i = 0; i < 6; i++) g.lineTo(8 - i * 4.5 - 2.2, len + (i % 2 ? -3.4 : 1.6));
        g.lineTo(-18, len - 2);
        g.quadraticCurveTo(-14, len * 0.45, -3, -2);
        g.closePath();
      };
      path();
      g.fillStyle = cyl(g, -18, 9, L.cape, 1.35);
      g.fill();
      grain(g, -20, -2, 30, len + 4, 160, 23);
      strokes(g, [[-2, 6, -6, len * 0.5, -10, len - 4], [3, 6, 2, len * 0.5, 0, len - 2], [-8, len * 0.3, -12, len * 0.6, -15, len - 3]], sh(L.cape, 0.55), 0.7);
      strokes(g, [[1, 8, 0.4, len * 0.5, -2, len - 4]], 'rgba(255,200,180,0.12)', 1.2);
      g.beginPath();
      g.moveTo(-3, -2);
      g.quadraticCurveTo(-14, len * 0.45, -18, len - 2);
      g.strokeStyle = L.capeLining;
      g.lineWidth = 1.4;
      g.stroke();
      path();
      ink(g, 0.5);
      if (L.capeFur) {
        g.beginPath();
        g.ellipse(1, 0, 12, 4.6, 0, 0, PI * 2);
        g.fillStyle = cyl(g, -11, 13, L.capeFur, 1.3);
        g.fill();
        furStrokes(g, -10, -4, 22, 8, L.capeFur, 29);
      }
    });
  }

  function shield() {
    return part([19, 19, 19, 19], [0, 17], g => {
      g.beginPath();
      g.arc(0, 0, 17, 0, PI * 2);
      const gr = g.createRadialGradient(4, -5, 2, 0, 0, 17);
      gr.addColorStop(0, '#9a6a3a');
      gr.addColorStop(1, '#4a2e18');
      g.fillStyle = gr;
      g.fill();
      for (let i = 0; i < 16; i++) {
        const a = (i / 16) * PI * 2;
        strokes(g, [[Math.cos(a) * 6, Math.sin(a) * 6, Math.cos(a) * 15, Math.sin(a) * 15]], 'rgba(30,16,8,0.4)', 0.35);
      }
      g.beginPath();
      g.arc(0, 0, 16.4, 0, PI * 2);
      g.strokeStyle = '#c9a24a';
      g.lineWidth = 1.6;
      g.stroke();
      g.fillStyle = '#e0c070';
      for (let i = 0; i < 12; i++) {
        const a = (i / 12) * PI * 2;
        g.beginPath();
        g.arc(Math.cos(a) * 13.6, Math.sin(a) * 13.6, 0.7, 0, PI * 2);
        g.fill();
      }
      g.beginPath();
      g.arc(0, 0, 4.6, 0, PI * 2);
      const bg = g.createRadialGradient(1.5, -1.5, 0.5, 0, 0, 4.6);
      bg.addColorStop(0, '#fff0c0');
      bg.addColorStop(1, '#8a6a2a');
      g.fillStyle = bg;
      g.fill();
      g.beginPath();
      g.arc(0, 0, 17, 0, PI * 2);
      ink(g, 0.5);
    });
  }

  // ---------- Silahlar: kabza (0,0), uç (L,0) ----------
  function steel(g, y0, y1) {
    const gr = g.createLinearGradient(0, y0, 0, y1);
    gr.addColorStop(0, '#f6f8fa');
    gr.addColorStop(0.35, '#c8d0d8');
    gr.addColorStop(0.7, '#8a96a2');
    gr.addColorStop(1, '#e4eaee');
    return gr;
  }
  const WEAPONS = {
    kilic: () => part([16, 72, 14, 8], [68, 0], g => {
      g.fillStyle = cyl(g, -4, 4, '#3a2418');
      g.save();
      g.rotate(PI / 2);
      g.fillRect(-2.4, -3, 4.8, 13.6);
      g.restore();
      g.fillStyle = '#3a2418';
      g.fillRect(-11, -2.4, 14, 4.8);
      strokes(g, [[-10, -2.4, -8, 2.4], [-7, -2.4, -5, 2.4], [-4, -2.4, -2, 2.4], [-1, -2.4, 1, 2.4]], '#7a5a38', 0.5);
      g.beginPath();
      g.ellipse(-12.4, 0, 2.6, 2.2, 0, 0, PI * 2);
      g.fillStyle = cyl(g, -15, -10, '#c9a24a', 1.4);
      g.fill();
      ink(g, 0.35);
      g.beginPath();
      g.roundRect(3, -6.6, 3, 13.2, 1.4);
      g.fillStyle = cyl(g, 3, 6, '#d8b060', 1.45);
      g.fill();
      ink(g, 0.35);
      // kavisli ağız, uca doğru yelman
      const blade = () => {
        g.beginPath();
        g.moveTo(6, -2.2);
        g.quadraticCurveTo(38, -3.4, 58, -8.6);
        g.lineTo(69, -12.4);
        g.quadraticCurveTo(64, -6.6, 56, -3.6);
        g.quadraticCurveTo(36, 3.8, 6, 2.3);
        g.closePath();
      };
      blade();
      g.fillStyle = steel(g, -10, 3);
      g.fill();
      strokes(g, [[7, -0.6, 36, -1.2, 56, -6]], 'rgba(60,70,84,0.55)', 0.6);
      strokes(g, [[7, 1.9, 36, 3, 55, -3.4]], 'rgba(255,255,255,0.85)', 0.4);
      blade();
      ink(g, 0.35, '#2a3038');
    }),
    yatagan: () => part([16, 56, 10, 8], [52, 0], g => {
      g.beginPath();
      g.moveTo(-4, -2.6);
      g.lineTo(-12, -6.6);
      g.quadraticCurveTo(-14.6, -0.4, -12.6, 5.6);
      g.lineTo(-4, 2.6);
      g.closePath();
      g.fillStyle = cyl(g, -14, -4, '#e8dcc0', 1.1);
      g.fill();
      ink(g, 0.35);
      g.fillStyle = cyl(g, -4, 2, '#c9a24a', 1.4);
      g.fillRect(-4, -3, 6, 6);
      const blade = () => {
        g.beginPath();
        g.moveTo(2, -2.4);
        g.quadraticCurveTo(28, -9.6, 52, 0.6);
        g.quadraticCurveTo(28, 0.4, 2, 2.8);
        g.closePath();
      };
      blade();
      g.fillStyle = steel(g, -7, 3);
      g.fill();
      strokes(g, [[4, 0.4, 26, -3.4, 46, 0]], 'rgba(60,70,84,0.5)', 0.5);
      blade();
      ink(g, 0.35, '#2a3038');
    }),
    mizrak: () => part([66, 128, 8, 14], [124, 0], g => {
      const wg = g.createLinearGradient(0, -2, 0, 2);
      wg.addColorStop(0, '#9a7448');
      wg.addColorStop(1, '#4a3020');
      g.fillStyle = wg;
      g.fillRect(-62, -1.9, 160, 3.8);
      strokes(g, [[-62, -1.9, 98, -1.9]], 'rgba(255,230,190,0.3)', 0.4);
      for (const x of [-30, 20, 90]) {
        g.fillStyle = cyl(g, x, x + 3, '#c9a24a', 1.4);
        g.fillRect(x, -2.4, 2.4, 4.8);
      }
      const r = K.seeded(3);
      for (let i = 0; i < 14; i++) strokes(g, [[93, 0, 90 - r() * 4, 2 + r() * 2, 86 + r() * 4, 9 + r() * 4]], i % 3 ? '#a8262b' : '#d0403a', 0.6);
      g.beginPath();
      g.moveTo(96, -2.6);
      g.quadraticCurveTo(108, -6.4, 125, 0);
      g.quadraticCurveTo(108, 6.4, 96, 2.6);
      g.closePath();
      g.fillStyle = steel(g, -6, 6);
      g.fill();
      strokes(g, [[97, 0, 123, 0]], 'rgba(60,70,84,0.6)', 0.5);
      ink(g, 0.35, '#2a3038');
    }),
    tufek: () => part([46, 90, 8, 12], [86, 0], g => {
      g.beginPath();
      g.moveTo(-44, -2.4);
      g.lineTo(-10, -3.4);
      g.lineTo(-8, 3.6);
      g.lineTo(-44, 8.6);
      g.closePath();
      g.fillStyle = vgr(g, -3, 8, '#8a5a34', '#3a2214');
      g.fill();
      ink(g, 0.4);
      g.fillStyle = '#d8b060';
      for (const x of [-38, -30, -22]) g.fillRect(x, 0, 1.6, 1.6);
      g.fillStyle = vgr(g, -3, 1.5, '#7a7e86', '#2a2c32');
      g.fillRect(-12, -3, 98, 3.6);
      g.fillStyle = cyl(g, -15, -8, '#c9a24a', 1.4);
      g.fillRect(-15, -4.4, 7, 7.4);
      g.fillStyle = '#2a2c32';
      g.fillRect(-14, -7, 2.4, 3);
      for (const x of [16, 44, 70]) {
        g.fillStyle = '#c9a24a';
        g.fillRect(x, -3.4, 1.6, 4.4);
      }
    }),
    tabanca: () => part([12, 32, 8, 14], [28, 0], g => {
      g.beginPath();
      g.moveTo(-4, -3);
      g.lineTo(5, -3);
      g.quadraticCurveTo(-1, 6, -4, 12.4);
      g.lineTo(-10.6, 11.4);
      g.quadraticCurveTo(-6, 4, -4, -3);
      g.closePath();
      g.fillStyle = cyl(g, -11, 5, '#7a4a2a', 1.3);
      g.fill();
      ink(g, 0.35);
      g.fillStyle = '#d8b060';
      g.beginPath();
      g.arc(-8.6, 11.4, 2.2, 0, PI * 2);
      g.fill();
      g.fillStyle = vgr(g, -5.4, -1.4, '#8a8e96', '#2a2c32');
      g.fillRect(-1, -5.4, 29, 4.2);
      g.fillStyle = '#c9a24a';
      g.fillRect(-2, -6.2, 6, 6);
      g.fillRect(26, -5.8, 2, 5);
      g.fillStyle = '#2a2c32';
      g.fillRect(-3.4, -8.4, 2.2, 3);
    }),
    cirit: () => part([36, 80, 6, 6], [76, 0], g => {
      g.fillStyle = vgr(g, -1.6, 1.6, '#b08a5a', '#6a4a2a');
      g.fillRect(-34, -1.5, 96, 3);
      for (const x of [-20, 30]) {
        g.fillStyle = '#a8262b';
        g.fillRect(x, -1.8, 5, 3.6);
      }
      g.beginPath();
      g.moveTo(60, -3.4);
      g.lineTo(77, 0);
      g.lineTo(60, 3.4);
      g.closePath();
      g.fillStyle = steel(g, -3.4, 3.4);
      g.fill();
      ink(g, 0.3, '#2a3038');
    }),
    topuz: () => part([12, 74, 16, 16], [69, 0], g => {
      g.fillStyle = vgr(g, -2.4, 2.4, '#8a6040', '#3a2414');
      g.fillRect(-9, -2.4, 52, 4.8);
      g.fillStyle = '#c9a24a';
      g.fillRect(36, -3, 4, 6);
      for (let i = 0; i < 8; i++) {
        const a = (i / 8) * PI * 2;
        g.beginPath();
        g.moveTo(56 + Math.cos(a - 0.3) * 6, Math.sin(a - 0.3) * 6);
        g.lineTo(56 + Math.cos(a) * 14, Math.sin(a) * 14);
        g.lineTo(56 + Math.cos(a + 0.3) * 6, Math.sin(a + 0.3) * 6);
        g.closePath();
        g.fillStyle = steel(g, -14, 14);
        g.fill();
        ink(g, 0.3, '#2a3038');
      }
      g.beginPath();
      g.arc(56, 0, 8, 0, PI * 2);
      const bg = g.createRadialGradient(59, -3, 1, 56, 0, 8);
      bg.addColorStop(0, '#e8eef2');
      bg.addColorStop(1, '#4a5260');
      g.fillStyle = bg;
      g.fill();
      ink(g, 0.35, '#2a3038');
    }),
  };

  // ---------- Bütün karakterleri hazırla ----------
  K.paint = {
    S,
    bakeAll() {
      const weapons = {};
      for (const k in WEAPONS) weapons[k] = WEAPONS[k]();
      const shieldPart = shield();
      for (const name in K.LOOKS) {
        if (K.assets.hasUser(name)) continue;
        const L = K.LOOKS[name];
        const ua = upperArm(L), fa = foreArm(L), th = thigh(L), sn = shin(L);
        const parts = {
          torso: torso(L),
          head: head(L),
          skirt: skirt(L),
          upperArmF: ua,
          foreArmF: fa,
          upperArmB: darkened(ua, 0.38),
          foreArmB: darkened(fa, 0.38),
          thighF: th,
          shinF: sn,
          thighB: darkened(th, 0.38),
          shinB: darkened(sn, 0.38),
        };
        if (L.cape) parts.cape = cape(L);
        if (L.shield) parts.shield = shieldPart;
        K.assets.setChar(name, parts, weapons);
      }
    },
  };
})(window.K);
