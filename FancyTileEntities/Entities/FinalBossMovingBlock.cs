using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Linq;
using static Celeste.Mod.FancyTileEntities.Extensions;

namespace Celeste.Mod.FancyTileEntities {

    [CustomEntity("FancyTileEntities/FancyFinalBossMovingBlock")]
    [TrackedAs(typeof(FinalBossMovingBlock))]
    public class FancyFinalBossMovingBlock : FinalBossMovingBlock {

        private DynData<FinalBossMovingBlock> baseData;
        private VirtualMap<char> tileMap;
        private VirtualMap<char> tileMapHighlighted;
        private LightOcclude badLightOcclude;

        private ColliderList collider;
        private ColliderList highlightCollider;

        private bool wasHighlighted;

        public FancyFinalBossMovingBlock(EntityData data, Vector2 offset)
            : base(data.NodesWithPosition(offset), data.Width, data.Height, data.Int("nodeIndex", 0)) {
            baseData = new DynData<FinalBossMovingBlock>(this);
            Remove(baseData.Get<TileGrid>("sprite"));
            Remove(baseData.Get<TileGrid>("highlight"));
            Remove(Get<TileInterceptor>());
            badLightOcclude = Get<LightOcclude>();

            tileMap = GenerateTileMap(data.Attr("tileData", ""));
            tileMapHighlighted = GenerateTileMap(data.Attr("tileDataHighlight", ""));

            int newSeed = Calc.Random.Next();
            Calc.PushRandom(newSeed);
            baseData["sprite"] = GFX.FGAutotiler.GenerateMap(tileMap, default(Autotiler.Behaviour)).TileGrid;
            Add(baseData.Get<TileGrid>("sprite"));
            Calc.PopRandom();
            Calc.PushRandom(newSeed);
            TileGrid highlight = GFX.FGAutotiler.GenerateMap(tileMapHighlighted, default(Autotiler.Behaviour)).TileGrid;
            highlight.Alpha = 0f;
            baseData["highlight"] = highlight;
            Add(baseData.Get<TileGrid>("highlight"));
            Calc.PopRandom();

            Add(new TileInterceptor(baseData.Get<TileGrid>("sprite"), false));


            highlightCollider = GenerateBetterColliderGrid(tileMapHighlighted, 8, 8);
            collider = GenerateBetterColliderGrid(tileMap, 8, 8);
            AddLightOcclude(this, collider);
            Collider = collider;
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            Remove(badLightOcclude);
        }

        public override void Update() {
            base.Update();
            if (!wasHighlighted && baseData.Get<bool>("isHighlighted")) {
                Collider = highlightCollider;
                foreach (LightOcclude occlude in Components.GetAll<LightOcclude>().ToList()) // Cannot modify collection while iterating
                    Remove(occlude);
                AddLightOcclude(this, highlightCollider);
                wasHighlighted = true;
            }
        }

        internal static void StopParticles(On.Celeste.FinalBossMovingBlock.orig_StopParticles orig, FinalBossMovingBlock self, Vector2 moved) {
            if (self is FancyFinalBossMovingBlock) {
                Level level = self.SceneAs<Level>();
                float direction = moved.Angle();
                if (moved.X > 0f) {
                    Vector2 position = new Vector2(self.Right - 1f, self.Top);
                    for (int y = 0; y < self.Height; y += 4) {
                        // Move right to left
                        for (int x = (int) self.Width - 2; x >= 0; x -= 4) {
                            if (self.CollidePoint(new Vector2(self.X + x, self.Y + y))) {
                                level.Particles.Emit(P_Stop, position + Vector2.UnitY * (2 + x + Calc.Random.Range(-1, 1)), direction);
                                break;
                            }
                        }

                    }
                } else if (moved.X < 0f) {
                    Vector2 position = new Vector2(self.Left, self.Top);
                    for (int y = 0; y < self.Height; y += 4) {
                        // Move left to right
                        for (int x = 2; x <= self.Width; x += 4) {
                            if (self.CollidePoint(new Vector2(self.X + x, self.Y + y))) {
                                level.Particles.Emit(P_Stop, position + Vector2.UnitY * (2 + y + Calc.Random.Range(-1, 1)), direction);
                                break;
                            }
                        }
                    }
                }
                if (moved.Y > 0f) {
                    Vector2 position = new Vector2(self.Left, self.Bottom - 1f);
                    for (int x = 0; x < self.Width; x += 4) {
                        // Move bottom to top
                        for (int y = (int) self.Height - 2; y >= 0; y -= 4) {
                            if (self.CollidePoint(new Vector2(self.X + x, self.Y + y))) {
                                level.Particles.Emit(P_Stop, position + Vector2.UnitX * (2 + x + Calc.Random.Range(-1, 1)), direction);
                                break;
                            }
                        }
                    }
                } else if (moved.Y < 0f) {
                    Vector2 position = new Vector2(self.Left, self.Top);
                    for (int x = 0; x < self.Width; x += 4) {
                        // Move top to bottom
                        for (int y = 2; y <= self.Height; y += 4) {
                            if (self.CollidePoint(new Vector2(self.X + x, self.Y + y))) {
                                level.Particles.Emit(P_Stop, position + Vector2.UnitX * (2 + x + Calc.Random.Range(-1, 1)), direction);
                                break;
                            }
                        }
                    }
                }
            } else
                orig(self, moved);
        }

