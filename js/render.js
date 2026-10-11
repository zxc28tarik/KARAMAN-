'use strict';
// Karakterleri tam çözünürlükte çizer: koyu dış hat, ışık tarafında kenar ışığı, karşı tarafta gölge.
// Hepsi tuval birleştirme işlemleriyle (ekran kartında) yapılır; resim parçalı karakterlerde de aynen çalışır.
(function (K) {
  const W = 560, H = 400, FX = 280, FY = 350; // çalışma tuvali ve ayak noktası
  function mk() {
    const c = document.createElement('canvas');
    c.width = W;
    c.height = H;
    return { c, g: c.getContext('2d') };
  }
  const body = mk(), work = mk(), out = mk();
  // Karakterlere işlenen ince boya dokusu (bir kez üretilir)
  const NOISE = (() => {
    const n = mk();
    const g = n.g;
    const r = K.seeded(808);
    for (let i = 0; i < 9000; i++) {
      g.fillStyle = r() < 0.6 ? `rgba(0,0,0,${0.08 + r() * 0.18})` : `rgba(255,230,200,${0.04 + r() * 0.08})`;
      const x = r() * W, y = r() * H, a = -0.8 + r() * 0.4;
      g.save();
      g.translate(x, y);
      g.rotate(a);
      g.fillRect(0, 0, 1 + r() * 3, 0.8 + r() * 0.8);
      g.restore();
    }
    return n.c;
  })();
  body.g.imageSmoothingQuality = 'high';

  function silhouette(dst, src, col, dx = 0, dy = 0) {
    dst.g.globalCompositeOperation = 'source-over';
    dst.g.clearRect(0, 0, W, H);
    dst.g.drawImage(src.c, dx, dy);
    dst.g.globalCompositeOperation = 'source-in';
    dst.g.fillStyle = col;
    dst.g.fillRect(0, 0, W, H);
    dst.g.globalCompositeOperation = 'source-over';
  }
  // Işık yönündeki kenar şeridi: gövde, gövdenin ışığın tersine kaydırılmış halinden çıkarılır
  function edgeBand(dst, src, col, dir, width, lift) {
    silhouette(dst, src, col);
    dst.g.globalCompositeOperation = 'destination-out';
    dst.g.drawImage(src.c, -dir * width, lift);
    dst.g.globalCompositeOperation = 'source-over';
  }

  K.chr = {
    // drawFn(ctx) dünya koordinatıyla çizer; o: flash, alpha, rim, rimCol, rimDir, tint, outline
    draw(ctx, x, y, drawFn, o = {}) {
      const bx = Math.round(x), by = Math.round(y);
      const b = body.g;
      b.setTransform(1, 0, 0, 1, 0, 0);
      b.clearRect(0, 0, W, H);
      b.setTransform(1, 0, 0, 1, FX - bx, FY - by);
      drawFn(b);
      b.setTransform(1, 0, 0, 1, 0, 0);
      // vurulma anı: beyaz parlama (resim parçalı karakterlerde de çalışır)
      if (o.flash) {
        b.globalCompositeOperation = 'source-atop';
        b.fillStyle = 'rgba(255,246,236,0.85)';
        b.fillRect(0, 0, W, H);
        b.globalCompositeOperation = 'source-over';
      }
      // karanlık atmosfere uyum: hafif soğuk ton
      if (o.tint) {
        b.globalCompositeOperation = 'source-atop';
        b.globalAlpha = o.tint;
        b.fillStyle = '#140c22';
        b.fillRect(0, 0, W, H);
        b.globalAlpha = 1;
        b.globalCompositeOperation = 'source-over';
      }
      const og = out.g;
      og.globalCompositeOperation = 'source-over';
      og.clearRect(0, 0, W, H);
      // alt kısım karanlığa gömülür
      b.globalCompositeOperation = 'source-atop';
      const vg = b.createLinearGradient(0, FY - 150, 0, FY + 4);
      vg.addColorStop(0, 'rgba(8,4,10,0)');
      vg.addColorStop(1, `rgba(8,4,10,${o.flash ? 0 : 0.5})`);
      b.fillStyle = vg;
      b.fillRect(0, 0, W, H);
      // boya dokusu
      b.globalAlpha = 0.5;
      b.drawImage(NOISE, 0, 0);
      b.globalAlpha = 1;
      b.globalCompositeOperation = 'source-over';
      // ince dış hat
      silhouette(work, body, o.outline || 'rgba(8,4,6,0.9)');
      for (const [dx, dy] of [[-1.6, 0], [1.6, 0], [0, -1.6], [0, 1.6]]) og.drawImage(work.c, dx, dy);
      og.drawImage(body.c, 0, 0);
      if (!o.flash) {
        // gölge tarafı
        const dir = o.rimDir || 1;
        edgeBand(work, body, '#0a0614', -dir, 6, 0);
        og.globalAlpha = 0.45;
        og.drawImage(work.c, 0, 0);
        // kenar ışığı
        edgeBand(work, body, o.rimCol || '#ff8a5a', dir, 3.5, 2);
        og.globalAlpha = o.rim ?? 0.6;
        og.globalCompositeOperation = 'lighter';
        og.drawImage(work.c, 0, 0);
        // üstten soğuk ay ışığı
        silhouette(work, body, '#7a86b8');
        work.g.globalCompositeOperation = 'destination-out';
        work.g.drawImage(body.c, 0, 3);
        work.g.globalCompositeOperation = 'source-over';
        og.globalAlpha = 0.32;
        og.drawImage(work.c, 0, 0);
        og.globalCompositeOperation = 'source-over';
        og.globalAlpha = 1;
      }
      ctx.save();
      ctx.globalAlpha = o.alpha ?? 1;
      ctx.drawImage(out.c, bx - FX, by - FY);
      ctx.restore();
    },
    shadow(ctx, x, w, alpha = 0.45) {
      const g = ctx.createRadialGradient(x, K.GROUND + 2, 2, x, K.GROUND + 2, w);
      g.addColorStop(0, `rgba(4,2,8,${alpha})`);
      g.addColorStop(1, 'rgba(4,2,8,0)');
      ctx.fillStyle = g;
      ctx.save();
      ctx.translate(x, K.GROUND + 2);
      ctx.scale(1, 0.22);
      ctx.translate(-x, -(K.GROUND + 2));
      ctx.beginPath();
      ctx.arc(x, K.GROUND + 2, w, 0, Math.PI * 2);
      ctx.fill();
      ctx.restore();
    },
  };
})(window.K);
