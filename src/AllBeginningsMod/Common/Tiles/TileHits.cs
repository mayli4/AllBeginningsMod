using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;

namespace AllBeginningsMod.Common.Tiles;

// https://github.com/tModLoader/tModLoader/issues/4735

internal sealed class TileHits : ModSystem {
    internal readonly record struct TileHitCtx(HitTile Tile, int X, int Y, int HitType, Player player);
    
    public Dictionary<Point, TileHitCtx> Contexts { get; } = new();

    public override void OnModLoad() {
        base.OnModLoad();

        // We should ensure this is updated for 1.4.4.
        IL_Player.PlaceThing_TryReplacingTiles += RewriteHitObjectInvocations;
        IL_Player.ItemCheck_UseMiningTools_ActuallyUseMiningTool += RewriteHitObjectInvocations;
        IL_Player.ItemCheck_UseMiningTools_TryHittingWall += RewriteHitObjectInvocations;
        IL_Player.GetOtherPlayersPickTile += RewriteHitObjectInvocations;
        IL_Player.PickTile += RewriteHitObjectInvocations;
        IL_Player.HasEnoughPickPowerToHurtTile += RewriteHitObjectInvocations;
    }

    private void RewriteHitObjectInvocations(ILContext il) {
        var c = new ILCursor(il);

        c.Index = c.Instrs.Count - 1;

        // Jump to every HitTile::HitObject invocation, push Player instance (`this`, ldarg.0), and call our method instead of HitTile::HitObject (jump to label after).
        while (c.TryGotoPrev(MoveType.After, x => x.MatchCallvirt<HitTile>(nameof(HitTile.HitObject)))) {
            // Mark label to jump to later.
            var label = c.MarkLabel();

            // Go to before call
            c.Index--;

            // Emit `this` (Player instance), then invoke our own method
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate(HitObjectWithPlayerContext);

            // Jump to label after the call
            c.Emit(OpCodes.Br, label);
        }
    }

    public override void Unload() {
        base.Unload();

        Contexts.Clear();
    }

    public override void PostUpdateEverything() {
        base.PostUpdateEverything();

        Contexts.Clear();
    }

    public bool TryGetHitTileContext(Point point, out TileHitCtx context) {
        return Contexts.TryGetValue(point, out context);
    }

    private static int HitObjectWithPlayerContext(HitTile hitTile, int x, int y, int hitType, Player player) {
        ModContent.GetInstance<TileHits>().Contexts[new Point(x, y)] = new TileHitCtx(hitTile, x, y, hitType, player);
        return hitTile.HitObject(x, y, hitType);
    }
}