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

        // Evre kapısı: phase 0 iki evrede, 1 / 2 yalnız o evrede
        public static bool Allowed(Actor a, EnemyAttack k) => k.phase == 0 || k.phase == a.phase;

        // Adıyla saldırı başlatır (testler ve zincirleme saldırılar için)
        public bool StartAttack(Actor a, string name)
        {
            var k = EnemyProfile.For(a.role).Find(name);
            if (k == null || !Allowed(a, k)) return false;
            Begin(a, k); return true;
        }

        void Begin(Actor a, EnemyAttack k)
        {
            var state = k.kind switch { AttackKind.Reload => ActionState.Shoot, AttackKind.Backstep or AttackKind.Teleport => ActionState.Dodge, _ => ActionState.Light };
            a.Enter(state, k.Duration); a.attack = k; a.combo = 1;
            a.attackReady[k.name] = clock + k.cooldown * (a.phase > 1 ? .6f : 1);
            if (k.kind == AttackKind.Teleport) a.invincible = k.Duration;
            if (k.kind == AttackKind.LeapOver)
            {
                // Oyuncunun üstünden arkasına; arkası sağlam değilse önüne iner
                float side = Mathf.Sign(player.position.x - a.position.x);
                float behind = player.position.x + side * 1.6f, front = player.position.x - side * 1.6f;
                Leap(a, Standable(behind, out _) ? behind : front, k.active);
            }
            if (state == ActionState.Light && !k.Ranged) sound?.Invoke(k.heavy || k.red ? "heavy" : "swing");
        }

        void TickEnemy(Actor a, float dt)
        {
            if (a.Dead) { SettleCorpse(a, dt); return; }
            var p = EnemyProfile.For(a.role);
            a.intentTime -= dt;
            if (p.phaseAt > 0 && a.phase == 1 && a.health <= a.maxHealth * p.phaseAt)
            {
                a.phase = 2; a.attackReady.Clear();   // evreye özel saldırılar (çağrı gibi) bu evrede yeniden açılır
                Notify((a.boss ? zone.bossName : p.title).ToUpper(System.Globalization.CultureInfo.GetCultureInfo("tr-TR")) + " · İKİNCİ EVRE");
                if (p.phaseShift != null && StartAttack(a, p.phaseShift)) a.invincible = Mathf.Max(a.invincible, a.duration);
            }
            var k = a.attack;
            // Hazırlığın başında silahta parıltı: beyaz savuşturulabilir, kırmızı yalnız kaçışla atlatılır
            if (k != null && a.state == ActionState.Light && !a.telegraphed && a.stateTime >= k.windup * .25f)
            { a.telegraphed = true; effects.Glint(a.view.BladeTip, a.boss || p.halfBoss, k.red); }
            if (a.Locked && a.stateTime >= a.duration)
            {
                string next = a.state == ActionState.Hurt ? null : k?.then;
                a.Enter(ActionState.Idle);
                if (next != null) { a.face = player.position.x > a.position.x ? 1 : -1; StartAttack(a, next); } else a.cooldown = a.boss ? .46f : .7f;
            }
            if (a.state == ActionState.Block && a.stateTime >= a.duration) a.Enter(ActionState.Idle);   // kalkan klibi bitti
            float dx = player.position.x - a.position.x, dist = Mathf.Abs(dx), height = Mathf.Abs(player.position.y - a.position.y);
            bool awake = !player.Dead && dist < (a.boss || p.halfBoss ? 10 : 8.6f) + (p.ranged ? 3 : 0);
            float vx = 0, speed = p.speed * (a.phase > 1 ? p.phase2Speed : 1);
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
                bool slot = choice != null && (choice.kind is AttackKind.Backstep or AttackKind.Teleport or AttackKind.LeapOver || Attackers < MaxAttackers);
                if (choice != null && ready && slot && attackDirector <= 0) { Begin(a, choice); attackDirector = .26f; a.cooldown = .8f; }
                else if (a.state == ActionState.Block) { }   // kalkan klibi (~.2 sn): yerinde siper alır; saldırı yine başlayabilir
                else if (dist > p.keepMax) { vx = a.face * speed; a.state = ActionState.Move; }
                else if (dist < p.keepMin) { vx = -a.face * speed * .8f; a.state = ActionState.Move; }   // yüzü oyuncuda, geri çekilir
                else a.state = ActionState.Idle;
                if (vx != 0 && a.grounded && !GroundAhead(a, Mathf.Sign(vx) * .34f))
                {
                    // Önde boşluk: yakın dövüşçü oyuncuya doğruysa atlar; uzakçı yerini (platformunu) korur; değilse kenarda dur
                    if (!p.ranged && Mathf.Sign(vx) == a.face && dist > 2) { a.velocity.y = 8.4f; a.grounded = false; a.view.ResetContacts(); }
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
            // Sıçrayışta yatay hız korunur (havada yön değiştirmez)
            bool leaping = k != null && (k.kind is AttackKind.LeapOver or AttackKind.LeapSlam) && !a.grounded;
            if (!leaping) a.velocity.x = Mathf.MoveTowards(a.velocity.x, vx, dt * (k != null && k.dash != 0 ? 80 : 24));
            KinematicMotor.Step(a, zone, dt);
        }

        // Sıçrayış: time sn'de targetX'e aynı yüksekliğe iner (yerçekimi 20: dikey hız 10 x time)
        void Leap(Actor a, float targetX, float time)
        {
            a.velocity = new Vector2((targetX - a.position.x) / time, 10 * time); a.grounded = false; a.view.ResetContacts();
        }

        // Mesafeye ve yüksekliğe uyan, beklemesi dolmuş saldırılardan biri; 'first' işaretli olan önceliklidir
        EnemyAttack Choose(Actor a, EnemyProfile p, float dist, float height)
        {
            EnemyAttack pick = null; int seen = 0;
            foreach (var k in p.attacks)
            {
                if (!Allowed(a, k)) continue;
                if (k.grab && (player.Attacking || player.invincible > 0)) continue;   // yakalama yalnız boşta duran oyuncuya
                if (k.reach <= 0 || dist < k.minRange || dist > k.reach) continue;
                // Humbara da uzak saldırı yüksekliğiyle seçilir (platformdaki oyuncuya atılır); Ranged'e eklenmez, çünkü o savuruş sesini de belirler
                if (height > (k.Ranged || k.kind == AttackKind.Grenade ? 3 : .8f)) continue;
                if (a.attackReady.TryGetValue(k.name, out float ready) && clock < ready) continue;
                if (k.first) return k;
                if (rng.Next(++seen) == 0) pick = k;
            }
            return pick;
        }

        // Etkin pencere: her vuruşun ilk .12 sn'si; vuruş anları (hitTimes) verilmişse eşit aralık yerine onlar
        static bool InStrike(Actor a, out int strike)
        {
            var k = a.attack; float t = a.stateTime - k.windup;
            if (k.hitTimes != null)
            {
                strike = 0; while (strike + 1 < k.hitTimes.Length && t >= k.hitTimes[strike + 1]) strike++;
                return t >= 0 && t <= k.active && t - k.hitTimes[strike] < .12f;
            }
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
                    // Yanan sancak: savuruşun ilk etkin anında önüne (boss'un zemininde) kısa ömürlü ateş bırakır
                    if (k.patchLife > 0 && !a.attackConnected)
                    { a.attackConnected = true; SpawnFire(new Vector2(a.position.x + a.face * k.reach * .5f, a.position.y), k.patchWidth, k.patchLife, k.patchDamage, a); }
                    if (strike != a.strike) { a.strike = strike; a.hitTargets.Clear(); }
                    if (a.hitTargets.Contains(player.id) || !MeleeReach(a, k)) return;
                    a.hitTargets.Add(player.id);
                    // Yere vurma halkası: arkadaki oyuncu da dışa (kendi tarafına) itilir
                    int dir = k.kind == AttackKind.Slam ? (int)Mathf.Sign(player.position.x - a.position.x) : a.face;
                    bool avoided = player.invincible > 0;   // kaçışın dokunulmazlığı itmeyi de önler
                    Damage(player, k.damage, dir, k.heavy, false, a, -1, k.red);
                    if (k.push > 0 && player.state == ActionState.Hurt) player.knockback = dir * k.push;
                    // Kalkanla itme: blok ya da savuşturma da olsa geri iter ve dayanıklılığı düşürür
                    if (!avoided && !player.Dead && k.pushBack > 0) { player.pushTime = .2f; player.pushSpeed = dir * k.pushBack / .2f; }
                    if (!avoided && !player.Dead && k.staminaDrain > 0) { player.stamina = Mathf.Max(0, player.stamina - k.staminaDrain); player.staminaDelay = .7f; }
                    return;
                case AttackKind.LeapSlam:
                    // Hazırlık bitince oyuncunun bulunduğu yere sıçrar; yere değdiği an çevresine kırmızı çakılma
                    if (a.state != ActionState.Light) return;
                    if (!a.attackConnected && a.stateTime >= k.windup) { a.attackConnected = true; Leap(a, player.position.x, .8f); return; }
                    if (a.attackConnected && a.strike < 0 && a.grounded && a.stateTime > k.windup + .1f)
                    {
                        a.strike = 0; effects.cameraShake = Mathf.Max(effects.cameraShake, .12f);
                        effects.GlowBurst(a.position + Vector2.up * .3f, new Color(1, .8f, .55f));
                        if (Mathf.Abs(player.position.x - a.position.x) <= k.radius && Mathf.Abs(player.position.y - a.position.y) < 1.2f)
                            Damage(player, k.damage, (int)Mathf.Sign(player.position.x - a.position.x), true, false, a, -1, k.red);
                    }
                    return;
                case AttackKind.LeapOver:
                    return;
                case AttackKind.Pull:
                {
                    if (a.state != ActionState.Light || !InStrike(a, out _) || a.hitTargets.Contains(player.id)) return;
                    float d = (player.position.x - a.position.x) * a.face;
                    if (d < -.2f || d > k.reach || Mathf.Abs(player.position.y - a.position.y) > 1.2f) return;
                    a.hitTargets.Add(player.id);
                    Damage(player, k.damage, a.face, false, false, a, -1, k.red);
                    // Kanca tuttuysa (blok, savuşturma ya da kaçış değilse) oyuncu ayağının dibine çekilir
                    if (player.state == ActionState.Hurt && !player.Dead) { player.pushTime = .25f; player.pushSpeed = -a.face * Mathf.Max(0, d - 1.1f) / .25f; }
                    return;
                }
                case AttackKind.Teleport:
                    if (a.stateTime >= k.windup && !a.attackConnected) { a.attackConnected = true; Teleport(a); }
                    return;
                case AttackKind.Reload: case AttackKind.Backstep:
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
                    // Tüfek boş kalır: doldurma bölünse de atış + toparlanma + doldurma süresi dolmadan yeniden nişan alınmaz
                    var reload = k.then != null ? EnemyProfile.For(a.role).Find(k.then) : null;
                    if (k.kind == AttackKind.Bullet && reload != null) a.attackReady[k.name] = clock + k.active + k.recovery + reload.Duration;
                    break;
                }
                case AttackKind.Volley:
                {
                    // Düşeceği yer önceden kırmızı işaretle belli olur; oklar .8 sn sonra yukarıdan iner
                    float x = player.position.x, y = player.position.y;
                    AddShot(ShotKind.VolleyMark, new Vector2(x, y + .05f), Vector2.zero, .8f + 5f / 16 + .05f, k, a);   // işaret oklar inene dek görünür kalır
                    for (int i = -1; i <= 1; i++) AddShot(ShotKind.VolleyArrow, new Vector2(x + i * .8f, y + 5), new Vector2(0, -16), 5f / 16 + .05f, k, a, .8f);
                    break;
                }
                case AttackKind.FlameWave:
                {
                    // Yerde ilerleyen kara alev; ikinci evrede yarım saniye arayla iki dalga
                    Vector2 start = new Vector2(a.position.x + a.face * .6f, a.position.y);
                    int waves = a.phase > 1 ? 2 : 1;
                    for (int i = 0; i < waves; i++) AddShot(ShotKind.Wave, start, new Vector2(a.face * 6, 0), 2, k, a, i * .5f);
                    break;
                }
                case AttackKind.Orbs:
                    for (int i = 0; i < 3; i++) AddShot(ShotKind.Orb, hand + new Vector2(0, .4f * (i - 1)), new Vector2(a.face * (1 + i * .4f), .6f * (i - 1)), 4, k, a);
                    break;
                case AttackKind.Summon:
                    // Sancağı diker: iki yanında, yalnız sağlam zeminde birer nefer; döngü bitince doğarlar
                    for (int i = 0; i < k.summonCount; i++)
                    {
                        float x = a.position.x + (i % 2 == 0 ? -2.2f : 2.2f) * (1 + i / 2);
                        if (Standable(x, out float y)) spawnQueue.Add((new Vector2(x, y), k.summon));
                    }
                    break;
                case AttackKind.Rage:
                    effects.GlowBurst(a.position + Vector2.up * 1.4f, new Color(1, .35f, .15f));
                    break;
                case AttackKind.Grenade:
                {
                    // Kavisli atış: .9 sn'de oyuncunun bulunduğu yere düşer; düşeceği yer önceden kızıl işaretli
                    const float flight = .9f;
                    Vector2 target = new Vector2(player.position.x, player.position.y);
                    var mark = AddShot(ShotKind.VolleyMark, target + Vector2.up * .05f, Vector2.zero, flight, k, a);
                    mark.renderer.transform.localScale = new Vector3(Mathf.Max(1.6f, k.patchWidth), .35f, 1);
                    var v = new Vector2((target.x - hand.x) / flight, (target.y - hand.y) / flight + 10 * flight);
                    var pot = AddShot(ShotKind.Grenade, hand, v, flight, k, a); pot.target = target; pot.attack = k;
                    break;
                }
                case AttackKind.ArenaFire:
                {
                    // Arenanın yarısı tutuşur: boss'un arkası (home+1.5 … +7) ve giriş yanı (home-8 … -5); ortada dar alan kalır.
                    // Ateş .8 sn önce kızıl işaretle belli olur
                    (float from, float to)[] spans = { (a.home + 1.5f, a.home + 7), (a.home - 8, a.home - 5) };
                    foreach (var (from, to) in spans)
                    {
                        var at = new Vector2((from + to) * .5f, zone.floor); float width = to - from;
                        var mark = AddShot(ShotKind.VolleyMark, at + Vector2.up * .05f, Vector2.zero, .8f, k, a);
                        mark.renderer.transform.localScale = new Vector3(width, .35f, 1);
                        SpawnFire(at, width, 999, k.patchDamage, a, .8f);
                    }
                    break;
                }
            }
        }

        // Duman olup oyuncunun arkasına, olmazsa uzağa geçer; yalnız sağlam (tek yönlü olmayan) zemine iner
        void Teleport(Actor a)
        {
            var smoke = new Color(.35f, .1f, .45f);
            effects.GlowBurst(a.position + Vector2.up, smoke);
            float away = -Mathf.Sign(player.position.x - a.position.x);
            float[] options = { player.position.x - player.face * 2.2f, a.position.x + away * 5, player.position.x + player.face * 3 };
            foreach (float x in options)
                if (Standable(x, out float y)) { a.position = new Vector2(x, y); a.velocity = Vector2.zero; a.view.ResetContacts(); break; }
            a.face = player.position.x > a.position.x ? 1 : -1;
            effects.GlowBurst(a.position + Vector2.up, smoke);
        }

        bool Standable(float x, out float y)
        {
            foreach (var s in zone.surfaces) if (!s.oneWay && x >= s.left + .3f && x <= s.right - .3f) { y = s.y; return true; }
            y = 0; return false;
        }

        // Arayüzün can çubuğu: yakındaki boss ya da yarı-boss
        public Actor BarTarget
        {
            get
            {
                Actor best = null; float near = 9;
                foreach (var e in enemies)
                {
                    if (e.Dead || !(e.boss || EnemyProfile.For(e.role).halfBoss)) continue;
                    float d = Mathf.Abs(e.position.x - player.position.x); if (d < near) { near = d; best = e; }
                }
                return best;
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
                ShotKind.Grenade => (ArtLibrary.Glow, new Color(.55f, .35f, .2f, 1), new Vector3(.3f, .3f, 1)),
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
            float r = k.hitReach > 0 ? k.hitReach : k.reach;
            return k.kind == AttackKind.Slam ? Mathf.Abs(d) <= r : d >= -.2f && d <= r + .23f;
        }
    }
}
