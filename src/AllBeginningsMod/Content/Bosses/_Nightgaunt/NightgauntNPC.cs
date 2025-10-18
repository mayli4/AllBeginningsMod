using AllBeginningsMod.Common;
using AllBeginningsMod.Utilities;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace AllBeginningsMod.Content.Bosses;

[AutoloadBossHead]
internal partial class NightgauntNPC : ModNPC {
    internal enum NightgauntState {
        Idle,
        Crawling,
    }

    internal enum Attack {
        SwingLunge
    }

    [StructLayout(LayoutKind.Explicit, Size = sizeof(float) * 4)]
    internal struct NightgauntAi() {
        [FieldOffset(0)] public NightgauntState State;
    }
    
    public override string Texture => Textures.NPCs.Bosses.Nightgaunt.KEY_NightgauntNPC;

    public Player Target => Main.player[NPC.target];
    
    private NightgauntState State {
        get => (NightgauntState)NPC.ai[0];
        set {
            NPC.ai[0] = (int)value;
            NPC.netUpdate = true;
        }
    }

    float _distanceToTarget;
    Vector2 _directionToTarget;

    Vector2 _up;
    Vector2 _right;
    Vector2 _left;

    public override void SetDefaults() {
        NPC.width = 180;
        NPC.height = 140;
        NPC.damage = 15;
        NPC.defense = 8;
        NPC.lifeMax = 1500;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.value = 100f;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.boss = true;
    }

    public override void OnSpawn(IEntitySource source) {
        CreateLimbs();
        
        LegOffset = new(-15, -40);
        ShoulderOffset = new(-38, -30);
    }

    private void ResetState() {
        State = 0;
        NPC.netUpdate = true;
    }

    public override void AI() {
        NPC.TargetClosest(false);
        ref NightgauntAi ai = ref Unsafe.As<float, NightgauntAi>(ref NPC.ai[0]);

        if(_leftLeg == default || _rightLeg == default || _leftArm == default || _rightArm == default) {
            CreateLimbs();
        }

        switch(State) {
            case NightgauntState.Idle:
                break;
            case NightgauntState.Crawling:
                CrawlToPlayer();
                break;
        }
        
        State = NightgauntState.Crawling;
    }
}