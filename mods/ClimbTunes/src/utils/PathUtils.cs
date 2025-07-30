using System.IO;
using BepInEx;

namespace ClimbTunes.Utils
{
    public static class PathUtils
    {
        public static string GetAssetBundlePath()
        {
            string pluginDirectory = Path.GetDirectoryName(typeof(ClimbTunes).Assembly.Location);
            return Path.Combine(pluginDirectory, "assets", "climbtunes");
        }
    }
}
