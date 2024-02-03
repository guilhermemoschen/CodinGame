using System;
using System.Linq;

namespace TestTaker
{
    public static class CustomCode
    {
        public static int VersionCompare(string version1, string version2)
        {
            var version1Parts = version1.Split('.').Select(p => int.Parse(p)).ToList();
            var version2Parts = version2.Split('.').Select(p => int.Parse(p)).ToList();

            for (var i = 0; i < version1Parts.Count; i++)
            {
                var currentPart1 = version1Parts[i];

                if (i > version2Parts.Count - 1)
                {
                    return version1Parts.Skip(i).Sum() == 0
                        ? 0
                        : 1;
                }

                var currentPart2 = version2Parts[i];

                if (currentPart1 < currentPart2)
                {
                    return -1;
                }
                else if (currentPart1 > currentPart2)
                {
                    return 1;
                }

                if (i == version1Parts.Count - 1)
                {
                    if (version1Parts.Count == version2Parts.Count)
                    {
                        return 0;
                    }

                    return version2Parts.Skip(i + 1).Sum() == 0
                        ? 0
                        : -1;
                }
            }

            return 0;
        }
    }
}