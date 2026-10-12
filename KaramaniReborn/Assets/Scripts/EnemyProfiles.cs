using System.Collections.Generic;

namespace KaramaniReborn
{
    public enum AttackKind { Melee, Slam, Bullet, Arrow, Volley, FlameWave, Orbs, Reload, Backstep, Teleport }

    // Bir düşman saldırısı. Süreler saniye, mesafeler dünya birimi; oyun dengesi yalnız buradan ayarlanır.
    // Kırmızı (red) saldırı blok ve savuşturmayla durmaz: yalnız kaçışın dokunulmazlığı ya da zıplama kurtarır.
    public sealed class EnemyAttack
    {
        public string name, clip;
        public AttackKind kind = AttackKind.Melee;
        public float windup, active, recovery, minRange, reach, damage, dash, push, cooldown = .8f, hitGap;
        public int hits = 1;
        public bool red, heavy, punish, first;
        public string then;   // bitince hemen başlayan saldırı (tüfekte doldurma)
        public float Duration => windup + active + recovery;
        public bool Ranged => kind is AttackKind.Bullet or AttackKind.Arrow or AttackKind.Volley or AttackKind.FlameWave or AttackKind.Orbs;
    }

    // Düşman türü: değerler, tuttuğu mesafe ve saldırı listesi (Docs/specs/2026-10-12-enemy-roster-design.md §3)
    public sealed class EnemyProfile
    {
        public EnemyRole role;
        public string title, sheet;
        public float health, guard, speed, keepMin, keepMax, size = .98f, phaseAt, turnDelay;
        public bool halfBoss, shielded, ranged;
        public EnemyAttack[] attacks;

        public EnemyAttack Find(string name) { foreach (var a in attacks) if (a.name == name) return a; return null; }
        public static EnemyProfile For(EnemyRole role) => Table[role];

        static EnemyProfile() { foreach (var p in Table.Values) foreach (var a in p.attacks) a.clip ??= a.name; }

