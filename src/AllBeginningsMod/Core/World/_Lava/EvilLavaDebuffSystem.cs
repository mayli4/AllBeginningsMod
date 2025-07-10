using AllBeginningsMod.Content.Biomes;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Core.World;

public sealed class EvilLavaDebuffSystem : ModPlayer {
    public override void PostUpdateBuffs() {
        if(Player.lavaWet && Player.InModBiome<UnderworldCorruptionBiome>()) {
            Player.AddBuff(BuffID.CursedInferno, 7 * 60);
        }
    }
}