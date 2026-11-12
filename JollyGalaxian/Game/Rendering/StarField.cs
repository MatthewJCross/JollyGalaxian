using System;
using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;
using Jolly2DEngine.Rendering.DirectX;

namespace JollyGalaxian.Game.Rendering
{
    public sealed class Starfield
    {
        private sealed class Star
        {
            public float X;
            public float Y;
            public float Speed;
            public bool Bright;
            public int ColourIndex;
            public JollySprite Sprite = null!;
        }

        private static readonly byte[][] StarColours =
        [
            [255, 0, 255, 255],   // Magenta (BGRA)
            [0, 128, 0, 255],     // Green
            [0, 255, 255, 255],   // Yellow
            [255, 255, 255, 255]  // White
        ];

        private readonly Random _random = new();
        private readonly List<Star> _stars = [];
        private readonly JollyTexture[] _textures = new JollyTexture[4];

        private readonly DirectXJollyRenderer _renderer;

        private readonly int _width;
        private readonly int _height;

        public IReadOnlyList<JollySprite> Sprites => [.. _stars.Select(star => star.Sprite)];

        public Starfield(DirectXJollyRenderer renderer, int width, int height, int starCount = 100)
        {
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));

            _width = width;
            _height = height;

            CreateTextures();

            for (int i = 0; i < starCount; i++)
            {
                Star star = new()
                {
                    X = _random.Next(width),
                    Y = _random.Next(height),
                    Speed = 0.25f + (float)_random.NextDouble() * 0.125f,
                    Bright = _random.Next(4) == 0,
                    ColourIndex = _random.Next(StarColours.Length)
                };

                star.Sprite = CreateStarSprite(star);
                _stars.Add(star);
            }
        }

        private void CreateTextures()
        {
            for (int i = 0; i < StarColours.Length; i++)
            {
                _textures[i] = _renderer.CreateTextureFromBgraPixels(1, 1, StarColours[i]);
            }
        }

        private JollySprite CreateStarSprite(Star star)
        {
            JollySprite sprite = _renderer.CreateSprite(_textures[star.ColourIndex]);

            sprite.Size = new JollyVector2(1, 1);
            sprite.Position = new JollyVector2(star.X + 0.5f, star.Y + 0.5f);
            sprite.Visible = star.Bright;

            return sprite;
        }

        public void Update(float deltaSeconds)
        {
            foreach (Star star in _stars)
            {
                star.Y += star.Speed * deltaSeconds * 60.0f;

                if (star.Y >= _height)
                {
                    star.Y = 0;
                    star.X = _random.Next(_width);
                }

                if (_random.Next(50) == 0)
                {
                    star.Bright = !star.Bright;
                }

                star.Sprite.Position = new JollyVector2(star.X + 0.5f, star.Y + 0.5f);
                star.Sprite.Visible = star.Bright;
            }
        }
    }
}
