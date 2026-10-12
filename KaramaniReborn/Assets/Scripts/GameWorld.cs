using System;
using System.Collections.Generic;
using UnityEngine;

namespace KaramaniReborn
{
    public sealed partial class GameWorld
    {
        public readonly Actor player;
        public readonly List<Actor> enemies = new();
        public readonly List<Projectile> projectiles = new();
        // Çağrılan düşmanlar döngü bittikten sonra doğar (liste dolaşılırken değiştirilmez)
        readonly List<(Vector2 at, EnemyRole role)> spawnQueue = new();
        // Bölümlerin sonundaki ağalar (bölüm sırasıyla)
        public static readonly EnemyRole[] ZoneBosses = { EnemyRole.Arslan, EnemyRole.KorDemir, EnemyRole.Kurtbas, EnemyRole.KaraMurad };
        public readonly ZoneDefinition zone;
        public readonly Effects effects;
        public readonly Transform actorsRoot;
        public float clock, checkpoint = 1.7f;
        public bool cleared, transitionRequested;
        public string notification = "";
        public float notificationTime;
        public int kills;
        public Action<string> sound;
        public Action checkpointSaved;
        readonly float[] shrines = { 2, 21, 39 };
        readonly HashSet<int> shrineVisits = new();
        float dustClock, ghostClock, attackDirector;
        int heardSteps;
        readonly System.Random rng;
        public enum ShotKind { Pistol, Arrow, Bullet, VolleyMark, VolleyArrow, Wave, Orb, Fire, Grenade }
        // Ateşte: genişlik ve bir sonraki yakışa kalan süre; humbarada: düşeceği yer ve saldırının tanımı
        public sealed class Projectile { public ShotKind kind; public Vector2 position, velocity; public bool friendly, red; public float life, delay, damage; public SpriteRenderer renderer; public Actor source; public float width, tick; public Vector2 target; public EnemyAttack attack; }
        // Arayüzün göstereceği hasar sayıları
        public struct DamageEvent { public Vector2 position; public int amount; public bool toPlayer, strong; }
        public readonly List<DamageEvent> damageEvents = new();

        public GameWorld(int index, Transform parent, float checkpointX = 1.7f)
        {
            zone = ZoneDefinition.Create(index); rng = new System.Random(1566 + index);
            actorsRoot = new GameObject("Actors and projectiles").transform; actorsRoot.SetParent(parent, false);
            effects = new Effects(parent);
            checkpoint = checkpointX;
            player = new Actor { id = 0, player = true, position = new Vector2(checkpointX, zone.floor), maxGuard = 100, guard = 100 };
            new PixelCharacter(player, actorsRoot);
            foreach (var (x, y, role) in Layout(index)) Spawn(x, y, role);
            Spawn(50.4f, zone.floor, ZoneBosses[index], true);
        }

        // Bölge dizilişi (Docs/specs/2026-10-12-enemy-roster-design.md §5): önce insanlar, büyücü Kurt Geçidi'nde ilk kez.
        // Platformlar: 23.5-26.6 (y -0.75), 37.4-40.7 (y -0.85)
        (float x, float y, EnemyRole role)[] Layout(int index)
        {
            float f = zone.floor;
            return index switch
            {
                0 => new[] { (10f, f, EnemyRole.Nefer), (18.1f, f, EnemyRole.Spearman), (25.5f, -.75f, EnemyRole.Archer), (35.1f, f, EnemyRole.Nefer), (42.3f, f, EnemyRole.Nefer) },
                1 => new[] { (10f, f, EnemyRole.Nefer), (20f, f, EnemyRole.Musketeer), (35.1f, f, EnemyRole.Halberdier), (39f, -.85f, EnemyRole.Musketeer), (44f, f, EnemyRole.Nefer) },
                2 => new[] { (10f, f, EnemyRole.Spearman), (20f, f, EnemyRole.Musketeer), (25.5f, -.75f, EnemyRole.Archer), (35.1f, f, EnemyRole.Halberdier), (39f, -.85f, EnemyRole.Archer), (44f, f, EnemyRole.Sorcerer) },
                _ => new[] { (10f, f, EnemyRole.Nefer), (18.1f, f, EnemyRole.Spearman), (25.5f, -.75f, EnemyRole.Musketeer), (35.1f, f, EnemyRole.Halberdier), (39f, -.85f, EnemyRole.Archer), (44f, f, EnemyRole.Sorcerer) },
            };
        }

        public Actor Spawn(float x, float y, EnemyRole role, bool boss = false)
        {
            var p = EnemyProfile.For(role);
            var a = new Actor { id = enemies.Count + 1, role = role, boss = boss, position = new Vector2(x, y), face = -1,
                health = role == EnemyRole.Captain ? 210 + zone.index * 20 : p.health, maxGuard = p.guard, size = p.size, home = x, intentTime = .3f + enemies.Count * .11f };
            // Karanlık Kale'de yarı-boss daha güçlü (spec §5): can x1.3, denge x1.2; profil aynı kalır
            if (!boss && p.halfBoss && zone.index == 3) { a.health *= 1.3f; a.maxGuard *= 1.2f; }
            a.maxHealth = a.health; a.guard = a.maxGuard; new PixelCharacter(a, actorsRoot); enemies.Add(a);
            return a;
        }

