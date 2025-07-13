using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace AllBeginningsMod.Content.NPCs.Corruption;

public class HellbatNPC : ModNPC {
    public override string Texture => Assets.Assets.Textures.NPCs.Corruption.Hellbat.KEY_HellbatNPC;
    
    public enum State {
        IdleOnCeiling,
        WakingUp,
        Awake,
        Dashing,
        Spitting,
    }

    public State CurrentState {
        get => (State)NPC.ai[0];
        set => NPC.ai[0] = (float)value;
    }

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 10;
    }

    public override void SetDefaults() {
        NPC.width = 40;
        NPC.height = 30;
        NPC.lifeMax = 150;
        NPC.damage = 30;
        NPC.defense = 10;
        NPC.value = Item.buyPrice(0, 0, 50, 0);
        NPC.noTileCollide = false;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.knockBackResist = 0.05f;
        NPC.friendly = false;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath2;
    }
}