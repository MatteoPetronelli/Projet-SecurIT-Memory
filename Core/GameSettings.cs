namespace SecurIT_Memory.Core
{
    public static class GameSettings
    {
        public static int GridSize { get; set; } = 4;
        public static string Difficulty { get; set; } = "Normal";
        public static string ThemeName { get; set; } = "Crypto";

        public static int RevealDelayMilliseconds
        {
            get
            {
                return Difficulty switch
                {
                    "Facile" => 2000,
                    "Difficile" => 1000,
                    _ => 1500,
                };
            }
        }
    }
}
