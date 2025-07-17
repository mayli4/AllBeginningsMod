using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace AllBeginningsMod.Utilities;

public class BezierCurve {
    public Vector2[] ControlPoints;
    public float[] arcLengths;

    public BezierCurve(params Vector2[] controls) {
        ControlPoints = controls;
    }


    public Vector2 Evaluate(float interpolant) => PrivateEvaluate(ControlPoints, MathHelper.Clamp(interpolant, 0f, 1f));

    public List<Vector2> GetPoints(int totalPoints) {
        float perStep = 1f / totalPoints;

        List<Vector2> points = new List<Vector2>();

        for (float step = 0f; step <= 1f; step += perStep)
            points.Add(Evaluate(step));

        return points;
    }

    public float ArcLengthParametrize(float step, float totalCurveLentgh) {
        float pointAtLentgh = step * totalCurveLentgh;

        float longestLenghtFound = 0;
        float longerLenghtFound = 0;


        int index = 0;

        for (int i = 0; i < arcLengths.Length; i++) {
            if (arcLengths[i] == pointAtLentgh)
                return i / (float)(arcLengths.Length - 1);

            if (arcLengths[i] > pointAtLentgh)
            {
                longerLenghtFound = arcLengths[i];
                break;
            }

            index = i;
            longestLenghtFound = arcLengths[i];
        }

        if (longerLenghtFound != 0) {
            return (index + (pointAtLentgh - longestLenghtFound) / (longerLenghtFound - longestLenghtFound)) / (float)(arcLengths.Length - 1);
        }

        return 1;
    }

    public List<Vector2> GetEvenlySpacedPoints(int totalPoints, int computationPrecision = 30, bool forceRecalculate = false) {
        if (arcLengths == null || arcLengths.Length == 0 || forceRecalculate)
        {
            arcLengths = new float[computationPrecision + 1];
            arcLengths[0] = 0;

            //Calculate the arc lentgh at a bunch of points
            Vector2 oldPosition = ControlPoints[0];
            for (int i = 1; i <= computationPrecision; i += 1)
            {
                Vector2 position = Evaluate(i / (float)computationPrecision);
                float curveLength = (position - oldPosition).Length();
                arcLengths[i] = arcLengths[i - 1] + curveLength;

                oldPosition = position;
            }
        }


        float totalCurveLentgh = arcLengths[arcLengths.Length - 1];


        List<Vector2> points = new List<Vector2>();

        for (int step = 0; step < totalPoints; step++)
            points.Add(Evaluate(ArcLengthParametrize((step / (float)(totalPoints - 1)), totalCurveLentgh)));

        return points;
    }

    private Vector2 PrivateEvaluate(Vector2[] points, float T) {
        while (points.Length > 2) {
            Vector2[] nextPoints = new Vector2[points.Length - 1];
            for (int k = 0; k < points.Length - 1; k++)
                nextPoints[k] = Vector2.Lerp(points[k], points[k + 1], T);

            points = nextPoints;
        }

        if (points.Length <= 1)
            return Vector2.Zero;

        return Vector2.Lerp(points[0], points[1], T);
    }
}