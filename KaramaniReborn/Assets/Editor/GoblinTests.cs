using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KaramaniReborn.Editor
{
    // 2. kısım temelinin (Docs/plans/2026-10-12-goblin-part2-plan.md, Görev 1) otomatik denetimi:
    // roller, 4-6. bölgeler, kapı bekçileri, ilerleme, pusu noktaları ve saldırı sırası
    static class GoblinTests
    {
        public static void Verify(GameRoot root)
        {
            Roles(root);
            Zones(root);
            Gates(root);
            Progression(root);
            Menu(root);
            Perches(root);
            AttackerCaps(root);
            Debug.Log("KARAMANI_GOBLIN_FOUNDATION_PASS · nine goblin roles with placeholder profiles, zones 4-6 (titles, counts, gate keepers, pits <= 2.8), open gate in Sisli Koru, 3 -> 4 -> ... -> 6 -> complete, seven menu cards, perches over ground, goblin cap 3 / human cap 2");
        }

        static void Assert(bool ok, string message) => EnemyTests.Assert(ok, message);
        static void Run(GameWorld w, float seconds, System.Func<int, InputFrame> input = null) => EnemyTests.Run(w, seconds, input);

        // Deneme alanı: verilen bölüm (Sisli Koru = 4), bölümün kendi düşmanları x 58'e alınır, oyuncu x 4'te sağa bakar,
        // denenen düşman oyuncunun gap birim önüne konur. intentTime = 99: kendiliğinden saldırmaz.
        static GameWorld GoblinArena(GameRoot root, int zone, EnemyRole role, float gap, out Actor e)
        {
            root.LoadZone(zone, true); var w = root.world;
            foreach (var other in w.enemies) other.position.x = 58;
            w.player.position = new Vector2(4, w.zone.floor); w.player.face = 1; w.player.view.ResetContacts();
            e = w.Spawn(4 + gap, w.zone.floor, role); e.face = gap > 0 ? -1 : 1; e.intentTime = 99; e.view.ResetContacts();
            return w;
        }

        static int Count(GameWorld w, EnemyRole role) => w.enemies.FindAll(a => a.role == role).Count;

        static readonly EnemyRole[] GoblinRoles =
        {
            EnemyRole.Bicakci, EnemyRole.ZehirliOkcu, EnemyRole.Pusucu, EnemyRole.GoblinKalkanli, EnemyRole.Comlekci,
            EnemyRole.Kaba, EnemyRole.Kircil, EnemyRole.Catal, EnemyRole.KaraDis
        };

        // Dokuz rol: başlık, can, boy, çizim sayfası, goblin bayrağı, temel saldırı; 1. kısım rolleri goblin değil
        static void Roles(GameRoot root)
        {
            var expected = new (EnemyRole role, string title, float health, float size, string sheet)[]
            {
                (EnemyRole.Bicakci, "Bıçakçı", 30, .68f, "duelist"), (EnemyRole.ZehirliOkcu, "Zehirli Okçu", 30, .68f, "archer"),
                (EnemyRole.Pusucu, "Pusucu", 40, .68f, "duelist"), (EnemyRole.GoblinKalkanli, "Kalkanlı Mızrakçı", 70, .68f, "shield"),
                (EnemyRole.Comlekci, "Ateş Çömlekçisi", 35, .68f, "duelist"), (EnemyRole.Kaba, "Kaba", 130, 1f, "captain"),
                (EnemyRole.Kircil, "Kan Şamanı Kırçıl", 200, .9f, "captain"), (EnemyRole.Catal, "Domuz Binicisi Çatal", 160, 1.15f, "captain"),
                (EnemyRole.KaraDis, "Oymak Beyi Kara Diş", 420, 1.1f, "captain"),
            };
            Assert(expected.Length == GoblinRoles.Length, "Setup: nine goblin roles");
            foreach (var x in expected)
            {
                var p = EnemyProfile.For(x.role);
                Assert(p.goblin && p.title == x.title && p.health == x.health && Mathf.Approximately(p.size, x.size) && p.sheet == x.sheet, x.role + " profile " + p.title + "/" + p.health + "/" + p.size + "/" + p.sheet);
                Assert(p.attacks.Length > 0, x.role + " has no attack");
                foreach (var k in p.attacks) Assert(!string.IsNullOrEmpty(k.clip) && k.Duration > 0, x.role + "." + k.name + " has no clip or duration");
                GoblinArena(root, 4, x.role, 3, out var e);
                Assert(e.maxHealth == x.health && Mathf.Approximately(e.size, x.size) && e.view.variant == x.sheet, x.role + " spawned with " + e.maxHealth + "/" + e.size + "/" + e.view.variant);
                Assert(e.perch == -1, x.role + " must start unbound to a perch");
            }
            Assert(EnemyProfile.For(EnemyRole.Kaba).bulk == 2f && EnemyProfile.For(EnemyRole.Kaba).poise, "Kaba must be 1.0 tall with bulk 2");
            Assert(EnemyProfile.For(EnemyRole.Kircil).halfBoss && EnemyProfile.For(EnemyRole.Catal).halfBoss, "Kırçıl and Çatal are half-bosses");
            foreach (var role in new[] { EnemyRole.Nefer, EnemyRole.Spearman, EnemyRole.Halberdier, EnemyRole.Musketeer, EnemyRole.Archer, EnemyRole.Sorcerer, EnemyRole.Captain,
                         EnemyRole.Arslan, EnemyRole.KorDemir, EnemyRole.Kurtbas, EnemyRole.KaraMurad })
                Assert(!EnemyProfile.For(role).goblin, role + " must not be a goblin");

            // Her yer tutucu yakın dövüş saldırısı: hazırlık bitmeden vurmaz, etkin pencerede hasar x vuruş kadar iner
            int melee = 0;
            foreach (var role in GoblinRoles)
                foreach (var k in EnemyProfile.For(role).attacks)
                {
                    if (k.kind is not (AttackKind.Melee or AttackKind.Slam)) continue;
                    float gap = Mathf.Max(.4f, Mathf.Min(k.reach - .1f, k.reach * .9f));
                    var w = GoblinArena(root, 4, role, gap, out var e); var pl = w.player;
                    Assert(w.StartAttack(e, k.name), role + "." + k.name + " did not start");
                    Run(w, k.windup - .02f);
                    Assert(pl.health == pl.maxHealth, role + "." + k.name + " hit before its wind-up ended, health " + pl.health);
                    Run(w, k.Duration);
                    Assert(pl.health == pl.maxHealth - k.damage * k.hits, role + "." + k.name + " should deal " + k.damage + " x " + k.hits + " at gap " + gap + ", health " + pl.health);
                    melee++;
                }
            Assert(melee >= GoblinRoles.Length - 1, "Every goblin role needs a basic melee attack, found " + melee);
        }

        static bool Differs(Color a, Color b) => Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b) > .05f;

        static string SurfaceKey(ZoneDefinition z)
        {
            var parts = new List<string>();
            foreach (var s in z.surfaces) parts.Add(s.left + "," + s.right + "," + s.y + "," + s.oneWay);
            return string.Join(";", parts);
        }

        // Zemin parçaları arasındaki boşluklar (pit) en çok 2.8; zemin bandı bölüm 0-3 ile aynı
        static void GroundBand(ZoneDefinition z)
        {
            var ground = new List<Surface>();
            foreach (var s in z.surfaces) if (!s.oneWay) ground.Add(s);
            ground.Sort((a, b) => a.left.CompareTo(b.left));
            Assert(ground.Count >= 3, "Zone " + z.index + " needs at least three ground pieces");
            Assert(ground[0].left <= -8f && ground[^1].right >= 61f, "Zone " + z.index + " ground must span -8..61");
            foreach (var s in ground) Assert(s.y == -2.25f && Mathf.Approximately(s.thickness, 3.4f), "Zone " + z.index + " ground band y/thickness " + s.y + "/" + s.thickness);
            for (int i = 0; i + 1 < ground.Count; i++)
            {
                float pit = ground[i + 1].left - ground[i].right;
                Assert(pit > 0 && pit <= 2.8f + 1e-4f, "Zone " + z.index + " pit " + i + " is " + pit + " wide (max 2.8)");
            }
            Assert(z.length == 57 && z.floor == -2.25f, "Zone " + z.index + " length/floor " + z.length + "/" + z.floor);
        }

        static bool OnSurface(ZoneDefinition z, Vector2 p)
        {
            foreach (var s in z.surfaces) if (p.x >= s.left + .3f && p.x <= s.right - .3f && Mathf.Abs(s.y - p.y) < .02f) return true;
            return false;
        }

        // Bölge 4-6: ad, alt ad, boss adı, dizilim, kapı bekçisi, yer, renkler, sanat eşlemesi, pusucu bağları
        static void Zones(GameRoot root)
        {
            Assert(ZoneDefinition.Count == 7, "Seven zones, found " + ZoneDefinition.Count);
            bool threw = false; try { ZoneDefinition.Create(7); } catch (System.ArgumentOutOfRangeException) { threw = true; }
            Assert(threw, "Zone 7 must not exist");
            string[] titles = { "SİSLİ KORU", "DERİN MAĞARALAR", "KEMİK TAHTI" };
            string[] subtitles = { "Sınırın ötesindeki orman", "Çatlak Kaya'nın inleri", "Eski maden galerileri" };
            string[] bossNames = { "", "Kan Şamanı Kırçıl", "Oymak Beyi Kara Diş" };
            int[] artIndex = { 2, 3, 1 };
            int[] perchCount = { 3, 3, 2 };
            int[] total = { 8, 10, 11 };
            var seen = new HashSet<string>(); var ambient = new List<Color>(); var fog = new List<Color>(); var accent = new List<Color>();
            for (int z = 0; z < 4; z++)
            {
                var d = ZoneDefinition.Create(z);
                seen.Add(SurfaceKey(d)); ambient.Add(d.ambient); fog.Add(d.fog); accent.Add(d.accent);
                Assert(d.perches.Length == 0 && d.artIndex == z && d.banner == "", "Part-1 zone " + z + " must have no perches, own art, no banner");
            }
            for (int i = 0; i < 3; i++)
            {
                int zone = 4 + i;
                root.LoadZone(zone, true); var w = root.world; var d = w.zone;
                Assert(d.index == zone && d.title == titles[i] && d.subtitle == subtitles[i] && d.bossName == bossNames[i], "Zone " + zone + " names " + d.title + " / " + d.subtitle + " / " + d.bossName);
                Assert(d.artIndex == artIndex[i] && ZoneDefinition.ArtIndexFor(zone) == artIndex[i], "Zone " + zone + " placeholder painting " + d.artIndex);
                Assert(w.enemies.Count == total[i], "Zone " + zone + " enemy count " + w.enemies.Count);
                Assert(root.hud.EnemyMarkers == w.enemies.Count, "Zone " + zone + " minimap shows " + root.hud.EnemyMarkers + " of " + w.enemies.Count);
                GroundBand(d);
                Assert(seen.Add(SurfaceKey(d)), "Zone " + zone + " reuses another zone's surfaces");
                foreach (var c in ambient) Assert(Differs(c, d.ambient), "Zone " + zone + " ambient is too close to another zone");
                foreach (var c in fog) Assert(Differs(c, d.fog), "Zone " + zone + " fog is too close to another zone");
                foreach (var c in accent) Assert(Differs(c, d.accent), "Zone " + zone + " accent is too close to another zone");
                ambient.Add(d.ambient); fog.Add(d.fog); accent.Add(d.accent);
                Assert(d.perches.Length == perchCount[i], "Zone " + zone + " perches " + d.perches.Length);

                // Dizilim (zone plan, spec §6)
                var bosses = w.enemies.FindAll(a => a.boss);
                switch (zone)
                {
                    case 4:
                        Assert(Count(w, EnemyRole.Bicakci) == 4 && Count(w, EnemyRole.ZehirliOkcu) == 2 && Count(w, EnemyRole.Pusucu) == 2, "Zone 4 roles");
                        Assert(bosses.Count == 0 && GameWorld.ZoneBosses[4] == null, "Zone 4 has no gate keeper");
                        break;
                    case 5:
                        Assert(Count(w, EnemyRole.Bicakci) == 3 && Count(w, EnemyRole.Pusucu) == 2 && Count(w, EnemyRole.GoblinKalkanli) == 2
                            && Count(w, EnemyRole.Comlekci) == 1 && Count(w, EnemyRole.Kaba) == 1 && Count(w, EnemyRole.Kircil) == 1, "Zone 5 roles");
                        Assert(bosses.Count == 1 && bosses[0].role == EnemyRole.Kircil && w.enemies[^1] == bosses[0], "Zone 5 gate keeper is Kırçıl (boss = true, last)");
                        break;
                    default:
                        Assert(Count(w, EnemyRole.Bicakci) == 3 && Count(w, EnemyRole.GoblinKalkanli) == 2 && Count(w, EnemyRole.ZehirliOkcu) == 1
                            && Count(w, EnemyRole.Comlekci) == 2 && Count(w, EnemyRole.Kaba) == 1 && Count(w, EnemyRole.Catal) == 1 && Count(w, EnemyRole.KaraDis) == 1, "Zone 6 roles");
                        Assert(bosses.Count == 1 && bosses[0].role == EnemyRole.KaraDis && w.enemies[^1] == bosses[0], "Zone 6 gate keeper is Kara Diş (boss = true, last)");
                        Assert(!w.enemies.Find(a => a.role == EnemyRole.Catal).boss, "Çatal is a half-boss, not the gate keeper");
                        break;
                }
                // Kapı bekçisinin çubuğu başlığı gösterir (Kırçıl yarı-boss ama boss = true)
                if (bosses.Count == 1)
                {
                    w.player.position = new Vector2(bosses[0].position.x - 3, d.floor);
                    Assert(w.BarTarget == bosses[0], "Boss bar does not show the gate keeper of zone " + zone);
                }

                // Her düşman sağlam bir yüzeyde doğar; okçular platformda, pusucular pusu noktasına bağlı
                var perchesTaken = new HashSet<int>();
                foreach (var a in w.enemies)
                {
                    Assert(OnSurface(d, a.position), "Zone " + zone + " " + a.role + " spawns over nothing at " + a.position);
                    if (a.role == EnemyRole.ZehirliOkcu) Assert(a.position.y > d.floor + .3f, "Zone " + zone + " archers stand on platforms, found y " + a.position.y);
                    if (a.role == EnemyRole.Pusucu)
                    {
                        Assert(a.perch >= 0 && a.perch < d.perches.Length && perchesTaken.Add(a.perch), "Zone " + zone + " Pusucu bound to perch " + a.perch);
                        Assert(Mathf.Abs(a.position.x - d.perches[a.perch].x) < .01f, "Zone " + zone + " Pusucu starts under its perch");
                    }
                    else Assert(a.perch == -1, "Zone " + zone + " " + a.role + " must not be bound to a perch");
                }
                Assert(perchesTaken.Count == Count(w, EnemyRole.Pusucu), "Zone " + zone + " every Pusucu has its own perch");

                // Sahnede her pusu noktası için okunur bir işaret çizilir
                int markers = 0;
                foreach (var sr in root.stage.root.GetComponentsInChildren<SpriteRenderer>()) if (sr.name == "Ambush perch") markers++;
                Assert(markers == d.perches.Length, "Zone " + zone + " draws " + markers + " perch markers for " + d.perches.Length + " perches");
            }
            root.LoadZone(0, true);
            int none = 0; foreach (var sr in root.stage.root.GetComponentsInChildren<SpriteRenderer>()) if (sr.name == "Ambush perch") none++;
            Assert(none == 0, "Part-1 zones draw no perch markers");
        }

        // Kapı: 4'te bekçi yok, hemen açık; 5 ve 6'da bekçi (Kırçıl, Kara Diş) ölünce açılır, Çatal'ın ölümü açmaz
        static void Gates(GameRoot root)
        {
            root.LoadZone(4, true); var w = root.world;
            w.Simulate(1f / 120, new InputFrame(), false);
            Assert(w.cleared && w.enemies.TrueForAll(a => !a.Dead), "Sisli Koru's gate must be open at once with every goblin alive");
            w.player.position.x = 55; w.Simulate(1f / 120, new InputFrame { interact = true }, false);
            Assert(w.transitionRequested && w.InteractionHint == "E · İç kapıdan ilerle", "Sisli Koru exit did not work, hint '" + w.InteractionHint + "'");

            foreach (int zone in new[] { 5, 6 })
            {
                root.LoadZone(zone, true); w = root.world;
                w.player.position.x = 55; w.Simulate(1f / 120, new InputFrame { interact = true }, false);
                Assert(!w.cleared && !w.transitionRequested, "Zone " + zone + " gate must be sealed while the keeper lives");
                if (zone == 6)
                {
                    w.Damage(w.enemies.Find(a => a.role == EnemyRole.Catal), 5000, 1, true, true);
                    w.Simulate(1f / 120, new InputFrame { interact = true }, false);
                    Assert(!w.cleared && !w.transitionRequested, "Killing Çatal must not open the gate");
                }
                var keeper = w.enemies.Find(a => a.boss); w.Damage(keeper, 5000, 1, true, true);
                w.Simulate(1f / 120, new InputFrame { interact = true }, false);
                Assert(w.cleared && w.transitionRequested, "Zone " + zone + " gate must open when " + keeper.role + " falls");
                Assert(w.notification == w.zone.bossName + " yenildi. İç kapı açıldı.", "Zone " + zone + " victory line '" + w.notification + "'");
            }
        }

        static void Clear(GameWorld w)
        {
            var boss = w.enemies.Find(a => a.boss); if (boss != null) w.Damage(boss, 5000, 1, true, true);
            w.player.position.x = 55; w.Simulate(1f / 120, new InputFrame { interact = true }, false);
        }

        // Bölüm 3 -> 4, ... , 6 -> yolculuk tamam; kısım başlığı yalnız 4'e oynarken girince
        static void Progression(GameRoot root)
        {
            root.LoadZone(3, true); var w = root.world;
            Assert(string.IsNullOrEmpty(w.notification) || w.notificationTime <= 0, "Only the second part opens with a banner");
            Clear(w); Assert(w.transitionRequested, "Zone 3 boss did not unlock the gate");
            root.AdvanceIfRequested();
            Assert(root.world.zone.index == 4 && !root.journeyComplete && root.selectedZone == 4 && !root.menu, "After zone 3's boss play must continue into zone 4");
            Assert(root.world.notification == "İKİNCİ KISIM · ÇATLAK KAYA" && root.world.notificationTime > 0, "Part banner missing: '" + root.world.notification + "'");
            Assert(ZoneDefinition.Create(4).banner == "İkinci Kısım · Çatlak Kaya".ToUpper(System.Globalization.CultureInfo.GetCultureInfo("tr-TR")), "Banner must be upper-cased in tr-TR");

            Clear(root.world); root.AdvanceIfRequested();
            Assert(root.world.zone.index == 5, "Zone 4 -> zone 5, found " + root.world.zone.index);
            Clear(root.world); root.AdvanceIfRequested();
            Assert(root.world.zone.index == 6 && !root.journeyComplete, "Zone 5 -> zone 6, found " + root.world.zone.index);
            Assert(root.world.notificationTime <= 0 || root.world.notification != root.world.zone.banner, "Banner repeated in zone 6");
            Clear(root.world); root.AdvanceIfRequested();
            Assert(root.journeyComplete && root.world.zone.index == 6, "Zone 6 -> the journey is complete");

            root.LoadZone(4, false);
            Assert(string.IsNullOrEmpty(root.world.notification) || root.world.notificationTime <= 0, "A menu preview must not show the banner");
            root.LoadZone(0, true);
        }

        // Bölge seçim menüsü: 7 kart, hepsi tuvalin içinde ve birbirini örtmeden
        static void Menu(GameRoot root)
        {
            var corners = new Vector3[4];
            root.hud.root.GetWorldCorners(corners);
            var canvas = Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
            var cards = new List<Rect>();
            foreach (var image in root.hud.root.GetComponentsInChildren<Image>(true))
            {
                if (image.name != "Region painting") continue;
                image.rectTransform.GetWorldCorners(corners);
                var rect = Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
                Assert(canvas.Contains(new Vector2(rect.xMin, rect.yMin)) && canvas.Contains(new Vector2(rect.xMax, rect.yMax)), "Zone card outside the canvas: " + rect);
                cards.Add(rect);
            }
            Assert(cards.Count == 7, "The zone menu lists " + cards.Count + " cards, expected 7");
            for (int i = 0; i < cards.Count; i++) for (int j = i + 1; j < cards.Count; j++) Assert(!cards[i].Overlaps(cards[j]), "Zone cards " + i + " and " + j + " overlap");
            var titles = new HashSet<string>();
            foreach (var text in root.hud.root.GetComponentsInChildren<Text>(true)) for (int z = 0; z < 7; z++) if (text.text == ZoneDefinition.Create(z).title) titles.Add(text.text);
            Assert(titles.Count == 7, "Seven zone titles in the menu, found " + titles.Count);
        }

        // Pusu noktaları: zeminden 2.2-3.2 yukarıda, yürünebilir zeminin üstünde, hiçbir boşluğun üstünde değil
        static void Perches(GameRoot root)
        {
            for (int zone = 0; zone < 7; zone++)
            {
                var d = ZoneDefinition.Create(zone);
                Assert(d.perches.Length == (zone == 4 || zone == 5 ? 3 : zone == 6 ? 2 : 0), "Zone " + zone + " perch count " + d.perches.Length);
                foreach (var p in d.perches)
                {
                    float up = p.y - d.floor;
                    Assert(up >= 2.2f && up <= 3.2f, "Zone " + zone + " perch " + p + " is " + up + " above the floor (2.2-3.2)");
                    bool over = false;
                    foreach (var s in d.surfaces) if (!s.oneWay && Mathf.Approximately(s.y, d.floor) && p.x >= s.left + .3f && p.x <= s.right - .3f) over = true;
                    Assert(over, "Zone " + zone + " perch " + p + " is not over standable ground (pit or edge)");
                }
            }
        }

        // Saldırı sırası: goblinlerden en çok 3 saldırır, dördüncüsü bekler; insanlarda hâlâ 2; iki taraf ayrı sayılır
        static void AttackerCaps(GameRoot root)
        {
            Assert(GameWorld.MaxGoblinAttackers == 3 && GameWorld.MaxAttackers == 2, "Caps are 3 goblins and 2 humans");
            var w = GoblinArena(root, 4, EnemyRole.Bicakci, 1.0f, out var e); e.intentTime = 0; w.player.maxHealth = w.player.health = 9999;
            foreach (float gap in new[] { -1.0f, 1.1f, -1.1f }) w.Spawn(4 + gap, w.zone.floor, EnemyRole.Bicakci).intentTime = 0;
            int most = 0; bool fourthWaited = false;
            for (int i = 0; i < 360; i++)
            {
                w.Simulate(1f / 120, new InputFrame { block = true }, true);
                int n = w.AttackersOf(true); most = Mathf.Max(most, n);
                if (n == 3 && w.enemies.FindAll(a => a.role == EnemyRole.Bicakci && !a.Dead && !(a.attack != null && a.state == ActionState.Light) && Mathf.Abs(a.position.x - 4) < 3).Count == 1) fourthWaited = true;
            }
            Assert(most == 3, "Simultaneous goblin attackers " + most + " (spec: exactly 3)");
            Assert(fourthWaited, "A fourth goblin never waited while three attacked");

            w = GoblinArena(root, 4, EnemyRole.Nefer, 1.2f, out e); e.intentTime = 0; w.player.maxHealth = w.player.health = 9999;
            foreach (float gap in new[] { -1.2f, 1.4f, -1.4f }) w.Spawn(4 + gap, w.zone.floor, EnemyRole.Nefer).intentTime = 0;
            most = 0;
            for (int i = 0; i < 360; i++) { w.Simulate(1f / 120, new InputFrame { block = true }, true); most = Mathf.Max(most, w.AttackersOf(false)); Assert(w.AttackersOf(true) == 0, "No goblin is present"); }
            Assert(most == 2, "Humans are still capped at 2, found " + most);

            // İki taraf birbirinin sırasını yemez: aynı alanda 3 goblin ve 4 insan
            w = GoblinArena(root, 4, EnemyRole.Bicakci, 1.0f, out e); e.intentTime = 0; w.player.maxHealth = w.player.health = 9999;
            foreach (float gap in new[] { -1.0f, 1.1f }) w.Spawn(4 + gap, w.zone.floor, EnemyRole.Bicakci).intentTime = 0;
            foreach (float gap in new[] { -1.2f, 1.4f, -1.4f, 1.6f }) w.Spawn(4 + gap, w.zone.floor, EnemyRole.Nefer).intentTime = 0;
            int goblins = 0, humans = 0;
            for (int i = 0; i < 480; i++)
            {
                w.Simulate(1f / 120, new InputFrame { block = true }, true);
                goblins = Mathf.Max(goblins, w.AttackersOf(true)); humans = Mathf.Max(humans, w.AttackersOf(false));
                Assert(w.AttackersOf(true) <= 3 && w.AttackersOf(false) <= 2, "Cap exceeded: goblins " + w.AttackersOf(true) + ", humans " + w.AttackersOf(false));
            }
            Assert(goblins == 3 && humans == 2, "Caps are counted per side, reached goblins " + goblins + " / humans " + humans);
        }
    }
}
