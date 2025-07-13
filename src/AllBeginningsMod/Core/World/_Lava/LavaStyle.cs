using AllBeginningsMod.Content.Biomes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Liquid;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace AllBeginningsMod.Core.World;

public enum LiquidTileType {
    Block,
    Fall,
    Slope
}

public abstract class LavaStyle : ModTexturedType {
    internal Texture2D LavaTexture;
    internal Texture2D BlockTexture;
    internal Texture2D SlopeTexture;

    public int Type;

    public override void Load() {
        if (Main.netMode == NetmodeID.Server)
            return;

        LavaTexture = ModContent.Request<Texture2D>(LavaTexturePath, AssetRequestMode.ImmediateLoad).Value;
        BlockTexture = ModContent.Request<Texture2D>(BlockTexturePath, AssetRequestMode.ImmediateLoad).Value;
        SlopeTexture = ModContent.Request<Texture2D>(SlopeTexturePath, AssetRequestMode.ImmediateLoad).Value;
    }

    public override void Unload() {
        LavaTexture = null;
        BlockTexture = null;
    }
    
    public override void Register() {
        LavaStyleLoader.RegisterStyle(this);
        ModTypeLookup<LavaStyle>.Register(this);
    }

    public abstract string LavaTexturePath { get; }

    public abstract string BlockTexturePath { get; }

    public abstract string SlopeTexturePath { get; }

    public abstract int ChooseWaterfallStyle();

    public abstract int GetSplashDust();

    public abstract int GetDropletGore();

    public virtual void SelectLightColor(ref Color initialLightColor) {}

    public virtual void ModifyVertexColors(int x, int y, ref VertexColors colors) { }
}