using UnityEngine;

namespace KaramaniReborn
{
    // Düşman davranışı: mesafe, saldırı seçimi ve saldırının yürütülmesi. Değerler EnemyProfiles.cs'te.
    public sealed partial class GameWorld
    {
        // Adıyla saldırı başlatır (testler ve zincirleme saldırılar için)
        public bool StartAttack(Actor a, string name)
        {
            var k = EnemyProfile.For(a.role).Find(name);
            if (k == null) return false;
            Begin(a, k); return true;
        }

        void Begin(Actor a, EnemyAttack k)
        {
            var state = k.kind switch { AttackKind.Reload => ActionState.Shoot, AttackKind.Backstep or AttackKind.Teleport => ActionState.Dodge, _ => ActionState.Light };
            a.Enter(state, k.Duration); a.attack = k; a.combo = 1;
            a.attackReady[k.name] = clock + k.cooldown * (a.phase > 1 ? .6f : 1);
            if (k.kind == AttackKind.Teleport) a.invincible = k.Duration;
            if (state == ActionState.Light && !k.Ranged) sound?.Invoke(k.heavy || k.red ? "heavy" : "swing");
        }

        void TickEnemy(Actor a, float dt)
        {
            if (a.Dead) { SettleCorpse(a, dt); return; }
            var p = EnemyProfile.For(a.role);
            a.intentTime -= dt;
            var k = a.attack;
            // Hazırlığın başında silahta parıltı: beyaz savuşturulabilir, kırmızı yalnız kaçışla atlatılır
            if (k != null && a.state == ActionState.Light && !a.telegraphed && a.stateTime >= k.windup * .25f)
            { a.telegraphed = true; effects.Glint(a.view.BladeTip, a.boss || p.halfBoss, k.red); }
            if (a.Locked && a.stateTime >= a.duration)
            {
                string next = a.state == ActionState.Hurt ? null : k?.then;
                a.Enter(ActionState.Idle);
                if (next != null) StartAttack(a, next); else a.cooldown = a.boss ? .46f : .7f;
            }
            float dx = player.position.x - a.position.x, dist = Mathf.Abs(dx), height = Mathf.Abs(player.position.y - a.position.y);
            bool awake = !player.Dead && dist < (a.boss || p.halfBoss ? 10 : 8.6f) + (p.ranged ? 3 : 0);
            float vx = 0;
            if (!a.Locked && awake)
            {
                a.face = dx > 0 ? 1 : -1;
                var choice = a.intentTime <= 0 ? Choose(a, p, dist, height) : null;
                if (choice != null && a.cooldown <= 0 && attackDirector <= 0) { Begin(a, choice); attackDirector = .26f; a.cooldown = .8f; }
                else if (dist > p.keepMax) { vx = a.face * p.speed; a.state = ActionState.Move; }
                else a.state = ActionState.Idle;
                if (vx != 0 && a.grounded && !GroundAhead(a, Mathf.Sign(vx) * .34f))
                {
                    // Önde boşluk: oyuncuya doğruysa atla, değilse kenarda dur
                    if (Mathf.Sign(vx) == a.face && dist > 2) { a.velocity.y = 8.4f; a.grounded = false; a.view.ResetContacts(); }
                    else { vx = 0; a.state = ActionState.Idle; }
                }
            }
            k = a.attack;
            if (k != null && (a.state is ActionState.Light or ActionState.Dodge) && k.dash != 0 && a.stateTime >= k.windup && a.stateTime <= k.windup + k.active)
            {
                vx = a.face * k.dash / Mathf.Max(.05f, k.active);
                if (a.grounded && !GroundAhead(a, Mathf.Sign(vx) * .34f)) vx = 0;   // atılış / geri sıçrama kenardan taşmaz
            }
            if (a.state == ActionState.Hurt) vx = a.knockback * (1 - a.Progress);
            a.velocity.x = Mathf.MoveTowards(a.velocity.x, vx, dt * (k != null && k.dash != 0 ? 80 : 24)); KinematicMotor.Step(a, zone, dt);
        }

        // Mesafeye ve yüksekliğe uyan, beklemesi dolmuş saldırılardan biri; 'first' işaretli olan önceliklidir
        EnemyAttack Choose(Actor a, EnemyProfile p, float dist, float height)
        {
            EnemyAttack pick = null; int seen = 0;
            foreach (var k in p.attacks)
            {
                if (k.reach <= 0 || dist < k.minRange || dist > k.reach) continue;
                if (height > (k.Ranged ? 3 : .8f)) continue;
                if (a.attackReady.TryGetValue(k.name, out float ready) && clock < ready) continue;
                if (k.first) return k;
                if (rng.Next(++seen) == 0) pick = k;
            }
            return pick;
        }

        // Etkin pencere: çok vuruşlu saldırıda her vuruşun ilk .12 sn'si
        static bool InStrike(Actor a, out int strike)
        {
            var k = a.attack; float t = a.stateTime - k.windup;
            strike = k.hits > 1 ? Mathf.Clamp(Mathf.FloorToInt(t / k.hitGap), 0, k.hits - 1) : 0;
            if (t < 0 || t > k.active) return false;
            return k.hits <= 1 || t - strike * k.hitGap < Mathf.Min(.12f, k.hitGap);
        }

        void PerformAttack(Actor a)
        {
            var k = a.attack;
            if (k == null || a.Dead || a.state == ActionState.Hurt) return;
            switch (k.kind)
            {
                case AttackKind.Melee: case AttackKind.Slam:
                    if (a.state != ActionState.Light || !InStrike(a, out int strike)) return;
                    if (strike != a.strike) { a.strike = strike; a.hitTargets.Clear(); }
                    if (a.hitTargets.Contains(player.id) || !MeleeReach(a, k)) return;
                    a.hitTargets.Add(player.id);
                    Damage(player, k.damage, a.face, k.heavy, false, a, -1, k.red);
                    if (k.push > 0 && player.state == ActionState.Hurt) player.knockback = a.face * k.push;
                    return;
                default: return;
            }
        }

        bool MeleeReach(Actor a, EnemyAttack k)
        {
            if (Mathf.Abs(player.position.y - a.position.y) > 1.2f) return false;
            float d = (player.position.x - a.position.x) * a.face;
            return k.kind == AttackKind.Slam ? Mathf.Abs(d) <= k.reach : d >= -.2f && d <= k.reach + .23f;
        }
    }
}
