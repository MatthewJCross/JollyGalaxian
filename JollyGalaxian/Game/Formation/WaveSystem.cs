using Jolly2DEngine.Audio;
using Jolly2DEngine.Graphics;
using JollyGalaxian.Game.Entities;

namespace JollyGalaxian.Game.Formation
{
    public sealed class WaveSystem
    {
        public int AnimationFrame { get; private set; }

        private float _animationTimer;
        private float _enemyFireTimer;
        private float _enemyFireInterval = 2.8f;
        private int _maxEnemyBullets = 1;

        public void Reset()
        {
            AnimationFrame = 0;
            _animationTimer = 0;
            _enemyFireTimer = 0;
        }

        public void Update(GalaxianWave wave, float deltaSeconds, int currentWave, JollyAudioManager audio)
        {
            _enemyFireInterval = MathF.Max(0.85f, 2.8f - (currentWave - 1) * 0.19f);
            _maxEnemyBullets = Math.Min(3, 1 + (currentWave - 1) / 3);
            _animationTimer += deltaSeconds;
            if (_animationTimer >= 0.20f)
            {
                _animationTimer %= 0.20f;
                AnimationFrame ^= 1;
            }

            if (!wave.FormationReady)
            {
                foreach (Enemy enemy in wave.Enemies)
                    enemy.UpdateFormationEntry(deltaSeconds);

                wave.FormationReady = wave.Enemies.All(enemy => !enemy.Alive || !enemy.IsEnteringFormation);
                return;
            }

            int alive = wave.Enemies.Count(enemy => enemy.Alive);
            // Background tempo is controlled by GalaxianGame from wave and survivor counts.

            wave.StepDelay = Math.Max(3, 11 - (currentWave - 1) / 2 - Math.Max(0, 46 - alive) / 12);
            wave.StepCounter++;

            if (wave.StepCounter < wave.StepDelay)
                return;

            wave.StepCounter = 0;

            List<Enemy> formation = wave.Enemies.Where(enemy => enemy.Alive && !enemy.IsDiving && !enemy.IsReturning && !enemy.IsEnteringFormation).ToList();

            if (formation.Count == 0)
                return;

            int direction = wave.Direction;
            bool hitEdge = formation.Any(enemy => enemy.X + direction < 0 || enemy.X + enemy.Width + direction > GameConstants.ScreenW);

            if (hitEdge)
            {
                wave.Direction *= -1;

            }

            direction = wave.Direction;

            foreach (Enemy enemy in wave.Enemies)
            {
                if (!enemy.Alive)
                    continue;

                enemy.HomeX += direction;

                if (!enemy.IsDiving && !enemy.IsReturning && !enemy.IsEnteringFormation)
                {
                    enemy.X += direction;
                    enemy.Frame = enemy.Type == (int)WaveFactory.EnemyType.Yellow ? 0 : AnimationFrame;

                }
            }
        }

        public bool LaunchDive(GalaxianWave wave, float playerX, Random random, int currentWave)
        {
            List<Enemy> available = wave.Enemies.Where(enemy => enemy.Alive && !enemy.IsDiving && !enemy.IsReturning && !enemy.IsEnteringFormation && enemy.DiveLeader == null).ToList();
            if (available.Count == 0)
                return false;

            List<Enemy> leaders = available.Where(enemy => enemy.Type == (int)WaveFactory.EnemyType.Yellow).ToList();
            Enemy leader = leaders.Count > 0 ? leaders[random.Next(leaders.Count)] : available[random.Next(available.Count)];
            int desiredEscorts = currentWave <= 2 ? 0 :
                leader.Type == (int)WaveFactory.EnemyType.Yellow ? random.Next(1, currentWave >= 6 ? 3 : 2) : random.Next(0, 2);
            List<Enemy> escorts = available.Where(enemy => enemy != leader && enemy.Type == (int)WaveFactory.EnemyType.Red).OrderBy(enemy => MathF.Abs(enemy.HomeX - leader.HomeX)).Take(desiredEscorts).ToList();

            leader.IsDiving = true;
            leader.IsReturning = false;
            leader.IsPackLeader = escorts.Count > 0;
            leader.HasEscorts = escorts.Count > 0;
            leader.DiveTime = 0;
            leader.DiveStartX = leader.X;
            leader.DiveStartY = leader.Y;
            leader.PlayerX = playerX;
            leader.TargetX = playerX;

            for (int i = 0; i < escorts.Count; i++)
            {
                Enemy escort = escorts[i];
                escort.DiveLeader = leader;
                escort.IsPackEscort = true;
                escort.IsDiving = true;
                escort.IsReturning = false;
                escort.DiveOffsetX = (i == 0 ? -1 : 1) * 19.0f;
                escort.DiveOffsetY = -12.0f;
            }

            return true;
        }

