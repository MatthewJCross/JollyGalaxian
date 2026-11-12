using JollyGalaxian.Game.Entities;

namespace JollyGalaxian.Game.Formation
{
    public sealed class GalaxianWave
    {
        public int SpacingX = 20;
        public int SpacingY = 18;
        public List<Enemy> Enemies { get; } = [];
        public List<Bullet> EnemyBullets { get; } = [];

        public int Direction = 1;
        public bool FormationReady;
        public int StepCounter;
        public int StepDelay = 6;

        public void ClearBullets() => EnemyBullets.Clear();
    }
}