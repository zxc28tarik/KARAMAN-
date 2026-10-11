'use strict';
// Oyun döngüsü, durumlar, arayüz
(function (K) {
  const canvas = document.getElementById('game');
  const ctx = canvas.getContext('2d');
  const params = new URLSearchParams(location.search);

  const G = (K.game = {
    state: 'title',
    player: null,
    enemies: [],
    projs: [],
    kills: 0,
    bounds: { min: 0, max: K.W },
    tokens: 0,
    maxTokens: 2,
    hitstop: 0,
    slowmo: 0,
    flash: 0,
    encIndex: 0,
    encounter: null,
    between: null,
    banner: null,
    stateT: 0,
    time: 0,
    god: params.has('god'),
    addProj(p) {
      this.projs.push(p);
    },
    onKill(e) {
      if (!e.isBoss && e.counted) this.kills++;
      if (e.isBoss) {
        this.slowmo = 0.9;
        K.cam.shake(12, 0.4);
        K.fx.text(e.x, e.y - 220, e.B.isAga ? 'AĞA DÜŞTÜ!' : 'KIDEMLİ DÜŞTÜ!', '#ffd36a', 30, 1.6);
        if (e.B.isAga) {
          for (const o of this.enemies) if (!o.dead) o.die({ dir: K.sign(o.x - e.x), kb: 400 });
        }
      }
    },
    onPlayerDeath() {
      this.state = 'dead';
      this.stateT = 0;
      this.slowmo = 1.0;
    },
    onEncounterDone(enc) {
      if (enc.def.final) return startEnding();
      const P = this.player;
      const heal = Math.round(P.maxHp * 0.3);
      P.hp = Math.min(P.maxHp, P.hp + heal);
      K.fx.text(P.x, P.y - 160, `Nefeslendi +${heal}`, '#9be29a', 18, 1.4);
      this.banner = { text: enc.def.type === 'wave' ? 'TEMİZLENDİ' : 'KIDEMLİ YENİLDİ', sub: 'İlerle →', t: 0, dur: 1.8 };
      const next = enc.i + 1;
      this.between = { next };
      this.bounds = { min: enc.left, max: K.arenaLeft(next) + K.W };
    },
  });

  // ---------- Başlatma ----------
  K.bg.init(K.LEVEL_END + 2000);
  K.paint.bakeAll();

  function newGame() {
    G.player = new K.Player(420);
    G.enemies = [];
    G.projs = [];
    G.kills = 0;
    G.time = 0;
    G.player.maxCombo = 0;
    K.fx.list = [];
    K.fx.decals = [];
    G.bounds = { min: 0, max: K.W };
    K.cam.x = 0;
  }

  function startIntro() {
    newGame();
    G.state = 'intro';
    G.stateT = 0;
    G.horse = { x: 300, run: false };
    G.player.ctrl = false;
    G.player.facing = 1;
    G.caption = null;
  }

  function startEncounter(i) {
    G.encIndex = i;
    G.between = null;
    G.encounter = new K.Encounter(i);
    G.snapshot = { kills: G.kills, dur: Object.assign({}, G.player.dur), akin: G.player.akin, hp: G.player.hp };
    G.encounter.start();
  }

  function jumpTo(i) {
    newGame();
    G.player.x = K.arenaLeft(i) + 300;
    K.cam.x = K.arenaLeft(i);
    // Atlanan dalgaların yeniçerileri sayılmış kabul edilir
    let k = 0;
    for (let j = 0; j < i; j++) {
      const d = K.ENC[j];
      if (d.type === 'wave') k += d.n;
      if (d.escorts) k += d.escorts.length;
      if (d.key === 'bayraktar') k += 4;
    }
    G.kills = k;
    G.state = 'play';
    startEncounter(i);
  }

  function retry() {
    const i = G.encIndex;
    const s = G.snapshot;
    G.enemies = [];
    G.projs = [];
    K.fx.list = [];
    const keep = { dur: s.dur, mi: G.player.mi, ri: G.player.ri, maxCombo: G.player.maxCombo };
    G.player.reset(K.arenaLeft(i) + 300);
    Object.assign(G.player, keep);
    G.player.akin = s.akin;
    G.kills = s.kills;
    G.state = 'play';
    G.slowmo = 0;
    startEncounter(i);
  }

  function startEnding() {
    G.state = 'ending';
    G.stateT = 0;
    G.finishTime = G.time;
    G.player.ctrl = false;
    G.player.setState('cinematic');
    G.player.inv = 99;
    G.breakIdx = 0;
    G.caption = null;
  }

  // ---------- Güncelleme ----------
  const BREAK_ORDER = [
    ['kilic', 'Kılıcı kırıldı.'],
    ['mizrak', 'Mızrağı parçalandı.'],
    ['yay', 'Yayının kirişi koptu.'],
    ['tabanca', 'Tabancası çatladı.'],
    ['cirit', 'Ciritleri tükendi.'],
  ];

  function update(dt) {
    const I = K.input;
    G.time += dt;
    G.stateT += dt;
    if (G.state === 'title') {
      if (I.hit('confirm') || I.hit('jump')) {
        K.audio.unlock();
        K.sfx('ui');
        if (params.has('enc')) jumpTo(K.clamp(parseInt(params.get('enc'), 10) || 0, 0, K.ENC.length - 1));
        else startIntro();
      }
      K.cam.update(dt, K.W / 2 + Math.sin(G.time * 0.1) * 300 + 300, -1000, 99999);
      return;
    }
    if (G.state === 'victory') {
      if (I.hit('confirm')) {
        G.state = 'title';
        G.stateT = 0;
      }
      K.fx.update(dt);
      return;
    }
    if (I.hit('pause') && (G.state === 'play' || G.state === 'paused')) {
      G.state = G.state === 'play' ? 'paused' : 'play';
      K.sfx('ui');
    }
    if (G.state === 'paused') return;

    if (G.state === 'intro') updateIntro(dt);
    if (G.state === 'ending') updateEnding(dt);
    if (G.state === 'dead' && G.stateT > 1.4 && I.hit('confirm')) return retry();

    const real = dt;
    if (G.slowmo > 0) {
      G.slowmo -= real;
      dt *= 0.3;
    }
    if (G.banner) G.banner.t += real;
    if (G.flash > 0) G.flash -= real;

    if (G.hitstop > 0) {
      G.hitstop -= real;
    } else {
      G.player.update(dt);
      for (const e of G.enemies) e.update(dt);
      for (const p of G.projs) p.update(dt);
      separate();
      G.enemies = G.enemies.filter(e => !e.remove);
      G.projs = G.projs.filter(p => !p.dead);
      if (G.encounter && G.state === 'play') G.encounter.update(dt);
      if (G.between && G.player.x > K.arenaLeft(G.between.next) + 520) startEncounter(G.between.next);
    }
    K.fx.update(dt);
    K.cam.update(real, G.player.x + G.player.facing * 60, G.bounds.min, G.bounds.max);
  }

  function separate() {
    const E = G.enemies;
    for (let i = 0; i < E.length; i++) {
      const a = E[i];
      if (a.dead || !a.onGround) continue;
      for (let j = i + 1; j < E.length; j++) {
        const b = E[j];
        if (b.dead || !b.onGround) continue;
        const d = b.x - a.x;
        const min = 30 * Math.max(a.look.scale, b.look.scale);
        if (Math.abs(d) < min) {
          const push = (min - Math.abs(d)) * 0.5 * (d === 0 ? 1 : K.sign(d));
          a.x -= push;
          b.x += push;
        }
      }
    }
  }

  function updateIntro(dt) {
    const t = G.stateT;
    const P = G.player;
    if (t < 0.1) G.caption = { text: 'İstanbul, 1566. Şafak vakti.', t: 0 };
    if (t > 2.6 && !G.introHorn) {
      G.introHorn = true;
      K.sfx('horn');
      G.caption = { text: 'Paşanın yeniçerileri yolu kesiyor.', t: 0 };
    }
    if (t > 4.6 && !G.horse.run) {
      G.horse.run = true;
      G.caption = { text: 'Karamani: “Git oğlum. Bu kavga senin değil.”', t: 0 };
      P.facing = -1;
    }
    if (G.horse.run) G.horse.x -= 520 * dt;
    if (t > 7.2 || (t > 0.6 && K.input.hit('confirm'))) {
      G.introHorn = false;
      G.horse = null;
      G.caption = null;
      G.state = 'play';
      P.ctrl = true;
      P.facing = 1;
      startEncounter(0);
    }
    if (G.caption) G.caption.t += dt;
  }

  function updateEnding(dt) {
    const t = G.stateT;
    const P = G.player;
    const start = 2.2, step = 1.0;
    if (t > start && G.breakIdx < BREAK_ORDER.length && t > start + G.breakIdx * step) {
      const [w, text] = BREAK_ORDER[G.breakIdx];
      P.broken[w] = true;
      P.dur[w] = 0;
      G.caption = { text, t: 0 };
      K.sfx('break');
      for (let i = 0; i < 14; i++) K.fx.add({ type: 'spark', x: P.x + P.facing * 30, y: P.y - 90, vx: K.rand(-300, 300), vy: K.rand(-500, -100), grav: 1200, life: K.rand(0.4, 0.8), size: 2.5, color: '#dfe6ee' });
      G.breakIdx++;
    }
    const after = start + BREAK_ORDER.length * step + 0.6;
    if (t > after && !G.itemLine) {
      G.itemLine = true;
      G.caption = { text: 'Yanında yalnızca özel eşyası kaldı.', t: 0, long: true };
    }
    if (t > after + 3.2) {
      G.itemLine = false;
      G.caption = null;
      G.state = 'victory';
      G.stateT = 0;
    }
    if (G.caption) G.caption.t += dt;
  }

  // ---------- Çizim ----------
  function render() {
    ctx.setTransform(1, 0, 0, 1, 0, 0);
    ctx.fillStyle = '#000';
    ctx.fillRect(0, 0, K.W, K.H);
    const camX = Math.round(K.cam.x);
    ctx.save();
    ctx.translate(K.cam.ox, K.cam.oy);
    K.bg.draw(ctx, camX, K.GATE_X);
    if (G.state !== 'title' && G.player) {
      drawWorld(camX);
      ctx.save();
      ctx.translate(-camX, 0);
      for (const e of G.enemies) e.overlay(ctx);
      K.fx.draw(ctx, 'text');
      if (G.between) drawGoArrow(camX);
      ctx.restore();
    }
    ctx.restore();

    // Kenar kararması
    const v = ctx.createRadialGradient(K.W / 2, K.H / 2, K.H * 0.35, K.W / 2, K.H / 2, K.H * 0.95);
    v.addColorStop(0, 'rgba(0,0,0,0)');
    v.addColorStop(1, 'rgba(10,4,8,0.55)');
    ctx.fillStyle = v;
    ctx.fillRect(0, 0, K.W, K.H);
    if (G.flash > 0) {
      ctx.fillStyle = `rgba(255,236,190,${G.flash * 1.2})`;
      ctx.fillRect(0, 0, K.W, K.H);
    }

    if (G.state === 'title') return drawTitle();
    drawHUD();
    if (G.banner && G.banner.t < G.banner.dur) drawBanner(G.banner);
    if (G.caption) drawCaption(G.caption);
    if (G.state === 'paused') drawPause();
    if (G.state === 'dead') drawDead();
    if (G.state === 'victory') drawVictory();
  }

  // Karakterler, mermiler ve efektler (tam çözünürlük)
  function lightOpts(e, base) {
    const L = K.bg.lightAt(e.x, e.y - 60);
    if (L && L.k > 0.08) return Object.assign(base, { rimDir: L.dir, rimCol: L.col, rim: Math.min(0.95, (base.rim || 0.5) + L.k * 0.6) });
    return Object.assign(base, { rimDir: 1, rimCol: '#ff7a5a' });
  }
  function drawWorld(camX) {
    const P = G.player;
    const dead = G.enemies.filter(e => e.dead), alive = G.enemies.filter(e => !e.dead);
    ctx.save();
    ctx.translate(-camX, 0);
    if (G.horse) {
      K.chr.shadow(ctx, G.horse.x, 70);
      K.chr.draw(ctx, G.horse.x, K.GROUND, g => K.drawHorse(g, G.horse.x, K.GROUND, G.horse.run ? -1 : 1, G.time, G.horse.run), { rim: 0.4, tint: 0.2 });
    }
    K.fx.drawDecals(ctx);
    for (const e of [...alive, P]) K.chr.shadow(ctx, e.x, 34 * e.look.scale);
    for (const e of dead) K.chr.draw(ctx, e.x, e.y, g => e.rig(g), { alpha: e.fadeAlpha, rim: 0.25, tint: 0.35 });
    for (const e of alive) K.chr.draw(ctx, e.x, e.y, g => e.rig(g), lightOpts(e, { flash: e.flash > 0, rim: e.isBoss ? 0.55 : 0.4, tint: e.isBoss ? 0.12 : 0.2 }));
    if (!P.hidden) K.chr.draw(ctx, P.x, P.y, g => P.rig(g), lightOpts(P, { flash: P.flash > 0, rim: 0.65, tint: 0.05 }));
    for (const p of G.projs) p.draw(ctx);
    for (const e of alive) e.drawFx(ctx);
    P.drawFx(ctx);
    K.fx.draw(ctx, 'world');
    ctx.restore();
    K.bg.drawFore(ctx, camX);
  }

  const FONT = 'Georgia, "Times New Roman", serif';
  function text(str, x, y, size, color, align = 'left', weight = 'bold') {
    ctx.font = `${weight} ${size}px ${FONT}`;
    ctx.textAlign = align;
    ctx.lineWidth = Math.max(3, size / 7);
    ctx.strokeStyle = 'rgba(0,0,0,0.85)';
    ctx.strokeText(str, x, y);
    ctx.fillStyle = color;
    ctx.fillText(str, x, y);
  }
  const GOLD = '#c9a24a', GOLD_D = '#5e4422', INK = 'rgba(14,8,12,0.84)';
  // Uçları sivri, altın kenarlı süslü çubuk
  function ornBar(x, y, w, h, k, c0, c1) {
    const tip = h * 0.9;
    const path = () => {
      ctx.beginPath();
      ctx.moveTo(x, y);
      ctx.lineTo(x + w, y);
      ctx.lineTo(x + w + tip, y + h / 2);
      ctx.lineTo(x + w, y + h);
      ctx.lineTo(x, y + h);
      ctx.lineTo(x - tip * 0.5, y + h / 2);
      ctx.closePath();
    };
    path();
    ctx.fillStyle = INK;
    ctx.fill();
    ctx.save();
    path();
    ctx.clip();
    const fw = (w + tip * 1.5) * K.clamp(k, 0, 1);
    const g = ctx.createLinearGradient(0, y, 0, y + h);
    g.addColorStop(0, c1);
    g.addColorStop(1, c0);
    ctx.fillStyle = g;
    ctx.fillRect(x - tip, y, fw, h);
    ctx.fillStyle = 'rgba(255,240,220,0.2)';
    ctx.fillRect(x - tip, y + 1, fw, Math.max(1, h * 0.22));
    ctx.restore();
    path();
    ctx.strokeStyle = GOLD_D;
    ctx.lineWidth = 3.5;
    ctx.stroke();
    ctx.strokeStyle = GOLD;
    ctx.lineWidth = 1.2;
    ctx.stroke();
  }
  function diamond(cx, cy, r, fill, stroke = GOLD, lw = 1.5) {
    ctx.beginPath();
    ctx.moveTo(cx, cy - r);
    ctx.lineTo(cx + r, cy);
    ctx.lineTo(cx, cy + r);
    ctx.lineTo(cx - r, cy);
    ctx.closePath();
    if (fill) {
      ctx.fillStyle = fill;
      ctx.fill();
    }
    if (stroke) {
      ctx.strokeStyle = stroke;
      ctx.lineWidth = lw;
      ctx.stroke();
    }
  }
  function ornLine(cx, y, w) {
    ctx.strokeStyle = GOLD;
    ctx.lineWidth = 1;
    ctx.beginPath();
    ctx.moveTo(cx - w / 2, y);
    ctx.lineTo(cx + w / 2, y);
    ctx.stroke();
    diamond(cx, y, 4, '#1a1010');
    diamond(cx - w / 2, y, 2.5, GOLD, null);
    diamond(cx + w / 2, y, 2.5, GOLD, null);
  }

  // Karamani portresi (bir kez çizilir)
  let portrait = null;
  function getPortrait() {
    if (portrait) return portrait;
    const c = document.createElement('canvas');
    c.width = c.height = 120;
    const g = c.getContext('2d');
    const bg = g.createRadialGradient(70, 40, 6, 60, 60, 80);
    bg.addColorStop(0, '#7a3020');
    bg.addColorStop(0.5, '#3a1418');
    bg.addColorStop(1, '#0e060a');
    g.fillStyle = bg;
    g.fillRect(0, 0, 120, 120);
    g.save();
    g.translate(50, 84);
    g.scale(2.3, 2.3);
    K.drawRig(g, 0, 111, 1, K.pose({ lean: 0.05, au: 0.3, af: 0.4, wa: -1, bu: -0.2, bf: 0.2, head: -0.04 }), K.LOOKS.karamani, null, false, {});
    g.restore();
    portrait = c;
    return c;
  }

  const PLACE = { mahalle: 'Mahalle', carsi: 'Çarşı', cami: 'Cami Avlusu', bostan: 'Bostanlar', sur: 'Edirnekapı' };

  function drawHUD() {
    const P = G.player;
    if (!P) return;
    // Portre: elmas çerçeve
    const px = 68, py = 70, pr = 48;
    ctx.save();
    ctx.beginPath();
    ctx.moveTo(px, py - pr);
    ctx.lineTo(px + pr, py);
    ctx.lineTo(px, py + pr);
    ctx.lineTo(px - pr, py);
    ctx.closePath();
    ctx.clip();
    ctx.drawImage(getPortrait(), px - 60, py - 60);
    ctx.restore();
    diamond(px, py, pr, null, GOLD_D, 5);
    diamond(px, py, pr, null, GOLD, 1.5);
    diamond(px, py, pr + 7, null, 'rgba(201,162,74,0.5)', 1);
    for (const [dx, dy] of [[0, -pr - 7], [pr + 7, 0], [0, pr + 7]]) diamond(px + dx, py + dy, 4, GOLD, null);

    // Can ve Akın
    text('Akıncı Çavuşu Karamani', 128, 36, 17, '#f3e2b0', 'left', 'normal');
    ornBar(128, 46, 330, 16, P.hp / P.maxHp, '#5e0c10', P.hp / P.maxHp < 0.3 ? '#ff4a3a' : '#cc2c2a');
    text(`${Math.ceil(P.hp)}`, 452, 59, 12, '#f8e8d0', 'right', 'normal');
    const full = P.akin >= 100;
    ornBar(128, 72, 240, 8, P.akin / 100, '#4a3010', full ? `hsl(42,95%,${60 + Math.sin(G.time * 10) * 14}%)` : '#c9a24a');
    if (full) text('AKIN  [F]', 384, 81, 12, '#ffd36a', 'left');
    // Cirit hakları
    for (let i = 0; i < 3; i++) diamond(136 + i * 18, 98, 6, i < P.javelins ? '#c9a24a' : 'rgba(20,12,10,0.85)');

    // Sağ üst: yer adı ve görevler
    const place = PLACE[K.district(P.x)] || '';
    text('İstanbul', K.W - 120, 44, 26, '#f3e2b0', 'center', 'normal');
    ornLine(K.W - 120, 56, 200);
    text(place, K.W - 120, 78, 15, '#cbb88a', 'center', 'normal');
    const enc = G.encounter;
    let obj = 'Yola devam et';
    if (enc && !G.between) obj = enc.def.type === 'wave' ? `${enc.def.label}: yeniçerileri püskürt` : enc.def.final ? 'Ağayı yen' : 'Kıdemli yeniçeriyi yen';
    const gx0 = K.W - 340;
    const pg = ctx.createLinearGradient(gx0, 0, K.W, 0);
    pg.addColorStop(0, 'rgba(10,6,10,0)');
    pg.addColorStop(0.4, 'rgba(10,6,10,0.6)');
    pg.addColorStop(1, 'rgba(10,6,10,0.75)');
    ctx.fillStyle = pg;
    ctx.fillRect(gx0, 96, 340, 58);
    diamond(K.W - 290, 113, 6, '#1a1010');
    diamond(K.W - 290, 113, 2.5, GOLD, null);
    text(obj, K.W - 276, 119, 16, '#f3e2b0', 'left', 'normal');
    diamond(K.W - 290, 140, 5, null);
    text(`Yeniçeri  ${G.kills} / 100`, K.W - 276, 145, 15, '#cbb88a', 'left', 'normal');

    // Kıdemli / ağa: alt ortada
    const boss = enc && enc.boss && !enc.boss.dead && enc.boss.state !== 'enter' ? enc.boss : null;
    if (boss) {
      const w = 540, x = K.W / 2 - w / 2 + 20, y = K.H - 54;
      text(boss.name, x + 4, y - 12, 19, '#f3e2b0', 'left', 'normal');
      text(boss.title, x + w, y - 12, 13, '#a89070', 'right', 'normal');
      ornBar(x, y, w, 13, boss.hp / boss.maxHp, '#4e0a0e', '#c42a28');
      ctx.fillStyle = 'rgba(232,196,90,0.85)';
      ctx.fillRect(x, y + 18, w * (boss.poise / boss.poiseMax), 2);
    }

    // Kombo
    if (P.combo >= 3) {
      const k = Math.min(1, P.comboT / 2.2);
      ctx.globalAlpha = 0.4 + k * 0.6;
      text(String(P.combo), K.W - 44, 230, 50, '#ffd36a', 'right');
      ornLine(K.W - 90, 244, 90);
      text('VURUŞ', K.W - 44, 266, 15, '#f3e2b0', 'right', 'normal');
      ctx.globalAlpha = 1;
    }

    // Silah yuvaları
    const slots = [...P.melee.map((w, i) => ({ w, sel: i === P.mi, key: 'Q' })), ...P.ranged.map((w, i) => ({ w, sel: i === P.ri, key: 'E' }))];
    let x = 30;
    const y = K.H - 92;
    slots.forEach((sl, i) => {
      if (i === 2) x += 14;
      drawSlot(x, y, sl, P);
      x += 62;
    });
  }

  function drawSlot(x, y, s, P) {
    const S = 54;
    const broken = P.broken[s.w];
    ctx.fillStyle = INK;
    ctx.fillRect(x, y, S, S);
    const ig = ctx.createLinearGradient(0, y, 0, y + S);
    ig.addColorStop(0, 'rgba(90,60,40,0.25)');
    ig.addColorStop(1, 'rgba(0,0,0,0)');
    ctx.fillStyle = ig;
    ctx.fillRect(x, y, S, S);
    ctx.strokeStyle = GOLD_D;
    ctx.lineWidth = 3;
    ctx.strokeRect(x, y, S, S);
    if (s.sel) {
      ctx.save();
      ctx.shadowColor = 'rgba(255,200,110,0.9)';
      ctx.shadowBlur = 12;
      ctx.strokeStyle = '#f2d684';
      ctx.lineWidth = 2;
      ctx.strokeRect(x + 1, y + 1, S - 2, S - 2);
      ctx.restore();
    } else {
      ctx.strokeStyle = 'rgba(201,162,74,0.55)';
      ctx.lineWidth = 1;
      ctx.strokeRect(x + 1.5, y + 1.5, S - 3, S - 3);
    }
    ctx.save();
    ctx.translate(x + S / 2, y + S / 2 - 3);
    ctx.globalAlpha = broken ? 0.3 : s.sel ? 1 : 0.75;
    const icon = { kilic: [-26, 7, -0.45, 0.54], mizrak: [-21, 14, -0.6, 0.28], yay: [0, 0, 0, 0.52], tabanca: [-10, 2, 0, 0.86], cirit: [-19, 12, -0.6, 0.39] }[s.w];
    ctx.scale(icon[3], icon[3]);
    K.drawWeapon(ctx, s.w, icon[0] / icon[3], icon[1] / icon[3], icon[2], false, { draw: 0 });
    ctx.restore();
    ctx.globalAlpha = 1;
    const d = P.dur[s.w];
    ctx.fillStyle = 'rgba(0,0,0,0.7)';
    ctx.fillRect(x + 5, y + S - 8, S - 10, 3);
    ctx.fillStyle = d > 0.5 ? '#a8a070' : d > 0.25 ? '#d8a04a' : '#d0583a';
    ctx.fillRect(x + 5, y + S - 8, (S - 10) * d, 3);
    if (s.w === 'tabanca' && P.pistolReload > 0) {
      ctx.fillStyle = 'rgba(0,0,0,0.6)';
      ctx.fillRect(x + 2, y + 2, S - 4, (S - 4) * (P.pistolReload / 2.4));
    }
    if (s.w === 'cirit') text(`${P.javelins}`, x + S - 5, y + S - 11, 13, '#f3e2b0', 'right');
    if (broken) {
      ctx.strokeStyle = '#b8402a';
      ctx.lineWidth = 3;
      ctx.beginPath();
      ctx.moveTo(x + 10, y + 10);
      ctx.lineTo(x + S - 10, y + S - 10);
      ctx.moveTo(x + S - 10, y + 10);
      ctx.lineTo(x + 10, y + S - 10);
      ctx.stroke();
    }
    // tuş etiketi
    ctx.fillStyle = INK;
    ctx.fillRect(x + S / 2 - 9, y + S + 6, 18, 17);
    ctx.strokeStyle = 'rgba(201,162,74,0.6)';
    ctx.lineWidth = 1;
    ctx.strokeRect(x + S / 2 - 9, y + S + 6, 18, 17);
    text(s.key, x + S / 2, y + S + 19, 12, s.sel ? '#f3e2b0' : '#8a7a60', 'center', 'normal');
  }

  function drawBanner(b) {
    const k = b.t < 0.25 ? b.t / 0.25 : b.t > b.dur - 0.4 ? (b.dur - b.t) / 0.4 : 1;
    ctx.globalAlpha = K.clamp(k, 0, 1);
    ctx.fillStyle = 'rgba(10,6,8,0.55)';
    ctx.fillRect(0, 250, K.W, b.boss ? 130 : 110);
    if (b.boss) text(b.sub.toUpperCase(), K.W / 2, 290, 18, '#cbb88a', 'center', 'normal');
    text(b.text, K.W / 2, b.boss ? 340 : 315, b.boss ? 44 : 50, '#f3e2b0', 'center');
    if (!b.boss) text(b.sub, K.W / 2, 345, 18, '#cbb88a', 'center', 'normal');
    ctx.globalAlpha = 1;
  }

  function drawCaption(c) {
    const k = Math.min(1, c.t * 3);
    ctx.globalAlpha = k;
    ctx.fillStyle = 'rgba(8,5,6,0.6)';
    ctx.fillRect(0, K.H - 170, K.W, 64);
    text(c.text, K.W / 2, K.H - 128, 26, '#f3e2b0', 'center', c.long ? 'bold' : 'normal');
    ctx.globalAlpha = 1;
  }

  function drawGoArrow(camX) {
    if (Math.floor(G.time * 2.5) % 2) return;
    const x = camX + K.W - 90, y = K.GROUND - 200;
    ctx.fillStyle = 'rgba(243,226,176,0.9)';
    ctx.beginPath();
    ctx.moveTo(x - 30, y - 22);
    ctx.lineTo(x + 20, y);
    ctx.lineTo(x - 30, y + 22);
    ctx.closePath();
    ctx.fill();
    text('İLERLE', x - 6, y - 34, 18, '#f3e2b0', 'center');
  }

  function drawTitle() {
    ctx.fillStyle = 'rgba(10,6,12,0.55)';
    ctx.fillRect(0, 0, K.W, K.H);
    text('KARAMANİ', K.W / 2, 220, 104, '#e8c45a', 'center');
    text('Bölüm I — İstanbul’dan Çıkış', K.W / 2, 272, 26, '#f3e2b0', 'center', 'normal');
    if (Math.floor(G.time * 2) % 2 === 0) text('Başlamak için Enter', K.W / 2, 350, 24, '#fff', 'center');
    const rows = [
      ['A / D', 'Hareket'],
      ['Boşluk', 'Zıpla (havada bir kez daha)'],
      ['J', 'Saldırı (art arda kombo) · W+J havaya kaldır · havada S+J dalış'],
      ['K', 'Ağır saldırı (kalkan kırar)'],
      ['L', 'Menzilli silah (yayı basılı tutup ger)'],
      ['Q / E', 'Yakın / menzilli silah değiştir'],
      ['Shift', 'Kaçınma'],
      ['F', 'Akın (bar dolunca)'],
      ['Esc', 'Duraklat'],
    ];
    const x0 = K.W / 2 - 300;
    ctx.fillStyle = 'rgba(10,6,8,0.6)';
    ctx.fillRect(x0 - 20, 392, 640, rows.length * 28 + 24);
    rows.forEach(([k, v], i) => {
      text(k, x0 + 90, 420 + i * 28, 17, '#e8c45a', 'right');
      text(v, x0 + 110, 420 + i * 28, 17, '#f3e2b0', 'left', 'normal');
    });
  }

  function drawPause() {
    ctx.fillStyle = 'rgba(0,0,0,0.55)';
    ctx.fillRect(0, 0, K.W, K.H);
    text('DURAKLATILDI', K.W / 2, K.H / 2, 48, '#f3e2b0', 'center');
    text('Devam etmek için Esc', K.W / 2, K.H / 2 + 44, 20, '#cbb88a', 'center', 'normal');
  }

  function drawDead() {
    const k = Math.min(1, G.stateT / 1.2);
    ctx.fillStyle = `rgba(30,0,0,${0.55 * k})`;
    ctx.fillRect(0, 0, K.W, K.H);
    ctx.globalAlpha = k;
    text('Karamani düştü', K.W / 2, K.H / 2 - 10, 54, '#e8c45a', 'center');
    if (G.stateT > 1.4) text('Bu çarpışmayı yeniden denemek için Enter', K.W / 2, K.H / 2 + 40, 22, '#f3e2b0', 'center', 'normal');
    ctx.globalAlpha = 1;
  }

  function drawVictory() {
    const k = Math.min(1, G.stateT / 1.5);
    ctx.fillStyle = `rgba(8,5,6,${0.8 * k})`;
    ctx.fillRect(0, 0, K.W, K.H);
    ctx.globalAlpha = k;
    text('BÖLÜM I — SONU', K.W / 2, 220, 56, '#e8c45a', 'center');
    text('Karamani İstanbul’dan çıktı. Yol, Macar sınırına uzanıyor.', K.W / 2, 270, 22, '#f3e2b0', 'center', 'normal');
    const ft = G.finishTime || G.time;
    const m = Math.floor(ft / 60), s = Math.floor(ft % 60);
    const rows = [
      ['Yeniçeri', `${G.kills} / 100`],
      ['En uzun kombo', `${G.player.maxCombo} vuruş`],
      ['Süre', `${m} dk ${String(s).padStart(2, '0')} sn`],
    ];
    rows.forEach(([a, b], i) => {
      text(a, K.W / 2 - 20, 350 + i * 40, 24, '#cbb88a', 'right', 'normal');
      text(b, K.W / 2 + 20, 350 + i * 40, 24, '#f3e2b0', 'left');
    });
    if (G.stateT > 2) text('Ana menüye dönmek için Enter', K.W / 2, 560, 20, '#cbb88a', 'center', 'normal');
    ctx.globalAlpha = 1;
  }

  // ---------- Döngü ----------
  let last = performance.now();
  function frame(now) {
    const dt = Math.min(0.033, (now - last) / 1000);
    last = now;
    try {
      update(dt);
      render();
    } catch (err) {
      console.error(err);
    }
    K.input.endFrame();
    requestAnimationFrame(frame);
  }
  requestAnimationFrame(frame);

  // Test ve hata ayıklama için
  K.debug = {
    G, jumpTo, startIntro, retry,
    // Görünmez sekmede test için: n kare ilerlet
    step(n = 1, dt = 1 / 60) {
      for (let i = 0; i < n; i++) {
        update(dt);
        K.input.endFrame();
      }
      render();
    },
  };
})(window.K);
