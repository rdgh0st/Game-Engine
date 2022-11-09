using System.Collections.Generic;
using CPI311.GameEngine;
using CPI311.GameEngine.GUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab11;

public class Lab11 : Game
{
    
    public class Scene
    {
        public delegate void CallMethod();
        public CallMethod Update;
        public CallMethod Draw;
        public Scene(CallMethod update, CallMethod draw)
        { Update = update; Draw = draw; }
    }
    
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D texture;
    private Button exitButton;
    private SpriteFont font;
    private Color background = Color.White;
    Dictionary<string, Scene> scenes;
    Scene currentScene;
    private List<GUIElement> guiElements;

    public Lab11()
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
        scenes = new Dictionary<string, Scene>();
        guiElements = new List<GUIElement>();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        texture = Content.Load<Texture2D>("Square");
        font = Content.Load<SpriteFont>("font");
        /*
        exitButton = new Button();
        exitButton.Texture = texture;
        exitButton.Text = "Exit";
        exitButton.Bounds = new Rectangle(50, 50, 300, 20);
        exitButton.Action += ExitGame;
        guiElements.Add(exitButton);
        */

        Checkbox box = new Checkbox();
        box.Texture = texture;
        box.Text = "Switch Scene";
        box.Bounds = new Rectangle(50, 50, 300, 50);
        box.Action += SwitchScenes;
        box.Box = texture;
        guiElements.Add(box);

        Button fullButton = new Button();
        fullButton.Texture = texture;
        fullButton.Text = "Full Screen";
        fullButton.Bounds = new Rectangle(50, 150, 300, 50);
        fullButton.Action += ToggleFullScreen;
        guiElements.Add(fullButton);
        
        scenes.Add("Menu", new Scene(MainMenuUpdate, MainMenuDraw));
        scenes.Add("Play", new Scene(PlayUpdate, PlayDraw));
        currentScene = scenes["Menu"];

    }

    protected override void Update(GameTime gameTime)
    {
        Time.Update(gameTime);
        InputManager.Update();
        
        currentScene.Update();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(background);

        currentScene.Draw();

        base.Draw(gameTime);
    }

    void ToggleFullScreen(GUIElement element)
    {
        ScreenManager.IsFullScreen = !ScreenManager.IsFullScreen;
    }
    void SwitchScenes(GUIElement element)
    {
        currentScene = currentScene == scenes["Play"] ? scenes["Menu"] : scenes["Play"];
    }
    
    void ExitGame(GUIElement element)
    {
        currentScene = scenes["Play"];
        background = (background == Color.White ? Color.Blue : Color.White);
    }
    void MainMenuUpdate()
    {
        foreach (GUIElement element in guiElements)
            element.Update();
    }
    void MainMenuDraw()
    {
        _spriteBatch.Begin();
        foreach (GUIElement element in guiElements)
            element.Draw(_spriteBatch, font);
        _spriteBatch.End();
    }
    void PlayUpdate()
    {
        if (InputManager.IsKeyPressed(Keys.Escape))
            currentScene = scenes["Menu"];
    }
    void PlayDraw()
    {
        _spriteBatch.Begin();
        _spriteBatch.DrawString(font, "Play Mode! Press \"Esc\" to go back", 
            Vector2.Zero, Color.Black);
        _spriteBatch.End();
    }
}

