using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace AllBeginningsMod.Common.World;

internal sealed class ProtectedAreaSystem : ModSystem {
    public static List<Rectangle> ProtectedRegions = new List<Rectangle>();

    public static bool IsProtected(int x, int y) {
        if(Main.gameMenu || Main.dedServ)
            return false;

        foreach(Rectangle region in ProtectedRegions) {
            if(region.Contains(x, y))
                return true;
        }
        return false;
    }
    public static void AddProtectedRegion(Rectangle region) {
        if(!ProtectedRegions.Contains(region))
            ProtectedRegions.Add(region);
    }

    public static void RemoveProtectedRegion(Rectangle region) {
        ProtectedRegions.Remove(region);
    }

    public override void PreWorldGen() {
        ProtectedRegions.Clear();
    }

    public override void LoadWorldData(TagCompound tag) {
        ProtectedRegions.Clear();

        int length = tag.GetInt("AllBeginnings_ProtectedRegionCount");

        for(int k = 0; k < length; k++) {
            ProtectedRegions.Add(new Rectangle
            (
                tag.GetInt($"AllBeginnings_x{k}"),
                tag.GetInt($"AllBeginnings_y{k}"),
                tag.GetInt($"AllBeginnings_w{k}"),
                tag.GetInt($"AllBeginnings_h{k}")
                )
            );
        }
    }

    public override void SaveWorldData(TagCompound tag) {
        tag["AllBeginnings_ProtectedRegionCount"] = ProtectedRegions.Count;

        for(int k = 0; k < ProtectedRegions.Count; k++) {
            Rectangle region = ProtectedRegions[k];
            tag.Add($"AllBeginnings_x{k}", region.X);
            tag.Add($"AllBeginnings_y{k}", region.Y);
            tag.Add($"AllBeginnings_w{k}", region.Width);
            tag.Add($"AllBeginnings_h{k}", region.Height);
        }
    }

    public override void NetSend(BinaryWriter writer) {
        writer.Write(ProtectedRegions.Count);

        for(int i = 0; i < ProtectedRegions.Count; i++) {
            var region = ProtectedRegions[i];
            writer.Write(region.X);
            writer.Write(region.Y);
            writer.Write(region.Width);
            writer.Write(region.Height);
        }
    }

    public override void NetReceive(BinaryReader reader) {
        ProtectedRegions.Clear();

        int numRegions = reader.ReadInt32();

        for(int i = 0; i < numRegions; i++) {
            ProtectedRegions.Add(new Rectangle
            {
                X = reader.ReadInt32(),
                Y = reader.ReadInt32(),
                Width = reader.ReadInt32(),
                Height = reader.ReadInt32()
            });
        }
    }
}

internal sealed class ProtectionGlobalItem : GlobalItem {
    private static List<int> _blacklist = new List<int> {
        ItemID.WaterBucket, ItemID.LavaBucket, ItemID.HoneyBucket, ItemID.BottomlessBucket,
        ItemID.Wrench, ItemID.BlueWrench, ItemID.GreenWrench, ItemID.YellowWrench, ItemID.MulticolorWrench,
        ItemID.ActuationRod, ItemID.Actuator, ItemID.WireKite, ItemID.WireCutter, ItemID.WireBulb,
        ItemID.Paintbrush, ItemID.PaintRoller, ItemID.PaintScraper,
        ItemID.SpectrePaintbrush, ItemID.SpectrePaintRoller, ItemID.SpectrePaintScraper
    };

    public override void Load() {
        On_Player.PickTile += DontPickInZone;
        On_Player.PickWall += DontPickWallInZone;
        On_WorldGen.PlaceTile += DontManuallyPlaceInZone;
        On_WorldGen.PoundTile += DontPoundTile;
        On_WorldGen.PlaceWire += DontPlaceWire;
        On_WorldGen.PlaceWire2 += DontPlaceWire2;
        On_WorldGen.PlaceWire3 += DontPlaceWire3;
        On_WorldGen.PlaceWire4 += DontPlaceWire4;
        On_WorldGen.PlaceActuator += DontPlaceActuator;
        On_WorldGen.KillTile += DontExplodeAtRuntime;
    }

