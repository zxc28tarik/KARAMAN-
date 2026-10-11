using System;
using System.Collections.Generic;
using UnityEngine;

namespace KaramaniReborn
{
    public sealed class GameWorld
    {
        public readonly Actor player;
        public readonly List<Actor> enemies = new();
        public readonly List<Projectile> projectiles = new();
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
        Vector2 previousPlayerTip;
        float dustClock, ghostClock, attackDirector;
        readonly System.Random rng;
        public sealed class Projectile { public Vector2 position, velocity; public bool friendly; public float life; public SpriteRenderer renderer; public Actor source; }

        public GameWorld(int index, Transform parent, float checkpointX = 1.7f)
        {
            zone = ZoneDefinition.Create(index); rng = new System.Random(1566 + index);
            actorsRoot = new GameObject("Actors and projectiles").transform; actorsRoot.SetParent(parent, false);
            effects = new Effects(parent);
            checkpoint = checkpointX;
            player = new Actor { id = 0, player = true, position = new Vector2(checkpointX, zone.floor), maxGuard = 100, guard = 100 };
            new PaintedRig(player, actorsRoot);
            Spawn(10.0f, zone.floor, EnemyRole.Duelist);
            Spawn(18.1f, zone.floor, EnemyRole.Shield);
            Spawn(25.5f, -.75f, EnemyRole.Archer);
            Spawn(35.1f, zone.floor, index == 1 ? EnemyRole.Ghoul : EnemyRole.Duelist);
            Spawn(42.3f, zone.floor, EnemyRole.Shield);
            Spawn(50.4f, zone.floor, EnemyRole.Captain, true);
            previousPlayerTip = player.rig.BladeTip;
        }

        void Spawn(float x, float y, EnemyRole role, bool boss = false)
        {
            var a = new Actor { id = enemies.Count + 1, role = role, boss = boss, position = new Vector2(x, y), face = -1,
                health = boss ? 210 + zone.index * 20 : role == EnemyRole.Shield ? 95 : 70, maxGuard = boss ? 95 : role == EnemyRole.Shield ? 75 : 35,
                size = boss ? 1.09f : .98f, intentTime = .3f + enemies.Count * .11f };
            a.maxHealth = a.health; a.guard = a.maxGuard; new PaintedRig(a, actorsRoot); enemies.Add(a);
        }

        public void Simulate(float dt, InputFrame input, bool enemyAI = true)
        {
            clock += dt; notificationTime -= dt; attackDirector -= dt;
            TickTimers(player, dt);
            TickPlayer(dt, input);
            player.rig.Render(player, dt, clock);
            ResolveBlade(player);
            foreach (var a in enemies)
            {
                TickTimers(a, dt);
                if (enemyAI) TickEnemy(a, dt); else a.velocity.x = 0;
                a.rig.Render(a, dt, clock);
                if (enemyAI) ResolveBlade(a);
            }
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
            { dustClock = .17f; effects.Dust(player.position, player.face, zone.snow); }
            if (player.state == ActionState.Spirit && ghostClock <= 0)
            { ghostClock = .065f; player.rig.AfterImage(effects, new Color(1, .80f, .45f, .30f)); }
            Vector2 tip = player.rig.BladeTip;
            if (player.Attacking && Active(player)) effects.SwordTrace(previousPlayerTip, tip, new Color(1f, .88f, .69f, .82f));
            previousPlayerTip = tip;
        }

        void TickTimers(Actor a, float dt)
        {
            a.stateTime += dt; a.cooldown -= dt; a.invincible -= dt; a.parryWindow -= dt; a.hitFlash -= dt; a.dropTimer -= dt; a.skillCooldown -= dt; a.shotCooldown -= dt; a.staminaDelay -= dt;
            if (!a.Locked && a.staminaDelay <= 0) a.stamina = Mathf.MoveTowards(a.stamina, 100, dt * (a.state == ActionState.Block ? 6 : 24));
            if (!a.Attacking) a.guard = Mathf.MoveTowards(a.guard, a.maxGuard, dt * 7);
        }

