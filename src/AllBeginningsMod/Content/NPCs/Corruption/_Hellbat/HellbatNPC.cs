using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.NPCs.Corruption;

public class HellbatNPC : ModNPC {
    public enum States {
        IdleOnCeiling,
        WakingUp,
        Awake,
    }

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 10;
    }

    public override void SetDefaults() {
        NPC.width = 20;
        NPC.height = 20;
        NPC.lifeMax = 100;
        NPC.value = 250f;
        NPC.noTileCollide = false;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.knockBackResist = 0.05f;
        NPC.friendly = false;
    }
}