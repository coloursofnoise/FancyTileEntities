using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;

namespace Celeste.Mod.FancyTileEntities {
    public class BetterIntroCrusher : IntroCrusher {

        protected bool manualTrigger;
        protected bool triggered;

        protected float delay;

        protected float speed;

        protected DynData<IntroCrusher> baseData;

        public BetterIntroCrusher(EntityData data, Vector2 offset)
            : base(data, offset) {
            baseData = new DynData<IntroCrusher>(this);

            manualTrigger = data.Bool("manualTrigger");
            delay = data.Float("delay", 1.2f);
            speed = data.Float("speed", 2f);

            Remove(Get<EntityTriggerListener>()); // Remove Everest added listener if it exists
            Add(new EntityTriggerListener(Trigger, StartTriggered));
        }

        internal static IEnumerator Sequence(On.Celeste.IntroCrusher.orig_Sequence orig, IntroCrusher self) {
            if (self is BetterIntroCrusher crusher)
                yield return new SwapImmediately(crusher.Sequence());
            else
                yield return new SwapImmediately(orig(self));
        }

        private IEnumerator Sequence() {
            Player player;

            do {
                yield return null;
                player = Scene.Tracker.GetEntity<Player>();
            }
            while (!triggered && (manualTrigger || player == null || player.X < X + 30f || player.X > Right + 8f));

            SoundSource shakingSfx = baseData.Get<SoundSource>("shakingSfx");
            shakingSfx.Play(SFX.game_00_fallingblock_prologue_shake);
            float time = delay;
            Shaker shaker = new Shaker(time, removeOnFinish: true, v => baseData["shake"] = v);
            if (delay > 0f) {
                Add(shaker);
            }
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

            while (time > 0f) {
                player = Scene.Tracker.GetEntity<Player>();
                if (!manualTrigger && player != null && (player.X >= X + Width - 8f || player.X < X + 28f)) {
                    shaker.RemoveSelf();
                    break;
                }
                yield return null;
                time -= Engine.DeltaTime;
            }

            for (int i = 2; i < Width; i += 4) {
                SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustA, 2, new Vector2(X + i, Y), Vector2.One * 4f, (float) Math.PI / 2f);
                SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustB, 2, new Vector2(X + i, Y), Vector2.One * 4f);
            }
            shakingSfx.Param("release", 1f);

            time = 0f;
            do {
                yield return null;
                time = Calc.Approach(time, 1f, speed * Engine.DeltaTime);
                MoveTo(Vector2.Lerp(baseData.Get<Vector2>("start"), baseData.Get<Vector2>("end"), Ease.CubeIn(time)));
            }
            while (time < 1f);

            for (int j = 0; j <= Width; j += 4) {
                SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_FallDustA, 1, new Vector2(X + j, Bottom), Vector2.One * 4f, -(float) Math.PI / 2f);
                float direction = (j < Width / 2f) ? ((float) Math.PI) : 0f;
                SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_LandDust, 1, new Vector2(X + j, Bottom), Vector2.One * 4f, direction);
            }
            shakingSfx.Stop();
            Audio.Play(SFX.game_00_fallingblock_prologue_impact, Position);
            SceneAs<Level>().Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Add(new Shaker(0.25f, removeOnFinish: true, v => baseData["shake"] = v));
        }

        public void Trigger() {
            if (manualTrigger) {
                triggered = true;
            }
        }

        public void StartTriggered() {
            if (manualTrigger) {
                triggered = true;
                Position = baseData.Get<Vector2>("end");
                Remove(Get<Coroutine>());
            }
        }
    }
}
