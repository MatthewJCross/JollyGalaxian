using JollyGalaxian.Game.Rendering;
using JollyGalaxian.Game.Entities;

namespace JollyGalaxian.Game.Formation
{
    public static class WaveFactory
    {
        public enum EnemyType
        {
            Yellow,
            Blue,
            Purple,
            Red
        }

        private static readonly int[] RowCounts = [2, 6, 8, 10, 10, 10];

        private static EnemyType GetTypeForRow(int row) => row switch
        {
            0 => EnemyType.Yellow,
            1 => EnemyType.Red,
            2 => EnemyType.Purple,
            _ => EnemyType.Blue
        };

        private static int GetScore(EnemyType type) => type switch
        {
            EnemyType.Red => 50,
            EnemyType.Purple => 40,
            EnemyType.Blue => 30,
            _ => 60
        };

        private static SpriteAsset GetSprite(Sprites sprites, EnemyType type) => type switch
        {
            EnemyType.Red => sprites.AlienRed,
            EnemyType.Purple => sprites.AlienPurple,
            EnemyType.Blue => sprites.AlienBlue,
            _ => sprites.AlienYellow
        };

        public static GalaxianWave Create(int waveNumber, Sprites sprites, Random random)
        {
            GalaxianWave wave = new();
            int rowIndex = 0;
            int startY = 22;

            foreach (int count in RowCounts)
            {
                EnemyType type = GetTypeForRow(rowIndex);
                SpriteAsset asset = GetSprite(sprites, type);
                int totalWidth = (count - 1) * wave.SpacingX;
                int startX = ((int)GameConstants.ScreenW - totalWidth) / 2;

                for (int column = 0; column < count; column++)
                {
                    int homeX = startX + column * wave.SpacingX;
                    if (rowIndex == 0)
                        homeX += column == 0 ? -wave.SpacingX : wave.SpacingX;

                    int homeY = startY + rowIndex * wave.SpacingY;
                    Enemy enemy = new(asset)
                    {
                        X = homeX,
                        Y = homeY,
                        HomeX = homeX,
                        HomeY = homeY,
                        Width = asset.FrameWidth,
                        Height = asset.FrameHeight,
                        Type = (int)type,
                        Score = GetScore(type),
                        Row = rowIndex,
                        Column = column,
                        HomingStrength = MathF.Min(0.006f + (waveNumber - 1) * 0.003f, 0.055f),
                        IsEnteringFormation = true,
                        EntryStartX = random.Next(2) == 0 ? -24 - random.Next(30) : GameConstants.ScreenW + random.Next(30),
                        EntryStartY = -20 - random.Next(35),
                        EntryDelay = (rowIndex * 0.12f) + (column * 0.045f),
                        EntryElapsed = 0
                    };

                    enemy.X = enemy.EntryStartX;
                    enemy.Y = enemy.EntryStartY;
                    wave.Enemies.Add(enemy);
                }

                rowIndex++;
            }

            return wave;
        }
    }
}