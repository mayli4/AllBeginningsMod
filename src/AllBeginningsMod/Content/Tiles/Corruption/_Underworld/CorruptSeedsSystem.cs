using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.Tiles.Corruption;

public sealed class CorruptSeedsSystem : GlobalItem {
    public override bool AppliesToEntity(Item entity, bool lateInstantiation) 
        => entity.type == ItemID.CorruptSeeds;

    public override bool? UseItem(Item item, Player player) {
        if (Main.myPlayer != player.whoAmI) {
            return null;
        }
        
        var tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
        if (tile.HasTile && tile.TileType == ModContent.TileType<CorruptAsh>() && player.IsTargetTileInItemRange(item)) {
            WorldGen.PlaceTile(Player.tileTargetX, Player.tileTargetY, ModContent.TileType<OvergrownCorruptAsh>(), forced: true);

            if (Main.netMode != NetmodeID.SinglePlayer)
                NetMessage.SendTileSquare(player.whoAmI, Player.tileTargetX, Player.tileTargetY);

            return true;
        }
        return null;
    }
}