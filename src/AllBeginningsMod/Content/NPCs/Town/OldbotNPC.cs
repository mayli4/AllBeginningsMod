using Terraria.ID;

namespace AllBeginningsMod.Content.NPCs.Town;

public class OldbotNPC : ModNPC {
    public override string Texture => Textures.NPCs.Town.KEY_OldbotNPC;

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 12;
    }
    
    public override void SetDefaults() {
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.width = 18;
        NPC.height = 40;
        NPC.aiStyle = -1;
        NPC.damage = 10;
        NPC.defense = 15;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0.5f;
        NPC.rarity = 1;
    }

    public override void AI() {
        //todo
    }
}