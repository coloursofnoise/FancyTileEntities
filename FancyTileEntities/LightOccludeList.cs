using Monocle;

namespace Celeste.Mod.FancyTileEntities {

    [Tracked]
    public class LightOccludeList : Component {
        public LightOccludeList(bool active, bool visible) : base(active, visible) {
        }
    }
}
