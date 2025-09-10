using System;

namespace AllBeginningsMod.Common.World;

[Flags]
public enum RegionFlags {
    None = 0x0,
    All = 0x1,
    Surface = 0x2,
    Underground = 0x4,
    Ice = 0x8,
    Livingwood = 0x10,
    Desert = 0x20,
    Sky = 0x40,
    Underwater = 0x80,
    Spider = 0x100,
    Granite = 0x200,
    Marble = 0x400,
    Underworld = 0x800,
    Dungeon = 0x1000,
    Biome = 0x2000,
    Jungle = 0x4000,
    JungleShrine = 0x8000,
    Temple = 0x10000,
    Mushroom = 0x20000,
    TrappedUnderground = 0x40000,
    Barrel = 0x80000,
    Trashcan = 0x100000,
}