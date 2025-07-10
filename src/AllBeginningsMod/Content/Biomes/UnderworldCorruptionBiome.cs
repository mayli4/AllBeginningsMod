using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.Biomes;

public sealed class UnderworldCorruptionBiome : ModBiome {
    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
    public override float GetWeight(Player player) => 0.75f;

    public override int Music => MusicID.UndergroundCorruption;
    
    public override bool IsBiomeActive(Player player) {
        var underworld = player.ZoneUnderworldHeight;
        return Main.SceneMetrics.EvilTileCount >= 200 && underworld;
    }
}

internal sealed class UnderworldCorruptionSystem : ModSystem {
    public override void ModifyLightingBrightness(ref float scale) {
        base.ModifyLightingBrightness(ref scale);
    }
}