        void TickPlayer(float dt, InputFrame input)
        {
            var a = player;
            if (a.Dead) { a.velocity.x = Mathf.MoveTowards(a.velocity.x, 0, dt * 12); KinematicMotor.Step(a, zone, dt); return; }
            a.jumpBuffer = Mathf.Max(0, a.jumpBuffer - dt); if (input.jump) a.jumpBuffer = .13f;
            a.coyote = a.grounded ? .10f : Mathf.Max(0, a.coyote - dt);
            if (a.Attacking && input.light && a.Progress > .20f) a.bufferedLight = true;
            if (a.Locked && a.stateTime >= a.duration)
            {
                bool chain = a.state == ActionState.Light && a.bufferedLight && a.stamina >= 10;
                int combo = a.combo % 3 + 1; a.bufferedLight = false; a.Enter(ActionState.Idle);
                if (chain) BeginAttack(a, false, combo);
            }
            if (!a.Locked)
            {
                if (Mathf.Abs(input.move) > .01f) a.face = input.move > 0 ? 1 : -1;
                bool wasBlocking = a.state == ActionState.Block;
                if (input.block && a.grounded && a.stamina > 4)
                { if (!wasBlocking) { a.Enter(ActionState.Block); a.parryWindow = .14f; } }
                else if (wasBlocking || a.state == ActionState.Parry) a.Enter(ActionState.Idle);
                if (input.heal && a.potions > 0 && a.health < a.maxHealth && a.grounded)
                { a.Enter(ActionState.Heal, .8f); a.potions--; sound?.Invoke("heal"); }
                else if (input.dodge && a.stamina >= 22 && a.grounded)
                { a.Enter(ActionState.Dodge, .40f); a.stamina -= 22; a.invincible = .29f; a.staminaDelay = .7f; sound?.Invoke("dodge"); }
                else if (input.spirit && a.skillCooldown <= 0 && a.stamina >= 28)
                { a.Enter(ActionState.Spirit, .54f); a.stamina -= 28; a.skillCooldown = 8; a.invincible = .42f; effects.Wolf(a.position, a.face); sound?.Invoke("spirit"); }
                else if (input.shoot && a.ammo > 0 && a.shotCooldown <= 0)
                { a.Enter(ActionState.Shoot, .55f); a.ammo--; a.shotCooldown = 1.1f; }
                else if (a.state != ActionState.Block && input.heavy && a.stamina >= 28) BeginAttack(a, true, 1);
                else if (a.state != ActionState.Block && input.light && a.stamina >= 10) BeginAttack(a, false, 1);
                if (!a.Locked && a.state != ActionState.Block && a.jumpBuffer > 0 && a.coyote > 0)
                {
                    if (input.down && a.position.y > zone.floor + .2f) { a.dropTimer = .24f; a.position.y -= .08f; a.velocity.y = -1; }
                    else { a.velocity.y = 8.7f; sound?.Invoke("jump"); }
                    a.grounded = false; a.jumpBuffer = a.coyote = 0; a.rig.ResetContacts();
                }
            }
            float desired = input.move * (input.slow ? 2.15f : 4.25f);
            if (a.state == ActionState.Block) desired *= .24f;
            if (a.state is ActionState.Heal or ActionState.Shoot) desired = 0;
            if (a.Attacking) desired = a.face * (Active(a) ? a.state == ActionState.Heavy ? 2.5f : a.state == ActionState.Spirit ? 8.1f : 1.8f : a.state == ActionState.Spirit ? 7.5f : 0);
            if (a.state == ActionState.Dodge) desired = a.face * 8.2f;
            if (a.state == ActionState.Hurt) desired = a.knockback * (1 - a.Progress);
            a.velocity.x = Mathf.MoveTowards(a.velocity.x, desired, dt * (a.Locked ? 65 : a.grounded ? 25 : 12));
            bool landed = !a.grounded;
            KinematicMotor.Step(a, zone, dt, !input.jumpHeld);
            if (landed && a.grounded) { a.rig.ResetContacts(); effects.Dust(a.position, a.face, zone.snow); }
            if (!a.Locked && a.state != ActionState.Block)
                a.state = !a.grounded ? a.velocity.y > 0 ? ActionState.Jump : ActionState.Fall : Mathf.Abs(a.velocity.x) > .1f ? ActionState.Move : ActionState.Idle;
            if (a.state == ActionState.Heal && !a.attackConnected && a.Progress > .55f)
            { a.attackConnected = true; a.health = Mathf.Min(a.maxHealth, a.health + 48); effects.GlowBurst(a.position + Vector2.up, new Color(.5f, 1, .72f)); }
            if (a.state == ActionState.Shoot && !a.attackConnected && a.Progress > .35f)
            { a.attackConnected = true; Shoot(a, true); sound?.Invoke("shot"); }
        }

        void BeginAttack(Actor a, bool heavy, int combo)
        {
            a.combo = combo; a.Enter(heavy ? ActionState.Heavy : ActionState.Light, heavy ? .80f : combo == 3 ? .47f : .44f);
            if (a.player) { a.stamina -= heavy ? 28 : 10; a.staminaDelay = .45f; }
            sound?.Invoke(heavy ? "heavy" : "swing");
        }

