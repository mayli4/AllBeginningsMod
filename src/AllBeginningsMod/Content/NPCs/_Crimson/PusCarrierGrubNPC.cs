using AllBeginningsMod.Content.Biomes;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.NPCs;

public class PusCarrierGrubNPC : ModNPC {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.PusCarrier.KEY_PusCarrierGrub;
    
    public override void SetDefaults() {
        NPC.width = 32;
        NPC.height = 32;
        NPC.lifeMax = 640;
        NPC.value = 250f;
        NPC.noTileCollide = true;
        NPC.aiStyle = -1;
        NPC.noGravity = false;
        NPC.knockBackResist = 0.05f;
        NPC.friendly = false;
        NPC.damage = 20;

        NPC.HitSound = SoundID.NPCHit23;

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCrimsonBiome>().Type];

        NPC.buffImmune[BuffID.Ichor] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;
    }

    public override void AI() {
        
    }
}