using CPI311.GameEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalGame3D;

public class FinalGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private GameObject player;
    private Camera camera;
    private Light light;
    private Model playerModel;

    public FinalGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        Time.Initialize();
        InputManager.Initialize();
        camera = new Camera();
        camera.Transform = new Transform();
        camera.Transform.LocalPosition = Vector3.Backward * 25;
        //camera.Transform.Rotate(Vector3.Right, -MathHelper.PiOver2);
        light = new Light();
        Transform lightTransform = new Transform();
        lightTransform.LocalPosition = Vector3.Backward * 10 + Vector3.Right * 5;
        light.Transform = lightTransform;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        playerModel = Content.Load<Model>("PirateAssets/pirate_captain");

        player = new GameObject();
        PlayerController controller = new PlayerController(new Vector3(20, 0, 20));
        player.Add<PlayerController>(controller);
        Renderer renderer = new Renderer(playerModel, player.Transform, camera, Content, GraphicsDevice, light, "Shader", 1, 20f, null);
        player.Add<Renderer>(renderer);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        InputManager.Update();
        Time.Update(gameTime);
        
        

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        player.Renderer.Material.Diffuse = Color.White.ToVector3();
        player.Draw();

        base.Draw(gameTime);
    }
}

