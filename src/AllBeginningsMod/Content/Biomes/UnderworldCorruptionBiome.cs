using AllBeginningsMod.Core.World;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.Biomes;

public sealed class UnderworldCorruptionBiome : ModBiome, IHasCustomLavaBiome {
    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
    public override float GetWeight(Player player) => 0.75f;

    public override int Music => MusicID.UndergroundCorruption;

    public LavaStyle LavaStyle => new UnderworldCorruptLava();
    
    public override bool IsBiomeActive(Player player) {
        var underworld = player.ZoneUnderworldHeight;
        return EvilTileCountSystem.InUnderworldCorruption && underworld;
    }
}