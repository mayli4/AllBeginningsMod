using AllBeginningsMod.Content.Miscellaneous;

namespace AllBeginningsMod.Common.World;

internal sealed class MiscLootPool : ModChestLoot {
    public override RegionFlags Region => RegionFlags.Surface | RegionFlags.Underwater | RegionFlags.Livingwood;

    public override void AddLoot() {
        AddItem(ModContent.ItemType<UrnOfGreedItem>(), chance: 25);
    }
}