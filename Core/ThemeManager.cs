using System;
using System.Collections.Generic;
using System.IO;

namespace SecurIT_Memory.Core
{
    public static class ThemeManager
    {
        public static List<string> GetImagePaths(string themeName)
        {
            List<string> paths = new List<string>();
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string basePath = Path.Combine(appDirectory, "Ressources", "Images", themeName);

            if (!Directory.Exists(basePath))
            {
                basePath = Path.GetFullPath(Path.Combine(appDirectory, "..", "..", "..", "Ressources", "Images", themeName));
            }

            if (Directory.Exists(basePath))
            {
                paths.AddRange(Directory.GetFiles(basePath, "*.png"));
                paths.AddRange(Directory.GetFiles(basePath, "*.jpg"));
            }

            return paths;
        }
    }
}