using Jolly2DEngine.Assets;
using Jolly2DEngine.Audio;
using Jolly2DEngine.Core;
using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;
using Jolly2DEngine.Rendering.DirectX;
using Jolly2DEngine.UI;
using JollyGalaxian.Game.Entities;
using JollyGalaxian.Game.Formation;
using JollyGalaxian.Game.Gameplay;
using JollyGalaxian.Game.Rendering;
using System.Numerics;
using System.Windows.Input;

namespace JollyGalaxian.Game
{
    internal class GalaxianGame : JollyGame
    {
        private enum GalaxianGameState
        {
            Intro,
            Playing,
            GameOver
        }
        private GalaxianGameState _gameState = GalaxianGameState.Intro;

        private readonly DirectXControl _gameView;
        private DirectXJollyRenderer? _renderer;
        private JollyAudioManager? _audio;
        private JollyTexture? _enemyBulletTexture;
        private float _gameOverTimer;
        private const float GameOverHoldSeconds = 3f;
        private float _lastMusicSpeed = 1f;
        private float _musicSpeedUpdateElapsed;
        private JollyAssetManager? _assets;

        private JollyUIManager? _uiManager;
        private JollyUIRenderer? _uiRenderer;

        private Sprites? _sprites;
        private Starfield? _starfield;
        private Player? _player;
        private PlayerBullet? _playerBullet;
        private readonly List<JollyLineSnapshot> _frameLines = [];

        private JollySprite[] _lifeSprites = [];
        private readonly List<JollySprite> _waveFlagSprites = [];
        private int _displayedFlagWave = -1;

        private readonly Random _random = new();
        private readonly WaveSystem _waveSystem = new();

        private JollyUIText? _titleText;
        private JollyUIText? _messageText;
        private JollyUIText? _scoreText;
        private JollyUIText? _scoreValueText;
        private JollyUIText? _hiScoreText;
        private JollyUIText? _hiScoreValueText;

        private bool _spaceWasDown;
        private byte _displayText = 0;

        private int _nextExtraLifeScore = GameConstants.ExtraLifeScore;
        
        private double _flashTimer;

        public int CurrentWave { get; private set; } = 1;
        public GalaxianWave Wave { get; private set; } = new();
        public int Score { get; private set; }
        public int HighScore { get; private set; }
        private float _diveTimer;


        public GalaxianGame(DirectXControl gameView)
        {
            ArgumentNullException.ThrowIfNull(gameView);

            _gameView = gameView;

            GameLoopFPS = 240.0;
            RenderFPS = 120.0;
        }

        protected override void OnInitialize()
        {
            _renderer = new DirectXJollyRenderer(_gameView);
            _gameView.RenderSizeChanged += OnRenderSizeChanged;
            _audio = new JollyAudioManager();
            _assets = new JollyAssetManager(_renderer, _audio);

            _renderer.Camera.SetBounds(0.0f, 0.0f, GameConstants.ScreenW, GameConstants.ScreenH);
            _renderer.Camera.Position = new Vector2(GameConstants.ScreenW * 0.5f, GameConstants.ScreenH * 0.5f);
            _renderer.Camera.Rotation = 0.0f;
            UpdateCameraScale();


            
            _uiManager = new JollyUIManager();
            _uiRenderer = new JollyUIRenderer(_uiManager, texture => _renderer.CreateSprite(texture), (r, g, b, a) => _renderer.CreateSolidTexture(r, g, b, a), (text, fontFamily, fontSize, color, bold, italic, ultrabold) => _renderer.CreateTextTexture(text, fontFamily, fontSize, color, bold, italic));

            _sprites = new Sprites(_renderer);
            _starfield = new Starfield(_renderer, (int)GameConstants.ScreenW, (int)GameConstants.ScreenH, 100);
            _player = new Player(GameConstants.ScreenW * 0.5f - 6.5f, GameConstants.ScreenH - 40, 13, 18, Input, _sprites);

            JollySprite bulletSprite = _renderer.CreateSprite(_renderer.CreateSolidTexture(255, 255, 255, 255));
            bulletSprite.Size = new JollyVector2(1, 4);
            _enemyBulletTexture = _renderer.CreateSolidTexture(255, 255, 255, 255);
            _playerBullet = new PlayerBullet(bulletSprite);



            CreateUI();
            CreateLifeSprites();
            LoadSounds();
            HighScore = HighScoreManager.Load();
        }