        public void Simulate(float dt, InputFrame input, bool enemyAI = true)
        {
            clock += dt; notificationTime -= dt; attackDirector -= dt; LightField.Clock = clock;
            UpdateGuard();
            TickTimers(player, dt);
            TickPlayer(dt, input);
            player.view.Render(player, dt, clock);
            ResolveBlade(player);
            foreach (var a in enemies)
            {
                TickTimers(a, dt);
                if (enemyAI) TickEnemy(a, dt); else a.velocity.x = 0;
                a.view.Render(a, dt, clock);
                if (enemyAI) PerformAttack(a);
            }
            // Çağrılan düşmanlar döngü bittikten sonra doğar (liste dolaşılırken değiştirilmez)
            foreach (var (at, role) in spawnQueue) Spawn(at.x, at.y, role).intentTime = .4f;
            spawnQueue.Clear();
            TickProjectiles(dt);
            if (player.position.y < -6 && !player.Dead) Damage(player, 200, 0, false, true);
            foreach (var a in enemies) if (a.position.y < -6 && !a.Dead) Damage(a, 1000, 0, false, true);
            cleared = enemies.Find(a => a.boss)?.Dead ?? true;
            if (input.interact && !player.Dead)
            {
                for (int i = 0; i < shrines.Length; i++) if (Mathf.Abs(player.position.x - shrines[i]) < 1.4f)
                {
                    checkpoint = shrines[i]; player.health = player.maxHealth; player.stamina = 100; player.potions = 3;
                    if (shrineVisits.Add(i)) effects.GlowBurst(player.position + Vector2.up, zone.accent);
                    checkpointSaved?.Invoke(); Notify("YOL TAŞI · Nefes ve hatıra yenilendi."); sound?.Invoke("shrine");
                }
                if (cleared && player.position.x > zone.length - 3) transitionRequested = true;
            }
            dustClock -= dt; ghostClock -= dt;
            if (player.grounded && Mathf.Abs(player.velocity.x) > 1 && dustClock <= 0)
            { dustClock = player.skid > 0 ? .05f : .17f; effects.Dust(player.position + Vector2.right * player.face * .3f, player.face, zone.snow); }
            if (player.view.Steps != heardSteps) { heardSteps = player.view.Steps; sound?.Invoke("step"); }
            if (player.state == ActionState.Spirit && ghostClock <= 0)
            { ghostClock = .065f; player.view.AfterImage(effects, new Color(1, .80f, .45f, .30f)); }
        }

        void TickTimers(Actor a, float dt)
        {
            a.stateTime += dt; a.grabCooldown -= dt; a.airFlip -= dt; a.riposte -= dt; a.cooldown -= dt; a.invincible -= dt; a.parryWindow -= dt; a.hitFlash -= dt; a.dropTimer -= dt; a.skillCooldown -= dt; a.shotCooldown -= dt; a.staminaDelay -= dt; a.pushTime -= dt;
            if (!a.Locked && a.staminaDelay <= 0) a.stamina = Mathf.MoveTowards(a.stamina, 100, dt * (a.state == ActionState.Block ? 6 : 24));
            if (!a.Attacking) a.guard = Mathf.MoveTowards(a.guard, a.maxGuard, dt * 7);
        }

