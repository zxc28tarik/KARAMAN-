using System;
using UnityEngine;

namespace KaramaniReborn.Editor
{
    // Düşman tasarımının (Docs/specs/2026-10-12-enemy-roster-design.md) otomatik denetimi
    static class EnemyTests
    {
        public static void Verify(GameRoot root)
        {
            Profiles(root);
            Debug.Log("KARAMANI_ENEMY_PROFILES_PASS · six regular enemies with spec values and placeholder sheets");
            Attacks(root);
            Behaviour(root);
            Ranged(root);
            Sorcerer(root);
            Layouts(root);
        }

        static void Assert(bool ok, string message) => ArtValidation.Assert(ok, message);

        // Deneme alanı: 0. bölüm, oyuncu x=4'te sağa bakar; bölümün kendi düşmanları uzağa alınır,
        // denenen düşman oyuncunun gap birim önüne konur. intentTime=99: kendiliğinden saldırmaz.
        static GameWorld Arena(GameRoot root, EnemyRole role, float gap, out Actor e)
        {
            root.LoadZone(0, true); var w = root.world;
            foreach (var other in w.enemies) other.position.x = 58;
            w.player.position = new Vector2(4, w.zone.floor); w.player.face = 1; w.player.view.ResetContacts();
            e = w.Spawn(4 + gap, w.zone.floor, role); e.face = gap > 0 ? -1 : 1; e.intentTime = 99; e.view.ResetContacts();
            return w;
        }

        // 120 Hz simülasyon; input kare sırasını alır
        static void Run(GameWorld w, float seconds, Func<int, InputFrame> input = null)
        {
            int steps = Mathf.RoundToInt(seconds * 120);
            for (int i = 0; i < steps; i++) w.Simulate(1f / 120, input?.Invoke(i) ?? new InputFrame(), true);
        }

        static void Ranged(GameRoot root)
        {
            // Tüfek: kırmızı mermi bloğu deler (35), ardından 2 sn doldurma
            var w = Arena(root, EnemyRole.Musketeer, 7, out var e); var pl = w.player;
            w.StartAttack(e, "aim");
            Run(w, 1.7f, i => new InputFrame { block = true });
            Assert(pl.health == pl.maxHealth - 35, "Musket shot should pierce the block for 35, health " + pl.health);
            Assert(e.state == ActionState.Shoot && e.attack?.name == "reload", "Musketeer did not reload after firing");

            // Mermiye doğru zamanında yuvarlanma kurtarır
            w = Arena(root, EnemyRole.Musketeer, 7, out e); pl = w.player;
            w.StartAttack(e, "aim");
            int press = Mathf.RoundToInt(1.36f * 120);
            Run(w, 1.8f, i => new InputFrame { move = i >= press && i < press + 2 ? 1 : 0, dodge = i == press });
            Assert(pl.health == pl.maxHealth, "Dodging into the bullet should avoid it, health " + pl.health);

            // Nişan sırasında vurulan tüfekli doldurmaya geçmez
            w = Arena(root, EnemyRole.Musketeer, 7, out e);
            w.StartAttack(e, "aim"); Run(w, .3f);
            w.Damage(e, 10, 1, false, false, w.player);
            Run(w, .6f);
            Assert(e.attack == null || e.attack.name != "reload", "Interrupted aim must not chain into reload");

            // Ok yağmuru: işaretli yerde duran vurulur
            w = Arena(root, EnemyRole.Archer, 7, out e); pl = w.player;
            w.StartAttack(e, "volley");
            Run(w, 2f);
            Assert(pl.health == pl.maxHealth - 18, "Volley should hit a player standing on the mark, health " + pl.health);

            // Ok yağmuru: işaretten çıkan kurtulur
            w = Arena(root, EnemyRole.Archer, 7, out e); pl = w.player;
            var volley = EnemyProfile.For(EnemyRole.Archer).Find("volley");
            int leave = Mathf.RoundToInt((volley.windup + .1f) * 120);
            Run(w, 2f, i => new InputFrame { move = i >= leave && i < leave + 60 ? -1 : 0 });
            Assert(pl.health == pl.maxHealth, "Leaving the volley mark should avoid the arrows, health " + pl.health);

            // Okçu yakındaki oyuncudan geri sıçrar
            w = Arena(root, EnemyRole.Archer, 1f, out e); e.intentTime = 0;
            Run(w, .6f);
            Assert(e.position.x - w.player.position.x > 2.3f, "Archer did not backstep, gap " + (e.position.x - w.player.position.x));

            // Platformdaki okçu geri çekilirken kenardan düşmez (platform 23.5 - 26.6, y = -0.75)
            w = Arena(root, EnemyRole.Archer, 1f, out e); e.intentTime = 0;
            e.position = new Vector2(26.2f, -.75f); w.player.position = new Vector2(24.8f, -.75f);
            Run(w, 2f);
            Assert(e.attackReady.ContainsKey("backstep"), "Archer never tried to backstep at the platform edge");
            Assert(e.grounded && Mathf.Abs(e.position.y + .75f) < .01f, "Archer fell off the platform while retreating");
            Debug.Log("KARAMANI_ENEMY_RANGED_PASS · musket pierce + reload + dodge, interrupted chain, volley mark, backstep, platform edge");
        }

