'use strict';
// Karamani: hareket, kombolar, silahlar
(function (K) {
  K.defineLook('karamani', {
    scale: 1.06,
    skin: '#b48060',
    beard: '#1c1410',
    beardGray: true,
    mustache: '#1c1410',
    hat: 'wrap',
    hatColor: '#d0c6b2',
    coat: '#3a322c',
    trim: '#8a6a3a',
    sash: '#6a1418',
    pants: '#221e1e',
    boots: '#36281c',
    mail: true,
    cape: '#4a0c10',
    capeLining: '#6a2a1c',
    capeLen: 82,
    bracer: true,
    baldric: true,
    furMantle: '#2e241c',
    pauldron: true,
    tattered: true,
    age: true,
    scar: true,
    brow: '#1a120c',
    quiver: true,
    scabbard: true,
    dagger: true,
  });

  const P = K.pose;
  const AIR_LEGS = { ft: 0.75, fs: -0.25, bt: -0.15, bs: -1.0 };
  const STANCE = P({ lean: 0.18, bob: 5, ft: 0.6, fs: 0.1, bt: -0.35, bs: -0.48, au: 0.55, af: 1.25, wa: -0.45, bu: -0.5, bf: 0.1 });
  const SPEAR_STANCE = P({ lean: 0.18, bob: 5, ft: 0.6, fs: 0.1, bt: -0.35, bs: -0.48, au: 0.8, af: 1.4, wa: -0.08 });

  // Saldırı tablosu. Süreler saniye, mesafeler piksel.
  const A = {};
  const def = (id, o) => (A[id] = Object.assign({ id, startup: 0.08, active: 0.09, recovery: 0.2, dmg: 15, reach: 105, back: 10, hy: -125, hh: 105, kb: 150, launch: 0, stun: 0.38, knockdown: false, lunge: 0, sfx: 'swing' }, o));

  // Kılıç: yer kombosu
  def('kilic_1', { next: 'kilic_2', lunge: 150, wind: P({ lean: 0, au: 2.5, af: 2.9, wa: -2.3, bu: -0.4, bf: 0.2 }), strike: P({ lean: 0.32, au: 1.0, af: 0.6, wa: 0.55, bu: -0.8, bf: -0.4, ft: 0.55, fs: 0.2, bt: -0.45, bs: -0.6 }), arc: [-2.0, 0.7, 82] });
  def('kilic_2', { next: 'kilic_3', lunge: 150, wind: P({ lean: 0.25, au: -0.5, af: -0.1, wa: 2.4, ft: 0.5, bt: -0.4 }), strike: P({ lean: 0.12, au: 2.3, af: 2.6, wa: -1.0, ft: 0.4, fs: 0.1, bt: -0.4, bs: -0.5 }), arc: [2.2, -1.1, 82] });
  def('kilic_3', { next: 'kilic_4', dmg: 18, lunge: 160, kb: 110, launch: 480, stun: 0.5, wind: P({ lean: -0.05, au: -1.4, af: -1.7, wa: 3.0 }), strike: P({ lean: 0.36, au: 1.6, af: 1.6, wa: 0.05, ft: 0.6, fs: 0.25, bt: -0.5, bs: -0.6 }), arc: [-2.9, 0.25, 92] });
  def('kilic_4', { dmg: 30, reach: 140, hy: -110, hh: 70, startup: 0.11, active: 0.1, recovery: 0.32, kb: 580, knockdown: true, stun: 0.6, lunge: 380, sfx: 'heavy', thrust: true, wind: P({ lean: -0.18, au: -0.4, af: 0.9, wa: 0, ft: 0.1, bt: -0.15 }), strike: P({ lean: 0.5, au: 1.55, af: 1.57, wa: 0, ft: 0.95, fs: 0.3, bt: -0.65, bs: -0.75 }) });
  def('kilic_launch', { dmg: 16, launch: 1000, kb: 40, stun: 0.6, startup: 0.08, active: 0.1, recovery: 0.22, jumpCancel: true, hy: -170, hh: 170, wind: P({ lean: 0.1, au: 0.3, af: 0.4, wa: 1.3, ft: 0.35, fs: 0.35, bt: -0.25, bs: -0.7, bob: 7 }), strike: P({ lean: -0.05, au: 3.0, af: 3.1, wa: -1.5, bob: -4 }), arc: [1.2, -1.6, 86] });
  // Havada
  def('kilic_a1', { next: 'kilic_a2', air: true, dmg: 13, float: true, kb: 60, stun: 0.55, hy: -150, hh: 150, wind: P(Object.assign({ au: 2.5, af: 2.9, wa: -2.3 }, AIR_LEGS)), strike: P(Object.assign({ lean: 0.3, au: 1.0, af: 0.6, wa: 0.55 }, AIR_LEGS)), arc: [-2.0, 0.7, 82] });
  def('kilic_a2', { next: 'kilic_a3', air: true, dmg: 13, float: true, kb: 60, stun: 0.55, hy: -150, hh: 150, wind: P(Object.assign({ au: -0.5, af: -0.1, wa: 2.4 }, AIR_LEGS)), strike: P(Object.assign({ au: 2.3, af: 2.6, wa: -1.0 }, AIR_LEGS)), arc: [2.2, -1.1, 82] });
  def('kilic_a3', { air: true, dmg: 22, spike: true, kb: 220, knockdown: true, stun: 0.6, startup: 0.1, recovery: 0.26, hy: -150, hh: 170, sfx: 'heavy', wind: P(Object.assign({ au: 2.8, af: 3.0, wa: -2.0 }, AIR_LEGS)), strike: P(Object.assign({ lean: 0.4, au: 0.6, af: 0.3, wa: 1.2 }, AIR_LEGS)), arc: [-2.2, 1.3, 96] });
  def('kilic_heavy', { dmg: 40, reach: 145, back: 30, hy: -160, hh: 160, startup: 0.26, active: 0.1, recovery: 0.34, kb: 640, knockdown: true, guardBreak: true, poise: 2, stun: 0.7, lunge: 120, sfx: 'heavy', wind: P({ lean: -0.22, au: -2.7, af: -2.9, wa: -2.2, ft: 0.25, bt: -0.35, bob: 3 }), strike: P({ lean: 0.48, au: 0.9, af: 0.5, wa: 0.9, ft: 0.8, fs: 0.4, bt: -0.6, bs: -0.7, bob: 6 }), arc: [-2.6, 1.1, 104], arcW: 18 });

  // Mızrak
  def('mizrak_1', { next: 'mizrak_2', dmg: 17, reach: 168, hy: -112, hh: 50, kb: 230, lunge: 100, thrust: true, wind: P({ lean: -0.1, au: 0.5, af: 1.25, wa: 0.05, ft: 0.2, bt: -0.3 }), strike: P({ lean: 0.38, au: 1.4, af: 1.55, wa: 0, ft: 0.75, fs: 0.3, bt: -0.6, bs: -0.7 }) });
  def('mizrak_2', { next: 'mizrak_3', dmg: 17, reach: 168, hy: -95, hh: 50, kb: 230, lunge: 100, thrust: true, wind: P({ lean: -0.1, au: 0.4, af: 1.1, wa: 0.3 }), strike: P({ lean: 0.4, au: 1.3, af: 1.4, wa: 0.22, ft: 0.75, fs: 0.3, bt: -0.6, bs: -0.7 }) });
  def('mizrak_3', { dmg: 24, reach: 165, back: 70, hy: -130, hh: 120, startup: 0.12, active: 0.12, recovery: 0.3, kb: 380, knockdown: true, stun: 0.6, sfx: 'heavy', wind: P({ lean: -0.25, au: -0.6, af: -0.3, wa: -2.5 }), strike: P({ lean: 0.35, au: 1.3, af: 1.1, wa: 0.45, ft: 0.7, fs: 0.3, bt: -0.6, bs: -0.7 }), arc: [-2.5, 0.6, 140], arcW: 8 });
  def('mizrak_launch', { dmg: 16, reach: 150, launch: 980, kb: 40, stun: 0.6, jumpCancel: true, hy: -190, hh: 190, wind: P({ lean: 0.05, au: 0.4, af: 0.6, wa: 0.9, bob: 6 }), strike: P({ lean: -0.05, au: 2.4, af: 2.6, wa: -1.15, bob: -4 }), arc: [0.9, -1.2, 130], arcW: 8 });
  def('mizrak_a1', { next: 'mizrak_a2', air: true, dmg: 15, reach: 160, float: true, kb: 70, stun: 0.55, hy: -140, hh: 120, thrust: true, wind: P(Object.assign({ au: 0.5, af: 1.0, wa: 0.4 }, AIR_LEGS)), strike: P(Object.assign({ lean: 0.3, au: 1.3, af: 1.4, wa: 0.35 }, AIR_LEGS)) });
  def('mizrak_a2', { air: true, dmg: 22, reach: 160, spike: true, knockdown: true, kb: 200, stun: 0.6, hy: -140, hh: 160, sfx: 'heavy', wind: P(Object.assign({ au: 2.4, af: 2.6, wa: -1.6 }, AIR_LEGS)), strike: P(Object.assign({ lean: 0.4, au: 1.0, af: 0.8, wa: 0.9 }, AIR_LEGS)), arc: [-1.6, 1.0, 130], arcW: 8 });
  def('mizrak_heavy', { dmg: 30, reach: 160, back: 20, hy: -125, hh: 80, startup: 0.16, active: 0.26, recovery: 0.3, kb: 320, stun: 0.5, dash: 950, guardBreak: true, poise: 2, sfx: 'dash', thrust: true, wind: P({ lean: -0.25, au: 0.3, af: 1.0, wa: 0, ft: 0.0, bt: -0.1, bob: 4 }), strike: P({ lean: 0.55, au: 1.5, af: 1.57, wa: 0, ft: 1.0, fs: 0.4, bt: -0.8, bs: -0.9 }) });

  const RANGED = {
    yay: { name: 'Yay' },
    tabanca: { name: 'Tabanca' },
    cirit: { name: 'Cirit' },
  };
  K.WEAPON_NAMES = { kilic: 'Kılıç', mizrak: 'Mızrak', yay: 'Yay', tabanca: 'Tabanca', cirit: 'Cirit' };

  class Player {
    constructor(x) {
      this.look = K.LOOKS.karamani;
      this.team = 'player';
      this.reset(x);
      this.melee = ['kilic', 'mizrak'];
      this.ranged = ['yay', 'tabanca', 'cirit'];
      this.dur = { kilic: 1, mizrak: 1, yay: 1, tabanca: 1, cirit: 1 };
      this.broken = {};
      this.mi = 0;
      this.ri = 0;
      this.maxCombo = 0;
    }
    reset(x) {
      this.x = x;
      this.y = K.GROUND;
      this.vx = this.vy = 0;
      this.facing = 1;
      this.onGround = true;
      this.maxHp = 420;
      this.hp = this.maxHp;
      this.dead = false;
      this.state = 'idle';
      this.st = 0;
      this.jumps = 0;
      this.airDash = true;
      this.atk = null;
      this.hitSet = new Set();
      this.buf = null;
      this.bufT = 0;
      this.inv = 0;
      this.flash = 0;
      this.combo = 0;
      this.comboT = 0;
      this.akin = 0;
      this.dodgeCd = 0;
      this.pistolReload = 0;
      this.javelins = 3;
      this.javT = 0;
      this.drawAmt = 0;
      this.anim = 0;
      this.ctrl = true;
      this.pose = STANCE;
    }
    get meleeW() { return this.melee[this.mi]; }
    get rangedW() { return this.ranged[this.ri]; }
    hurtbox() {
      if (this.state === 'down') return { x: this.x - 40, y: this.y - 30, w: 80, h: 30 };
      return { x: this.x - 17, y: this.y - 112, w: 34, h: 112 };
    }
    setState(s) {
      this.state = s;
      this.st = 0;
    }
    queue(a) {
      this.buf = a;
      this.bufT = 0.2;
    }
    take(a) {
      if (this.buf === a) {
        this.buf = null;
        return true;
      }
      return false;
    }

    update(dt) {
      const I = K.input, G = K.game;
      this.st += dt;
      this.anim += dt;
      this.inv = Math.max(0, this.inv - dt);
      this.flash = Math.max(0, this.flash - dt);
      this.dodgeCd = Math.max(0, this.dodgeCd - dt);
      this.bufT -= dt;
      if (this.bufT <= 0) this.buf = null;
      this.comboT -= dt;
      if (this.comboT <= 0) this.combo = 0;
      if (this.pistolReload > 0) {
        this.pistolReload -= dt;
        if (this.pistolReload <= 0) {
          K.sfx('clang', 0.6);
          K.fx.text(this.x, this.y - 140, 'Dolduruldu', '#d8e6ff', 14, 0.6);
        }
      }
      if (this.javelins < 3) {
        this.javT += dt;
        if (this.javT >= 3.2) {
          this.javT = 0;
          this.javelins++;
        }
      }

      const canAct = this.ctrl && G.state === 'play' && !this.dead;
      if (canAct) {
        if (I.hit('attack')) this.queue('attack');
        if (I.hit('heavy')) this.queue('heavy');
        if (I.hit('jump')) this.queue('jump');
        if (I.hit('dodge')) this.queue('dodge');
        if (I.hit('super')) this.queue('super');
        if (I.hit('ranged')) this.queue('ranged');
        if (I.hit('swapMelee')) this.swap('melee');
        if (I.hit('swapRanged')) this.swap('ranged');
      }
      const mx = canAct ? (I.held('right') ? 1 : 0) - (I.held('left') ? 1 : 0) : 0;
      let grav = 1;

      switch (this.state) {
        case 'idle':
        case 'run':
        case 'air':
          this.free(dt, mx);
          break;
        case 'attack':
          grav = this.updateAttack(dt, mx);
          break;
        case 'plunge':
          this.vx = K.approach(this.vx, 0, 600 * dt);
          if (this.onGround) this.landPlunge();
          break;
        case 'plungeRec':
          this.vx = 0;
          if (this.st > 0.22) this.setState('idle');
          break;
        case 'bow':
          grav = this.updateBow(dt, mx);
          break;
        case 'shoot':
          this.vx = K.approach(this.vx, 0, 2000 * dt);
          if (this.st > 0.28) this.setState(this.onGround ? 'idle' : 'air');
          break;
        case 'throw':
          this.vx = K.approach(this.vx, 0, 2000 * dt);
          if (!this.thrown && this.st > 0.13) {
            this.thrown = true;
            this.fireJavelin();
          }
          if (this.st > 0.34) this.setState(this.onGround ? 'idle' : 'air');
          break;
        case 'dodge':
          grav = this.onGround ? 1 : 0.15;
          this.vx = this.dodgeDir * (this.st < 0.22 ? 640 : 260);
          if (this.st > 0.3) this.setState(this.onGround ? 'idle' : 'air');
          break;
        case 'hurt':
          this.vx = K.approach(this.vx, 0, 1600 * dt);
          if (this.st > this.stunT) {
            this.inv = 0.3;
            this.setState(this.onGround ? 'idle' : 'air');
          }
          break;
        case 'down':
          if (this.onGround) this.vx = K.approach(this.vx, 0, 1400 * dt);
          if (this.st > 0.7 && this.onGround) {
            this.setState('getup');
            this.inv = 0.9;
          }
          break;
        case 'getup':
          if (this.st > 0.32) this.setState('idle');
          break;
        case 'akin':
          grav = 0;
          this.updateAkin(dt);
          break;
        case 'dead':
          if (this.onGround) this.vx = K.approach(this.vx, 0, 900 * dt);
          break;
        case 'cinematic':
          this.vx = K.approach(this.vx, 0, 2000 * dt);
          break;
      }

      // Fizik
      if (!this.onGround || this.vy < 0) this.vy += K.GRAV * grav * dt;
      this.x += this.vx * dt;
      this.y += this.vy * dt;
      if (this.y >= K.GROUND) {
        if (!this.onGround && this.vy > 500) K.fx.dust(this.x, K.GROUND, 5);
        this.y = K.GROUND;
        this.vy = 0;
        if (!this.onGround) {
          this.onGround = true;
          this.jumps = 0;
          this.airDash = true;
          if (this.state === 'air') this.setState('idle');
          if (this.state === 'attack' && this.atk && this.atk.air) this.setState('idle');
        }
      } else if (this.y < K.GROUND - 0.5) {
        this.onGround = false;
      }
      const b = G.bounds;
      this.x = K.clamp(this.x, b.min + 24, b.max - 24);
      this.computePose();
    }

    free(dt, mx) {
      const run = 400;
      if (this.onGround) this.vx = K.approach(this.vx, mx * run, (mx ? 3600 : 3000) * dt);
      else this.vx = K.approach(this.vx, mx * run, 2200 * dt);
      if (mx) this.facing = mx;

      if (this.take('jump')) {
        if (this.onGround) {
          this.vy = -930;
          this.onGround = false;
          this.jumps = 1;
          K.fx.dust(this.x, K.GROUND, 6);
          K.sfx('jump');
        } else if (this.jumps < 2) {
          this.vy = -820;
          this.jumps = 2;
          K.fx.add({ type: 'ring', x: this.x, y: this.y, size: 50, life: 0.3, color: 'rgba(255,240,200,0.8)' });
          K.sfx('jump');
        }
      }
      if (this.take('dodge') && this.dodgeCd <= 0 && (this.onGround || this.airDash)) {
        this.dodgeDir = mx || this.facing;
        this.facing = this.dodgeDir;
        if (!this.onGround) {
          this.airDash = false;
          this.vy = 0;
        }
        this.inv = 0.26;
        this.dodgeCd = 0.38;
        this.setState('dodge');
        K.sfx('dash');
        K.fx.dust(this.x, this.y, 4);
        return;
      }
      if (this.take('super') && this.akin >= 100) {
        this.startAkin();
        return;
      }
      if (this.take('attack')) {
        const I = K.input;
        const w = this.meleeW;
        if (!this.onGround && I.held('down')) {
          this.startPlunge();
          return;
        }
        if (this.onGround && I.held('up')) return this.startAttack(w + '_launch');
        return this.startAttack(this.onGround ? w + '_1' : w + '_a1');
      }
      if (this.take('heavy')) {
        if (!this.onGround) return this.startPlunge();
        return this.startAttack(this.meleeW + '_heavy');
      }
      if (this.take('ranged')) return this.startRanged();

      if (this.onGround) this.setStateSoft(Math.abs(this.vx) > 30 ? 'run' : 'idle');
      else this.setStateSoft('air');
    }
    setStateSoft(s) {
      if (this.state !== s) {
        this.state = s;
        this.st = 0;
      }
    }

    swap(kind) {
      if (kind === 'melee') this.mi = (this.mi + 1) % this.melee.length;
      else this.ri = (this.ri + 1) % this.ranged.length;
      K.sfx('ui');
      const w = kind === 'melee' ? this.meleeW : this.rangedW;
      K.fx.text(this.x, this.y - 150, K.WEAPON_NAMES[w], '#f3e2b0', 16, 0.7);
    }

    // ---------- Yakın dövüş ----------
    startAttack(id) {
      const a = A[id];
      if (!a) return;
      const I = K.input;
      const mx = (I.held('right') ? 1 : 0) - (I.held('left') ? 1 : 0);
      if (mx) this.facing = mx;
      this.atk = a;
      this.hitSet = new Set();
      this.setState('attack');
      this.swung = false;
      if (a.air) {
        this.vy = Math.min(this.vy, -150);
      }
    }
    updateAttack(dt, mx) {
      const a = this.atk;
      const t = this.st;
      let grav = 1;
      if (a.air) grav = 0.25;
      // İleri atılma
      if (a.dash && t >= a.startup && t < a.startup + a.active) {
        this.vx = this.facing * a.dash;
      } else if (this.onGround) {
        if (t < a.startup + a.active && a.lunge) this.vx = K.approach(this.vx, this.facing * a.lunge, 4000 * dt);
        else this.vx = K.approach(this.vx, 0, 2600 * dt);
      } else {
        this.vx = K.approach(this.vx, 0, 900 * dt);
      }
      if (t >= a.startup && !this.swung) {
        this.swung = true;
        K.sfx(a.sfx);
        this.spawnTrail(a);
      }
      if (t >= a.startup && t < a.startup + a.active) {
        const r = this.attackRect(a);
        const n = K.strike('player', r, this.hitParams(a), this.hitSet);
        if (n && a.spike) this.vy = -200;
      }
      const done = a.startup + a.active;
      // Kombo devamı
      if (t >= done) {
        if (a.next && this.buf === 'attack' && !(K.input.held('up') && this.onGround)) {
          if (!a.air || !this.onGround) {
            this.buf = null;
            this.startAttack(a.next);
            return grav;
          }
        }
        if (this.buf === 'heavy' && this.onGround && !a.id.endsWith('heavy')) {
          this.buf = null;
          this.startAttack(this.meleeW + '_heavy');
          return grav;
        }
        if (this.buf === 'dodge' && this.dodgeCd <= 0) {
          this.setState('idle');
          this.free(dt, mx);
          return grav;
        }
        if (a.jumpCancel && this.buf === 'jump') {
          this.buf = null;
          this.vy = -1000;
          this.onGround = false;
          this.jumps = 1;
          this.setState('air');
          return grav;
        }
        if (this.buf === 'attack' && !a.next && t >= done + a.recovery * 0.5) {
          this.setState(this.onGround ? 'idle' : 'air');
          this.free(dt, mx);
          return grav;
        }
      }
      if (t >= done + a.recovery) this.setState(this.onGround ? 'idle' : 'air');
      return grav;
    }
    attackRect(a) {
      const s = this.look.scale;
      const front = a.reach * s, back = a.back * s;
      return this.facing > 0
        ? { x: this.x - back, y: this.y + a.hy * s, w: front + back, h: a.hh * s }
        : { x: this.x - front, y: this.y + a.hy * s, w: front + back, h: a.hh * s };
    }
    hitParams(a) {
      return {
        dmg: a.dmg,
        dir: this.facing,
        kb: a.kb,
        launch: a.launch,
        float: a.float,
        spike: a.spike,
        stun: a.stun,
        knockdown: a.knockdown,
        guardBreak: a.guardBreak,
        poise: a.poise || 1,
        heavy: a.sfx === 'heavy',
        src: this,
        onHit: (t, r) => this.onDealt(a.dmg, t, r, a),
      };
    }
    spawnTrail(a) {
      const s = this.look.scale;
      const sx = this.x + this.facing * 6 * s, sy = this.y - 95 * s;
      if (a.arc) {
        K.fx.add({ type: 'arc', x: sx, y: sy, dir: this.facing, a0: a.arc[0], a1: a.arc[1], r: a.arc[2] * s, width: a.arcW || 12, life: 0.16, color: this.meleeW === 'mizrak' ? 'rgba(255,236,190,0.85)' : 'rgba(235,245,255,0.95)' });
      }
      if (a.thrust) {
        const y = this.y + (a.hy + a.hh / 2) * s;
        K.fx.add({ type: 'line', x: this.x + this.facing * 30, y, x2: this.x + this.facing * a.reach * s * 1.05, y2: y, size: 8, life: 0.14, color: 'rgba(240,248,255,0.9)' });
      }
    }
    onDealt(dmg, t, r, a) {
      const G = K.game;
      if (r === 'blocked') {
        G.hitstop = Math.max(G.hitstop, 0.04);
        return;
      }
      this.combo++;
      this.comboT = 2.2;
      this.maxCombo = Math.max(this.maxCombo, this.combo);
      this.akin = Math.min(100, this.akin + dmg * 0.16);
      const w = a && a.weapon ? a.weapon : this.meleeW;
      this.wear(w, dmg / 3500);
      const heavy = a && (a.knockdown || a.sfx === 'heavy');
      G.hitstop = Math.max(G.hitstop, heavy ? 0.085 : 0.05);
      K.cam.shake(heavy ? 7 : 3, heavy ? 0.16 : 0.08);
    }
    wear(w, amt) {
      this.dur[w] = Math.max(0.12, this.dur[w] - amt);
    }

    // ---------- Dalış ----------
    startPlunge() {
      this.setState('plunge');
      this.vy = 1500;
      this.vx = this.facing * 80;
      this.hitSet = new Set();
      K.sfx('dash');
    }
    landPlunge() {
      const s = this.look.scale;
      const r = { x: this.x - 130, y: this.y - 70 * s, w: 260, h: 70 * s };
      K.strike('player', r, { dmg: 24, dir: this.facing, kb: 320, launch: 420, stun: 0.6, knockdown: true, heavy: true, poise: 2, src: this, onHit: (t, res) => this.onDealt(24, t, res, { knockdown: true }) }, this.hitSet);
      K.fx.add({ type: 'ring', x: this.x, y: K.GROUND, size: 160, life: 0.35, color: 'rgba(255,230,190,0.9)' });
      K.fx.dust(this.x, K.GROUND, 14, 2);
      K.cam.shake(8, 0.18);
      K.sfx('heavy');
      this.setState('plungeRec');
    }

    // ---------- Menzilli ----------
    startRanged() {
      const w = this.rangedW;
      const I = K.input;
      const mx = (I.held('right') ? 1 : 0) - (I.held('left') ? 1 : 0);
      if (mx) this.facing = mx;
      if (w === 'yay') {
        this.drawAmt = 0;
        this.setState('bow');
      } else if (w === 'tabanca') {
        if (this.pistolReload > 0) {
          K.fx.text(this.x, this.y - 140, 'Dolduruluyor…', '#aab4c8', 14, 0.5);
          return;
        }
        this.setState('shoot');
        this.firePistol();
      } else if (w === 'cirit') {
        if (this.javelins <= 0) {
          K.fx.text(this.x, this.y - 140, 'Cirit yok', '#aab4c8', 14, 0.5);
          return;
        }
        this.thrown = false;
        this.setState('throw');
      }
    }
    updateBow(dt, mx) {
      const I = K.input;
      this.drawAmt = Math.min(1, this.drawAmt + dt / 0.65);
      this.vx = K.approach(this.vx, mx * 120, 2400 * dt);
      if (this.take('dodge')) {
        this.setState('idle');
        this.queue('dodge');
        return 1;
      }
      if (!I.held('ranged') || !this.ctrl) {
        this.fireArrow();
        this.setState('shoot');
        this.st = 0.16;
        return 1;
      }
      return this.onGround ? 1 : 0.35;
    }
    handPos() {
      const s = this.look.scale;
      return { x: this.x + this.facing * 38 * s, y: this.y - 96 * s };
    }
    fireArrow() {
      const d = this.drawAmt;
      const h = this.handPos();
      K.game.addProj(new K.Proj({
        kind: 'arrow', team: 'player', x: h.x, y: h.y, vx: this.facing * (1100 + d * 1000), vy: -40 * (1 - d), grav: 260,
        dmg: Math.round(10 + d * 30), kb: 80 + d * 240, stun: 0.3 + d * 0.2, pierce: d > 0.85 ? 3 : 1, knockdown: d > 0.95,
        onHit: (t, r) => this.onDealt(10 + d * 30, t, r, { weapon: 'yay' }),
      }));
      this.wear('yay', 0.004);
      K.sfx('bow');
      if (d > 0.95) K.fx.add({ type: 'ring', x: h.x, y: h.y, size: 30, life: 0.2, flat: 1, color: 'rgba(255,240,190,0.9)' });
    }
    firePistol() {
      const h = this.handPos();
      K.game.addProj(new K.Proj({
        kind: 'bullet', team: 'player', x: h.x, y: h.y, vx: this.facing * 2700, vy: 0, grav: 0,
        dmg: 50, kb: 480, stun: 0.6, knockdown: true, pierce: 1, guardBreak: true, poise: 3,
        onHit: (t, r) => this.onDealt(50, t, r, { weapon: 'tabanca', knockdown: true }),
      }));
      this.pistolReload = 2.4;
      this.wear('tabanca', 0.02);
      K.fx.smoke(h.x + this.facing * 26, h.y, this.facing, 12);
      K.fx.sparks(h.x + this.facing * 28, h.y, this.facing, 8, '#ffcf6a', 700);
      K.cam.shake(6, 0.12);
      K.sfx('shot');
      this.vx = -this.facing * 160;
    }
    fireJavelin() {
      const h = this.handPos();
      this.javelins--;
      K.game.addProj(new K.Proj({
        kind: 'javelin', team: 'player', x: h.x, y: h.y - 10, vx: this.facing * 1500, vy: -140, grav: 520,
        dmg: 34, kb: 340, stun: 0.6, knockdown: true, pierce: 3, guardBreak: true, poise: 2,
        onHit: (t, r) => this.onDealt(34, t, r, { weapon: 'cirit', knockdown: true }),
      }));
      this.wear('cirit', 0.01);
      K.sfx('throw');
    }

    // ---------- Akın (özel güç) ----------
    startAkin() {
      this.akin = 0;
      this.setState('akin');
      this.akinStep = 0;
      this.inv = 2;
      this.vx = this.vy = 0;
      K.game.flash = 0.35;
      K.game.slowmo = 0.5;
      K.sfx('horn');
      K.fx.text(this.x, this.y - 170, 'AKIN!', '#ffcf6a', 34, 1.1);
    }
    updateAkin(dt) {
      const times = [0.15, 0.45, 0.75, 1.05];
      this.vy = 0;
      if (this.akinStep < times.length && this.st >= times[this.akinStep]) {
        const b = K.game.bounds;
        const from = this.x;
        let to = from + this.facing * 520;
        if (to < b.min + 60 || to > b.max - 60) {
          this.facing *= -1;
          to = from + this.facing * 520;
        }
        to = K.clamp(to, b.min + 60, b.max - 60);
        this.x = to;
        const r = { x: Math.min(from, to) - 40, y: this.y - 150, w: Math.abs(to - from) + 80, h: 150 };
        K.strike('player', r, { dmg: 55, dir: this.facing, kb: 300, launch: 700, stun: 0.8, knockdown: true, guardBreak: true, poise: 3, heavy: true, src: this, onHit: (t, res) => this.onDealt(0, t, res, { knockdown: true }) }, new Set());
        const y = this.y - 80;
        K.fx.add({ type: 'line', x: from, y, x2: to, y2: y, size: 26, life: 0.3, color: 'rgba(255,214,140,0.95)' });
        K.fx.add({ type: 'line', x: from, y: y - 20, x2: to, y2: y - 20, size: 6, life: 0.4, color: 'rgba(255,255,255,0.9)' });
        K.cam.shake(10, 0.15);
        K.sfx('heavy');
        this.facing *= -1;
        this.akinStep++;
      }
      if (this.st > 1.3) {
        this.inv = 0.5;
        this.setState('idle');
      }
    }

    // ---------- Hasar alma ----------
    takeHit(p) {
      if (this.dead || this.inv > 0 || this.state === 'akin' || K.game.god) return false;
      if (this.state === 'dodge' && this.st < 0.26) return false;
      if (this.state === 'down' || this.state === 'getup') return false;
      this.hp -= p.dmg;
      this.flash = 0.12;
      this.combo = 0;
      this.akin = Math.min(100, this.akin + p.dmg * 0.3);
      K.fx.blood(this.x, this.y - 70, p.dir, 8);
      K.fx.number(this.x, this.y - 130, p.dmg, '#ff7a6a', 18);
      K.cam.shake(6, 0.15);
      K.game.hitstop = Math.max(K.game.hitstop, 0.06);
      K.sfx('hurt');
      if (this.hp <= 0) {
        this.hp = 0;
        this.dead = true;
        this.setState('dead');
        this.vx = p.dir * 300;
        this.vy = -500;
        this.onGround = false;
        K.game.onPlayerDeath();
        return 'hit';
      }
      if (p.knockdown) {
        this.setState('down');
        this.vx = p.dir * (p.kb || 300);
        this.vy = -380;
        this.onGround = false;
      } else {
        this.setState('hurt');
        this.stunT = Math.min(0.35, (p.stun || 0.3) * 0.7);
        this.vx = p.dir * (p.kb || 200) * 0.6;
      }
      if (p.dir) this.facing = -p.dir;
      return 'hit';
    }

    // ---------- Poz ----------
    computePose() {
      const melee = this.meleeW;
      const base = melee === 'mizrak' ? SPEAR_STANCE : STANCE;
      let pose = base;
      const t = this.st;
      this.weaponDrawn = melee;
      switch (this.state) {
        case 'idle': {
          pose = Object.assign({}, base, { bob: Math.sin(this.anim * 3) * 1.5 });
          break;
        }
        case 'run':
          pose = K.runPose(this.anim * 13, base);
          break;
        case 'air':
          pose = Object.assign({}, base, AIR_LEGS, { lean: 0.15 });
          if (this.jumps === 2 && t < 0.3) pose.rot = (t / 0.3) * Math.PI * 2;
          break;
        case 'attack': {
          const a = this.atk;
          if (t < a.startup) pose = K.lerpPose(base, a.wind, K.ease(t / a.startup));
          else if (t < a.startup + a.active) pose = K.lerpPose(a.wind, a.strike, Math.min(1, (t - a.startup) / (a.active * 0.6)));
          else pose = K.lerpPose(a.strike, base, K.clamp((t - a.startup - a.active) / a.recovery, 0, 1) ** 2);
          break;
        }
        case 'plunge':
          pose = P(Object.assign({ lean: 0.2, au: 1.0, af: 0.2, wa: 1.45 }, AIR_LEGS));
          break;
        case 'plungeRec':
          pose = P({ lean: 0.5, au: 0.9, af: 0.3, wa: 1.4, ft: 0.9, fs: -0.6, bt: -0.6, bs: -1.6, bob: 18 });
          break;
        case 'bow':
        case 'shoot':
        case 'throw':
          pose = this.rangedPose(t);
          break;
        case 'dodge':
          pose = P({ lean: 0.9, ft: 1.2, fs: -0.4, bt: 0.2, bs: -1.4, au: 0.6, af: 0.4, wa: 0.4, bob: 22, rot: this.onGround ? 0 : 0.2 });
          if (this.onGround) pose.rot = (t / 0.3) * Math.PI * 2;
          break;
        case 'hurt':
          pose = P({ lean: -0.35, au: -0.2, af: 0.5, wa: -1.2, ft: 0.4, bt: -0.4, head: -0.3 });
          break;
        case 'down':
        case 'dead':
          pose = P({ lean: -0.2, rot: -Math.PI / 2 * Math.min(1, t * 4), au: 2.0, af: 2.4, wa: -2, ft: 0.4, bt: 0.2, bob: 8 });
          break;
        case 'getup':
          pose = K.lerpPose(P({ rot: -Math.PI / 2, bob: 8 }), base, t / 0.32);
          break;
        case 'akin':
          pose = P({ lean: 0.6, au: 1.5, af: 1.5, wa: 0, ft: 1.0, fs: 0.3, bt: -0.8, bs: -0.9 });
          break;
        case 'cinematic':
          pose = Object.assign({}, base, { bob: Math.sin(this.anim * 3) * 1.5 });
          break;
      }
      this.pose = pose;
    }
    rangedPose(t) {
      const w = this.state === 'bow' || (this.state === 'shoot' && this.rangedW === 'yay') ? 'yay' : this.rangedW;
      this.weaponDrawn = w;
      if (w === 'yay') {
        return P({ lean: 0.05, au: 1.55, af: 1.57, wa: 0, ft: 0.35, fs: 0.1, bt: -0.35, bs: -0.4, draw: this.state === 'bow' ? this.drawAmt : 0 });
      }
      if (w === 'tabanca') {
        const k = Math.max(0, 1 - t / 0.2);
        return P({ lean: -0.05 * k, au: 1.55 + 0.25 * k, af: 1.6 + 0.6 * k, wa: -0.6 * k, bu: -0.3, bf: 0.2, ft: 0.35, bt: -0.35 });
      }
      // cirit
      if (t < 0.13) return P({ lean: -0.25, au: -2.3, af: -1.9, wa: -0.25, bu: 1.2, bf: 1.4, ft: 0.4, bt: -0.4 });
      return P({ lean: 0.4, au: 2.0, af: 1.6, wa: 0.1, bu: -0.6, bf: -0.3, ft: 0.8, fs: 0.3, bt: -0.6, bs: -0.7 });
    }

    // Piksel hattı için: yalnızca gövde (dünya koordinatı)
    rig(ctx) {
      let weapon = this.weaponDrawn;
      if (this.state === 'throw' && this.thrown) weapon = null;
      if (weapon && this.broken[weapon]) weapon = null;
      K.drawRig(ctx, this.x, this.y, this.facing, this.pose, this.look, weapon, this.flash > 0, { cape: { t: this.anim, v: Math.min(1, Math.abs(this.vx) / 420) } });
    }
    get hidden() {
      return this.inv > 0 && this.state !== 'akin' && this.state !== 'dodge' && !(this.flash > 0) && Math.floor(this.inv * 20) % 2 === 1;
    }
    drawFx(ctx) {
      if (this.state === 'bow' && this.drawAmt > 0.95) {
        const h = this.handPos();
        ctx.fillStyle = 'rgba(255,240,180,0.9)';
        ctx.beginPath();
        ctx.arc(h.x + this.facing * 20, h.y, 7 + Math.sin(this.anim * 30) * 3, 0, 7);
        ctx.fill();
      }
    }
  }
  K.ease = t => 1 - (1 - t) * (1 - t);
  K.Player = Player;
  K.ATTACKS = A;
  K.RANGED = RANGED;
})(window.K);
