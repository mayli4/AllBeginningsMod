using System.Text;
using System.Threading;

namespace Generators.Utilities;

internal static class StringBuilderPool
{
    private const int MAX_POOLED_BUILDERS = 8;
    private static readonly StringBuilder[] PooledBuilders = new StringBuilder[MAX_POOLED_BUILDERS];
    private static volatile int pooledCount = 0;
    private static SpinLock requestLock = new(false);

    internal static StringBuilder Rent(int sizeHint)
    {
        StringBuilder builder = null;
        bool lockObtained = false;

        try
        {
            requestLock.Enter(ref lockObtained);
            if (lockObtained && pooledCount > 0)
            {
                builder = PooledBuilders[pooledCount--];
            }
        }
        finally
        {
            if (lockObtained)
            {
                requestLock.Exit();
            }
        }

        builder ??= new StringBuilder(sizeHint);
        return builder;
    }

    internal static void Return(StringBuilder builder)
    {
        builder.Clear();
        // allow 32kb buffers maximum, otherwise let the GC claim them
        if (builder.Length < 1024 * 32 && pooledCount != MAX_POOLED_BUILDERS)
        {
            bool lockTaken = false;
            try
            {
                requestLock.Enter(ref lockTaken);
                if (lockTaken)
                {
                    if (pooledCount != MAX_POOLED_BUILDERS)
                    {
                        PooledBuilders[pooledCount++] = builder;
                    }
                }
            }
            finally
            {
                if (lockTaken)
                {
                    requestLock.Exit();
                }
            }
        }
    }
}