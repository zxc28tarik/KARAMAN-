using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace KaramaniReborn
{
    // The paintings are rigid pieces: only translations and rotations change during animation.
    // The feet lock in WORLD space during stance; a two-link IK solver keeps bone lengths constant.
    public sealed class PaintedRig
    {
        const float ThighLength = .57f, ShinLength = .53f, ArmLength = .34f, ForearmLength = .35f, Ankle = .184f;
        public readonly Transform root;
        readonly SpriteRenderer[] parts = new SpriteRenderer[16];
        readonly float[] scales = new float[16];
        readonly Foot[] feet = { new Foot(), new Foot() };
        readonly SpriteRenderer shadow;
        readonly SpriteRenderer equipment;
        float gait, previousX, movementBlend, previousSpeed;
        int lastFace;
        bool first = true;
        Vector2 nearHand, farHand, nearFoot, farFoot;
        float swordAngle;
        public float MaxBoneError { get; private set; }
        public float MaxFootSlip { get; private set; }
        public int PlantedSamples { get; private set; }
        public Vector2 BladeTip => parts[(int)PartId.Saber].transform.TransformPoint(new Vector3(parts[(int)PartId.Saber].sprite.bounds.max.x, 0, 0));
        public Vector2 BladeRoot => root.TransformPoint(nearHand);
        public bool PaintScalePreserved
        {
            get { for (int i = 0; i < 16; i++) if (Mathf.Abs(parts[i].transform.localScale.x - scales[i]) > .00001f || Mathf.Abs(parts[i].transform.localScale.y - scales[i]) > .00001f) return false; return true; }
        }

        sealed class Foot { public bool planted; public Vector2 world; public float x; public float angle; }

        public PaintedRig(Actor actor, Transform parent)
        {
            var go = new GameObject(actor.player ? "Karamani · Painted Rig" : actor.role + " · Painted Rig");
            root = go.transform; root.SetParent(parent, false);
            var group = go.AddComponent<SortingGroup>(); group.sortingOrder = actor.player ? 142 : 140;
            var art = ArtLibrary.Rig(!actor.player);
            int[] order = { 139, 132, 131, 121, 136, 138, 125, 127, 130, 131, 123, 124, 134, 126, 140, 142 };
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = ArtLibrary.Draw(((PartId)i).ToString(), art[i], root, Vector2.zero, order[i]);
                float height = i switch { 0 => .70f, 1 => .78f, 2 => .51f, 3 => 1.16f, 4 or 6 => ArmLength / .8f, 5 or 7 => ForearmLength / .8f, 8 or 10 => ThighLength / .8f, 9 or 11 => ShinLength / .8f, 12 or 13 => .23f, 15 => .78f, _ => .2f };
                float scale = i == 14 ? 1.16f / art[i].bounds.size.x : height / art[i].bounds.size.y;
                scales[i] = scale; parts[i].transform.localScale = new Vector3(scale, scale, 1);
            }
            shadow = ArtLibrary.Draw("Contact shadow", ArtLibrary.Glow, parent, actor.position, 59, new Color(0, 0, 0, .75f));
            shadow.transform.localScale = new Vector3(1.5f * actor.size, .2f, 1);
            equipment = ArtLibrary.Draw("Held equipment", ArtLibrary.Gear()[0], root, Vector2.zero, 143);
            actor.rig = this;
            Render(actor, 0, 0);
        }

        public void Destroy()
        {
            if (Application.isPlaying) { UnityEngine.Object.Destroy(root.gameObject); UnityEngine.Object.Destroy(shadow.gameObject); }
            else { UnityEngine.Object.DestroyImmediate(root.gameObject); UnityEngine.Object.DestroyImmediate(shadow.gameObject); }
        }

        public static Vector2 SolveJoint(Vector2 start, Vector2 end, float upper, float lower, int bend)
        {
            Vector2 delta = end - start; float raw = delta.magnitude;
            float distance = Mathf.Clamp(raw, Mathf.Abs(upper - lower) + .0001f, upper + lower - .0001f);
            Vector2 axis = raw > .00001f ? delta / raw : Vector2.down;
            float along = (upper * upper - lower * lower + distance * distance) / (2 * distance);
            float across = Mathf.Sqrt(Mathf.Max(0, upper * upper - along * along));
            return start + axis * along + new Vector2(-axis.y, axis.x) * across * bend;
        }

        public void ResetContacts() { first = true; foreach (var foot in feet) foot.planted = false; }

        public void Render(Actor actor, float dt, float clock)
        {
            float size = actor.size;
            root.position = actor.position;
            root.localScale = new Vector3(actor.face * size, size, 1);
            root.localRotation = Quaternion.identity;
            if (first || lastFace != actor.face)
            {
                previousX = actor.position.x; lastFace = actor.face;
                for (int i = 0; i < 2; i++) { feet[i].x = i == 0 ? .17f : -.17f; feet[i].planted = true; feet[i].world = actor.position + new Vector2(feet[i].x * actor.face * size, Ankle * size); }
                first = false;
            }
            float dx = actor.position.x - previousX; previousX = actor.position.x;
            float speed = Mathf.Abs(actor.velocity.x / size);
            bool walking = actor.grounded && speed > .12f && !actor.Locked && actor.state != ActionState.Block;
            float run = Mathf.Clamp01((speed - 2.3f) / 2.0f);
            float stride = Mathf.Lerp(1.18f, 1.52f, run);
            if (walking) gait = Mathf.Repeat(gait + Mathf.Abs(dx) / (stride * size), 1);
            movementBlend = Mathf.MoveTowards(movementBlend, walking ? 1 : 0, dt * 9);
            float breath = Mathf.Sin(clock * 2.2f) * .009f;
            float hipY = Mathf.Lerp(1.215f + breath, 1.115f + Mathf.Cos(gait * Mathf.PI * 4) * .016f, movementBlend);
            float lean = walking ? Mathf.Lerp(-3f, -9f, run) : 0;
            float p = actor.Progress;
            Vector2 nearHip = new Vector2(.035f, hipY), farHip = new Vector2(-.035f, hipY + .012f);
            nearFoot = FootPose(actor, 0, gait, stride, walking, dt);
            farFoot = FootPose(actor, 1, Mathf.Repeat(gait + .5f, 1), stride, walking, dt);

            if (!actor.grounded)
            {
                feet[0].planted = feet[1].planted = false;
                float jump = Mathf.Clamp01(actor.velocity.y / 7);
                nearFoot = new Vector2(.29f, .37f + jump * .24f); farFoot = new Vector2(-.24f, .24f + jump * .22f);
                hipY = 1.08f; lean = -5;
            }
            if (actor.Attacking || actor.state == ActionState.Block || actor.state == ActionState.Parry)
            {
                hipY = 1.125f + .025f * Mathf.Sin(p * Mathf.PI);
                lean = actor.state == ActionState.Heavy ? -6 - Mathf.Sin(p * Mathf.PI) * 7 : -Mathf.Sin(p * Mathf.PI) * 6;
            }
            if (actor.state == ActionState.Hurt) { lean = 10 * Mathf.Sin(p * Mathf.PI); hipY -= .03f; }
            if (actor.state == ActionState.Spirit)
            { nearFoot = new Vector2(.42f, .26f); farFoot = new Vector2(-.36f, .13f); hipY = 1.00f; lean = -23; feet[0].planted = feet[1].planted = false; }
            if (actor.state == ActionState.Heal) { hipY = 1.02f; lean = 2; }
            if (actor.state is ActionState.Dodge or ActionState.Dead)
            {
                hipY = .76f; nearFoot = new Vector2(.19f, .35f); farFoot = new Vector2(-.16f, .32f);
                feet[0].planted = feet[1].planted = false;
            }

            // Lower the pelvis rather than lengthening the painted leg if the stance has spread.
            hipY = Mathf.Min(hipY, ReachableHip(nearFoot, .035f), ReachableHip(farFoot, -.035f));
            nearHip.y = hipY; farHip.y = hipY + .012f;
            Leg(0, nearHip, nearFoot); Leg(1, farHip, farFoot);
            Set(PartId.Hips, new Vector2(0, hipY + .15f), lean * .25f);
            Set(PartId.Torso, new Vector2(0, hipY + .06f), lean);
            Vector2 shoulder = new Vector2(0, hipY + .06f) + Rotate(new Vector2(.075f, .56f), lean);
            Set(PartId.Head, new Vector2(0, hipY + .06f) + Rotate(new Vector2(.13f, .66f), lean), lean * .35f);
            float clothAngle = Mathf.Lerp(0, -24, movementBlend) + Mathf.Sin(clock * 4 + gait * Mathf.PI * 2) * (walking ? 8 : 3);
            Set(PartId.Cape, new Vector2(-.16f, hipY + .66f), clothAngle);

            nearHand = shoulder + new Vector2(.38f, -.36f);
            farHand = shoulder + new Vector2(-.18f, -.34f);
            swordAngle = 8;
            if (walking)
            {
                float arm = Mathf.Sin(gait * Mathf.PI * 2) * .11f;
                nearHand += new Vector2(-arm, -.05f); farHand += new Vector2(arm, -.04f); swordAngle = -9 + arm * 18;
            }
            if (!actor.grounded)
            {
                nearHand = shoulder + new Vector2(.30f, -.29f); farHand = shoulder + new Vector2(-.35f, .10f); swordAngle = 28;
            }
            if (actor.Attacking) AttackHands(actor, shoulder, p);
            if (actor.state is ActionState.Block or ActionState.Parry)
            {
                nearHand = shoulder + new Vector2(.26f, -.07f); farHand = shoulder + new Vector2(.34f, -.12f); swordAngle = 74;
            }
            if (actor.state == ActionState.Heal)
            { farHand = shoulder + new Vector2(.19f, .16f); nearHand = shoulder + new Vector2(.15f, -.40f); swordAngle = -35; }
            if (actor.state == ActionState.Shoot)
            { nearHand = shoulder + new Vector2(.52f, -.05f); farHand = shoulder + new Vector2(.32f, -.03f); swordAngle = 0; }
            if (actor.state is ActionState.Dodge or ActionState.Dead)
            { nearHand = shoulder + new Vector2(.17f, -.24f); farHand = shoulder + new Vector2(-.05f, -.22f); swordAngle = -30; }
            Arm(PartId.FarUpperArm, PartId.FarForearm, shoulder + new Vector2(-.11f, .01f), ref farHand, -1);
            Arm(PartId.UpperArm, PartId.Forearm, shoulder, ref nearHand, -1);
            Set(PartId.Saber, nearHand, swordAngle);
            Set(PartId.Shield, farHand + new Vector2(.035f, -.035f), -5);
            parts[(int)PartId.Shield].enabled = !actor.player && actor.role is EnemyRole.Shield or EnemyRole.Captain || actor.state is ActionState.Block or ActionState.Parry;
            bool bow = !actor.player && actor.role == EnemyRole.Archer;
            bool potion = actor.state == ActionState.Heal;
            bool gun = actor.player && actor.state == ActionState.Shoot;
            equipment.enabled = bow || potion || gun;
            parts[(int)PartId.Saber].enabled = !bow && !potion && !gun;
            if (equipment.enabled)
            {
                equipment.sprite = ArtLibrary.Gear()[potion ? 2 : bow ? 1 : 0];
                float scale = potion ? .28f / equipment.sprite.bounds.size.y : bow ? .92f / equipment.sprite.bounds.size.y : .62f / equipment.sprite.bounds.size.x;
                equipment.transform.localScale = new Vector3(scale, scale, 1);
                equipment.transform.localPosition = potion ? farHand + Vector2.down * .07f : nearHand;
                equipment.transform.localRotation = Quaternion.identity;
            }

            if (actor.state == ActionState.Dodge)
            {
                float turn = -360 * Smooth(p);
                RotateAroundBody(actor, turn, 1.03f);
            }
            if (actor.state == ActionState.Dead) RotateAroundBody(actor, -82 * Smooth(Mathf.Clamp01(actor.stateTime / .75f)), .43f);
            Color tint = actor.player ? Color.white : actor.role == EnemyRole.Ghoul ? new Color(.7f, .77f, .74f) : actor.boss ? new Color(1, .91f, .78f) : new Color(.94f, .93f, 1);
            if (actor.hitFlash > 0) tint = Color.Lerp(tint, new Color(1f, .45f, .28f), .6f);
            if (actor.Dead) tint = Color.Lerp(tint, new Color(.34f, .34f, .38f, .35f), Mathf.Clamp01(actor.stateTime / 3));
            foreach (var part in parts) part.color = tint;
            shadow.transform.position = new Vector3(actor.position.x, actor.position.y + .005f, 0);
            shadow.color = new Color(0, 0, 0, actor.grounded ? .7f : .22f);
            // Measure the visible boot pivots, not a reconstruction of the target equation.
            for (int i = 0; i < 2; i++) if (feet[i].planted)
            {
                Vector3 renderedAnkle = parts[(int)(i == 0 ? PartId.Boot : PartId.FarBoot)].transform.position;
                MaxFootSlip = Mathf.Max(MaxFootSlip, Mathf.Abs(renderedAnkle.x - feet[i].world.x));
                PlantedSamples++;
            }
            previousSpeed = speed;
        }

        Vector2 FootPose(Actor actor, int i, float phase, float stride, bool walking, float dt)
        {
            var foot = feet[i];
            float stance = .59f;
            float localX = stride * stance * .5f - stride * phase;
            if (walking && phase >= stance)
            {
                float t = (phase - stance) / (1 - stance);
                localX = Mathf.Lerp(-stride * stance * .5f, stride * stance * .5f, Smooth(t));
                foot.planted = false; foot.x = localX; foot.angle = Mathf.Sin(t * Mathf.PI) * 14;
                return new Vector2(localX, Ankle + Mathf.Sin(t * Mathf.PI) * .20f);
            }
            if (!foot.planted)
            {
                float desired = walking ? localX : i == 0 ? .17f : -.17f;
                if (!walking && Mathf.Abs(foot.x - desired) > .008f)
                {
                    foot.x = Mathf.MoveTowards(foot.x, desired, dt * 1.6f);
                    return new Vector2(foot.x, Ankle + .025f);
                }
                foot.world = actor.position + new Vector2(desired * actor.face * actor.size, Ankle * actor.size); foot.planted = true;
            }
            foot.x = (foot.world.x - actor.position.x) * actor.face / actor.size;
            if (Mathf.Abs(foot.x) > .67f)
            { foot.planted = false; return new Vector2(Mathf.Clamp(foot.x, -.60f, .60f), Ankle + .055f); }
            foot.angle = 0;
            Vector2 reconstructed = actor.position + new Vector2(foot.x * actor.face * actor.size, Ankle * actor.size);
            // Ignore vertical level changes; a new surface contact is established at landing.
            if (Mathf.Abs(foot.world.y - reconstructed.y) > .06f) foot.world.y = reconstructed.y;
            return new Vector2(foot.x, Ankle);
        }

        float ReachableHip(Vector2 foot, float x)
        {
            float reach = ThighLength + ShinLength - .025f;
            return foot.y + Mathf.Sqrt(Mathf.Max(.25f, reach * reach - Mathf.Pow(foot.x - x, 2)));
        }

        void Leg(int i, Vector2 hip, Vector2 ankle)
        {
            Vector2 knee = SolveJoint(hip, ankle, ThighLength, ShinLength, 1);
            Bone(i == 0 ? PartId.Thigh : PartId.FarThigh, hip, knee, ThighLength);
            Bone(i == 0 ? PartId.Shin : PartId.FarShin, knee, ankle, ShinLength);
            Set(i == 0 ? PartId.Boot : PartId.FarBoot, ankle, feet[i].angle);
        }

        void Arm(PartId upper, PartId lower, Vector2 shoulder, ref Vector2 hand, int bend)
        {
            Vector2 d = hand - shoulder; if (d.magnitude > ArmLength + ForearmLength - .012f) hand = shoulder + d.normalized * (ArmLength + ForearmLength - .012f);
            Vector2 elbow = SolveJoint(shoulder, hand, ArmLength, ForearmLength, bend);
            Bone(upper, shoulder, elbow, ArmLength); Bone(lower, elbow, hand, ForearmLength);
        }

        void AttackHands(Actor actor, Vector2 shoulder, float p)
        {
            float wind = actor.state == ActionState.Heavy ? .44f : .27f, strike = actor.state == ActionState.Heavy ? .69f : .58f;
            if (actor.state == ActionState.Spirit)
            { nearHand = shoulder + new Vector2(.61f, -.13f); farHand = shoulder + new Vector2(-.24f, -.15f); swordAngle = -5; return; }
            Vector2 ready = shoulder + new Vector2(.37f, -.34f);
            Vector2 raised = shoulder + new Vector2(-.12f, .43f);
            Vector2 follow = shoulder + new Vector2(.48f, -.23f);
            float from = 135, to = -32;
            if (actor.combo == 2 && actor.state == ActionState.Light)
            { raised = shoulder + new Vector2(.19f, -.46f); follow = shoulder + new Vector2(.49f, .18f); from = -58; to = 63; }
            if (actor.combo == 3 && actor.state == ActionState.Light)
            { raised = shoulder + new Vector2(-.17f, -.20f); follow = shoulder + new Vector2(.63f, -.05f); from = -5; to = 5; }
            if (p < wind)
            { float t = Smooth(p / wind); nearHand = Vector2.Lerp(ready, raised, t); swordAngle = Mathf.Lerp(8, from, t); }
            else if (p < strike)
            { float t = Smooth((p - wind) / (strike - wind)); nearHand = Vector2.Lerp(raised, follow, t); swordAngle = Mathf.Lerp(from, to, t); }
            else
            { float t = Smooth((p - strike) / (1 - strike)); nearHand = Vector2.Lerp(follow, ready, t); swordAngle = Mathf.Lerp(to, 8, t); }
            farHand = shoulder + new Vector2(-.17f, -.24f);
        }

        void RotateAroundBody(Actor actor, float degrees, float centerY)
        {
            root.localRotation = Quaternion.Euler(0, 0, degrees * actor.face);
            Vector3 offset = new Vector3(0, centerY * actor.size, 0);
            root.position = (Vector3)actor.position + offset - root.localRotation * offset;
        }

        void Bone(PartId id, Vector2 start, Vector2 end, float length)
        {
            Vector2 d = end - start;
            MaxBoneError = Mathf.Max(MaxBoneError, Mathf.Abs(d.magnitude - length));
            Set(id, start, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg + 90);
            var part = parts[(int)id];
            Vector3 renderedEnd = part.transform.TransformPoint(new Vector3(0, -length / scales[(int)id], 0));
            MaxBoneError = Mathf.Max(MaxBoneError, Vector3.Distance(renderedEnd, root.TransformPoint(end)));
        }

        void Set(PartId id, Vector2 position, float angle)
        { var t = parts[(int)id].transform; t.localPosition = position; t.localRotation = Quaternion.Euler(0, 0, angle); }
        static Vector2 Rotate(Vector2 p, float angle) { float a = angle * Mathf.Deg2Rad; return new Vector2(p.x * Mathf.Cos(a) - p.y * Mathf.Sin(a), p.x * Mathf.Sin(a) + p.y * Mathf.Cos(a)); }
        static float Smooth(float v) { v = Mathf.Clamp01(v); return v * v * (3 - 2 * v); }

        public void AfterImage(Effects effects, Color color)
        { for (int i = 0; i < parts.Length; i++) if (parts[i].enabled) effects.Ghost(parts[i], color); }
    }
}
