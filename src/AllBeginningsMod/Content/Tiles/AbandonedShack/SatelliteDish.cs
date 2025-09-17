using System;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ObjectData;

namespace AllBeginningsMod.Content.Tiles.AbandonedShack;

public class SatelliteDish : ModTile {
    public override string Texture =>  Textures.Tiles.AbandonedShack.KEY_SatelliteDishTile;
    
    private float _currentDishRotation = 0f;
    private float _targetDishRotation = 0f;
    private int _dishRotationPauseTimer = 0;
    
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = false;
        Main.tileTable[Type] = true;
        Main.tileLavaDeath[Type] = true;

        Main.tileSolid[Type] = false;
        
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.Width = 4;
        TileObjectData.newTile.Height = 3;
        
        TileObjectData.newTile.Origin = new Point16(0, 0);

        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        
        TileObjectData.newTile.CoordinateHeights = new [] { 16, 16, 16 };
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        
        TileObjectData.addTile(Type);
        
        AddMapEntry(Color.Gray);

        DustType = DustID.WoodFurniture;
        HitSound = SoundID.Dig;
    }
    
        
    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
        if(!TileObjectData.IsTopLeft(i, j))
            return;
        
        Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
    }

    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
        var worldPos = new Vector2(i, j + 1) * 16 + new Vector2(12, 4);
        var drawPos = worldPos - Main.screenPosition;

        var tex = Textures.Tiles.AbandonedShack.SatelliteDish_Dish.Value;
        
        if (_dishRotationPauseTimer > 0) {
            _dishRotationPauseTimer--;
        } else {
            if (Math.Abs(_currentDishRotation - _targetDishRotation) < 0.01f) {
                _targetDishRotation = WorldGen.genRand.NextFloat(-MathHelper.Pi / 10f, MathHelper.Pi / 10f);
                _dishRotationPauseTimer = WorldGen.genRand.Next(120, 121);
            } else {
                _currentDishRotation = MathHelper.Lerp(_currentDishRotation, _targetDishRotation, 0.0040f);
            }
        }

        var dishTex = Textures.Tiles.AbandonedShack.SatelliteDish_Dish.Value;

        spriteBatch.Draw(
            dishTex,
            drawPos,
            null,
            Lighting.GetColor(i, j),
            _currentDishRotation,
            new Vector2(10, tex.Height - 9),
            1f,
            SpriteEffects.None,
            0f
        );
    }
}