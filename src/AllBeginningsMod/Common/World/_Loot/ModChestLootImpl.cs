using AllBeginningsMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace AllBeginningsMod.Common.World;

public abstract class ModChestLoot : ModType {
    internal readonly Dictionary<RegionFlags, List<ModChestLootImpl.LootInfo>> LootInfo = new();
    internal readonly Dictionary<RegionFlags, List<ModChestLootImpl.LootInfo>> ExclusiveLootInfo = new();

    public virtual RegionFlags Region => RegionFlags.All;

    public override void Register() {
        ModTypeLookup<ModChestLoot>.Register(this);
        AddLoot();
    }

    public abstract void AddLoot();

    protected void AddItem(int itemType, RegionFlags chestRegions, float chance, (int min, int max) stackRange, bool exclusive = false, int slotIndex = -1) {
        AddItem(new int[] { itemType }, chestRegions, chance, stackRange, exclusive, slotIndex);
    }

    protected void AddItem(int itemType, float chance = 0.125f, int stack = 1, bool exclusive = false, int slotIndex = -1) {
        AddItem(new int[] { itemType }, Region, chance, (stack, stack), exclusive, slotIndex);
    }

    protected void AddItem(int itemType, RegionFlags chestRegions, float chance = 0.125f, int stack = 1, bool exclusive = false, int slotIndex = -1) {
        AddItem(new int[] { itemType }, chestRegions, chance, (stack, stack), exclusive, slotIndex);
    }

    protected void AddItem(int[] itemTypes, RegionFlags chestRegions, float chance, (int min, int max) stackRange, bool exclusive = false, int slotIndex = -1) {
        var newLootInfo = new ModChestLootImpl.LootInfo(itemTypes, new ModChestLootImpl.Range(stackRange.min, stackRange.max), chance, slotIndex);

        foreach (RegionFlags flag in chestRegions.GetFlags()) {
            var dict = exclusive ? ExclusiveLootInfo : LootInfo;

            if (!dict.TryGetValue(flag, out List<ModChestLootImpl.LootInfo> list)) {
                list = new List<ModChestLootImpl.LootInfo>();
                dict.Add(flag, list);
            }
            list.Add(newLootInfo);
        }
    }

    protected void AddItem(int[] itemTypes, float chance, (int min, int max) stackRange, bool exclusive = false, int slotIndex = -1) {
        AddItem(itemTypes, Region, chance, stackRange, exclusive, slotIndex);
    }
}

public class ModChestLootImpl : ModSystem {
    private Dictionary<int, RegionFlags> _framingToRegion;
    private Dictionary<RegionFlags, List<LootInfo>> _regionLootInfo;
    private Dictionary<RegionFlags, List<LootInfo>> _regionExclusiveLootInfo;
    
    public override void PostWorldGen() => PopulateAllChests();

    public override void PostSetupContent() {
        _framingToRegion = new Dictionary<int, RegionFlags> {
            [0] = RegionFlags.Surface,
            [36] = RegionFlags.Underground,
            [396] = RegionFlags.Ice,
            [288] = RegionFlags.Jungle,
            [360] = RegionFlags.JungleShrine,
            [576] = RegionFlags.Temple,
            [468] = RegionFlags.Sky,
            [612] = RegionFlags.Underwater,
            [540] = RegionFlags.Spider,
            [1800] = RegionFlags.Granite,
            [1836] = RegionFlags.Marble,
            [144] = RegionFlags.Underworld,
            [72] = RegionFlags.Dungeon,
            [432] = RegionFlags.Livingwood,
            [1152] = RegionFlags.Mushroom,
            [180] = RegionFlags.Barrel,
            [216] = RegionFlags.Trashcan,
            [648] = RegionFlags.Biome,
            [684] = RegionFlags.Biome,
            [720] = RegionFlags.Biome,
            [756] = RegionFlags.Biome,
            [792] = RegionFlags.Biome,
            [2432] = RegionFlags.Desert,
            [2144] = RegionFlags.TrappedUnderground,
            [2360] = RegionFlags.Desert,
        };

        _regionLootInfo = new Dictionary<RegionFlags, List<LootInfo>>();
        _regionExclusiveLootInfo = new Dictionary<RegionFlags, List<LootInfo>>();

        foreach (RegionFlags val in Enum.GetValues<RegionFlags>()) {
            _regionLootInfo.Add(val, new List<LootInfo>());
            _regionExclusiveLootInfo.Add(val, new List<LootInfo>());
        }

        foreach (var loot in ModContent.GetContent<ModChestLoot>()) {
            foreach (KeyValuePair<RegionFlags, List<LootInfo>> pair in loot.LootInfo) {
                _regionLootInfo[pair.Key].AddRange(pair.Value);
            }

            foreach (KeyValuePair<RegionFlags, List<LootInfo>> pair in loot.ExclusiveLootInfo) {
                _regionExclusiveLootInfo[pair.Key].AddRange(pair.Value);
            }
        }
    }

