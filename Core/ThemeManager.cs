using System.Collections.Generic;
using System.IO;

namespace SecurIT_Memory.Core
{
    public static class ThemeManager
    {
        public static List<string> GetImagePaths(string themeName)
        {
            List<string> paths = new List<string>();
            string basePath = Path.Combine("Resources", "Images", themeName);
            
            if (Directory.Exists(basePath))
            {
                paths.AddRange(Directory.GetFiles(basePath, "*.png"));
                paths.AddRange(Directory.GetFiles(basePath, "*.jpg"));
            }
            
            return paths;
        }
    }
}