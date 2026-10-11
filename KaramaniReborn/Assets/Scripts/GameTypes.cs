using System;
using UnityEngine;

namespace KaramaniReborn
{
    public enum ActionState { Idle, Move, Jump, Fall, Light, Heavy, Block, Parry, Dodge, Hurt, Dead, Heal, Spirit, Shoot, Hang, Climb }
    public enum EnemyRole { Duelist, Shield, Archer, Captain, Ghoul }

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
        public PixelCharacter view;
        public float Progress => duration > 0 ? Mathf.Clamp01(stateTime / duration) : 0;
        public bool Dead => health <= 0 || state == ActionState.Dead;
        public bool Attacking => state is ActionState.Light or ActionState.Heavy or ActionState.Spirit;
        public bool Locked => state is ActionState.Light or ActionState.Heavy or ActionState.Dodge or ActionState.Hurt or ActionState.Heal or ActionState.Spirit or ActionState.Shoot or ActionState.Dead or ActionState.Hang or ActionState.Climb;
        public void Enter(ActionState next, float seconds = 0) { state = next; stateTime = 0; duration = seconds; attackConnected = false; telegraphed = false; backdash = false; riposting = false; hitTargets.Clear(); }
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
        public int index;
        public string title, subtitle, objective, bossName;
        public Color ambient, fog, accent;
        public bool snow;
        public float length = 57, floor = -2.25f;
        public Surface[] surfaces;

        public static ZoneDefinition Create(int index)
        {
            string[] names = { "KIZIL SURLAR", "YANMIŞ MAHALLE", "KURT GEÇİDİ", "KARANLIK KALE" };
            string[] sub = { "Gün batımında sınır", "Küllerin arasındaki iz", "Ayın altında uyananlar", "Son nöbet" };
            string[] bosses = { "Yeniçeri Kaptanı", "Kül Muhafızı", "Geçidin Bekçisi", "Kale Komutanı" };
            return new ZoneDefinition {
                index = index, title = names[index], subtitle = sub[index], bossName = bosses[index], objective = "İç kapıya ulaş. Nöbeti kır.",
                ambient = index == 0 ? new Color(1, .92f, .83f) : index == 3 ? new Color(.85f, .93f, 1) : new Color(.84f, .88f, 1),
                fog = index == 0 ? new Color(.21f, .23f, .28f) : new Color(.23f, .29f, .38f), accent = index == 0 ? new Color(1, .65f, .27f) : new Color(.88f, .73f, .45f), snow = index == 3,
                surfaces = new[] {
                    new Surface(-8, 13.5f, -2.25f, 3.4f), new Surface(15.7f, 29.5f, -2.25f, 3.4f), new Surface(31.5f, 61, -2.25f, 3.4f),
                    new Surface(7, 10.6f, -.9f, .5f, true, true), new Surface(12.4f, 17.1f, -.8f, .55f, true, true),
                    new Surface(23.5f, 26.6f, -.75f, .5f, true), new Surface(28.2f, 33, -.45f, .5f, true, true),
                    new Surface(37.4f, 40.7f, -.85f, .5f, true), new Surface(43, 47.5f, -.75f, .5f, true, true)
                }
            };
        }
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
