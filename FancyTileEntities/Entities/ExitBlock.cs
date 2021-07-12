using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using System;
using System.Reflection;
using static Celeste.Mod.FancyTileEntities.Extensions;

namespace Celeste.Mod.FancyTileEntities {

    [CustomEntity("FancyTileEntities/FancyExitBlock", "FancyTileEntities/FancyConditionBlock=LoadConditionBlock")]
    public class FancyExitBlock : ExitBlock {

        public enum ConditionBlockModes {
            Key,
            Button,
            Strawberry
        }

        private static readonly FieldInfo<TileGrid> f_ExitBlock_tiles;
        private VirtualMap<char> tileMap;

        static FancyExitBlock() {
            f_ExitBlock_tiles = typeof(ExitBlock).GetField<TileGrid>("tiles", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public FancyExitBlock(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, data.Char("tileType", '3')) {
            tileMap = GenerateTileMap(data.Attr("tileData", ""));
            Collider = GenerateBetterColliderGrid(tileMap, 8, 8);
        }

        [MonoModLinkTo("Monocle.Entity", "System.Void Added(Monocle.Scene)")]
        public void base_Added(Scene scene) {
            base.Added(scene);
        }

        public override void Added(Scene scene) {
            base_Added(scene);

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
            ConditionBlockModes conditionBlockModes = entityData.Enum("condition", ConditionBlockModes.Key);
            EntityID conditionEntity = EntityID.None;
            string[] condition = entityData.Attr("conditionID").Split(':');
            conditionEntity.Level = condition[0];
            conditionEntity.ID = Convert.ToInt32(condition[1]);
            if (conditionBlockModes switch {
                ConditionBlockModes.Key => level.Session.GetFlag(DashSwitch.GetFlagName(conditionEntity)),
                ConditionBlockModes.Button => level.Session.DoNotLoad.Contains(conditionEntity),
                ConditionBlockModes.Strawberry => level.Session.Strawberries.Contains(conditionEntity),
                _ => throw new Exception("Condition type not supported!")
            }) {
                return new FancyExitBlock(entityData, offset);
            }
            return null;
        }

        public override void MoveHExact(int move) => this.MoveHExactSmooth(move);
        public override void MoveVExact(int move) => this.MoveVExactSmooth(move);

        public override int GetLandSoundIndex(Entity entity) => this.GetLandSoundIndex(entity, tileMap);
        public override int GetWallSoundIndex(Player player, int side) => this.GetWallSoundIndex(player, side, tileMap);
        public override int GetStepSoundIndex(Entity entity) => this.GetStepSoundIndex(entity, tileMap);

    }
}
