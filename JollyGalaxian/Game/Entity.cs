namespace JollyGalaxian.Game
{
    public abstract class Entity
    {
        public float X;
        public float Y;
        public int Width;
        public int Height;
        public bool Alive = true;

        public bool Intersects(Entity other)
        {
            return X < other.X + other.Width && X + Width > other.X && Y < other.Y + other.Height && Y + Height > other.Y;
        }
    }
}