        static readonly Dictionary<EnemyRole, EnemyProfile> Table = new()
        {
            [EnemyRole.Nefer] = new EnemyProfile
            {
                role = EnemyRole.Nefer, title = "Kılıç Neferi", sheet = "duelist", health = 70, guard = 35, speed = 1.35f, keepMin = .9f, keepMax = 1.5f,
                attacks = new[]
                {
                    // İkinci vuruş oyuncunun .4 sn'lik dokunulmazlığından sonra iner
                    new EnemyAttack { name = "combo", windup = .35f, active = .6f, hitGap = .45f, hits = 2, recovery = .35f, reach = 1.5f, damage = 14, punish = true },
                    new EnemyAttack { name = "lunge", windup = .3f, active = .2f, recovery = .4f, minRange = 1.8f, reach = 2.6f, dash = 1.2f, damage = 16, cooldown = 2.5f },
                }
            },
            [EnemyRole.Spearman] = new EnemyProfile
            {
                role = EnemyRole.Spearman, title = "Kalkanlı Mızrakçı", sheet = "shield", health = 95, guard = 75, speed = 1.1f, keepMin = 1.4f, keepMax = 2.2f,
                shielded = true, turnDelay = .6f,
                attacks = new[]
                {
                    new EnemyAttack { name = "thrust", windup = .4f, active = .15f, recovery = .45f, reach = 2.4f, damage = 16 },
                    new EnemyAttack { name = "bash", windup = .25f, active = .12f, recovery = .4f, reach = 1.2f, damage = 8, push = 5, punish = true, cooldown = 1.5f },
                }
            },
            [EnemyRole.Halberdier] = new EnemyProfile
            {
                role = EnemyRole.Halberdier, title = "Teberli", sheet = "duelist", health = 110, guard = 90, speed = 1f, keepMin = 1.8f, keepMax = 2.6f,
                attacks = new[]
                {
                    new EnemyAttack { name = "shove", windup = .2f, active = .12f, recovery = .35f, reach = 1f, damage = 6, push = 6, punish = true, first = true, cooldown = 2 },
                    new EnemyAttack { name = "chop", windup = .55f, active = .15f, recovery = .8f, reach = 2.4f, damage = 20, heavy = true },
                    new EnemyAttack { name = "sweep", windup = .6f, active = .2f, recovery = .8f, reach = 2.8f, damage = 22, red = true, cooldown = 1.6f },
                }
            },
            [EnemyRole.Musketeer] = new EnemyProfile
            {
                role = EnemyRole.Musketeer, title = "Tüfekli", sheet = "archer", health = 55, guard = 30, speed = 1.2f, keepMin = 5, keepMax = 9, ranged = true,
                attacks = new[]
                {
                    new EnemyAttack { name = "butt", windup = .25f, active = .12f, recovery = .4f, reach = 1.3f, damage = 12, push = 4, punish = true },
                    new EnemyAttack { name = "aim", kind = AttackKind.Bullet, windup = 1.2f, active = .05f, recovery = .3f, minRange = 2, reach = 12, damage = 35, red = true, then = "reload", cooldown = 0 },
                    new EnemyAttack { name = "reload", kind = AttackKind.Reload, active = 2 },
                }
            },
            [EnemyRole.Archer] = new EnemyProfile
            {
                role = EnemyRole.Archer, title = "Okçu", sheet = "archer", health = 50, guard = 25, speed = 1.3f, keepMin = 6, keepMax = 10, ranged = true,
                attacks = new[]
                {
                    new EnemyAttack { name = "backstep", kind = AttackKind.Backstep, active = .3f, recovery = .1f, reach = 2.5f, dash = -2, first = true, cooldown = 2 },
                    new EnemyAttack { name = "dagger", windup = .25f, active = .12f, recovery = .35f, reach = 1.2f, damage = 9 },
                    new EnemyAttack { name = "arrow", clip = "shoot", kind = AttackKind.Arrow, windup = .45f, active = .05f, recovery = .4f, minRange = 2, reach = 11, damage = 14, cooldown = 1.2f },
                    new EnemyAttack { name = "volley", kind = AttackKind.Volley, windup = .6f, active = .05f, recovery = .5f, minRange = 4, reach = 11, damage = 18, red = true, cooldown = 3 },
                }
            },
            [EnemyRole.Sorcerer] = new EnemyProfile
            {
                role = EnemyRole.Sorcerer, title = "Paşanın Büyücüsü", sheet = "captain", health = 220, guard = 60, speed = 1f, keepMin = 4, keepMax = 8, size = 1,
                halfBoss = true, ranged = true, phaseAt = .5f,
                attacks = new[]
                {
                    new EnemyAttack { name = "teleport", kind = AttackKind.Teleport, windup = .35f, active = .05f, recovery = .35f, reach = 1.8f, first = true, cooldown = 4 },
                    new EnemyAttack { name = "staff", windup = .3f, active = .6f, hitGap = .45f, hits = 2, recovery = .4f, reach = 1.6f, damage = 15, punish = true },
                    new EnemyAttack { name = "slam", kind = AttackKind.Slam, windup = .5f, active = .12f, recovery = .6f, reach = 1.5f, damage = 20, red = true, push = 6, cooldown = 3 },
                    new EnemyAttack { name = "flame", kind = AttackKind.FlameWave, windup = .9f, active = .05f, recovery = .5f, minRange = 2, reach = 12, damage = 22, red = true, cooldown = 3.5f },
                    new EnemyAttack { name = "orbs", kind = AttackKind.Orbs, windup = 1f, active = .05f, recovery = .5f, minRange = 2, reach = 12, damage = 16, red = true, cooldown = 6 },
                }
            },
            // Boss'lar kendi planlarına kadar eski komutan çizimi ve bu basit kılıç / ağır vuruş setiyle
            [EnemyRole.Captain] = new EnemyProfile
            {
                role = EnemyRole.Captain, title = "Komutan", sheet = "captain", health = 210, guard = 95, speed = 1.7f, keepMin = .9f, keepMax = 1.56f, size = 1.09f,
                attacks = new[]
                {
                    new EnemyAttack { name = "slash", clip = "light1", windup = .3f, active = .3f, recovery = .35f, reach = 1.7f, damage = 19, punish = true },
                    new EnemyAttack { name = "heavy", windup = .5f, active = .25f, recovery = .5f, reach = 1.9f, damage = 25, heavy = true, cooldown = 2 },
                }
            },
        };
    }
}
