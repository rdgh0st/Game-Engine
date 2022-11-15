namespace CPI311.GameEngine;

public class GameConstants
{
    //camera constants
    public const float CameraHeight = 800.0f;
    public const float PlayfieldSizeX = 800;
    public const float PlayfieldSizeY = 600;
//asteroid constants
    public const int NumAsteroids = 10;
    public const int NumBullets = 30;
    public const float BulletSpeedAdjustment = 3000.0f;
    public const float PlayerSpeedAdjustment = 300.0f;
    public const float AsteroidMinSpeed = 100.0f;
    public const float AsteroidMaxSpeed = 300.0f;
    public const int ShotPenalty = 1;
    public const int DeathPenalty = 100;
    public const int WarpPenalty = 50;
    public const int KillBonus = 25;
}