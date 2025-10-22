using AllBeginningsMod.Common.Graphics;
using AllBeginningsMod.Content.Dusts;
using AllBeginningsMod.Utilities;
using System;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ObjectData;
using Terraria.Utilities;

namespace AllBeginningsMod.Content.AbandonedShack;

internal sealed class LargeCeilingLamp : ModTile {
    public override string Texture => Helper.PlaceholderTextureKey;

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = false;
        Main.tileTable[Type] = true;
        Main.tileLavaDeath[Type] = true;

        Main.tileSolid[Type] = false;
        Main.tileSolidTop[Type] = true;

        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.Width = 1;
        TileObjectData.newTile.Height = 1;

        TileObjectData.newTile.Origin = new Point16(1, 0);

        TileObjectData.newTile.Origin = new Point16(0, 0);
        TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidBottom, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.AnchorBottom = default;

        TileObjectData.newTile.CoordinateHeights = new int[] { 16 };
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;

        TileObjectData.addTile(Type);

        AddMapEntry(Color.Gray);

        DustType = DustID.WoodFurniture;
        HitSound = SoundID.Dig;
    }

    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
        Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
    }

    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
        var worldPos = new Vector2(i, j) * 16;
        var drawPos = worldPos - Main.screenPosition;

        var tex = Textures.Tiles.AbandonedShack.LargeCeilingLamp.Value;
        var ray = Textures.Tiles.AbandonedShack.LargeCeilingLampRay.Value;

        var rayPos = drawPos + new Vector2(-1, 14);

        if(!Main.rand.NextBool(134)) {
            spriteBatch.Draw(ray, rayPos, Color.DarkGoldenrod.Additive() * 0.4f);
        }

        spriteBatch.Draw(tex, drawPos + new Vector2(-8, 0), Lighting.GetColor(i, j));

        var color = Color.Lerp(Color.White, Color.Gold, 0.4f);
        Lighting.AddLight(new Vector2(i, j) * 16, color.ToVector3());
    }

    public override void NearbyEffects(int i, int j, bool closer) {
        if(Main.rand.NextBool(30)) {
            // Dust.NewDustPerfect(
            //     new Vector2(i, j + 1) * 16,
            //     ModContent.DustType<LampLightDust>(),
            //     new Vector2(0, 10),
            //     0,
            //     new Color(255, 120, 0, 5)
            // );   
        }
    }
}

internal sealed class SmallLongCeilingLamp : ModTile {
    public override string Texture => Helper.PlaceholderTextureKey;

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = false;
        Main.tileTable[Type] = true;
        Main.tileLavaDeath[Type] = true;

        Main.tileSolid[Type] = false;
        Main.tileSolidTop[Type] = true;

        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.Width = 1;
        TileObjectData.newTile.Height = 1;

        TileObjectData.newTile.Origin = new Point16(1, 0);

        TileObjectData.newTile.Origin = new Point16(0, 0);
        TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidBottom, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.AnchorBottom = default;

        TileObjectData.newTile.CoordinateHeights = new int[] { 16 };
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;

        TileObjectData.addTile(Type);

        AddMapEntry(Color.Gray);

        DustType = DustID.WoodFurniture;
        HitSound = SoundID.Dig;
    }

    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
        Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
    }

    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
        var worldPos = new Vector2(i, j) * 16;
        var drawPos = worldPos - Main.screenPosition;

        var tex = Textures.Tiles.AbandonedShack.SmallLongCeilingLamp.Value;
        var ray = Textures.Tiles.AbandonedShack.SmallCeilingLampRay.Value;

        var rayPos = drawPos + new Vector2(3, 23);

        if(!Main.rand.NextBool(134)) {
            spriteBatch.Draw(ray, rayPos, Color.DarkGoldenrod.Additive() * 0.4f);
        }

        spriteBatch.Draw(tex, drawPos + new Vector2(0, 0), Lighting.GetColor(i, j));

        var color = Color.Lerp(Color.White, Color.Gold, 0.4f);
        Lighting.AddLight(new Vector2(i, j) * 16, color.ToVector3());
    }
}

internal sealed class SmallShortCeilingLamp : ModTile {
    public override string Texture => Helper.PlaceholderTextureKey;

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = false;
        Main.tileTable[Type] = true;
        Main.tileLavaDeath[Type] = true;

        Main.tileSolid[Type] = false;
        Main.tileSolidTop[Type] = true;

        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.Width = 1;
        TileObjectData.newTile.Height = 1;

        TileObjectData.newTile.Origin = new Point16(0, 0);
        TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidBottom, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.AnchorBottom = default;

        TileObjectData.newTile.CoordinateHeights = new int[] { 16 };
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;

        TileObjectData.addTile(Type);

        AddMapEntry(Color.Gray);

        DustType = DustID.WoodFurniture;
        HitSound = SoundID.Dig;
    }

    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
        Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
    }

    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
        var worldPos = new Vector2(i, j) * 16;
        var drawPos = worldPos - Main.screenPosition;

        var tex = Textures.Tiles.AbandonedShack.SmallShortCeilingLamp.Value;
        var ray = Textures.Tiles.AbandonedShack.SmallCeilingLampRay.Value;

        var rayPos = drawPos + new Vector2(3, 18);

        if(!Main.rand.NextBool(134)) {
            spriteBatch.Draw(ray, rayPos, Color.DarkGoldenrod.Additive() * 0.4f);
        }

        spriteBatch.Draw(tex, drawPos + new Vector2(0, 0), Lighting.GetColor(i, j));

        var color = Color.Lerp(Color.White, Color.Gold, 0.4f);
        Lighting.AddLight(new Vector2(i, j) * 16, color.ToVector3());
    }
}


internal sealed class LampLightDust : ModDust {
    public override string Texture => Helper.PlaceholderTextureKey;

    public override void OnSpawn(Dust dust) {
        dust.frame = new Rectangle(0, 0, 4, 4);
    }

    public override bool Update(Dust dust) {
        dust.position += dust.velocity;
        dust.velocity *= 0.9f;
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