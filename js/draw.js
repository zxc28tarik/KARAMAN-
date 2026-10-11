'use strict';
// Çizim: karakter iskeleti, silahlar, arka plan
(function (K) {
  // ---------- Renk yardımcıları ----------
  function shade(hex, f) {
    const n = parseInt(hex.slice(1), 16);
    let r = (n >> 16) & 255, g = (n >> 8) & 255, b = n & 255;
    r = Math.round(K.clamp(r * f, 0, 255));
    g = Math.round(K.clamp(g * f, 0, 255));
    b = Math.round(K.clamp(b * f, 0, 255));
    return '#' + ((1 << 24) | (r << 16) | (g << 8) | b).toString(16).slice(1);
  }
  K.shade = shade;

  // Görünüm tanımları; eksik koyu tonlar otomatik üretilir
  K.LOOKS = {};
  K.defineLook = function (name, o) {
    const L = Object.assign({ scale: 1, skin: '#b88462', beard: null, mustache: '#2b1f17', hat: 'janissary', coat: '#2f4f8f', trim: '#d9c08a', sash: '#b23a2a', pants: '#7a2a2a', boots: '#3b2a1e', mail: false, hatColor: '#efe9dc' }, o);
    for (const k of ['skin', 'coat', 'pants', 'boots']) L[k + 'Dark'] = L[k + 'Dark'] || shade(L[k], 0.72);
    L.partsName = name;
    K.LOOKS[name] = L;
    return L;
  };

  // İskelet ölçüleri (birim): uyluk, baldır, gövde, üst kol, alt kol
  K.RIG = { TH: 30, SN: 31, TR: 44, UA: 22, FA: 21 };

  // ---------- Pozlar ----------
  const DEF = { lean: 0.05, bob: 0, ft: 0.2, fs: 0.05, bt: -0.2, bs: -0.28, au: 0.35, af: 1.0, bu: -0.15, bf: 0.25, wa: -0.7, wb: -0.7, head: 0, rot: 0, draw: 0 };
  K.pose = o => Object.assign({}, DEF, o);
  K.lerpPose = function (a, b, t) {
    const o = {};
    for (const k in DEF) o[k] = K.lerp(a[k] ?? DEF[k], b[k] ?? DEF[k], t);
    return o;
  };
  K.runPose = function (phase, base) {
    const s = Math.sin(phase);
    const p = Object.assign({}, base);
    p.ft = s * 0.85;
    p.fs = p.ft - 0.15 - 0.85 * Math.max(0, -Math.cos(phase));
    p.bt = -s * 0.85;
    p.bs = p.bt - 0.15 - 0.85 * Math.max(0, Math.cos(phase));
    p.bob = -Math.abs(Math.cos(phase)) * 5;
    p.lean = (base.lean || 0) + 0.18;
    p.bu = -s * 0.7;
    p.bf = p.bu + 0.7;
    return p;
  };

  // ---------- İskelet ----------
  function limbPts(x, y, a1, a2, l1, l2) {
    const kx = x + Math.sin(a1) * l1, ky = y + Math.cos(a1) * l1;
    return [kx, ky, kx + Math.sin(a2) * l2, ky + Math.cos(a2) * l2];
  }
  function ik(sx, sy, tx, ty, l1, l2, bend) {
    const dx = tx - sx, dy = ty - sy;
    let d = Math.hypot(dx, dy);
    d = K.clamp(d, Math.abs(l1 - l2) + 0.5, l1 + l2 - 0.5);
    const a = Math.atan2(dy, dx);
    const b = Math.acos(K.clamp((l1 * l1 + d * d - l2 * l2) / (2 * l1 * d), -1, 1));
    const ea = a + bend * b;
    return [sx + Math.cos(ea) * l1, sy + Math.sin(ea) * l1, sx + Math.cos(a) * d, sy + Math.sin(a) * d];
  }
  function stroke2(ctx, x, y, kx, ky, fx, fy, w, color) {
    ctx.strokeStyle = color;
    ctx.lineWidth = w;
    ctx.beginPath();
    ctx.moveTo(x, y);
    ctx.lineTo(kx, ky);
    ctx.lineTo(fx, fy);
    ctx.stroke();
  }
  const TWO_HAND = { mizrak: 1, tufek: 1, bayrak: 1 };

  // x,y = ayak noktası (dünya)
  K.drawRig = function (ctx, x, y, facing, pose, look, weapon, flash, extra) {
    const c = k => (flash ? '#fff4e8' : look[k]);
    extra = extra || {};
    ctx.save();
    ctx.translate(x, y);
    ctx.scale(facing * look.scale, look.scale);
    if (pose.rot) {
      ctx.translate(0, -26);
      ctx.rotate(pose.rot);
      ctx.translate(0, 26);
    }
    ctx.lineCap = 'round';
    ctx.lineJoin = 'round';
    const { TH, SN, TR, UA, FA } = K.RIG;
    const hipY = -(TH + SN) + (pose.bob || 0);
    const lean = pose.lean || 0;
    const shX = Math.sin(lean) * TR, shY = hipY - Math.cos(lean) * TR;

    // Ön kol ve silah noktaları önceden hesaplanır (iki elli silahlar için)
    const fa = limbPts(shX + 3, shY + 4, pose.au, pose.af, UA, FA);
    const hx = fa[2], hy = fa[3];
    const wdx = Math.cos(pose.wa), wdy = Math.sin(pose.wa);
    let back;
    if (weapon && TWO_HAND[weapon]) {
      back = ik(shX - 3, shY + 4, hx - wdx * 30, hy - wdy * 30, UA, FA, 1);
    } else if (weapon === 'yay') {
      const pull = 8 + 42 * (pose.draw || 0);
      back = ik(shX - 3, shY + 4, hx - wdx * pull, hy - wdy * pull, UA, FA, 1);
    } else {
      back = limbPts(shX - 3, shY + 4, pose.bu, pose.bf, UA, FA);
    }

    // Resim parçalı karakter varsa onu çiz
    const parts = K.assets && K.assets.charParts(look.partsName);
    if (parts) {
      const bl = limbPts(-4, hipY, pose.bt, pose.bs, TH, SN), fl = limbPts(4, hipY, pose.ft, pose.fs, TH, SN);
      drawParts(ctx, parts, { hipY, shX, shY, lean, fa, hx, hy, back, bl, fl }, pose, weapon, extra, look);
      ctx.restore();
      return;
    }

    // Pelerin: hıza göre arkaya savrulur, ucu dalgalanır
    if (look.cape && extra.cape) {
      const v = extra.cape.v || 0, t = extra.cape.t || 0;
      const wave = Math.sin(t * (6 + v * 6)) * (2 + v * 5);
      const len = look.capeLen || 62;
      const bx = shX - 24 - v * 34 + wave * 0.6, by = shY + len - v * 22 + wave;
      ctx.fillStyle = c('cape');
      ctx.beginPath();
      ctx.moveTo(shX + 4, shY + 1);
      ctx.lineTo(shX - 10, shY);
      ctx.quadraticCurveTo(shX - 22 - v * 18, shY + len * 0.45 + wave * 0.5, bx, by);
      ctx.lineTo(bx + 9, by + 4 - wave * 0.4);
      ctx.lineTo(bx + 17, by + 1);
      ctx.quadraticCurveTo(-6, hipY + 18, 2, hipY + 6);
      ctx.closePath();
      ctx.fill();
      ctx.strokeStyle = c('capeLining');
      ctx.lineWidth = 3;
      ctx.beginPath();
      ctx.moveTo(shX - 10, shY);
      ctx.quadraticCurveTo(shX - 22 - v * 18, shY + len * 0.45 + wave * 0.5, bx, by);
      ctx.stroke();
      if (look.capeFur) {
        ctx.fillStyle = c('capeFur');
        ctx.beginPath();
        ctx.ellipse(shX - 3, shY + 3, 15, 7, 0, 0, 7);
        ctx.fill();
      }
    }

    // Arka kol
    stroke2(ctx, shX - 3, shY + 4, back[0], back[1], back[2], back[3], 9.5, c('coatDark'));
    if (extra.offhand) K.drawWeapon(ctx, extra.offhand, back[2], back[3], pose.wb, flash);
    ctx.fillStyle = c('skinDark');
    ctx.beginPath();
    ctx.arc(back[2], back[3], 4, 0, 7);
    ctx.fill();

    // Bacaklar
    const bl = limbPts(-4, hipY, pose.bt, pose.bs, TH, SN);
    stroke2(ctx, -4, hipY, bl[0], bl[1], bl[2], bl[3], 11.5, c('pantsDark'));
    ctx.fillStyle = c('bootsDark');
    ctx.fillRect(bl[2] - 4, bl[3] - 5, 14, 8);
    const fl = limbPts(4, hipY, pose.ft, pose.fs, TH, SN);
    stroke2(ctx, 4, hipY, fl[0], fl[1], fl[2], fl[3], 11.5, c('pants'));
    ctx.fillStyle = c('boots');
    ctx.fillRect(fl[2] - 4, fl[3] - 5, 14, 8);

    // Kaftan eteği
    const hemY = hipY + 30;
    const frontX = Math.max(fl[0], bl[0], 12) + 6;
    const backX = Math.min(fl[0], bl[0], -12) - 6;
    ctx.fillStyle = c('coat');
    ctx.beginPath();
    ctx.moveTo(-12, hipY - 8);
    ctx.lineTo(12, hipY - 8);
    ctx.lineTo(frontX, hemY);
    ctx.quadraticCurveTo((frontX + backX) / 2, hemY + 5, backX, hemY);
    ctx.closePath();
    ctx.fill();
    ctx.strokeStyle = c('trim');
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.moveTo(frontX, hemY);
    ctx.quadraticCurveTo((frontX + backX) / 2, hemY + 5, backX, hemY);
    ctx.stroke();

    // Gövde
    ctx.fillStyle = c('coat');
    ctx.beginPath();
    ctx.moveTo(-12, hipY - 4);
    ctx.lineTo(12, hipY - 4);
    ctx.lineTo(shX + 16, shY + 2);
    ctx.quadraticCurveTo(shX, shY - 5, shX - 15, shY + 2);
    ctx.closePath();
    ctx.fill();
    if (look.mail) {
      ctx.fillStyle = flash ? '#fff' : '#8d949b';
      ctx.beginPath();
      ctx.moveTo(shX + 2, shY + 2);
      ctx.lineTo(shX + 11, shY + 4);
      ctx.lineTo(9, hipY - 8);
      ctx.lineTo(2, hipY - 8);
      ctx.closePath();
      ctx.fill();
      if (!flash) {
        ctx.strokeStyle = '#6b7178';
        ctx.lineWidth = 1;
        for (let i = 1; i < 5; i++) {
          const t = i / 5;
          ctx.beginPath();
          ctx.moveTo(K.lerp(shX + 2, 2, t), K.lerp(shY + 3, hipY - 8, t));
          ctx.lineTo(K.lerp(shX + 11, 9, t), K.lerp(shY + 4, hipY - 8, t));
          ctx.stroke();
        }
      }
    }
    ctx.strokeStyle = c('trim');
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.moveTo(shX + 2, shY + 2);
    ctx.lineTo(2, hipY - 6);
    ctx.stroke();
    // Kuşak
    const wx = K.lerp(0, shX, 0.12), wy = K.lerp(hipY, shY, 0.12);
    ctx.fillStyle = c('sash');
    ctx.save();
    ctx.translate(wx, wy);
    ctx.rotate(lean);
    ctx.fillRect(-13, -5, 26, 9);
    ctx.restore();
    if (look.shield && !flash) {
      // Kalkan arka koldadır, gövdenin önünde durur
    }

    // Baş
    const headX = shX + Math.sin(lean) * 12, headY = shY - 12;
    ctx.save();
    ctx.translate(headX, headY);
    ctx.rotate(pose.head || 0);
    ctx.fillStyle = c('skin');
    ctx.fillRect(-4, 4, 8, 7);
    ctx.beginPath();
    ctx.arc(0, 0, 10, 0, 7);
    ctx.fill();
    if (look.beard) {
      ctx.fillStyle = c('beard');
      ctx.beginPath();
      ctx.moveTo(-5, 3);
      ctx.quadraticCurveTo(-2, 16, 8, 13);
      ctx.quadraticCurveTo(11, 6, 10, 2);
      ctx.closePath();
      ctx.fill();
      if (look.beardGray && !flash) {
        ctx.strokeStyle = '#9a948c';
        ctx.lineWidth = 1;
        ctx.beginPath();
        ctx.moveTo(0, 8);
        ctx.lineTo(4, 12);
        ctx.stroke();
      }
    }
    if (look.mustache) {
      ctx.strokeStyle = c('mustache');
      ctx.lineWidth = 2.5;
      ctx.beginPath();
      ctx.moveTo(10, 3);
      ctx.quadraticCurveTo(6, 6, 2, 7);
      ctx.stroke();
    }
    if (extra.eyeGlow) {
      ctx.fillStyle = `rgba(255,${60 - extra.eyeGlow * 30},40,${0.6 + extra.eyeGlow * 0.4})`;
      ctx.fillRect(3.5, -4.5, 6, 4);
    } else {
      ctx.fillStyle = flash ? '#fff' : '#1a120d';
      ctx.fillRect(5, -3, 2.5, 2.5);
    }
    ctx.fillRect(3, -6, 6, 1.5);
    drawHat(ctx, look, flash);
    ctx.restore();

    // Kalkan (ön tarafta)
    if (look.shield) {
      const sx = K.lerp(fa[0], hx, 0.5), sy = K.lerp(fa[1], hy, 0.5);
      ctx.fillStyle = flash ? '#fff' : '#7a5a32';
      ctx.beginPath();
      ctx.arc(sx + 4, sy, 17, 0, 7);
      ctx.fill();
      ctx.strokeStyle = flash ? '#fff' : '#c9a24a';
      ctx.lineWidth = 3;
      ctx.stroke();
      ctx.fillStyle = flash ? '#fff' : '#c9a24a';
      ctx.beginPath();
      ctx.arc(sx + 4, sy, 4, 0, 7);
      ctx.fill();
    }

    // Ön kol + silah
    if (weapon) K.drawWeapon(ctx, weapon, hx, hy, pose.wa, flash, pose);
    stroke2(ctx, shX + 3, shY + 4, fa[0], fa[1], hx, hy, 9.5, c('coat'));
    ctx.fillStyle = c('skin');
    ctx.beginPath();
    ctx.arc(hx, hy, 4.2, 0, 7);
    ctx.fill();
    if (extra.glint) {
      const gx = hx + wdx * (extra.glintLen || 55), gy = hy + wdy * (extra.glintLen || 55);
      ctx.fillStyle = 'rgba(255,240,180,' + extra.glint + ')';
      ctx.beginPath();
      ctx.arc(gx, gy, 9 * extra.glint + 3, 0, 7);
      ctx.fill();
    }
    ctx.restore();
  };

  // Silah ucunun eldeki kabzaya uzaklığı (iskelet birimi)
  const TIP = { kilic: 68, yatagan: 52, mizrak: 124, bayrak: 124, tufek: 86, tabanca: 28, cirit: 76, topuz: 69 };
  function drawParts(ctx, C, g, pose, weapon, extra, look) {
    const A = K.assets, P = C.parts;
    const d = (k, ax, ay, bx, by) => P[k] && A.drawPart(ctx, P[k], ax, ay, bx, by);
    const wpn = (kind, hx, hy, ang) => {
      if (!kind) return;
      const p = C.weapons[kind];
      if (!p) return K.drawWeapon(ctx, kind, hx, hy, ang, false, pose);
      if (kind === 'yay') return A.drawPart(ctx, p, hx, hy, hx + Math.cos(ang - Math.PI / 2) * 40, hy + Math.sin(ang - Math.PI / 2) * 40);
      const L = TIP[kind] || 60;
      A.drawPart(ctx, p, hx, hy, hx + Math.cos(ang) * L, hy + Math.sin(ang) * L);
    };
    if (P.cape && extra.cape) {
      const v = extra.cape.v || 0, t = extra.cape.t || 0;
      const wave = Math.sin(t * (6 + v * 6)) * (2 + v * 5);
      const len = look.capeLen || 62;
      d('cape', g.shX - 6, g.shY, g.shX - 24 - v * 34 + wave * 0.6 + 8, g.shY + len - v * 22 + wave);
    }
    d('upperArmB', g.shX - 3, g.shY + 4, g.back[0], g.back[1]);
    d('foreArmB', g.back[0], g.back[1], g.back[2], g.back[3]);
    if (extra.offhand) wpn(extra.offhand, g.back[2], g.back[3], pose.wb);
    d('thighB', -4, g.hipY, g.bl[0], g.bl[1]);
    d('shinB', g.bl[0], g.bl[1], g.bl[2], g.bl[3]);
    d('thighF', 4, g.hipY, g.fl[0], g.fl[1]);
    d('shinF', g.fl[0], g.fl[1], g.fl[2], g.fl[3]);
    d('skirt', 0, g.hipY - 8, (g.fl[0] + g.bl[0]) / 2, g.hipY + 34);
    d('torso', 0, g.hipY, g.shX, g.shY);
    const ang = g.lean + (pose.head || 0);
    const hX = g.shX + Math.sin(g.lean) * 12, hY = g.shY - 12;
    d('head', hX - Math.sin(ang) * 10, hY + Math.cos(ang) * 10, hX + Math.sin(ang) * 10, hY - Math.cos(ang) * 10);
    if (extra.eyeGlow) {
      const ex = hX + Math.cos(ang) * 6.4 + Math.sin(ang) * 2.3, ey = hY + Math.sin(ang) * 6.4 - Math.cos(ang) * 2.3;
      const k = extra.eyeGlow;
      const gl = ctx.createRadialGradient(ex, ey, 0.3, ex, ey, 5);
      gl.addColorStop(0, `rgba(255,${200 - k * 120},120,${0.7 + k * 0.3})`);
      gl.addColorStop(0.35, `rgba(255,40,30,${0.5 + k * 0.4})`);
      gl.addColorStop(1, 'rgba(255,0,0,0)');
      ctx.fillStyle = gl;
      ctx.fillRect(ex - 5, ey - 5, 10, 10);
    }
    if (P.shield) {
      const mx = (g.fa[0] + g.hx) / 2 + 4, my = (g.fa[1] + g.hy) / 2;
      A.drawPart(ctx, P.shield, mx, my, mx, my + 17, false);
    }
    wpn(weapon, g.hx, g.hy, pose.wa);
    d('upperArmF', g.shX + 3, g.shY + 4, g.fa[0], g.fa[1]);
    d('foreArmF', g.fa[0], g.fa[1], g.hx, g.hy);
    if (extra.glint) {
      const L = extra.glintLen || 55;
      const gx = g.hx + Math.cos(pose.wa) * L, gy = g.hy + Math.sin(pose.wa) * L;
      const gl = ctx.createRadialGradient(gx, gy, 0.5, gx, gy, 12 * extra.glint + 4);
      gl.addColorStop(0, `rgba(255,250,220,${extra.glint})`);
      gl.addColorStop(1, 'rgba(255,220,150,0)');
      ctx.fillStyle = gl;
      ctx.fillRect(gx - 20, gy - 20, 40, 40);
    }
  }

  function drawHat(ctx, look, flash) {
    const hc = flash ? '#fff' : look.hatColor;
    switch (look.hat) {
      case 'akinci': {
        // Kürk börk ve turna tüyü
        ctx.fillStyle = flash ? '#fff' : '#5b4030';
        ctx.beginPath();
        ctx.ellipse(-1, -9, 12.5, 9, 0, Math.PI, 0);
        ctx.fill();
        ctx.fillRect(-13.5, -10, 26, 5);
        ctx.fillStyle = flash ? '#fff' : '#7a5a42';
        for (let i = -11; i < 12; i += 4) ctx.fillRect(i, -12, 2, 3);
        ctx.fillStyle = flash ? '#fff' : '#a8262b';
        ctx.fillRect(-12, -7, 24, 3);
        ctx.strokeStyle = flash ? '#fff' : '#e9e4da';
        ctx.lineWidth = 3;
        ctx.beginPath();
        ctx.moveTo(-6, -16);
        ctx.quadraticCurveTo(-12, -30, -22, -38);
        ctx.stroke();
        ctx.strokeStyle = flash ? '#fff' : '#2a2a2a';
        ctx.lineWidth = 1.5;
        ctx.beginPath();
        ctx.moveTo(-17, -33);
        ctx.lineTo(-22, -38);
        ctx.stroke();
        break;
      }
      case 'janissary': {
        // Yeniçeri börkü: dik keçe, arkaya sarkan yatırtma
        ctx.fillStyle = hc;
        ctx.beginPath();
        ctx.moveTo(-10, -6);
        ctx.lineTo(8, -6);
        ctx.lineTo(4, -30);
        ctx.quadraticCurveTo(-4, -38, -14, -30);
        ctx.quadraticCurveTo(-24, -10, -26, 18);
        ctx.lineTo(-18, 20);
        ctx.quadraticCurveTo(-16, 0, -10, -6);
        ctx.closePath();
        ctx.fill();
        ctx.strokeStyle = flash ? '#fff' : '#b9b09d';
        ctx.lineWidth = 1.2;
        ctx.stroke();
        ctx.fillStyle = flash ? '#fff' : '#c9a24a';
        ctx.fillRect(-11, -9, 20, 4);
        ctx.fillRect(1, -22, 4, 13);
        break;
      }
      case 'aga': {
        ctx.fillStyle = hc;
        ctx.beginPath();
        ctx.ellipse(0, -16, 15, 16, 0, Math.PI, 0);
        ctx.fill();
        ctx.fillRect(-15, -17, 30, 10);
        ctx.strokeStyle = flash ? '#fff' : '#b9b09d';
        ctx.lineWidth = 1.2;
        for (let i = -12; i < 14; i += 6) {
          ctx.beginPath();
          ctx.moveTo(i, -8);
          ctx.quadraticCurveTo(i + 3, -20, i, -30);
          ctx.stroke();
        }
        ctx.fillStyle = flash ? '#fff' : '#c9a24a';
        ctx.fillRect(-3, -24, 6, 10);
        // Sorguç
        ctx.strokeStyle = flash ? '#fff' : '#f2efe8';
        ctx.lineWidth = 4;
        for (const [dx, h] of [[-4, -58], [0, -64], [4, -56]]) {
          ctx.beginPath();
          ctx.moveTo(0, -24);
          ctx.quadraticCurveTo(dx * 2, -40, dx * 4, h);
          ctx.stroke();
        }
        break;
      }
      case 'kavuk': {
        ctx.fillStyle = hc;
        ctx.beginPath();
        ctx.ellipse(0, -10, 12, 10, 0, Math.PI, 0);
        ctx.fill();
        ctx.fillStyle = flash ? '#fff' : '#a8262b';
        ctx.fillRect(-3, -24, 6, 10);
        ctx.fillStyle = flash ? '#fff' : '#efe9dc';
        ctx.fillRect(-13, -11, 26, 6);
        break;
      }
    }
  }

  // ---------- Silahlar (el noktasında, açıyla) ----------
  K.drawWeapon = function (ctx, kind, hx, hy, ang, flash, pose) {
    const metal = flash ? '#fff' : '#dfe6ee';
    const metalEdge = flash ? '#fff' : '#7d8994';
    const wood = flash ? '#fff' : '#6b4a2e';
    const gold = flash ? '#fff' : '#c9a24a';
    ctx.save();
    ctx.translate(hx, hy);
    ctx.rotate(ang);
    ctx.lineCap = 'round';
    switch (kind) {
      case 'kilic': {
        ctx.fillStyle = wood;
        ctx.fillRect(-10, -2.5, 13, 5);
        ctx.fillStyle = gold;
        ctx.fillRect(3, -7, 3.5, 14);
        ctx.beginPath();
        ctx.arc(-11, 0, 3.5, 0, 7);
        ctx.fill();
        ctx.fillStyle = metal;
        ctx.beginPath();
        ctx.moveTo(6, -2.5);
        ctx.quadraticCurveTo(40, -3, 68, -11);
        ctx.quadraticCurveTo(44, 4, 6, 2.5);
        ctx.closePath();
        ctx.fill();
        ctx.strokeStyle = metalEdge;
        ctx.lineWidth = 1;
        ctx.stroke();
        break;
      }
      case 'yatagan': {
        ctx.fillStyle = flash ? '#fff' : '#e8e0cf';
        ctx.beginPath();
        ctx.moveTo(-4, -3);
        ctx.lineTo(-12, -7);
        ctx.lineTo(-13, 5);
        ctx.lineTo(-4, 3);
        ctx.closePath();
        ctx.fill();
        ctx.fillStyle = gold;
        ctx.fillRect(-4, -3.5, 6, 7);
        ctx.fillStyle = metal;
        ctx.beginPath();
        ctx.moveTo(2, -2.5);
        ctx.quadraticCurveTo(30, -9, 52, 1);
        ctx.quadraticCurveTo(30, 0, 2, 3);
        ctx.closePath();
        ctx.fill();
        ctx.strokeStyle = metalEdge;
        ctx.lineWidth = 1;
        ctx.stroke();
        break;
      }
      case 'mizrak':
      case 'bayrak': {
        ctx.strokeStyle = wood;
        ctx.lineWidth = 4;
        ctx.beginPath();
        ctx.moveTo(-62, 0);
        ctx.lineTo(98, 0);
        ctx.stroke();
        ctx.fillStyle = metal;
        ctx.beginPath();
        ctx.moveTo(96, -5);
        ctx.quadraticCurveTo(110, -5, 124, 0);
        ctx.quadraticCurveTo(110, 5, 96, 5);
        ctx.closePath();
        ctx.fill();
        ctx.strokeStyle = metalEdge;
        ctx.lineWidth = 1;
        ctx.stroke();
        ctx.fillStyle = flash ? '#fff' : '#a8262b';
        ctx.beginPath();
        ctx.moveTo(93, 0);
        ctx.lineTo(87, 12);
        ctx.lineTo(95, 10);
        ctx.closePath();
        ctx.fill();
        if (kind === 'bayrak') {
          const wave = Math.sin(performance.now() / 160) * 5;
          ctx.fillStyle = flash ? '#fff' : '#b0232a';
          ctx.beginPath();
          ctx.moveTo(84, 2);
          ctx.quadraticCurveTo(70, 30 + wave, 48, 52);
          ctx.lineTo(24, 40 + wave);
          ctx.lineTo(40, 26);
          ctx.quadraticCurveTo(56, 12, 60, 2);
          ctx.closePath();
          ctx.fill();
          ctx.strokeStyle = gold;
          ctx.lineWidth = 2;
          ctx.stroke();
        }
        break;
      }
      case 'tufek': {
        ctx.fillStyle = wood;
        ctx.beginPath();
        ctx.moveTo(-44, -2);
        ctx.lineTo(-10, -3);
        ctx.lineTo(-10, 4);
        ctx.lineTo(-44, 8);
        ctx.closePath();
        ctx.fill();
        ctx.fillStyle = flash ? '#fff' : '#3a3a3e';
        ctx.fillRect(-12, -3, 98, 4);
        ctx.fillStyle = gold;
        ctx.fillRect(-14, -4, 6, 7);
        ctx.fillRect(40, -4, 3, 6);
        break;
      }
      case 'tabanca': {
        ctx.fillStyle = wood;
        ctx.beginPath();
        ctx.moveTo(-4, -3);
        ctx.lineTo(4, -3);
        ctx.lineTo(-2, 12);
        ctx.lineTo(-9, 11);
        ctx.closePath();
        ctx.fill();
        ctx.fillStyle = flash ? '#fff' : '#3a3a3e';
        ctx.fillRect(0, -5, 28, 4);
        ctx.fillStyle = gold;
        ctx.fillRect(-2, -6, 6, 6);
        ctx.beginPath();
        ctx.arc(-8, 11, 3, 0, 7);
        ctx.fill();
        break;
      }
      case 'yay': {
        // Türk yayı: el noktası kabzadır, yay namluya dik durur
        const pull = 8 + 42 * ((pose && pose.draw) || 0);
        ctx.strokeStyle = flash ? '#fff' : '#4a2f1d';
        ctx.lineWidth = 4;
        ctx.beginPath();
        ctx.moveTo(-6, -40);
        ctx.quadraticCurveTo(2, -36, 4, -22);
        ctx.quadraticCurveTo(10, -6, 0, 0);
        ctx.quadraticCurveTo(10, 6, 4, 22);
        ctx.quadraticCurveTo(2, 36, -6, 40);
        ctx.stroke();
        ctx.strokeStyle = flash ? '#fff' : '#e8e2d2';
        ctx.lineWidth = 1;
        ctx.beginPath();
        ctx.moveTo(-6, -40);
        ctx.lineTo(-pull, 0);
        ctx.lineTo(-6, 40);
        ctx.stroke();
        if (pose && pose.draw > 0.05) {
          ctx.strokeStyle = wood;
          ctx.lineWidth = 2;
          ctx.beginPath();
          ctx.moveTo(-pull, 0);
          ctx.lineTo(16, 0);
          ctx.stroke();
          ctx.fillStyle = metal;
          ctx.beginPath();
          ctx.moveTo(16, -3);
          ctx.lineTo(23, 0);
          ctx.lineTo(16, 3);
          ctx.fill();
        }
        break;
      }
      case 'cirit': {
        ctx.strokeStyle = flash ? '#fff' : '#8a6a44';
        ctx.lineWidth = 3;
        ctx.beginPath();
        ctx.moveTo(-34, 0);
        ctx.lineTo(62, 0);
        ctx.stroke();
        ctx.fillStyle = metal;
        ctx.beginPath();
        ctx.moveTo(60, -3.5);
        ctx.lineTo(76, 0);
        ctx.lineTo(60, 3.5);
        ctx.fill();
        break;
      }
      case 'topuz': {
        ctx.strokeStyle = wood;
        ctx.lineWidth = 5;
        ctx.beginPath();
        ctx.moveTo(-8, 0);
        ctx.lineTo(48, 0);
        ctx.stroke();
        ctx.fillStyle = flash ? '#fff' : '#6c737a';
        ctx.beginPath();
        ctx.arc(56, 0, 13, 0, 7);
        ctx.fill();
        ctx.fillStyle = flash ? '#fff' : '#9aa2aa';
        for (let i = 0; i < 6; i++) {
          const a = (i / 6) * Math.PI * 2;
          ctx.beginPath();
          ctx.arc(56 + Math.cos(a) * 12, Math.sin(a) * 12, 4, 0, 7);
          ctx.fill();
        }
        break;
      }
    }
    ctx.restore();
  };

  // ---------- At ----------
  K.drawHorse = function (ctx, x, y, facing, t, running) {
    ctx.save();
    ctx.translate(x, y);
    ctx.scale(facing, 1);
    const leg = running ? Math.sin(t * 16) : 0;
    ctx.strokeStyle = '#3d2618';
    ctx.lineWidth = 8;
    ctx.lineCap = 'round';
    const legs = [[-40, leg], [-28, -leg], [32, -leg], [44, leg]];
    for (const [lx, l] of legs) {
      ctx.beginPath();
      ctx.moveTo(lx, -62);
      ctx.lineTo(lx + l * 14, -30);
      ctx.lineTo(lx + l * 8, -2);
      ctx.stroke();
    }
    ctx.fillStyle = '#5a3a24';
    ctx.beginPath();
    ctx.ellipse(0, -76, 58, 24, 0, 0, 7);
    ctx.fill();
    ctx.beginPath();
    ctx.moveTo(40, -88);
    ctx.lineTo(70, -132);
    ctx.lineTo(92, -122);
    ctx.lineTo(62, -76);
    ctx.closePath();
    ctx.fill();
    ctx.beginPath();
    ctx.ellipse(86, -124, 18, 10, 0.5, 0, 7);
    ctx.fill();
    ctx.fillStyle = '#1c120b';
    ctx.beginPath();
    ctx.moveTo(44, -92);
    ctx.lineTo(66, -134);
    ctx.lineTo(58, -100);
    ctx.closePath();
    ctx.fill();
    ctx.strokeStyle = '#1c120b';
    ctx.lineWidth = 6;
    ctx.beginPath();
    ctx.moveTo(-56, -80);
    ctx.quadraticCurveTo(-76, -60, -70, -36 + leg * 4);
    ctx.stroke();
    // Eyer ve örtü
    ctx.fillStyle = '#a8262b';
    ctx.fillRect(-22, -102, 40, 14);
    ctx.fillStyle = '#c9a24a';
    ctx.fillRect(-22, -90, 40, 4);
    ctx.restore();
  };

})(window.K);
