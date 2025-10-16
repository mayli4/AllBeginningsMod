using AllBeginningsMod.Common;
using AllBeginningsMod.Utilities;
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

    readonly static Vector2 ShoulderOffset = new(50, 20);
    Vector2 RightShoulderPosition => NPC.Center + _right * ShoulderOffset.X + _up * ShoulderOffset.Y;
    Vector2 LeftShoulderPosition => NPC.Center - _right * ShoulderOffset.X + _up * ShoulderOffset.Y;
    
    readonly static Vector2 LegOffset = new(35, -120);
    Vector2 RightLegBasePosition => NPC.Center + _right * LegOffset.X + _up * LegOffset.Y;
    Vector2 LeftLegBasePosition => NPC.Center - _right * LegOffset.X + _up * LegOffset.Y;
    
    Vector2 _rightArmTargetPosition;
    Vector2 _rightArmEndPosition;
    bool _rightArmAnchored;

    Vector2 _leftArmTargetPosition;
    Vector2 _leftArmEndPosition;
    bool _leftArmAnchored;
    
    Vector2 _rightLegTargetPosition;
    Vector2 _rightLegEndPosition;
    bool _rightLegAnchored;

    Vector2 _leftLegTargetPosition;
    Vector2 _leftLegEndPosition;
    bool _leftLegAnchored;
    
    private NightgauntLimb _rightArm;
    private NightgauntLimb _leftArm;
    private NightgauntLimb _rightLeg;
    private NightgauntLimb _leftLeg;

    int _handSwapTimer;
    bool _rightHandSwap;
    
    int _legSwapTimer;
    bool _rightLegSwap;

    public override void SetDefaults() {
        NPC.width = 30;
        NPC.height = 40;
        NPC.damage = 15;
        NPC.defense = 8;
        NPC.lifeMax = 150;
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
    }

    private void ResetState() {
        State = 0;
        NPC.netUpdate = true;
    }

    public override void AI() {
        NPC.TargetClosest(false);

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