using Terraria.ID;

namespace AllBeginningsMod.Content.NPCs;

public class TestSpecialDialogueNPC : ModNPC {
    public override string Texture => Textures.Items.Misc.UrnOfGreed.KEY_ClayUrnProj;

    public override void SetStaticDefaults() {
        NPC.netAlways = true;
        NPC.friendly = true;
        NPC.width = 64;
        NPC.height = 64;
        NPC.aiStyle = -1;
        NPC.damage = 10;
        NPC.defense = 15;
        NPC.lifeMax = 250;
        NPC.dontTakeDamage = true;
        NPC.immortal = true;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0;
    }

    public override void AI() {
        
    }

    public override bool CanChat() {
        return true;
    }
    
    public override string GetChat() {
        return "ok";
    }
}