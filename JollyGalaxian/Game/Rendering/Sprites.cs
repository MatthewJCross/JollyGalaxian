using Jolly2DEngine.Rendering.DirectX;

namespace JollyGalaxian.Game.Rendering;

public sealed class Sprites(DirectXJollyRenderer renderer)
{
    public SpriteAsset Player { get; } = new SpriteAsset(renderer, "Assets/Textures/Player.png", 13, 18);
    public SpriteAsset PlayerLives { get; } = new SpriteAsset(renderer, "Assets/Textures/PlayerLives.png", 11, 16);
    public SpriteAsset PlayerExplode { get; } = new SpriteAsset(renderer, "Assets/Textures/PlayerExplode.png", 136, 38, 34, 38);
    public SpriteAsset AlienYellow { get; } = new SpriteAsset(renderer, "Assets/Textures/AlienYellow.png", 126, 14, 18, 14);
    public SpriteAsset AlienPurple { get; } = new SpriteAsset(renderer, "Assets/Textures/AlienPurple.png", 218, 16, 18, 16);
    public SpriteAsset AlienRed { get; } = new SpriteAsset(renderer, "Assets/Textures/AlienRed.png", 218, 16, 18, 16);
    public SpriteAsset AlienBlue { get; } = new SpriteAsset(renderer, "Assets/Textures/AlienBlue.png", 218, 16, 18, 16);
    public SpriteAsset WaveFlag { get; } = new SpriteAsset(renderer, "Assets/Textures/WaveFlag.png", 8, 15);
    public SpriteAsset WaveFlag10 { get; } = new SpriteAsset(renderer, "Assets/Textures/WaveFlag10.png", 16, 17);
}
