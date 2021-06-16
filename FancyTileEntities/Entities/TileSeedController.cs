using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Celeste.Mod.FancyTileEntities {
    public static class TileSeedController {
        public static void Load() {
            Everest.Events.Level.OnLoadEntity += Level_OnLoadEntity;
            On.Celeste.LevelLoader.LoadingThread += LevelLoader_LoadingThread;
        }

        public static void Unload() {
            Everest.Events.Level.OnLoadEntity -= Level_OnLoadEntity;
            On.Celeste.LevelLoader.LoadingThread -= LevelLoader_LoadingThread;
        }

        private static bool Level_OnLoadEntity(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            if (entityData.Name == "FancyTileEntities/TileSeedController")
                return true;
            return false;
        }

        private static void LevelLoader_LoadingThread(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self) {
            orig(self);
            MapData mapData = self.Level.Session.MapData;

            var controllers = from level in mapData.Levels
                              from entity in level.Entities
                              where entity.Name == "FancyTileEntities/TileSeedController"
                              select new { entity, level };

            Rectangle mapTileBounds = mapData.TileBounds;
            SolidTiles fgTiles = self.Level.SolidTiles;
            BackgroundTiles bgTiles = self.Level.BgTiles;

            Regex regex = new Regex("\\r\\n|\\n\\r|\\n|\\r");

            Autotiler.Behaviour behaviour = new Autotiler.Behaviour {
                EdgesExtend = true,
                EdgesIgnoreOutOfLevel = false,
                PaddingIgnoreOutOfLevel = true
            };

            foreach (var controller in controllers) {
                EntityData data = controller.entity;
                LevelData level = controller.level;

                int randomSeed = data.Int("randomSeed", 42);
                bool positionBased = data.Bool("localPosition", true);

                Rectangle bounds = level.TileBounds;
                Rectangle tileBounds = self.Level.Session.MapData.TileBounds;

                if (data.Bool("fg", true)) {
                    VirtualMap<char> map = new VirtualMap<char>(bounds.Width, bounds.Height, '0');

                    string[] array = regex.Split(level.Solids);
                    for (int y = 0; y < array.Length; y++) {
                        for (int x = 0; x < array[y].Length; x++) { 
                            map[x, y] = array[y][x];
                        }
                    }

                    Calc.PushRandom(randomSeed);
                    Autotiler.Generated gen = Extensions.GenerateOverlay(GFX.FGAutotiler, map, bounds.X - tileBounds.Left, bounds.Y - tileBounds.Top, self.Level.SolidsData, behaviour, positionBased);
                    Calc.PopRandom();

                    int left = bounds.Left;
                    int top = bounds.Top;
                    for (int y = top; y < top + array.Length; y++) {
                        for (int x = left; x < left + array[y - top].Length; x++) {
                            fgTiles.Tiles.Tiles[x - mapTileBounds.Left, y - mapTileBounds.Top] = gen.TileGrid.Tiles[x - left, y - top];
                        }
                    }
                }

                if (data.Bool("bg", true)) {
                    VirtualMap<char> map = new VirtualMap<char>(bounds.Width, bounds.Height, '0');

                    string[] array = regex.Split(level.Bg);
                    for (int y = 0; y < array.Length; y++) {
                        for (int x = 0; x < array[y].Length; x++) {
                            map[x, y] = array[y][x];
                        }
                    }

                    Calc.PushRandom(randomSeed);
                    Extensions.RNGSeed = randomSeed;
                    Autotiler.Generated gen = Extensions.GenerateOverlay(GFX.BGAutotiler, map, bounds.X - tileBounds.Left, bounds.Y - tileBounds.Top, self.Level.BgData, behaviour, positionBased);
                    Calc.PopRandom();

                    int left = bounds.Left;
                    int top = bounds.Top;
                    for (int y = top; y < top + array.Length; y++) {
                        for (int x = left; x < left + array[y - top].Length; x++) {
                            bgTiles.Tiles.Tiles[x - mapTileBounds.Left, y - mapTileBounds.Top] = gen.TileGrid.Tiles[x - left, y - top];
                        }
                    }
                }

            }
        }
    }
}
