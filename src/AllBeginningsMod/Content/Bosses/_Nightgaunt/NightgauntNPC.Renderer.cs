using AllBeginningsMod.Common.Rendering;
using AllBeginningsMod.Utilities;
using Terraria.GameContent;

namespace AllBeginningsMod.Content.Bosses;

internal class NightgauntDistortion : ILoadable {
    public static ManagedRenderTarget Target;
    
    public void Load(Mod mod) {
        Main.QueueMainThreadAction(() => {
            Target = new ManagedRenderTarget(InitNPCMapTarget, true);
            Target.Initialize(Main.screenWidth, Main.screenHeight);
        });
    }
    
    private RenderTarget2D InitNPCMapTarget(int width, int height) {
        return new RenderTarget2D(Main.instance.GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
    }

    public void Unload() {
        
    }
}

internal partial class NightgauntNPC {

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if(NPC.IsABestiaryIconDummy) return false;

        var gd = Main.graphics.graphicsDevice;
        var bindings = RtContentPreserver.GetAndPreserveMainRTs();
        
        Main.spriteBatch.End(out var ss);
        Main.instance.GraphicsDevice.SetRenderTarget(NightgauntDistortion.Target.Value);
        Main.instance.GraphicsDevice.Clear(Color.Transparent);
        Main.spriteBatch.Begin(ss with { SortMode = SpriteSortMode.Immediate, TransformMatrix = Main.GameViewMatrix.EffectMatrix });
        
        DrawBody(ref _body, drawColor, SpriteEffects.None);
        DrawHeadNeck(ref _headNeck, drawColor, SpriteEffects.FlipVertically);

        DrawArm(ref _rightArm, true, drawColor, SpriteEffects.FlipHorizontally);
        DrawArm(ref _leftArm, false, drawColor, SpriteEffects.None);

        DrawLeg(ref _rightLeg, true, drawColor, SpriteEffects.FlipHorizontally);
        DrawLeg(ref _leftLeg, false, drawColor, SpriteEffects.None);
        
        Main.spriteBatch.End(out ss);
        
        Main.instance.GraphicsDevice.SetRenderTargets(bindings);
        Main.spriteBatch.Begin(ss with { TransformMatrix = Main.Transform });
        //spriteBatch.Draw(NightgauntDistortion.Target.Value, Vector2.Zero, Color.Red);

        Graphics.BeginPipeline(1.0f)
            .DrawBasicTrail(
            _tailPoints.ToArray(),
            static t => (1.25f - t) * 40f,
            Textures.NPCs.Bosses.Nightgaunt.NightgauntTail.Value,
            drawColor)
            .ApplyOutline(Color.White)
            .Flush();
        
        Graphics.BeginPipeline(1.0f)
            .DrawSprite(NightgauntDistortion.Target.Value, Vector2.Zero, drawColor)
            .ApplyOutline(Color.White)
            .Flush();
        
        return false;
    }

    static void DrawArm(ref NightgauntLimb limb, bool right, Color drawColor, SpriteEffects effects) {
        var armTexture = Textures.NPCs.Bosses.Nightgaunt.NightgauntNPC.Value;

        Vector2 shoulder = limb.Skeleton.Position(0);
        Vector2 elbow = limb.Skeleton.Position(1);
        Vector2 wrist = limb.Skeleton.Position(2);
        Vector2 handTip = limb.Skeleton.Position(3);

        var upperArmFrame = new Rectangle(0, 0, 32, 88);
        var forearmDefaultFrame = new Rectangle(0, 94, 32, 62);
        var handFrame = new Rectangle(0, 162, 32, 70);
        var handAnchoredFrame = new Rectangle(0, 238, 32, 70);

        if(right) {
            forearmDefaultFrame = new Rectangle(38, 94, 32, 62);
            handFrame = new Rectangle(38, 162, 32, 70);
        }

        var currentHandFrame = limb.IsAnchored ? handAnchoredFrame : handFrame;

        var upperArmOrigin = new Vector2(14, 6);
        var forearmOrigin = new Vector2(16, 0);
        var handOrigin = right ? new Vector2(16, 0) : new Vector2(24, 0);

        if(limb.IsAnchored) {
            handOrigin = right ? new Vector2(16, 3) : new Vector2(24, 0);
        }

        float rotationOffset = MathHelper.PiOver2;

        Main.spriteBatch.Draw(armTexture, shoulder - Main.screenPosition, upperArmFrame, drawColor, (shoulder - elbow).ToRotation() + rotationOffset, upperArmOrigin, 1f, effects, 0f);
        Main.spriteBatch.Draw(armTexture, elbow - Main.screenPosition, forearmDefaultFrame, drawColor, (elbow - wrist).ToRotation() + rotationOffset, forearmOrigin, 1f, effects, 0f);
        Main.spriteBatch.Draw(armTexture, wrist - Main.screenPosition, currentHandFrame, drawColor, (wrist - handTip).ToRotation() + rotationOffset, handOrigin, 1f, effects, 0f);
    }

