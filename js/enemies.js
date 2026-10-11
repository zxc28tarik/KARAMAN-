'use strict';
// Yeniçeriler, kıdemliler, ağa ve mermiler
(function (K) {
  const P = K.pose;
  // Yeniçeriler: koyu kızıl dolama, deri teçhizat, kirli beyaz börk
  const JAN = { pants: '#2c2220', boots: '#3c2a1c', trim: '#9a7a44', sash: '#2a1c14', hatColor: '#d2c8b6', mustache: '#1a120c', tattered: true };
  K.defineLook('kilicli', Object.assign({}, JAN, { coat: '#6a1a1c' }));
  K.defineLook('mizrakli', Object.assign({}, JAN, { coat: '#561a20', armor: 'leather' }));
  K.defineLook('tufekli', Object.assign({}, JAN, { coat: '#4c2620', baldric: true }));
  K.defineLook('kalkanli', Object.assign({}, JAN, { coat: '#701e22', shield: true, armor: 'leather', pauldron: true }));
  K.defineLook('odabasi', Object.assign({}, JAN, { scale: 1.12, coat: '#6a2c16', trim: '#a8884a', mustache: '#140c08', scar: true, bracer: true, armor: 'leather', pauldron: true }));
  K.defineLook('tufekcibasi', Object.assign({}, JAN, { scale: 1.1, coat: '#2c3428', trim: '#8a7a4a', beard: '#2a1e16', baldric: true, age: true }));
  K.defineLook('topuzcu', Object.assign({}, JAN, { scale: 1.32, coat: '#3a2c24', beard: '#1e1610', mail: true, shield: true, bracer: true, furMantle: '#2e2219', age: true, pauldron: true }));
  K.defineLook('bayraktar', Object.assign({}, JAN, { scale: 1.14, coat: '#5a1214', hat: 'kavuk', hatColor: '#5e1216', armor: 'leather' }));
  K.defineLook('aga', { scale: 1.45, coat: '#4c0a10', trim: '#b08a40', sash: '#9a7a3a', pants: '#1a1622', boots: '#3a2a1e', beard: '#140e0a', mustache: '#140e0a', hat: 'aga', hatColor: '#d6ccb8', mail: true, cape: '#1c0e16', capeLining: '#8a6a30', capeFur: '#3a2a1e', capeLen: 74, furMantle: '#3a2a1e', age: true, scar: true, pauldron: true, tattered: true });

  const STANCE = P({ lean: 0.14, bob: 5, ft: 0.6, fs: 0.1, bt: -0.35, bs: -0.48, au: 0.55, af: 1.2, wa: -0.5 });
  const SPEAR = P({ lean: 0.14, bob: 5, ft: 0.6, fs: 0.1, bt: -0.35, bs: -0.48, au: 0.75, af: 1.38, wa: -0.08 });
  const GUN = P({ lean: 0.05, au: 0.9, af: 1.2, wa: -1.2, ft: 0.25, bt: -0.25 });
  const AIM = P({ lean: 0.0, au: 1.5, af: 1.57, wa: 0, ft: 0.35, bt: -0.35, bs: -0.4 });

  // Hamle şablonları
  const SLASH = { windup: 0.42, active: 0.12, recover: 0.45, dmg: 20, reach: 95, hy: -125, hh: 100, kb: 240, stun: 0.3, wind: P({ lean: -0.1, au: 2.7, af: 3.0, wa: -2.0 }), strike: P({ lean: 0.35, au: 1.0, af: 0.7, wa: 0.6, ft: 0.6, fs: 0.2, bt: -0.4, bs: -0.6 }), lunge: 220, arc: [-2.0, 0.8, 70] };
  const THRUST = { windup: 0.55, active: 0.14, recover: 0.5, dmg: 24, reach: 150, hy: -115, hh: 50, kb: 260, stun: 0.3, wind: P({ lean: -0.15, au: 0.4, af: 1.1, wa: 0 }), strike: P({ lean: 0.4, au: 1.45, af: 1.57, wa: 0, ft: 0.75, fs: 0.3, bt: -0.6, bs: -0.7 }), lunge: 200, thrust: true };
  const BASH = { windup: 0.32, active: 0.1, recover: 0.4, dmg: 14, reach: 80, hy: -120, hh: 90, kb: 340, stun: 0.3, wind: P({ au: -0.6, af: 0.2, wa: -1.0 }), strike: P({ lean: 0.3, au: 1.4, af: 1.3, wa: -0.4, ft: 0.6, bt: -0.4 }), lunge: 160 };

  const TYPES = {
    kilicli: { hp: 62, speed: 210, look: 'kilicli', weapon: 'yatagan', base: STANCE, moves: { slash: SLASH }, range: 88 },
    mizrakli: { hp: 72, speed: 185, look: 'mizrakli', weapon: 'mizrak', base: SPEAR, moves: { slash: THRUST }, range: 140 },
    tufekli: { hp: 52, speed: 175, look: 'tufekli', weapon: 'tufek', base: GUN, gunner: true, moves: { slash: BASH }, range: 80 },
    kalkanli: { hp: 86, speed: 165, look: 'kalkanli', weapon: 'yatagan', base: STANCE, moves: { slash: Object.assign({}, SLASH, { windup: 0.5 }) }, range: 88, shield: true },
  };
  K.ENEMY_TYPES = TYPES;

  // ================= Mermiler =================
  class Proj {
    constructor(o) {
      Object.assign(this, { w: 16, h: 10, life: 3, grav: 0, pierce: 1, stun: 0.3, kb: 100, dmg: 10 }, o);
      this.hitSet = new Set();
      this.dead = false;
      this.stuck = false;
      this.t = 0;
    }
    update(dt) {
      this.t += dt;
      this.life -= dt;
      if (this.life <= 0) this.dead = true;
      if (this.stuck) return;
      if (this.kind === 'bomb') return this.updateBomb(dt);
      if (this.kind === 'wave') return this.updateWave(dt);
      this.vy += this.grav * dt;
      this.x += this.vx * dt;
      this.y += this.vy * dt;
      if (this.kind === 'bullet' || this.kind === 'ebullet') {
        if (Math.random() < 0.6) K.fx.add({ type: 'dot', x: this.x, y: this.y, size: 2, life: 0.15, color: 'rgba(255,220,150,0.8)' });
      }
      if (this.y >= K.GROUND - 2) {
        this.y = K.GROUND - 2;
        if (this.kind === 'arrow' || this.kind === 'javelin') {
          this.stuck = true;
          this.life = 1.5;
        } else this.dead = true;
        K.fx.dust(this.x, K.GROUND, 3);
        return;
      }
      const r = { x: this.x - this.w / 2, y: this.y - this.h / 2, w: this.w, h: this.h };
      const dir = K.sign(this.vx);
      K.strike(this.team, r, {
        dmg: this.dmg, dir, kb: this.kb, stun: this.stun, knockdown: this.knockdown, guardBreak: this.guardBreak, poise: this.poise || 1, projectile: true, src: this,
        onHit: (t, res) => {
          if (res === 'blocked') {
            this.dead = true;
            K.fx.sparks(this.x, this.y, -dir, 8, '#fff2c0');
            return;
          }
          if (this.onHit) this.onHit(t, res);
          this.pierce--;
          if (this.pierce <= 0) this.dead = true;
        },
      }, this.hitSet);
      const b = K.game.bounds;
      if (this.x < b.min - 200 || this.x > b.max + 200) this.dead = true;
    }
    updateBomb(dt) {
      if (!this.landed) {
        this.vy += 1500 * dt;
        this.x += this.vx * dt;
        this.y += this.vy * dt;
        if (this.y >= K.GROUND - 8) {
          this.y = K.GROUND - 8;
          this.landed = true;
          this.fuse = 0.55;
        }
        return;
      }
      this.fuse -= dt;
      if (Math.random() < 0.5) K.fx.add({ type: 'dot', x: this.x + K.rand(-3, 3), y: this.y - 10, vy: -60, size: 3, life: 0.3, color: '#ffcf6a' });
      if (this.fuse <= 0) {
        this.dead = true;
        const R = 120;
        K.strike('enemy', { x: this.x - R, y: this.y - 140, w: R * 2, h: 150 }, { dmg: 38, dir: K.sign(K.game.player.x - this.x), kb: 420, knockdown: true, stun: 0.6 }, new Set());
        K.fx.add({ type: 'ring', x: this.x, y: K.GROUND, size: 180, life: 0.4, color: 'rgba(255,190,90,0.9)' });
        K.fx.smoke(this.x, this.y - 20, 1, 10);
        K.fx.smoke(this.x, this.y - 20, -1, 10);
        K.fx.sparks(this.x, this.y - 10, 1, 14, '#ffb24a', 700);
        K.fx.sparks(this.x, this.y - 10, -1, 14, '#ffb24a', 700);
        K.cam.shake(9, 0.25);
        K.sfx('boom');
      }
    }
    updateWave(dt) {
      this.x += this.vx * dt;
      if (Math.random() < 0.7) K.fx.dust(this.x, K.GROUND, 1, 0.6);
      K.strike('enemy', { x: this.x - 22, y: K.GROUND - 46, w: 44, h: 46 }, { dmg: this.dmg, dir: K.sign(this.vx), kb: 260, knockdown: true, stun: 0.5 }, this.hitSet);
      const b = K.game.bounds;
      if (this.x < b.min - 40 || this.x > b.max + 40) this.dead = true;
    }
    draw(ctx) {
      ctx.save();
      ctx.translate(this.x, this.y);
      const ang = this.stuck ? this.stuckAng : Math.atan2(this.vy, this.vx);
      if (!this.stuck) this.stuckAng = ang;
      if (this.stuck) ctx.globalAlpha = Math.min(1, this.life);
      switch (this.kind) {
        case 'arrow':
          ctx.rotate(ang);
          ctx.strokeStyle = '#7a5a3a';
          ctx.lineWidth = 2;
          ctx.beginPath();
          ctx.moveTo(-34, 0);
          ctx.lineTo(4, 0);
          ctx.stroke();
          ctx.fillStyle = '#dfe6ee';
          ctx.beginPath();
          ctx.moveTo(4, -3);
          ctx.lineTo(12, 0);
          ctx.lineTo(4, 3);
          ctx.fill();
          ctx.fillStyle = '#e9e2d2';
          ctx.fillRect(-36, -3, 8, 2);
          ctx.fillRect(-36, 1, 8, 2);
          break;
        case 'javelin':
          ctx.rotate(ang);
          K.drawWeapon(ctx, 'cirit', -30, 0, 0, false);
          break;
        case 'bullet':
        case 'ebullet':
          ctx.fillStyle = this.kind === 'bullet' ? '#fff3c4' : '#ffb08a';
          ctx.beginPath();
          ctx.arc(0, 0, 4, 0, 7);
          ctx.fill();
          break;
        case 'bomb':
          ctx.fillStyle = '#26221f';
          ctx.beginPath();
          ctx.arc(0, 0, 9, 0, 7);
          ctx.fill();
          if (this.landed) {
            const r = 120 * (1 - this.fuse / 0.55);
            ctx.strokeStyle = 'rgba(255,90,60,0.7)';
            ctx.lineWidth = 2;
            ctx.beginPath();
            ctx.ellipse(0, 8, 120, 22, 0, 0, 7);
            ctx.stroke();
            ctx.fillStyle = 'rgba(255,90,60,0.18)';
            ctx.beginPath();
            ctx.ellipse(0, 8, r, r * 0.18, 0, 0, 7);
            ctx.fill();
          }
          break;
        case 'wave':
          ctx.fillStyle = 'rgba(200,170,130,0.75)';
          ctx.beginPath();
          ctx.moveTo(-22, 0);
          ctx.quadraticCurveTo(0, -60, 22, 0);
          ctx.fill();
          break;
      }
      ctx.restore();
    }
  }
  K.Proj = Proj;

  // ================= Temel düşman =================
  class Enemy {
    constructor(type, x, opts = {}) {
      const T = typeof type === 'string' ? TYPES[type] : type;
      this.T = T;
      this.type = opts.typeName || type;
      this.look = K.LOOKS[T.look];
      this.team = 'enemy';
      this.x = x;
      this.y = K.GROUND;
      this.vx = this.vy = 0;
      this.facing = x < K.game.player.x ? 1 : -1;
      this.maxHp = this.hp = T.hp;
      this.speed = T.speed * K.rand(0.9, 1.1);
      this.state = 'enter';
      this.st = 0;
      this.dead = false;
      this.remove = false;
      this.onGround = true;
      this.flash = 0;
      this.cd = K.rand(0.4, 1.4);
      this.token = false;
      this.ring = K.rand(150, 270);
      this.jitter = K.rand(0, 10);
      this.anim = K.rand(0, 5);
      this.poiseMax = T.poise || 0;
      this.poise = this.poiseMax;
      this.counted = opts.counted !== false;
      this.isBoss = false;
      this.blocks = 0;
      this.pose = T.base;
    }
    get player() { return K.game.player; }
    hurtbox() {
      const s = this.look.scale;
      if (this.state === 'down' || (this.dead && this.onGround)) return { x: this.x - 40 * s, y: this.y - 30, w: 80 * s, h: 30 };
      return { x: this.x - 18 * s, y: this.y - 112 * s, w: 36 * s, h: 112 * s };
    }
    setState(s) {
      this.state = s;
      this.st = 0;
    }
    releaseToken() {
      if (this.token) {
        this.token = false;
        K.game.tokens = Math.max(0, K.game.tokens - 1);
      }
    }
    // ---- Hasar ----
    takeHit(p) {
      if (this.dead) return false;
      if (this.state === 'getup' && this.st < 0.25) return false;
      const fromFront = K.sign(p.src ? p.src.x - this.x : -p.dir) === this.facing;
      // Kalkan: önden gelen hafif vuruşları keser
      if (this.T.shield && !p.guardBreak && fromFront && this.onGround && !['hurt', 'down', 'air', 'stagger', 'getup'].includes(this.state) && !this.state.startsWith('m_act')) {
        this.blocks++;
        K.fx.sparks(this.x + this.facing * 26, this.y - 80 * this.look.scale, -this.facing, 7, '#fff2c0');
        K.sfx('clang');
        this.vx = p.dir * 120;
        if (this.blocks >= 4) {
          this.blocks = 0;
          this.stagger(0.9);
        }
        return p.projectile ? 'blocked' : 'blocked';
      }
      const dmg = p.dmg;
      this.hp -= dmg;
      this.flash = 0.1;
      K.fx.number(this.x, this.y - 125 * this.look.scale, dmg, dmg >= 30 ? '#ffd36a' : '#fff1c8', dmg >= 30 ? 22 : 16);
      K.fx.blood(this.x, this.y - 75 * this.look.scale, p.dir, dmg >= 30 ? 16 : 9);
      K.fx.spray(this.x, this.y - 80 * this.look.scale, p.dir, dmg >= 30 ? 16 : 9, dmg >= 30 ? 1.3 : 1);
      K.fx.sparks(this.x, this.y - 80 * this.look.scale, p.dir, 5);
      K.sfx(p.heavy ? 'heavy' : 'hit');
      if (this.hp <= 0) {
        this.die(p);
        return 'hit';
      }
      // Yerde yatana vuruş: yerde tutar
      if (this.state === 'down' && !p.launch) {
        this.st = Math.min(this.st, 0.35);
        this.vx = p.dir * 90;
        return 'hit';
      }
      // Dayanıklılık (kıdemliler ve ağa)
      if (this.poiseMax > 0) {
        this.poise -= dmg * (p.poise || 1);
        if (this.poise > 0) {
          this.vx += p.dir * 40;
          return 'hit';
        }
        this.poise = this.poiseMax;
        this.stagger(1.3);
        K.fx.text(this.x, this.y - 190 * this.look.scale, 'SARSILDI!', '#ffcf6a', 22, 0.9);
        return 'hit';
      }
      this.interrupt();
      if (p.launch || !this.onGround || p.float || p.spike) {
        this.onGround = false;
        if (p.spike) this.vy = 1100;
        else if (p.launch) this.vy = -p.launch;
        else this.vy = -320;
        this.vx = p.dir * Math.min(p.kb, 260) * (p.launch ? 0.3 : 0.6);
        this.hardFall = p.knockdown || p.spike || this.hardFall;
        this.setState('air');
      } else if (p.knockdown) {
        this.vx = p.dir * p.kb;
        this.vy = -360;
        this.onGround = false;
        this.hardFall = true;
        this.setState('air');
      } else {
        this.vx = p.dir * p.kb;
        this.stunT = p.stun || 0.3;
        this.setState('hurt');
      }
      return 'hit';
    }
    interrupt() {
      this.move = null;
      this.releaseToken();
    }
    stagger(t) {
      this.interrupt();
      this.stunT = t;
      this.setState('stagger');
    }
    die(p) {
      this.dead = true;
      this.interrupt();
      this.hp = 0;
      this.vx = p.dir * Math.max(p.kb || 0, 320);
      this.vy = -460;
      this.onGround = false;
      this.deadT = 0;
      this.setState('dead');
      K.game.onKill(this);
    }

    // ---- Döngü ----
    update(dt) {
      this.st += dt;
      this.anim += dt;
      this.flash = Math.max(0, this.flash - dt);
      if (this.cd > 0) this.cd -= dt;
      const pl = this.player;
      const dx = pl.x - this.x;
      const adx = Math.abs(dx);
      let grav = 1;

      if (this.dead) {
        this.deadT += dt;
        if (this.onGround) this.vx = K.approach(this.vx, 0, 900 * dt);
        if (this.deadT > 2.2) this.remove = true;
      } else {
        switch (this.state) {
          case 'enter': {
            const b = K.game.bounds;
            this.facing = this.x < (b.min + b.max) / 2 ? 1 : -1;
            this.vx = this.facing * this.speed * 1.2;
            if (this.x > b.min + 40 && this.x < b.max - 40) this.setState('approach');
            break;
          }
          case 'approach':
            this.think(dt, dx, adx);
            break;
          case 'hurt':
            this.vx = K.approach(this.vx, 0, 1500 * dt);
            if (this.st > this.stunT) this.setState('approach');
            break;
          case 'stagger':
            this.vx = K.approach(this.vx, 0, 1500 * dt);
            if (this.st > this.stunT) this.setState('approach');
            break;
          case 'air':
            grav = this.vy < 0 ? 1 : 0.75;
            break;
          case 'down':
            this.vx = K.approach(this.vx, 0, 1500 * dt);
            if (this.st > 0.75) this.setState('getup');
            break;
          case 'getup':
            if (this.st > 0.4) this.setState('approach');
            break;
          default:
            if (this.move) this.runMove(dt, dx);
            break;
        }
      }

      if (!this.onGround || this.vy < 0) this.vy += K.GRAV * grav * dt;
      this.x += this.vx * dt;
      this.y += this.vy * dt;
      if (this.y >= K.GROUND) {
        this.y = K.GROUND;
        if (!this.onGround) {
          this.onGround = true;
          if (this.vy > 600) {
            K.fx.dust(this.x, K.GROUND, 8, 1.4);
            if (this.vy > 900) K.cam.shake(3, 0.06);
          }
          if (!this.dead && this.state === 'air') {
            this.setState(this.hardFall ? 'down' : 'approach');
            this.hardFall = false;
          }
        }
        this.vy = 0;
      } else if (this.y < K.GROUND - 0.5) this.onGround = false;
      if (this.state !== 'enter' && !this.dead) {
        const b = K.game.bounds;
        this.x = K.clamp(this.x, b.min + 20, b.max - 20);
      }
      this.computePose();
    }

    // Sıradan yeniçeri yapay zekâsı
    think(dt, dx, adx) {
      const G = K.game;
      const pl = this.player;
      this.facing = K.sign(dx);
      if (pl.dead) {
        this.vx = K.approach(this.vx, 0, 1000 * dt);
        return;
      }
      if (this.T.gunner) return this.thinkGunner(dt, dx, adx);
      const range = this.T.range;
      const wantAttack = this.cd <= 0 && (this.token || G.tokens < G.maxTokens);
      if (wantAttack) {
        if (adx < range) {
          if (!this.token) {
            this.token = true;
            G.tokens++;
          }
          this.startMove(this.T.moves.slash);
          return;
        }
        this.vx = K.approach(this.vx, this.facing * this.speed, 1800 * dt);
      } else {
        // Çevrede dolaş
        const want = pl.x - this.facing * this.ring;
        const diff = want - this.x;
        const sp = Math.abs(diff) > 20 ? K.sign(diff) * this.speed * 0.55 : 0;
        this.vx = K.approach(this.vx, sp + Math.sin(this.anim * 1.7 + this.jitter) * 30, 1200 * dt);
      }
    }
    thinkGunner(dt, dx, adx) {
      const G = K.game;
      if (adx < 90 && this.cd <= 0 && G.tokens < G.maxTokens + 1) {
        this.startMove(this.T.moves.slash);
        return;
      }
      const want = K.clamp(adx, 320, 560);
      const diff = adx - want;
      let sp = 0;
      if (Math.abs(diff) > 30) sp = diff > 0 ? this.facing * this.speed * 0.7 : -this.facing * this.speed * 0.8;
      this.vx = K.approach(this.vx, sp, 1200 * dt);
      if (this.cd <= 0 && adx < 760 && adx > 180) {
        this.startMove(AIMSHOT);
      }
    }

    // ---- Hamle yürütücü ----
    startMove(m) {
      this.move = m;
      this.moveHit = new Set();
      this.swung = false;
      this.setState('m_wind');
      if (m.onStart) m.onStart(this);
    }
    runMove(dt, dx) {
      const m = this.move;
      const s = this.state;
      const sp = this.speedMul || 1;
      if (s === 'm_wind') {
        this.vx = K.approach(this.vx, 0, 2000 * dt);
        if (m.track) this.facing = K.sign(dx);
        if (m.onWind) m.onWind(this, dt);
        if (this.st >= m.windup / sp) {
          this.setState('m_act');
          K.sfx(m.sfx || 'swing', 0.8);
          if (m.arc) {
            const sc = this.look.scale;
            K.fx.add({ type: 'arc', x: this.x + this.facing * 6 * sc, y: this.y - 92 * sc, dir: this.facing, a0: m.arc[0], a1: m.arc[1], r: m.arc[2] * sc, width: 9, life: 0.14, color: 'rgba(255,200,170,0.85)' });
          }
          if (m.onAct) m.onAct(this);
        }
      } else if (s === 'm_act') {
        if (m.lunge) this.vx = this.facing * m.lunge;
        if (m.dash) this.vx = this.facing * m.dash;
        if (m.onActive) m.onActive(this, dt);
        if (m.dmg && m.reach) {
          const sc = this.look.scale;
          const r = this.facing > 0
            ? { x: this.x - (m.back || 0) * sc, y: this.y + m.hy * sc, w: (m.reach + (m.back || 0)) * sc, h: m.hh * sc }
            : { x: this.x - m.reach * sc, y: this.y + m.hy * sc, w: (m.reach + (m.back || 0)) * sc, h: m.hh * sc };
          K.strike('enemy', r, { dmg: m.dmg, dir: this.facing, kb: m.kb, stun: m.stun, knockdown: m.knockdown, src: this }, this.moveHit);
        }
        if (this.st >= m.active) {
          this.setState('m_rec');
          if (m.onEnd) m.onEnd(this);
        }
      } else if (s === 'm_rec') {
        this.vx = K.approach(this.vx, 0, 2200 * dt);
        if (this.st >= m.recover / sp) {
          const next = m.next;
          this.releaseToken();
          if (next && !this.player.dead) {
            this.startMove(next);
            return;
          }
          this.move = null;
          this.cd = m.cd ? K.rand(m.cd[0], m.cd[1]) : K.rand(0.9, 1.9);
          this.setState('approach');
        }
      }
    }

    computePose() {
      const T = this.T;
      const base = this.basePose || T.base;
      let p = base;
      const t = this.st;
      const m = this.move;
      switch (this.state) {
        case 'enter':
        case 'approach':
          p = Math.abs(this.vx) > 25 ? K.runPose(this.anim * 11, base) : Object.assign({}, base, { bob: Math.sin(this.anim * 3) * 1.5 });
          if (Math.abs(this.vx) > 25 && K.sign(this.vx) !== this.facing) p.lean = -0.05;
          break;
        case 'm_wind':
          p = K.lerpPose(base, m.wind, K.ease(Math.min(1, t / (m.windup / (this.speedMul || 1)) * 1.4)));
          break;
        case 'm_act':
          p = K.lerpPose(m.wind, m.strike, Math.min(1, t / (Math.max(0.05, m.active) * 0.6)));
          break;
        case 'm_rec':
          p = K.lerpPose(m.strike, base, K.clamp(t / (m.recover / (this.speedMul || 1)), 0, 1) ** 2);
          break;
        case 'hurt':
        case 'stagger':
          p = P({ lean: -0.4, au: -0.4, af: 0.4, wa: -1.6, ft: 0.4, bt: -0.4, head: -0.3 });
          if (this.state === 'stagger') p.bob = 6 + Math.sin(this.anim * 12) * 2;
          break;
        case 'air':
          p = P({ lean: -0.3, au: 2.2, af: 2.6, wa: -1.8, ft: 0.6, fs: -0.4, bt: 0.2, bs: -0.6, rot: K.clamp(this.vy / 1800, -0.5, 0.6) - 0.4 });
          break;
        case 'down':
          p = P({ rot: -Math.PI / 2, lean: -0.2, au: 2.0, af: 2.4, wa: -2, ft: 0.3, bt: 0.1, bob: 8 });
          break;
        case 'getup':
          p = K.lerpPose(P({ rot: -Math.PI / 2, bob: 8 }), base, t / 0.4);
          break;
        case 'dead':
          p = P({ rot: this.onGround ? -Math.PI / 2 : -Math.PI / 2 * Math.min(1, this.deadT * 3), lean: -0.2, au: 2.4, af: 2.8, wa: -2.5, ft: 0.4, bt: 0.1, bob: 8, head: -0.4 });
          break;
      }
      this.pose = p;
    }
    glint() {
      if (this.state !== 'm_wind' || !this.move) return 0;
      const w = this.move.windup / (this.speedMul || 1);
      return this.st > w * 0.45 ? K.clamp((this.st - w * 0.45) / (w * 0.55), 0, 1) : 0;
    }
    rig(ctx) {
      const g = this.glint();
      const extra = { glint: g, glintLen: this.T.weapon === 'mizrak' ? 120 : 45, offhand: this.T.offhand, eyeGlow: this.dead ? 0 : this.isBoss ? Math.max(0.35, g) : g > 0.2 ? g : 0 };
      if (this.look.cape) extra.cape = { t: this.anim, v: Math.min(1, Math.abs(this.vx) / 500) };
      K.drawRig(ctx, this.x, this.y, this.facing, this.pose, this.look, this.dead ? null : this.weaponNow || this.T.weapon, this.flash > 0, extra);
    }
    get fadeAlpha() {
      return this.dead && this.deadT > 1.6 ? Math.max(0, 1 - (this.deadT - 1.6) / 0.6) : 1;
    }
    drawFx(ctx) {
      if (this.aimLine && !this.dead) this.drawAim(ctx);
    }
    overlay(ctx) {
      const s = this.look.scale;
      if (!this.isBoss && !this.dead && this.hp < this.maxHp && this.poiseMax === 0) {
        const w = 46, x = Math.round(this.x - w / 2), y = Math.round(this.y - 140 * s);
        ctx.fillStyle = 'rgba(12,6,10,0.85)';
        ctx.fillRect(x - 1, y - 1, w + 2, 6);
        ctx.fillStyle = '#c42a28';
        ctx.fillRect(x, y, Math.round((w * this.hp) / this.maxHp), 4);
        ctx.strokeStyle = 'rgba(201,162,74,0.7)';
        ctx.lineWidth = 1;
        ctx.strokeRect(x - 1.5, y - 1.5, w + 3, 7);
        ctx.fillStyle = '#c9a24a';
        ctx.beginPath();
        ctx.moveTo(x - 9, y + 2);
        ctx.lineTo(x - 5, y - 2);
        ctx.lineTo(x - 1, y + 2);
        ctx.lineTo(x - 5, y + 6);
        ctx.fill();
      }
    }
    drawAim(ctx) {
      const a = this.aimLine;
      ctx.strokeStyle = `rgba(255,70,50,${0.35 + a.k * 0.6})`;
      ctx.lineWidth = 3 + a.k * 3;
      ctx.setLineDash([10, 8]);
      ctx.beginPath();
      ctx.moveTo(a.x, a.y);
      ctx.lineTo(a.x + this.facing * 900, a.y);
      ctx.stroke();
      ctx.setLineDash([]);
    }
  }

  // Tüfekli nişan atışı
  const AIMSHOT = {
    windup: 0.9, active: 0.05, recover: 1.0, sfx: 'shot', cd: [2.6, 3.6],
    wind: AIM, strike: P({ lean: -0.15, au: 1.9, af: 2.1, wa: -0.4, ft: 0.3, bt: -0.4 }),
    onStart(e) { e.weaponNow = 'tufek'; },
    onWind(e, dt) {
      const s = e.look.scale;
      e.aimLine = { x: e.x + e.facing * 70 * s, y: e.y - 95 * s, k: e.st / (this.windup / (e.speedMul || 1)) };
    },
    onAct(e) {
      e.aimLine = null;
      fireEnemyBullet(e, 36);
    },
    onEnd(e) { e.aimLine = null; },
  };
  function fireEnemyBullet(e, dmg) {
    const s = e.look.scale;
    const x = e.x + e.facing * 80 * s, y = e.y - 95 * s;
    K.game.addProj(new Proj({ kind: 'ebullet', team: 'enemy', x, y, vx: e.facing * 1500, vy: 0, dmg, kb: 360, stun: 0.4, knockdown: true }));
    K.fx.smoke(x, y, e.facing, 10);
    K.fx.sparks(x, y, e.facing, 6, '#ffcf6a', 600);
    K.cam.shake(3, 0.08);
  }
  K.fireEnemyBullet = fireEnemyBullet;

  // ================= Kıdemliler =================
  const BOSSES = {};
  K.BOSSES = BOSSES;

  BOSSES.odabasi = {
    name: 'Odabaşı Deli Hüseyin', title: 'Kıdemli Yeniçeri', hp: 520, poise: 70, speed: 270, look: 'odabasi', weapon: 'yatagan', offhand: 'yatagan', base: P({ lean: 0.15, ft: 0.35, fs: 0.1, bt: -0.35, bs: -0.4, au: 0.6, af: 1.2, wa: -0.4, bu: 0.6, bf: 1.0, wb: -0.2 }),
    brain(e, dt, dx, adx) {
      if (e.cd > 0) {
        const want = K.game.player.x - e.facing * 200;
        e.vx = K.approach(e.vx, K.sign(want - e.x) * (Math.abs(want - e.x) > 30 ? e.speed * 0.6 : 0), 1500 * dt);
        return;
      }
      if (adx < 110) return e.startMove(e.moves.combo1);
      if (adx > 260 && Math.random() < 0.5) return e.startMove(e.moves.dash);
      e.vx = K.approach(e.vx, e.facing * e.speed, 2000 * dt);
    },
    moves() {
      const w = (x) => Object.assign({}, SLASH, x);
      const c3 = w({ windup: 0.2, dmg: 24, kb: 380, knockdown: true, lunge: 300, recover: 0.6, cd: [0.6, 1.2], wind: P({ lean: -0.1, au: 2.8, af: 3.0, wa: -2.0, bu: 2.8, bf: 3.0, wb: -2.0 }), strike: P({ lean: 0.45, au: 0.9, af: 0.5, wa: 0.8, bu: 0.9, bf: 0.5, wb: 0.8, ft: 0.7, fs: 0.3, bt: -0.6, bs: -0.7 }), arc: [-2.2, 1.0, 80] });
      const c2 = w({ windup: 0.18, recover: 0.08, dmg: 16, lunge: 260, next: c3, wind: P({ au: -0.6, af: -0.2, wa: 2.4, bu: 2.6, bf: 2.9, wb: -2.0 }), strike: P({ lean: 0.3, au: 2.3, af: 2.6, wa: -1.0, bu: 1.0, bf: 0.7, wb: 0.5 }), arc: [2.2, -1.1, 70] });
      const c1 = w({ windup: 0.38, recover: 0.08, dmg: 16, lunge: 240, next: c2, track: true, wind: P({ lean: -0.1, au: 2.7, af: 3.0, wa: -2.0, bu: -0.6, bf: -0.2, wb: 2.4 }), strike: P({ lean: 0.35, au: 1.0, af: 0.7, wa: 0.6, bu: 2.3, bf: 2.6, wb: -1.0 }) });
      const dash = { windup: 0.55, active: 0.32, recover: 0.6, dmg: 26, reach: 90, hy: -120, hh: 100, kb: 420, knockdown: true, dash: 820, track: true, sfx: 'dash', cd: [0.8, 1.4], wind: P({ lean: -0.3, ft: 0.6, fs: 0.9, bt: -0.6, bs: -0.2, bob: 14, au: -1.2, af: -1.4, wa: 2.8, bu: -1.2, bf: -1.4, wb: 2.8 }), strike: P({ lean: 0.6, au: 1.5, af: 1.5, wa: 0.1, bu: 1.4, bf: 1.4, wb: 0.2, ft: 1.0, fs: 0.3, bt: -0.8, bs: -0.9 }) };
      return { combo1: c1, dash };
    },
  };

  BOSSES.tufekcibasi = {
    name: 'Tüfekçibaşı Sarı Ahmed', title: 'Kıdemli Yeniçeri', hp: 460, poise: 50, speed: 230, look: 'tufekcibasi', weapon: 'tufek', base: GUN, escorts: 4,
    brain(e, dt, dx, adx) {
      if (adx < 120 && e.cd <= 0) return e.startMove(e.moves.kick);
      const want = 460;
      const diff = adx - want;
      let sp = 0;
      if (Math.abs(diff) > 40) sp = diff > 0 ? e.facing * e.speed * 0.7 : -e.facing * e.speed;
      e.vx = K.approach(e.vx, sp, 1500 * dt);
      if (e.cd <= 0) e.startMove(Math.random() < 0.55 ? e.moves.shot : e.moves.bomb);
    },
    moves() {
      const shot = Object.assign({}, AIMSHOT, { windup: 0.7, recover: 0.5, cd: [0.7, 1.3], onAct(e) { e.aimLine = null; fireEnemyBullet(e, 42); } });
      const bomb = {
        windup: 0.45, active: 0.05, recover: 0.4, sfx: 'throw', cd: [0.8, 1.4],
        wind: P({ lean: -0.2, au: -2.4, af: -2.0, wa: -0.3 }), strike: P({ lean: 0.3, au: 2.2, af: 1.8, wa: -0.6 }),
        onStart(e) { e.weaponNow = null; },
        onAct(e) {
          const pl = K.game.player;
          const x0 = e.x + e.facing * 30, y0 = e.y - 120;
          const vy = -650, drop = K.GROUND - 8 - y0;
          const t = (-vy + Math.sqrt(vy * vy + 2 * 1500 * drop)) / 1500;
          const tx = pl.x + pl.vx * 0.3;
          K.game.addProj(new Proj({ kind: 'bomb', team: 'enemy', x: x0, y: y0, vx: (tx - x0) / t, vy, life: 4 }));
        },
        onEnd(e) { e.weaponNow = 'tufek'; },
      };
      const kick = {
        windup: 0.3, active: 0.1, recover: 0.25, dmg: 16, reach: 85, hy: -90, hh: 70, kb: 460, stun: 0.35, cd: [0.3, 0.6],
        wind: P({ lean: -0.3, ft: 1.2, fs: 0.6, au: 0.9, af: 1.2, wa: -1.2 }), strike: P({ lean: -0.4, ft: 1.7, fs: 1.6, au: 0.9, af: 1.2, wa: -1.2 }),
        onEnd(e) {
          e.vx = -e.facing * 520;
          e.vy = -560;
          e.onGround = false;
        },
      };
      return { shot, bomb, kick };
    },
  };

  BOSSES.topuzcu = {
    name: 'Topuzcu Koca Mehmed', title: 'Kıdemli Yeniçeri', hp: 880, poise: 150, speed: 130, look: 'topuzcu', weapon: 'topuz', base: P({ lean: 0.12, ft: 0.3, fs: 0.1, bt: -0.3, bs: -0.35, au: 0.2, af: 0.6, wa: -1.1 }), shield: true,
    brain(e, dt, dx, adx) {
      if (e.cd > 0) {
        e.vx = K.approach(e.vx, e.facing * e.speed * 0.5, 800 * dt);
        return;
      }
      if (adx < 150) return e.startMove(Math.random() < 0.55 ? e.moves.swing : e.moves.smash);
      if (adx > 380 && Math.random() < 0.6) return e.startMove(e.moves.smash);
      e.vx = K.approach(e.vx, e.facing * e.speed, 1200 * dt);
    },
    moves() {
      const swing = { windup: 0.6, active: 0.14, recover: 0.6, dmg: 34, reach: 125, back: 10, hy: -140, hh: 130, kb: 560, knockdown: true, stun: 0.5, lunge: 120, sfx: 'heavy', cd: [0.9, 1.5], track: true, wind: P({ lean: -0.25, au: -1.6, af: -1.9, wa: 3.0 }), strike: P({ lean: 0.4, au: 1.5, af: 1.4, wa: 0.2, ft: 0.6, fs: 0.2, bt: -0.5, bs: -0.6 }), arc: [-2.9, 0.4, 100] };
      const smash = {
        windup: 0.85, active: 0.1, recover: 0.8, dmg: 42, reach: 120, hy: -110, hh: 110, kb: 420, knockdown: true, sfx: 'heavy', cd: [1.0, 1.6], track: true,
        wind: P({ lean: -0.3, au: 3.0, af: 3.1, wa: -1.6, bob: -4 }), strike: P({ lean: 0.55, au: 0.9, af: 0.3, wa: 1.3, ft: 0.8, fs: 0.4, bt: -0.6, bs: -0.8, bob: 12 }),
        onAct(e) {
          const x = e.x + e.facing * 110 * e.look.scale;
          K.fx.add({ type: 'ring', x, y: K.GROUND, size: 200, life: 0.4, color: 'rgba(230,200,150,0.9)' });
          K.fx.dust(x, K.GROUND, 14, 2);
          K.cam.shake(10, 0.25);
          K.sfx('boom', 0.7);
          for (const d of [-1, 1]) K.game.addProj(new Proj({ kind: 'wave', team: 'enemy', x: x + d * 30, y: K.GROUND, vx: d * 520, dmg: 24, life: 3 }));
        },
      };
      return { swing, smash };
    },
  };

  BOSSES.bayraktar = {
    name: 'Bayraktar Arslan', title: 'Kıdemli Yeniçeri', hp: 620, poise: 80, speed: 220, look: 'bayraktar', weapon: 'bayrak', base: SPEAR, gates: [0.75, 0.4],
    brain(e, dt, dx, adx) {
      const hpk = e.hp / e.maxHp;
      if (e.forceCall || (e.rallies < 2 && hpk < (e.rallies === 0 ? 0.75 : 0.4) && e.cd <= 0)) return e.startMove(e.moves.rally);
      if (e.cd > 0) {
        const want = K.game.player.x - e.facing * 230;
        e.vx = K.approach(e.vx, K.sign(want - e.x) * (Math.abs(want - e.x) > 30 ? e.speed * 0.5 : 0), 1400 * dt);
        return;
      }
      if (adx < 175) return e.startMove(Math.random() < 0.6 ? e.moves.thrust : e.moves.sweep);
      e.vx = K.approach(e.vx, e.facing * e.speed, 1600 * dt);
    },
    moves() {
      const thrust2 = Object.assign({}, THRUST, { windup: 0.22, dmg: 24, reach: 175, kb: 380, knockdown: true, cd: [0.7, 1.2] });
      const thrust = Object.assign({}, THRUST, { windup: 0.45, recover: 0.12, reach: 175, next: thrust2, track: true });
      const sweep = { windup: 0.5, active: 0.14, recover: 0.5, dmg: 28, reach: 170, back: 80, hy: -120, hh: 110, kb: 420, knockdown: true, cd: [0.8, 1.3], sfx: 'heavy', wind: P({ lean: -0.25, au: -0.6, af: -0.3, wa: -2.5 }), strike: P({ lean: 0.35, au: 1.3, af: 1.1, wa: 0.45, ft: 0.7, fs: 0.3, bt: -0.6, bs: -0.7 }), arc: [-2.5, 0.6, 150] };
      const rally = {
        windup: 1.0, active: 0.1, recover: 0.6, sfx: 'horn', cd: [0.6, 1.0],
        wind: P({ lean: -0.1, au: 2.6, af: 2.9, wa: -1.55, bob: -2 }), strike: P({ lean: 0.1, au: 2.7, af: 2.9, wa: -1.57 }),
        onStart(e) { K.fx.text(e.x, e.y - 200, '“Yetişin yoldaşlar!”', '#ffd9a0', 20, 1.4); },
        onAct(e) {
          e.rallies++;
          e.forceCall = false;
          K.game.encounter.summon(['kilicli', 'kilicli']);
        },
      };
      return { thrust, sweep, rally };
    },
  };

  BOSSES.aga = {
    name: 'Yeniçeri Ağası Kara Murad', title: 'Ağa', hp: 2100, poise: 210, speed: 240, look: 'aga', weapon: 'kilic', base: P({ lean: 0.1, ft: 0.3, fs: 0.1, bt: -0.3, bs: -0.35, au: 0.5, af: 1.1, wa: -0.8 }), isAga: true, gates: [0.6, 0.3],
    brain(e, dt, dx, adx) {
      const hpk = e.hp / e.maxHp;
      e.speedMul = hpk < 0.3 ? 1.3 : 1;
      if (e.forceCall || (e.summons < 2 && hpk < (e.summons === 0 ? 0.6 : 0.3) && e.cd <= 0)) return e.startMove(e.moves.summon);
      if (e.cd > 0) {
        const want = K.game.player.x - e.facing * 240;
        e.vx = K.approach(e.vx, K.sign(want - e.x) * (Math.abs(want - e.x) > 30 ? e.speed * 0.45 : 0), 1200 * dt);
        return;
      }
      const r = Math.random();
      if (hpk < 0.6 && adx > 300 && r < 0.3) return e.startMove(e.moves.pistol);
      if (adx < 160) return e.startMove(r < 0.65 ? e.moves.combo1 : e.moves.leap);
      if (adx > 420) return e.startMove(r < 0.5 ? e.moves.charge : e.moves.leap);
      if (r < 0.02) return e.startMove(e.moves.charge);
      e.vx = K.approach(e.vx, e.facing * e.speed, 1600 * dt);
    },
    moves() {
      const c3 = { windup: 0.38, active: 0.12, recover: 0.7, dmg: 40, reach: 165, back: 20, hy: -190, hh: 190, kb: 620, knockdown: true, lunge: 260, sfx: 'heavy', cd: [0.6, 1.1], wind: P({ lean: -0.3, au: -2.7, af: -2.9, wa: -2.2 }), strike: P({ lean: 0.5, au: 0.9, af: 0.5, wa: 0.9, ft: 0.8, fs: 0.4, bt: -0.6, bs: -0.7, bob: 5 }), arc: [-2.6, 1.1, 110] };
      const c2 = { windup: 0.3, active: 0.11, recover: 0.1, dmg: 28, reach: 150, hy: -170, hh: 160, kb: 260, lunge: 240, next: c3, track: true, wind: P({ lean: 0.2, au: -0.5, af: -0.1, wa: 2.4 }), strike: P({ lean: 0.15, au: 2.3, af: 2.6, wa: -1.0 }), arc: [2.2, -1.1, 95] };
      const combo1 = { windup: 0.55, active: 0.11, recover: 0.1, dmg: 28, reach: 150, hy: -170, hh: 160, kb: 260, lunge: 240, next: c2, track: true, wind: P({ lean: 0, au: 2.6, af: 2.9, wa: -2.3 }), strike: P({ lean: 0.35, au: 1.0, af: 0.6, wa: 0.55, ft: 0.55, fs: 0.2, bt: -0.45, bs: -0.6 }), arc: [-2.0, 0.7, 95] };
      const charge = { windup: 0.65, active: 0.6, recover: 0.7, dmg: 38, reach: 110, hy: -170, hh: 160, kb: 560, knockdown: true, dash: 940, track: true, sfx: 'dash', cd: [0.7, 1.2], wind: P({ lean: -0.35, ft: 0.6, fs: 0.9, bt: -0.6, bs: -0.2, bob: 12, au: 1.4, af: 1.5, wa: 0 }), strike: P({ lean: 0.6, au: 1.5, af: 1.55, wa: 0, ft: 1.0, fs: 0.3, bt: -0.8, bs: -0.9 }),
        onActive(e) {
          const b = K.game.bounds;
          if (e.x < b.min + 60 || e.x > b.max - 60) e.st = 99;
          if (Math.random() < 0.5) K.fx.dust(e.x - e.facing * 30, K.GROUND, 1);
        },
      };
      const leap = {
        windup: 0.45, active: 0.75, recover: 0.65, sfx: 'jump', cd: [0.6, 1.1], track: true,
        wind: P({ lean: 0.2, ft: 0.7, fs: 1.1, bt: -0.5, bs: -0.1, bob: 16, au: 2.6, af: 2.9, wa: -1.8 }), strike: P({ lean: 0.2, au: 2.8, af: 3.0, wa: -1.6, ft: 0.8, fs: -0.3, bt: -0.2, bs: -1.0 }),
        onAct(e) {
          const pl = K.game.player;
          const t = 0.72;
          e.vx = (pl.x - e.x) / t;
          e.vy = -0.5 * K.GRAV * t;
          e.onGround = false;
          e.leaping = true;
        },
        onActive(e) {
          e.facing = K.sign(e.vx || e.facing);
          if (e.leaping && e.onGround && e.st > 0.1) {
            e.leaping = false;
            e.vx = 0;
            const R = 170;
            K.strike('enemy', { x: e.x - R, y: e.y - 120, w: R * 2, h: 120 }, { dmg: 44, dir: K.sign(K.game.player.x - e.x), kb: 460, knockdown: true, stun: 0.6, src: e }, new Set());
            K.fx.add({ type: 'ring', x: e.x, y: K.GROUND, size: 240, life: 0.45, color: 'rgba(255,210,160,0.95)' });
            K.fx.dust(e.x, K.GROUND, 20, 2.5);
            K.cam.shake(13, 0.3);
            K.sfx('boom', 0.8);
            if (e.hp / e.maxHp < 0.6) for (const d of [-1, 1]) K.game.addProj(new Proj({ kind: 'wave', team: 'enemy', x: e.x + d * 40, y: K.GROUND, vx: d * 560, dmg: 22, life: 3 }));
            e.st = e.move.active;
          }
        },
      };
      const pistol = Object.assign({}, AIMSHOT, { windup: 0.6, recover: 0.5, cd: [0.6, 1.0], onStart(e) { e.weaponNow = 'tabanca'; }, onAct(e) { e.aimLine = null; fireEnemyBullet(e, 40); }, onEnd(e) { e.aimLine = null; e.weaponNow = null; } });
      const summon = {
        windup: 0.9, active: 0.1, recover: 0.5, sfx: 'horn', cd: [0.5, 0.9],
        wind: P({ lean: -0.1, au: 2.6, af: 2.9, wa: -1.55 }), strike: P({ au: 2.7, af: 2.9, wa: -1.57 }),
        onStart(e) { K.fx.text(e.x, e.y - 250, '“Ocak, bana!”', '#ffd9a0', 22, 1.4); },
        onAct(e) {
          e.summons++;
          e.forceCall = false;
          K.game.encounter.summon(e.summons === 1 ? ['kilicli', 'mizrakli', 'kilicli'] : ['kilicli', 'tufekli', 'mizrakli']);
        },
      };
      return { combo1, charge, leap, pistol, summon };
    },
  };

  class Boss extends Enemy {
    constructor(key, x) {
      const B = BOSSES[key];
      const T = { hp: B.hp, speed: B.speed, look: B.look, weapon: B.weapon, base: B.base, poise: B.poise, shield: B.shield, offhand: B.offhand, moves: {} };
      super(T, x, { counted: false, typeName: key });
      this.B = B;
      this.isBoss = true;
      this.name = B.name;
      this.title = B.title;
      this.moves = B.moves();
      this.rallies = 0;
      this.summons = 0;
      this.cd = 1.0;
      this.speed = B.speed;
    }
    think(dt, dx, adx) {
      this.facing = K.sign(dx);
      if (this.player.dead) {
        this.vx = K.approach(this.vx, 0, 1000 * dt);
        return;
      }
      this.weaponNow = this.B.weapon;
      this.B.brain(this, dt, dx, adx);
    }
    // Çağrı eşikleri: can bu eşiklerin altına çağrı yapılmadan inemez, böylece 100 yeniçerinin hepsi gelir
    takeHit(p) {
      const gates = this.B.gates;
      const done = this.B.isAga ? this.summons : this.rallies;
      const gate = gates && done < gates.length ? gates[done] * this.maxHp : -Infinity;
      if (!this.dead && this.hp - p.dmg < gate) {
        p = Object.assign({}, p, { dmg: Math.max(1, Math.floor(this.hp - gate)) });
        this.forceCall = true;
      }
      return super.takeHit(p);
    }
  }

  K.Enemy = Enemy;
  K.Boss = Boss;
})(window.K);