        void TickPlayer(float dt, InputFrame input)
        {
            var a = player;
            if (a.Dead) { SettleCorpse(a, dt); return; }
            if (a.state == ActionState.Hang) { Hang(a, input); return; }
            if (a.state == ActionState.Climb) { Climb(a); return; }
            a.skid = Mathf.Max(0, a.skid - dt);
            a.jumpBuffer = Mathf.Max(0, a.jumpBuffer - dt); if (input.jump) a.jumpBuffer = .13f;
            a.coyote = a.grounded ? .10f : Mathf.Max(0, a.coyote - dt);
            if (a.grounded) a.airJumps = 1;
            if (a.Attacking && input.light && a.Progress > .20f) a.bufferedLight = true;
            // Geri adımın ortasından saldırıya geçilebilir
            if (a.state == ActionState.Dodge && a.backdash && a.stateTime > .1f && (input.light || input.heavy) && a.stamina >= 10)
                BeginAttack(a, input.heavy && a.stamina >= 28, 1);
            if (a.Locked && a.stateTime >= a.duration)
            {
                bool chain = a.state == ActionState.Light && a.combo < 4 && a.bufferedLight && a.stamina >= 10;
                int combo = a.combo % 3 + 1; a.bufferedLight = false; a.Enter(ActionState.Idle);
                if (chain) BeginAttack(a, false, combo);
            }
            if (!a.Locked)
            {
                // Tam hızda bırakınca ya da ters yöne basınca karakter önce kayarak durur, sonra döner
                bool moving = Mathf.Abs(input.move) > .01f;
                bool fast = a.grounded && Mathf.Abs(a.velocity.x) > 3.4f && !input.slow;
                bool reversing = moving && fast && Mathf.Sign(input.move) != Mathf.Sign(a.velocity.x);
                if (a.skid <= 0 && fast && (reversing || !moving) && a.state == ActionState.Move) { a.skid = reversing ? .26f : .2f; sound?.Invoke("skid"); }
                if (a.skid > 0 && moving && !reversing && Mathf.Sign(input.move) == a.face) a.skid = 0;
                if (moving && a.skid <= 0) a.face = input.move > 0 ? 1 : -1;
                bool wasBlocking = a.state == ActionState.Block;
                if (input.block && a.grounded && a.stamina > 4)
                { if (!wasBlocking) { a.Enter(ActionState.Block); a.parryWindow = .14f; } }
                else if (wasBlocking || a.state == ActionState.Parry) a.Enter(ActionState.Idle);
                if (input.heal && a.potions > 0 && a.health < a.maxHealth && a.grounded)
                { a.Enter(ActionState.Heal, .8f); a.potions--; sound?.Invoke("heal"); }
                else if (input.dodge && a.stamina >= (moving ? 22 : 12) && a.grounded)
                {
                    // Yönle birlikte: ileri yuvarlanma. Yalnız başına: hızlı geri adım.
                    if (moving) { a.Enter(ActionState.Dodge, .40f); a.stamina -= 22; a.invincible = .29f; }
                    else { a.Enter(ActionState.Dodge, .26f); a.backdash = true; a.stamina -= 12; a.invincible = .18f; }
                    a.staminaDelay = .7f; sound?.Invoke("dodge");
                }
                else if (input.spirit && a.skillCooldown <= 0 && a.stamina >= 28)
                { a.Enter(ActionState.Spirit, .54f); a.stamina -= 28; a.skillCooldown = 8; a.invincible = .42f; effects.Wolf(a.position, a.face); sound?.Invoke("spirit"); }
                else if (input.shoot && a.ammo > 0 && a.shotCooldown <= 0)
                { a.Enter(ActionState.Shoot, .55f); a.ammo--; a.shotCooldown = 1.1f; }
                else if (input.light && a.riposte > 0 && RiposteTarget(a) != null)
                { BeginAttack(a, true, 1); a.riposting = true; a.stamina += 28; a.riposte = 0; }
                else if (!a.grounded && input.down && input.light && a.stamina >= 10) { BeginAttack(a, false, 4); a.velocity.y = Mathf.Min(a.velocity.y, 1.5f); }
                else if (a.state != ActionState.Block && input.heavy && a.stamina >= 28) BeginAttack(a, true, 1);
                else if (a.state != ActionState.Block && input.light && a.stamina >= 10) BeginAttack(a, false, 1);
                if (!a.Locked && a.state != ActionState.Block && a.jumpBuffer > 0 && a.coyote > 0)
                {
                    if (input.down && a.position.y > zone.floor + .2f) { a.dropTimer = .24f; a.position.y -= .08f; a.velocity.y = -1; }
                    else { a.velocity.y = 8.7f; sound?.Invoke("jump"); }
                    a.grounded = false; a.jumpBuffer = a.coyote = 0; a.skid = 0; a.view.ResetContacts();
                }
                else if (!a.Locked && !a.grounded && a.jumpBuffer > 0 && a.coyote <= 0 && a.airJumps > 0)
                {
                    // Havada ikinci zıplama, öne takla ile
                    a.airJumps--; a.velocity.y = 7.6f; a.jumpBuffer = 0; a.airFlip = .38f;
                    effects.Dust(a.position, a.face, zone.snow); sound?.Invoke("jump2");
                }
                if (a.Locked) a.skid = 0;
            }
            float desired = input.move * (input.slow ? 2.15f : 4.25f);
            if (a.state == ActionState.Block) desired *= .24f;
            if (a.state is ActionState.Heal or ActionState.Shoot) desired = 0;
            if (a.Attacking) desired = a.face * (Active(a) ? a.state == ActionState.Heavy ? 2.5f : a.state == ActionState.Spirit ? 8.1f : 1.8f : a.state == ActionState.Spirit ? 7.5f : 0);
            if (a.state == ActionState.Light && a.combo == 4) desired = a.face * 1.4f;
            if (a.state == ActionState.Dodge) desired = a.backdash ? -a.face * 7.4f * (1 - a.Progress * .7f) : a.face * 8.2f;
            if (a.state == ActionState.Hurt) desired = a.knockback * (1 - a.Progress);
            if (a.skid > 0) desired = 0;
            a.velocity.x = Mathf.MoveTowards(a.velocity.x, desired, dt * (a.Locked ? 65 : a.skid > 0 ? 9 : a.grounded ? 25 : 12));
            // Kalkanla itme: blokta bile kısa sürede sabit mesafe geri iter; bitince kayma kalmaz
            if (a.pushTime > 0) a.velocity.x = a.pushSpeed;
            else if (a.pushSpeed != 0) { a.velocity.x = 0; a.pushSpeed = 0; }
            // Dalış: kısa bir asılı kalıştan sonra hızla aşağı
            if (a.state == ActionState.Light && a.combo == 4 && a.stateTime > .06f) a.velocity.y = Mathf.Min(a.velocity.y, -11f);
            bool landed = !a.grounded; float fallSpeed = a.velocity.y;
            KinematicMotor.Step(a, zone, dt, !input.jumpHeld);
            if (landed && a.grounded)
            {
                a.view.ResetContacts(); effects.Dust(a.position, a.face, zone.snow);
                // Yüksekten düşüş: sert iniş
                if (fallSpeed < -10.5f) { effects.Dust(a.position, -a.face, zone.snow); effects.cameraShake = Mathf.Max(effects.cameraShake, .07f); sound?.Invoke("land"); }
                else sound?.Invoke("step");
                if (a.state == ActionState.Light && a.combo == 4)
                {
                    a.Enter(ActionState.Idle); effects.Dust(a.position, -a.face, zone.snow);
                    effects.cameraShake = Mathf.Max(effects.cameraShake, .08f);
                }
            }
            if (!a.Locked && a.state != ActionState.Block)
                a.state = !a.grounded ? a.velocity.y > 0 ? ActionState.Jump : ActionState.Fall : Mathf.Abs(a.velocity.x) > .1f ? ActionState.Move : ActionState.Idle;
            if (!a.grounded) TryGrabLedge(a, input);
            if (a.state == ActionState.Heal && !a.attackConnected && a.Progress > .55f)
            { a.attackConnected = true; a.health = Mathf.Min(a.maxHealth, a.health + 48); effects.GlowBurst(a.position + Vector2.up, new Color(.5f, 1, .72f)); }
            if (a.state == ActionState.Shoot && !a.attackConnected && a.Progress > .35f)
            { a.attackConnected = true; Shoot(a); sound?.Invoke("shot"); }
        }

