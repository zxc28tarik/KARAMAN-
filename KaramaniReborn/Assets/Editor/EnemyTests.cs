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
    }
}
