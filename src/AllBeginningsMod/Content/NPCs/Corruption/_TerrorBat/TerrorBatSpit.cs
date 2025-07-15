using AllBeginningsMod.Common.PrimitiveDrawing;
using AllBeginningsMod.Core.Loaders;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.NPCs.Corruption;

public class TerrorBatSpit : ModProjectile {
    public override string Texture => "Terraria/Images/NPC_112";
    
    private PrimitiveTrail sparkleTrail;
    private OldPositionCache positionCache;
    private bool trailInit;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 20;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = 3222200;

        Projectile.penetrate = 2;

        Projectile.aiStyle = -1;
        Projectile.extraUpdates = 1;
        
        positionCache = new(30);
        sparkleTrail = new PrimitiveTrail(
            positionCache.Positions,
            factor => (-MathF.Pow(factor - 1f, 4) + 1f) * 40,
            factor => Color.Lerp(
                Color.Lerp(new Color(72, 96, 36, 255), Color.Transparent, factor * 1.2f),
                Color.Black,
                (MathF.Sin(Main.GameUpdateCount * 0.25f + factor * 10f) + 1f) * 0.1f
            ) * 0.9f
        );
    }

    public override void AI() {
        Projectile.velocity.Y += 0.1f;
        if (Projectile.velocity.Y > 16f) {
            Projectile.velocity.Y = 16f;
        }

        Projectile.rotation = Projectile.velocity.ToRotation();
        
        var pos = Projectile.Center;
        
        if(!trailInit) {
            positionCache.SetAll(pos);
            Projectile.oldPos = Projectile.oldPos.Select(_ => Projectile.position).ToArray();
            trailInit = true;
        }
        
        Dust.NewDustDirect(Projectile.position, 10, 10, DustID.CursedTorch);

        positionCache.Add(pos);
        sparkleTrail.Positions = positionCache.Positions;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

        Projectile.penetrate--;

        if (Projectile.penetrate <= 0) {
            Projectile.Kill();
        }
        else {      
            if (Projectile.velocity.X != oldVelocity.X) {
                Projectile.velocity.X = -oldVelocity.X * 0.85f;
            }
            if (Projectile.velocity.Y != oldVelocity.Y) {
                Projectile.velocity.Y = -oldVelocity.Y * 0.85f;
            }
        }
        return false;
    }

    public override bool PreDraw(ref Color lightColor) {
        var shader = Assets.Assets.Effects.Compiled.Trail.Fire.Value;

        
        shader.Parameters["transformationMatrix"].SetValue(MathUtilities.WorldTransformationMatrix);
        shader.Parameters["amp"].SetValue(0.15f);
        shader.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f);
        shader.Parameters["smooth"].SetValue(0.45f);
        shader.Parameters["sampleTexture"].SetValue(Assets.Assets.Textures.Sample.Noise2.Value);
        
        Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
        
        sparkleTrail.Draw(shader);
        return true;
    }
}