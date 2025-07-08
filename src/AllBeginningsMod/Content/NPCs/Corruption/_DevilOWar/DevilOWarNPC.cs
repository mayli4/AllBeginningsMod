using AllBeginningsMod.Common.PrimitiveDrawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.NPCs.Corruption;

public enum State {
    Idle,
    Charging
}

public sealed class DevilOWarNPC : ModNPC {
    public override string Texture => Assets.Assets.Textures.NPCs.Corruption.DevilOWar.KEY_DevilOWarHead;

    public State Phase {
        get => (State)NPC.ai[0];
        set => NPC.ai[0] = (float)value;
    }

    private Vector2 _targetPosition;
    private int _idleTimer;
    
    public override void SetDefaults() {
        NPC.width = 150;
        NPC.height = 175;
        NPC.knockBackResist = 0f;

        NPC.defense = 10;
        NPC.damage = 60;
        NPC.lifeMax = 360;

        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = false;
        NPC.friendly = false;

        NPC.alpha = 0;
        NPC.aiStyle = -1;

        NPC.npcSlots = 40f;
        NPC.HitSound = SoundID.NPCHit1;
    }

    public override void AI() {
        base.AI();
    }

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        base.PostDraw(spriteBatch, screenPos, drawColor);
    }
}