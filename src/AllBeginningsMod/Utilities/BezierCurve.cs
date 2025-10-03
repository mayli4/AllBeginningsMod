using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AllBeginningsMod.Utilities;

internal ref struct BezierCurve : IDisposable {
    private readonly ReadOnlySpan<Vector2> _controlPoints;
    private readonly Vector2[] _rentedArray;

    private readonly bool _ownsArray;

    public readonly Vector2 this[int x] {
        get => _controlPoints[x];
    }

    public int ControlPointCount => _controlPoints.Length;

    public BezierCurve(params Vector2[] controls) {
        if(controls == null || controls.Length < 2) {
            throw new ArgumentException("beziers require at least two control points.", nameof(controls));
        }

        _rentedArray = ArrayPool<Vector2>.Shared.Rent(controls.Length);
        controls.AsSpan().CopyTo(_rentedArray.AsSpan(0, controls.Length));
        _controlPoints = _rentedArray.AsSpan(0, controls.Length);
    }

    public BezierCurve(ReadOnlySpan<Vector2> controls) {
        if(controls.Length < 2) {
            throw new ArgumentException("beziers require at least two control points.", nameof(controls));
        }

        _controlPoints = controls;
        _rentedArray = null;
        _ownsArray = false;
    }

    public void Dispose() {
        if(_ownsArray && _rentedArray != null) {
            ArrayPool<Vector2>.Shared.Return(_rentedArray, clearArray: false);
        }
    }

    public Vector2 Evaluate(float T) {
        T = Math.Clamp(T, 0f, 1f);

        if(_controlPoints.Length == 2) {
            return Vector2.Lerp(_controlPoints[0], _controlPoints[1], T);
        }

        var tempArray = ArrayPool<Vector2>.Shared.Rent(_controlPoints.Length);

        _controlPoints.CopyTo(tempArray);

        try {
            return PrivateEvaluate(tempArray.AsSpan(0, _controlPoints.Length), T);
        }
        finally {
            ArrayPool<Vector2>.Shared.Return(tempArray, clearArray: false);
        }
    }
    public List<Vector2> GetPoints(int amount) {
        if(amount < 2) {
            amount = 2;
        }

        float perStep = 1f / (amount - 1);

        var points = new List<Vector2>(amount);

        for(int i = 0; i < amount; i++) {
            points.Add(Evaluate(perStep * i));
        }

        return points;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2 PrivateEvaluate(Span<Vector2> currentWorkingPoints, float T) {
        if(currentWorkingPoints.Length == 2) {
            return Vector2.Lerp(currentWorkingPoints[0], currentWorkingPoints[1], T);
        }
        for(int k = 0; k < currentWorkingPoints.Length - 1; k++) {
            currentWorkingPoints[k] = Vector2.Lerp(currentWorkingPoints[k], currentWorkingPoints[k + 1], T);
        }
        return PrivateEvaluate(currentWorkingPoints.Slice(0, currentWorkingPoints.Length - 1), T);
    }
}