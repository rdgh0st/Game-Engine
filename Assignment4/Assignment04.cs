using System;
using CPI311.GameEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Assignment4;

public class Assignment04 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    Camera camera;
    Light light;
    Random random;
    SoundEffect gunSound;
    SoundEffectInstance soundInstance;

    private SoundEffect explosion;
    private SoundEffect deathSound;
    private SoundEffectInstance explosionInstance;
    private SoundEffectInstance deathInstance;
    //Visual components
    Ship ship;
    Asteroid[] asteroidList = new Asteroid[GameConstants.NumAsteroids];
    Bullet[] bulletList = new Bullet[GameConstants.NumBullets];
    //Score & background
    int score;
    Texture2D stars;
    SpriteFont lucidaConsole;
    Vector2 scorePosition = new Vector2(100, 50);
    // Particles
    ParticleManager particleManager;
    Texture2D particleTex;
    Effect particleEffect;

    public Assignment04()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        Time.Initialize();
        InputManager.Initialize();
        ScreenManager.Initialize(_graphics);
        random = new Random();
        
        light = new Light();
        Transform lightTransform = new Transform();
        lightTransform.LocalPosition = Vector3.Backward * 10 + Vector3.Right * 5;
        light.Transform = lightTransform;
        Transform cameraTransform = new Transform();
        cameraTransform.LocalPosition = Vector3.Backward * GameConstants.CameraHeight;
        camera = new Camera();
        camera.Transform = cameraTransform;
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        ship = new Ship(Content, camera, GraphicsDevice, light);
        ship.Transform.Rotate(Vector3.Right, MathHelper.ToRadians(90));
        for (int i = 0; i < GameConstants.NumBullets; i++) 
            bulletList[i] = new Bullet(Content, camera, GraphicsDevice, light);
        ResetAsteroids(); // look at the below private method
        // *** Particle
        particleManager = new ParticleManager(GraphicsDevice, 100);
        particleEffect = Content.Load<Effect>("ParticleShader-complete");
        particleTex = Content.Load<Texture2D>("fire");
        stars = Content.Load<Texture2D>("B1_stars");
        gunSound = Content.Load<SoundEffect>("tx0_fire1");
        explosion = Content.Load<SoundEffect>("explosion2");
        deathSound = Content.Load<SoundEffect>("explosion3");
        lucidaConsole = Content.Load<SpriteFont>("font");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        InputManager.Update();
        Time.Update(gameTime);
        ship.Update();
        for (int i = 0; i < GameConstants.NumBullets; i++) 
            bulletList[i].Update();
        for (int i = 0; i < GameConstants.NumAsteroids; i++)
            asteroidList[i].Update();
        
        if (InputManager.isMouseLeftClicked() && ship.isActive)
        {
            for (int i = 0; i < GameConstants.NumBullets; i++)
            {
                if (!bulletList[i].isActive)
                {
                    bulletList[i].Rigidbody.Velocity = 
                        (ship.Transform.Forward)*GameConstants.BulletSpeedAdjustment;
                    Console.WriteLine(bulletList[i].Rigidbody.Velocity);
                    //bulletList[i].Transform.LocalPosition = ship.Transform.Position + (200 * bulletList[i].Transform.Forward);
                    bulletList[i].Transform.LocalPosition = ship.Transform.Position;
                    bulletList[i].isActive = true;
                    score -= GameConstants.ShotPenalty;
                    // sound
                    soundInstance = gunSound.CreateInstance();
                    soundInstance.Play();
                    break; //exit the loop     
                }
            } 
        }
        
        Vector3 normal;
        for (int i = 0; i < asteroidList.Length; i++)
            if (asteroidList[i].isActive)
                for (int j = 0; j < bulletList.Length; j++)
                    if (bulletList[j].isActive)
                        if (asteroidList[i].Collider.Collides(bulletList[j].Collider, out normal))
                        { 
                            // Particles
                            asteroidList[i].isActive = false;
                            bulletList[j].isActive = false;
                            Particle particle = particleManager.getNext();
                            particle.Position = asteroidList[i].Transform.Position;
                            particle.Velocity = new Vector3(
                                random.Next(-5, 5), 2, random.Next(-50, 50));
                            particle.Acceleration = new Vector3(0, 3, 0);
                            particle.MaxAge = random.Next(1, 6);
                            particle.Init();
                            explosionInstance = explosion.CreateInstance();
                            explosionInstance.Play();
                            score += GameConstants.KillBonus;
                            break; //no need to check other bullets
                        }
        for (int i = 0; i < asteroidList.Length; i++)
            if (asteroidList[i].isActive && ship.isActive)
                if (asteroidList[i].Collider.Collides(ship.Collider, out normal))
                {
                    ship.isActive = false;
                    Particle particle = particleManager.getNext();
                    particle.Position = ship.Transform.Position;
                    particle.Velocity = new Vector3(
                        random.Next(-5, 5), 2, random.Next(-50, 50));
                    particle.Acceleration = new Vector3(0, 3, 0);
                    particle.MaxAge = random.Next(1, 6);
                    particle.Init();
                    deathInstance = deathSound.CreateInstance();
                    deathInstance.Play();
                }
        // particles update
        particleManager.Update();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        _spriteBatch.Begin();
        _spriteBatch.Draw(stars, new Rectangle(0, 0, 800, 600), Color.White);
        _spriteBatch.DrawString(lucidaConsole, "Score: " + score, scorePosition, Color.White);
        if (!ship.isActive)
            _spriteBatch.DrawString(lucidaConsole, "GAME OVER", new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2), Color.White);
        _spriteBatch.End();
        GraphicsDevice.RasterizerState = RasterizerState.CullNone; 
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
          
            
        // ship, bullets, and asteroids
        if (ship.isActive)
            ship.Draw();
        
        for (int i = 0; i < GameConstants.NumBullets; i++) bulletList[i].Draw();
        for (int i = 0; i < GameConstants.NumAsteroids; i++) asteroidList[i].Draw();
        //particle draw
        GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
        particleEffect.CurrentTechnique = particleEffect.Techniques["particle"];
        particleEffect.CurrentTechnique.Passes[0].Apply();
        particleEffect.Parameters["ViewProj"].SetValue(camera.View *camera.Projection);
        particleEffect.Parameters["World"].SetValue(Matrix.Identity);
        particleEffect.Parameters["CamIRot"].SetValue(
            Matrix.Invert(Matrix.CreateFromQuaternion(camera.Transform.Rotation)));
        particleEffect.Parameters["Texture"].SetValue(particleTex);
        particleManager.Draw(GraphicsDevice);
        GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        base.Draw(gameTime);

        base.Draw(gameTime);
    }
    
    private void ResetAsteroids()
    {
        float xStart;
        float yStart;
        for (int i = 0; i < GameConstants.NumAsteroids; i++)
        {
            if (random.Next(2) == 0)  
                xStart = (float)-GameConstants.PlayfieldSizeX;
            else
                xStart = (float)GameConstants.PlayfieldSizeX;
            yStart = (float)random.NextDouble() * GameConstants.PlayfieldSizeY;
            asteroidList[i] = new Asteroid(Content, camera, GraphicsDevice, light);
            asteroidList[i].Transform.Position = new Vector3(xStart, yStart, 0);
            asteroidList[i].Transform.Rotation = Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(random.Next(360)), MathHelper.ToRadians(random.Next(360)), MathHelper.ToRadians(random.Next(360)));
            double angle = random.NextDouble() * 2 * Math.PI;
            asteroidList[i].Rigidbody.Velocity = new Vector3(
                                                     -(float)Math.Sin(angle),(float)Math.Cos(angle), 0) * 
                                                 (GameConstants.AsteroidMinSpeed + (float)random.NextDouble() * 
                                                     GameConstants.AsteroidMaxSpeed);
            asteroidList[i].isActive = true;
        }
    }
}

