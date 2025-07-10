using AllBeginningsMod.Core.Loaders;
using AllBeginningsMod.Common.PrimitiveDrawing;
using AllBeginningsMod.Common.Rendering;
using AllBeginningsMod.Content.CameraModifiers;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.NPCs.Enemies.Bosses.Bastroboy;

internal class NeutralSunProjectile : ModProjectile {
    private int bastroboyWhoAmI = -1;

    private PrimitiveTrail[] fireTrails;
    public override string Texture => "Terraria/Images/Item_0";

    private ref float Timer => ref Projectile.ai[0];
    private ref float State => ref Projectile.ai[1];

    public override void SetDefaults() {
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.Size = new(200, 200);
        Projectile.alpha = 255;
        Projectile.timeLeft = BastroboyNPC.StarWhirlTime;
    }

    public override void AI() {
        Projectile.scale = MathHelper.SmoothStep(
            0f,
            1f,
            MathF.Sin(MathHelper.Pi * Projectile.timeLeft / BastroboyNPC.StarWhirlTime)
        );

        if(fireTrails is null) {
            fireTrails = new PrimitiveTrail[8];

            for(int i = 0; i < fireTrails.Length; i++) {
                fireTrails[i] = new(
                    new Vector2[30],
                    factor => 120f * (1f - factor) * Projectile.scale,
                    factor => Color.Lerp(Color.Orange, Color.Red, factor),
                    initPosition: Projectile.Center
                );
            }
        }

        const float maxTimer = 80f;
        const float maxLength = 1400f;
        const float shortLength = 200f;
        float bigLength = MathHelper.Lerp(shortLength, maxLength, MathHelper.SmoothStep(0f, 1f, MathF.Sin(MathHelper.Pi * Timer / maxTimer)));

        for(int i = 0; i < fireTrails.Length; i++) {
            Vector2 direction = (MathHelper.TwoPi / fireTrails.Length * i).ToRotationVector2().RotatedBy(Projectile.rotation);
            float length = (int)State switch
            {
                0 => i % 2 == 0 ? shortLength : bigLength,
                _ => i % 2 == 0 ? bigLength : shortLength
            } +
                Main.rand.NextFloat(10f);

            length *= Projectile.scale;


            for(int j = 0; j < fireTrails[i].Positions.Length; j++) {
                float ratio = (float)j / fireTrails[i].Positions.Length;
                Vector2 target = Projectile.Center + direction * length * ratio + Main.rand.NextFloatDirection() * 2f * direction.RotatedBy(MathHelper.PiOver2);
                fireTrails[i].Positions[j] = target;
            }
        }

        Projectile.rotation += 0.01f + 0.01f * (1f - (float)Projectile.timeLeft / BastroboyNPC.StarWhirlTime);

        Timer++;

        if(Timer > maxTimer) {
            State = (State + 1) % 2;
            Timer = 0;
        }

        Lighting.AddLight(Projectile.Center, 10f, 10f, 10f);

        if(bastroboyWhoAmI == -1) {
            bastroboyWhoAmI = Main.npc.FirstOrDefault(npc => npc is not null && npc.active && npc.ModNPC is BastroboyNPC)?.whoAmI ?? -1;
        }

        Main.instance.CameraModifiers.Add(
            new ExplosionShakeCameraModifier(2f, 0.9f, Projectile.Center, 5000, FullName)
        );

        Projectile.Center = Vector2.Lerp(Projectile.Center, Main.npc[bastroboyWhoAmI].Center, 0.2f);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        foreach(PrimitiveTrail trail in fireTrails) {
            if(
                Collision.CheckAABBvLineCollision(
                    targetHitbox.TopLeft(),
                    targetHitbox.Size(),
                    trail.Positions[0],
                    trail.Positions[^1]
                )
            ) {
                return true;
            }
        }

        return false;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindNPCsAndTiles.Add(index);
    }

    public override bool PreDraw(ref Color lightColor) {
        Renderer.QueueRenderAction(
            () =>
            {
                Effect effect = EffectLoader.GetEffect("Trail::NeutralSunLaser");
                effect.Parameters["stretch1"].SetValue(0.4f);
                effect.Parameters["stretch2"].SetValue(0.8f);
                effect.Parameters["noise1"].SetValue(Mod.Assets.Request<Texture2D>("Assets/Textures/Sample/Noise3").Value);
                effect.Parameters["noise2"].SetValue(Mod.Assets.Request<Texture2D>("Assets/Textures/Sample/PerlinNoise").Value);
                effect.Parameters["transformationMatrix"]
                    .SetValue(
                        Matrix.CreateTranslation(-Main.screenPosition.ToVector3()) *
                        Main.GameViewMatrix.EffectMatrix *
                        Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1)
                    );

                float time = -Main.GameUpdateCount * 0.01f;

                foreach(PrimitiveTrail trail in fireTrails) {
                    effect.Parameters["time"].SetValue(time);
                    trail.Draw(effect);

                    // Yes this is a random number it has  no significance yea eyea yea and no reason for it to be that but it is what it is
                    time += 393.4390f;
                }
            },
            RenderLayer.NPCs,
            style: RenderStyle.Pixelated
        );

        Texture2D disk = Assets.Assets.Textures.NPCs.Bosses.Bastroboy.NeutralSunDisk.Value;
        Main.spriteBatch.Draw(
            disk,
            Projectile.Center - Main.screenPosition + Main.rand.NextVector2Unit() * 2f,
            null,
            Color.White,
            Projectile.rotation,
            disk.Size() / 2f,
            Projectile.scale,
            SpriteEffects.None,
            0f
        );

        return false;
    }
}