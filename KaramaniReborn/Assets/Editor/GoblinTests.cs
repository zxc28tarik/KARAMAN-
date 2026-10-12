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
            RegularsA(root);
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

        // ---------- Görev 2: sıradan goblinler A (Bıçakçı, Zehirli Okçu, Goblin Kalkanlı, Kaba) ----------

        static void RegularsA(GameRoot root)
        {
            RegularValues(root);
            Knifeman(root);
            PoisonArrows(root);
            ArcherBehaviour(root);
            ShieldGoblin(root);
            PairLine(root);
            Brute(root);
            HeavyKnockdown(root);
            Debug.Log("KARAMANI_GOBLIN_REGULARS_A_PASS · Bıçakçı (2 x 8 leaping stab, flank behind on standable ground, hop back, dies to 2 lights, parry knock-down 1.2 s), Zehirli Okçu (8 + 3 x 3 poison without hitstun, green health bar, volley marks, backstep), Kalkanlı (frontal light blocked, back hit 2x guard, pair line 0.9), Kaba (poise vs lights, cleaver drains 35, red chain sweep + grab, grab gate, never retreats), heavy hits 3 goblins and knocks them down 1.5 s");
        }

        static void Attack(EnemyRole role, string name, float damage, bool red, int hits = 1)
        {
            var k = EnemyProfile.For(role).Find(name);
            Assert(k != null, role + "." + name + " is missing");
            Assert(k.damage == damage && k.red == red && k.hits == hits, role + "." + name + " damage/red/hits " + k.damage + "/" + k.red + "/" + k.hits + " (expected " + damage + "/" + red + "/" + hits + ")");
        }

        // Saldırıyı başlatır, süresi + payı kadar oynatır; oyuncunun kaybettiği can (block: blok tutarak)
        static float Lost(GameRoot root, EnemyRole role, string attack, float gap, bool block, out GameWorld w, out Actor e)
        {
            w = GoblinArena(root, 4, role, gap, out e);
            Assert(w.StartAttack(e, attack), role + "." + attack + " did not start");
            Run(w, EnemyProfile.For(role).Find(attack).Duration + .05f, i => new InputFrame { block = block });
            return w.player.maxHealth - w.player.health;
        }

        static void RegularValues(GameRoot root)
        {
            var b = EnemyProfile.For(EnemyRole.Bicakci);
            Assert(b.health == 30 && b.guard == 15 && b.speed == 2.2f && Mathf.Approximately(b.size, .68f) && b.keepMin == 1.2f && b.keepMax == 2f, "Bıçakçı values");
            var stab = b.Find("stab");
            Assert(stab.hits == 2 && Mathf.Approximately(stab.hitReach, 1.1f) && Mathf.Approximately(stab.hitGap, .45f) && stab.hitGap >= .42f && stab.then == "hop" && !stab.red, "Bıçakçı stab: 2 hits, reach 1.1, gap .45, hop after");
            Attack(EnemyRole.Bicakci, "stab", 8, false, 2); Attack(EnemyRole.Bicakci, "flankstab", 8, false);
            var flank = b.Find("flank");
            Assert(flank.kind == AttackKind.LeapOver && flank.cooldown == 4 && flank.then == "flankstab" && b.Find("flankstab").then == "hop", "Bıçakçı flank: leap-over, cooldown 4, one stab, hop");
            var hop = b.Find("hop");
            Assert(hop.kind == AttackKind.Backstep && hop.dash < 0 && hop.reach <= 0 && b.parryStagger >= 1.2f, "Bıçakçı hop is a chained Backstep; parry stagger >= 1.2");

            var o = EnemyProfile.For(EnemyRole.ZehirliOkcu);
            Assert(o.health == 30 && o.guard == 10 && o.speed == 1.8f && Mathf.Approximately(o.size, .68f) && o.ranged && o.keepMin == 6 && o.keepMax == 10, "Zehirli Okçu values");
            Attack(EnemyRole.ZehirliOkcu, "arrow", 8, false); Attack(EnemyRole.ZehirliOkcu, "volley", 12, true);
            var arrow = o.Find("arrow"); var backstep = o.Find("backstep");
            Assert(arrow.poisonDamage == 3 && arrow.poisonSeconds == 3 && o.Find("volley").kind == AttackKind.Volley, "Okçu arrow poisons 3 x 3 s; volley reuses Volley");
            Assert(backstep.kind == AttackKind.Backstep && backstep.reach == 2.5f && backstep.first, "Okçu backs away within 2.5");

            var k = EnemyProfile.For(EnemyRole.GoblinKalkanli);
            Assert(k.health == 70 && k.guard == 60 && k.speed == 1.2f && Mathf.Approximately(k.size, .68f) && k.shielded && k.pairGap == .9f, "Goblin Kalkanlı values");
            Attack(EnemyRole.GoblinKalkanli, "thrust", 12, false);
            Assert(k.Find("thrust").reach == 2.2f && k.Find("bash").pushBack > 0 && k.Find("bash").staminaDrain > 0 && !k.Find("bash").red, "Kalkanlı thrust 2.2 and a Spearman-style bash");

            var kaba = EnemyProfile.For(EnemyRole.Kaba);
            Assert(kaba.health == 130 && kaba.guard == 100 && kaba.speed == 1f && kaba.size == 1f && kaba.bulk == 2f && kaba.poise && kaba.poiseAlways && kaba.keepMin == 0, "Kaba values (never retreats: keepMin 0)");
            Attack(EnemyRole.Kaba, "cleaver", 24, false); Attack(EnemyRole.Kaba, "chainsweep", 22, true); Attack(EnemyRole.Kaba, "grab", 26, true);
            Assert(kaba.Find("cleaver").heavy && kaba.Find("cleaver").blockDrain == 35 && kaba.Find("chainsweep").reach == 3.2f && kaba.Find("grab").reach == 1.4f && kaba.Find("grab").grab && kaba.Find("grab").heavy, "Kaba cleaver / chain sweep / grab shape");

            // Renk: beyaz saldırı blokta kalır, kırmızı geçer; hasar kadar iner
            Assert(Lost(root, EnemyRole.Bicakci, "stab", 1.8f, false, out _, out _) == 16, "Bıçakçı stab should deal 2 x 8");
            Assert(Lost(root, EnemyRole.Bicakci, "stab", 1.8f, true, out _, out _) == 0, "A white stab must be blocked");
            Assert(Lost(root, EnemyRole.Bicakci, "flankstab", .99f, false, out _, out _) == 8 && Lost(root, EnemyRole.Bicakci, "flankstab", .99f, true, out _, out _) == 0, "Flank stab: 8, white");
            Assert(Lost(root, EnemyRole.GoblinKalkanli, "thrust", 2f, false, out _, out _) == 12 && Lost(root, EnemyRole.GoblinKalkanli, "thrust", 2f, true, out _, out _) == 0, "Kalkanlı thrust: 12, white");
            Assert(Lost(root, EnemyRole.GoblinKalkanli, "bash", 1.1f, false, out _, out _) == 8 && Lost(root, EnemyRole.GoblinKalkanli, "bash", 1.1f, true, out _, out _) == 0, "Kalkanlı bash: 8, white");
            Assert(Lost(root, EnemyRole.Kaba, "cleaver", 1.7f, false, out _, out _) == 24 && Lost(root, EnemyRole.Kaba, "cleaver", 1.7f, true, out var cw, out _) == 0 && cw.player.stamina < 100, "Kaba cleaver: 24, white");
            Assert(Lost(root, EnemyRole.Kaba, "chainsweep", 2.9f, false, out _, out _) == 22 && Lost(root, EnemyRole.Kaba, "chainsweep", 2.9f, true, out _, out _) == 22, "Kaba chain sweep: 22, red (block does nothing)");
            Assert(Lost(root, EnemyRole.Kaba, "grab", 1.3f, false, out _, out _) == 26 && Lost(root, EnemyRole.Kaba, "grab", 1.3f, true, out _, out _) == 26, "Kaba grab: 26, red");
        }

        // Oyuncudan vuran: sendelemeyi ve infazı karıştırmadan düz hasar
        static void Hit(GameWorld w, Actor e, float damage = 20, bool heavy = false) => w.Damage(e, damage, 1, heavy, false, w.player);

        static void Knifeman(GameRoot root)
        {
            // İki hafif vuruş (20 + 20) öldürür; ilki 10 can bırakır
            var w = GoblinArena(root, 4, EnemyRole.Bicakci, 1f, out var e);
            Run(w, 1f, i => new InputFrame { light = i == 0 });
            Assert(e.health == 10, "One light hit should leave the Bıçakçı at 10, health " + e.health);
            Run(w, 1f, i => new InputFrame { light = i == 0 });
            Assert(e.Dead, "Two light hits must kill a Bıçakçı, health " + e.health);

            // Sıçrayarak gelen saplama: vuruştan önce oyuncunun ~1 birim yanına iner; ardından geri sıçrar
            w = GoblinArena(root, 4, EnemyRole.Bicakci, 1.8f, out e); var pl = w.player; var stab = EnemyProfile.For(EnemyRole.Bicakci).Find("stab");
            w.StartAttack(e, "stab"); Run(w, stab.windup - .01f);
            Assert(e.position.x - pl.position.x < 1.15f && e.position.x - pl.position.x > .85f && pl.health == pl.maxHealth, "Stab leap should land about 1 u away before the first hit: gap " + (e.position.x - pl.position.x));
            bool hopped = false; float widest = 0;
            Run(w, stab.Duration + .6f, i => { hopped |= e.attack?.name == "hop" && e.state == ActionState.Dodge; widest = Mathf.Max(widest, e.position.x - pl.position.x); return new InputFrame(); });
            Assert(hopped && widest > 2.2f, "The Bıçakçı must hop back right after its stab (hopped " + hopped + ", gap " + widest + ")");
            Assert(pl.health == pl.maxHealth - 16, "Stab total 16, health " + pl.health);

            // Savuşturulan Bıçakçı yere düşer: en az 1.2 sn, zincirdeki geri sıçrama yok
            w = GoblinArena(root, 4, EnemyRole.Bicakci, 1.8f, out e); pl = w.player;
            w.StartAttack(e, "stab"); int press = Mathf.RoundToInt((stab.windup - .06f) * 120);
            Run(w, stab.windup + .1f, i => new InputFrame { block = i >= press });
            Assert(e.state == ActionState.Hurt && e.duration >= 1.2f - 1e-4f && pl.health == pl.maxHealth, "A parried Bıçakçı should be knocked down >= 1.2 s: " + e.state + " " + e.duration);
            Run(w, 1.0f, i => new InputFrame { block = true });   // 0.1 + 1.0 = 1.1 sn: hâlâ yerde
            Assert(e.state == ActionState.Hurt && e.attack == null, "Parry knock-down ended early: " + e.state);
            Run(w, .15f, i => new InputFrame { block = true });
            Assert(e.state != ActionState.Hurt, "The knock-down should end after about 1.2 s: " + e.state);

            // Dolanma: oyuncunun arkasına, sağlam zemine iner ve tek saplama (8) vurur; 4 sn bekler; sonra geri sıçrar
            w = GoblinArena(root, 4, EnemyRole.Bicakci, 1.5f, out e); pl = w.player; var flank = EnemyProfile.For(EnemyRole.Bicakci).Find("flank");
            float start = w.clock;
            w.StartAttack(e, "flank"); Run(w, flank.Duration + .02f);
            Assert(e.position.x < pl.position.x && e.grounded && Mathf.Abs(e.position.y - w.zone.floor) < .01f && OnSurface(w.zone, e.position) && Mathf.Abs(pl.position.x - e.position.x - .9f) < .15f,
                "Flank must land behind the player, on firm ground, ~0.9 away: goblin " + e.position + " player " + pl.position);
            Assert(Mathf.Abs(e.attackReady["flank"] - (start + 4)) < .05f, "Flank cooldown should be 4 s");
            Run(w, 1f, i => new InputFrame { block = true });
            Assert(pl.health == pl.maxHealth - 8, "Flank stab from behind deals 8 (block does not cover the back), health " + pl.health);
            Assert(e.position.x < pl.position.x - 1.5f, "Hop after the flank stab, gap " + (pl.position.x - e.position.x));

            // Arkası çukursa önüne iner (Sisli Koru çukuru 11.8-14.4)
            w = GoblinArena(root, 4, EnemyRole.Bicakci, 1.5f, out e); pl = w.player;
            pl.position = new Vector2(14.9f, w.zone.floor); pl.view.ResetContacts(); e.position = new Vector2(16.4f, w.zone.floor); e.view.ResetContacts();
            w.StartAttack(e, "flank"); Run(w, flank.Duration + .02f);
            Assert(e.grounded && Mathf.Abs(e.position.y - w.zone.floor) < .01f && e.position.x > pl.position.x && OnSurface(w.zone, e.position), "Flank must not land in the pit: goblin " + e.position + " player " + pl.position);

            // Kendiliğinden: yakındaki oyuncuya sıçrayıp saplar ve dolanır
            w = GoblinArena(root, 4, EnemyRole.Bicakci, 1.6f, out e); e.intentTime = 0; w.player.maxHealth = w.player.health = 9999;
            var names = new HashSet<string>();
            Run(w, 20f, i => { if (e.attack != null && e.stateTime < 1f / 60) names.Add(e.attack.name); return new InputFrame(); });
            Assert(names.Contains("stab") && names.Contains("flank"), "A free Bıçakçı should stab and flank on its own, started: " + string.Join(",", names));
        }

        static void PoisonArrows(GameRoot root)
        {
            // Zehirli ok: 8 hasar, ardından 3 sn x 3; vuruşlar sendeletmez
            var w = GoblinArena(root, 4, EnemyRole.ZehirliOkcu, 7, out var e); var pl = w.player;
            w.StartAttack(e, "arrow");
            Run(w, 1.4f);
            Assert(pl.health == pl.maxHealth - 8 && pl.poisonTicks == 3, "Arrow: 8 damage and 3 poison ticks, health " + pl.health + " ticks " + pl.poisonTicks);
            root.hud.Tick(1f / 60);
            Color poisoned = root.hud.HealthTint;
            Assert(poisoned.g > poisoned.r + .15f && poisoned.g > .4f, "Health bar must turn green while poisoned: " + poisoned);
            bool hurt = false;
            Run(w, 3.4f, i => { hurt |= pl.state == ActionState.Hurt; return new InputFrame(); });
            Assert(pl.health == pl.maxHealth - 8 - 9 && pl.poisonTicks == 0, "Poison should total 8 + 9 = 17, health " + pl.health + " ticks " + pl.poisonTicks);
            Assert(!hurt, "Poison ticks must not cause Hurt");
            root.hud.Tick(1f / 60);
            Assert(root.hud.HealthTint.r > root.hud.HealthTint.g + .3f, "Health bar must return to red after the poison ends: " + root.hud.HealthTint);

            // Kalkan zehri engeller
            w = GoblinArena(root, 4, EnemyRole.ZehirliOkcu, 7, out e); pl = w.player;
            w.StartAttack(e, "arrow"); Run(w, 1.4f, i => new InputFrame { block = true });
            Assert(pl.health == pl.maxHealth && pl.poisonTicks == 0, "A blocked arrow must not poison, health " + pl.health);

            // Yenilenir, üst üste binmez: ikinci ok ilkinden ~1.5 sn sonra gelir -> 1 vuruş + yenilenen 3 vuruş = 4 x 3
            w = GoblinArena(root, 4, EnemyRole.ZehirliOkcu, 7, out e); pl = w.player; pl.maxHealth = pl.health = 200;
            int hits = 0; float last = pl.health; float firstHit = -1;
            w.StartAttack(e, "arrow");
            bool second = false;
            Run(w, 9f, i =>
            {
                if (pl.health < last && pl.state == ActionState.Hurt && pl.stateTime < .02f) { hits++; if (firstHit < 0) firstHit = w.clock; }
                last = pl.health;
                if (!second && firstHit > 0 && w.clock > firstHit + 1.0f) { second = true; w.StartAttack(e, "arrow"); }
                return new InputFrame();
            });
            Assert(hits == 2, "Setup: exactly two arrows should land, " + hits);
            Assert(pl.health == 200 - 16 - 12, "Re-poisoning refreshes instead of stacking: 16 + 4 ticks x 3 = 28, health " + pl.health);

            // Zehir ölümcül olabilir (ateş gibi): can bitince ölür
            w = GoblinArena(root, 4, EnemyRole.ZehirliOkcu, 7, out e); pl = w.player; pl.health = 12;
            w.StartAttack(e, "arrow"); Run(w, 4.5f);
            Assert(pl.Dead, "Poison should be able to finish a dying player, health " + pl.health);
            // Yol taşı zehri temizler
            w = GoblinArena(root, 4, EnemyRole.ZehirliOkcu, 7, out e); pl = w.player;
            w.StartAttack(e, "arrow"); Run(w, 1.4f);
            pl.position.x = 2; w.Simulate(1f / 120, new InputFrame { interact = true }, true);
            Assert(pl.poisonTicks == 0 && pl.health == pl.maxHealth, "A shrine cures poison");
        }

        static void ArcherBehaviour(GameRoot root)
        {
            // Ok yağmuru: işaretli yerde duran vurulur (12, kırmızı); üç ok ve bir işaret
            var w = GoblinArena(root, 4, EnemyRole.ZehirliOkcu, 7, out var e); var pl = w.player; var volley = EnemyProfile.For(EnemyRole.ZehirliOkcu).Find("volley");
            w.StartAttack(e, "volley"); Run(w, volley.windup + .05f);
            Assert(w.projectiles.FindAll(s => s.kind == GameWorld.ShotKind.VolleyMark).Count == 1 && w.projectiles.FindAll(s => s.kind == GameWorld.ShotKind.VolleyArrow).Count == 3, "Volley: one ground mark and three arrows");
            var mark = w.projectiles.Find(s => s.kind == GameWorld.ShotKind.VolleyMark);
            Assert(Mathf.Abs(mark.position.x - pl.position.x) < .01f, "The mark lies under the player");
            Run(w, 1.6f, i => new InputFrame { block = true });
            Assert(pl.health == pl.maxHealth - 12 && pl.poisonTicks == 0, "Volley (red) pierces block for 12, health " + pl.health);
            // İşaretten çıkan kurtulur
            w = GoblinArena(root, 4, EnemyRole.ZehirliOkcu, 7, out e); pl = w.player;
            w.StartAttack(e, "volley"); int leave = Mathf.RoundToInt((volley.windup + .1f) * 120);
            Run(w, 2f, i => new InputFrame { move = i >= leave && i < leave + 60 ? -1 : 0 });
            Assert(pl.health == pl.maxHealth, "Leaving the volley mark avoids the arrows, health " + pl.health);

            // Yaklaşan oyuncudan geri sıçrar (2.5 içinde)
            w = GoblinArena(root, 4, EnemyRole.ZehirliOkcu, 2.2f, out e); e.intentTime = 0;
            Run(w, .7f);
            Assert(e.position.x - w.player.position.x > 3.5f, "Okçu did not back away from a player within 2.5, gap " + (e.position.x - w.player.position.x));
            // Uzaktaki oyuncuya kendiliğinden ok atar ve zehirler
            w = GoblinArena(root, 4, EnemyRole.ZehirliOkcu, 7, out e); e.intentTime = 0;
            bool shot = false, poisoned = false;
            Run(w, 6f, i => { shot |= e.attack?.name == "arrow"; poisoned |= w.player.poisonTicks > 0; return new InputFrame(); });
            Assert(shot && poisoned, "A free Okçu should shoot and poison on its own (shot " + shot + ", poisoned " + poisoned + ")");
        }

        static void ShieldGoblin(GameRoot root)
        {
            // Önden hafif vuruş kalkana çarpar: can gitmez, denge düşer
            var w = GoblinArena(root, 4, EnemyRole.GoblinKalkanli, 1.3f, out var e);
            Hit(w, e);
            Assert(e.health == e.maxHealth && e.guard < e.maxGuard, "Frontal light hit should stop on the shield: health " + e.health + " guard " + e.guard);
            // Ağır vuruş geçer, dengeyi iki kat düşürür
            w = GoblinArena(root, 4, EnemyRole.GoblinKalkanli, 1.3f, out e);
            Hit(w, e, 20, true);
            Assert(e.health == e.maxHealth - 20 && Mathf.Approximately(e.guard, e.maxGuard - 40), "Heavy hit passes the shield and drains guard 2x: health " + e.health + " guard " + e.guard);
            // Arkadan vuruş: can gider, denge iki kat düşer
            w = GoblinArena(root, 4, EnemyRole.GoblinKalkanli, 1.3f, out e); e.face = 1;
            Hit(w, e);
            Assert(e.health == e.maxHealth - 20 && Mathf.Approximately(e.guard, e.maxGuard - 40), "Back hit should hurt and drain guard 2x: health " + e.health + " guard " + e.guard);
            // Dönüş gecikmesi: arkasına geçen oyuncuya geç döner (fırsat)
            w = GoblinArena(root, 4, EnemyRole.GoblinKalkanli, 1.3f, out e); e.face = -1;
            w.player.position.x = e.position.x + 1.5f;
            Run(w, .3f); Assert(e.face == -1, "Kalkanlı turned too fast");
            // Kalkanla itme blok tutan oyuncuyu iter ve dayanıklılığını düşürür
            w = GoblinArena(root, 4, EnemyRole.GoblinKalkanli, 1.1f, out e); var pl = w.player; float x0 = pl.position.x;
            w.StartAttack(e, "bash"); Run(w, 1f, i => new InputFrame { block = true });
            Assert(x0 - pl.position.x >= 1.2f && 100 - pl.stamina >= 20, "Bash should push a blocking player back and drain stamina: moved " + (x0 - pl.position.x) + ", stamina " + pl.stamina);
        }

        // İkili sıra: iki Kalkanlı birbirinin .9 yakınında tek hat olarak yürür; hiçbiri hattı bırakıp tek başına gitmez
        static void PairLine(GameRoot root)
        {
            var w = GoblinArena(root, 4, EnemyRole.GoblinKalkanli, 6, out var a);
            float floor = w.zone.floor; var pl = w.player;
            pl.position = new Vector2(16, floor); pl.view.ResetContacts();
            a.position = new Vector2(22, floor); a.view.ResetContacts();
            var b = w.Spawn(24.4f, floor, EnemyRole.GoblinKalkanli); b.face = -1; b.intentTime = 99; b.view.ResetContacts();
            float x0 = a.position.x;
            Run(w, .5f);
            Assert(Mathf.Abs(a.position.x - x0) < .06f && b.position.x < 24.4f - .4f, "The front Kalkanlı must wait for the rear one: front moved " + (a.position.x - x0) + ", rear " + (b.position.x - 24.4f));
            float hi = 0, lo = 99; bool facing = true;
            Run(w, 7f, i =>
            {
                float gap = b.position.x - a.position.x;
                if (w.clock > 3.5f) { lo = Mathf.Min(lo, gap); hi = Mathf.Max(hi, gap); }
                facing &= a.face == -1 && b.face == -1;
                return new InputFrame();
            });
            Assert(lo >= .6f && hi <= 1.2f, "The pair line must hold 0.9 +- 0.3, saw " + lo + " .. " + hi);
            Assert(facing, "Both face the player");
            Assert(a.position.x - pl.position.x < 2.3f && a.position.x - pl.position.x > 1.1f, "The line advances to the player: front gap " + (a.position.x - pl.position.x));
            Assert(a.state != ActionState.Light && b.state != ActionState.Light, "Setup: nobody attacked");

            // Eş çok uzaktaysa (> 3) hat yok: ikisi de kendi başına yürür
            w = GoblinArena(root, 4, EnemyRole.GoblinKalkanli, 6, out a); pl = w.player;
            pl.position = new Vector2(16, floor); pl.view.ResetContacts(); a.position = new Vector2(19.5f, floor); a.view.ResetContacts();
            b = w.Spawn(23.5f, floor, EnemyRole.GoblinKalkanli); b.face = -1; b.intentTime = 99; b.view.ResetContacts();
            x0 = a.position.x; float bx0 = b.position.x; Run(w, 1f);
            Assert(x0 - a.position.x > .9f && bx0 - b.position.x > .9f, "Two Kalkanlı more than 3 apart act alone: moved " + (x0 - a.position.x) + " / " + (bx0 - b.position.x));
            // Eş ölünce kalan olağan davranışa döner
            w = GoblinArena(root, 4, EnemyRole.GoblinKalkanli, 6, out a); pl = w.player;
            pl.position = new Vector2(16, floor); pl.view.ResetContacts(); a.position = new Vector2(22, floor); a.view.ResetContacts();
            b = w.Spawn(24.4f, floor, EnemyRole.GoblinKalkanli); b.face = -1; b.intentTime = 99; b.view.ResetContacts();
            w.Damage(b, 5000, 1, true, true); x0 = a.position.x; Run(w, 1f);
            Assert(x0 - a.position.x > .9f, "A lone Kalkanlı must advance normally, moved " + (x0 - a.position.x));
        }

        static void Brute(GameRoot root)
        {
            // Duruş: hafif vuruş sendeletmez (bekleyen Kaba'yı da), yalnız denge kırılınca sendeler
            var w = GoblinArena(root, 4, EnemyRole.Kaba, 1f, out var e);
            for (int i = 0; i < 3; i++) { Hit(w, e); Run(w, .2f); Assert(e.state != ActionState.Hurt, "A light hit staggered the Kaba (hit " + (i + 1) + ")"); }
            Assert(e.health == e.maxHealth - 60 && e.guard < e.maxGuard, "Lights hurt but do not stagger: health " + e.health + " guard " + e.guard);
            w = GoblinArena(root, 4, EnemyRole.Kaba, 1f, out e);
            Run(w, 1f, i => new InputFrame { light = i == 0 });
            Assert(e.health < e.maxHealth && e.state != ActionState.Hurt, "A real sword swing must not stagger the Kaba: health " + e.health + " " + e.state);
            w = GoblinArena(root, 4, EnemyRole.Kaba, 1f, out e); e.guard = 10;
            Hit(w, e);
            Assert(e.state == ActionState.Hurt && e.duration >= .9f - 1e-4f, "Guard break must stagger the Kaba: " + e.state + " " + e.duration);

            // Geri çekilmez: oyuncu dibinde; Bıçakçı (keepMin 1.2) geri çekilir, Kaba yerinde kalır
            w = GoblinArena(root, 4, EnemyRole.Kaba, .5f, out e); float kx = e.position.x; Run(w, 3f);
            Assert(Mathf.Abs(e.position.x - kx) < .03f, "The Kaba must never retreat, moved " + (e.position.x - kx));
            w = GoblinArena(root, 4, EnemyRole.Bicakci, .5f, out e); kx = e.position.x; Run(w, 1f);
            Assert(e.position.x - kx > .5f, "Control: a Bıçakçı steps back from a player at .5, moved " + (e.position.x - kx));

            // Satır: blok eden oyuncunun dayanıklılığından tam 35 düşer (genel ağır kuralı 65 idi)
            w = GoblinArena(root, 4, EnemyRole.Kaba, 1.7f, out e); var pl = w.player; var cleaver = EnemyProfile.For(EnemyRole.Kaba).Find("cleaver");
            w.StartAttack(e, "cleaver"); float min = 100;
            Run(w, cleaver.windup + cleaver.active + .1f, i => { min = Mathf.Min(min, pl.stamina); return new InputFrame { block = true }; });
            Assert(pl.health == pl.maxHealth && Mathf.Abs(min - 65) < 1f, "A blocked cleaver drains 35 stamina (to 65), lowest " + min + ", health " + pl.health);
            // Satır savuşturulabilir (beyaz)
            w = GoblinArena(root, 4, EnemyRole.Kaba, 1.7f, out e); pl = w.player;
            w.StartAttack(e, "cleaver"); int press = Mathf.RoundToInt((cleaver.windup - .06f) * 120);
            Run(w, cleaver.windup + .1f, i => new InputFrame { block = i >= press });
            Assert(e.state == ActionState.Hurt && pl.health == pl.maxHealth, "The cleaver can be parried: " + e.state);

            // Yakalama kapısı (Kara Murad gibi): oyuncu saldırırken ya da dokunulmazken hiç başlamaz; boşta duranı yakalar (26)
            w = GoblinArena(root, 4, EnemyRole.Kaba, 1f, out e); pl = w.player; pl.maxHealth = pl.health = 9999; e.intentTime = 0;
            e.attackReady["cleaver"] = e.attackReady["chainsweep"] = 999;
            bool grabbed = false;
            for (int i = 0; i < 480; i++)
            {
                w.Simulate(1f / 120, new InputFrame { light = i % 30 == 0 }, true);
                if (e.attack != null && e.attack.grab && (pl.Attacking || pl.invincible > 0) && e.stateTime < 1f / 60) grabbed = true;
            }
            Assert(!grabbed, "Grab must never start while the player attacks or is invulnerable");
            w = GoblinArena(root, 4, EnemyRole.Kaba, 1f, out e); pl = w.player; e.intentTime = 0; e.attackReady["cleaver"] = e.attackReady["chainsweep"] = 999;
            Run(w, .6f, i => { pl.invincible = 1; return new InputFrame(); });
            Assert(e.attack == null, "Grab must not start on an invulnerable player, started " + e.attack?.name);
            pl.invincible = 0;
            string first = null;
            for (int i = 0; i < 90 && first == null; i++) { w.Simulate(1f / 120, new InputFrame(), true); first = e.attack?.name; }
            Assert(first == "grab", "Kaba should grab an idle player in reach, started " + (first ?? "nothing"));
            float hp = pl.health;
            Run(w, EnemyProfile.For(EnemyRole.Kaba).Find("grab").Duration + .1f, i => new InputFrame { block = true });
            Assert(hp - pl.health == 26, "The grab takes 26 (red), took " + (hp - pl.health));

            // Kendiliğinden: yaklaşır ve saldırılarından en az ikisini kullanır
            w = GoblinArena(root, 4, EnemyRole.Kaba, 5, out e); e.intentTime = 0; w.player.maxHealth = w.player.health = 9999;
            var used = new HashSet<string>();
            Run(w, 30f, i => { if (e.attack != null && e.stateTime < 1f / 60) used.Add(e.attack.name); return new InputFrame(); });
            Assert(used.Count >= 2, "A free Kaba should use at least two of its attacks, used " + string.Join(",", used));
        }

        // §2.4: oyuncunun ağır vuruşu kılıcın değdiği her goblini vurur; sıradan goblinler yere serilir (1.5 sn)
        static void HeavyKnockdown(GameRoot root)
        {
            float[] gaps = { .9f, 1.2f, 1.5f };
            var w = GoblinArena(root, 4, EnemyRole.Bicakci, gaps[0], out var e1);
            var e2 = w.Spawn(4 + gaps[1], w.zone.floor, EnemyRole.Bicakci); var e3 = w.Spawn(4 + gaps[2], w.zone.floor, EnemyRole.Bicakci);
            foreach (var g in new[] { e2, e3 }) { g.face = -1; g.intentTime = 99; g.view.ResetContacts(); }
            Run(w, 1f, i => new InputFrame { heavy = i == 0 });
            Assert(e1.Dead && e2.Dead && e3.Dead, "One heavy hit must hit all three Bıçakçı: " + e1.health + "/" + e2.health + "/" + e3.health);

            // Dayanıklı hâli: üçü de 44 alır ve 1.5 sn yere serilir
            w = GoblinArena(root, 4, EnemyRole.Bicakci, gaps[0], out e1);
            e2 = w.Spawn(4 + gaps[1], w.zone.floor, EnemyRole.Bicakci); e3 = w.Spawn(4 + gaps[2], w.zone.floor, EnemyRole.Bicakci);
            foreach (var g in new[] { e1, e2, e3 }) { g.face = -1; g.intentTime = 99; g.health = g.maxHealth = 300; g.view.ResetContacts(); }
            Run(w, .9f, i => new InputFrame { heavy = i == 0 });
            foreach (var g in new[] { e1, e2, e3 })
                Assert(g.health == 256 && g.state == ActionState.Hurt && Mathf.Abs(g.duration - 1.5f) < 1e-3f, "A heavy hit must knock a goblin down for 1.5 s: health " + g.health + " " + g.state + " " + g.duration);

            // Hafif vuruş 1.5 sn yere sermez
            w = GoblinArena(root, 4, EnemyRole.Bicakci, 1f, out e1); e1.health = e1.maxHealth = 300; e1.guard = e1.maxGuard = 9999;
            Hit(w, e1);
            Assert(e1.duration < 1f, "A light hit must not knock the goblin down for 1.5 s, duration " + e1.duration);

            // Sıradan goblin listesi: Bıçakçı, Okçu, Pusucu, Çömlekçi yere serilir; Kalkanlı / Kaba / insan serilmez
            foreach (var role in new[] { EnemyRole.Bicakci, EnemyRole.ZehirliOkcu, EnemyRole.Pusucu, EnemyRole.Comlekci, EnemyRole.GoblinKalkanli, EnemyRole.Kaba, EnemyRole.Nefer })
            {
                w = GoblinArena(root, 4, role, 1.2f, out e1); e1.health = e1.maxHealth = 999; e1.guard = e1.maxGuard = 9999; e1.face = 1;   // arkadan: kalkan sayılmaz
                Hit(w, e1, 44, true);
                bool ordinary = role is EnemyRole.Bicakci or EnemyRole.ZehirliOkcu or EnemyRole.Pusucu or EnemyRole.Comlekci;
                Assert((e1.state == ActionState.Hurt && Mathf.Abs(e1.duration - 1.5f) < 1e-3f) == ordinary, role + " heavy knock-down expectation " + ordinary + ", got " + e1.state + " " + e1.duration);
            }
        }
    }
}