        void BeginAttack(Actor a, bool heavy, int combo)
        {
            a.combo = combo; a.Enter(heavy ? ActionState.Heavy : ActionState.Light, heavy ? .80f : combo == 4 ? 1.2f : combo == 3 ? .47f : .44f);
            if (a.player) { a.stamina -= heavy ? 28 : 10; a.staminaDelay = .45f; }
            sound?.Invoke(heavy ? "heavy" : "swing");
        }

        // ---------- Kenara tutunma ve tırmanma ----------
        const float HangDepth = 1.78f, HangReach = .3f;

        // Düşerken önündeki bir kenarı ellerin hizasında yakalar. Platformun altından geçerken değil,
        // yalnızca yanındayken; aynı yükseklikte devam eden bir zemin varsa orası kenar sayılmaz.
        void TryGrabLedge(Actor a, InputFrame input)
        {
            if (a.Locked || a.velocity.y > 1.5f || input.down || a.grabCooldown > 0) return;
            foreach (var s in zone.surfaces)
            {
                float edgeX = a.face > 0 ? s.left : s.right;
                float ahead = (edgeX - a.position.x) * a.face;
                if (ahead < .05f || ahead > .6f) continue;
                float rise = s.y - a.position.y;
                if (rise < 1.55f || rise > 2.1f) continue;
                if (SurfaceContinues(edgeX - a.face * .1f, s.y)) continue;
                a.Enter(ActionState.Hang); a.ledge = new Vector2(edgeX, s.y);
                a.position = new Vector2(edgeX - a.face * HangReach * a.size, s.y - HangDepth * a.size);
                a.velocity = Vector2.zero; a.grounded = false; a.skid = 0; a.view.ResetContacts(); sound?.Invoke("grab");
                return;
            }
        }

        Actor RiposteTarget(Actor a)
        {
            foreach (var e in enemies)
                if (e.id == a.riposteTarget && !e.Dead && e.state == ActionState.Hurt && Mathf.Abs(e.position.x - a.position.x) < 2.2f) return e;
            return null;
        }

        bool SurfaceContinues(float x, float y)
        { foreach (var s in zone.surfaces) if (x >= s.left && x <= s.right && Mathf.Abs(s.y - y) < .1f) return true; return false; }

