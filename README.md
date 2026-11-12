# JollyGalaxian

A Galaxian-inspired 2D arcade shooter written in **C#** using **WPF** and **Jolly2DEngine**, a custom DirectX 11 game engine.

Defend your ship against waves of alien fighters. Enemies break away from their formation, dive towards the player, fire projectiles and become more aggressive as the waves progress.

## Features

- Classic arcade-style fixed-screen shooting, with a scrolling starfield.
- Animated alien formations, individual dives and multi-alien escort attacks.
- Rotating aliens during dive attacks and return-to-formation behaviour.
- Player and enemy projectiles, collision detection, scoring and lives.
- Gradually increasing difficulty, including faster and more frequent attacks in later waves.
- Wave indicators: small flags for individual levels and larger flags for groups of ten.
- High-score tracking, extra lives and game-over / restart flow.
- Background audio that accelerates with wave progression and the number of aliens defeated, plus arcade sound effects.
- Rendering and audio through **Jolly2DEngine**.

## Controls

| Key | Action |
| --- | --- |
| **Left Arrow** | Move left |
| **Right Arrow** | Move right |
| **Space** | Fire / start a new game |

## Requirements

- Windows with DirectX 11 support
- .NET 10 SDK
- Visual Studio 2022 or newer with the **.NET desktop development** workload
- x64 build configuration

The project references **Jolly2DEngine** from `Libraries/Jolly2DEngine.dll`. It also uses NAudio 3.1.0 and Vortice.Direct3D11 / Vortice.DXGI 3.8.1.

## Build and run

1. Clone the repository.
2. Open `JollyGalaxian.csproj` in Visual Studio.
3. Restore NuGet packages.
4. Select the **x64** configuration.
5. Build and run.

The repository includes the engine DLL and compiled DirectX shaders under `Libraries/`. The project copies the shaders, audio and texture assets to the build output. Keep these files in their existing locations.

## Project structure

```text
JollyGalaxian/
├── Assets/
│   ├── Audio/              # Music and sound effects
│   └── Textures/           # Player, aliens and wave flags
├── Game/
│   ├── Entities/           # Player, enemies and bullets
│   ├── Formation/          # Wave generation and dive logic
│   ├── Rendering/          # Sprite helpers and starfield
│   ├── GalaxianGame.cs     # Main game loop and state management
│   └── HighScoreManager.cs
├── Libraries/
│   ├── Jolly2DEngine.dll
│   └── Rendering/DirectX/Shaders/
├── MainWindow.xaml
└── JollyGalaxian.csproj
```

## Gameplay progression

The early waves are deliberately more forgiving. Diving attacks and enemy fire become more frequent over time, and escorted multi-alien dives are introduced from **wave 3**. Wave flags at the bottom of the screen show progress: a large flag represents ten levels, while smaller flags represent individual levels.

## Development status

This is a **work-in-progress hobby emulator-style arcade recreation**, not a finished commercial release. Gameplay, collision handling, audio looping and difficulty balance are still being refined. In particular, background music speed changes depend on the version of `Jolly2DEngine.dll` included with the project.

## Acknowledgements

Inspired by the original **Galaxian** arcade game. This is an independent fan project and is not affiliated with or endorsed by Bandai Namco Entertainment.

## License

No license
