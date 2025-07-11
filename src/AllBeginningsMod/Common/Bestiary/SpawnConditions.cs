using Terraria.GameContent;

namespace AllBeginningsMod.Common.Bestiary;

public static class SpawnConditions {
    public static ModBiomeSpawnCondition UnderworldCorruption = new(
        "Underworld Corruption", 
        Assets.Assets.Textures.Misc.KEY_UnderworldCorruptionIcon, 
        Assets.Assets.Textures.Backgrounds.UnderworldCorruption.KEY_UnderworldCorruptionMapBG
        );
}