        void TickEnemy(Actor a, float dt)
        {
            if (a.Dead) { a.velocity.x = Mathf.MoveTowards(a.velocity.x, 0, dt * 12); KinematicMotor.Step(a, zone, dt); return; }
            a.intentTime -= dt;
            if (a.Locked && a.stateTime >= a.duration) { a.Enter(ActionState.Idle); a.cooldown = a.boss ? .46f : .7f; }
            float dx = player.position.x - a.position.x, dist = Mathf.Abs(dx), height = Mathf.Abs(player.position.y - a.position.y);
            bool awake = !player.Dead && dist < (a.boss ? 10 : 8.6f);
            float vx = 0;
            if (!a.Locked && awake)
            {
                a.face = dx > 0 ? 1 : -1;
                if (a.role == EnemyRole.Archer && dist < 8 && height < 3)
                {
                    if (a.cooldown <= 0 && a.intentTime <= 0) { a.Enter(ActionState.Shoot, .94f); a.cooldown = 1.7f; }
                    else a.state = ActionState.Idle;
                }
                else if (dist > (a.boss ? 1.56f : 1.48f))
                {
                    vx = a.face * (a.role == EnemyRole.Ghoul ? 1.9f : a.boss ? 1.7f : 1.35f); a.state = ActionState.Move;
                    if (!GroundAhead(a, a.face * .34f) && a.grounded && dist > 2) { a.velocity.y = 8.4f; a.grounded = false; a.rig.ResetContacts(); }
                }
                else if (height < .8f)
                {
                    bool shield = a.role is EnemyRole.Shield or EnemyRole.Captain;
                    if (shield && a.guard > 12 && player.Attacking && player.state != ActionState.Heavy && a.intentTime <= 0) a.state = ActionState.Block;
                    else if (a.cooldown <= 0 && attackDirector <= 0 && a.intentTime <= 0)
                    { BeginAttack(a, a.boss && rng.NextDouble() > .45, 1); attackDirector = .26f; a.cooldown = .8f; }
                    else a.state = ActionState.Idle;
                }
            }
            if (a.Attacking) vx = Active(a) ? a.face * .85f : 0;
            if (a.state == ActionState.Hurt) vx = a.knockback * (1 - a.Progress);
            a.velocity.x = Mathf.MoveTowards(a.velocity.x, vx, dt * 24); KinematicMotor.Step(a, zone, dt);
            if (a.state == ActionState.Shoot && !a.attackConnected && a.Progress > .64f) { a.attackConnected = true; Shoot(a, false); }
        }

        bool GroundAhead(Actor a, float offset)
        { foreach (var s in zone.surfaces) if (a.position.x + offset >= s.left && a.position.x + offset <= s.right && Mathf.Abs(s.y - a.position.y) < .10f) return true; return false; }

        static bool Active(Actor a) => a.state == ActionState.Spirit ? a.Progress > .15f && a.Progress < .85f : a.state == ActionState.Heavy ? a.Progress > .44f && a.Progress < .73f : a.Progress > .27f && a.Progress < .64f;

        void ResolveBlade(Actor source)
        {
            if (!source.Attacking || !Active(source) || source.Dead) return;
            if (source.player) { foreach (var target in enemies) TryBlade(source, target); }
            else TryBlade(source, player);
        }

        void TryBlade(Actor source, Actor target)
        {
            if (target.Dead || source.hitTargets.Contains(target.id)) return;
            Vector2 p = target.position;
            var box = new Rect(p.x - .24f * target.size, p.y + .20f, .48f * target.size, 1.91f * target.size);
            bool hit = SegmentHits(source.rig.BladeRoot, source.rig.BladeTip, box);
            if (source.state == ActionState.Spirit) hit |= Vector2.Distance(source.position, target.position) < 1.6f;
            if (!hit) return;
            source.hitTargets.Add(target.id);
            float damage = source.player ? source.state == ActionState.Heavy ? 44 : source.state == ActionState.Spirit ? 46 : source.combo == 3 ? 29 : 20 : source.boss ? source.state == ActionState.Heavy ? 25 : 19 : 14;
            Damage(target, damage, source.face, source.state is ActionState.Heavy or ActionState.Spirit, false, source);
        }

        public static bool SegmentHits(Vector2 a, Vector2 b, Rect box)
        {
            float low = 0, high = 1; Vector2 d = b - a;
            return Clip(-d.x, a.x - box.xMin, ref low, ref high) && Clip(d.x, box.xMax - a.x, ref low, ref high) && Clip(-d.y, a.y - box.yMin, ref low, ref high) && Clip(d.y, box.yMax - a.y, ref low, ref high);
        }
        static bool Clip(float p, float q, ref float low, ref float high)
        { if (Mathf.Abs(p) < .00001f) return q >= 0; float r = q / p; if (p < 0) { if (r > high) return false; low = Mathf.Max(low, r); } else { if (r < low) return false; high = Mathf.Min(high, r); } return true; }

