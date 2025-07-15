using AllBeginningsMod.Content.Biomes;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace AllBeginningsMod.Core.World;

internal sealed class HellBiomeExclusionGlobalNPC : ModSystem {
    public override void Load() {
        IL_NPC.SpawnNPC += NPCSpawningEdit;
    }

    public override void Unload() {
        IL_NPC.SpawnNPC -= NPCSpawningEdit;
    }
    
    //removes vanilla npcs from hell spawn pool while in an evil biome
    private void NPCSpawningEdit(ILContext il) {
        ILCursor c = new ILCursor(il);
        ILLabel IL_10d3d = null;
        c.TryGotoNext(MoveType.After,
            i => i.MatchBr(out _),
            i => i.MatchLdloc(5),
            i => i.MatchLdsfld<Main>("maxTilesY"),
            i => i.MatchLdcI4(190),
            i => i.MatchSub(),
            i => i.MatchBle(out IL_10d3d));
        
        c.EmitLdloc(14);
        c.EmitDelegate((int k) =>
        {
            return !Main.player[k].InModBiome<UnderworldCorruptionBiome>();
        });
        c.EmitBrfalse(IL_10d3d);
    }
}