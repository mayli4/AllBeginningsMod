using AllBeginningsMod.Common;
using AllBeginningsMod.Utilities;
using System.Runtime.CompilerServices;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace AllBeginningsMod.Content.Jungle;

//ty scalar for helping with grappling

internal class TreePlatformPlayer : ModPlayer {
	public override void PreUpdateMovement() {
		foreach (var p in GiantMahoganyTree.Platforms) {
			var lowRect = Player.getRect() with { Height = Player.height / 2, Y = (int)Player.position.Y + Player.height / 2 };
			if (lowRect.Intersects(p.NPC.Hitbox) && Player.velocity.Y >= 0 && !Player.FallThrough()) {
				p.UpdateStanding(Player);

				if (Player.controlDown)
					Player.GetModPlayer<CollisionPlayer>().fallThrough = true;

                break;
            }
		}
	}
}

internal class TreePlatformImpl : ILoadable {
	public double OldTreeWindCounter { get; private set; }

	public void Load(Mod mod) {
		On_Projectile.AI_007_GrapplingHooks += CheckGrappling;
		On_NPC.UpdateCollision += CheckNPCCollision;
	}

    public void Unload() {
        On_Projectile.AI_007_GrapplingHooks -= CheckGrappling;
        On_NPC.UpdateCollision -= CheckNPCCollision;
        TileSwaying.PreUpdateWind += PreserveWindCounter;
    }
    
    private void PreserveWindCounter() => OldTreeWindCounter = TileSwaying.Instance.TreeWindCounter;

	private static void CheckGrappling(On_Projectile.orig_AI_007_GrapplingHooks orig, Projectile self) {
		foreach (var p in GiantMahoganyTree.Platforms) {
			const int height = 4;
			var hitbox = new Rectangle(p.NPC.Hitbox.X, p.NPC.Hitbox.Y + height + 16, p.NPC.Hitbox.Width, height);

			if (self.getRect().Intersects(hitbox) && !Collision.SolidCollision(self.Bottom, self.width, 8)) {
				self.Center = new Vector2(self.Center.X, hitbox.Center.Y);
				Latch(self);
			}
		}

		orig(self);

		static void Latch(Projectile p) {
			var owner = Main.player[p.owner];

			p.ai[0] = 2f;
			p.velocity *= 0;
			p.netUpdate = true;

			owner.grappling[0] = p.whoAmI;
			owner.grapCount++;
			owner.GrappleMovement();
        }
	}

	private static void CheckNPCCollision(On_NPC.orig_UpdateCollision orig, NPC self) {
		if (!self.noGravity) {
			foreach (var p in GiantMahoganyTree.Platforms)
				if (self.getRect().Intersects(p.NPC.Hitbox) && self.velocity.Y >= 0)
					p.UpdateStanding(self);
		}

		orig(self);
	}
}

internal sealed class TreetopPlatformNPC : ModNPC {
    public Point16 TreePosition {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new((int)NPC.ai[0], (int)NPC.ai[1]);
        set {
            NPC.ai[0] = value.X;
            NPC.ai[1] = value.Y;
        }
    }
    
    public Point16 WindSourceTile {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new((int)NPC.ai[2], (int)NPC.ai[3]);
        set {
            NPC.ai[2] = value.X;
            NPC.ai[3] = value.Y;
        }
    }

    public override string Texture => Helper.PlaceholderTextureKey;

    public override void SetStaticDefaults() {
        Main.npcFrameCount[NPC.type] = 1;
    }

    public override void SetDefaults() {
        NPC.width = 180;
        NPC.height = 8;
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.lifeMax = 1;
        NPC.knockBackResist = 0f;
        NPC.immortal = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.dontTakeDamage = true;
        NPC.value = 0f;
        NPC.aiStyle = -1;
        NPC.HitSound = null;
        NPC.DeathSound = null;
        NPC.dontCountMe = true;
        NPC.ShowNameOnHover = false;
    }

    public override void AI() {
        if (TreePosition.X == 0 && TreePosition.Y == 0 && NPC.Center != Vector2.Zero) {
            TreePosition = NPC.Center.ToPoint16();
        }

        if (WindSourceTile.X == 0 && WindSourceTile.Y == 0 && (TreePosition.X != 0 || TreePosition.Y != 0)) {
            WindSourceTile = TreePosition;
        }

        var tilePos = TreePosition;
        
        Point16 checkTileCoord = WindSourceTile; 
        if (checkTileCoord.X == 0 && checkTileCoord.Y == 0) {
            checkTileCoord = tilePos;
        }

        if (!WorldGen.InWorld(checkTileCoord.X, checkTileCoord.Y)) {
            NPC.active = false;
            NPC.netUpdate = true;
            return;
        }

        Tile treeTile = Main.tile[checkTileCoord.X, checkTileCoord.Y];
        if (!treeTile.HasTile || treeTile.TileType != ModContent.TileType<GiantMahoganyTree>()) {
            NPC.active = false;
            NPC.netUpdate = true;
            return;
        }

        NPC.Center = tilePos.ToVector2() * 16;
    }

    public void UpdateStanding(Entity entity) {
        if (TreePosition.X == 0 && TreePosition.Y == 0)
            return;

        Point16 actualWindSource = WindSourceTile;
        if (actualWindSource.X == 0 && actualWindSource.Y == 0) {
            actualWindSource = TreePosition;
        }

        float currentRotation = GiantMahoganyTree.GetSway(actualWindSource.X, actualWindSource.Y);

        var oldWindCounter = ModContent.GetInstance<TreePlatformImpl>().OldTreeWindCounter;
        float previousRotation = GiantMahoganyTree.GetSway(actualWindSource.X, actualWindSource.Y, oldWindCounter);

        float diff = currentRotation - previousRotation;

        float strength = (entity.Center.X - NPC.Center.X) / (NPC.width * .5f);
        float disp = (entity is NPC) ? 5f : 10f;

        entity.velocity.Y = 0;  
        var newPosition = new Vector2(
            entity.position.X + diff * disp,
            NPC.Hitbox.Top + 10 - entity.height + currentRotation * strength * disp
        );

        if (!Collision.SolidCollision(newPosition, entity.width, entity.height)) {
            entity.position = newPosition;
        }

        if (entity is Player player) {
            player.Rotate(currentRotation * .07f, new Vector2(player.width * .5f, player.height));
            player.gfxOffY = 0;
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        //spriteBatch.Draw(Textures.Sample.Blobs.Value, NPC.position - screenPos, Color.White);
        
        return false;
    }
}