        // Asılıyken: yukarı ya da zıplama tırmanır; aşağı ya da kenardan uzağa basmak bırakır
        void Hang(Actor a, InputFrame input)
        {
            a.velocity = Vector2.zero;
            if (a.stateTime < .12f) return;
            if (input.down || (Mathf.Abs(input.move) > .01f && Mathf.Sign(input.move) != a.face))
            { a.Enter(ActionState.Fall); a.grabCooldown = .35f; a.velocity.y = -1; return; }
            if (input.up || input.jump) { a.Enter(ActionState.Climb, .62f); a.climbFrom = a.position; sound?.Invoke("climb"); }
        }

        // Tırmanış: önce kendini yukarı çeker, sonra kenarın üstüne geçer, en sonda ayağa kalkar
        void Climb(Actor a)
        {
            float p = a.Progress;
            Vector2 top = new Vector2(a.ledge.x + a.face * .28f * a.size, a.ledge.y);
            float rise = Mathf.SmoothStep(0, 1, Mathf.Clamp01(p / .7f)), over = Mathf.SmoothStep(0, 1, Mathf.Clamp01((p - .3f) / .6f));
            a.position = new Vector2(Mathf.Lerp(a.climbFrom.x, top.x, over), Mathf.Lerp(a.climbFrom.y, top.y, rise));
            a.velocity = Vector2.zero;
            if (a.stateTime >= a.duration)
            { a.position = top; a.grounded = true; a.Enter(ActionState.Idle); a.view.ResetContacts(); sound?.Invoke("step"); }
        }

        // Kılıç duruşu: rakip yakında ve önündeyse
        void UpdateGuard()
        {
            Actor nearest = null; float best = 3.4f;
            foreach (var e in enemies)
            {
                if (e.Dead || Mathf.Abs(e.position.y - player.position.y) > 1) continue;
                float d = Mathf.Abs(e.position.x - player.position.x); if (d < best) { best = d; nearest = e; }
            }
            player.enGarde = nearest != null && player.grounded && (nearest.position.x - player.position.x) * player.face > 0;
            foreach (var e in enemies)
                e.enGarde = !e.Dead && !player.Dead && !EnemyProfile.For(e.role).ranged && e.grounded && Mathf.Abs(player.position.x - e.position.x) < 3.4f
                    && Mathf.Abs(player.position.y - e.position.y) < 1 && (player.position.x - e.position.x) * e.face > 0;
        }

        void SettleCorpse(Actor a, float dt)
        {
            if (a.grounded) a.velocity.x = Mathf.MoveTowards(a.velocity.x, 0, dt * 14);
            KinematicMotor.Step(a, zone, dt);
            if (a.grounded && !a.pooled && a.stateTime > .3f) { a.pooled = true; effects.BloodPool(a.position, a.size); }
        }

        bool GroundAhead(Actor a, float offset)
        { foreach (var s in zone.surfaces) if (a.position.x + offset >= s.left && a.position.x + offset <= s.right && Mathf.Abs(s.y - a.position.y) < .10f) return true; return false; }

        static bool Active(Actor a) => a.state == ActionState.Light && a.combo == 4 ? a.stateTime > .05f : a.state == ActionState.Spirit ? a.Progress > .15f && a.Progress < .85f : a.state == ActionState.Heavy ? a.Progress > .44f && a.Progress < .73f : a.Progress > .27f && a.Progress < .64f;

        // Oyuncunun kılıcı. Düşmanlar yalnız PerformAttack (EnemyBrain.cs) üzerinden vurur.
        void ResolveBlade(Actor source)
        {
            if (!source.Attacking || !Active(source) || source.Dead) return;
            // Lanet küreleri kılıçla söndürülür
            foreach (var shot in projectiles)
                if (shot.kind == ShotKind.Orb && shot.life > 0 && shot.delay <= 0 && SegmentHits(source.view.StrikeRoot, source.view.BladeTip, new Rect(shot.position.x - .3f, shot.position.y - .3f, .6f, .6f)))
                { shot.life = 0; effects.GlowBurst(shot.position, new Color(.55f, .1f, .8f)); }
            foreach (var target in enemies) TryBlade(source, target);
        }

        // Vurulma kutusunun genişlik çarpanı: oyuncuda ve sıradan düşmanda boy, bulk verilmişse o
        static float BodyWidth(Actor a)
        {
            if (a.player) return a.size;
            float bulk = EnemyProfile.For(a.role).bulk;
            return bulk > 0 ? bulk : a.size;
        }

