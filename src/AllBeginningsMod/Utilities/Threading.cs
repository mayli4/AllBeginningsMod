using System;
using System.Threading;
using Terraria;

namespace AllBeginningsMod.Utilities;

internal static class Threading {
    public static bool IsMainThread => Program.IsMainThread;
    
    internal static void RunOnMainThread(Action action) {
        if (IsMainThread) {
            action();
        } else {
            Main.RunOnMainThread(action).Wait();
        }
    }
}