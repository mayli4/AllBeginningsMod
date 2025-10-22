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
    
    private NightgauntLimb _body;
    private NightgauntLimb _rightArm;
    private NightgauntLimb _leftArm;
    private NightgauntLimb _rightLeg;
    private NightgauntLimb _leftLeg;
    
    private NightgauntLimb _headNeck;

// The point on the body IK chain where the neck roots
    private const int NeckRootBodySegmentIndex = 0; // Typically the very first segment (closest to NPC.Center)

// NEW: Offset for the neck's attachment point relative to NeckRootBodySegmentIndex
    readonly static Vector2 NeckAttachmentOffset = new(0, -10);

    public static Vector2 ShoulderOffset;
    
    public Vector2 RightShoulderPosition {
        get {
            var shoulderSegmentUp = (_body.Skeleton.Position(1) - _body.Skeleton.Position(0)).SafeNormalize(Vector2.UnitY);
            var shoulderSegmentRight = shoulderSegmentUp.RotatedBy(MathHelper.PiOver2);

            return _body.Skeleton.Position(0) 
                   + shoulderSegmentRight * ShoulderOffset.X
                   - shoulderSegmentUp * ShoulderOffset.Y;
        }
    }
    
    public Vector2 LeftShoulderPosition {
        get {
            var shoulderSegmentUp = (_body.Skeleton.Position(1) - _body.Skeleton.Position(0)).SafeNormalize(Vector2.UnitY);
            var shoulderSegmentRight = shoulderSegmentUp.RotatedBy(MathHelper.PiOver2);

            return _body.Skeleton.Position(0) 
                   - shoulderSegmentRight * ShoulderOffset.X
                   - shoulderSegmentUp * ShoulderOffset.Y;
        }
    }

    public static Vector2 LegOffset;
    public Vector2 RightLegBasePosition {
        get {
            var hipSegmentUp = (_body.Skeleton.Position(2) - _body.Skeleton.Position(1)).SafeNormalize(Vector2.UnitY);
            var hipSegmentRight = hipSegmentUp.RotatedBy(MathHelper.PiOver2);

            return _body.Skeleton.Position(2) 
                   + hipSegmentRight * LegOffset.X
                   - hipSegmentUp * LegOffset.Y;
        }
    }

    public Vector2 LeftLegBasePosition {
        get {
            var hipSegmentUp = (_body.Skeleton.Position(2) - _body.Skeleton.Position(1)).SafeNormalize(Vector2.UnitY);
            var hipSegmentRight = hipSegmentUp.RotatedBy(MathHelper.PiOver2);

            return _body.Skeleton.Position(2) 
                   - hipSegmentRight * LegOffset.X
                   - hipSegmentUp * LegOffset.Y;
        }
    }

    private IKSkeleton _bodySkeleton;
    private Vector2 _bodyTargetPosition;
    
    Vector2 _rightArmTargetPosition;
    Vector2 _rightArmEndPosition;
    bool _rightArmAnchored;

    Vector2 _leftArmTargetPosition;
    Vector2 _leftArmEndPosition;
    bool _leftArmAnchored;
    
    Vector2 _rightLegTargetPosition;
    Vector2 _rightLegEndPosition;
    bool _rightLegAnchored;

    Vector2 _leftLegTargetPosition;
    Vector2 _leftLegEndPosition;
    bool _leftLegAnchored;

    int _handSwapTimer;
    bool _rightHandSwap;
    
    int _legSwapTimer;
    bool _rightLegSwap;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void UpdateLimbState(ref NightgauntLimb nightgauntLimb, Vector2 basePos, float lerpSpeed, float anchorThreshold) {
        nightgauntLimb.EndPosition = Vector2.Lerp(nightgauntLimb.EndPosition, nightgauntLimb.TargetPosition, lerpSpeed);
        nightgauntLimb.Skeleton.Update(basePos, nightgauntLimb.EndPosition);
        nightgauntLimb.IsAnchored = Vector2.Distance(nightgauntLimb.EndPosition, nightgauntLimb.TargetPosition) < anchorThreshold;
    }

    void CreateLimbs() {
        float upperArmLength = 76f;
        float forearmLength = 54f;
        float handLength = 15f;

        var shoulderConstraints = new IKSkeleton.Constraints();
        var rightElbowConstraints = new IKSkeleton.Constraints() { MinAngle = -MathHelper.Pi, MaxAngle = 0f };
        var leftElbowConstraints = new IKSkeleton.Constraints() { MinAngle = 0f, MaxAngle = MathHelper.Pi };

        var wristConstraints = new IKSkeleton.Constraints() { MinAngle = -MathHelper.PiOver4, MaxAngle = MathHelper.PiOver4 };

        _rightArm = new NightgauntLimb(new IKSkeleton(
            (upperArmLength, shoulderConstraints),
            (forearmLength, rightElbowConstraints),
            (handLength, wristConstraints)
        ));
        _leftArm = new NightgauntLimb(new IKSkeleton(
            (upperArmLength, shoulderConstraints),
            (forearmLength, leftElbowConstraints),
            (handLength, wristConstraints)
        ));
        
        float thighLength = 68f;
        float calfLength = 58f;
        float footLength = 15f;

        var hipConstraints = new IKSkeleton.Constraints();
        var rightKneeConstraints = new IKSkeleton.Constraints() { MinAngle = 0f, MaxAngle = MathHelper.Pi };
        var leftKneeConstraints = new IKSkeleton.Constraints() { MinAngle = -MathHelper.Pi, MaxAngle = 0f };
        var ankleConstraints = new IKSkeleton.Constraints() { MinAngle = -MathHelper.PiOver4, MaxAngle = MathHelper.PiOver4 };

        _rightLeg = new NightgauntLimb(new IKSkeleton(
            (thighLength, hipConstraints),
            (calfLength, rightKneeConstraints),
            (footLength, ankleConstraints)
        ));
        
        _leftLeg = new NightgauntLimb(new IKSkeleton(
            (thighLength, hipConstraints),
            (calfLength, leftKneeConstraints),
            (footLength, ankleConstraints)
        ));
        
        float torsoLength = 40f;
        float assLength = 30f;

        _body = new NightgauntLimb(new IKSkeleton(
            (torsoLength, new() { }),
            (torsoLength, new() { }),
            (assLength, new() { })
        ));
        
        float neckBaseLength = 30f;
        float midNeckLength = 20f;
        float headLength = 60f;

        var neckBaseConstraints = new IKSkeleton.Constraints() {  };

        var midNeckConstraints = new IKSkeleton.Constraints() { MinAngle = -MathHelper.PiOver2 * 0.9f, MaxAngle = MathHelper.PiOver2 * 0.9f };
        var headJointConstraints = new IKSkeleton.Constraints() { MinAngle = -MathHelper.PiOver4, MaxAngle = MathHelper.PiOver4 };

        _headNeck = new NightgauntLimb(new IKSkeleton(
            (neckBaseLength, neckBaseConstraints),
            (midNeckLength, midNeckConstraints),
            (headLength, headJointConstraints)
        ));
    }
}