        public void Damage(Actor target, float damage, int direction, bool heavy, bool unblockable, Actor source = null)
        {
            if (target.Dead || target.invincible > 0 && !unblockable) return;
            bool frontal = source == null || (source.position.x - target.position.x) * target.face >= 0;
            if (!unblockable && frontal && target.state == ActionState.Block)
            {
                if (target.player && target.parryWindow > 0)
                {
                    target.Enter(ActionState.Parry, .15f); target.stamina = Mathf.Min(100, target.stamina + 12); effects.Impact(target.position + Vector2.up * 1.45f, -direction, true, true); Notify("SAVUŞTURMA"); sound?.Invoke("parry");
                    if (source != null) { source.Enter(ActionState.Hurt, .9f); source.guard = 0; source.knockback = -direction * 2; }
                    return;
                }
                float guardDamage = heavy ? 65 : damage * 1.3f;
                if (target.player) target.stamina -= guardDamage; else target.guard -= guardDamage;
                effects.Impact(target.position + Vector2.up * 1.35f, direction, true, heavy); sound?.Invoke("block");
                if ((target.player ? target.stamina : target.guard) > 0) return;
                Notify("SAVUNMA KIRILDI"); damage *= .6f;
            }
            target.health = Mathf.Max(0, target.health - damage); target.hitFlash = .14f; target.invincible = target.player ? .40f : .14f;
            target.knockback = direction * (heavy ? 3.1f : 1.5f); target.Enter(target.health <= 0 ? ActionState.Dead : ActionState.Hurt, heavy ? .6f : .28f);
            effects.Impact(target.position + Vector2.up * 1.25f, direction, false, heavy); sound?.Invoke("hit");
            if (target.Dead && !target.player)
            { kills++; player.stamina = Mathf.Min(100, player.stamina + 14); if (target.boss) { Notify(zone.bossName + " yenildi. İç kapı açıldı."); player.ammo += 4; } }
        }

        void Shoot(Actor a, bool friendly)
        {
            Vector2 position = a.rig.BladeRoot + new Vector2(a.face * (friendly ? .46f : .08f), friendly ? .06f : 0);
            Vector2 velocity = Vector2.right * a.face * (friendly ? 19 : 10);
            if (!friendly)
            {
                Vector2 aim = player.position + Vector2.up * 1.2f - position;
                if (aim.x * a.face > 0) velocity = aim.normalized * 10;
            }
            var sr = ArtLibrary.Draw(friendly ? "Pistol shot" : "Arrow", ArtLibrary.White, actorsRoot, position, 170, friendly ? new Color(1, .88f, .63f) : new Color(.7f, .59f, .43f));
            sr.transform.localScale = new Vector3(.23f, .025f, 1);
            sr.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);
            projectiles.Add(new Projectile { position = position, velocity = velocity, life = 1.4f, friendly = friendly, renderer = sr, source = a });
            effects.GlowBurst(position, new Color(1, .62f, .25f, .65f));
        }

        void TickProjectiles(float dt)
        {
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                var p = projectiles[i]; Vector2 old = p.position; p.position += p.velocity * dt; p.life -= dt;
                if (p.friendly) { foreach (var a in enemies) if (!a.Dead && SegmentHits(old, p.position, new Rect(a.position.x - .3f, a.position.y + .2f, .6f, 2.1f))) { Damage(a, 38, p.velocity.x > 0 ? 1 : -1, true, false, p.source); p.life = 0; break; } }
                else if (SegmentHits(old, p.position, new Rect(player.position.x - .23f, player.position.y + .2f, .46f, 1.95f))) { Damage(player, 12, p.velocity.x > 0 ? 1 : -1, false, false, p.source); p.life = 0; }
                p.renderer.transform.position = p.position;
                if (p.life <= 0) { if (Application.isPlaying) UnityEngine.Object.Destroy(p.renderer.gameObject); else UnityEngine.Object.DestroyImmediate(p.renderer.gameObject); projectiles.RemoveAt(i); }
            }
        }

        public void Notify(string text) { notification = text; notificationTime = 2.2f; }
        public string InteractionHint
        {
            get { foreach (float x in shrines) if (Mathf.Abs(player.position.x - x) < 1.4f) return "E · Yol taşında dinlen"; return player.position.x > zone.length - 3 ? cleared ? "E · İç kapıdan ilerle" : "Kapı mühürlü · Komutanı yen" : ""; }
        }
    }
}
