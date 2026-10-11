'use strict';
// Bölüm I: İstanbul'dan çıkış. Çarpışmalar sırayla gelir; her biri kilitli bir arenadır.
(function (K) {
  // Yeniçeri sayısı tam 100: dalgalar 86 + tüfekçibaşının 4 muhafızı + bayraktarın 4 çağrısı + ağanın 6 çağrısı
  const ENC = [
    { type: 'wave', label: 'Dalga 1', n: 8, max: 5, tokens: 2, mix: { kilicli: 1 } },
    { type: 'wave', label: 'Dalga 2', n: 12, max: 6, tokens: 2, mix: { kilicli: 0.8, tufekli: 0.2 } },
    { type: 'boss', key: 'odabasi' },
    { type: 'wave', label: 'Dalga 3', n: 12, max: 6, tokens: 2, mix: { kilicli: 0.6, mizrakli: 0.25, tufekli: 0.15 } },
    { type: 'wave', label: 'Dalga 4', n: 14, max: 7, tokens: 3, mix: { kilicli: 0.5, mizrakli: 0.3, tufekli: 0.2 } },
    { type: 'boss', key: 'tufekcibasi', escorts: ['kilicli', 'kilicli', 'mizrakli', 'kilicli'] },
    { type: 'wave', label: 'Dalga 5', n: 14, max: 7, tokens: 3, mix: { kilicli: 0.45, mizrakli: 0.25, tufekli: 0.15, kalkanli: 0.15 } },
    { type: 'boss', key: 'topuzcu' },
    { type: 'wave', label: 'Dalga 6', n: 14, max: 8, tokens: 3, mix: { kilicli: 0.4, mizrakli: 0.25, tufekli: 0.15, kalkanli: 0.2 } },
    { type: 'boss', key: 'bayraktar' },
    { type: 'wave', label: 'Dalga 7', n: 12, max: 8, tokens: 3, mix: { kilicli: 0.35, mizrakli: 0.25, tufekli: 0.2, kalkanli: 0.2 } },
    { type: 'boss', key: 'aga', final: true },
  ];
  K.ENC = ENC;
  K.TOTAL_WAVES = ENC.filter(e => e.type === 'wave').length;
  K.arenaLeft = i => i * 1600;
  K.LEVEL_END = K.arenaLeft(ENC.length - 1) + K.W;
  K.GATE_X = K.arenaLeft(ENC.length - 1) + 940;

  function pickType(mix) {
    let r = Math.random(), acc = 0;
    for (const k in mix) {
      acc += mix[k];
      if (r <= acc) return k;
    }
    return Object.keys(mix)[0];
  }

  class Encounter {
    constructor(i) {
      this.i = i;
      this.def = ENC[i];
      this.left = K.arenaLeft(i);
      this.right = this.left + K.W;
      this.spawned = 0;
      this.spawnT = 0.6;
      this.pending = [];
      this.pendT = 0;
      this.done = false;
      this.boss = null;
    }
    start() {
      const G = K.game, d = this.def;
      G.bounds = { min: this.left, max: this.right };
      G.maxTokens = d.tokens || 2;
      G.tokens = 0;
      if (d.type === 'wave') {
        G.banner = { text: d.label.toUpperCase(), sub: `${d.n} yeniçeri`, t: 0, dur: 2 };
        K.sfx('horn');
      } else {
        const B = K.BOSSES[d.key];
        this.boss = new K.Boss(d.key, this.right + 80);
        G.enemies.push(this.boss);
        G.maxTokens = 2;
        G.banner = { text: B.name, sub: B.title, t: 0, dur: 2.6, boss: true };
        if (d.escorts) this.pending.push(...d.escorts);
        K.sfx('horn');
      }
    }
    summon(types) {
      this.pending.push(...types);
    }
    spawnGrunt(type) {
      const side = Math.random() < 0.5 ? -1 : 1;
      const x = side < 0 ? this.left - K.rand(60, 160) : this.right + K.rand(60, 160);
      K.game.enemies.push(new K.Enemy(type, x));
    }
    alive() {
      return K.game.enemies.filter(e => !e.dead).length;
    }
    update(dt) {
      if (this.done) return;
      const d = this.def;
      if (this.pending.length) {
        this.pendT -= dt;
        if (this.pendT <= 0) {
          this.spawnGrunt(this.pending.shift());
          this.pendT = 0.3;
        }
      }
      if (d.type === 'wave' && this.spawned < d.n) {
        const alive = this.alive();
        if (alive < d.max) {
          this.spawnT -= dt;
          if (this.spawnT <= 0) {
            this.spawnGrunt(pickType(d.mix));
            this.spawned++;
            this.spawnT = K.rand(0.3, 0.75);
          }
        }
      }
      const waveDone = d.type === 'wave' ? this.spawned >= d.n : this.boss && this.boss.dead;
      if (waveDone && !this.pending.length && this.alive() === 0) this.finish();
    }
    finish() {
      this.done = true;
      K.game.onEncounterDone(this);
    }
  }
  K.Encounter = Encounter;
})(window.K);
