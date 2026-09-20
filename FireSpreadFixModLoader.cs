using HarmonyLib;

namespace Game {
    public class FireSpreadFixModLoader : ModLoader {
        public const string PackageName = "FireSpreadFix";

        public override void __ModInitialize() {
            Harmony harmony = new Harmony(PackageName);
            harmony.PatchAll();
        }
    }
}
