using AllBeginningsMod.Core.Loaders;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.NPCs.Enemies.Bosses.Bastroboy;

internal class StarWhirlProjectile : ModProjectile {
    public override string Texture => Assets.Assets.Textures.NPCs.Bosses.Bastroboy.KEY_StarWhirlProjectile;
    
    private int bastroboyWhoAmI = -1;
    private const int MaxWidth = 300;
    private const int MaxHeight = 300;
    public override void SetDefaults() {
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.Size = new Vector2(0, 0);
        Projectile.timeLeft = BastroboyNPC.StarWhirlTime;
    }

    public override void AI() {
        if(bastroboyWhoAmI == -1) {
            bastroboyWhoAmI = Main.npc.FirstOrDefault(npc => npc is not null && npc.active && npc.ModNPC is BastroboyNPC)?.whoAmI ?? -1;
        }

        NPC bastroboy = Main.npc[bastroboyWhoAmI];

        float factor = 1f - (float)Projectile.timeLeft / BastroboyNPC.StarWhirlTime;
        Projectile.scale = factor switch
        {
            <= 0.1f => -MathF.Pow(10f * factor - 1, 2) + 1,
            >= 0.9f => -MathF.Pow(10f * (factor - 0.8f) - 1, 2) + 1,
            _ => 1f
        };

        Projectile.scale *= Projectile.scale;

        Projectile.width = (int)(MaxWidth * Projectile.scale);
        Projectile.height = (int)(MaxHeight * Projectile.scale);
        Projectile.Center = new(bastroboy.Center.X, MathHelper.Lerp(Projectile.Center.Y, bastroboy.Center.Y - 130f, 0.1f));
        MathUtilities.ForEachPlayerInRange(
            Projectile.Center,
            16 * 100,
            player =>
            {
                player.GetModPlayer<BastroboyStarWhirlPlayer>().StarWhirlWhoAmI = Projectile.whoAmI;
            }
        );
    }

    public override bool PreDraw(ref Color lightColor) {
        Effect effect = EffectLoader.GetEffect("Pixel::BastroboyStarWhirl");
        Texture2D sampleTexture1 = Mod.Assets.Request<Texture2D>("Assets/Textures/Sample/PlasmaNoise", AssetRequestMode.ImmediateLoad).Value;
        Texture2D sampleTexture2 = Mod.Assets.Request<Texture2D>("Assets/Textures/Sample/Noise1", AssetRequestMode.ImmediateLoad).Value;

        effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.06f);
        effect.Parameters["holeSize"].SetValue(0.05f);
        effect.Parameters["topHalf"].SetValue(false);
        effect.Parameters["sampleTexture2"].SetValue(sampleTexture2);
        effect.Parameters["intensity"].SetValue(4f);

        Color color = Color.CornflowerBlue;

        SpriteBatchData snapshot = Main.spriteBatch.Capture();
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(snapshot with { Effect = effect, BlendState = BlendState.AlphaBlend });

        Main.spriteBatch.Draw(
            sampleTexture1,
            Projectile.Hitbox.Modified((int)-Main.screenPosition.X, (int)-Main.screenPosition.Y, 0, -Projectile.height / 2),
            color
        );

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(snapshot);

        Texture2D orbTexture = TextureAssets.Projectile[Type].Value;
        Main.spriteBatch.Draw(
            orbTexture,
            Projectile.Center - Main.screenPosition,
            null,
            lightColor,
            0f,
            orbTexture.Size() / 2f,
            Projectile.scale,
            SpriteEffects.None,
            0f
        );

        effect.Parameters["topHalf"].SetValue(true);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(snapshot with { Effect = effect });

        Main.spriteBatch.Draw(
            sampleTexture1,
            Projectile.Hitbox.Modified((int)-Main.screenPosition.X, (int)-Main.screenPosition.Y + Projectile.height / 2, 0, -Projectile.height / 2),
            color
        );

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(snapshot);

        return false;
    }
}


public class BastroboyStarWhirlPlayer : ModPlayer {
    public int StarWhirlWhoAmI { get; set; }
    public override void PreUpdateMovement() {
        if(StarWhirlWhoAmI == -1) {
            return;
        }

        Player.velocity += Player.Center.DirectionTo(Main.projectile[StarWhirlWhoAmI].Center) * 0.25f;
        StarWhirlWhoAmI = -1;
    }
}