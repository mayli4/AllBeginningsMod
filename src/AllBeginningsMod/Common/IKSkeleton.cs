using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;

namespace AllBeginningsMod.Common;

public struct IKConstraints {
    [Flags]
    enum Flags {
        AngleDifference,
        UpwardOnlyAngle,
    }
    record struct AngleDifference(float MaxAngle);
    record struct UpwardOnlyAngle(float UpAngle);

    Flags _flags;
    AngleDifference _angleDifference;
    UpwardOnlyAngle _upwardOnlyAngle;

    public void SetUpwardOnlyAngleConstraint(Vector2? up = null) {
        _flags |= Flags.UpwardOnlyAngle;
        _upwardOnlyAngle = new((up ?? -Vector2.UnitY).ToRotation());
    }

    public void SetAngleDifferenceConstraint(float maxAngle) {
        _flags |= Flags.AngleDifference;
        _angleDifference = new(maxAngle);
    }

    public readonly float Penalties(Span<IKSkeleton.Joint> joints, int index, float completion) {
        if(index == 0) return 0f;

        float penalties = 0f;
        if((_flags & Flags.AngleDifference) != 0) {
            float rotation = joints[index].Rotation;
            float previousRotation = joints[index - 1].Rotation;

            penalties += Interpolation.InverseLerp(_angleDifference.MaxAngle, MathF.PI, rotation - previousRotation) * 9f;
        }

        if((_flags & Flags.UpwardOnlyAngle) != 0) {
            float rotation = joints[index].Rotation;
            float upAngle = _upwardOnlyAngle.UpAngle;

            float factor = Interpolation.InverseLerp(0.4f, 0f, completion) * 1500f;
            penalties += Interpolation.InverseLerp(0.01f, MathF.PI, rotation - upAngle) * factor;
        }

        return penalties;
    }
}

public struct IKSkeleton {
    public record struct JointOptions(float Length, IKConstraints Constraints = default);

    public IKSkeleton(params ReadOnlySpan<JointOptions> options) {
        _jointCount = options.Length;
        for(var i = 0; i < options.Length; i += 1) {
            _joints[i] = new(options[i].Length, 0f);
            _constraints[i] = options[i].Constraints;
        }
    }

    public readonly int JointCount => _jointCount;
    public Vector2 EndPosition { get; private set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 Position(int index) => _positions[index];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float Rotation(int index) => _joints[index].Rotation;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float Length(int index) => _joints[index].Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 Offset(int index) => Rotation(index).ToRotationVector2() * Length(index);

    public void Update(Vector2 startPosition, Vector2 targetEndPosition, int stepCount = 400, float updateStep = 0.24f) {
        Span<float> gradient = stackalloc float[_jointCount];
        for(var i = 0; i < stepCount; i += 1) {
            float progress = (float)i / stepCount;
            for(var j = 0; j < _jointCount; j += 1) {
                ref Joint joint = ref _joints[j];
                var initialRotation = joint.Rotation;
                var initialEndPosition = EndPosition;

                const float HalfDerivativeOffset = 0.00034526f;

                joint.Rotation = initialRotation + HalfDerivativeOffset;
                RecalculatePositions(startPosition);
                double lossLeft = LossFunction(
                    targetEndPosition,
                    EndPosition,
                    progress,
                    j,
                    _joints[0.._jointCount],
                    in _constraints[j]
                );

                joint.Rotation = initialRotation - HalfDerivativeOffset;
                RecalculatePositions(startPosition);
                double lossRight = LossFunction(
                    targetEndPosition,
                    EndPosition,
                    progress,
                    j,
                    _joints[0.._jointCount],
                    in _constraints[j]
                );

                joint.Rotation = initialRotation;
                EndPosition = initialEndPosition;

                const double InverseDerivativeOffset = 1448.1546;
                double loss = (lossLeft - lossRight) * InverseDerivativeOffset;

                const double maxAbsoluteValue = 0.17f;
                gradient[j] = (float)(Math.Tanh(loss / maxAbsoluteValue) * maxAbsoluteValue);
            }

            float factor = MathF.Sqrt(1f - progress);
            for(var j = 0; j < _jointCount; j++) {
                _joints[j].Rotation = MathHelper.WrapAngle(_joints[j].Rotation - gradient[j] * factor * updateStep);
            }

            RecalculatePositions(startPosition);
            if(EndPosition.WithinRange(targetEndPosition, 7.4f)) break;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double LossFunction(
            Vector2 targetEndPosition,
            Vector2 endPosition,
            float completion,
            int index,
            Span<Joint> joints,
            in IKConstraints constraints
        ) {
            float distance = targetEndPosition.Distance(endPosition);
            float penalties = constraints.Penalties(joints, index, completion);

            return Math.Pow(distance + penalties, 0.1f);
        }
    }

    void RecalculatePositions(Vector2 startPosition) {
        var position = startPosition;
        for(var i = 0; i < _jointCount; i += 1) {
            var joint = _joints[i];

            _positions[i] = position;
            position += joint.Rotation.ToRotationVector2() * joint.Length;
        }

        EndPosition = position;
    }

    public record struct Joint(float Length, float Rotation);

    const int MaxJointCount = 16;

    [InlineArray(MaxJointCount)]
    struct Joints {
        Joint _;
    }

    [InlineArray(MaxJointCount)]
    struct Positions {
        Vector2 _;
    }

    [InlineArray(MaxJointCount)]
    struct Constraints {
        IKConstraints _;
    }

    readonly int _jointCount;
    Joints _joints;
    Positions _positions;
    Constraints _constraints;
}
