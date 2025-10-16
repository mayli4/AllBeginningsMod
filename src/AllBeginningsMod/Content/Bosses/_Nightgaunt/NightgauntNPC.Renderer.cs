using AllBeginningsMod.Utilities;
using Terraria.GameContent;

namespace AllBeginningsMod.Content.Bosses;

internal partial class NightgauntNPC {
    static void DrawArm(ref NightgauntLimb nightgauntLimb, Color drawColor, SpriteEffects effects) {
        var armTexture = Textures.NPCs.Bosses.Nightgaunt.NightgauntArm.Value;
        var defaultForearmFrame = new Rectangle(0, 0, 84, 32);
        var anchoredForearmFrame = new Rectangle(0, 32, 84, 32);
        
        var currentFrame = nightgauntLimb.IsAnchored ? anchoredForearmFrame : defaultForearmFrame;

        Main.spriteBatch.Draw(
            armTexture,
            nightgauntLimb.Skeleton.Position(0) - Main.screenPosition,
            new Rectangle(94, 0, 48, 24),
            drawColor,
            (nightgauntLimb.Skeleton.Position(0) - nightgauntLimb.Skeleton.Position(1)).ToRotation(),
            new(134 - 94, 12),
            1f,
            effects,
            0f
        );

        Main.spriteBatch.Draw(
            armTexture,
            nightgauntLimb.Skeleton.Position(1) - Main.screenPosition,
            currentFrame,
            drawColor,
            (nightgauntLimb.Skeleton.Position(1) - nightgauntLimb.Skeleton.Position(2)).ToRotation(),
            new Vector2(72, 14),
            1f,
            effects,
            0f
        );
    }

    static void DrawLeg(ref NightgauntLimb nightgauntLimb, Color drawColor, SpriteEffects effects) {
        var legTexture = Textures.NPCs.Bosses.Nightgaunt.NightgauntLeg.Value;
        var defaultLowerLegFrame = new Rectangle(0, 0, 80, 26);
        var anchoredLowerLegFrame = new Rectangle(0, 26, 80, 26);
        
        var currentFrame = nightgauntLimb.IsAnchored ? anchoredLowerLegFrame : defaultLowerLegFrame;
        
        Main.spriteBatch.Draw(
            legTexture,
            nightgauntLimb.Skeleton.Position(0) - Main.screenPosition,
            new Rectangle(84, 0, 66, 26),
            drawColor,
            (nightgauntLimb.Skeleton.Position(0) - nightgauntLimb.Skeleton.Position(1)).ToRotation(),
            new(134 - 74, 12),
            1f,
            effects,
            0f
        );

        Main.spriteBatch.Draw(
            legTexture,
            nightgauntLimb.Skeleton.Position(1) - Main.screenPosition,
            currentFrame,
            drawColor,
            (nightgauntLimb.Skeleton.Position(1) - nightgauntLimb.Skeleton.Position(2)).ToRotation(),
            new(72, 14),
            1f,
            effects,
            0f
        );
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if(NPC.IsABestiaryIconDummy) return false;
        
        DrawArm(ref _rightArm, drawColor, SpriteEffects.FlipVertically);
        DrawArm(ref _leftArm, drawColor, SpriteEffects.None);
        
        DrawLeg(ref _rightLeg, drawColor, SpriteEffects.FlipVertically);
        DrawLeg(ref _leftLeg, drawColor, SpriteEffects.None);

#if DEBUG
        
        spriteBatch.Draw(Textures.Sample.Pixel.Value, _rightArmTargetPosition - screenPos, new Rectangle(0, 0, 5, 5), Color.Lime);
        spriteBatch.Draw(Textures.Sample.Pixel.Value, _rightArmEndPosition - screenPos, new Rectangle(0, 0, 5, 5), Color.Teal);
        
        spriteBatch.Draw(Textures.Sample.Pixel.Value, _leftArmTargetPosition - screenPos, new Rectangle(0, 0, 5, 5), Color.Lime);
        spriteBatch.Draw(Textures.Sample.Pixel.Value, _leftArmEndPosition - screenPos, new Rectangle(0, 0, 5, 5), Color.Teal);
        
        spriteBatch.Draw(Textures.Sample.Pixel.Value, _rightLegTargetPosition - screenPos, new Rectangle(0, 0, 5, 5), Color.Orange);
        spriteBatch.Draw(Textures.Sample.Pixel.Value, _rightLegEndPosition - screenPos, new Rectangle(0, 0, 5, 5), Color.Yellow);
        
        spriteBatch.Draw(Textures.Sample.Pixel.Value, _leftLegTargetPosition - screenPos, new Rectangle(0, 0, 5, 5), Color.Orange);
        spriteBatch.Draw(Textures.Sample.Pixel.Value, _leftLegEndPosition - screenPos, new Rectangle(0, 0, 5, 5), Color.Yellow);
        
#endif

        return false;
    }
}