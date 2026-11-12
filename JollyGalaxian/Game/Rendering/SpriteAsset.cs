using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;
using Jolly2DEngine.Rendering.DirectX;
using System.IO;

namespace JollyGalaxian.Game.Rendering
{
    public sealed class SpriteAsset
    {
        private readonly DirectXJollyRenderer _renderer;

        public JollyTexture Texture { get; }
        public int Width { get; }
        public int Height { get; }
        public int FrameWidth { get; }
        public int FrameHeight { get; }
        public int FrameCount => FrameWidth > 0 ? Width / FrameWidth : 1;

        public SpriteAsset(DirectXJollyRenderer renderer, string assetPath, int width, int height, int frameWidth = 0, int frameHeight = 0)
        {
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            Width = width;
            Height = height;
            FrameWidth = frameWidth > 0 ? frameWidth : width;
            FrameHeight = frameHeight > 0 ? frameHeight : height;

            string fullPath = Path.Combine(AppContext.BaseDirectory, assetPath);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Sprite asset not found: {fullPath}", fullPath);

            Texture = _renderer.LoadTexture(fullPath);
        }

        public JollySprite CreateSprite(int frame = 0)
        {
            JollySprite sprite = _renderer.CreateSprite(Texture);
            sprite.Size = new JollyVector2(FrameWidth, FrameHeight);
            SetFrame(sprite, frame);
            return sprite;
        }

        public void SetFrame(JollySprite sprite, int frame)
        {
            ArgumentNullException.ThrowIfNull(sprite);

            frame = ((frame % FrameCount) + FrameCount) % FrameCount;
            sprite.SourceX = (float)(frame * FrameWidth) / Texture.Width;
            sprite.SourceY = 0.0f;
            sprite.SourceWidth = (float)FrameWidth / Texture.Width;
            sprite.SourceHeight = (float)FrameHeight / Texture.Height;
        }

        public void SetPosition(JollySprite sprite, float x, float y)
        {
            sprite.Position = new JollyVector2(x + FrameWidth * 0.5f, y + FrameHeight * 0.5f);
        }
    }
}