    static void DrawLeg(ref NightgauntLimb limb, bool right, Color drawColor, SpriteEffects effects) {
        var legTexture = Textures.NPCs.Bosses.Nightgaunt.NightgauntNPC.Value;

        Vector2 hip = limb.Skeleton.Position(0);
        Vector2 knee = limb.Skeleton.Position(1);
        Vector2 ankle = limb.Skeleton.Position(2);
        Vector2 toeTip = limb.Skeleton.Position(3);

        var thighFrame = new Rectangle(76, 0, 32, 70);
        var calfFrame = new Rectangle(76, 76, 32, 70);
        var footFrame = new Rectangle(76, 152, 32, 70);
        var footAnchoredFrame = new Rectangle(76, 228, 32, 70);

        var thighOrigin = new Vector2(13, 0);
        var calfOrigin = new Vector2(18, 10);
        var footOrigin = new Vector2(14, 0);

        if(right) {
            thighFrame = new Rectangle(114, 0, 32, 70);
            calfFrame = new Rectangle(114, 76, 32, 70);
            footFrame = new Rectangle(114, 152, 32, 70);

            calfOrigin = new Vector2(16, 10);
            footOrigin = new Vector2(24, 6);
        }

        float rotationOffset = MathHelper.PiOver2;

        var currentFootFrame = limb.IsAnchored ? footAnchoredFrame : footFrame;

        Main.spriteBatch.Draw(legTexture, hip - Main.screenPosition, thighFrame, drawColor, (hip - knee).ToRotation() + rotationOffset, thighOrigin, 1f, effects, 0f);
        Main.spriteBatch.Draw(legTexture, knee - Main.screenPosition, calfFrame, drawColor, (knee - ankle).ToRotation() + rotationOffset, calfOrigin, 1f, effects, 0f);
        Main.spriteBatch.Draw(legTexture, ankle - Main.screenPosition, currentFootFrame, drawColor, (ankle - toeTip).ToRotation() + rotationOffset, footOrigin, 1f, effects, 0f);
    }

    static void DrawBody(ref NightgauntLimb body, Color drawColor, SpriteEffects effects) {
        var bodyTexture = Textures.NPCs.Bosses.Nightgaunt.NightgauntNPC.Value;

        Vector2 neck = body.Skeleton.Position(0);
        Vector2 midTorso = body.Skeleton.Position(1);
        Vector2 hips = body.Skeleton.Position(2);
        Vector2 tailEnd = body.Skeleton.Position(3);

        var segment1Frame = new Rectangle(152, 0, 116, 70);
        var segment2Frame = new Rectangle(152, 76, 116, 70);
        var segment3Frame = new Rectangle(152, 152, 116, 70);

        var segment1Origin = new Vector2(60, 0);
        var segment2Origin = new Vector2(60, 0);
        var segment3Origin = new Vector2(60, 0);

        float rotationOffset = MathHelper.PiOver2;

        Main.spriteBatch.Draw(bodyTexture, neck - Main.screenPosition, segment1Frame, drawColor, (neck - midTorso).ToRotation() + rotationOffset, segment1Origin, 1f, effects, 0f);
        Main.spriteBatch.Draw(bodyTexture, midTorso - Main.screenPosition, segment2Frame, drawColor, (midTorso - hips).ToRotation() + rotationOffset, segment2Origin, 1f, effects, 0f);
        Main.spriteBatch.Draw(bodyTexture, hips - Main.screenPosition, segment3Frame, drawColor, (hips - tailEnd).ToRotation() + rotationOffset, segment3Origin, 1f, effects, 0f);
    }

    static void DrawHeadNeck(ref NightgauntLimb headNeck, Color drawColor, SpriteEffects effects) {
        var headNeckTexture = Textures.NPCs.Bosses.Nightgaunt.NightgauntNPC.Value;

        Vector2 neckBase = headNeck.Skeleton.Position(0);
        Vector2 midNeck = headNeck.Skeleton.Position(1);
        Vector2 headPivot = headNeck.Skeleton.Position(2);
        Vector2 headTip = headNeck.Skeleton.Position(3);

        var neckBaseFrame = new Rectangle(276, 90, 34, 40);
        var midNeckFrame = new Rectangle(276, 52, 34, 32);
        var headFrame = new Rectangle(276, 0, 34, 46);
        
        var eyesFrame = new Rectangle(276, 136, 34, 46);

        var neckBaseOrigin = new Vector2(17, 10);
        var midNeckOrigin = new Vector2(17, 10);
        var headOrigin = new Vector2(17, 10);

        float rotationOffset = MathHelper.PiOver2;

        Main.spriteBatch.Draw(headNeckTexture, neckBase - Main.screenPosition, neckBaseFrame, drawColor, (neckBase - midNeck).ToRotation() + rotationOffset, neckBaseOrigin, 1f, effects, 0f);
        Main.spriteBatch.Draw(headNeckTexture, midNeck - Main.screenPosition, midNeckFrame, drawColor, (midNeck - headPivot).ToRotation() + rotationOffset, midNeckOrigin, 1f, effects, 0f);
        Main.spriteBatch.Draw(headNeckTexture, headPivot - Main.screenPosition, headFrame, drawColor, (headPivot - headTip).ToRotation() + rotationOffset, headOrigin, 1f, effects, 0f);
        
        Main.spriteBatch.Draw(headNeckTexture, headPivot - Main.screenPosition, eyesFrame, Color.White, (headPivot - headTip).ToRotation() + rotationOffset, headOrigin, 1f, effects, 0f);
    }
}