using MonoMod.ModInterop;
using System;

namespace Celeste.Mod.FancyTileEntities.ModInterop {
    /// <summary>
    /// Imports methods defined <see href="https://gist.github.com/swoolcock/c0d2a708a393c2c762ad8abf614a941b">GravityHelperExports</see>.
    /// </summary>
    [ModImportName("GravityHelper")]
    public static class GravityHelper {
        public static void Load() {
            typeof(GravityHelper).ModInterop();
            RegisterModSupportBlacklist?.Invoke("FancyTileEntities");
        }

        public static Action<string> RegisterModSupportBlacklist;

        public static Action BeginOverride;
        public static Action EndOverride;

        public static Func<bool> IsActorInverted;


    }
}