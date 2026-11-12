using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;

namespace JollyGalaxian.Game.Entities
{
    public sealed class Bullet
    {
        private readonly JollySprite _sprite;
        public float X;
        public float Y;
        public float VX;
        public float VY;
        public bool Active = true;
        public int Damage = 1;

        public Bullet(JollySprite sprite)
        {
            _sprite = sprite ?? throw new ArgumentNullException(nameof(sprite));
        }

        public void AddToFrame(List<JollySprite> sprites)
        {
            if (!Active) return;
            _sprite.Position = new JollyVector2(X, Y);
            sprites.Add(_sprite);
        }
    }
}