        void TryBlade(Actor source, Actor target)
        {
            if (!source.Attacking || target.Dead || source.hitTargets.Contains(target.id)) return;
            Vector2 p = target.position;
            float w = BodyWidth(target);   // gövde genişliği: iri boss'ta boydan fazla (bulk)
            var box = new Rect(p.x - .24f * w, p.y + .20f, .48f * w, 1.91f * target.size);
            bool hit = SegmentHits(source.view.StrikeRoot, source.view.BladeTip, box);
            if (source.state == ActionState.Spirit) hit |= Vector2.Distance(source.position, target.position) < 1.6f;
            // Dalış: kılıcın yanı sıra ayakların hemen altı da vurur
            if (source.state == ActionState.Light && source.combo == 4)
                hit |= new Rect(source.position.x - .45f, source.position.y - .8f, .9f, 1.1f).Overlaps(box);
            if (!hit) return;
            source.hitTargets.Add(target.id);
            float damage = source.state == ActionState.Heavy ? 44 : source.state == ActionState.Spirit ? 46 : source.combo == 3 ? 29 : 20;
            bool dive = source.state == ActionState.Light && source.combo == 4;
            if (dive) damage = 26;
            float stop = source.state is ActionState.Heavy or ActionState.Spirit ? .1f : source.combo == 3 ? .075f : .055f;
            // İnfaz: savuşturulup sersemleyen rakibe karşı darbe
            bool execute = source.riposting && target.id == source.riposteTarget;
            if (execute) { damage *= 2.6f; stop = .16f; }
            Damage(target, damage, source.face, source.state is ActionState.Heavy or ActionState.Spirit, false, source, stop);
            if (execute) { effects.slowMotion = Mathf.Max(effects.slowMotion, .22f); sound?.Invoke("execute"); Notify("İNFAZ"); }
            // Dalış isabet edince oyuncu geri seker ve ikinci zıplaması yenilenir
            if (dive) { source.Enter(ActionState.Fall); source.velocity.y = 9; source.airJumps = 1; source.view.ResetContacts(); sound?.Invoke("jump2"); }
        }

        public static bool SegmentHits(Vector2 a, Vector2 b, Rect box)
        {
            float low = 0, high = 1; Vector2 d = b - a;
            return Clip(-d.x, a.x - box.xMin, ref low, ref high) && Clip(d.x, box.xMax - a.x, ref low, ref high) && Clip(-d.y, a.y - box.yMin, ref low, ref high) && Clip(d.y, box.yMax - a.y, ref low, ref high);
        }
        static bool Clip(float p, float q, ref float low, ref float high)
        { if (Mathf.Abs(p) < .00001f) return q >= 0; float r = q / p; if (p < 0) { if (r > high) return false; low = Mathf.Max(low, r); } else { if (r < low) return false; high = Mathf.Min(high, r); } return true; }