        private void CreateUI()
        {
            ArgumentNullException.ThrowIfNull(_renderer);
            ArgumentNullException.ThrowIfNull(_uiManager);

            float worldHeight = (float)_gameView.ActualHeight;
            float top = _renderer.Camera.Position.Y - worldHeight * 0.5f;

            _titleText = new JollyUIText
            {
                Text = "Galaxian",
                FontFamily = "Consolas",
                FontSize = 36.0f,
                Color = JollyColor.Yellow,
                Bold = true,
                Layer = 300
            };
            _titleText.Position = new Vector2((GameConstants.ScreenW - DirectXJollyRenderer.MeasureTextWidth(_titleText!.Text, _titleText.FontFamily, _titleText.FontSize, _titleText.Bold, _titleText.UltraBold)) * 0.5f, top + worldHeight * 0.5f - 80.0f);

            _messageText = new JollyUIText
            {
                Text = "PRESS SPACE TO START",
                FontFamily = "Consolas",
                FontSize = 16.0f,
                Color = JollyColor.White,
                Bold = true,
                Layer = 300
            };
            CentreMessage();

            _scoreText = new JollyUIText
            {
                Text = "SCORE",
                Position = new Vector2(22.0f, 1.0f),
                FontFamily = "Consolas",
                FontSize = 8.0f,
                Color = JollyColor.White,
                Bold = true,
                Visible = false,
                Layer = 300
            };

            _scoreValueText = new JollyUIText
            {
                Text = "00000",
                Position = new Vector2( 20.0f, 7.0f),
                FontFamily = "Consolas",
                FontSize = 10.0f,
                Color = JollyColor.Red,
                Bold = true,
                Visible = false,
                Layer = 300
            };

            _hiScoreText = new JollyUIText
            {
                Text = "HI-SCORE",
                Position = new Vector2(122.0f, 1.0f),
                FontFamily = "Consolas",
                FontSize = 8.0f,
                Color = JollyColor.White,
                Bold = true,
                Visible = false,
                Layer = 300
            };

            _hiScoreValueText = new JollyUIText
            {
                Text = "00000",
                Position = new Vector2(120.0f, 7.0f),
                FontFamily = "Consolas",
                FontSize = 10.0f,
                Color = JollyColor.Red,
                Bold = true,
                Visible = false,
                Layer = 300
            };

            _uiManager.Add(_titleText);
            _uiManager.Add(_messageText);
            _uiManager.Add(_scoreText);
            _uiManager.Add(_scoreValueText);
            _uiManager.Add(_hiScoreText);
            _uiManager.Add(_hiScoreValueText);
        }

        private void CentreMessage()
        {
            if (_messageText is null) return;
            float width = DirectXJollyRenderer.MeasureTextWidth(_messageText.Text, _messageText.FontFamily, _messageText.FontSize, _messageText.Bold, _messageText.UltraBold);
            _messageText.Position = new Vector2((GameConstants.ScreenW - width) * 0.5f, (GameConstants.ScreenH - _messageText.FontSize) * 0.5f);
        }

        private void SetMessage(string message)
        {
            if (_messageText is null) 
                return;

            _messageText.Text = message;
            CentreMessage();
        }

        private void CreateLifeSprites()
        {
            const int maximumLives = 5;

            _lifeSprites = new JollySprite[maximumLives];

            for (int i = 0; i < maximumLives; i++)
            {
                _lifeSprites[i] = _renderer!.CreateSprite(_sprites!.PlayerLives.Texture);
                _lifeSprites[i].Visible = i < 3;
                _lifeSprites[i].Position = new JollyVector2(10 + i * 20, GameConstants.ScreenH - 11);
            }
        }

        private void UpdateWaveFlags()
        {
            if (_displayedFlagWave == CurrentWave)
                return;

            _displayedFlagWave = CurrentWave;
            _waveFlagSprites.Clear();

            // A large flag represents ten stages; a small flag represents one.
            int largeCount = CurrentWave / 10;
            int smallCount = CurrentWave % 10;

            // Keep the indicators within the right-hand side of the 252-pixel playfield.
            // For very high waves, show the most recent ten large flags.
            largeCount = Math.Min(largeCount, 10);
            const float gap = 2f;
            const float right = GameConstants.ScreenW - 8f;
            const float bottom = GameConstants.ScreenH - 3f;
            float x = right;

            for (int i = 0; i < smallCount; i++)
            {
                SpriteAsset asset = _sprites!.WaveFlag;
                x -= asset.FrameWidth;
                JollySprite flag = asset.CreateSprite();
                asset.SetPosition(flag, x, bottom - asset.FrameHeight);
                _waveFlagSprites.Add(flag);
                x -= gap;
            }

            for (int i = 0; i < largeCount; i++)
            {
                SpriteAsset asset = _sprites!.WaveFlag10;
                x -= asset.FrameWidth;
                JollySprite flag = asset.CreateSprite();
                asset.SetPosition(flag, x, bottom - asset.FrameHeight);
                _waveFlagSprites.Add(flag);
                x -= gap;
            }
        }

