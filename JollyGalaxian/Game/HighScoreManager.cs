using System.IO;

namespace JollyGalaxian.Game.Gameplay
{
    public static class HighScoreManager
    {
        private static readonly string SavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JollyGalaxian", "Highscore.dat");

        public static int Load()
        {
            try
            {
                if (!File.Exists(SavePath))
                    return 0;

                return int.TryParse(File.ReadAllText(SavePath), out int score) ? Math.Max(0, score) : 0;
            }
            catch
            {
                return 0;
            }
        }

        public static void Save(int score)
        {
            try
            {
                string? directory = Path.GetDirectoryName(SavePath);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(SavePath, Math.Max(0, score).ToString());
            }
            catch
            {
                // High-score persistence must not interrupt gameplay.
            }
        }
    }
}