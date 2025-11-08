using Microsoft.Xna.Framework;
using System;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ObjectData;

namespace AllBeginningsMod.Content.Miscellaneous;

//really not a fan of abstracts for content like this :\\, but i didnt wanna dupe everything like 30 times over

[Flags]
internal enum UrnType : sbyte {
    Copper,
    CopperRich,
    Stone,
    StoneRich,
    Clay,
    ClayRich
}

internal sealed class CopperUrn : BaseUrnTile {
    public override UrnType UrnType => UrnType.Copper;
    public override int Dust => DustID.Copper;

    public override string Texture => Assets.Textures.Items.Misc.UrnOfGreed.CopperUrnTile.KEY;
}

internal sealed class CopperUrnRich : BaseUrnTile {
    public override UrnType UrnType => UrnType.CopperRich;
    public override int Dust => DustID.Gold;

    public override string Texture => Assets.Textures.Items.Misc.UrnOfGreed.CopperUrnRichTile.KEY;
}

internal sealed class StoneUrn : BaseUrnTile {
    public override UrnType UrnType => UrnType.Stone;
    public override int Dust => DustID.Stone;

    public override string Texture => Assets.Textures.Items.Misc.UrnOfGreed.StoneUrnTile.KEY;
}

internal sealed class StoneUrnRich : BaseUrnTile {
    public override UrnType UrnType => UrnType.StoneRich;
    public override int Dust => DustID.Granite;

    public override string Texture => Assets.Textures.Items.Misc.UrnOfGreed.GildedGraniteTile.KEY;
}

internal sealed class ClayUrn : BaseUrnTile {
    public override UrnType UrnType => UrnType.Clay;
    public override int Dust => DustID.Lead;

    public override string Texture => Assets.Textures.Items.Misc.UrnOfGreed.ClayUrnTile.KEY;
}

internal sealed class ClayUrnRich : BaseUrnTile {
    public override UrnType UrnType => UrnType.ClayRich;
    public override int Dust => DustID.Stone;

    public override string Texture => Assets.Textures.Items.Misc.UrnOfGreed.PorcelainUrnTile.KEY;
}