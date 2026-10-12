using UnityEngine;

namespace KaramaniReborn
{
    // Düşman davranışı: mesafe, saldırı seçimi ve saldırının yürütülmesi. Değerler EnemyProfiles.cs'te.
    public sealed partial class GameWorld
    {
        public const int MaxAttackers = 2;

        // Şu anda saldırı yürüten (hazırlık dahil) düşman sayısı
        public int Attackers { get { int n = 0; foreach (var e in enemies) if (!e.Dead && e.attack != null && e.state == ActionState.Light) n++; return n; } }

        // Oyuncuya dönüş; kalkanlı yavaş döner (arkasına geçmek fırsattır)
        static void Face(Actor a, EnemyProfile p, float dx, float dt)
        {
            int want = dx > 0 ? 1 : -1;
            if (want == a.face) { a.turnTimer = 0; return; }
            if (p.turnDelay > 0 && (a.turnTimer += dt) < p.turnDelay) return;
            a.face = want; a.turnTimer = 0;
        }

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
                Face(a, p, dx, dt);
                // Açık: iksir içen, ağır vuruşunu ıskalayıp toparlanan ya da savuşturma penceresini ıskalayıp blokta kalan oyuncu
                bool opening = player.state == ActionState.Heal || player.state == ActionState.Heavy && player.Progress > .73f
                    || player.state == ActionState.Block && player.stateTime > .14f && player.stateTime < .5f;
                bool facing = dx * a.face > 0;
                var choice = facing && (a.intentTime <= 0 || opening) ? Choose(a, p, dist, height) : null;
                if (choice != null && a.intentTime > 0 && !choice.punish) choice = null;   // niyet dolmadan yalnız cezalandırma saldırıları açılır
                bool ready = a.cooldown <= 0 || opening && choice != null && choice.punish;
                bool slot = choice != null && (choice.kind is AttackKind.Backstep or AttackKind.Teleport || Attackers < MaxAttackers);
                if (choice != null && ready && slot && attackDirector <= 0) { Begin(a, choice); attackDirector = .26f; a.cooldown = .8f; }
                else if (dist > p.keepMax) { vx = a.face * p.speed; a.state = ActionState.Move; }
                else if (dist < p.keepMin) { vx = -a.face * p.speed * .8f; a.state = ActionState.Move; }   // yüzü oyuncuda, geri çekilir
                else a.state = ActionState.Idle;
                if (vx != 0 && a.grounded && !GroundAhead(a, Mathf.Sign(vx) * .34f))
                {
                    // Önde boşluk: oyuncuya doğruysa atla, değilse kenarda dur
                    if (Mathf.Sign(vx) == a.face && dist > 2) { a.velocity.y = 8.4f; a.grounded = false; a.view.ResetContacts(); }
                    else { vx = 0; a.state = ActionState.Idle; }
                }
            }
            else a.turnTimer = 0;   // kilitliyken ya da uykudayken yarım kalan dönüş sayılmaz
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
                case AttackKind.Reload: case AttackKind.Backstep: case AttackKind.Teleport:
                    return;
                default:   // mermi ve büyüler: hazırlık bittiği an bir kez
                    if (a.state == ActionState.Light && a.stateTime >= k.windup && !a.attackConnected) { a.attackConnected = true; Release(a, k); }
                    return;
            }
        }

        void Release(Actor a, EnemyAttack k)
        {
            Vector2 hand = a.position + new Vector2(a.face * .5f, 1.35f * a.size);
            Vector2 chest = player.position + Vector2.up * 1.2f;
            switch (k.kind)
            {
                case AttackKind.Bullet: case AttackKind.Arrow:
                {
                    float speed = k.kind == AttackKind.Bullet ? 22 : 13;
                    Vector2 aim = chest - hand, velocity = aim.x * a.face > 0 ? aim.normalized * speed : Vector2.right * a.face * speed;
                    AddShot(k.kind == AttackKind.Bullet ? ShotKind.Bullet : ShotKind.Arrow, hand, velocity, 1.4f, k, a);
                    sound?.Invoke("shot"); effects.GlowBurst(hand, new Color(1, .62f, .25f, .65f));
                    break;
                }
                case AttackKind.Volley:
                {
                    // Düşeceği yer önceden kırmızı işaretle belli olur; oklar .8 sn sonra yukarıdan iner
                    float x = player.position.x, y = player.position.y;
                    AddShot(ShotKind.VolleyMark, new Vector2(x, y + .05f), Vector2.zero, .8f, k, a);
                    for (int i = -1; i <= 1; i++) AddShot(ShotKind.VolleyArrow, new Vector2(x + i * .8f, y + 5), new Vector2(0, -16), 5f / 16 + .05f, k, a, .8f);
                    break;
                }
            }
        }

        Projectile AddShot(ShotKind kind, Vector2 position, Vector2 velocity, float life, EnemyAttack k, Actor source, float delay = 0)
        {
            var (sprite, color, scale) = kind switch
            {
                ShotKind.Bullet => (ArtLibrary.White, new Color(1, .86f, .55f), new Vector3(.32f, .035f, 1)),
                ShotKind.Arrow or ShotKind.VolleyArrow => (ArtLibrary.White, new Color(.7f, .59f, .43f), new Vector3(.36f, .03f, 1)),
                ShotKind.VolleyMark => (ArtLibrary.Glow, new Color(1, .15f, .1f, .55f), new Vector3(2.2f, .35f, 1)),
                ShotKind.Wave => (ArtLibrary.Glow, new Color(.3f, .02f, .08f, .9f), new Vector3(.7f, 1.1f, 1)),
                _ => (ArtLibrary.Glow, new Color(.55f, .1f, .8f, .9f), new Vector3(.45f, .45f, 1)),
            };
            var sr = ArtLibrary.Draw(kind.ToString(), sprite, actorsRoot, position, 170, color);
            sr.transform.localScale = scale;
            if (kind is ShotKind.Bullet or ShotKind.Arrow or ShotKind.VolleyArrow)
                sr.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);
            sr.enabled = delay <= 0;
            var shot = new Projectile { kind = kind, position = position, velocity = velocity, life = life, delay = delay, damage = k.damage, red = k.red, renderer = sr, source = source };
            projectiles.Add(shot); return shot;
        }

        bool MeleeReach(Actor a, EnemyAttack k)
        {
            if (Mathf.Abs(player.position.y - a.position.y) > 1.2f) return false;
            float d = (player.position.x - a.position.x) * a.face;
            return k.kind == AttackKind.Slam ? Mathf.Abs(d) <= k.reach : d >= -.2f && d <= k.reach + .23f;
        }
    }
}
