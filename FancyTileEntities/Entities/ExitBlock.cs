using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Reflection;
using static Celeste.Mod.FancyTileEntities.Extensions;

namespace Celeste.Mod.FancyTileEntities {

    [CustomEntity("FancyTileEntities/FancyExitBlock", "FancyTileEntities/FancyConditionBlock=LoadConditionBlock")]
    public class FancyExitBlock : ExitBlock {
        private static readonly FieldInfo<TileGrid> f_ExitBlock_tiles;
        private VirtualMap<char> tileMap;

        static FancyExitBlock() {
            f_ExitBlock_tiles = typeof(ExitBlock).GetField<TileGrid>("tiles", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public FancyExitBlock(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, data.Char("tileType", '3')) {
            tileMap = GenerateTileMap(data.Attr("tileData", ""));
            Collider = GenerateInefficientColliderGrid(tileMap, 8, 8);
        }

        public override void Added(Scene scene) {
            IntPtr ptr = typeof(Entity).GetMethod("Added").MethodHandle.GetFunctionPointer();
            Action<Scene> added_Entity = (Action<Scene>) Activator.CreateInstance(typeof(Action<Scene>), this, ptr);
            added_Entity(scene);

            Level level = SceneAs<Level>();
            Rectangle tileBounds = level.Session.MapData.TileBounds;
            VirtualMap<char> solidsData = level.SolidsData;
            int x = (int) (X / 8f) - tileBounds.Left;
            int y = (int) (Y / 8f) - tileBounds.Top;

            f_ExitBlock_tiles[this] = GFX.FGAutotiler.GenerateOverlay(tileMap, x, y, solidsData).TileGrid;
            Add(f_ExitBlock_tiles[this]);
            Add(new TileInterceptor(f_ExitBlock_tiles[this], false));
        }

        public static Entity LoadConditionBlock(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            //0=Key, 1=Button, 2=Strawberry
            int conditionBlockModes = entityData.Int("condition");
            EntityID conditionEntity = EntityID.None;
            string[] condition = entityData.Attr("conditionID").Split(':');
            conditionEntity.Level = condition[0];
            conditionEntity.ID = Convert.ToInt32(condition[1]);
            bool conditionMet;
            switch (conditionBlockModes) {
                case 1:
                    conditionMet = level.Session.GetFlag(DashSwitch.GetFlagName(conditionEntity));
                    break;
                case 0:
                    conditionMet = level.Session.DoNotLoad.Contains(conditionEntity);
                    break;
                case 2:
                    conditionMet = level.Session.Strawberries.Contains(conditionEntity);
                    break;
                default:
                    throw new Exception("Condition type not supported!");
            }
            if (conditionMet) {
                return new FancyExitBlock(entityData, offset);
            }
            return null;
        }

        #region SoundIndex

        public override int GetLandSoundIndex(Entity entity) {
            int idx = SurfaceSoundIndexAt(entity.BottomCenter + Vector2.UnitY * 4f);
            if (idx == -1) {
                idx = SurfaceSoundIndexAt(entity.BottomLeft + Vector2.UnitY * 4f);
            }
            if (idx == -1) {
                idx = SurfaceSoundIndexAt(entity.BottomRight + Vector2.UnitY * 4f);
            }
            return idx;
        }

        public override int GetWallSoundIndex(Player player, int side) {
            int idx = SurfaceSoundIndexAt(player.Center + Vector2.UnitX * side * 8f);
            if (idx < 0) {
                idx = SurfaceSoundIndexAt(player.Center + new Vector2(side * 8, -6f));
            }
            if (idx < 0) {
                idx = SurfaceSoundIndexAt(player.Center + new Vector2(side * 8, 6f));
            }
            return idx;
        }

        public override int GetStepSoundIndex(Entity entity) {
            int idx = SurfaceSoundIndexAt(entity.BottomCenter + Vector2.UnitY * 4f);
            if (idx == -1) {
                idx = SurfaceSoundIndexAt(entity.BottomLeft + Vector2.UnitY * 4f);
            }
            if (idx == -1) {
                idx = SurfaceSoundIndexAt(entity.BottomRight + Vector2.UnitY * 4f);
            }
            return idx;
        }

        private int SurfaceSoundIndexAt(Vector2 readPosition) {
            int x = (int) ((readPosition.X - X) / 8f);
            int y = (int) ((readPosition.Y - Y) / 8f);
            if (x >= 0 && y >= 0 && x < tileMap.Columns && y < tileMap.Rows) {
                char c = tileMap[x, y];
                if (c == 'k') {
                    return CoreTileSurfaceIndex();
                }
                if (c != '0' && SurfaceIndex.TileToIndex.ContainsKey(c)) {
                    return SurfaceIndex.TileToIndex[c];
                }
            }
            return -1;
        }

        private int CoreTileSurfaceIndex() {
            Level level = SceneAs<Level>();
            if (level.CoreMode == Session.CoreModes.Hot) {
                return 37;
            }
            if (level.CoreMode == Session.CoreModes.Cold) {
                return 36;
            }
            return 3;
        }

        #endregion

    }
}
