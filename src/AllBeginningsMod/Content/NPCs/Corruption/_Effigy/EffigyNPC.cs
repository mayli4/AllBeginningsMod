using AllBeginningsMod.Content.Biomes;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.NPCs.Corruption;
public sealed class EffigyNPC : ModNPC {
    byte _spawnedSprits = 0;

    public override string Texture => Assets.Assets.Textures.NPCs.Corruption.Effigy.KEY_EffigyNPC;
    public override void SetDefaults() {
        NPC.width = 36;
        NPC.height = 36;
        NPC.lifeMax = 640;
        NPC.value = 250f;
        NPC.noTileCollide = false;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.knockBackResist = 0.05f;
        NPC.friendly = false;

        NPC.HitSound = SoundID.NPCHit23;

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];

        NPC.buffImmune[BuffID.CursedInferno] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
        modifiers.Cancel();
    }

    public override void AI() { }
}