    public override void Unload() {
        On_Player.PickTile -= DontPickInZone;
        On_Player.PickWall -= DontPickWallInZone;
        On_WorldGen.PlaceTile -= DontManuallyPlaceInZone;
        On_WorldGen.PoundTile -= DontPoundTile;
        On_WorldGen.PlaceWire -= DontPlaceWire;
        On_WorldGen.PlaceWire2 -= DontPlaceWire2;
        On_WorldGen.PlaceWire3 -= DontPlaceWire3;
        On_WorldGen.PlaceWire4 -= DontPlaceWire4;
        On_WorldGen.PlaceActuator -= DontPlaceActuator;
        On_WorldGen.KillTile -= DontExplodeAtRuntime;
    }

    private bool DontPoundTile(On_WorldGen.orig_PoundTile orig, int x, int y) {
        if(ProtectedAreaSystem.IsProtected(x, y)) {
            return false;
        }
        return orig(x, y);
    }

    private bool DontPlaceWire(On_WorldGen.orig_PlaceWire orig, int x, int y) {
        if(ProtectedAreaSystem.IsProtected(x, y)) {
            return false;
        }
        return orig(x, y);
    }
    private bool DontPlaceWire2(On_WorldGen.orig_PlaceWire2 orig, int x, int y) {
        if(ProtectedAreaSystem.IsProtected(x, y)) {
            return false;
        }
        return orig(x, y);
    }
    private bool DontPlaceWire3(On_WorldGen.orig_PlaceWire3 orig, int x, int y) {
        if(ProtectedAreaSystem.IsProtected(x, y)) {
            return false;
        }
        return orig(x, y);
    }
    private bool DontPlaceWire4(On_WorldGen.orig_PlaceWire4 orig, int x, int y) {
        if(ProtectedAreaSystem.IsProtected(x, y)) {
            return false;
        }
        return orig(x, y);
    }

    private bool DontPlaceActuator(On_WorldGen.orig_PlaceActuator orig, int x, int y) {
        if(ProtectedAreaSystem.IsProtected(x, y)) {
            return false;
        }
        return orig(x, y);
    }

    private void DontPickWallInZone(On_Player.orig_PickWall orig, Player self, int x, int y, int damage) {
        if(ProtectedAreaSystem.IsProtected(x, y)) {
            return;
        }
        orig(self, x, y, damage);
    }

    private void DontPickInZone(On_Player.orig_PickTile orig, Player self, int x, int y, int pickPower) {
        if(ProtectedAreaSystem.IsProtected(x, y)) {
            return;
        }
        orig(self, x, y, pickPower);
    }

    private bool DontManuallyPlaceInZone(On_WorldGen.orig_PlaceTile orig, int i, int j, int type, bool mute, bool forced, int plr, int style) {
        if(ProtectedAreaSystem.IsProtected(i, j)) {
            return false;
        }
        return orig(i, j, type, mute, forced, plr, style);
    }

    private void DontExplodeAtRuntime(On_WorldGen.orig_KillTile orig, int i, int j, bool fail, bool effectOnly, bool noItem) {
        if(ProtectedAreaSystem.IsProtected(i, j) && !WorldGen.generatingWorld) {
            return;
        }
        orig(i, j, fail, effectOnly, noItem);
    }

    public override bool CanUseItem(Item item, Player player) {
        if(player != Main.LocalPlayer)
            return base.CanUseItem(item, player);

        if(item.createTile != -1 || item.createWall != -1 || _blacklist.Contains(item.type)) {
            var targetPoint = Main.SmartCursorIsUsed ? new Point16(Main.SmartCursorX, Main.SmartCursorY) : new Point16(Player.tileTargetX, Player.tileTargetY);
            if(ProtectedAreaSystem.IsProtected(targetPoint.X, targetPoint.Y)) {
                return false;
            }
        }
        return base.CanUseItem(item, player);
    }
}


internal sealed class ProtectionGlobalTile : GlobalTile {
    public override bool CanExplode(int i, int j, int type) {
        if(ProtectedAreaSystem.IsProtected(i, j)) {
            return false;
        }
        return base.CanExplode(i, j, type);
    }
}