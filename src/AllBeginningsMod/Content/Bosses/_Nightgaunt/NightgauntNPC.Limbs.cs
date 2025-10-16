using AllBeginningsMod.Common;
using System.Runtime.CompilerServices;

namespace AllBeginningsMod.Content.Bosses;

internal partial class NightgauntNPC {
    internal record struct NightgauntLimb(IKSkeleton skeleton, bool anchored = false) {
        public IKSkeleton Skeleton = skeleton;
        public Vector2 TargetPosition = Vector2.Zero;
        public Vector2 EndPosition = Vector2.Zero;
        public bool IsAnchored = anchored;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void UpdateLimbState(ref NightgauntLimb nightgauntLimb, Vector2 basePos, float lerpSpeed, float anchorThreshold) {
        nightgauntLimb.EndPosition = Vector2.Lerp(nightgauntLimb.EndPosition, nightgauntLimb.TargetPosition, lerpSpeed);
        nightgauntLimb.Skeleton.Update(basePos, nightgauntLimb.EndPosition);
        nightgauntLimb.IsAnchored = Vector2.Distance(nightgauntLimb.EndPosition, nightgauntLimb.TargetPosition) < anchorThreshold;
    }

    void CreateLimbs() {
        _rightArm = new NightgauntLimb(new IKSkeleton((36f, new()), (60f, new() { MinAngle = -MathHelper.Pi, MaxAngle = 0f })));
        _leftArm = new NightgauntLimb(new IKSkeleton((36f, new()), (60f, new() { MinAngle = 0f, MaxAngle = MathHelper.Pi })));
        
        _rightLeg = new NightgauntLimb(new IKSkeleton((46f, new()), (60f, new() { MinAngle = 0f, MaxAngle = MathHelper.Pi })));
        _leftLeg = new NightgauntLimb(new IKSkeleton((46f, new()), (60f, new() { MinAngle = -MathHelper.Pi, MaxAngle = 0f })));
    }
}