        private void LoadSounds()
        {
            _assets!.LoadSound("Background.wav");
            _assets!.LoadSound("Dive.wav");
            _assets!.LoadSound("ExtraLife.wav");
            _assets!.LoadSound("FighterLoss.wav");
            _assets!.LoadSound("Fire.wav");
            _assets!.LoadSound("HitBoss.wav");
            _assets!.LoadSound("HitEnemy.wav");
            _assets!.LoadSound("StartGame.wav");
        }

        private bool IsSpacePressed()
        {
            bool spaceDown = Input.IsKeyDown(Key.Space);
            bool pressed = spaceDown && !_spaceWasDown;

            _spaceWasDown = spaceDown;

            return pressed;
        }

        protected override void OnUpdate(JollyGameTime gameTime)
        {
            float deltaSeconds = (float)gameTime.DeltaSeconds;
            List<JollySprite> sprites = [];
            _frameLines.Clear();

            bool spacePressed = IsSpacePressed();

            _uiRenderer?.Update();

            if (_gameState == GalaxianGameState.Intro)
            {
                _flashTimer += gameTime.DeltaMilliseconds;
                if (spacePressed)
                {
                    StartGame();
                }

                if (_flashTimer >= 500.0)
                {
                    _flashTimer -= 500.0;
                    _displayText ^= 1;
                    _messageText!.Color = _displayText == 1 ? JollyColor.White : JollyColor.Transparent;
                }
            }
            else
            {
                _starfield!.Update(deltaSeconds);
                if (_gameState == GalaxianGameState.Playing || _gameState == GalaxianGameState.GameOver)
                    UpdatePlayer(deltaSeconds);

                if (_gameState == GalaxianGameState.GameOver)
                {
                    // Let the final player explosion finish before the GAME OVER hold begins.
                    if (_player!.IsAlive)
                    {
                        _gameOverTimer += deltaSeconds;
                        if (_gameOverTimer >= GameOverHoldSeconds) 
                            ReturnToIntro();
                    }
                }
                if (_gameState == GalaxianGameState.GameOver)
                {
                    // Still submit a frame when the game ends.
                    _playerBullet!.Reset(_player!.X, _player!.Y);
                }

                if (_gameState == GalaxianGameState.Playing)
                {
                    float previousBulletY = _playerBullet!.Y;
                    UpdatePlayerBullet(deltaSeconds);
                    UpdateWave(deltaSeconds, previousBulletY);
                    UpdateBackgroundSpeed(deltaSeconds);
                }

                sprites.AddRange(_starfield.Sprites);
                if (_gameState != GalaxianGameState.Intro)
                    _player!.AddToFrame(sprites);

                _playerBullet!.AddToFrame(sprites);
                foreach (Enemy enemy in Wave.Enemies)
                    enemy.AddToFrame(sprites);

                foreach (Bullet bullet in Wave.EnemyBullets)
                    bullet.AddToFrame(sprites);

                for (int i = 0; i < _lifeSprites.Length; i++)
                {
                    _lifeSprites[i].Visible = _gameState != GalaxianGameState.Intro && i < _player!.Lives;
                    if (_lifeSprites[i].Visible)
                        sprites.Add(_lifeSprites[i]);
                }

                if (_gameState == GalaxianGameState.Playing)
                {
                    UpdateWaveFlags();
                    sprites.AddRange(_waveFlagSprites);
                    CheckExtraLife();
                }
            }

            if (_uiRenderer is not null)
            {
                sprites.AddRange(_uiRenderer.Sprites);
            }

            JollyRenderFrame frame = new(sprites, _frameLines);
            _renderer?.SubmitFrame(frame);        
        }

