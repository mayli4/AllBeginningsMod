using AllBeginningsMod.Common;
using AllBeginningsMod.Utilities;
using Terraria.GameContent;
using Terraria.ID;

namespace AllBeginningsMod.Content.Bosses;

//had a solver but removed since it sucks, new one tbd

public class NightgauntNPC : ModNPC {
    public override string Texture => Helper.PlaceholderTextureKey;

    public Player Target => Main.player[NPC.target];

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

    public override void AI() {
        NPC.TargetClosest(false);

        var targetDelta = Target.Center - NPC.Center;
        _distanceToTarget = targetDelta.Length();
        _directionToTarget = targetDelta / _distanceToTarget;

        NPC.rotation = Utils.AngleLerp(NPC.rotation, _directionToTarget.ToRotation(), 0.05f);
        NPC.velocity += _directionToTarget * 0.05f;
        NPC.velocity *= 0.95f;

        _up = NPC.rotation.ToRotationVector2();
        _right = _up.RotatedBy(MathHelper.PiOver2);

        Vector2 rightShoulderPosition = RightShoulderPosition;
        Vector2 leftShoulderPosition = LeftShoulderPosition;

        if(_handSwapTimer == 0) {
            _handSwapTimer = Main.rand.Next(14, 22);

            var hSpeed = 2f;

            var grabOffset = 160f;
            if(_rightHandSwap) {
                _rightArmTargetPosition = rightShoulderPosition + _directionToTarget * grabOffset;
                NPC.velocity += _right * hSpeed;
            }
            else {
                _leftArmTargetPosition = leftShoulderPosition + _directionToTarget * grabOffset;
                NPC.velocity -= _right * hSpeed;
            }

            _rightHandSwap = !_rightHandSwap;

            NPC.velocity += _directionToTarget * 2.5f;
        }
        else _handSwapTimer -= 1;

        var grabLerpSpeed = 0.15f;
        _rightArmEndPosition = Vector2.Lerp(_rightArmEndPosition, _rightArmTargetPosition, grabLerpSpeed);
        _leftArmEndPosition = Vector2.Lerp(_leftArmEndPosition, _leftArmTargetPosition, grabLerpSpeed);

        _rightArm.Update(rightShoulderPosition, _rightArmEndPosition);
        _leftArm.Update(leftShoulderPosition, _leftArmEndPosition);
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