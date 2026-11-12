using Jolly2DEngine.Graphics;
using Jolly2DEngine.Input;
using JollyGalaxian.Game.Rendering;
using System.Windows.Input;

namespace JollyGalaxian.Game.Entities
{
    public sealed class Player : Entity
    {
        private const float DeathDuration = 1.0f;

        private readonly JollyInput _input;
        private readonly SpriteAsset _playerAsset;
        private readonly SpriteAsset _explosionAsset;

        private readonly JollySprite _playerSprite;
        private readonly JollySprite _explosionSprite;

        private bool _isDying;
        private float _deathTimer;
        private int _deathFrame;

        public int Lives { get; set; } = GameConstants.StartLives;

        public bool Visible { get; set; } = true;
        public bool IsAlive => !_isDying;
        public bool WantsToFire { get; private set; }

        public float Speed { get; set; } = GameConstants.PlayerSpeed;

        public Player(float x, float y, int width, int height, JollyInput input, Sprites sprites)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;

            _input = input;

            _playerAsset = sprites.Player;
            _explosionAsset = sprites.PlayerExplode;

            _playerSprite = _playerAsset.CreateSprite();
            _explosionSprite = _explosionAsset.CreateSprite();

            UpdateSpritePositions();
        }

        public void ResetForNewGame()
        {
            Lives = GameConstants.StartLives;

            _isDying = false;
            _deathTimer = 0;
            _deathFrame = 0;

            Visible = true;
            WantsToFire = false;

            X = GameConstants.ScreenW * 0.5f - Width * 0.5f;
            Y = GameConstants.ScreenH - 40;

            UpdateSpritePositions();
        }

        public void Kill()
        {
            if (_isDying || Lives <= 0)
                return;

            Lives--;

            _isDying = true;
            Visible = true;

            _deathTimer = DeathDuration;
            _deathFrame = 0;
        }

        public void Update(float deltaSeconds)
        {
            WantsToFire = false;

            if (_isDying)
            {
                UpdateDeath(deltaSeconds);
                UpdateSpritePositions();
                return;
            }

            if (_input.IsKeyDown(Key.Left))
                X -= Speed * deltaSeconds;

            if (_input.IsKeyDown(Key.Right))
                X += Speed * deltaSeconds;

            X = Math.Clamp(X, 0, GameConstants.ScreenW - Width);

            WantsToFire = _input.IsKeyDown(Key.Space);

            UpdateSpritePositions();
        }

        private void UpdateDeath(float deltaSeconds)
        {
            _deathTimer -= deltaSeconds;
            float progress = 1.0f - Math.Max(0, _deathTimer) / DeathDuration;
            _deathFrame = Math.Clamp((int)(progress * _explosionAsset.FrameCount), 0, _explosionAsset.FrameCount - 1);

            if (_deathTimer <= 0)
                Respawn();
        }

        private void Respawn()
        {
            _isDying = false;

            X = GameConstants.ScreenW * 0.5f - Width * 0.5f;
            Y = GameConstants.ScreenH - 40;

            Visible = true;
        }

        private void UpdateSpritePositions()
        {
            _playerAsset.SetPosition(_playerSprite, X, Y);
            _explosionAsset.SetPosition(_explosionSprite, X - 10, Y - 10);
        }

        public void AddToFrame(List<JollySprite> sprites)
        {
            if (!Visible)
                return;

            if (_isDying)
            {
                _explosionAsset.SetFrame(_explosionSprite, _deathFrame);
                sprites.Add(_explosionSprite);
            }
            else
            {
                sprites.Add(_playerSprite);
            }
        }
    }
}