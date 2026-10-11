'use strict';
// Çekirdek: sabitler, yardımcılar, girdi, ses, efektler, kamera, çarpışma
window.K = window.K || {};
(function (K) {
  K.W = 1280;
  K.H = 720;
  K.GROUND = 610;
  K.GRAV = 2400;

  K.rand = (a, b) => a + Math.random() * (b - a);
  K.randi = (a, b) => Math.floor(K.rand(a, b + 1));
  K.pick = arr => arr[Math.floor(Math.random() * arr.length)];
  K.clamp = (v, a, b) => (v < a ? a : v > b ? b : v);
  K.lerp = (a, b, t) => a + (b - a) * t;
  K.approach = (v, t, d) => (v < t ? Math.min(v + d, t) : Math.max(v - d, t));
  K.sign = v => (v < 0 ? -1 : 1);
  K.overlap = (a, b) => a.x < b.x + b.w && a.x + a.w > b.x && a.y < b.y + b.h && a.y + a.h > b.y;
  K.seeded = function (seed) {
    let s = seed >>> 0;
    return () => {
      s = (s * 1664525 + 1013904223) >>> 0;
      return s / 4294967296;
    };
  };

  // ---------- Girdi ----------
  const BIND = {
    left: ['KeyA', 'ArrowLeft'],
    right: ['KeyD', 'ArrowRight'],
    up: ['KeyW', 'ArrowUp'],
    down: ['KeyS', 'ArrowDown'],
    jump: ['Space'],
    attack: ['KeyJ'],
    heavy: ['KeyK'],
    ranged: ['KeyL'],
    dodge: ['ShiftLeft', 'ShiftRight'],
    swapMelee: ['KeyQ'],
    swapRanged: ['KeyE'],
    super: ['KeyF'],
    pause: ['Escape', 'KeyP'],
    confirm: ['Enter'],
  };
  const down = {}, pressed = {}, released = {};
  const GAME_KEYS = new Set(Object.values(BIND).flat());
  addEventListener('keydown', e => {
    if (GAME_KEYS.has(e.code)) e.preventDefault();
    if (!down[e.code]) pressed[e.code] = true;
    down[e.code] = true;
    K.audio.unlock();
  });
  addEventListener('keyup', e => {
    down[e.code] = false;
    released[e.code] = true;
  });
  addEventListener('blur', () => {
    for (const k in down) down[k] = false;
  });
  K.input = {
    held: a => BIND[a].some(c => down[c]),
    hit: a => BIND[a].some(c => pressed[c]),
    up: a => BIND[a].some(c => released[c]),
    endFrame() {
      for (const k in pressed) delete pressed[k];
      for (const k in released) delete released[k];
    },
  };

  // ---------- Ses (WebAudio ile üretilen kısa efektler) ----------
  K.audio = {
    ctx: null,
    unlock() {
      if (!this.ctx) {
        try {
          this.ctx = new (window.AudioContext || window.webkitAudioContext)();
        } catch (e) {
          this.ctx = null;
        }
      }
      if (this.ctx && this.ctx.state === 'suspended') this.ctx.resume();
    },
  };
  let noiseBuf = null;
  function noise(ctx) {
    if (noiseBuf) return noiseBuf;
    const b = ctx.createBuffer(1, ctx.sampleRate * 0.6, ctx.sampleRate);
    const d = b.getChannelData(0);
    for (let i = 0; i < d.length; i++) d[i] = Math.random() * 2 - 1;
    return (noiseBuf = b);
  }
  const SFX = {
    swing: { n: 1, f: 2200, q: 1.4, ft: 'bandpass', d: 0.1, v: 0.1 },
    hit: { n: 1, f: 1100, d: 0.12, v: 0.32, tone: 160 },
    heavy: { n: 1, f: 600, d: 0.24, v: 0.5, tone: 90 },
    shot: { n: 1, f: 1400, d: 0.4, v: 0.55, tone: 70 },
    bow: { tone: 520, sweep: 160, d: 0.12, v: 0.14, wave: 'triangle' },
    throw: { n: 1, f: 900, q: 1, ft: 'bandpass', d: 0.18, v: 0.14 },
    hurt: { tone: 220, sweep: 90, d: 0.2, v: 0.2, wave: 'square' },
    clang: { tone: 1500, sweep: 900, d: 0.25, v: 0.16, wave: 'triangle' },
    break: { n: 1, f: 3200, d: 0.5, v: 0.4, tone: 340, sweep: 60, wave: 'sawtooth' },
    boom: { n: 1, f: 260, d: 0.7, v: 0.7, tone: 55 },
    ui: { tone: 660, sweep: 880, d: 0.09, v: 0.1 },
    jump: { tone: 300, sweep: 520, d: 0.08, v: 0.06 },
    dash: { n: 1, f: 1600, q: 0.8, ft: 'bandpass', d: 0.16, v: 0.12 },
    horn: { tone: 180, sweep: 240, d: 0.9, v: 0.12, wave: 'sawtooth' },
  };
  const lastPlay = {};
  K.sfx = function (type, vol = 1) {
    const ctx = K.audio.ctx;
    const P = SFX[type];
    if (!ctx || !P) return;
    const now = performance.now();
    if (lastPlay[type] && now - lastPlay[type] < 28) return;
    lastPlay[type] = now;
    const t = ctx.currentTime;
    const g = ctx.createGain();
    g.connect(ctx.destination);
    const v = P.v * vol * 0.8;
    g.gain.setValueAtTime(v, t);
    g.gain.exponentialRampToValueAtTime(0.0008, t + P.d);
    if (P.n) {
      const s = ctx.createBufferSource();
      s.buffer = noise(ctx);
      const f = ctx.createBiquadFilter();
      f.type = P.ft || 'lowpass';
      f.frequency.value = P.f;
      if (P.q) f.Q.value = P.q;
      s.connect(f);
      f.connect(g);
      s.start(t);
      s.stop(t + P.d);
    }
    if (P.tone) {
      const o = ctx.createOscillator();
      o.type = P.wave || 'sine';
      o.frequency.setValueAtTime(P.tone, t);
      o.frequency.exponentialRampToValueAtTime(P.sweep || Math.max(30, P.tone * 0.5), t + P.d);
      const og = ctx.createGain();
      og.gain.value = 0.6;
      o.connect(og);
      og.connect(g);
      o.start(t);
      o.stop(t + P.d);
    }
  };

  // ---------- Efektler ----------
  K.fx = {
    list: [],
    add(p) {
      const o = Object.assign({ life: 0.5, vx: 0, vy: 0, grav: 0, size: 3, color: '#fff', type: 'dot', drag: 0 }, p);
      o.max = o.life;
      this.list.push(o);
      return o;
    },
    update(dt) {
      for (const p of this.list) {
        p.life -= dt;
        p.vy += p.grav * dt;
        if (p.drag) {
          p.vx *= Math.max(0, 1 - p.drag * dt);
          p.vy *= Math.max(0, 1 - p.drag * dt);
        }
        p.x += p.vx * dt;
        p.y += p.vy * dt;
        if (p.type === 'dot' && p.grav > 0 && p.y > K.GROUND + 4) {
          if (!p.landed && p.blood) {
            p.landed = true;
            this.decals.push({ x: p.x, dy: K.rand(-2, 10), r: K.rand(2, 6), c: K.pick(['rgba(70,6,8,0.85)', 'rgba(96,10,12,0.8)', 'rgba(50,4,6,0.9)']) });
            if (this.decals.length > 260) this.decals.shift();
          }
          p.y = K.GROUND + 4;
          p.vy *= -0.25;
          p.vx *= 0.5;
        }
      }
      this.list = this.list.filter(p => p.life > 0);
      if (this.list.length > 1200) this.list.splice(0, this.list.length - 1200);
    },
    draw(ctx, only) {
      for (const p of this.list) {
        if (only === 'text' && p.type !== 'text') continue;
        if (only === 'world' && p.type === 'text') continue;
        const a = K.clamp(p.life / p.max, 0, 1);
        ctx.globalAlpha = p.type === 'text' ? Math.min(1, a * 2) : a;
        switch (p.type) {
          case 'dot':
            ctx.fillStyle = p.color;
            ctx.fillRect(p.x - p.size / 2, p.y - p.size / 2, p.size, p.size);
            break;
          case 'spark':
            ctx.strokeStyle = p.color;
            ctx.lineWidth = p.size;
            ctx.lineCap = 'round';
            ctx.beginPath();
            ctx.moveTo(p.x, p.y);
            ctx.lineTo(p.x - p.vx * 0.035, p.y - p.vy * 0.035);
            ctx.stroke();
            break;
          case 'smoke':
            ctx.fillStyle = p.color;
            ctx.beginPath();
            ctx.arc(p.x, p.y, p.size * (1.8 - a * 0.8), 0, Math.PI * 2);
            ctx.fill();
            break;
          case 'ring': {
            const r = p.size * (1 - a) + 12;
            ctx.strokeStyle = p.color;
            ctx.lineWidth = 5 * a + 1;
            ctx.beginPath();
            ctx.ellipse(p.x, p.y, r, r * (p.flat || 0.28), 0, 0, Math.PI * 2);
            ctx.stroke();
            break;
          }
          case 'arc': {
            // Kılıç izi: p.a0 -> p.a1 arası yay
            ctx.save();
            ctx.translate(p.x, p.y);
            ctx.scale(p.dir, 1);
            ctx.strokeStyle = p.color;
            ctx.lineCap = 'round';
            for (let i = 0; i < 3; i++) {
              ctx.globalAlpha = a * (0.8 - i * 0.22);
              ctx.lineWidth = (p.width || 10) * (1 - i * 0.3) * a;
              ctx.beginPath();
              const rr = p.r - i * 7;
              if (p.a1 > p.a0) ctx.arc(0, 0, rr, p.a0, p.a1, false);
              else ctx.arc(0, 0, rr, p.a1, p.a0, false);
              ctx.stroke();
            }
            ctx.restore();
            break;
          }
          case 'line':
            ctx.strokeStyle = p.color;
            ctx.lineWidth = (p.size || 6) * a;
            ctx.lineCap = 'round';
            ctx.beginPath();
            ctx.moveTo(p.x, p.y);
            ctx.lineTo(p.x2, p.y2);
            ctx.stroke();
            break;
          case 'ghost':
            if (p.drawFn) p.drawFn(ctx, a);
            break;
          case 'text':
            ctx.font = `bold ${p.size}px Georgia, serif`;
            ctx.textAlign = 'center';
            ctx.lineWidth = 4;
            ctx.strokeStyle = 'rgba(0,0,0,0.85)';
            ctx.strokeText(p.text, p.x, p.y);
            ctx.fillStyle = p.color;
            ctx.fillText(p.text, p.x, p.y);
            break;
        }
      }
      ctx.globalAlpha = 1;
    },
    sparks(x, y, dir, n = 8, color = '#ffe08a', spd = 600) {
      for (let i = 0; i < n; i++) {
        const a = (dir > 0 ? 0 : Math.PI) + K.rand(-0.9, 0.9);
        const s = K.rand(spd * 0.4, spd);
        this.add({ type: 'spark', x, y, vx: Math.cos(a) * s, vy: Math.sin(a) * s - 80, grav: 900, life: K.rand(0.15, 0.32), size: 2, color });
      }
    },
    blood(x, y, dir, n = 8) {
      for (let i = 0; i < n; i++) {
        this.add({ type: 'dot', blood: true, x, y, vx: dir * K.rand(80, 460), vy: K.rand(-460, -60), grav: 1500, life: K.rand(0.5, 1.0), size: K.rand(2.5, 6), color: K.pick(['#7a1010', '#9a1a18', '#5a0a0a']) });
      }
    },
    dust(x, y, n = 6, spread = 1) {
      for (let i = 0; i < n; i++) {
        this.add({ type: 'smoke', x: x + K.rand(-10, 10) * spread, y: y - 4, vx: K.rand(-120, 120) * spread, vy: K.rand(-60, -10), drag: 3, life: K.rand(0.3, 0.6), size: K.rand(5, 10), color: 'rgba(170,150,125,0.55)' });
      }
    },
    smoke(x, y, dir, n = 10) {
      for (let i = 0; i < n; i++) {
        this.add({ type: 'smoke', x, y, vx: dir * K.rand(40, 260), vy: K.rand(-80, 20), drag: 2.4, life: K.rand(0.5, 1.1), size: K.rand(6, 13), color: 'rgba(210,205,195,0.5)' });
      }
    },
    spray(x, y, dir, n = 12, power = 1) {
      for (let i = 0; i < n; i++) {
        const a = (dir > 0 ? 0 : Math.PI) + K.rand(-0.7, 0.5) * (dir > 0 ? 1 : -1);
        const s = K.rand(250, 750) * power;
        this.add({ type: 'spark', x, y, vx: Math.cos(a) * s, vy: Math.sin(a) * s - 120, grav: 1100, life: K.rand(0.25, 0.55), size: K.rand(2.5, 4.5), color: K.pick(['#6a0808', '#8e1010', '#a81c16']) });
      }
      for (let i = 0; i < n * 0.6; i++) {
        this.add({ type: 'smoke', x: x + K.rand(-6, 6), y: y + K.rand(-6, 6), vx: dir * K.rand(40, 160), vy: K.rand(-60, 20), drag: 3, life: K.rand(0.3, 0.6), size: K.rand(3, 6), color: 'rgba(120,10,12,0.55)' });
      }
    },
    decals: [],
    drawDecals(ctx) {
      for (const d of this.decals) {
        ctx.fillStyle = d.c;
        ctx.beginPath();
        ctx.ellipse(d.x, K.GROUND + 3 + d.dy, d.r, d.r * 0.3, 0, 0, Math.PI * 2);
        ctx.fill();
      }
    },
    number(x, y, val, color = '#ffe9a8', size = 18) {
      this.add({ type: 'text', text: String(Math.round(val)), x: x + K.rand(-12, 12), y, vy: -90, drag: 2, life: 0.7, size, color });
    },
    text(x, y, text, color = '#fff', size = 22, life = 1) {
      this.add({ type: 'text', text, x, y, vy: -40, drag: 1, life, size, color });
    },
  };

  // ---------- Kamera ----------
  K.cam = {
    x: 0,
    shakeT: 0,
    shakeMag: 0,
    ox: 0,
    oy: 0,
    shake(mag, t) {
      if (mag >= this.shakeMag || this.shakeT <= 0) this.shakeMag = mag;
      this.shakeT = Math.max(this.shakeT, t);
    },
    update(dt, focusX, minX, maxX) {
      let tx = focusX - K.W / 2;
      tx = K.clamp(tx, minX, Math.max(minX, maxX - K.W));
      this.x = K.lerp(this.x, tx, 1 - Math.pow(0.002, dt));
      if (this.shakeT > 0) {
        this.shakeT -= dt;
        this.ox = K.rand(-1, 1) * this.shakeMag;
        this.oy = K.rand(-1, 1) * this.shakeMag * 0.6;
        if (this.shakeT <= 0) this.shakeMag = 0;
      } else {
        this.ox = this.oy = 0;
      }
    },
  };

  // ---------- Çarpışma ----------
  // Saldırı dikdörtgeninin karşı takımdaki hedeflere değmesi; her hedef bir saldırıda bir kez vurulur.
  K.strike = function (team, rect, p, hitSet) {
    const G = K.game;
    const targets = team === 'player' ? G.enemies : [G.player];
    let n = 0;
    for (const t of targets) {
      if (!t || t.dead || hitSet.has(t)) continue;
      if (!K.overlap(rect, t.hurtbox())) continue;
      hitSet.add(t);
      const r = t.takeHit(p);
      if (r) {
        n++;
        if (p.onHit) p.onHit(t, r);
      }
    }
    return n;
  };
})(window.K);
