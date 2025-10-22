using System;

namespace Generators.Utilities;

internal static class PathUtils {
    public static string GetFolder(string path) {
        int lastSlash = path.LastIndexOf('/');
        if(lastSlash == -1) {
            return string.Empty;
        }

        return path.Substring(0, lastSlash);
    }

    public static string GetFileNameWithoutExtension(string file) {
        string result = file;
        int lastDot = result.LastIndexOf('.');
        if(lastDot != -1) {
            result = result.Substring(0, lastDot);
        }

        int lastSlash = result.LastIndexOf('/');
        if(lastSlash != -1) {
            result = result.Substring(lastSlash + 1);
        }

        return result;
    }

    /// <summary>
    ///     Returns the extension including the dot '.' of the given file path.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static string GetExtension(string file) {
        int index = file.LastIndexOf('.');
        if(index == -1) {
            return string.Empty;
        }

        return file.Substring(index);
    }

    public static string RemoveExtension(string file) {
        int index = file.LastIndexOf('.');
        if(index == -1) {
            return file;
        }

        return file.Substring(0, index);
    }
}