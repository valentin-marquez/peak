using System.Text.RegularExpressions;

namespace ClimbTunes.Utils
{
    public static class SceneUtils
    {
        private static readonly Regex LEVEL_SCENE_REGEX = new Regex(@"^Level_\d+$");

        public static bool IsLevelScene(string sceneName)
        {
            return LEVEL_SCENE_REGEX.IsMatch(sceneName);
        }
    }
}
