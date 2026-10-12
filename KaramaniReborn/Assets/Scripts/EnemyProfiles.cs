using System.Collections.Generic;

namespace KaramaniReborn
{
    public enum AttackKind { Melee, Slam, Bullet, Arrow, Volley, FlameWave, Orbs, Reload, Backstep, Teleport, Summon, Rage, Grenade, ArenaFire, Pull, LeapOver, LeapSlam }

    // Bir düşman saldırısı. Süreler saniye, mesafeler dünya birimi; oyun dengesi yalnız buradan ayarlanır.
    // Kırmızı (red) saldırı blok ve savuşturmayla durmaz: yalnız kaçışın dokunulmazlığı ya da zıplama kurtarır.
    public sealed class EnemyAttack
    {
        public string name, clip;
        public AttackKind kind = AttackKind.Melee;
        public float windup, active, recovery, minRange, reach, damage, dash, push, cooldown = .8f, hitGap;
        // Kalkanla itme: blok ya da savuşturma da olsa oyuncuyu pushBack birim geri iter, dayanıklılığından staminaDrain düşer
        public float pushBack, staminaDrain;
        // Boss alanları: yalnız bu evrede (0 = iki evre); yakalama (oyuncu saldırmıyor ve dokunulmaz değilken);
        // eşit aralık yerine vuruş anları; çağrı (tür, sayı); arkasında bıraktığı ateş (genişlik, süre, vuruş başı hasar);
        // sıçrayıp çakılmanın etki yarıçapı
        public int phase;
        public bool grab;
        public float[] hitTimes;
        public EnemyRole summon; public int summonCount;
        public float patchWidth, patchLife, patchDamage = 6, radius;
        // Vuruş mesafesi: seçim mesafesinden (reach) farklıysa (uzaktan başlayan hücum yalnız temas edince vurur)
        public float hitReach;
        // Zehir (ok isabet edince): saniyede poisonDamage hasar, poisonSeconds saniye; yenilenir, üst üste binmez
        public float poisonDamage, poisonSeconds;
        // Hazırlığın son leapTime saniyesinde oyuncuya doğru leap birim sıçrar (iner inmez vurur); en çok oyuncunun 1 birim önüne kadar
        public float leap, leapTime;
        // Üstünden aşan sıçrayışta oyuncunun arkasında (ya da önünde) duracağı uzaklık
        public float landGap = 1.6f;
        // Blok tutan oyuncunun kaybettiği dayanıklılık (0 = genel kural: ağır 65, diğeri hasar x 1.3)
        public float blockDrain;
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
        // Çatlak Kaya oymağının goblini: saldırı sırası insanlardan ayrı sayılır (en çok 3)
        public bool goblin;
        // Duruş: kendi saldırısını yürütürken vurulunca durmaz (can ve denge yine düşer); yalnız denge kırılınca sendeler
        public bool poise;
        // Duruş her an geçerli: hafif ve ağır vuruş saldırı dışında da sendeletmez, yalnız denge kırılınca sendeler
        public bool poiseAlways;
        // İkili sıra: yakınındaki aynı türden eşiyle (<= 3 birim) ön ve arka olarak yürür; arka olan pairGap birim gerisinde durur (0 = yok)
        public float pairGap;
        // Oyuncunun ağır vuruşunda yere serilme süresi (0 = yok): sıradan goblinler
        public float heavyKnockdown;
        // Savuşturulunca sendeleme süresi (denge kırılınca her zaman .9 sn)
        public float parryStagger = .9f;
        // Evre geçişi: canı phaseAt'e inince oynayan saldırı (dokunulmaz); ikinci evrede hız ve alınan hasar çarpanı,
        // sendelemezlik (çılgınlık)
        public string phaseShift;
        public float phase2Speed = 1, phase2Taken = 1;
        public bool phase2Unstoppable;
        public float bulk;   // vurulma kutusu genişlik çarpanı (0 = size)
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
                shielded = true, turnDelay = .6f, poise = true,
                attacks = new[]
                {
                    new EnemyAttack { name = "thrust", windup = .4f, active = .15f, recovery = .45f, reach = 2.4f, damage = 16 },
                    new EnemyAttack { name = "bash", windup = .25f, active = .12f, recovery = .4f, reach = 1.2f, damage = 8, pushBack = 1.5f, staminaDrain = 20, punish = true, cooldown = 1.5f },
                }
            },
            [EnemyRole.Halberdier] = new EnemyProfile
            {
                role = EnemyRole.Halberdier, title = "Teberli", sheet = "duelist", health = 110, guard = 90, speed = 1f, keepMin = 1.8f, keepMax = 2.6f, poise = true,
                attacks = new[]
                {
                    new EnemyAttack { name = "shove", windup = .2f, active = .12f, recovery = .8f, reach = 1f, damage = 6, push = 6, punish = true, first = true, cooldown = 2 },
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
                halfBoss = true, ranged = true, phaseAt = .5f, poise = true, parryStagger = 1.4f,
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
            [EnemyRole.Arslan] = new EnemyProfile
            {
                role = EnemyRole.Arslan, title = "Bayraktar Arslan", sheet = "captain", health = 260, guard = 110, speed = 1.4f, keepMin = 1.6f, keepMax = 2.6f, size = 1.15f,
                poise = true, phaseAt = .5f, phaseShift = "ignite",
                attacks = new[]
                {
                    new EnemyAttack { name = "thrust", windup = .45f, active = .15f, recovery = .5f, reach = 2.7f, damage = 18, punish = true },
                    new EnemyAttack { name = "sweep", windup = .6f, active = .25f, recovery = .8f, reach = 3f, damage = 22, red = true, cooldown = 2, phase = 1 },
                    new EnemyAttack { name = "burnsweep", windup = .6f, active = .25f, recovery = .8f, reach = 3f, damage = 24, red = true, cooldown = 2, phase = 2, patchWidth = 2.6f, patchLife = 1.5f },
                    new EnemyAttack { name = "plant", kind = AttackKind.Summon, windup = .8f, active = .05f, recovery = .6f, reach = 7, summon = EnemyRole.Nefer, summonCount = 2, cooldown = 999 },
                    new EnemyAttack { name = "ignite", kind = AttackKind.Rage, windup = 1f, active = .05f, recovery = .3f },
                }
            },
            [EnemyRole.KorDemir] = new EnemyProfile
            {
                role = EnemyRole.KorDemir, title = "Humbaracıbaşı Kör Demir", sheet = "captain", health = 280, guard = 100, speed = 1.2f, keepMin = 3, keepMax = 6.5f, size = 1.12f,
                poise = true, phaseAt = .5f, phaseShift = "ignite",
                attacks = new[]
                {
                    new EnemyAttack { name = "axe", windup = .45f, active = .15f, recovery = .6f, reach = 1.8f, damage = 20, heavy = true, punish = true },
                    new EnemyAttack { name = "hook", kind = AttackKind.Pull, windup = .4f, active = .15f, recovery = .6f, minRange = 2, reach = 4.5f, damage = 8, cooldown = 3 },
                    new EnemyAttack { name = "grenade", kind = AttackKind.Grenade, windup = .5f, active = .05f, recovery = .5f, minRange = 2, reach = 9, damage = 18, red = true, patchWidth = 1.6f, patchLife = 4, cooldown = 2.5f },
                    new EnemyAttack { name = "oil", kind = AttackKind.Grenade, windup = .7f, active = .05f, recovery = .6f, minRange = 3, reach = 9, damage = 10, red = true, patchWidth = 4.5f, patchLife = 5, cooldown = 6 },
                    new EnemyAttack { name = "ignite", kind = AttackKind.ArenaFire, windup = 1f, active = .05f, recovery = .3f },
                }
            },
            [EnemyRole.Kurtbas] = new EnemyProfile
            {
                role = EnemyRole.Kurtbas, title = "Deli Kurtbaş", sheet = "captain", health = 300, guard = 90, speed = 2.6f, keepMin = 1, keepMax = 1.8f, size = 1.12f,
                poise = true, phaseAt = .5f, phaseShift = "howl", phase2Taken = 1.25f, phase2Unstoppable = true,
                attacks = new[]
                {
                    new EnemyAttack { name = "storm", windup = .3f, active = 1.4f, hits = 4, hitGap = .42f, recovery = .5f, reach = 1.6f, damage = 10, punish = true },
                    new EnemyAttack { name = "leapover", kind = AttackKind.LeapOver, active = .7f, recovery = .1f, reach = 2, cooldown = 3 },
                    new EnemyAttack { name = "leapslam", kind = AttackKind.LeapSlam, windup = .4f, active = 1f, recovery = .6f, minRange = 3.5f, reach = 9, radius = 1.6f, damage = 24, red = true, cooldown = 4 },
                    new EnemyAttack { name = "howl", kind = AttackKind.Rage, windup = 1f, active = .05f, recovery = .3f },
                }
            },
            [EnemyRole.KaraMurad] = new EnemyProfile
            {
                role = EnemyRole.KaraMurad, title = "Yeniçeri Ağası Kara Murad", sheet = "captain", health = 380, guard = 140, speed = 1.5f, keepMin = 1, keepMax = 1.8f, size = 1.6f, bulk = 2f,
                poise = true, phaseAt = .5f, phaseShift = "throwpala", phase2Speed = 1.3f,
                attacks = new[]
                {
                    new EnemyAttack { name = "combo", windup = .45f, active = 1.25f, hitTimes = new[] { 0f, .45f, 1.1f }, recovery = .6f, reach = 2.4f, damage = 18, punish = true, phase = 1 },
                    new EnemyAttack { name = "charge", windup = .5f, active = .9f, recovery = .8f, minRange = 3, reach = 12, hitReach = 1.5f, dash = 8, damage = 26, red = true, push = 6, cooldown = 5, phase = 1 },
                    new EnemyAttack { name = "grab", windup = .35f, active = .15f, recovery = .7f, reach = 1.5f, damage = 30, red = true, heavy = true, grab = true, cooldown = 3, phase = 1 },
                    new EnemyAttack { name = "quake", kind = AttackKind.FlameWave, windup = .7f, active = .05f, recovery = .6f, minRange = 2.5f, reach = 10, damage = 20, red = true, cooldown = 4, phase = 1 },
                    new EnemyAttack { name = "throwpala", kind = AttackKind.Rage, windup = 1f, active = .05f, recovery = .2f },
                    new EnemyAttack { name = "fists", windup = .25f, active = 1f, hitTimes = new[] { 0f, .42f, .84f }, recovery = .4f, reach = 1.65f, damage = 14, punish = true, phase = 2 },
                    new EnemyAttack { name = "charge2", clip = "charge", windup = .4f, active = .9f, recovery = .3f, minRange = 3, reach = 12, hitReach = 1.5f, dash = 8, damage = 26, red = true, push = 6, cooldown = 4, then = "recharge", phase = 2 },
                    new EnemyAttack { name = "recharge", clip = "charge", windup = .35f, active = .9f, recovery = .8f, hitReach = 1.5f, dash = 8, damage = 26, red = true, push = 6, phase = 2 },
                    new EnemyAttack { name = "grab2", clip = "grab", windup = .3f, active = .15f, recovery = .6f, reach = 1.5f, damage = 30, red = true, heavy = true, grab = true, cooldown = 1.5f, phase = 2 },
                }
            },
            // ---- 2. kısım: goblinler (Docs/specs/2026-10-12-goblin-part2-design.md §3-§5)
            // Yer tutucu profiller: can / boy spec'ten, tek temel saldırı; asıl saldırı setleri sonraki görevlerde bunların yerine geçer.
            // Boy (Karamani = 1): sıradan goblin .68, Kaba 1.0 (gövde 2 kat), Kırçıl .9, Çatal 1.15, Kara Diş 1.1. Çizim: mevcut sayfalar.
            [EnemyRole.Bicakci] = new EnemyProfile
            {
                role = EnemyRole.Bicakci, title = "Bıçakçı", sheet = "duelist", goblin = true, health = 30, guard = 15, speed = 2.2f, keepMin = 1.2f, keepMax = 2f, size = .68f,
                parryStagger = 1.2f, heavyKnockdown = 1.5f,
                attacks = new[]
                {
                    // Sıçrayarak gelen 2'li saplama: seçim mesafesi 2, vuruş mesafesi 1.1; hazırlığın son .2 sn'sinde oyuncunun 1 birim yanına sıçrar.
                    // İkinci vuruş oyuncunun .4 sn'lik dokunulmazlığından sonra iner (.45). Vurunca hemen geri sıçrar
                    new EnemyAttack { name = "stab", windup = .3f, active = .6f, hitGap = .45f, hits = 2, recovery = .15f, reach = 2f, hitReach = 1.1f, leap = 1f, leapTime = .2f, damage = 8, punish = true, then = "hop" },
                    // Yuvarlanıp oyuncunun arkasına geçer (sağlam zemine, arkası yoksa önüne), orada bir kez saplar; 4 sn'de bir
                    new EnemyAttack { name = "flank", kind = AttackKind.LeapOver, active = .45f, recovery = .05f, reach = 2f, landGap = .9f, cooldown = 4, then = "flankstab" },
                    new EnemyAttack { name = "flankstab", windup = .25f, active = .12f, recovery = .15f, reach = 1.1f, damage = 8, then = "hop" },
                    // Geri sıçrama: yalnız saldırıların ardından zincirle çalışır (reach 0: kendiliğinden seçilmez)
                    new EnemyAttack { name = "hop", kind = AttackKind.Backstep, active = .25f, recovery = .1f, dash = -1.7f },
                }
            },
            [EnemyRole.ZehirliOkcu] = new EnemyProfile
            {
                role = EnemyRole.ZehirliOkcu, title = "Zehirli Okçu", sheet = "archer", goblin = true, health = 30, guard = 10, speed = 1.8f, keepMin = 6, keepMax = 10, size = .68f, ranged = true,
                heavyKnockdown = 1.5f,
                attacks = new[]
                {
                    new EnemyAttack { name = "backstep", kind = AttackKind.Backstep, active = .3f, recovery = .1f, reach = 2.5f, dash = -2, first = true, cooldown = 2 },
                    new EnemyAttack { name = "dagger", windup = .25f, active = .12f, recovery = .35f, reach = 1.2f, damage = 8 },
                    // Zehirli ok: 8 hasar, ardından 3 sn boyunca saniyede 3 zehir
                    new EnemyAttack { name = "arrow", clip = "shoot", kind = AttackKind.Arrow, windup = .45f, active = .05f, recovery = .4f, minRange = 2, reach = 11, damage = 8, poisonDamage = 3, poisonSeconds = 3, cooldown = 1.2f },
                    new EnemyAttack { name = "volley", kind = AttackKind.Volley, windup = .6f, active = .05f, recovery = .5f, minRange = 4, reach = 11, damage = 12, red = true, cooldown = 3 },
                }
            },
            [EnemyRole.Pusucu] = new EnemyProfile
            {
                role = EnemyRole.Pusucu, title = "Pusucu", sheet = "duelist", goblin = true, health = 40, guard = 20, speed = 1.8f, keepMin = .8f, keepMax = 1.2f, size = .68f, heavyKnockdown = 1.5f,
                attacks = new[]
                {
                    new EnemyAttack { name = "dagger", windup = .3f, active = .15f, recovery = .4f, reach = 1.3f, damage = 10, punish = true },
                }
            },
            [EnemyRole.GoblinKalkanli] = new EnemyProfile
            {
                role = EnemyRole.GoblinKalkanli, title = "Kalkanlı Mızrakçı", sheet = "shield", goblin = true, health = 70, guard = 60, speed = 1.2f, keepMin = 1.2f, keepMax = 2f, size = .68f,
                shielded = true, turnDelay = .6f, poise = true, pairGap = .9f,
                attacks = new[]
                {
                    new EnemyAttack { name = "thrust", windup = .4f, active = .15f, recovery = .45f, reach = 2.2f, damage = 12 },
                    new EnemyAttack { name = "bash", windup = .25f, active = .12f, recovery = .4f, reach = 1.2f, damage = 8, pushBack = 1.5f, staminaDrain = 20, punish = true, cooldown = 1.5f },
                }
            },
            [EnemyRole.Comlekci] = new EnemyProfile
            {
                role = EnemyRole.Comlekci, title = "Ateş Çömlekçisi", sheet = "duelist", goblin = true, health = 35, guard = 15, speed = 1.6f, keepMin = .9f, keepMax = 1.4f, size = .68f, heavyKnockdown = 1.5f,
                attacks = new[]
                {
                    new EnemyAttack { name = "pot", windup = .3f, active = .15f, recovery = .4f, reach = 1.3f, damage = 9 },
                }
            },
            [EnemyRole.Kaba] = new EnemyProfile
            {
                role = EnemyRole.Kaba, title = "Kaba", sheet = "captain", goblin = true, health = 130, guard = 100, speed = 1f, keepMin = 0, keepMax = 1.2f, size = 1f, bulk = 2f,
                poise = true, poiseAlways = true,
                attacks = new[]
                {
                    // Ağır iniş: bloklayanın dayanıklılığından 35 düşer
                    new EnemyAttack { name = "cleaver", windup = .6f, active = .15f, recovery = .8f, reach = 1.9f, damage = 24, heavy = true, blockDrain = 35 },
                    new EnemyAttack { name = "chainsweep", windup = .65f, active = .25f, recovery = .9f, reach = 3.2f, damage = 22, red = true, cooldown = 3 },
                    new EnemyAttack { name = "grab", windup = .4f, active = .15f, recovery = .8f, reach = 1.4f, damage = 26, red = true, heavy = true, grab = true, cooldown = 3 },
                }
            },
            [EnemyRole.Kircil] = new EnemyProfile
            {
                role = EnemyRole.Kircil, title = "Kan Şamanı Kırçıl", sheet = "captain", goblin = true, health = 200, guard = 50, speed = 1.2f, keepMin = 1.2f, keepMax = 2f, size = .9f,
                halfBoss = true,
                attacks = new[]
                {
                    new EnemyAttack { name = "staff", windup = .3f, active = .6f, hitGap = .45f, hits = 2, recovery = .4f, reach = 1.6f, damage = 12, punish = true },
                }
            },
            [EnemyRole.Catal] = new EnemyProfile
            {
                role = EnemyRole.Catal, title = "Domuz Binicisi Çatal", sheet = "captain", goblin = true, health = 160, guard = 80, speed = 1.6f, keepMin = 1.6f, keepMax = 2.4f, size = 1.15f,
                halfBoss = true,
                attacks = new[]
                {
                    new EnemyAttack { name = "spear", windup = .45f, active = .15f, recovery = .5f, reach = 2.6f, damage = 18, punish = true },
                }
            },
            [EnemyRole.KaraDis] = new EnemyProfile
            {
                role = EnemyRole.KaraDis, title = "Oymak Beyi Kara Diş", sheet = "captain", goblin = true, health = 420, guard = 80, speed = 3.2f, keepMin = 1f, keepMax = 1.7f, size = 1.1f,
                poise = true,
                attacks = new[]
                {
                    new EnemyAttack { name = "slash", clip = "light1", windup = .35f, active = .3f, recovery = .4f, reach = 1.8f, damage = 18, punish = true },
                }
            },
        };
    }
}
