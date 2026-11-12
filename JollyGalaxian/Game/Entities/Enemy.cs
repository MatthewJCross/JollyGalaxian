using Jolly2DEngine.Graphics;
using JollyGalaxian.Game.Rendering;

namespace JollyGalaxian.Game.Entities
{
    public sealed class Enemy : Entity
    {
        private readonly SpriteAsset _spriteAsset;
        private readonly JollySprite _sprite;

        public float HomeX;
        public float HomeY;
        public bool IsEnteringFormation;
        public float EntryStartX;
        public float EntryStartY;
        public float EntryDelay;
        public float EntryElapsed;

        public int Row;
        public int Column;

        public bool IsDiving;
        public bool IsReturning;
        public float DiveTime;
        public float DiveStartX;
        public float DiveStartY;
        public float DiveSpeed = 80.0f;
        public float BreakawaySpeed = 40.0f;
        public float ReturnSpeed = 120.0f;
        public float HomingStrength = 0.02f;
        public float PlayerX;
        public float TargetX;
        public float Rotation;

        public int Type;
        public int Score;
        public int Frame;

        public Enemy? DiveLeader;
        public bool IsPackLeader;
        public bool IsPackEscort;
        public bool HasEscorts;
        public float DiveOffsetX;
        public float DiveOffsetY;

        public SpriteAsset SpriteAsset => _spriteAsset;

        public Enemy(SpriteAsset spriteAsset)
        {
            _spriteAsset = spriteAsset;
            _sprite = spriteAsset.CreateSprite();
            Width = spriteAsset.FrameWidth;
            Height = spriteAsset.FrameHeight;
        }

        public void UpdateFormationEntry(float deltaSeconds)
        {
            if (!IsEnteringFormation || !Alive)
                return;

            EntryElapsed += deltaSeconds;
            if (EntryElapsed < EntryDelay)
                return;

            float t = Math.Clamp((EntryElapsed - EntryDelay) / 1.15f, 0.0f, 1.0f);
            float eased = t * t * (3.0f - 2.0f * t);
            float arc = MathF.Sin(t * MathF.PI) * (Row % 2 == 0 ? -22.0f : 22.0f);

            X = EntryStartX + (HomeX - EntryStartX) * eased + arc;
            Y = EntryStartY + (HomeY - EntryStartY) * eased;

            if (t >= 1.0f)
            {
                X = HomeX;
                Y = HomeY;
                IsEnteringFormation = false;
                Rotation = 0;
            }
        }

        public void UpdateDiving(float deltaSeconds)
        {
            if (!Alive || IsEnteringFormation)
                return;

            if (DiveLeader is { Alive: false })
            {
                DiveLeader = null;
                IsDiving = false;
                IsReturning = true;
            }

            if (DiveLeader is { Alive: true } leader)
            {
                if (leader.IsDiving)
                {
                    float angle = (leader.Rotation - 90.0f) * MathF.PI / 180.0f;
                    float cos = MathF.Cos(angle);
                    float sin = MathF.Sin(angle);
                    X = leader.X + DiveOffsetX * cos - DiveOffsetY * sin;
                    Y = leader.Y + DiveOffsetX * sin + DiveOffsetY * cos;
                    Rotation = leader.Rotation;
                    return;
                }

                IsDiving = false;
                IsReturning = true;
                DiveLeader = null;
            }

            if (IsDiving)
            {
                DiveTime += deltaSeconds;
                float oldX = X;
                float oldY = Y;

                if (DiveTime < 1.0f)
                {
                    X = DiveStartX + MathF.Sin(DiveTime * 4.0f) * 25.0f;
                    Y = DiveStartY + DiveTime * BreakawaySpeed;
                }
                else
                {
                    float attackTime = DiveTime - 1.0f;
                    float tracking = 1.0f - MathF.Exp(-HomingStrength * 60.0f * deltaSeconds);
                    TargetX += (PlayerX - TargetX) * tracking;

                    float centreX = DiveStartX + (TargetX - DiveStartX) * MathF.Min(attackTime / 2.0f, 1.0f);
                    X = centreX + MathF.Sin(attackTime * 3.0f) * 60.0f;
                    Y = DiveStartY + BreakawaySpeed + attackTime * DiveSpeed;
                }

                Rotation = MathF.Atan2(Y - oldY, X - oldX) * 180.0f / MathF.PI - 90.0f;

                if (Y > GameConstants.ScreenH + 20)
                {
                    IsDiving = false;
                    IsReturning = true;
                    Y = -20;
                }

                return;
            }

            if (!IsReturning)
                return;

            float dx = HomeX - X;
            float dy = HomeY - Y;
            float distance = MathF.Sqrt(dx * dx + dy * dy);

            if (distance <= 4.0f)
            {
                X = HomeX;
                Y = HomeY;
                IsReturning = false;
                Rotation = 0;
                return;
            }

            float oldReturnX = X;
            float oldReturnY = Y;
            float step = Math.Min(ReturnSpeed * deltaSeconds, distance);
            X += dx / distance * step;
            Y += dy / distance * step;
            Rotation = MathF.Atan2(Y - oldReturnY, X - oldReturnX) * 180.0f / MathF.PI - 90.0f;
        }

        public void AddToFrame(List<JollySprite> sprites)
        {
            if (!Alive || X < -Width || X > GameConstants.ScreenW || Y < -Height || Y > GameConstants.ScreenH)
                return;

            _spriteAsset.SetFrame(_sprite, Frame);
            _spriteAsset.SetPosition(_sprite, X, Y);
            _sprite.Rotation = IsDiving || IsReturning ? Rotation : 0;
            sprites.Add(_sprite);
        }
    }
}