        public void Damage(Actor target, float damage, int direction, bool heavy, bool unblockable, Actor source = null, float stop = -1, bool red = false)
        {
            if (target.Dead || target.invincible > 0 && !unblockable) return;
            // Kalkanlı: önden gelen hafif vuruşu kalkan karşılar; ağır vuruş geçer, arkadan vuruş dengeyi iki kat düşürür
            var profile = target.player ? null : EnemyProfile.For(target.role);
            if (profile != null && target.phase > 1) damage *= profile.phase2Taken;   // çılgınlık: daha çok hasar alır
            bool front = source != null && (source.position.x - target.position.x) * target.face > 0;
            if (profile != null && profile.shielded && front && !heavy && !red && !unblockable && target.state != ActionState.Hurt)
            {
                target.guard -= damage * 1.3f;
                effects.Impact(target.position + Vector2.up * 1.35f, direction, true, false, float.NaN, .045f); sound?.Invoke("block");
                if (target.guard > 0)
                {
                    // Yalnız görüntü: saldırmıyorsa kısa bir an kalkan bloğu klibi
                    if (!target.Locked) target.Enter(ActionState.Block, .2f);
                    return;
                }
                Stagger(target, source); Notify("KALKAN KIRILDI"); return;
            }
            bool frontal = source == null || (source.position.x - target.position.x) * target.face >= 0;
            // Blok yalnız oyuncuda; düşmanın Block durumu kalkan klibidir (ağır vuruş kalkanı yine geçer)
            if (target.player && !unblockable && !red && frontal && target.state == ActionState.Block)
            {
                if (target.parryWindow > 0)
                {
                    target.Enter(ActionState.Parry, .15f); target.stamina = Mathf.Min(100, target.stamina + 12); effects.Impact(target.position + Vector2.up * 1.45f, -direction, true, true, float.NaN, .12f); Notify("SAVUŞTURMA"); sound?.Invoke("parry");
                    effects.slowMotion = .4f; effects.Glint(target.view.BladeTip, true);
                    // Savuşturulan sendeler: denge dolar, geri itilmez (infaz menzilinde kalır), oyuncuya infaz açılır
                    if (source != null) Stagger(source, target, EnemyProfile.For(source.role).parryStagger);
                    return;
                }
                float guardDamage = heavy ? 65 : damage * 1.3f;
                target.stamina -= guardDamage;
                effects.Impact(target.position + Vector2.up * 1.35f, direction, true, heavy, float.NaN, heavy ? .07f : .045f); sound?.Invoke("block");
                if (target.stamina > 0) return;
                Notify("SAVUNMA KIRILDI"); damage *= .6f;
            }
            target.health = Mathf.Max(0, target.health - damage); target.hitFlash = .14f;
            damageEvents.Add(new DamageEvent { position = target.position + Vector2.up * (2.55f * target.size), amount = Mathf.Max(1, Mathf.RoundToInt(damage)), toPlayer = target.player, strong = heavy || damage >= 40 }); target.invincible = target.player ? .40f : .14f;
            // Duruş: kendi saldırısını yürüten ağır düşman vurulunca durmaz (can ve denge yine düşer); denge kırılır ya da ölürse durur
            bool unstoppable = profile != null && profile.phase2Unstoppable && target.phase > 1;
            bool poised = profile != null && (unstoppable || profile.poise && target.attack != null && (target.state is ActionState.Light or ActionState.Shoot));
            // Sendeleme kısalmaz: daha uzun sürecek bir sendelemenin içindeki sıradan vuruş onu (ve geri itmesini) değiştirmez
            float hurt = heavy ? .6f : .28f;
            bool longer = target.state == ActionState.Hurt && target.duration - target.stateTime > hurt;
            if (target.health <= 0 || !poised && !longer) { target.knockback = direction * (heavy ? 3.1f : 1.5f); target.Enter(target.health <= 0 ? ActionState.Dead : ActionState.Hurt, hurt); }
            // Denge: her vuruş düşürür; sıfırlanınca uzun sendeleme ve infaz fırsatı
            // İnfaz vuruşu dengeyi düşürmez: savuşturmayla sıfırlanan denge yüzünden yeniden sendeletmez
            bool executed = source != null && source.riposting && target.id == source.riposteTarget;
            if (profile != null && !target.Dead && !executed)
            {
                // Kalkanlıya ağır ya da arkadan vuruş x2 (üst üste binmez); kalkansıza ağır x1.5
                target.guard -= damage * (profile.shielded ? (heavy || !front ? 2 : 1) : heavy ? 1.5f : 1);
                if (target.guard <= 0) { if (unstoppable) target.guard = target.maxGuard; else Stagger(target, source); }
            }
            bool held = poised && !target.Dead && target.state != ActionState.Hurt;   // saldırısı sürüyor: havaya kalkmaz
            effects.Impact(target.position + Vector2.up * 1.25f, direction, false, heavy, target.position.y, stop);
            effects.lastStruck = target; sound?.Invoke(heavy ? "heavyhit" : "hit");
            // Ölüm: geriye ve yukarı savrulur. Ağır darbe yaşayan sıradan düşmanı kısa süre havaya kaldırır.
            if (target.Dead && direction != 0) Launch(target, direction * (heavy ? 5.4f : 3.6f), heavy ? 5.6f : 4.2f);
            else if (heavy && !target.player && !target.boss && !held && direction != 0) Launch(target, direction * 1.2f, 3.4f);
            if (target.Dead && !target.player)
            { kills++; player.stamina = Mathf.Min(100, player.stamina + 14); if (target.boss) { Notify(zone.bossName + " yenildi. İç kapı açıldı."); player.ammo += 4; } }
        }

        // Uzun sendeleme: denge dolar, geri itme yok; oyuncu sendeletmişse infaz fırsatı açılır
        void Stagger(Actor target, Actor source, float seconds = .9f)
        {
            target.Enter(ActionState.Hurt, seconds); target.guard = target.maxGuard; target.knockback = 0;
            if (source != null && source.player) { source.riposte = .8f; source.riposteTarget = target.id; }
        }

        static void Launch(Actor a, float vx, float vy)
        {
            a.velocity = new Vector2(vx, vy); a.grounded = false; a.view?.ResetContacts();
        }

        // Karamani'nin tabancası
        void Shoot(Actor a)
        {
            Vector2 position = a.view.BladeRoot + new Vector2(a.face * .46f, .06f);
            var sr = ArtLibrary.Draw("Pistol shot", ArtLibrary.White, actorsRoot, position, 170, new Color(1, .88f, .63f));
            sr.transform.localScale = new Vector3(.23f, .025f, 1);
            sr.transform.rotation = Quaternion.Euler(0, 0, a.face > 0 ? 0 : 180);
            projectiles.Add(new Projectile { kind = ShotKind.Pistol, position = position, velocity = Vector2.right * a.face * 19, life = 1.4f, friendly = true, damage = 38, renderer = sr, source = a });
            effects.GlowBurst(position, new Color(1, .62f, .25f, .65f));
        }