        static void Layouts(GameRoot root)
        {
            // Bölge dizilişi (spec §5): insanlar önce, büyücü Kurt Geçidi'nde ilk kez; her bölümde bir boss
            var expect = new[]
            {
                new[] { EnemyRole.Nefer, EnemyRole.Spearman, EnemyRole.Archer, EnemyRole.Nefer, EnemyRole.Nefer, EnemyRole.Captain },
                new[] { EnemyRole.Nefer, EnemyRole.Musketeer, EnemyRole.Halberdier, EnemyRole.Musketeer, EnemyRole.Nefer, EnemyRole.Captain },
                new[] { EnemyRole.Spearman, EnemyRole.Musketeer, EnemyRole.Archer, EnemyRole.Halberdier, EnemyRole.Archer, EnemyRole.Sorcerer, EnemyRole.Captain },
                new[] { EnemyRole.Nefer, EnemyRole.Spearman, EnemyRole.Musketeer, EnemyRole.Halberdier, EnemyRole.Archer, EnemyRole.Sorcerer, EnemyRole.Captain },
            };
            for (int zone = 0; zone < 4; zone++)
            {
                root.LoadZone(zone, true); var w = root.world;
                Assert(w.enemies.Count == expect[zone].Length, "Zone " + zone + " enemy count " + w.enemies.Count);
                for (int i = 0; i < expect[zone].Length; i++) Assert(w.enemies[i].role == expect[zone][i], "Zone " + zone + " enemy " + i + " is " + w.enemies[i].role);
                Assert(w.enemies.FindAll(a => a.boss).Count == 1 && w.enemies[^1].boss, "Zone " + zone + " must end with one boss");
                Assert(!w.enemies.Exists(a => EnemyProfile.For(a.role).halfBoss) || zone >= 2, "Sorcerer before Kurt Geçidi");
            }
            Debug.Log("KARAMANI_ENEMY_LAYOUT_PASS · four zones per spec §5");
        }