        private void UpdateWave(float deltaSeconds, float previousBulletY)
        {
            if (_gameState != GalaxianGameState.Playing || _audio is null || Wave.Enemies.Count == 0)
                return;

            _waveSystem.Update(Wave, deltaSeconds, CurrentWave, _audio);
            if (!Wave.FormationReady)
                return;

            float playerCentre = _player!.X + _player!.Width * 0.5f;
            // Capture positions so fast diving aliens cannot tunnel through the player.
            var previousEnemyPositions = Wave.Enemies.Where(enemy => enemy.Alive).ToDictionary(enemy => enemy, enemy => (enemy.X, enemy.Y));
            _waveSystem.UpdateDives(Wave, deltaSeconds, playerCentre);
            _diveTimer += deltaSeconds;
            float diveInterval = MathF.Max(1.15f, 4.8f - (CurrentWave - 1) * 0.30f);
            if (_diveTimer >= diveInterval)
            {
                _diveTimer = 0;
                if (_waveSystem.LaunchDive(Wave, playerCentre, _random, CurrentWave))
                    _assets?.PlaySound("Dive");
            }

            _waveSystem.UpdateEnemyFiring(deltaSeconds, _player, Wave, _random, CreateEnemyBulletSprite);
            _waveSystem.UpdateEnemyBullets(deltaSeconds, Wave);
            if (_player.IsAlive && _player.Visible)
            {
                bool bulletHit = _waveSystem.CheckEnemyBulletPlayerCollision(_player, Wave);
                bool alienHit = false;
                if (!bulletHit)
                {
                    foreach (Enemy enemy in Wave.Enemies)
                    {
                        if (!enemy.Alive || (!enemy.IsDiving && !enemy.IsReturning))
                            continue;

                        var (X, Y) = previousEnemyPositions[enemy];
                        if (!SweptEnemyHitsPlayer(enemy, X, Y, _player))
                            continue;

                        // A ramming alien is destroyed along with the player.
                        enemy.Alive = false;
                        alienHit = true;
                        break;
                    }
                }

                if (bulletHit || alienHit)
                {
                    _player.Kill();
                    _playerBullet!.Reset(_player!.X + _player!.Width * 0.5f, _player!.Y - 1);
                    _assets?.PlaySound("FighterLoss");
                }
            }

            if (_playerBullet!.Active)
            {
                foreach (Enemy enemy in Wave.Enemies)
                {
                    if (!enemy.Alive || _playerBullet!.X + 0.5f < enemy.X || _playerBullet!.X - 0.5f > enemy.X + enemy.Width ||
                        MathF.Max(_playerBullet!.Y, previousBulletY) + 4 < enemy.Y ||
                        MathF.Min(_playerBullet!.Y, previousBulletY) > enemy.Y + enemy.Height)
                        continue;

                    enemy.Alive = false;
                    _playerBullet!.Reset(_player!.X + _player!.Width * 0.5f, _player!.Y - 1);
                    Score += enemy.Score;
                    _assets?.PlaySound(enemy.Type == (int)WaveFactory.EnemyType.Yellow ? "HitBoss" : "HitEnemy");
                    _scoreValueText?.Text = Score.ToString("D5");
                    if (Score > HighScore)
                    {
                        HighScore = Score;
                        _hiScoreValueText?.Text = HighScore.ToString("D5");
                    }
                    break;
                }
            }

            if (Wave.Enemies.All(enemy => !enemy.Alive))
            {
                CurrentWave++;
                Wave = WaveFactory.Create(CurrentWave, _sprites!, _random);
                _diveTimer = 0;
            }
        }

        private static bool SweptEnemyHitsPlayer(Enemy enemy, float previousX, float previousY, Player player)
        {
            // Sweep the enemy's rectangle along its movement this tick against
            // the player's rectangle expanded by the enemy's dimensions.
            float minX = player.X - enemy.Width;
            float maxX = player.X + player.Width;
            float minY = player.Y - enemy.Height;
            float maxY = player.Y + player.Height;

            float dx = enemy.X - previousX;
            float dy = enemy.Y - previousY;
            float tMin = 0f;
            float tMax = 1f;

            static bool Clip(float start, float delta, float min, float max, ref float enter, ref float exit)
            {
                if (MathF.Abs(delta) < 0.000001f)
                    return start >= min && start <= max;
                float a = (min - start) / delta;
                float b = (max - start) / delta;
                if (a > b) (a, b) = (b, a);
                enter = MathF.Max(enter, a);
                exit = MathF.Min(exit, b);
                return enter <= exit;
            }

            return Clip(previousX, dx, minX, maxX, ref tMin, ref tMax) && Clip(previousY, dy, minY, maxY, ref tMin, ref tMax);
        }

        private void UpdatePlayer(float deltaSeconds)
        {
            _player!.Visible = _audio!.IsSoundPlaying("StartGame") != true;
            _player!.Update(deltaSeconds);

            if (_player!.Lives <= 0 && _player!.IsAlive && _gameState == GalaxianGameState.Playing)
            {
                _gameState = GalaxianGameState.GameOver;
                _gameOverTimer = 0;
                _audio.StopMusic();
                Wave.ClearBullets();
                SetMessage("GAME OVER");
                _messageText!.Visible = true;
                _messageText.Color = JollyColor.Red;
            }

            if (_gameState == GalaxianGameState.Playing && _player!.WantsToFire && _player!.Visible && _player!.IsAlive)
                _playerBullet!.Fire(_audio);
        }