        public void UpdateDives(GalaxianWave wave, float deltaSeconds, float playerX)
        {
            foreach (Enemy enemy in wave.Enemies.Where(enemy => enemy.Alive && enemy.DiveLeader == null))
            {
                enemy.PlayerX = playerX;
                enemy.UpdateDiving(deltaSeconds);
            }

            foreach (Enemy enemy in wave.Enemies.Where(enemy => enemy.Alive && enemy.DiveLeader != null))
                enemy.UpdateDiving(deltaSeconds);
        }

        public void UpdateEnemyFiring(float deltaSeconds, Player player, GalaxianWave wave, Random random, Func<JollySprite> bulletSpriteFactory)
        {
            _enemyFireTimer += deltaSeconds;
            if (_enemyFireTimer < _enemyFireInterval)
                return;

            _enemyFireTimer = 0;

            if (wave.EnemyBullets.Count(bullet => bullet.Active) >= _maxEnemyBullets)
                return;

            List<Enemy> shooters = wave.Enemies.Where(enemy => enemy.Alive && !enemy.IsEnteringFormation).ToList();

            if (shooters.Count == 0)
                return;

            Enemy shooter = shooters[random.Next(shooters.Count)];
            FireEnemyBullet(shooter, player, wave, bulletSpriteFactory);
        }

        private static void FireEnemyBullet(Enemy enemy, Player player, GalaxianWave wave, Func<JollySprite> bulletSpriteFactory)
        {
            float dx = (player.X + player.Width * 0.5f) - (enemy.X + enemy.Width * 0.5f);
            float dy = GameConstants.ScreenH * 0.8f;
            float length = MathF.Sqrt(dx * dx + dy * dy);

            if (length <= float.Epsilon)
                return;

            const float speed = 150.0f;
            dx /= length;
            dy /= length;

            wave.EnemyBullets.Add(new Bullet(bulletSpriteFactory())
            {
                X = enemy.X + enemy.Width * 0.5f,
                Y = enemy.Y + enemy.Height,
                VX = dx * speed,
                VY = dy * speed,
                Active = true
            });
        }

        public void UpdateEnemyBullets(float deltaSeconds, GalaxianWave wave)
        {
            foreach (Bullet bullet in wave.EnemyBullets)
            {
                if (!bullet.Active)
                    continue;

                bullet.X += bullet.VX * deltaSeconds;
                bullet.Y += bullet.VY * deltaSeconds;

                if (bullet.Y > GameConstants.ScreenH + 8 || bullet.X < -8 || bullet.X > GameConstants.ScreenW + 8)
                {
                    bullet.Active = false;
                }
            }

            wave.EnemyBullets.RemoveAll(bullet => !bullet.Active);
        }

        public bool CheckEnemyBulletPlayerCollision(Player player, GalaxianWave wave)
        {
            foreach (Bullet bullet in wave.EnemyBullets)
            {
                if (!bullet.Active)
                    continue;

                if (bullet.X >= player.X && bullet.X <= player.X + player.Width && bullet.Y >= player.Y && bullet.Y <= player.Y + player.Height)
                {
                    bullet.Active = false;
                    return true;
                }
            }

            return false;
        }
    }
}