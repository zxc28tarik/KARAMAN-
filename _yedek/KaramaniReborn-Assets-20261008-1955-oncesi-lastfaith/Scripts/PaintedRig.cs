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
        // İkincil hareket: yay fiziğiyle pelerin, gecikmeli baş, inişte çökme
        float capeAngle, capeVelocity, headAngle, landDip, airVelocity, guardBlend, accelLean, turnTwist;
        public int Steps { get; private set; }
        int capeFace;
        bool wasGrounded = true;
        static Material paintedMaterial;
        readonly MaterialPropertyBlock block = new();
        public readonly BladeTrail trail;
        readonly Color trailColor;
        static readonly int FlashId = Shader.PropertyToID("_Flash"), RimColorId = Shader.PropertyToID("_RimColor"), RimDirId = Shader.PropertyToID("_RimDir"),
            RimStrengthId = Shader.PropertyToID("_RimStrength"), LightColorId = Shader.PropertyToID("_LightColor"), LightAmountId = Shader.PropertyToID("_LightAmount"), MoonId = Shader.PropertyToID("_MoonStrength");
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
            if (!paintedMaterial) paintedMaterial = new Material(Resources.Load<Shader>("Shaders/PaintedSprite")) { name = "Painted sprite" };
            foreach (var part in parts) part.sharedMaterial = paintedMaterial;
            equipment.sharedMaterial = paintedMaterial;
            trailColor = actor.player ? new Color(1f, .86f, .62f, .85f) : new Color(1f, .42f, .26f, .5f);
            trail = new BladeTrail(parent, trailColor, 175);
            capeFace = actor.face;
            actor.rig = this;
            Render(actor, 0, 0);
        }

        public void Destroy()
        {
            if (Application.isPlaying) { UnityEngine.Object.Destroy(root.gameObject); UnityEngine.Object.Destroy(shadow.gameObject); }
            else { UnityEngine.Object.DestroyImmediate(root.gameObject); UnityEngine.Object.DestroyImmediate(shadow.gameObject); }
            trail.Destroy();
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
                if (!first) turnTwist = 1;
                previousX = actor.position.x; lastFace = actor.face;
                for (int i = 0; i < 2; i++) { feet[i].x = i == 0 ? .17f : -.17f; feet[i].planted = true; feet[i].world = actor.position + new Vector2(feet[i].x * actor.face * size, Ankle * size); }
                first = false;
            }
            float dx = actor.position.x - previousX; previousX = actor.position.x;
            float speed = Mathf.Abs(actor.velocity.x / size);
            bool skidding = actor.skid > 0 && actor.grounded && !actor.Locked;
            bool walking = actor.grounded && speed > .12f && !actor.Locked && actor.state != ActionState.Block && !skidding;
            float run = Mathf.Clamp01((speed - 2.3f) / 2.0f);
            // Kılıç duruşu yumuşak geçer: daha geniş ayaklar, kısa adımlar, önde tutulan kılıç
            bool guarding = actor.enGarde && actor.state is ActionState.Idle or ActionState.Move;
            guardBlend = dt > 0 ? Mathf.MoveTowards(guardBlend, guarding ? 1 : 0, dt * 6) : guarding ? 1 : 0;
            float stride = Mathf.Lerp(1.18f, 1.52f, run) * Mathf.Lerp(1, .72f, guardBlend);
            if (walking) gait = Mathf.Repeat(gait + Mathf.Abs(dx) / (stride * size), 1);
            movementBlend = Mathf.MoveTowards(movementBlend, walking ? 1 : 0, dt * 9);
            float breath = Mathf.Sin(clock * 2.2f) * .009f;
            float hipY = Mathf.Lerp(1.215f + breath, 1.115f + Mathf.Cos(gait * Mathf.PI * 4) * .016f, movementBlend) - .09f * guardBlend;
            // Koşu: hızlanırken öne, yavaşlarken geriye eğilir; her adımda gövde hafifçe salınır
            if (dt > 0) accelLean = Mathf.Lerp(accelLean, -Mathf.Clamp((speed - previousSpeed) / dt * .35f, -6, 8), 1 - Mathf.Exp(-dt * 10));
            float lean = walking ? Mathf.Lerp(-3f, -9f, run) + Mathf.Sin(gait * Mathf.PI * 4) * 1.2f * run + accelLean : 0;
            lean -= 4 * guardBlend;
            turnTwist = Mathf.MoveTowards(turnTwist, 0, dt * 6);
            lean += 7 * turnTwist;
            float p = actor.Progress;
            Vector2 nearHip = new Vector2(.035f, hipY), farHip = new Vector2(-.035f, hipY + .012f);
            nearFoot = FootPose(actor, 0, gait, stride, walking, dt);
            farFoot = FootPose(actor, 1, Mathf.Repeat(gait + .5f, 1), stride, walking, dt);

            if (!actor.grounded)
            {
                // Havada: kalkışta itiş, tepede toplanma, düşüşte bacaklar yere uzanır
                feet[0].planted = feet[1].planted = false;
                float rise = Mathf.Clamp01(actor.velocity.y / 8f), fall = Mathf.Clamp01(-actor.velocity.y / 9f);
                nearFoot = new Vector2(Mathf.Lerp(.29f, .16f, rise), Mathf.Lerp(.37f, .14f, rise));
                farFoot = new Vector2(Mathf.Lerp(-.24f, -.30f, rise), Mathf.Lerp(.24f, .08f, rise));
                nearFoot = Vector2.Lerp(nearFoot, new Vector2(.21f, .10f), fall); farFoot = Vector2.Lerp(farFoot, new Vector2(-.14f, .17f), fall);
                hipY = 1.08f + rise * .06f; lean = -5 + fall * 5;
                if (actor.airFlip > 0) { nearFoot = new Vector2(.24f, .55f); farFoot = new Vector2(-.04f, .50f); hipY = 1.1f; }
            }
            if (skidding)
            {
                // Kayarak durma: ön ayak ileride, topuk yere basar, gövde geriye yaslanır
                feet[0].planted = feet[1].planted = false; feet[0].angle = 14; feet[1].angle = 0;
                nearFoot = new Vector2(.40f, Ankle); farFoot = new Vector2(-.12f, Ankle + .02f); hipY = 1.02f; lean = 12;
            }
            if (actor.state is ActionState.Light or ActionState.Heavy) AttackBody(actor, p, ref hipY, ref lean);
            bool diving = actor.state == ActionState.Light && actor.combo == 4;
            if (diving)
            {
                // Dalış: dizler toplanır, kılıç aşağı
                feet[0].planted = feet[1].planted = false;
                nearFoot = new Vector2(.18f, .42f); farFoot = new Vector2(-.12f, .36f); hipY = 1.05f; lean = -6;
            }
            else if (actor.Attacking || actor.state == ActionState.Block || actor.state == ActionState.Parry)
            {
                hipY = 1.125f + .025f * Mathf.Sin(p * Mathf.PI);
                lean = actor.state == ActionState.Heavy ? -6 - Mathf.Sin(p * Mathf.PI) * 7 : -Mathf.Sin(p * Mathf.PI) * 6;
            }
            // Vurulma: hızlı geriye savrulma, yavaş toparlanma
            float recoil = actor.state == ActionState.Hurt ? (1 - p) * (1 - p) * Smooth(Mathf.Min(1, p * 6)) : 0;
            if (actor.state == ActionState.Hurt) { lean = 16 * recoil; hipY -= .06f * recoil; }
            if (actor.state == ActionState.Spirit)
            { nearFoot = new Vector2(.42f, .26f); farFoot = new Vector2(-.36f, .13f); hipY = 1.00f; lean = -23; feet[0].planted = feet[1].planted = false; }
            if (actor.state == ActionState.Heal) { hipY = 1.02f; lean = 2; }
            if (actor.state == ActionState.Dodge && actor.backdash)
            {
                // Geri adım: alçak, geriye kayan duruş; kılıç önde kalır
                hipY = .98f; lean = 10; nearFoot = new Vector2(.26f, Ankle); farFoot = new Vector2(-.30f, Ankle + .02f);
                feet[0].planted = feet[1].planted = false;
            }
            else if (actor.state is ActionState.Dodge or ActionState.Dead)
            {
                hipY = .76f; nearFoot = new Vector2(.19f, .35f); farFoot = new Vector2(-.16f, .32f);
                feet[0].planted = feet[1].planted = false;
            }
            Vector2 ledge = new Vector2((actor.ledge.x - actor.position.x) * actor.face / size, (actor.ledge.y - actor.position.y) / size);
            bool hanging = actor.state == ActionState.Hang, climbing = actor.state == ActionState.Climb;
            if (hanging)
            {
                // Asılı: kollar kenarda, bacaklar hafifçe sallanarak sarkar, ayak uçları aşağı
                feet[0].planted = feet[1].planted = false; feet[0].angle = feet[1].angle = -25;
                hipY = .52f; lean = 4;
                nearFoot = new Vector2(.07f + Mathf.Sin(clock * 2.1f) * .03f, hipY - 1.0f); farFoot = new Vector2(-.05f - Mathf.Sin(clock * 2.1f) * .02f, hipY - .97f);
            }
            if (climbing) ClimbBody(p, ledge, ref hipY, ref lean);

            // İniş: düşüş hızına göre kalça kısa süre çöker
            if (!actor.grounded) airVelocity = actor.velocity.y;
            if (actor.grounded && !wasGrounded) landDip = Mathf.Clamp(-airVelocity / 9f, 0, 1.4f) * .8f + .2f;
            wasGrounded = actor.grounded;
            landDip = Mathf.MoveTowards(landDip, 0, dt * 4.5f);
            if (actor.grounded) hipY -= .085f * Mathf.Min(1.3f, landDip);
            // Lower the pelvis rather than lengthening the painted leg if the stance has spread.
            hipY = Mathf.Min(hipY, ReachableHip(nearFoot, .035f), ReachableHip(farFoot, -.035f));
            nearHip.y = hipY; farHip.y = hipY + .012f;
            Leg(0, nearHip, nearFoot); Leg(1, farHip, farFoot);
            StepCape(actor, dt, clock, size);
            Set(PartId.Hips, new Vector2(0, hipY + .15f), lean * .25f + capeAngle * .06f);
            Set(PartId.Torso, new Vector2(0, hipY + .06f), lean);
            Vector2 shoulder = new Vector2(0, hipY + .06f) + Rotate(new Vector2(.075f, .56f), lean);
            float headTarget = lean * .35f + 12 * recoil;
            headAngle = dt > 0 ? Mathf.Lerp(headAngle, headTarget, 1 - Mathf.Exp(-dt * 14)) : headTarget;
            Set(PartId.Head, new Vector2(0, hipY + .06f) + Rotate(new Vector2(.13f, .66f), lean), headAngle);
            Set(PartId.Cape, new Vector2(-.16f, hipY + .66f), capeAngle + lean * .6f);

            nearHand = shoulder + new Vector2(.38f, -.36f);
            farHand = shoulder + new Vector2(-.18f, -.34f);
            swordAngle = 8;
            if (guardBlend > 0)
            {
                // Kılıç duruşu: kılıç önde ve yukarıda; kalkanlılar kalkanı öne alır, diğerleri arka kolla denge kurar
                bool shieldBearer = !actor.player && actor.role is EnemyRole.Shield or EnemyRole.Captain;
                nearHand = Vector2.Lerp(nearHand, shoulder + new Vector2(.40f, -.12f), guardBlend);
                farHand = Vector2.Lerp(farHand, shoulder + (shieldBearer ? new Vector2(.24f, -.16f) : new Vector2(-.30f, -.24f)), guardBlend);
                swordAngle = Mathf.Lerp(swordAngle, 28, guardBlend);
            }
            if (walking)
            {
                // Adımla uyumlu kol salınımı: arka kol koşuya daha çok katılır, kılıçlı el daha az
                float s = Mathf.Sin(gait * Mathf.PI * 2), swing = Mathf.Lerp(.11f, .2f, run) * (1 - guardBlend * .7f);
                nearHand += new Vector2(-s * swing * .55f, -.05f + Mathf.Abs(s) * .03f);
                farHand += new Vector2(s * swing * 1.4f, -.04f + Mathf.Max(0, s) * swing * .8f);
                swordAngle += -9 * (1 - guardBlend) + s * 9;
            }
            if (skidding) { nearHand = shoulder + new Vector2(.25f, -.30f); farHand = shoulder + new Vector2(-.40f, -.05f); swordAngle = -20; }
            if (!actor.grounded)
            {
                float fall = Mathf.Clamp01(-actor.velocity.y / 9f);
                nearHand = shoulder + new Vector2(.30f, -.29f); farHand = shoulder + new Vector2(-.35f, .10f + fall * .12f); swordAngle = 28;
            }
            if (hanging || climbing)
            {
                // Eller kenarı tutar; tırmanışın sonunda kol yeniden kılıca döner
                float release = climbing ? Smooth((p - .78f) / .22f) : 0;
                nearHand = Vector2.Lerp(ledge + new Vector2(0, -.02f), shoulder + new Vector2(.38f, -.36f), release);
                farHand = Vector2.Lerp(ledge + new Vector2(-.08f, -.02f), shoulder + new Vector2(-.18f, -.34f), release);
                swordAngle = 8;
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
            if (actor.state is ActionState.Dodge or ActionState.Dead && !actor.backdash)
            { nearHand = shoulder + new Vector2(.17f, -.24f); farHand = shoulder + new Vector2(-.05f, -.22f); swordAngle = -30; }
            if (actor.state == ActionState.Dodge && actor.backdash)
            { nearHand = shoulder + new Vector2(.36f, -.20f); farHand = shoulder + new Vector2(-.28f, -.22f); swordAngle = 20; }
            if (recoil > 0) { nearHand += new Vector2(-.18f, .16f) * recoil; farHand += new Vector2(-.22f, .12f) * recoil; swordAngle += 35 * recoil; }
            Arm(PartId.FarUpperArm, PartId.FarForearm, shoulder + new Vector2(-.11f, .01f), ref farHand, -1);
            Arm(PartId.UpperArm, PartId.Forearm, shoulder, ref nearHand, -1);
            Set(PartId.Saber, nearHand, swordAngle);
            Set(PartId.Shield, farHand + new Vector2(.035f, -.035f), -5);
            parts[(int)PartId.Shield].enabled = !actor.player && actor.role is EnemyRole.Shield or EnemyRole.Captain || actor.state is ActionState.Block or ActionState.Parry;
            bool bow = !actor.player && actor.role == EnemyRole.Archer;
            bool potion = actor.state == ActionState.Heal;
            bool gun = actor.player && actor.state == ActionState.Shoot;
            equipment.enabled = bow || potion || gun;
            parts[(int)PartId.Saber].enabled = !bow && !potion && !gun && !hanging && !climbing;
            if (equipment.enabled)
            {
                equipment.sprite = ArtLibrary.Gear()[potion ? 2 : bow ? 1 : 0];
                float scale = potion ? .28f / equipment.sprite.bounds.size.y : bow ? .92f / equipment.sprite.bounds.size.y : .62f / equipment.sprite.bounds.size.x;
                equipment.transform.localScale = new Vector3(scale, scale, 1);
                equipment.transform.localPosition = potion ? farHand + Vector2.down * .07f : nearHand;
                equipment.transform.localRotation = Quaternion.identity;
            }

            if (actor.state == ActionState.Dodge && !actor.backdash)
            {
                float turn = -360 * Smooth(p);
                RotateAroundBody(actor, turn, 1.03f);
            }
            if (actor.airFlip > 0 && !actor.grounded && actor.state is ActionState.Jump or ActionState.Fall)
                RotateAroundBody(actor, -360 * Smooth(1 - actor.airFlip / .38f), 1.03f);
            if (actor.state == ActionState.Dead) RotateAroundBody(actor, -82 * Smooth(Mathf.Clamp01(actor.stateTime / .75f)), .43f);
            Color tint = actor.player ? Color.white : actor.role == EnemyRole.Ghoul ? new Color(.7f, .77f, .74f) : actor.boss ? new Color(1, .91f, .78f) : new Color(.94f, .93f, 1);
            if (actor.Dead) tint = Color.Lerp(tint, new Color(.34f, .34f, .38f, .35f), Mathf.Clamp01(actor.stateTime / 3));
            foreach (var part in parts) part.color = tint;
            ApplyLight(actor, size);
            // Kılıç izi: hazırlığın sonundan vuruşun hemen sonrasına kadar
            bool slash = actor.state is ActionState.Light or ActionState.Heavy && parts[(int)PartId.Saber].enabled;
            float windEnd = actor.state == ActionState.Heavy ? .44f : .27f, strikeEnd = actor.state == ActionState.Heavy ? .69f : .58f;
            trail.lifetime = actor.state == ActionState.Heavy ? .17f : .12f;
            trail.color = actor.riposting ? new Color(1f, .74f, .3f, 1f) : trailColor;
            trail.Step(Vector2.Lerp(BladeRoot, BladeTip, .3f), BladeTip, slash && (diving || p > windEnd * .8f && p < strikeEnd + .07f), dt);
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
            float rest = i == 0 ? Mathf.Lerp(.17f, .31f, guardBlend) : Mathf.Lerp(-.17f, -.29f, guardBlend);
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
                float desired = walking ? localX : rest;
                if (!walking && Mathf.Abs(foot.x - desired) > .008f)
                {
                    foot.x = Mathf.MoveTowards(foot.x, desired, dt * 1.6f);
                    return new Vector2(foot.x, Ankle + .025f);
                }
                foot.world = actor.position + new Vector2(desired * actor.face * actor.size, Ankle * actor.size); foot.planted = true;
                if (walking) Steps++;
            }
            foot.x = (foot.world.x - actor.position.x) * actor.face / actor.size;
            if (!walking && actor.grounded && Mathf.Abs(foot.x - rest) > .07f && feet[1 - i].planted)
            { foot.planted = false; return new Vector2(foot.x, Ankle + .025f); }
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
            if (actor.state == ActionState.Light && actor.combo == 4)
            { nearHand = shoulder + new Vector2(.14f, -.56f); farHand = shoulder + new Vector2(.04f, -.50f); swordAngle = -82; return; }
            Vector2 ready = shoulder + new Vector2(.37f, -.34f);
            Vector2 raised = shoulder + new Vector2(-.12f, .43f);
            Vector2 follow = shoulder + new Vector2(.48f, -.23f);
            float from = 135, to = -32;
            if (actor.combo == 2 && actor.state == ActionState.Light)
            { raised = shoulder + new Vector2(.19f, -.46f); follow = shoulder + new Vector2(.49f, .18f); from = -58; to = 63; }
            if (actor.combo == 3 && actor.state == ActionState.Light)
            { raised = shoulder + new Vector2(-.17f, -.20f); follow = shoulder + new Vector2(.63f, -.05f); from = -5; to = 5; }
            // Hazırlıkta kısa bir duraksama, vuruşta hedefi biraz aşıp geri dönen takip hareketi
            if (p < wind)
            { float t = Smooth(Mathf.Min(1, p / (wind * .85f))); nearHand = Vector2.Lerp(ready, raised, t); swordAngle = Mathf.Lerp(8, from, t); }
            else if (p < strike)
            { float t = BackOut((p - wind) / (strike - wind), actor.state == ActionState.Heavy ? 1.1f : 1.5f); nearHand = Vector2.LerpUnclamped(raised, follow, t); swordAngle = Mathf.LerpUnclamped(from, to, t); }
            else
            { float t = Smooth((p - strike) / (1 - strike)); nearHand = Vector2.Lerp(follow, ready, t); swordAngle = Mathf.Lerp(to, 8, t); }
            farHand = shoulder + new Vector2(-.17f, -.24f);
        }

        // Saldırıda gövde: hazırlıkta geriye yaslanma, vuruşta öne ağırlık verip çökme, sonra toparlanma
        void AttackBody(Actor actor, float p, ref float hipY, ref float lean)
        {
            bool heavy = actor.state == ActionState.Heavy, thrust = !heavy && actor.combo == 3;
            float wind = heavy ? .44f : .27f, strike = heavy ? .69f : .58f;
            float back = heavy ? 9 : thrust ? 2 : 4.5f, forward = heavy ? -17 : thrust ? -13 : -9, dip = heavy ? .075f : .04f;
            if (p < wind) { float t = Smooth(p / wind); lean = back * t; hipY = 1.15f - .015f * t; }
            else if (p < strike) { float t = Smooth((p - wind) / (strike - wind)); lean = Mathf.Lerp(back, forward, t); hipY = Mathf.Lerp(1.135f, 1.125f - dip, t); }
            else { float t = Smooth((p - strike) / (1 - strike)); lean = Mathf.Lerp(forward, 0, t); hipY = Mathf.Lerp(1.125f - dip, 1.2f, t); }
        }

        // Tırmanış evreleri: kendini çekme (dizler toplanır), kenara diz atma (öne eğilme), ayağa kalkma
        void ClimbBody(float p, Vector2 ledge, ref float hipY, ref float lean)
        {
            feet[0].planted = feet[1].planted = false; feet[0].angle = feet[1].angle = 0;
            if (p < .45f)
            {
                float t = Smooth(p / .45f); hipY = Mathf.Lerp(.52f, .95f, t); lean = Mathf.Lerp(4, -10, t);
                nearFoot = new Vector2(.18f, hipY - Mathf.Lerp(1f, .55f, t)); farFoot = new Vector2(-.05f, hipY - .97f);
            }
            else if (p < .75f)
            {
                float t = Smooth((p - .45f) / .3f); hipY = Mathf.Lerp(.95f, .85f, t); lean = Mathf.Lerp(-10, -22, t);
                nearFoot = Vector2.Lerp(new Vector2(.18f, hipY - .55f), ledge + new Vector2(.12f, Ankle), t); farFoot = new Vector2(-.05f, hipY - .95f);
            }
            else
            {
                float t = Smooth((p - .75f) / .25f); hipY = Mathf.Lerp(.85f, 1.2f, t); lean = Mathf.Lerp(-22, 0, t);
                nearFoot = Vector2.Lerp(ledge + new Vector2(.12f, Ankle), new Vector2(.17f, Ankle), t);
                farFoot = Vector2.Lerp(new Vector2(-.05f, hipY - .95f), new Vector2(-.17f, Ankle), t);
            }
        }

        // Pelerin: hıza, düşüşe ve dönüşe tepki veren sönümlü yay
        void StepCape(Actor actor, float dt, float clock, float size)
        {
            float forward = actor.velocity.x * actor.face / Mathf.Max(.5f, size);
            float target = -Mathf.Clamp(forward * 6.5f, -10, 42) + Mathf.Sin(clock * 1.6f) * 2.2f + Mathf.Sin(clock * .53f + 1) * 2.6f;
            if (!actor.grounded) target -= Mathf.Clamp(-actor.velocity.y * 3.2f, -12, 26);
            if (actor.state is ActionState.Dodge or ActionState.Dead) target = -30;
            if (capeFace != actor.face) { capeVelocity += 260; capeFace = actor.face; }
            if (dt <= 0) { capeAngle = target; return; }
            capeVelocity += ((target - capeAngle) * 42 - capeVelocity * 7.2f) * dt;
            capeAngle = Mathf.Clamp(capeAngle + capeVelocity * dt, -75, 28);
        }

        // Sahne ışığı: en yakın ateşten kenar ışığı ve sıcaklık; vurulunca beyaz parlama
        void ApplyLight(Actor actor, float size)
        {
            LightField.Sample(actor.position + Vector2.up * 1.2f * size, out var direction, out var color, out float amount, out float rim);
            block.SetFloat(FlashId, actor.hitFlash > 0 ? Mathf.Clamp01(actor.hitFlash / .14f) * .9f : 0);
            block.SetVector(RimDirId, new Vector4(direction.x, direction.y, 0, 0));
            block.SetColor(RimColorId, color);
            block.SetFloat(RimStrengthId, actor.Dead ? rim * .3f : rim);
            block.SetColor(LightColorId, color);
            block.SetFloat(LightAmountId, amount);
            block.SetFloat(MoonId, actor.Dead ? .08f : .22f);
            foreach (var part in parts) part.SetPropertyBlock(block);
            equipment.SetPropertyBlock(block);
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
        static float BackOut(float t, float s) { t = Mathf.Clamp01(t) - 1; return t * t * ((s + 1) * t + s) + 1; }

        public void AfterImage(Effects effects, Color color)
        { for (int i = 0; i < parts.Length; i++) if (parts[i].enabled) effects.Ghost(parts[i], color); }
    }
}