        static void Sorcerer(GameRoot root)
        {
            var flame = EnemyProfile.For(EnemyRole.Sorcerer).Find("flame");
            // Alev dalgası: yerde duran yanar
            var w = Arena(root, EnemyRole.Sorcerer, 6, out var e); var pl = w.player;
            w.StartAttack(e, "flame"); Run(w, 2.4f);
            Assert(pl.health == pl.maxHealth - 22, "Flame wave should hit a grounded player, health " + pl.health);

            // Alev dalgası: zamanında zıplayan kurtulur
            w = Arena(root, EnemyRole.Sorcerer, 6, out e); pl = w.player;
            w.StartAttack(e, "flame");
            float arrive = flame.windup + (6 - .6f - .53f) / 6;   // dalganın ön kenarı oyuncu kutusuna varır
            int jump = Mathf.RoundToInt((arrive - .25f) * 120);
            Run(w, 2.4f, i => new InputFrame { jump = i == jump, jumpHeld = i >= jump && i < jump + 40 });
            Assert(pl.health == pl.maxHealth, "Jumping should clear the flame wave, health " + pl.health);

            // Lanet küreleri: üç küre çıkar, kılıçla söndürülür
            w = Arena(root, EnemyRole.Sorcerer, 6, out e);
            var orbs = EnemyProfile.For(EnemyRole.Sorcerer).Find("orbs");
            w.StartAttack(e, "orbs"); Run(w, orbs.windup + .05f);
            Assert(w.projectiles.FindAll(s => s.kind == GameWorld.ShotKind.Orb).Count == 3, "Sorcerer should release three orbs");
            Run(w, .02f, i => new InputFrame { light = i == 0 });
            Run(w, .12f);   // hafif vuruşun etkin penceresine
            var orb = w.projectiles.Find(s => s.kind == GameWorld.ShotKind.Orb);
            orb.position = Vector2.Lerp(w.player.view.StrikeRoot, w.player.view.BladeTip, .7f); orb.velocity = Vector2.zero;
            Run(w, 2f / 120);
            Assert(!w.projectiles.Contains(orb), "A sword swing should snuff out a curse orb");

            // Işınlanma: yaklaşan oyuncudan kaçar ve sağlam zemine iner
            w = Arena(root, EnemyRole.Sorcerer, 1.2f, out e); e.intentTime = 0; float before = e.position.x;
            Run(w, .9f);
            Assert(Mathf.Abs(e.position.x - before) > 1.5f && e.grounded, "Sorcerer did not teleport away: " + before + " -> " + e.position.x);

            // Işınlanma: oyuncunun arkası çukursa başka yere iner (çukur 13.5 - 15.7)
            w = Arena(root, EnemyRole.Sorcerer, 1.2f, out e); e.intentTime = 0;
            w.player.position = new Vector2(16.4f, w.zone.floor); w.player.face = 1; e.position.x = 17.6f;
            Run(w, .9f);
            Assert(e.grounded && Mathf.Abs(e.position.y - w.zone.floor) < .01f && !(e.position.x > 13.5f && e.position.x < 15.7f), "Sorcerer teleported into a pit: x " + e.position.x);
            Assert(Mathf.Abs(e.position.x - 17.6f) > 1f, "Sorcerer did not teleport at all: x " + e.position.x);

            // İkinci evre: canı yarının altına inince alev dalgaları ikişer gelir
            w = Arena(root, EnemyRole.Sorcerer, 6, out e);
            e.health = e.maxHealth * .45f; Run(w, .05f);
            Assert(e.phase == 2, "Sorcerer did not enter phase 2");
            w.StartAttack(e, "flame"); Run(w, flame.windup + .6f);
            Assert(w.projectiles.FindAll(s => s.kind == GameWorld.ShotKind.Wave).Count == 2, "Phase 2 flame should release two waves");

            // Yarı-boss can çubuğu
            Assert(w.BarTarget == e, "Half-boss bar should target the sorcerer");
            Debug.Log("KARAMANI_ENEMY_SORCERER_PASS · flame wave + jump, orbs + snuff, teleport + pit guard, phase 2, half-boss bar");
        }

