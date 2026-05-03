using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace SecurIT_Memory.Core
{
    public static class ThemeManager
    {
        public static List<string> GetImagePaths(string themeName)
        {
            List<string> paths = new List<string>();
            string themeFolder = GetThemeFolder(themeName);
            System.Diagnostics.Debug.WriteLine($"[ThemeManager] GetImagePaths('{themeName}') -> themeFolder='{themeFolder}'");

            if (Directory.Exists(themeFolder))
            {
                paths.AddRange(Directory.GetFiles(themeFolder, "*.png"));
                paths.AddRange(Directory.GetFiles(themeFolder, "*.jpg"));
                paths.AddRange(Directory.GetFiles(themeFolder, "*.jpeg"));
                paths.Sort(StringComparer.OrdinalIgnoreCase);
                System.Diagnostics.Debug.WriteLine($"[ThemeManager] Found {paths.Count} images");
                File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.log"), $"{DateTime.Now:HH:mm:ss} [ThemeManager] Found {paths.Count} images\n");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ThemeManager] Theme folder does not exist: {themeFolder}");
                File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.log"), $"{DateTime.Now:HH:mm:ss} [ThemeManager] Theme folder not found: {themeFolder}\n");
            }

            return paths;
        }

        public static string GetImageRootPath()
        {
            return GetImageRootFolder();
        }

        public static List<string> GetAvailableThemes()
        {
            string imageRoot = GetImageRootFolder();
            var themes = new List<string>();

            if (!Directory.Exists(imageRoot))
            {
                return themes;
            }

            foreach (string folder in Directory.GetDirectories(imageRoot))
            {
                themes.Add(Path.GetFileName(folder));
            }

            return themes;
        }

        private static string GetImageRootFolder()
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string currentDirectory = Environment.CurrentDirectory;

            string imageRoot = FindImageRootFromBase(appDirectory);
            if (!string.IsNullOrEmpty(imageRoot))
            {
                return imageRoot;
            }

            imageRoot = FindImageRootFromBase(currentDirectory);
            if (!string.IsNullOrEmpty(imageRoot))
            {
                return imageRoot;
            }

            return Path.Combine(appDirectory, "Ressources", "Images");
        }

        private static string FindImageRootFromBase(string startDirectory)
        {
            var directory = new DirectoryInfo(startDirectory);
            for (int depth = 0; depth < 6 && directory != null; depth++)
            {
                foreach (var resourceFolderName in new[] { "Ressources", "Resources" })
                {
                    string candidate = Path.Combine(directory.FullName, resourceFolderName, "Images");
                    if (Directory.Exists(candidate))
                    {
                        return candidate;
                    }
                }

                directory = directory.Parent;
            }

            return null;
        }

        private static string GetThemeFolder(string themeName)
        {
            return Path.Combine(GetImageRootFolder(), themeName);
        }
    }
}