        private void UpdatePlayerBullet(float deltaSeconds)
        {
            _playerBullet!.Update(deltaSeconds, _player!.X + _player!.Width * 0.5f, _player!.Y - 1);
        }

        private void StartGame()
        {
            _titleText!.Visible = false;
            _messageText!.Visible = false;
            _scoreText!.Visible = true;
            _scoreValueText!.Visible = true;
            _hiScoreText!.Visible = true;
            _hiScoreValueText!.Visible = true;

            _gameState = GalaxianGameState.Playing;
            _displayText = 0;
            _flashTimer = 0;
            _lastMusicSpeed = 1f;
            _musicSpeedUpdateElapsed = 0f;
            _audio!.SetMusicSpeed(1.0f);
            _assets!.PlayMusic("Background", loop: true);
            _assets!.PlaySound("StartGame");

            Score = 0;
            CurrentWave = 1;
            _displayedFlagWave = -1;
            _waveFlagSprites.Clear();
            _player!.ResetForNewGame();
            _scoreValueText?.Text = "00000";
            _hiScoreValueText?.Text = HighScore.ToString("D5");

            _playerBullet!.Reset(_player!.X + 6, _player!.Y - 1);

            _waveSystem.Reset();
            _gameOverTimer = 0;
            Wave = WaveFactory.Create(CurrentWave, _sprites!, _random);
            _diveTimer = 0;
            _nextExtraLifeScore = GameConstants.ExtraLifeScore;
        }

        private void CheckExtraLife()
        {
            while (Score >= _nextExtraLifeScore)
            {
                if (_player!.Lives < GameConstants.MaxLives)
                {
                    _player!.Lives++;
                    _assets!.PlaySound("ExtraLife");
                }

                _nextExtraLifeScore += GameConstants.ExtraLifeScore;
            }
        }

        private void OnRenderSizeChanged(float width, float height)
        {
            if (_renderer is null)
            {
                return;
            }

            _renderer.Camera.SetViewport(width, height);
            UpdateCameraScale();
        }

        private void UpdateCameraScale()
        {
            float viewportWidth = _renderer!.Camera.ViewportSize.X;
            float viewportHeight = _renderer.Camera.ViewportSize.Y;

            float scaleX = viewportWidth / GameConstants.ScreenW;
            float scaleY = viewportHeight / GameConstants.ScreenH;

            _renderer.Camera.Zoom = MathF.Min(scaleX, scaleY);
        }

        private JollySprite CreateEnemyBulletSprite()
        {
            JollySprite sprite = _renderer!.CreateSprite(_enemyBulletTexture!);
            sprite.Size = new JollyVector2(1, 4);
            return sprite;
        }

        private void UpdateBackgroundSpeed(float deltaSeconds)
        {
            _musicSpeedUpdateElapsed += deltaSeconds;
            if (_musicSpeedUpdateElapsed < 0.25f) 
                return;

            _musicSpeedUpdateElapsed = 0f;

            int alive = Wave.Enemies.Count(enemy => enemy.Alive);
            float remainingFraction = Math.Clamp(alive / (float)GameConstants.EnemyFormationCount, 0f, 1f);
            float targetSpeed = MathF.Min(2f, 1f + (CurrentWave - 1) * 0.06f + (1f - remainingFraction) * 0.65f);

            if (MathF.Abs(targetSpeed - _lastMusicSpeed) < 0.025f) 
                return;

             _audio!.SetMusicSpeed(targetSpeed);
            _lastMusicSpeed = targetSpeed;
        }

        private void ReturnToIntro()
        {
            _gameState = GalaxianGameState.Intro;
            _diveTimer = 0;
            _audio!.StopMusic();
            Wave.ClearBullets();
            Wave.Enemies.Clear();
            _playerBullet!.Reset(_player!.X, _player!.Y);
            _titleText!.Visible = true;
            SetMessage("PRESS SPACE TO START");
            _messageText!.Visible = true;
            _messageText.Color = JollyColor.White;
            _player!.Visible = false;
            _scoreText!.Visible = false;
            _scoreValueText!.Visible = false;
            _hiScoreText!.Visible = false;
            _hiScoreValueText!.Visible = false;
            _flashTimer = 0;
            _displayText = 0;
            HighScoreManager.Save(HighScore);
        }
    }
}
