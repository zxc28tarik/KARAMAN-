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
            AllMelee(root);
            Behaviour(root);
            Ranged(root);
            Sorcerer(root);
            Layouts(root);
            Bosses(root);
            Arslan(root);
            KorDemir(root);
            Kurtbas(root);
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

            // Tüfekli boş kalır: doldurması bölünse de doldurma süresi dolmadan yeniden nişan almaz
            w = Arena(root, EnemyRole.Musketeer, 7, out e); pl = w.player;
            var aim = EnemyProfile.For(EnemyRole.Musketeer).Find("aim");
            w.StartAttack(e, "aim"); Run(w, aim.Duration + .15f);
            Assert(e.state == ActionState.Shoot && e.attack?.name == "reload", "Setup: musketeer should be reloading");
            w.Damage(e, 10, 1, false, false, pl);   // doldurma bölünür
            e.intentTime = 0;
            bool aimed = false;
            Run(w, 1.2f, i => { aimed |= e.state == ActionState.Light && e.attack?.name == "aim"; return new InputFrame(); });
            Assert(!aimed, "Musketeer aimed again before its reload time elapsed");
            Run(w, 1.5f, i => { aimed |= e.state == ActionState.Light && e.attack?.name == "aim"; return new InputFrame(); });
            Assert(aimed, "Musketeer never aimed again after the reload time");

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
            // Uzakçı yerini korur: platformdaki tüfekli uzaktaki oyuncuya yürürken kenardan inmez, boşluğa atlamaz
            // (platform 37.4 - 40.7, y = -0.85; oyuncu 11 b ötede, böylece kenarda bile keepMax 9'un dışında kalır)
            w = Arena(root, EnemyRole.Musketeer, 7, out e);
            e.position = new Vector2(39, -.85f); e.view.ResetContacts();
            w.player.position = new Vector2(28, w.zone.floor); w.player.view.ResetContacts();
            Run(w, 3);
            Assert(e.grounded && Mathf.Abs(e.position.y + .85f) < .01f && e.position.x > 37.4f, "Musketeer left its platform: " + e.position);
            Debug.Log("KARAMANI_ENEMY_RANGED_PASS · musket pierce + reload + dodge, interrupted chain, stays empty after an interrupted reload, volley mark, backstep, platform edge, keeps its perch");
        }

        static void Layouts(GameRoot root)
        {
            // Bölge dizilişi (spec §5): insanlar önce, büyücü Kurt Geçidi'nde ilk kez; her bölümde bir boss
            var expect = new[]
            {
                new[] { EnemyRole.Nefer, EnemyRole.Spearman, EnemyRole.Archer, EnemyRole.Nefer, EnemyRole.Nefer, EnemyRole.Arslan },
                new[] { EnemyRole.Nefer, EnemyRole.Musketeer, EnemyRole.Halberdier, EnemyRole.Musketeer, EnemyRole.Nefer, EnemyRole.KorDemir },
                new[] { EnemyRole.Spearman, EnemyRole.Musketeer, EnemyRole.Archer, EnemyRole.Halberdier, EnemyRole.Archer, EnemyRole.Sorcerer, EnemyRole.Kurtbas },
                new[] { EnemyRole.Nefer, EnemyRole.Spearman, EnemyRole.Musketeer, EnemyRole.Halberdier, EnemyRole.Archer, EnemyRole.Sorcerer, EnemyRole.KaraMurad },
            };
            for (int zone = 0; zone < 4; zone++)
            {
                root.LoadZone(zone, true); var w = root.world;
                Assert(w.enemies.Count == expect[zone].Length, "Zone " + zone + " enemy count " + w.enemies.Count);
                for (int i = 0; i < expect[zone].Length; i++) Assert(w.enemies[i].role == expect[zone][i], "Zone " + zone + " enemy " + i + " is " + w.enemies[i].role);
                Assert(w.enemies.FindAll(a => a.boss).Count == 1 && w.enemies[^1].boss, "Zone " + zone + " must end with one boss");
                Assert(root.hud.EnemyMarkers == w.enemies.Count, "Zone " + zone + " minimap shows " + root.hud.EnemyMarkers + " of " + w.enemies.Count + " enemies");
                Assert(!w.enemies.Exists(a => EnemyProfile.For(a.role).halfBoss) || zone >= 2, "Sorcerer before Kurt Geçidi");
                // Karanlık Kale'de büyücü daha güçlü (spec §5): can x1.3, denge x1.2; Kurt Geçidi'nde profil değerleri
                var sorcerer = w.enemies.Find(a => a.role == EnemyRole.Sorcerer); var sp = EnemyProfile.For(EnemyRole.Sorcerer);
                if (sorcerer != null)
                    Assert(Mathf.Approximately(sorcerer.maxHealth, sp.health * (zone == 3 ? 1.3f : 1)) && Mathf.Approximately(sorcerer.maxGuard, sp.guard * (zone == 3 ? 1.2f : 1)),
                        "Zone " + zone + " sorcerer health/guard " + sorcerer.maxHealth + "/" + sorcerer.maxGuard);
            }
            Debug.Log("KARAMANI_ENEMY_LAYOUT_PASS · four zones per spec §5, stronger sorcerer in Karanlık Kale");
        }

        static void Bosses(GameRoot root)
        {
            // Her bölümün sonunda kendi ağası; değerler profilden, boy %12-25 büyük, ad çubukta
            EnemyRole[] expect = { EnemyRole.Arslan, EnemyRole.KorDemir, EnemyRole.Kurtbas, EnemyRole.KaraMurad };
            string[] names = { "Bayraktar Arslan", "Humbaracıbaşı Kör Demir", "Deli Kurtbaş", "Yeniçeri Ağası Kara Murad" };
            for (int zone = 0; zone < 4; zone++)
            {
                root.LoadZone(zone, true); var w = root.world; var boss = w.enemies[^1]; var p = EnemyProfile.For(boss.role);
                Assert(boss.boss && boss.role == expect[zone], "Zone " + zone + " boss is " + boss.role);
                // Boy kullanıcı kararı: Karamani'ye göre %12-25 büyük; Kara Murad %60 uzun ve iki kat iri (vurulma kutusu bulk)
                float[] sizes = { 1.15f, 1.12f, 1.12f, 1.6f };
                Assert(boss.maxHealth == p.health && boss.maxGuard == p.guard && Mathf.Approximately(boss.size, sizes[zone]) && p.size == sizes[zone], boss.role + " values from profile, size " + boss.size);
                Assert(zone == 3 ? p.bulk == 2f : p.bulk == 0, boss.role + " bulk " + p.bulk);
                Assert(w.zone.bossName == names[zone], "Zone " + zone + " boss name " + w.zone.bossName);
                Assert(p.phaseAt == .5f && p.poise && p.phaseShift != null && p.Find(p.phaseShift) != null, boss.role + " needs a phase change at 50% with a transition attack");
            }

            // Evre kapısı: yalnız ikinci evre saldırısı birinci evrede başlamaz; geçişte bekleme süreleri sıfırlanır ve geçiş saldırısı oynar
            var w2 = Arena(root, EnemyRole.KaraMurad, 1.4f, out var e);
            Assert(!w2.StartAttack(e, "fists") && w2.StartAttack(e, "combo"), "Phase 1 must allow only phase-1 attacks");
            e.Enter(ActionState.Idle); e.attackReady["combo"] = 999;
            e.health = e.maxHealth * .45f; Run(w2, .05f);
            Assert(e.phase == 2 && e.attack != null && e.attack.name == EnemyProfile.For(e.role).phaseShift && e.invincible > 0, "Phase change must start the transition attack, invulnerable");
            Assert(!e.attackReady.ContainsKey("combo"), "Phase change must reset cooldowns");
            Assert(w2.StartAttack(e, "fists") && !w2.StartAttack(e, "combo"), "Phase 2 must allow only phase-2 attacks");

            // Seçim kapısı: ikinci evredeki Kara Murad kendiliğinden yalnız ikinci evre saldırılarını seçer
            w2 = Arena(root, EnemyRole.KaraMurad, 1.2f, out e); w2.player.maxHealth = w2.player.health = 9999;
            e.health = e.maxHealth * .45f; Run(w2, .05f);
            Assert(e.phase == 2, "Setup: Kara Murad should be in phase 2");
            Run(w2, EnemyProfile.For(e.role).Find(EnemyProfile.For(e.role).phaseShift).Duration + .1f);
            var started = new System.Collections.Generic.HashSet<EnemyAttack>();
            e.intentTime = 0; e.cooldown = 0;
            Run(w2, 3f, i => { if (e.attack != null && e.stateTime < 1f / 60) started.Add(e.attack); return new InputFrame(); });
            Assert(started.Count > 0, "Phase-2 Kara Murad never started an attack on its own");
            foreach (var k in started) Assert(k.phase != 1, "Phase-2 Kara Murad picked the phase-1 attack " + k.name);

            // Düzensiz ritim: Kara Murad'ın 3'lü palası yavaş-hızlı-yavaş (vuruş anları 0, .45, 1.1)
            w2 = Arena(root, EnemyRole.KaraMurad, 1.4f, out e); var pl = w2.player;
            var combo = EnemyProfile.For(EnemyRole.KaraMurad).Find("combo");
            w2.StartAttack(e, "combo");
            Run(w2, combo.windup + .05f); Assert(pl.health == pl.maxHealth - combo.damage, "First pala hit at 0 s, health " + pl.health);
            Run(w2, .35f); Assert(pl.health == pl.maxHealth - combo.damage, "No second hit before .45 s, health " + pl.health);
            Run(w2, .15f); Assert(pl.health == pl.maxHealth - 2 * combo.damage, "Second hit at .45 s, health " + pl.health);
            Run(w2, .45f); Assert(pl.health == pl.maxHealth - 2 * combo.damage, "No third hit before 1.1 s, health " + pl.health);
            Run(w2, .25f); Assert(pl.health == pl.maxHealth - 3 * combo.damage, "Third hit at 1.1 s, health " + pl.health);
            Debug.Log("KARAMANI_BOSS_FOUNDATION_PASS · four aghas per zone, profile values, phase gating + transition, uneven rhythm");
        }

        static void Arslan(GameRoot root)
        {
            // Direk dürtüşü beyaz: zamanında blok savuşturur
            var w = Arena(root, EnemyRole.Arslan, 2.4f, out var e); var pl = w.player;
            var thrust = EnemyProfile.For(EnemyRole.Arslan).Find("thrust");
            w.StartAttack(e, "thrust"); int press = Mathf.RoundToInt((thrust.windup - .06f) * 120);
            Run(w, thrust.windup + .1f, i => new InputFrame { block = i >= press });
            Assert(pl.health == pl.maxHealth && e.state == ActionState.Hurt, "Arslan's thrust must be parryable");

            // Geniş savuruş kırmızı: blok işe yaramaz
            w = Arena(root, EnemyRole.Arslan, 2.6f, out e); pl = w.player;
            w.StartAttack(e, "sweep"); Run(w, 1.8f, i => new InputFrame { block = true });
            Assert(pl.health == pl.maxHealth - 22, "Red sweep must ignore block, health " + pl.health);

            // Sancak dikme: iki nefer, yalnız sağlam zeminde; bu evrede bir daha olmaz, ikinci evrede yeniden açılır
            w = Arena(root, EnemyRole.Arslan, 3, out e); int before = w.enemies.Count;
            w.StartAttack(e, "plant"); Run(w, .9f);
            var summoned = w.enemies.GetRange(before, w.enemies.Count - before);
            Assert(summoned.Count == 2 && summoned.TrueForAll(n => n.role == EnemyRole.Nefer && n.grounded), "Plant must summon two grounded Nefers, got " + summoned.Count);
            Assert(e.attackReady["plant"] > w.clock + 100, "Plant must be once per phase");
            // Çağrılan neferler haritada da görünür (arayüz düşman sayısı değişince işaretleri tamamlar)
            root.hud.Tick(0);
            Assert(root.hud.EnemyMarkers == w.enemies.Count, "Minimap shows " + root.hud.EnemyMarkers + " of " + w.enemies.Count + " enemies after the summon");
            // Çukur kenarında (13.5-15.7 çukuru): çukurun üstüne doğmaz
            w = Arena(root, EnemyRole.Arslan, 3, out e); e.position.x = 15.9f; before = w.enemies.Count;
            w.StartAttack(e, "plant"); Run(w, .9f);
            foreach (var n in w.enemies.GetRange(before, w.enemies.Count - before)) Assert(!(n.position.x > 13.5f && n.position.x < 15.7f), "Nefer summoned over the pit at " + n.position.x);

            // İkinci evre: sancak yanar; yanan savuruş ardında kısa ateş bırakır, birinci evre savuruşu bırakmaz
            w = Arena(root, EnemyRole.Arslan, 2.6f, out e);
            w.StartAttack(e, "sweep"); Run(w, .9f);
            Assert(!w.projectiles.Exists(s => s.kind == GameWorld.ShotKind.Fire), "Phase-1 sweep must not leave fire");
            e.Enter(ActionState.Idle); e.health = e.maxHealth * .45f; Run(w, 1.5f);
            Assert(e.phase == 2 && !e.attackReady.ContainsKey("plant"), "Phase 2 must reopen the standard summon");
            w.StartAttack(e, "burnsweep"); Run(w, .9f);
            Assert(w.projectiles.Exists(s => s.kind == GameWorld.ShotKind.Fire), "Burning sweep must leave a fire trail");

            // Boss ölünce çağrılan neferler yaşasa da kapı açılır
            w = Arena(root, EnemyRole.Arslan, 3, out e);
            var gateBoss = w.enemies.Find(a => a.boss); w.StartAttack(e, "plant"); Run(w, .9f);
            w.Damage(gateBoss, 5000, 1, true, true); w.Simulate(1f / 120, new InputFrame(), true);
            Assert(w.cleared, "Gate must open when the boss dies even with summons alive");
            Debug.Log("KARAMANI_BOSS_ARSLAN_PASS · parryable thrust, red sweep, summon on ground once per phase, burning standard trail, gate");
        }

        static void KorDemir(GameRoot root)
        {
            // Humbara: kavisle düşer, patlama 18 (kırmızı), yerinde 4 sn yanan ateş bırakır
            var w = Arena(root, EnemyRole.KorDemir, 6, out var e); var pl = w.player;
            var g = EnemyProfile.For(EnemyRole.KorDemir).Find("grenade");
            w.StartAttack(e, "grenade"); Run(w, g.windup + .9f + .1f);
            Assert(pl.health == pl.maxHealth - 18, "Grenade blast should deal 18, health " + pl.health);
            var fire = w.projectiles.Find(s => s.kind == GameWorld.ShotKind.Fire);
            Assert(fire != null && Mathf.Abs(fire.width - 1.6f) < .01f && fire.life > 3.5f, "Grenade must leave a 1.6 u fire patch for ~4 s");
            Run(w, 1f); Assert(pl.health < pl.maxHealth - 18, "Standing in the fire must burn");
            // Humbaradan kaçan kurtulur; ateşin üstünden zıplayan yanmaz
            w = Arena(root, EnemyRole.KorDemir, 6, out e); pl = w.player;
            w.StartAttack(e, "grenade");
            Run(w, 1.6f, i => new InputFrame { move = i >= 70 && i < 130 ? -1 : 0 });
            Assert(pl.health == pl.maxHealth, "Leaving the grenade mark should avoid it, health " + pl.health);
            w = Arena(root, EnemyRole.KorDemir, 6, out e); pl = w.player;
            w.SpawnFireForTest(new Vector2(5.5f, w.zone.floor), 1.6f, 4);
            // Tam zıplama (tuş basılı): kısa sıçrayış (.33 sn) x≈6.45'te, yanma sınırının (5.5 + .8 + .2 = 6.5) içinde iner; tam zıplama ≈6.62'de
            Run(w, 1.2f, i => new InputFrame { move = 1, jump = i == 0, jumpHeld = true });
            Assert(pl.health == pl.maxHealth || pl.position.x < 4.7f, "Jumping over a fire patch should not burn, health " + pl.health);

            // Yüksekteki oyuncuya humbara atar: humbara uzak saldırı yükseklik sınırını (3) kullanır, yakın dövüşün .8'ini değil
            // (oyuncu 23.5-26.6 platformunda, y -0.75: yerden 1.5 yukarıda)
            w = Arena(root, EnemyRole.KorDemir, 6, out e); pl = w.player;
            pl.position = new Vector2(25, -.75f); pl.view.ResetContacts(); e.position.x = 20; e.intentTime = 0;
            bool threw = false;
            Run(w, 1.5f, i => { threw |= e.attack != null && e.attack.kind == AttackKind.Grenade; return new InputFrame(); });
            Assert(threw, "Kör Demir must throw at a player on a platform above (ranged height gate)");

            // Kanca: tutarsa oyuncu ayağının dibine çekilir (8 hasar); blokta çekilmez
            w = Arena(root, EnemyRole.KorDemir, 3.5f, out e); pl = w.player;
            var hook = EnemyProfile.For(EnemyRole.KorDemir).Find("hook");
            w.StartAttack(e, "hook"); Run(w, hook.windup + .5f);
            Assert(pl.health == pl.maxHealth - 8 && Mathf.Abs(e.position.x - pl.position.x) < 1.7f, "Hook should pull the player in, gap " + Mathf.Abs(e.position.x - pl.position.x));
            w = Arena(root, EnemyRole.KorDemir, 3.5f, out e); pl = w.player;
            w.StartAttack(e, "hook"); Run(w, hook.windup + .5f, i => new InputFrame { block = true });
            Assert(Mathf.Abs(e.position.x - pl.position.x) > 3f, "A blocked hook must not pull");

            // Yağ küpü: uzun yanan şerit (4.5)
            w = Arena(root, EnemyRole.KorDemir, 6, out e);
            w.StartAttack(e, "oil"); Run(w, .7f + .9f + .1f);
            Assert(w.projectiles.Exists(s => s.kind == GameWorld.ShotKind.Fire && Mathf.Abs(s.width - 4.5f) < .01f), "Oil pot must leave a 4.5 u burning strip");

            // İkinci evre: arenanın yarısı tutuşur, ortada dar bir alan kalır
            w = Arena(root, EnemyRole.KorDemir, 3, out e); pl = w.player; float home = e.home;
            e.health = e.maxHealth * .45f; Run(w, 2.2f);
            Assert(e.phase == 2, "Kör Demir did not enter phase 2");
            pl.position = new Vector2(home + 3, w.zone.floor); pl.invincible = 0; float hp = pl.health; Run(w, 1f);
            Assert(pl.health < hp, "Arena fire must burn beside the boss (home + 3)");
            pl.position = new Vector2(home - 2, w.zone.floor); pl.invincible = 0; hp = pl.health; Run(w, 1f);
            Assert(pl.health == hp || e.Attacking, "The middle strip (home - 2) must stay free of fire");
            Debug.Log("KARAMANI_BOSS_KORDEMIR_PASS · grenade blast + fire patch, dodge, jump over fire, throws up to a platform, hook pull / blocked, oil strip, arena fire");
        }

        static void Kurtbas(GameRoot root)
        {
            // Balta fırtınası: 4 vuruş x 10, aralar .42 (oyuncunun .4 sn dokunulmazlığından uzun)
            // Her vuruşta oyuncu ~.2 birim geri savrulur: 1.0'dan başlayınca dördüncü vuruş da menzilde (1.6 + .23)
            var w = Arena(root, EnemyRole.Kurtbas, 1f, out var e); var pl = w.player;
            var storm = EnemyProfile.For(EnemyRole.Kurtbas).Find("storm");
            w.StartAttack(e, "storm"); Run(w, storm.Duration + .05f);
            Assert(pl.health == pl.maxHealth - 40, "Axe storm should land 4 x 10, health " + pl.health);

            // Üstünden atlama: oyuncunun arkasına, sağlam zemine iner
            w = Arena(root, EnemyRole.Kurtbas, 1.5f, out e); pl = w.player;
            w.StartAttack(e, "leapover"); Run(w, 1.2f);
            Assert(e.position.x < pl.position.x && e.grounded, "Leap-over must land behind the player: boss " + e.position.x + " player " + pl.position.x);
            // Arkası çukursa önüne iner (çukur 13.5-15.7)
            w = Arena(root, EnemyRole.Kurtbas, 1.5f, out e); pl = w.player;
            pl.position = new Vector2(16.4f, w.zone.floor); e.position.x = 17.9f; e.face = -1;
            w.StartAttack(e, "leapover"); Run(w, 1.4f);
            Assert(e.grounded && Mathf.Abs(e.position.y - w.zone.floor) < .01f && !(e.position.x > 13.5f && e.position.x < 15.7f), "Leap-over landed in the pit at " + e.position.x);

            // Sıçrayıp çakılma: kırmızı, blok işe yaramaz (24); kaçan kurtulur
            w = Arena(root, EnemyRole.Kurtbas, 6, out e); pl = w.player;
            w.StartAttack(e, "leapslam"); Run(w, 1.8f, i => new InputFrame { block = true });
            Assert(pl.health == pl.maxHealth - 24, "Leap slam must ignore block for 24, health " + pl.health);
            w = Arena(root, EnemyRole.Kurtbas, 6, out e); pl = w.player;
            w.StartAttack(e, "leapslam");
            Run(w, 1.8f, i => new InputFrame { move = i >= 60 && i < 120 ? -1 : 0 });
            Assert(pl.health == pl.maxHealth, "Running out of the slam radius should avoid it, health " + pl.health);

            // Çılgınlık: ikinci evrede vuruşla sendelemez, %25 fazla hasar alır, denge kırılsa da durmaz
            w = Arena(root, EnemyRole.Kurtbas, 1.3f, out e);
            e.health = e.maxHealth * .45f; Run(w, 1.5f);
            Assert(e.phase == 2, "Kurtbaş did not enter phase 2");
            e.Enter(ActionState.Idle); e.invincible = 0; float hp = e.health; e.guard = 5;
            w.Damage(e, 20, 1, false, false, w.player);
            Assert(Mathf.Approximately(e.health, hp - 25) && e.state != ActionState.Hurt && e.guard > 0, "Frenzy: +25% damage, no stagger, guard refills");
            Debug.Log("KARAMANI_BOSS_KURTBAS_PASS · 4-hit storm, leap-over behind + pit fallback, red leap slam + dodge, frenzy");
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

            // Savuşturulan asa: sendeleme 1.4 sn (spec §3.6)
            var staff = EnemyProfile.For(EnemyRole.Sorcerer).Find("staff");
            w = Arena(root, EnemyRole.Sorcerer, 1.3f, out e); pl = w.player;
            w.StartAttack(e, "staff");
            int parry = Mathf.RoundToInt((staff.windup - .06f) * 120);
            Run(w, staff.windup + .1f, i => new InputFrame { block = i >= parry });
            Assert(e.state == ActionState.Hurt && Mathf.Abs(e.duration - 1.4f) < .01f && pl.health == pl.maxHealth, "Parried staff should stagger the sorcerer for 1.4 s: " + e.state + " " + e.duration);
            Debug.Log("KARAMANI_ENEMY_SORCERER_PASS · flame wave + jump, orbs + snuff, teleport + pit guard, phase 2, half-boss bar, parried staff 1.4 s");
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

            // Kırmızı savuruş: teberliye DOĞRU (içinden) zamanında yuvarlanma yalnız dokunulmazlıkla kurtarır
            w = Arena(root, EnemyRole.Halberdier, 2.2f, out e); pl = w.player;
            var sweep = EnemyProfile.For(EnemyRole.Halberdier).Find("sweep");
            w.StartAttack(e, "sweep");
            int press = Mathf.RoundToInt((sweep.windup - .03f) * 120);
            Run(w, 1.7f, i => new InputFrame { move = i >= press && i < press + 2 ? 1 : 0, dodge = i == press });
            Assert(pl.health == pl.maxHealth, "Rolling through the halberdier should avoid the red sweep, health " + pl.health);
            // Kontrol: aynı hareket, yuvarlanmadan: savuruş isabet eder
            w = Arena(root, EnemyRole.Halberdier, 2.2f, out e); pl = w.player;
            w.StartAttack(e, "sweep");
            Run(w, 1.7f, i => new InputFrame { move = i >= press && i < press + 2 ? 1 : 0 });
            Assert(pl.health == pl.maxHealth - 22, "Control without dodge should take the sweep (22), health " + pl.health);

            // Kırmızı savuruş savuşturulamaz: blok vuruş anında savuşturma penceresindeyken bile 22 hasar, teberli sendelemez
            w = Arena(root, EnemyRole.Halberdier, 2.2f, out e); pl = w.player;
            w.StartAttack(e, "sweep");
            press = Mathf.RoundToInt((sweep.windup - .06f) * 120);
            bool windowAtStrike = false;
            for (int i = 0; i < Mathf.RoundToInt((sweep.windup + .1f) * 120); i++)
            {
                float hp = pl.health;
                w.Simulate(1f / 120, new InputFrame { block = i >= press }, true);
                if (pl.health < hp) windowAtStrike = pl.parryWindow > 0;
            }
            Assert(windowAtStrike, "Setup: the parry window should still be open when the red sweep strikes");
            Assert(pl.health == pl.maxHealth - 22 && e.state == ActionState.Light && e.attack?.name == "sweep", "Timed parry must not stop a red sweep: health " + pl.health + ", halberdier " + e.state);

            // Beyaz vuruş: zamanında blok savuşturur, saldıran sendeler, infaz açılır
            w = Arena(root, EnemyRole.Nefer, 1.2f, out e); pl = w.player;
            w.StartAttack(e, "combo");
            press = Mathf.RoundToInt((combo.windup - .06f) * 120);
            Run(w, combo.windup + .1f, i => new InputFrame { block = i >= press });
            Assert(pl.health == pl.maxHealth && e.state == ActionState.Hurt && pl.riposte > 0, "Parry of a white attack should stagger and open a riposte");

            // Savuşturma sendelemesi Stagger'dan geçer: denge dolar, geri itme yok; infazdan sonraki sıradan vuruş yeniden sendeletmez
            w = Arena(root, EnemyRole.Nefer, 1.2f, out e); pl = w.player; e.health = e.maxHealth = 300;   // infazdan sağ çıksın
            w.StartAttack(e, "combo");
            press = Mathf.RoundToInt((combo.windup - .06f) * 120);
            Run(w, combo.windup + .1f, i => new InputFrame { block = i >= press });
            Assert(e.state == ActionState.Hurt && e.guard == e.maxGuard && e.knockback == 0 && pl.riposte > 0, "Parry should stagger through Stagger: guard " + e.guard + "/" + e.maxGuard + ", knockback " + e.knockback);
            float before = e.health; bool hit = false;
            for (int i = 0; i < 120 && !hit; i++) { w.Simulate(1f / 120, new InputFrame { light = i == 0 }, true); hit = e.health < before; }
            Assert(hit, "Execution never connected: player " + pl.state + ", gap " + (e.position.x - pl.position.x));
            for (int i = 0; i < 120 && pl.Locked; i++) w.Simulate(1f / 120, new InputFrame(), true);
            e.position = new Vector2(pl.position.x + 1, w.zone.floor); e.velocity = Vector2.zero; e.view.ResetContacts();   // infaz savurur; vuruş menzilinde dursun
            before = e.health; hit = false;
            for (int i = 0; i < 90 && !hit; i++) { w.Simulate(1f / 120, new InputFrame { light = i == 0 }, true); hit = e.health < before; }
            Assert(hit && e.duration < .8f && pl.riposte <= 0, "Light hit after an execution re-staggered: hit " + hit + ", hurt duration " + e.duration + ", riposte " + pl.riposte);
            Debug.Log("KARAMANI_ENEMY_ATTACKS_PASS · active windows, red ignores block and timed parry, dodge-through avoids red (control is hit), white parry staggers via Stagger, no re-stagger after execution");
        }
    

        // Her yakın dövüş / yere vurma saldırısı: hazırlık bitmeden vurmaz, etkin pencerede hasar x vuruş sayısı kadar iner
        static void AllMelee(GameRoot root)
        {
            int count = 0;
            foreach (var role in new[] { EnemyRole.Nefer, EnemyRole.Spearman, EnemyRole.Halberdier, EnemyRole.Musketeer, EnemyRole.Archer, EnemyRole.Sorcerer })
                foreach (var k in EnemyProfile.For(role).attacks)
                {
                    if (k.kind is not (AttackKind.Melee or AttackKind.Slam)) continue;
                    float gap = Mathf.Max(.4f, Mathf.Min(k.reach - .1f, k.reach * .9f));
                    var w = Arena(root, role, gap, out var e); var pl = w.player;
                    Assert(w.StartAttack(e, k.name), role + "." + k.name + " did not start");
                    Run(w, k.windup - .02f);
                    Assert(pl.health == pl.maxHealth, role + "." + k.name + " hit before its wind-up ended, health " + pl.health);
                    Run(w, k.Duration);
                    Assert(pl.health == pl.maxHealth - k.damage * k.hits, role + "." + k.name + " should deal " + k.damage + " x " + k.hits + " at gap " + gap + ", health " + pl.health);
                    count++;
                }
            Debug.Log("KARAMANI_ENEMY_ALLMELEE_PASS · " + count + " melee/slam attacks: no hit before wind-up, damage x hits in the active window");
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
            Run(w, 1f / 120);
            Assert(e.state == ActionState.Block && e.view.Clip == "block", "Spearman should show its block clip on a shield hit: " + e.state + " / " + e.view.Clip);
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

            // Kalkanla itme: blok tutan oyuncuyu da ~1.5 b geri iter ve dayanıklılığını düşürür (spec §3.2)
            w = Arena(root, EnemyRole.Spearman, 1.1f, out e); pl = w.player; float x0 = pl.position.x;
            w.StartAttack(e, "bash");
            Run(w, 1f, i => new InputFrame { block = true });
            Assert(x0 - pl.position.x >= 1.2f && 100 - pl.stamina >= 20, "Shield bash should push a blocking player back and drain stamina: moved " + (x0 - pl.position.x) + ", stamina " + pl.stamina);

            // Duruş: teberli kendi savuruşunun hazırlığında vurulunca durmaz (can düşer); nefer kombosunda vurulunca durur
            w = Arena(root, EnemyRole.Halberdier, 2.2f, out e);
            w.StartAttack(e, "sweep"); Run(w, .3f);
            w.Damage(e, 20, 1, false, false, w.player);
            Assert(e.state == ActionState.Light && e.attack?.name == "sweep" && e.health == e.maxHealth - 20, "Poise: halberdier wind-up should not be interrupted: " + e.state + " hp " + e.health);
            w = Arena(root, EnemyRole.Nefer, 1.2f, out e);
            w.StartAttack(e, "combo"); Run(w, .2f);
            w.Damage(e, 20, 1, false, false, w.player);
            Assert(e.state == ActionState.Hurt, "Nefer combo should be interrupted by a hit: " + e.state);

            // Sendeleme kısalmaz: sendeleyen neferi vuran sıradan vuruş kalan süreyi kısaltmaz
            w = Arena(root, EnemyRole.Nefer, 1.2f, out e); e.guard = 10;
            w.Damage(e, 20, 1, false, false, w.player);
            Run(w, .15f);   // düşmanın .14 sn'lik dokunulmazlığı geçsin
            w.Damage(e, 20, 1, false, false, w.player);
            Assert(e.state == ActionState.Hurt && e.duration - e.stateTime >= .7f, "A light hit shortened the stagger: remaining " + (e.duration - e.stateTime));

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
            Debug.Log("KARAMANI_ENEMY_BEHAVIOUR_PASS · range, two attackers, punish, height, dead player, shield + block clip, guard break, shield bash push, poise, stagger kept, turn delay");
        }
    }
}
