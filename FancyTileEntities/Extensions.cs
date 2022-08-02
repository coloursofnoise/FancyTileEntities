using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.FancyTileEntities {
    public static partial class Extensions {
        private static readonly Type Autotiler_Tiles;
        private static readonly Type Autotiler_TerrainType;
        private static readonly FieldInfo<List<MTexture>> f_Tiles_Textures;
        private static readonly FieldInfo<bool> f_Tiles_HasOverlays;
        private static readonly FieldInfo<List<string>> f_Tiles_OverlapSprites;

        private static readonly MethodInfo m_TileHandler;
        private static readonly MethodInfo m_TerrainType_Ignore;

        private static bool usingCustomAutotiler;
        private static VirtualMap<char> forceData_CustomAutotiler;
        private static Point startPoint_CustomAutotiler;

        static Extensions() {
            Autotiler_Tiles = typeof(Autotiler).GetNestedType("Tiles", BindingFlags.NonPublic);
            Autotiler_TerrainType = typeof(Autotiler).GetNestedType("TerrainType", BindingFlags.NonPublic);
            f_Tiles_Textures = Autotiler_Tiles.GetField<List<MTexture>>("Textures");
            f_Tiles_HasOverlays = Autotiler_Tiles.GetField<bool>("HasOverlays");
            f_Tiles_OverlapSprites = Autotiler_Tiles.GetField<List<string>>("OverlapSprites");

            m_TileHandler = typeof(Autotiler).GetMethod("TileHandler", BindingFlags.NonPublic | BindingFlags.Instance);
            m_TerrainType_Ignore = Autotiler_TerrainType.GetMethod("Ignore");

            usingCustomAutotiler = false;
        }

        public static void Load() {
            On.Celeste.Autotiler.CheckTile += Autotiler_CheckTile;
        }

        public static void Unload() {
            On.Celeste.Autotiler.CheckTile -= Autotiler_CheckTile;
        }

        public static IEnumerable<T> GetEnumerator<T>(this VirtualMap<T> map) {
            for (int y = 0; y < map.Rows; y++)
                for (int x = 0; x < map.Columns; x++)
                    yield return map[x, y];
        }

        public static void CopyInto<T>(this VirtualMap<T> src, VirtualMap<T> dest, Point position, bool blindCopy = false) {
            if (!blindCopy && (dest.Rows < src.Rows - position.Y || dest.Columns < src.Columns - position.X)) {
                throw new IndexOutOfRangeException("Destination not large enough to hold contents of source. \n Set `blindCopy` to true to disregard this message.");
            }
            if (blindCopy) {
                for (int x = 0; x < Math.Min(src.Columns, dest.Columns); x++) {
                    for (int y = 0; y < Math.Min(src.Rows, dest.Rows); y++) {
                        dest[position.X + x, position.Y + y] = src[x, y];
                    }
                }
            } else {
                for (int x = 0; x < src.Columns; x++) {
                    for (int y = 0; y < src.Rows; y++) {
                        dest[position.X + x, position.Y + y] = src[x, y];
                    }
                }
            }
        }
        public static void CopyInto<T>(this VirtualMap<T> src, VirtualMap<T> dest, bool blindCopy = false) {
            src.CopyInto(dest, Point.Zero, blindCopy);
        }

        public static ColliderList GenerateInefficientColliderGrid(VirtualMap<char> tileMap, int cellWidth, int cellHeight) {
            ColliderList colliders = new ColliderList();
            for (int x = 0; x < tileMap.Columns; x++) {
                for (int y = 0; y < tileMap.Rows; y++) {
                    if (tileMap.AnyInSegmentAtTile(x, y) && tileMap[x, y] != '0') {
                        colliders.Add(new Hitbox(cellWidth, cellHeight, x * cellWidth, y * cellHeight));
                    }
                }
            }
            return colliders.colliders.Length > 0 ? colliders : null;
        }

        public static ColliderList GenerateSemiEfficientColliderGrid(VirtualMap<char> tileMap, int cellWidth, int cellHeight) {
            ColliderList colliders = new ColliderList();
            for (int x = 0; x < tileMap.Columns; x++) {
                for (int y = 0; y < tileMap.Rows; y++) {
                    if (tileMap.AnyInSegmentAtTile(x, y) && tileMap[x, y] != '0') {
                        int width = 1;
                        int height = 1;
                        Hitbox h = new Hitbox(cellWidth, cellHeight, x, y);
                        while (tileMap.AnyInSegmentAtTile(x, y) && tileMap[x, y] != '0') {
                            width++;
                            height++;
                            x++;
                            y++;
                        }
                        h.Width *= width;
                        h.Height *= height;
                        colliders.Add(h);
                    }
                }
            }
            return colliders.colliders.Length > 0 ? colliders : null;
        }

        public static ColliderList GenerateBetterColliderGrid(VirtualMap<char> tileMap, int cellWidth, int cellHeight) {
            ColliderList colliders = new ColliderList();
            List<Hitbox> prevCollidersOnX = new List<Hitbox>();
            Hitbox prevCollider = null;

            void ExtendOrAdd() {
                bool extendedOnX = false;
                foreach (Hitbox hitbox in prevCollidersOnX) {
                    if (hitbox.Position.X + hitbox.Width == prevCollider.Position.X &&
                       hitbox.Position.Y == prevCollider.Position.Y &&
                       hitbox.Height == prevCollider.Height) {
                        // Weird check, but hey.
                        extendedOnX = true;
                        hitbox.Width += cellWidth;
                        prevCollider = null;
                        break;
                    }
                }
                if (!extendedOnX) {
                    colliders.Add(prevCollider);
                    prevCollidersOnX.Add(prevCollider);
                    prevCollider = null;
                }
            }

            for (int x = 0; x < tileMap.Columns; x++) {
                for (int y = 0; y < tileMap.Rows; y++) {
                    if (tileMap.AnyInSegmentAtTile(x, y) && tileMap[x, y] != '0') {
                        if (prevCollider == null)
                            prevCollider = new Hitbox(cellWidth, cellHeight, x * cellWidth, y * cellHeight);
                        else
                            prevCollider.Height += cellHeight;

                    } else if (prevCollider != null) {
                        ExtendOrAdd();
                    }
                }

                if (prevCollider != null) {
                    ExtendOrAdd();
                }

            }
            return colliders.colliders.Length > 0 ? colliders : null;
        }

        public static void AddLightOcclude(Entity entity, ColliderList colliders, float alpha = 1f) {
            foreach (Hitbox hitbox in colliders.colliders)
                entity.Add(new LightOcclude(new Rectangle((int) hitbox.Position.X, (int) hitbox.Position.Y, (int) hitbox.Width, (int) hitbox.Height), alpha));
        }

        public static VirtualMap<char> GenerateTileMap(string tileMap) {
            if (string.IsNullOrWhiteSpace(tileMap))
                throw new ArgumentException("Attempted to generate a TileMap with no tiles in it!");

            // Backwards compatibility, tileMap strings previously used `,` as the row separator
            char delim = tileMap.Contains(',') ? ',' : '\n';

            string[] tileStrings = tileMap.Split(delim);
            tileStrings = Array.ConvertAll(tileStrings, s => s.Trim());

            int columns = tileStrings.Max(s => s.Length);
            int rows = tileStrings.Length;

            tileStrings = Array.ConvertAll(tileStrings, s => {
                while (s.Length < columns) {
                    s += '0';
                }
                return s;
            });

            char[,] tileData = new char[columns, rows];
            for (int x = 0; x < columns; x++) {
                for (int y = 0; y < rows; y++) {
                    tileData[x, y] = tileStrings[y][x];
                }
            }
            return new VirtualMap<char>(tileData);
        }

        public static Autotiler.Generated GenerateOverlay(this Autotiler tiler, VirtualMap<char> tileMap, int x, int y, VirtualMap<char> mapData) {
            Autotiler.Behaviour behaviour = new Autotiler.Behaviour {
                EdgesExtend = true,
                EdgesIgnoreOutOfLevel = true,
                PaddingIgnoreOutOfLevel = true
            };
            return tiler.Generate(mapData, x, y, tileMap, behaviour);
        }
        public static Autotiler.Generated GenerateOverlay(this Autotiler tiler, VirtualMap<char> tileMap, int x, int y, VirtualMap<char> mapData, Autotiler.Behaviour behaviour) {
            return tiler.Generate(mapData, x, y, tileMap, behaviour);
        }
        private static Autotiler.Generated Generate(this Autotiler tiler, VirtualMap<char> mapData, int startX, int startY, VirtualMap<char> forceData, Autotiler.Behaviour behaviour) {
            usingCustomAutotiler = true;
            forceData_CustomAutotiler = forceData;
            startPoint_CustomAutotiler = new Point(startX, startY);

            TileGrid tileGrid = new TileGrid(8, 8, forceData.Columns, forceData.Rows);
            AnimatedTiles animatedTiles = new AnimatedTiles(forceData.Columns, forceData.Rows, GFX.AnimatedTilesBank);
            Rectangle empty = new Rectangle(startX, startY, forceData.Columns, forceData.Rows);
            for (int i = startX; i < startX + forceData.Columns; i += 50) {
                for (int j = startY; j < startY + forceData.Rows; j += 50) {
                    if (!mapData.AnyInSegmentAtTile(i, j)) {
                        j = j / 50 * 50;
                    } else {
                        int k = i;
                        int num = Math.Min(i + 50, startX + forceData.Columns);
                        while (k < num) {
                            int l = j;
                            int num2 = Math.Min(j + 50, startY + forceData.Rows);
                            while (l < num2) {
                                object tiles = m_TileHandler.Invoke(tiler, new object[] { mapData, k, l, empty, forceData[k - startX, l - startY], behaviour });
                                if (tiles != null) {
                                    (Calc.Random as PositionRandom)?.SetPosition(k - startX, l - startY); // Positional Randomization for TileSeedController
                                    tileGrid.Tiles[k - startX, l - startY] = Calc.Random.Choose(f_Tiles_Textures.GetValue(tiles));
                                    if (f_Tiles_HasOverlays.GetValue(tiles)) {
                                        animatedTiles.Set(k - startX, l - startY, Calc.Random.Choose(f_Tiles_OverlapSprites.GetValue(tiles)), 1f, 1f);
                                    }
                                }
                                l++;
                            }
                            k++;
                        }
                    }
                }
            }
            usingCustomAutotiler = false;
            return new Autotiler.Generated {
                TileGrid = tileGrid,
                SpriteOverlay = animatedTiles
            };
        }

        private static bool Autotiler_CheckTile(On.Celeste.Autotiler.orig_CheckTile orig, Autotiler self, object set, VirtualMap<char> mapData, int x, int y, Rectangle forceFill, Autotiler.Behaviour behaviour) {
            if (usingCustomAutotiler) {
                Point origin = startPoint_CustomAutotiler;
                char c = forceData_CustomAutotiler[x - origin.X, y - origin.Y];
                if (IsEmpty(c)) {
                    forceFill = Rectangle.Empty;
                } else if ((bool) m_TerrainType_Ignore.Invoke(set, new object[] { c })) {
                    // Hack to make sure `ignores` attributes are respected
                    return false;
                }
            }
            return orig(self, set, mapData, x, y, forceFill, behaviour);
        }

        public static bool IsEmpty(char id) {
            return id == '0' || id == '\0';
        }

        public static EntityData Modify(this EntityData data, Action<EntityData> processor) {
            processor(data);
            return data;
        }

    }
}
