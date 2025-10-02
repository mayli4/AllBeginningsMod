using System;
using Terraria.WorldBuilding;

namespace AllBeginningsMod.Common.World;

public class Splatter(int radius, int samples, int distance) : GenShape
{
    private readonly int _radius = radius;
    private readonly int _samples = samples;
    private readonly int _distance = distance;

    public override bool Perform(Point origin, GenAction action)
    {
        for (int a = 0; a < _samples; a++)
        {
            float strength = _random.NextFloat();
            int finalRadius = (int)(_radius * strength);

            if (finalRadius < 2)
                continue;

            var location = origin + (_random.NextVector2Unit() * (_distance * (1f - strength))).ToPoint();

            if (!GenCircle(location, new(finalRadius, finalRadius), action))
                return false;
        }

        return true;
    }

    private bool GenCircle(Point origin, Point radius, GenAction action)
    {
        int xRadius = radius.X;
        int yRadius = radius.Y;
        int num = (xRadius + 1) * (xRadius + 1);

        for (int i = origin.Y - yRadius; i <= origin.Y + yRadius; i++)
        {
            double num2 = xRadius / (double)yRadius * (i - origin.Y);
            int num3 = Math.Min(xRadius, (int)Math.Sqrt(num - num2 * num2));

            for (int j = origin.X - num3; j <= origin.X + num3; j++)
            {
                if (!UnitApply(action, origin, j, i) && _quitOnFail)
                    return false;
            }
        }

        return true;
    }
}