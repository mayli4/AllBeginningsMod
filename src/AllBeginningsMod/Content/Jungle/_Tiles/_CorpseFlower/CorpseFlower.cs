using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ObjectData;

namespace AllBeginningsMod.Content.Jungle;

internal sealed class CorpseFlower : ModTile {
    public override string Texture => Textures.Tiles.Jungle.KEY_CorpseFlowerBase;
    
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = false;
        Main.tileTable[Type] = true;
        Main.tileLavaDeath[Type] = true;

        Main.tileSolid[Type] = false;
        
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.Width = 3;
        TileObjectData.newTile.Height = 2;
        
        TileObjectData.newTile.Origin = new Point16(0, 0);

        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        
        TileObjectData.newTile.CoordinateHeights = new [] { 16, 16, 16 };
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.DrawYOffset = 2;
        
        TileObjectData.addTile(Type);
        
        AddMapEntry(Color.Brown);

        DustType = DustID.RichMahogany;
        HitSound = SoundID.Dig;
    }
    
    public override void NearbyEffects(int i, int j, bool closer) {
        var tile = Main.tile[i, j];
        if(tile.TileFrameX is not 18 || tile.TileFrameY is not 0) return;

        var platforms = from plat 
                    in Main.npc 
                    where plat.active 
                    where plat.type == ModContent.NPCType<CorpseFlowerBulb>() 
                    select plat.ModNPC as CorpseFlowerBulb;

        var pos = new Point16(i + 1, j - 2);
        
        if (!platforms.Any(p => p.ParentPosition == pos)) {
            int npcIndex = NPC.NewNPC(
                new EntitySource_SpawnNPC(),
                (i * 16),
                (j * 16),
                ModContent.NPCType<CorpseFlowerBulb>()
            );
        
            if (npcIndex != -1) {
                CorpseFlowerBulb newPlatform = (CorpseFlowerBulb)Main.npc[npcIndex].ModNPC;
                newPlatform.ParentPosition = pos;
                newPlatform.Bloomed = false;
                Main.npc[npcIndex].netUpdate = true;
            }
        }
    }
}

internal sealed class CorpseFlowerBulb : ModNPC {
    public override string Texture => Textures.Tiles.Jungle.KEY_CorpseFlowerBase;
    
    public Point16 ParentPosition { get; set; }

    public bool Bloomed { get; set; } = false;

    private bool _wasBloomed = false;

    public override void SetDefaults() {
        NPC.width = 94;
        NPC.height = 90;
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.lifeMax = int.MaxValue;
        NPC.knockBackResist = 0f;
        NPC.immortal = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.dontTakeDamage = false;
        NPC.value = 0f;
        NPC.aiStyle = -1;
        NPC.HitSound = null;
        NPC.DeathSound = null;
        NPC.dontCountMe = true;
        NPC.ShowNameOnHover = false;
    }

    public override void AI() {
        if (ParentPosition.X == 0 && ParentPosition.Y == 0 && NPC.Center != Vector2.Zero) {
            ParentPosition = NPC.Center.ToPoint16();
        }
        
        NPC.Center = ParentPosition.ToVector2() * 16;

        if(Bloomed)
            NPC.dontTakeDamage = true;
        
        if (Bloomed && !_wasBloomed) {
            SoundEngine.PlaySound(Sounds.Tile.Jungle.CorpseFlowerOpen with { PitchVariance = 0.5f}, NPC.Center);
        }
        
        _wasBloomed = Bloomed;
    }
    
    public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers) => modifiers.HideCombatText();
    public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
        if (!Bloomed) {
            Bloomed = true;
            NPC.netUpdate = true;
        }
    }
    public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
        if (!Bloomed) {
            Bloomed = true;
            NPC.netUpdate = true;
        }
    }

    public override void HitEffect(NPC.HitInfo hit) {
        base.HitEffect(hit);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var tile = Main.tile[ParentPosition.X - 1, ParentPosition.Y + 2];
        var tex = TextureAssets.Npc[Type].Value;
        
        if (tile.HasTile && tile.TileColor != PaintID.None) {
            var paintedTex = Main.instance.TilePaintSystem.TryGetTileAndRequestIfNotReady(ModContent.TileType<CorpseFlower>(), 0, tile.TileColor);
            tex = paintedTex ?? tex;
        }
        
        Vector2 scale = Vector2.One;
        Rectangle currentRect = default(Rectangle);
        var unbloomedRect = new Rectangle(66, 0, 122, 94);
        var bloomedRect = new Rectangle(190, 0, 122, 94);

        currentRect = Bloomed ? bloomedRect : unbloomedRect;
        
        var origin = new Vector2(currentRect.Width / 2f, currentRect.Height / 2f);
        
        var pos = this.NPC.position + new Vector2(40, 58) - Main.screenPosition;
        
        Main.EntitySpriteDraw(
            tex,
            pos,
            currentRect,
            drawColor,
            0.0f,
            origin, 
            scale,
            SpriteEffects.None
        );
        
        return false;
    }
}