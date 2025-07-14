using Microsoft.Xna.Framework;
using System;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.NPCs.Corruption;

public class TerrorBatSpit : ModProjectile {
    public override string Texture => "Terraria/Images/NPC_112";
    
    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = 300;

        Projectile.penetrate = 2;

        Projectile.aiStyle = -1;
        Projectile.extraUpdates = 1;
    }

    public override void AI() {
        Projectile.velocity.Y += 0.1f;
        if (Projectile.velocity.Y > 16f) {
            Projectile.velocity.Y = 16f;
        }

        Projectile.rotation += Projectile.velocity.X * 0.05f;
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
}