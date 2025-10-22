using System;
using System.Collections.Generic;

namespace AllBeginningsMod.Utilities;

public static class EnumExtensions {
    public static IEnumerable<T> GetFlags<T>(this T enumValue) where T : Enum {
        foreach(T flag in Enum.GetValues(typeof(T))) {
            if(enumValue.HasFlag(flag) && Convert.ToInt64(flag) != 0) {
                yield return flag;
            }
        }
    }
}