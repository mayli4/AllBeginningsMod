using System;
using System.Runtime.CompilerServices;

namespace AllBeginningsMod.Common;

public struct IKSkeleton {
    public struct Constraints() {
        public float MinAngle = -MathF.PI;
        public float MaxAngle = MathF.PI;
    }

    [InlineArray(MaxJointCount + 1)]
    struct PositionData { Vector2 _; }
    
    public readonly int JointCount => _options.Length;
    public readonly int PositionCount => JointCount + 1;
    
    const int MaxJointCount = 16;

    readonly (float length, Constraints constraints)[] _options;
    PositionData _previousPositions;
    PositionData _positions;
    float _maxDistance;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 Position(int index) => _positions[index];

    public IKSkeleton(params (float, Constraints)[] options) {
        if(options.Length > MaxJointCount) throw new Exception($"MaxJointCount is less than provided options ({options.Length}).");
        _options = options;

        foreach(var (length, _) in options) _maxDistance += length;
    }

    // http://www.andreasaristidou.com/FABRIK.html 
    public void Update(Vector2 startPosition, Vector2 targetEndPosition) {
        _previousPositions = _positions;

        var distance = UpdateInner(startPosition, targetEndPosition);
        if(distance > 26f) {
            _positions = _previousPositions;
            UpdateInner(
                startPosition,
                targetEndPosition + startPosition.DirectionTo(targetEndPosition) * startPosition.Distance(_positions[PositionCount - 1])
            );
        }
    }

    float UpdateInner(Vector2 startPosition, Vector2 targetEndPosition) {
        var iterations = 2 << 4;
        var distance = startPosition.DistanceSQ(targetEndPosition);
        if(distance > _maxDistance * _maxDistance) iterations = 1;

        for(var k = 0; k < iterations; k += 1) {
            _positions[PositionCount - 1] = targetEndPosition;
            _positions[0] = startPosition;

            float rootAngle;
            for(var i = JointCount - 1; i > 0; i -= 1) {
                var nextAngle = (_positions[i + 1] - _positions[i]).ToRotation();

                rootAngle = (_positions[i] - (i > 1 ? _positions[i - 1] : startPosition)).ToRotation();
                var angle = rootAngle + Math.Clamp(
                    MathHelper.WrapAngle(nextAngle - rootAngle),
                    _options[i].constraints.MinAngle,
                    _options[i].constraints.MaxAngle
                );

                _positions[i] = _positions[i + 1] + (angle + MathF.PI).ToRotationVector2() * _options[i].length;
            }

            rootAngle = 0f;
            for(var i = 0; i < JointCount; i += 1) {
                var nextAngle = (_positions[i + 1] - _positions[i]).ToRotation();
                var angle = rootAngle + Math.Clamp(
                    MathHelper.WrapAngle(nextAngle - rootAngle),
                    _options[i].constraints.MinAngle,
                    _options[i].constraints.MaxAngle
                );

                _positions[i + 1] = _positions[i] + angle.ToRotationVector2() * _options[i].length;
                rootAngle = angle;
            }

            distance = _positions[PositionCount - 1].DistanceSQ(targetEndPosition);
            if(distance <= 0.01f) break;
        }

        return distance;
    }
}
