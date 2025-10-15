using AllBeginningsMod.Utilities;
using Terraria.GameContent;

namespace AllBeginningsMod.Content.Bosses;

internal partial class NightgauntNPC {
    static void DrawArm(Vector2 a, Vector2 b, Vector2 c, Color drawColor, SpriteEffects effects, bool anchored) {
        var armTexture = Textures.NPCs.Bosses.Nightgaunt.NightgauntArm.Value;
        var defaultForearmFrame = new Rectangle(0, 0, 84, 32);
        var anchoredForearmFrame = new Rectangle(0, 32, 84, 32);
        
        var currentFrame = anchored ? anchoredForearmFrame : defaultForearmFrame;
        
        var forearmOrigin = anchored ? new Vector2(72, 42) : new Vector2(72, 14);
        
        Main.spriteBatch.Draw(
            armTexture,
            a - Main.screenPosition,
            new Rectangle(94, 0, 48, 24),
            drawColor,
            (a - b).ToRotation(),
            new(134 - 94, 12),
            1f,
            effects,
            0f
        );
        
        var lowerArmOrigin = new Vector2(72, 16);

        // if (effects.HasFlag(SpriteEffects.FlipVertically)) {
        //     lowerArmOrigin.X = currentFrame.Width;
        // }

        Main.spriteBatch.Draw(
            armTexture,
            b - Main.screenPosition,
            currentFrame,
            drawColor,
            (b - c).ToRotation(),
            lowerArmOrigin,
            1f,
            effects,
            0f
        );
    }

    static void DrawLeg(Vector2 a, Vector2 b, Vector2 c, Color drawColor, SpriteEffects effects, bool anchored) {
        var legTexture = Textures.NPCs.Bosses.Nightgaunt.NightgauntLeg.Value;
        var defaultLowerLegFrame = new Rectangle(0, 0, 80, 26);
        var anchoredLowerLegFrame = new Rectangle(0, 26, 80, 26);
        
        var currentFrame = anchored ? anchoredLowerLegFrame : defaultLowerLegFrame;
        
        Main.spriteBatch.Draw(
            legTexture,
            a - Main.screenPosition,
            new Rectangle(84, 0, 66, 26),
            drawColor,
            (a - b).ToRotation(),
            new(134 - 74, 12),
            1f,
            effects,
            0f
        );

        Main.spriteBatch.Draw(
            legTexture,
            b - Main.screenPosition,
            currentFrame,
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
        DrawArm(_rightArm.Position(0), _rightArm.Position(1), _rightArm.Position(2), drawColor, SpriteEffects.FlipVertically, _rightArmAnchored);
        DrawArm(_leftArm.Position(0), _leftArm.Position(1), _leftArm.Position(2), drawColor, SpriteEffects.None, _leftArmAnchored);
        
        DrawLeg(_rightLeg.Position(0), _rightLeg.Position(1), _rightLeg.Position(2), drawColor, SpriteEffects.FlipVertically, _rightLegAnchored);
        DrawLeg(_leftLeg.Position(0), _leftLeg.Position(1), _leftLeg.Position(2), drawColor, SpriteEffects.None, _leftLegAnchored);

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