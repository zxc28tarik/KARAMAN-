using System;
using UnityEngine;

namespace KaramaniReborn
{
    public enum ActionState { Idle, Move, Jump, Fall, Light, Heavy, Block, Parry, Dodge, Hurt, Dead, Heal, Spirit, Shoot, Hang, Climb }
    public enum EnemyRole
    {
        Nefer, Spearman, Halberdier, Musketeer, Archer, Sorcerer, Captain, Arslan, KorDemir, Kurtbas, KaraMurad,
        // 2. kısım: Çatlak Kaya oymağının goblinleri (Docs/specs/2026-10-12-goblin-part2-design.md)
        Bicakci, ZehirliOkcu, Pusucu, GoblinKalkanli, Comlekci, Kaba, Kircil, Catal, KaraDis
    }

    public struct InputFrame
    {
        public float move;
        public bool jump, jumpHeld, up, down, slow, light, heavy, block, dodge, heal, spirit, shoot, interact;
        public void Accumulate(InputFrame newer)
        {
            move = newer.move; jumpHeld = newer.jumpHeld; up = newer.up; down = newer.down; slow = newer.slow; block = newer.block;
            jump |= newer.jump; light |= newer.light; heavy |= newer.heavy; dodge |= newer.dodge;
            heal |= newer.heal; spirit |= newer.spirit; shoot |= newer.shoot; interact |= newer.interact;
        }
        public void ConsumeButtons() { jump = light = heavy = dodge = heal = spirit = shoot = interact = false; }
        public static InputFrame Read() => new InputFrame
        {
            move = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0),
            jump = Input.GetKeyDown(KeyCode.Space), jumpHeld = Input.GetKey(KeyCode.Space), up = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow),
            down = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow), slow = Input.GetKey(KeyCode.LeftControl),
            light = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J), heavy = Input.GetKeyDown(KeyCode.F), block = Input.GetMouseButton(1) || Input.GetKey(KeyCode.K),
            dodge = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift), heal = Input.GetKeyDown(KeyCode.H), spirit = Input.GetKeyDown(KeyCode.Q),
            shoot = Input.GetKeyDown(KeyCode.R), interact = Input.GetKeyDown(KeyCode.E)
        };
    }

    public sealed class Actor
    {
        public int id;
        public bool player, boss, grounded = true;
        // Pusu noktası: bölümün ZoneDefinition.perches dizisindeki sırası (-1 = bağlı değil). Davranış ayrı görevde.
        public int perch = -1;
        public EnemyRole role;
        public Vector2 position, velocity;
        public float health = 100, maxHealth = 100, stamina = 100, guard = 55, maxGuard = 55;
        public int face = 1, combo = 1, potions = 3, ammo = 8;
        public ActionState state;
        public float stateTime, duration, cooldown, invincible, parryWindow, dropTimer, skillCooldown, shotCooldown, hitFlash, intentTime;
        public bool attackConnected, bufferedLight, observedBlock, telegraphed, pooled, enGarde;
        // Prince of Persia tarzı hareket: kayarak durma süresi, tutunulan kenar, tırmanışın başladığı yer
        public float skid, grabCooldown;
        // Türün klasiklerinden: geri adım (SotN), havada ikinci zıplama ve takla, savuşturma sonrası infaz
        public bool backdash, riposting;
        public int airJumps, riposteTarget = -1;
        public float airFlip, riposte;
        public Vector2 ledge, climbFrom;
        public float jumpBuffer, coyote, staminaDelay;
        public readonly System.Collections.Generic.HashSet<int> hitTargets = new();
        public float knockback, speed, size = 1;
        // Kalkanla itme: kalan süre ve o süredeki yatay hız (blokta da uygulanır)
        public float pushTime, pushSpeed;
        public PixelCharacter view;
        // Düşman saldırısı: yürüyen saldırının tanımı, kaçıncı vuruşta olduğu, evre ve türlere özel sayaçlar
        public EnemyAttack attack;
        public int strike = -1, phase = 1;
        public float turnTimer;
        public float home;   // doğduğu x (boss arenasının merkezi)
        public readonly System.Collections.Generic.Dictionary<string, float> attackReady = new();
        public float Progress => duration > 0 ? Mathf.Clamp01(stateTime / duration) : 0;
        public bool Dead => health <= 0 || state == ActionState.Dead;
        public bool Attacking => state is ActionState.Light or ActionState.Heavy or ActionState.Spirit;
        public bool Locked => state is ActionState.Light or ActionState.Heavy or ActionState.Dodge or ActionState.Hurt or ActionState.Heal or ActionState.Spirit or ActionState.Shoot or ActionState.Dead or ActionState.Hang or ActionState.Climb;
        public void Enter(ActionState next, float seconds = 0) { state = next; stateTime = 0; duration = seconds; attackConnected = false; telegraphed = false; backdash = false; riposting = false; hitTargets.Clear(); attack = null; strike = -1; }
    }

    public struct Surface
    {
        public float left, right, y, thickness;
        public bool oneWay, wood;
        public Surface(float l, float r, float height, float thick = .6f, bool platform = false, bool wooden = false)
        { left = l; right = r; y = height; thickness = thick; oneWay = platform; wood = wooden; }
    }

    public sealed class ZoneDefinition
    {
        public const int Count = 7;
        public int index;
        public string title, subtitle, objective, bossName;
        // Bölüme girişte bir kez gösterilen kısım başlığı (boşsa yok)
        public string banner = "";
        public Color ambient, fog, accent;
        public bool snow;
        public float length = 57, floor = -2.25f;
        public Surface[] surfaces;
        // Pusu noktaları (dünya konumu): zeminden 2.2-3.2 yukarıda, yürünebilir zeminin üstünde, boşluğun üstünde değil
        public Vector2[] perches = Array.Empty<Vector2>();

        // Bölge sanatı gelene kadar 2. kısım mevcut arka plan resimlerini ve renk ayarını ödünç alır:
        // 4 -> 2 (sisli yeşil-mavi), 5 -> 3 (soğuk mağara), 6 -> 1 (sıcak kül). Resimlere dokunulmaz.
        public static int ArtIndexFor(int index) => index switch { 4 => 2, 5 => 3, 6 => 1, _ => index };
        public int artIndex => ArtIndexFor(index);

        public static ZoneDefinition Create(int index)
        {
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index), "Bölge 0-6 arasında olmalı");
            string[] names = { "KIZIL SURLAR", "YANMIŞ MAHALLE", "KURT GEÇİDİ", "KARANLIK KALE", "SİSLİ KORU", "DERİN MAĞARALAR", "KEMİK TAHTI" };
            string[] sub = { "Gün batımında sınır", "Küllerin arasındaki iz", "Ayın altında uyananlar", "Son nöbet", "Sınırın ötesindeki orman", "Çatlak Kaya'nın inleri", "Eski maden galerileri" };
            string[] bosses = { "Bayraktar Arslan", "Humbaracıbaşı Kör Demir", "Deli Kurtbaş", "Yeniçeri Ağası Kara Murad", "", "Kan Şamanı Kırçıl", "Oymak Beyi Kara Diş" };
            return new ZoneDefinition {
                index = index, title = names[index], subtitle = sub[index], bossName = bosses[index], objective = index == 4 ? "Ormanı geç. İç kapıya ulaş." : "İç kapıya ulaş. Nöbeti kır.",
                banner = index == 4 ? "İkinci Kısım · Çatlak Kaya".ToUpper(System.Globalization.CultureInfo.GetCultureInfo("tr-TR")) : "",
                // 4: yeşil-gri sis, 5: soğuk koyu mavi-gri mağara, 6: sıcak meşaleli koyu sarı
                ambient = index switch { 0 => new Color(1, .92f, .83f), 3 => new Color(.85f, .93f, 1), 4 => new Color(.80f, .90f, .84f), 5 => new Color(.66f, .74f, .88f), 6 => new Color(1, .86f, .62f), _ => new Color(.84f, .88f, 1) },
                fog = index switch { 0 => new Color(.21f, .23f, .28f), 4 => new Color(.19f, .27f, .24f), 5 => new Color(.11f, .15f, .22f), 6 => new Color(.26f, .19f, .11f), _ => new Color(.23f, .29f, .38f) },
                accent = index switch { 0 => new Color(1, .65f, .27f), 4 => new Color(.58f, .78f, .56f), 5 => new Color(.50f, .68f, .86f), 6 => new Color(.95f, .68f, .26f), _ => new Color(.88f, .73f, .45f) },
                snow = index == 3,
                surfaces = SurfacesFor(index), perches = PerchesFor(index)
            };
        }

        // Her yeni bölgenin zemin bandı aynı (uzunluk 57, zemin -2.25); boşlukların hiçbiri 2.8'den geniş değil
        static Surface[] SurfacesFor(int index)
        {
            switch (index)
            {
                case 4:   // Sisli Koru: devrik kütük ve dal platformları, üç zemin parçası
                    return new[] {
                        new Surface(-8, 11.8f, -2.25f, 3.4f), new Surface(14.4f, 27.5f, -2.25f, 3.4f), new Surface(30, 61, -2.25f, 3.4f),
                        new Surface(6.5f, 9.8f, -.85f, .5f, true, true), new Surface(16.5f, 20.2f, -.8f, .5f, true, true),
                        new Surface(22.4f, 25.8f, -.45f, .5f, true, true), new Surface(33.6f, 37.2f, -.8f, .5f, true, true),
                        new Surface(41.5f, 45.5f, -.5f, .5f, true, true), new Surface(47.8f, 51, -.85f, .5f, true, true)
                    };
                case 5:   // Derin Mağaralar: dört zemin parçası, kaya çıkıntıları
                    return new[] {
                        new Surface(-8, 10.6f, -2.25f, 3.4f), new Surface(13.2f, 25.4f, -2.25f, 3.4f), new Surface(28, 40.6f, -2.25f, 3.4f), new Surface(43.2f, 61, -2.25f, 3.4f),
                        new Surface(4.6f, 7.4f, -.8f, .5f, true), new Surface(15, 18.3f, -.85f, .5f, true, true),
                        new Surface(19.8f, 23, -.35f, .5f, true), new Surface(30.4f, 33.6f, -.75f, .5f, true, true),
                        new Surface(35.8f, 38.8f, -.4f, .5f, true), new Surface(45.6f, 49, -.8f, .5f, true, true)
                    };
                case 6:   // Kemik Tahtı: galeri iskeleleri, boss arenasında tek ahşap sıçrama platformu
                    return new[] {
                        new Surface(-8, 11, -2.25f, 3.4f), new Surface(13.6f, 26.3f, -2.25f, 3.4f), new Surface(28.9f, 42.1f, -2.25f, 3.4f), new Surface(44.6f, 61, -2.25f, 3.4f),
                        new Surface(5.6f, 9, -.8f, .5f, true, true), new Surface(15.2f, 18.6f, -.7f, .5f, true, true),
                        new Surface(20.8f, 24.2f, -.3f, .5f, true, true), new Surface(30.5f, 34, -.8f, .5f, true, true),
                        new Surface(36.8f, 40.2f, -.45f, .5f, true, true), new Surface(47.4f, 50.2f, -.8f, .5f, true, true)
                    };
                default:
                    return new[] {
                        new Surface(-8, 13.5f, -2.25f, 3.4f), new Surface(15.7f, 29.5f, -2.25f, 3.4f), new Surface(31.5f, 61, -2.25f, 3.4f),
                        new Surface(7, 10.6f, -.9f, .5f, true, true), new Surface(12.4f, 17.1f, -.8f, .55f, true, true),
                        new Surface(23.5f, 26.6f, -.75f, .5f, true), new Surface(28.2f, 33, -.45f, .5f, true, true),
                        new Surface(37.4f, 40.7f, -.85f, .5f, true), new Surface(43, 47.5f, -.75f, .5f, true, true)
                    };
            }
        }

        // Pusu noktaları (Docs/specs/2026-10-12-goblin-part2-design.md §2.3): zeminden 2.2-3.2 yukarıda
        static Vector2[] PerchesFor(int index) => index switch
        {
            4 => new[] { new Vector2(10.9f, .55f), new Vector2(31.6f, .75f), new Vector2(53, .5f) },   // dal, devrik kütük üstü, ağaç
            5 => new[] { new Vector2(9.6f, .6f), new Vector2(24.5f, .75f), new Vector2(29, .5f) },     // tavan girintisi, boşluğun iki yanı
            6 => new[] { new Vector2(45.3f, .8f), new Vector2(54, .9f) },                               // galeri kirişi, arenanın iki ucu
            _ => Array.Empty<Vector2>(),
        };
    }

    public static class KinematicMotor
    {
        public static void Step(Actor actor, ZoneDefinition zone, float dt, bool cutJump = false)
        {
            Vector2 old = actor.position;
            if (!actor.grounded) actor.velocity.y -= (cutJump && actor.velocity.y > 0 ? 31 : 20) * dt;
            actor.position += actor.velocity * dt;
            actor.position.x = Mathf.Clamp(actor.position.x, -5, zone.length + 1.6f);
            bool standing = false; float top = float.NegativeInfinity;
            foreach (var s in zone.surfaces)
            {
                if (s.oneWay && actor.dropTimer > 0) continue;
                if (actor.position.x < s.left - .08f || actor.position.x > s.right + .08f) continue;
                if (actor.velocity.y <= 0 && old.y >= s.y - .055f && actor.position.y <= s.y + .04f && s.y > top) { top = s.y; standing = true; }
            }
            actor.grounded = standing;
            if (standing) { actor.position.y = top; actor.velocity.y = 0; }
        }
    }
}