        void TickProjectiles(float dt)
        {
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                var p = projectiles[i];
                if (p.life <= 0) { if (p.kind == ShotKind.Grenade) Explode(p); RemoveShot(i); continue; }
                p.renderer.enabled = p.delay <= 0;
                if (p.delay > 0) { p.delay -= dt; continue; }
                // Lanet küresi oyuncuyu yavaşça izler
                if (p.kind == ShotKind.Orb) p.velocity = Vector2.MoveTowards(p.velocity, (player.position + Vector2.up * 1.2f - p.position).normalized * 2.2f, dt * 6);
                if (p.kind == ShotKind.Grenade) p.velocity.y -= 20 * dt;   // humbara kavisle düşer
                Vector2 old = p.position; p.position += p.velocity * dt; p.life -= dt;
                if (p.kind == ShotKind.Fire)
                {
                    p.tick -= dt;
                    bool inside = Mathf.Abs(player.position.x - p.position.x) < p.width * .5f + .2f && player.position.y - p.position.y < .6f && player.position.y >= p.position.y - .1f;
                    if (inside && p.tick <= 0 && !player.Dead) { Damage(player, p.damage, 0, false, false, p.source, -1, true); p.tick = .5f; }
                    if (p.life <= 0) RemoveShot(i);
                    continue;
                }
                if (p.friendly)
                {
                    foreach (var a in enemies)
                    {
                        float bulk = EnemyProfile.For(a.role).bulk, half = .3f * (bulk > 0 ? bulk : 1);   // iri boss'ta daha geniş
                        if (!a.Dead && SegmentHits(old, p.position, new Rect(a.position.x - half, a.position.y + .2f, 2 * half, 2.1f)))
                        { Damage(a, p.damage, p.velocity.x > 0 ? 1 : -1, true, false, p.source, .08f); p.life = 0; break; }
                    }
                }
                else if (p.kind is not (ShotKind.VolleyMark or ShotKind.Grenade) && !player.Dead && HitsPlayer(p, old))
                { Damage(player, p.damage, p.velocity.x >= 0 ? 1 : -1, false, false, p.source, -1, p.red); p.life = 0; }
                p.renderer.transform.position = p.kind == ShotKind.Wave ? p.position + Vector2.up * .45f : p.position;
                if (p.life <= 0) { if (p.kind == ShotKind.Grenade) Explode(p); RemoveShot(i); }
            }
        }

        // Yerde yanan ateş: genişlik boyunca, yerden .6 yüksekliğe kadar; her .5 sn'de bir kırmızı hasar (zıplayan yanmaz)
        Projectile SpawnFire(Vector2 ground, float width, float life, float damage, Actor source, float delay = 0)
        {
            var sr = ArtLibrary.Draw("Fire", ArtLibrary.Glow, actorsRoot, ground + Vector2.up * .35f, 172, new Color(1, .45f, .1f, .75f));
            sr.transform.localScale = new Vector3(width, .8f, 1); sr.enabled = delay <= 0;
            var fire = new Projectile { kind = ShotKind.Fire, position = ground, life = life, delay = delay, damage = damage, red = true, width = width, renderer = sr, source = source };
            projectiles.Add(fire); return fire;
        }

        // Test için: verilen yere doğrudan ateş koyar
        public void SpawnFireForTest(Vector2 ground, float width, float life) => SpawnFire(ground, width, life, 6, null);

        // Humbara / yağ küpü yere değdi: yakındakine kırmızı patlama, yerinde ateş
        void Explode(Projectile p)
        {
            var k = p.attack; effects.GlowBurst(p.target + Vector2.up * .4f, new Color(1, .5f, .15f));
            if (!player.Dead && Mathf.Abs(player.position.x - p.target.x) < 1.2f && Mathf.Abs(player.position.y - p.target.y) < 1.5f)
                Damage(player, p.damage, (int)Mathf.Sign(player.position.x - p.target.x), false, false, p.source, -1, true);
            if (k != null && k.patchLife > 0) SpawnFire(p.target, k.patchWidth, k.patchLife, k.patchDamage, p.source);
        }

        bool HitsPlayer(Projectile p, Vector2 old)
        {
            var box = new Rect(player.position.x - .23f, player.position.y + .2f, .46f, 1.95f);
            if (p.kind == ShotKind.Wave) return new Rect(p.position.x - .3f, p.position.y, .6f, .9f).Overlaps(box);
            if (p.kind == ShotKind.Orb) return SegmentHits(old, p.position, new Rect(box.x - .2f, box.y - .2f, box.width + .4f, box.height + .4f));
            return SegmentHits(old, p.position, box);
        }

        void RemoveShot(int i)
        {
            var p = projectiles[i];
            if (Application.isPlaying) UnityEngine.Object.Destroy(p.renderer.gameObject); else UnityEngine.Object.DestroyImmediate(p.renderer.gameObject);
            projectiles.RemoveAt(i);
        }

        public void Notify(string text) { notification = text; notificationTime = 2.2f; }
        public string InteractionHint
        {
            get { foreach (float x in shrines) if (Mathf.Abs(player.position.x - x) < 1.4f) return "E · Yol taşında dinlen"; return player.position.x > zone.length - 3 ? cleared ? "E · İç kapıdan ilerle" : "Kapı mühürlü · Komutanı yen" : ""; }
        }
    }
}