        // This method is not hooked, just defined, since it's only used in Finish()
        private static void BreakParticles(FinalBossMovingBlock self) {
            Vector2 center = self.Center;
            for (int x = 0; x < self.Width; x += 4) {
                for (int y = 0; y < self.Height; y += 4) {
                    Vector2 position = self.Position + new Vector2(2 + x, 2 + y);
                    if (self.CollidePoint(position)) {
                        self.SceneAs<Level>().Particles.Emit(P_Break, 1, position, Vector2.One * 2f, (position - center).Angle());
                    }
                }
            }
        }

        internal static void ImpactParticles(On.Celeste.FinalBossMovingBlock.orig_ImpactParticles orig, FinalBossMovingBlock self, Vector2 moved) {
            if (self is FancyFinalBossMovingBlock) {
                if (moved.X < 0f) {
                    for (int y = 0; y < self.Height / 8f; y++) {
                        //Move left to right
                        for (int x = 2; x <= self.Width; x += 4) {
                            if (self.CollidePoint(new Vector2(self.X + x, self.Y + y))) {
                                Vector2 collidePos = new Vector2(self.Left + x - 1f, self.Top + 4f + y * 8);
                                if (!self.Scene.CollideCheck<Water>(collidePos) && self.Scene.CollideCheck<Solid>(collidePos)) {
                                    self.SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, collidePos + Vector2.UnitY * 2 + Vector2.UnitY * 2, 0f);
                                    self.SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, collidePos + Vector2.UnitY * 2 - Vector2.UnitY * 2, 0f);
                                }
                            }
                        }
                    }
                } else if (moved.X > 0f) {
                    for (int y = 0; y < self.Height / 8f; y++) {
                        //Move right to left
                        for (int x = (int) self.Width - 2; x >= 0; x -= 4) {
                            if (self.CollidePoint(new Vector2(self.X + x, self.Y + y))) {
                                Vector2 collidePos = new Vector2(self.Left + x + 1f, self.Top + 4f + y * 8);
                                if (!self.Scene.CollideCheck<Water>(collidePos) && self.Scene.CollideCheck<Solid>(collidePos)) {
                                    self.SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, collidePos + Vector2.UnitY * 2, (float) Math.PI);
                                    self.SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, collidePos - Vector2.UnitY * 2, (float) Math.PI);
                                }
                            }
                        }
                    }
                }

                if (moved.Y < 0f) {
                    for (int x = 0; x < self.Width / 8f; x++) {
                        //Move top to bottom
                        for (int y = 2; y <= self.Height; y += 4) {
                            if (self.CollidePoint(new Vector2(self.X + x, self.Y + y))) {
                                Vector2 collidePos = new Vector2(self.Left + 4f + x * 8, self.Top + y - 1f);
                                if (!self.Scene.CollideCheck<Water>(collidePos) && self.Scene.CollideCheck<Solid>(collidePos)) {
                                    self.SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, collidePos + Vector2.UnitX * 2, (float) Math.PI / 2f);
                                    self.SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, collidePos - Vector2.UnitX * 2, (float) Math.PI / 2f);
                                }
                                break;
                            }
                        }
                    }
                } else if (moved.Y > 0f) {
                    for (int x = 0; x < self.Width / 8f; x++) {
                        //Move bottom to top
                        for (int y = (int) self.Height - 2; y >= 0; y -= 4) {
                            if (self.CollidePoint(new Vector2(self.X + x, self.Y + y))) {
                                Vector2 collidePos = new Vector2(self.Left + 4f + x * 8, self.Top + y + 1f);
                                if (!self.Scene.CollideCheck<Water>(collidePos) && self.Scene.CollideCheck<Solid>(collidePos)) {
                                    self.SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, collidePos + Vector2.UnitX * 2, -(float) Math.PI / 2f);
                                    self.SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, collidePos - Vector2.UnitX * 2, -(float) Math.PI / 2f);
                                }
                                break;
                            }
                        }

                    }
                }
            } else
                orig(self, moved);
        }

        internal static void Finish(On.Celeste.FinalBossMovingBlock.orig_Finish orig, FinalBossMovingBlock self) {
            if (self is FancyFinalBossMovingBlock) {
                Vector2 from = self.CenterRight + Vector2.UnitX * 10f;
                for (int i = 0; i < self.Width / 8f; i++) {
                    for (int j = 0; j < self.Height / 8f; j++) {
                        self.Scene.Add(Engine.Pooler.Create<Debris>().Init(self.Position + new Vector2(4 + i * 8, 4 + j * 8), 'f', playSound: true).BlastFrom(from));
                    }
                }
                //This one's just defined and used here
                BreakParticles(self);
                self.DestroyStaticMovers();
                self.RemoveSelf();
            } else
                orig(self);
        }

        public override void MoveHExact(int move) => this.MoveHExactSmooth(move);
        public override void MoveVExact(int move) => this.MoveVExactSmooth(move);

    }
}
