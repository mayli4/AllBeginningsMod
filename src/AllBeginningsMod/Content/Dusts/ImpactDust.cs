using AllBeginningsMod.Common.Rendering;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace AllBeginningsMod.Content.Dusts;

public class ImpactLineDust : ModDust {
    public override string Texture => Helper.PlaceholderTextureKey;

    public override void OnSpawn(Dust dust) {
        dust.frame = new Rectangle(0, 0, 4, 4);
    }

    public override bool Update(Dust dust) {
        dust.position += dust.velocity;
        dust.velocity *= 0.9f;
        dust.rotation = dust.velocity.ToRotation();

        dust.alpha += 10;

        dust.alpha = (int)(dust.alpha * 1.01f);

        if(dust.alpha >= 255)
            dust.active = false;

        return false;
    }

    public override bool PreDraw(Dust dust) {
        float lerper = 1f - dust.alpha / 255f;

        Texture2D tex = Textures.Sample.Line.Value;

        Graphics.BeginPipeline(0.5f)
            .DrawSprite(tex, dust.position - Main.screenPosition, dust.color * lerper, null, dust.rotation, tex.Size() / 2f, new Vector2(dust.scale * lerper, dust.scale), 0f)
            .Flush();

        return false;
    }
}