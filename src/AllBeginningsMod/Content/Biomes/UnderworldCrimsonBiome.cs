using AllBeginningsMod.Core.World;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.Biomes;

public sealed class UnderworldCrimsonBiome : ModBiome, IHasCustomLavaBiome {
    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
    public override float GetWeight(Player player) => 0.75f;

    public override int Music => MusicID.UndergroundCrimson;

    public ModLavaStyle ModLavaStyle => ModContent.GetInstance<IchorModLava>();
    
    public override bool IsBiomeActive(Player player) {
        var underworld = player.ZoneUnderworldHeight;
        return EvilTileCountSystem.InUnderworldCrimson && underworld;
    }
}