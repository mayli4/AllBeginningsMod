﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.Dusts; 

internal class HellVampireExplosionDust : ModDust {
    public override string Texture => Assets.Assets.Textures.Dusts.KEY_HellVampireExplosionDust;
    
    public override void OnSpawn(Dust dust) {
        dust.frame = new Rectangle(0, 58 * Main.rand.Next(3), 40, 58);
        dust.fadeIn = Main.rand.Next(120, 250);
        dust.rotation = Main.rand.NextFloatDirection() * MathHelper.Pi;
    }

    public override bool Update(Dust dust) {
        dust.position += dust.velocity;
        dust.velocity.Y -= 0.1f;
        dust.velocity *= 0.94f;

        dust.color = Color.Lerp(dust.color, Color.Black, 0.005f);
        dust.alpha += 2;
        dust.scale += 0.005f;
        if(dust.alpha >= 255) {
            dust.active = false;
        }
        return false;
    }
}