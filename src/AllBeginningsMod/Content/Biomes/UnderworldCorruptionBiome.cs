using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.Biomes;

internal sealed class UnderworldCorruptionBiome : ModBiome {
    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
    public override float GetWeight(Player player) => 0.75f;

    public override int Music => MusicID.UndergroundCorruption;
    
    public override bool IsBiomeActive(Player player) {
        var underworld = player.ZoneUnderworldHeight;
        return Main.SceneMetrics.EvilTileCount >= 200 && underworld;
    }
}