        static void Profiles(GameRoot root)
        {
            var expected = new (EnemyRole role, float health, float guard, string sheet)[]
            {
                (EnemyRole.Nefer, 70, 35, "duelist"), (EnemyRole.Spearman, 95, 75, "shield"), (EnemyRole.Halberdier, 110, 90, "duelist"),
                (EnemyRole.Musketeer, 55, 30, "archer"), (EnemyRole.Archer, 50, 25, "archer"), (EnemyRole.Sorcerer, 220, 60, "captain")
            };
            foreach (var x in expected)
            {
                Arena(root, x.role, 3, out var e);
                Assert(e.maxHealth == x.health && e.maxGuard == x.guard, x.role + " health/guard " + e.maxHealth + "/" + e.maxGuard);
                Assert(e.view.variant == x.sheet, x.role + " placeholder sheet " + e.view.variant);
                foreach (var k in EnemyProfile.For(x.role).attacks)
                    Assert(!string.IsNullOrEmpty(k.clip) && k.Duration > 0, x.role + "." + k.name + " has no clip or duration");
            }
            Assert(EnemyProfile.For(EnemyRole.Spearman).shielded && EnemyProfile.For(EnemyRole.Sorcerer).halfBoss, "Shield / half-boss flags");
            var aim = EnemyProfile.For(EnemyRole.Musketeer).Find("aim");
            Assert(aim.red && aim.then == "reload", "Musket shot must be red and chain into reload");
        }

        static void Attacks(GameRoot root)
        {
            // Kombo: iki vuruş da yalnız etkin pencerede iner (2 x 14)
            var w = Arena(root, EnemyRole.Nefer, 1.2f, out var e); var pl = w.player;
            var combo = EnemyProfile.For(EnemyRole.Nefer).Find("combo");
            Assert(w.StartAttack(e, "combo"), "StartAttack(combo) failed");
            Run(w, combo.windup - .05f);
            Assert(pl.health == pl.maxHealth, "Combo hit before its wind-up ended");
            Run(w, combo.Duration);
            Assert(pl.health == pl.maxHealth - 28, "Nefer combo should land 2 x 14, health " + pl.health);

            // Kırmızı savuruş: blok işe yaramaz
            w = Arena(root, EnemyRole.Halberdier, 2.2f, out e); pl = w.player;
            w.StartAttack(e, "sweep");
            Run(w, 1.7f, i => new InputFrame { block = true });
            Assert(pl.health == pl.maxHealth - 22, "Red sweep must ignore block, health " + pl.health);

            // Kırmızı savuruş: zamanında yuvarlanma kurtarır
            w = Arena(root, EnemyRole.Halberdier, 2.2f, out e); pl = w.player;
            var sweep = EnemyProfile.For(EnemyRole.Halberdier).Find("sweep");
            w.StartAttack(e, "sweep");
            int press = Mathf.RoundToInt((sweep.windup - .03f) * 120);
            Run(w, 1.7f, i => new InputFrame { move = i >= press && i < press + 2 ? -1 : 0, dodge = i == press });
            Assert(pl.health == pl.maxHealth, "Dodge i-frames should avoid the red sweep, health " + pl.health);

            // Beyaz vuruş: zamanında blok savuşturur, saldıran sendeler, infaz açılır
            w = Arena(root, EnemyRole.Nefer, 1.2f, out e); pl = w.player;
            w.StartAttack(e, "combo");
            press = Mathf.RoundToInt((combo.windup - .06f) * 120);
            Run(w, combo.windup + .1f, i => new InputFrame { block = i >= press });
            Assert(pl.health == pl.maxHealth && e.state == ActionState.Hurt && pl.riposte > 0, "Parry of a white attack should stagger and open a riposte");
            Debug.Log("KARAMANI_ENEMY_ATTACKS_PASS · active windows, red ignores block, dodge avoids red, white parry staggers");
        }
    

