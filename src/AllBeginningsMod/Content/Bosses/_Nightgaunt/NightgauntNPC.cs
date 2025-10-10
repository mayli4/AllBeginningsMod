using AllBeginningsMod.Common;
using AllBeginningsMod.Utilities;
using Terraria.GameContent;
using Terraria.ID;

namespace AllBeginningsMod.Content.Bosses;

internal partial class NightgauntNPC : ModNPC {
    internal enum NightgauntState {
        Idle,
        Crawling,
    }
    
    public override string Texture => Helper.PlaceholderTextureKey;

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

    readonly static Vector2 ShoulderOffset = new(50, 20);
    Vector2 RightShoulderPosition => NPC.Center + _right * ShoulderOffset.X + _up * ShoulderOffset.Y;
    Vector2 LeftShoulderPosition => NPC.Center - _right * ShoulderOffset.X + _up * ShoulderOffset.Y;

    IKSkeleton _rightArm;
    Vector2 _rightArmTargetPosition;
    Vector2 _rightArmEndPosition;

    IKSkeleton _leftArm;
    Vector2 _leftArmTargetPosition;
    Vector2 _leftArmEndPosition;

    int _handSwapTimer;
    bool _rightHandSwap;

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

        _rightArm = new((36f, new()), (60f, new() { MinAngle = -MathHelper.Pi, MaxAngle = 0f }));
        _leftArm = new((36f, new()), (60f, new() { MinAngle = 0f, MaxAngle = MathHelper.Pi }));
    }

    private void ResetState() {
        State = 0;
        NPC.netUpdate = true;
    }

    public override void AI() {
        NPC.TargetClosest(false);

        switch(State) {
            case NightgauntState.Idle:
                break;
            case NightgauntState.Crawling:
                CrawlToPlayer();
                break;
        }
        
        State = NightgauntState.Crawling;
    }

    static void DrawArm(Vector2 a, Vector2 b, Vector2 c, Color drawColor, SpriteEffects effects) {
        var armTexture = Textures.NPCs.Bosses.Nightgaunt.NightgauntArm.Value;
        Main.spriteBatch.Draw(
            armTexture,
            a - Main.screenPosition,
            new Rectangle(94, 0, armTexture.Width - 94, armTexture.Height),
            drawColor,
            (a - b).ToRotation(),
            new(134 - 94, 12),
            1f,
            effects,
            0f
        );

        Main.spriteBatch.Draw(
            armTexture,
            b - Main.screenPosition,
            new Rectangle(0, 0, 84, armTexture.Height),
            drawColor,
            (b - c).ToRotation(),
            new(72, 14),
            1f,
            effects,
            0f
        );
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if(NPC.IsABestiaryIconDummy) return false;
        DrawArm(_rightArm.Position(0), _rightArm.Position(1), _rightArm.Position(2), drawColor, SpriteEffects.FlipVertically);
        DrawArm(_leftArm.Position(0), _leftArm.Position(1), _leftArm.Position(2), drawColor, SpriteEffects.None);

#if DEBUG
        spriteBatch.DrawLine(
            NPC.Center - Main.screenPosition,
            NPC.Center + _up * 40f - Main.screenPosition,
            Color.Red,
            4,
            TextureAssets.BlackTile.Value
        );

        spriteBatch.DrawLine(
            NPC.Center - Main.screenPosition,
            NPC.Center + _right * 25f - Main.screenPosition,
            Color.Yellow,
            4,
            TextureAssets.BlackTile.Value
        );

        // spriteBatch.DrawLine(
        //     _rightArm.Position(0) - Main.screenPosition,
        //     _rightArm.Position(1) - Main.screenPosition,
        //     Color.Blue,
        //     4,
        //     TextureAssets.BlackTile.Value
        // );

        // spriteBatch.DrawLine(
        //     _rightArm.Position(1) - Main.screenPosition,
        //     _rightArm.Position(2) - Main.screenPosition,
        //     Color.Red,
        //     4,
        //     TextureAssets.BlackTile.Value
        // );

        // spriteBatch.DrawLine(
        //     _leftArm.Position(0) - Main.screenPosition,
        //     _leftArm.Position(1) - Main.screenPosition,
        //     Color.Blue,
        //     4,
        //     TextureAssets.BlackTile.Value
        // );

        // spriteBatch.DrawLine(
        //     _leftArm.Position(1) - Main.screenPosition,
        //     _leftArm.Position(2) - Main.screenPosition,
        //     Color.Red,
        //     4,
        //     TextureAssets.BlackTile.Value
        // );
#endif

        return true;
    }
}