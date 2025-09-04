using Terraria.ID;

namespace AllBeginningsMod.Content.NPCs.Bosses;

[AutoloadBossHead]
public class NightgauntNPC : ModNPC {
    public override string Texture => Textures.NPCs.Bosses.Nightgaunt.KEY_NightgauntNPC;
    
    public override void SetDefaults() {
        NPC.width = 150;
        NPC.height = 175;
        NPC.knockBackResist = 0f;

        NPC.defense = 10;
        NPC.damage = 6;
        NPC.lifeMax = 999999999;

        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = false;
        NPC.friendly = false;

        NPC.alpha = 0;
        NPC.aiStyle = -1;

        NPC.npcSlots = 40f;
        NPC.HitSound = SoundID.NPCHit2;
    }
}