        static void Behaviour(GameRoot root)
        {
            // Mesafe: tüfekli yakındaki oyuncudan uzaklaşır, nefer uzaktaki oyuncuya yaklaşır
            var w = Arena(root, EnemyRole.Musketeer, 2.5f, out var e);
            Run(w, 2f);   // geri çekilme hızı .8×1.2 = .96 b/sn: 1.5 sn'de boşluk 3.9'da kalır, 2 sn gerekir
            Assert(e.position.x - w.player.position.x > 4f, "Musketeer did not retreat: gap " + (e.position.x - w.player.position.x));
            w = Arena(root, EnemyRole.Nefer, 6, out e);
            Run(w, 4);
            Assert(e.position.x - w.player.position.x < 1.6f, "Nefer did not close in: gap " + (e.position.x - w.player.position.x));

            // Sıra: aynı anda en fazla iki düşman saldırır
            w = Arena(root, EnemyRole.Nefer, 1.2f, out e); e.intentTime = 0;
            foreach (float gap in new[] { -1.2f, 1.4f, -1.4f }) w.Spawn(4 + gap, w.zone.floor, EnemyRole.Nefer).intentTime = 0;
            int most = 0;
            for (int i = 0; i < 360; i++) { w.Simulate(1f / 120, new InputFrame { block = true }, true); most = Mathf.Max(most, w.Attackers); }
            Assert(most == 2 && most <= GameWorld.MaxAttackers, "Simultaneous attackers " + most + " (spec: exactly 2, cap " + GameWorld.MaxAttackers + ")");   // sabit 2: sınırın kendisi değişirse de yakalanır

            // Açık cezalandırma: iksir içen oyuncuya bekleyen nefer hemen saldırır
            w = Arena(root, EnemyRole.Nefer, 1.3f, out e); e.intentTime = 0; e.cooldown = 5; w.player.health = 50;
            Run(w, .3f, i => new InputFrame { heal = i == 0 });
            Assert(e.state == ActionState.Light, "Nefer did not punish the potion");

            // Açık cezalandırma: savuşturma penceresini ıskalayıp blokta kalan oyuncuya da saldırır (kontrol: blok yoksa saldırmaz)
            w = Arena(root, EnemyRole.Nefer, 1.3f, out e); e.intentTime = 0; e.cooldown = 5;
            Run(w, .3f);
            Assert(e.state != ActionState.Light, "Nefer attacked with no opening");
            w = Arena(root, EnemyRole.Nefer, 1.3f, out e); e.intentTime = 0; e.cooldown = 5;
            Run(w, .3f, i => new InputFrame { block = true });
            Assert(e.state == ActionState.Light, "Nefer did not punish a missed parry");

            // Yükseklik: platformdaki oyuncuya yerdeki nefer yakın saldırı başlatmaz
            w = Arena(root, EnemyRole.Nefer, 1.0f, out e); e.intentTime = 0;
            w.player.position = new Vector2(8.5f, -.9f); e.position.x = 8.5f + 1f;
            Run(w, 1f);
            Assert(e.state != ActionState.Light, "Nefer attacked a player on a platform above");

            // Ölü oyuncu: yeni saldırı başlamaz
            w = Arena(root, EnemyRole.Nefer, 1.2f, out e); e.intentTime = 0;
            w.Damage(w.player, 1000, 1, true, true);
            Run(w, 1f);
            Assert(w.Attackers == 0, "Enemy attacked a dead player");

            // Kalkan: önden hafif vuruş can götürmez, ağır vuruş götürür, arkadan vuruş dengeyi iki kat düşürür
            w = Arena(root, EnemyRole.Spearman, 1.3f, out e);
            w.Damage(e, 20, 1, false, false, w.player);
            Assert(e.health == e.maxHealth && e.guard < e.maxGuard, "Shield did not stop a frontal light hit");
            e.Enter(ActionState.Idle); e.invincible = 0; float hp = e.health;
            w.Damage(e, 44, 1, true, false, w.player);
            Assert(e.health < hp, "Heavy hit should pass the shield");
            w = Arena(root, EnemyRole.Spearman, 1.3f, out e); e.face = 1;   // sırtı oyuncuya dönük
            w.Damage(e, 20, 1, false, false, w.player);
            Assert(e.health == e.maxHealth - 20 && Mathf.Approximately(e.guard, e.maxGuard - 40), "Back hit should hurt and drain guard twice: guard " + e.guard);

            // Denge bedeli: kalkanlıya ağır vuruş ya da arkadan vuruş x2 (üst üste binmez); kalkansıza ağır x1.5
            w = Arena(root, EnemyRole.Spearman, 1.3f, out e);
            w.Damage(e, 30, 1, true, false, w.player);
            Assert(e.health == e.maxHealth - 30 && Mathf.Approximately(e.guard, e.maxGuard - 60), "Frontal heavy should drain guard x2: guard " + e.guard);
            w = Arena(root, EnemyRole.Spearman, 1.3f, out e); e.face = 1;
            w.Damage(e, 30, 1, true, false, w.player);
            Assert(Mathf.Approximately(e.guard, e.maxGuard - 60), "Heavy from behind must not stack to x3: guard " + e.guard);
            w = Arena(root, EnemyRole.Nefer, 1.3f, out e);
            w.Damage(e, 20, 1, true, false, w.player);
            Assert(Mathf.Approximately(e.guard, e.maxGuard - 30), "Unshielded heavy should drain guard x1.5: guard " + e.guard);

            // Denge kırılınca uzun sendeleme ve infaz fırsatı
            w = Arena(root, EnemyRole.Nefer, 1.2f, out e); e.guard = 10;
            w.Damage(e, 20, 1, false, false, w.player);
            Assert(e.state == ActionState.Hurt && e.duration > .8f && w.player.riposte > 0, "Guard break should stagger for 0.9 s and open a riposte");

            // Savuşturma > infaz: infaz vuruşu dengeyi düşürüp yeniden sendeletmez, yeni infaz fırsatı açmaz
            // (infaz ağır vuruş x2.6 olduğundan sıradan neferi öldürür; sağ kalıp denge sınanabilsin diye komutan)
            w = Arena(root, EnemyRole.Captain, 1.2f, out e); var pl = w.player;
            var nc = EnemyProfile.For(EnemyRole.Captain).Find("slash");
            w.StartAttack(e, "slash");
            int press = Mathf.RoundToInt((nc.windup - .06f) * 120);
            Run(w, nc.windup + .1f, i => new InputFrame { block = i >= press });
            Assert(e.state == ActionState.Hurt && pl.riposte > 0, "Setup: parry should stagger the Captain");
            e.position.x = pl.position.x + 1f; e.knockback = 0;   // savuşturma geri iter; infaz menzilinde dursun
            float before = e.health; bool hit = false;
            for (int i = 0; i < 120 && !hit; i++)
            {
                w.Simulate(1f / 120, new InputFrame { light = i == 0 }, true);
                hit = e.health < before;
            }
            Assert(hit && !e.Dead, "Execution never connected or killed: player " + pl.state + ", enemy " + e.state + " hp " + e.health);
            Assert(pl.riposte <= 0 && e.duration < .8f, "Execution hit re-staggered the target: riposte " + pl.riposte + ", hurt duration " + e.duration);

            // Dönüş gecikmesi: oyuncu arkasına geçince kalkanlı geç döner
            w = Arena(root, EnemyRole.Spearman, 1.3f, out e); e.face = -1;
            w.player.position.x = e.position.x + 1.5f;
            Run(w, .3f); Assert(e.face == -1, "Spearman turned too fast");
            Run(w, .5f); Assert(e.face == 1, "Spearman never turned");
            // Yarım kalan dönüş (kilitlenme) bir sonraki dönüşü kısaltmaz
            w = Arena(root, EnemyRole.Spearman, 1.3f, out e); e.face = -1;
            w.player.position.x = e.position.x + 1.5f;
            Run(w, .4f); e.Enter(ActionState.Hurt, .3f); Run(w, .1f);
            Assert(e.turnTimer == 0, "Interrupted turn kept its timer: " + e.turnTimer);
            Debug.Log("KARAMANI_ENEMY_BEHAVIOUR_PASS · range, two attackers, punish, height, dead player, shield, guard break, turn delay");
        }
    }
}
