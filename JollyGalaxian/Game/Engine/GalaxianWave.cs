using JollyGalaxian.Game.Entities;

namespace JollyGalaxian.Game.Engine
{
    public class GalaxianWave
    {
        public int SpacingX = 16;
        public int SpacingY = 12;

        public int FormationX;
        public int FormationY;

        public List<Enemy> Enemies = new();
        public List<Bullet> EnemyBullets = new();

        public int Direction = 1;
        public int SpeedFP = 1 << 16; // fixed-point speed

        public int StepCounter;
        public int StepDelay = 6; // 60Hz ticks (slow movement)

        public void ClearBullets()
        {
            EnemyBullets.Clear();
        }
    }
}


