using Jolly2DEngine.Audio;
using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;

namespace JollyGalaxian.Game.Entities
{
    public sealed class PlayerBullet(JollySprite sprite)
    {
        private readonly JollySprite _sprite = sprite;

        public bool Active { get; private set; } = false;
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Speed { get; set; } = 300.0f;

        public void Fire(JollyAudioManager audio)
        {
            if (Active)
                return;

            audio.PlaySound("Fire");
            Active = true;
        }

        public void Reset(float x, float y)
        {
            Active = false;
            X = x;
            Y = y;
        }

        public void Update(float deltaSeconds, float x, float y)
        {
            if (!Active)
            {
                X = x;
                Y = y;
                return;
            }

            Y -= Speed * deltaSeconds;

            if (Y < -8)
                Reset(x, y);
        }

        public void AddToFrame(List<JollySprite> sprites)
        {
            if (!Active) return;
            _sprite.Position = new JollyVector2(X, Y);
            sprites.Add(_sprite);
        }
    }
}