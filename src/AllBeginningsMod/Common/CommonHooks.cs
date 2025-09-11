namespace AllBeginningsMod.Core;

internal sealed class CommonHooks : ModSystem {
    public delegate bool ModifyChestContentsDelegate(Chest chest, Tile chestTile, bool alreadyAddedItem);
    public static event ModifyChestContentsDelegate ModifyChestContentsEvent;

    public override void PostWorldGen() {
        if(ModifyChestContentsEvent is null) return;
    }
}