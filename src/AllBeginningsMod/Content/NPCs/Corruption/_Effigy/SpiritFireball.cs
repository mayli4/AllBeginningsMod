using AllBeginningsMod.Common.PrimitiveDrawing;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.NPCs.Corruption._Effigy;
internal class SpiritFireball : ModProjectile {
    public override string Texture => "Terraria/Images/Item_0";

    PrimitiveTrail trail;
    public static readonly float Gravity = 0.3f;
    public static readonly int MaxTimeLeft = 130;

    float Scale => 1f - MathF.Pow((float)(MaxTimeLeft - Projectile.timeLeft) / MaxTimeLeft, 2);

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = MaxTimeLeft;

        Projectile.aiStyle = -1;
    }

    public override void AI() {
        const float TrailSize = 28f;
        trail ??= new(
            [.. Enumerable.Repeat(Projectile.Center, 7)],
            t => TrailSize * Scale,
            static t => Color.Lerp(CursedSpiritNPC.GhostColor1, CursedSpiritNPC.GhostColor2, t + 0.7f)
        );

        var i = trail.Positions.Length - 1;
        while(i > 0) {
            trail.Positions[i] = trail.Positions[i - 1];
            i -= 1;
        }
        trail.Positions[0] = Projectile.Center + Projectile.velocity;

        Projectile.velocity.Y += Gravity;

        if(Main.rand.NextBool(14)) Dust.NewDust(
            Projectile.position,
            Projectile.width,
            Projectile.height,
            DustID.Pixie,
            newColor: Main.rand.NextFromList(CursedSpiritNPC.GhostColor1, CursedSpiritNPC.GhostColor2)
        );
    }

    public override bool PreDraw(ref Color lightColor) {
        var trailEffect = Assets.Assets.Effects.Compiled.Trail.CursedSpiritFire.Value;
        trailEffect.Parameters["time"].SetValue(0.025f * Main.GameUpdateCount);
        trailEffect.Parameters["mat"].SetValue(MathUtilities.WorldTransformationMatrix);
        trailEffect.Parameters["stepY"].SetValue(0.25f);
        trailEffect.Parameters["scale"].SetValue(0.25f);
        trailEffect.Parameters["texture1"].SetValue(Assets.Assets.Textures.Sample.Pebbles.Value);
        trailEffect.Parameters["texture2"].SetValue(Assets.Assets.Textures.Sample.Noise2.Value);
        trail.Draw(trailEffect);

        var glowTexture = Assets.Assets.Textures.Sample.Glow1.Value;
        var blinker = (MathF.Sin(0.1f * Main.GameUpdateCount + 23.2f * Projectile.whoAmI) +
            MathF.Cos(0.06f * Main.GameUpdateCount) + 2f) / 4f;
        var bigGlowColor = CursedSpiritNPC.GhostColor2 * (0.3f + 0.3f * blinker);
        var smallGlowColor = CursedSpiritNPC.GhostColor1;

        var snapshot = Main.spriteBatch.CaptureEndBegin(new() { BlendState = BlendState.Additive });
        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.Center - Main.screenPosition - Projectile.velocity * 0.6f,
            null,
            bigGlowColor,
            0f,
            glowTexture.Size() * 0.5f,
            0.3f * Scale,
            SpriteEffects.None,
            0
        );

        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.Center - Main.screenPosition,
            null,
            smallGlowColor,
            0f,
            glowTexture.Size() * 0.5f,
            0.1f * Scale,
            SpriteEffects.None,
            0
        );
        Main.spriteBatch.EndBegin(snapshot);

        return false;
    }
}