    public void PopulateAllChests() {
        for (int i = 0; i < Main.maxChests; i++) {
            if (i >= Main.chest.Length) {
                return;
            }

            Chest chest = Main.chest[i];

            if (chest != null && Framing.GetTileSafely(chest.x, chest.y) is Tile tile && tile.HasTile) {
                int tileFrameIdentifier = ModContent.GetModTile(tile.TileType) != null
                    ? tile.TileType + 10000 
                    : tile.TileFrameX + (tile.TileType == TileID.Containers2 ? 2000 : 0);

                if (!_framingToRegion.TryGetValue(tileFrameIdentifier, out RegionFlags region)) {
                    continue;
                }

                List<LootInfo> exclusiveItemInfoList = new(_regionExclusiveLootInfo[region]);
                exclusiveItemInfoList.AddRange(_regionExclusiveLootInfo[RegionFlags.All]);

                bool chestStartsWithKey = chest.item[0].type == ItemID.GoldenKey || chest.item[0].type == ItemID.ShadowKey;

                if (!chestStartsWithKey && exclusiveItemInfoList.Count > 0) {
                    LootInfo exclusiveItemInfo =
                        exclusiveItemInfoList[WorldGen.genRand.Next(exclusiveItemInfoList.Count)];

                    if (WorldGen.genRand.NextFloat() < exclusiveItemInfo.Chance)
                    {
                        AddChestItem(exclusiveItemInfo, chest);
                    }
                }

                List<LootInfo> itemInfoList = new(_regionLootInfo[region]);
                itemInfoList.AddRange(_regionLootInfo[RegionFlags.All]);

                itemInfoList = itemInfoList.OrderBy(x => WorldGen.genRand.Next()).ToList();

                if (itemInfoList.Count > 0) {
                    foreach (LootInfo itemInfo in itemInfoList) {
                        if (WorldGen.genRand.NextFloat() < itemInfo.Chance) {
                            AddChestItem(itemInfo, chest);
                        }
                    }
                }
            }
        }
    }

    private void AddChestItem(LootInfo info, Chest chest) {
        int stack = WorldGen.genRand.Next(info.StackRange.Min, info.StackRange.Min + 1);
        int slot = info.SlotIndex;

        if (slot == -1) {
            for (int g = 0; g < chest.item.Length; g++) {
                if (chest.item[g].IsAir) {
                    slot = g;
                    break;
                }
            }
        }

        if (slot != -1 && stack > 0) {
            int itemType = info.ItemTypes[WorldGen.genRand.Next(info.ItemTypes.Length)];
            chest.item[slot].SetDefaults(itemType);
            chest.item[slot].stack = stack;

            chest.item[slot].Prefix(ItemLoader.ChoosePrefix(chest.item[slot], Main.rand));
        }
    }
    
    internal record struct Range(int Min, int Max);

    internal readonly struct LootInfo(int[] types, Range stackRange, float chance, int slotIndex = -1) {
        public readonly int[] ItemTypes = types;
        public readonly Range StackRange = stackRange;
        public readonly float Chance = chance;
        public readonly int SlotIndex = slotIndex;
    }
}