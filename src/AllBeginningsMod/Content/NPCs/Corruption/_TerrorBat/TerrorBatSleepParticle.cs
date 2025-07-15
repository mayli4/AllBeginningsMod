using AllBeginningsMod.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.NPCs.Corruption;

public class Sleep : ModDust {
    public override string Texture => Assets.Assets.Textures.NPCs.Corruption.TerrorBat.KEY_TerrorBatSleepDust;

    public override bool Update(Dust dust) {

        float scaleReduction = Math.Clamp(dust.fadeIn / 60, 0, 1);

        dust.frame = new Rectangle(0, 0, 28, 30);

        dust.scale = scaleReduction * 0.9f + MathF.Pow(MathF.Sin((Main.GameUpdateCount ) / 15f), 2) * 0.3f;
        dust.rotation = MathF.Sin((Main.GameUpdateCount ) / 10f) * (MathF.PI / 180f) * 10;

        dust.velocity.X *= 0.975f;

        dust.position += dust.velocity + Vector2.UnitX * MathF.Sin(dust.fadeIn / 15f) * 0.6f;

        dust.fadeIn--;
        if (dust.fadeIn <= 0)
            dust.active = false;

        return false;
    }

    public override bool PreDraw(Dust dust) {
        var tex = Assets.Assets.Textures.NPCs.Corruption.TerrorBat.TerrorBatSleepDust.Value;
        Vector2 drawOrigin = new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);

        Main.EntitySpriteDraw(tex, dust.position - Main.screenPosition, dust.frame, Color.White, dust.rotation, drawOrigin, dust.scale, SpriteEffects.None);

        return false;
    }
}