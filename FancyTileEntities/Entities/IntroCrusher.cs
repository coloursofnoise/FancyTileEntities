using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Celeste.Mod.FancyTileEntities.Extensions;

namespace Celeste.Mod.FancyTileEntities {

    [CustomEntity("FancyTileEntities/FancyIntroCrusher")]
    public class FancyIntroCrusher : BetterIntroCrusher {

        private VirtualMap<char> tileMap;

        public FancyIntroCrusher(EntityData data, Vector2 offset)
            : base(data.Modify(data => data.Values["tiletype"] = "3"), offset) {
            Remove(baseData.Get<TileGrid>("tilegrid"));

            tileMap = GenerateTileMap(data.Attr("tileData", ""));
            Collider = GenerateBetterColliderGrid(tileMap, 8, 8);

            baseData["tilegrid"] = GFX.FGAutotiler.GenerateMap(tileMap, default(Autotiler.Behaviour)).TileGrid;
            Add(baseData.Get<TileGrid>("tilegrid"));
        }

        public override void MoveHExact(int move) => this.MoveHExactSmooth(move);
        public override void MoveVExact(int move) => this.MoveVExactSmooth(move);

        public override int GetLandSoundIndex(Entity entity) => this.GetLandSoundIndex(entity, tileMap);
        public override int GetWallSoundIndex(Player player, int side) => this.GetWallSoundIndex(player, side, tileMap);
        public override int GetStepSoundIndex(Entity entity) => this.GetStepSoundIndex(entity, tileMap);

    }
}
