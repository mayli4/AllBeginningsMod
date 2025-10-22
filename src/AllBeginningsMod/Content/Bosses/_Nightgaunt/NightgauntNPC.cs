using AllBeginningsMod.Common;
using AllBeginningsMod.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace AllBeginningsMod.Content.Bosses;

[AutoloadBossHead]
internal partial class NightgauntNPC : ModNPC {
    internal enum NightgauntState {
        Idle,
        Crawling,
    }

    internal enum Attack {
        SwingLunge
    }

    [StructLayout(LayoutKind.Explicit, Size = sizeof(float) * 4)]
    internal struct NightgauntAi() {
        [FieldOffset(0)] public NightgauntState State;
    }

    public override string Texture => Textures.NPCs.Bosses.Nightgaunt.KEY_NightgauntNPC;

    public Player Target => Main.player[NPC.target];

    private NightgauntState State {
        get => (NightgauntState)NPC.ai[0];
        set {
            NPC.ai[0] = (int)value;
            NPC.netUpdate = true;
        }
    }
    private List<Vector2> _tailPoints = new();
    private const int tail_points = 20;
    private const float tail_segment_length = 10f;

    private readonly static Vector2 TailRootOffset = new(0, 10);

    float _distanceToTarget;
    Vector2 _directionToTarget;

    Vector2 _up;
    Vector2 _right;
    Vector2 _left;

    public override void SetDefaults() {
        NPC.width = 180;
        NPC.height = 140;
        NPC.damage = 15;
        NPC.defense = 8;
        NPC.lifeMax = 1500;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.value = 100f;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.boss = true;
    }

    public override void OnSpawn(IEntitySource source) {
        CreateLimbs();

        LegOffset = new(-15, -40);
        ShoulderOffset = new(-38, -30);
    }

    private void ResetState() {
        State = 0;
        NPC.netUpdate = true;
    }

    public override void AI() {
        NPC.TargetClosest(false);
        ref NightgauntAi ai = ref Unsafe.As<float, NightgauntAi>(ref NPC.ai[0]);

        if(_leftLeg == default || _rightLeg == default || _leftArm == default || _rightArm == default) {
            CreateLimbs();
        }

        switch(State) {
            case NightgauntState.Idle:
                break;
            case NightgauntState.Crawling:
                CrawlToPlayer();
                break;
        }

        State = NightgauntState.Crawling;

        UpdateTail();
    }

    private void UpdateTail() {
        Vector2 bodyTailRootSegmentPosition = _body.Skeleton.Position(3);

        Vector2 segmentBeforeTailRootUp;
        segmentBeforeTailRootUp =
            (_body.Skeleton.Position(3) - _body.Skeleton.Position(3 - 1)).SafeNormalize(Vector2.UnitY);
        Vector2 segmentBeforeTailRootRight = segmentBeforeTailRootUp.RotatedBy(MathHelper.PiOver2);

        float tailOffset = -20f;

        Vector2 tailRoot = bodyTailRootSegmentPosition
                           + segmentBeforeTailRootRight
                           - segmentBeforeTailRootUp * tailOffset;


        if(_tailPoints.Count == 0) {
            _tailPoints.Add(tailRoot);
            for(int i = 1; i < tail_points; i++) {
                _tailPoints.Add(tailRoot - segmentBeforeTailRootUp * tail_segment_length * i);
            }
        }

        _tailPoints[0] = tailRoot;

        for(int i = 1; i < _tailPoints.Count; i++) {
            Vector2 prevPoint = _tailPoints[i - 1];
            Vector2 currentPoint = _tailPoints[i];

            Vector2 direction = (prevPoint - currentPoint).SafeNormalize(Vector2.Zero);
            _tailPoints[i] = prevPoint - direction * tail_segment_length;

            float sway = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 0.1f + i * 0.5f) * 1f;
            _tailPoints[i] += direction.RotatedBy(MathHelper.PiOver2) * sway;
        }

        while(_tailPoints.Count > tail_points) {
            _tailPoints.RemoveAt(_tailPoints.Count - 1);
        }
        while(_tailPoints.Count < tail_points) {
            Vector2 lastPoint = _tailPoints[^1];
            Vector2 secondLastPoint = _tailPoints.Count > 1 ? _tailPoints[^2] : tailRoot;
            Vector2 extendDirection = (lastPoint - secondLastPoint).SafeNormalize(Vector2.Zero);
            _tailPoints.Add(lastPoint + extendDirection * tail_segment_length);
        }
    }
}