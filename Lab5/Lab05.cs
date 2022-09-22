using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CPI311.GameEngine;

namespace Lab5;

public class Lab05 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Effect effect;
    private Camera camera;
    private Model model;
    private Model parent;
    private Transform parentTransform;
    private Transform modelTransform;
    private Transform cameraTransform;
    private int currentTechnique;
    private SpriteFont font;
    private string techniqueString = "Gouraud";

    public Lab05()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        InputManager.Initialize();
        Time.Initialize();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        effect = Content.Load<Effect>("Shader");
        model = Content.Load<Model>("Torus");
        parent = Content.Load<Model>("Torus");
        font = Content.Load<SpriteFont>("font");
        modelTransform = new Transform();
        parentTransform = new Transform();
        cameraTransform = new Transform();
        cameraTransform.LocalPosition = Vector3.Backward * 5;
        camera = new Camera();
        camera.Transform = cameraTransform;

        modelTransform.Parent = parentTransform;
        modelTransform.LocalPosition += new Vector3(3, 0, 0);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        InputManager.Update();
        Time.Update(gameTime);

        if (InputManager.IsKeyPressed(Keys.D1))
        {
            currentTechnique = 0;
            techniqueString = "Gouraud";
        } else if (InputManager.IsKeyPressed(Keys.D2))
        {
            currentTechnique = 1;
            techniqueString = "Phong";
        } else if (InputManager.IsKeyPressed(Keys.D3))
        {
            currentTechnique = 2;
            techniqueString = "Phong-Blinn";
        } else if (InputManager.IsKeyPressed(Keys.D4))
        {
            currentTechnique = 3;
            techniqueString = "Schlick";
        }
        
        if(InputManager.IsKeyDown(Keys.W))
            cameraTransform.LocalPosition += cameraTransform.Forward * Time.ElapsedGameTime;
        if(InputManager.IsKeyDown(Keys.S))
            cameraTransform.LocalPosition += cameraTransform.Backward * Time.ElapsedGameTime;
        if(InputManager.IsKeyDown(Keys.A))
            cameraTransform.Rotate(Vector3.Up, Time.ElapsedGameTime);
        if(InputManager.IsKeyDown(Keys.D))
            cameraTransform.Rotate(Vector3.Down, Time.ElapsedGameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        Matrix view = camera.View;
        Matrix projection = camera.Projection;
 
        effect.CurrentTechnique = effect.Techniques[currentTechnique]; //"0" is the first technique
        effect.Parameters["World"].SetValue(parentTransform.World);
        effect.Parameters["View"].SetValue(view);
        effect.Parameters["Projection"].SetValue(projection);
        effect.Parameters["LightPosition"].SetValue( Vector3.Backward * 10 + 
                                                     Vector3.Right * 5);
        effect.Parameters["CameraPosition"].SetValue(cameraTransform.Position);
        effect.Parameters["Shininess"].SetValue(20f);
        effect.Parameters["AmbientColor"].SetValue(new Vector3(0.2f, 0.2f, 0.2f));
        effect.Parameters["DiffuseColor"].SetValue(new Vector3(0.5f, 0, 0));
        effect.Parameters["SpecularColor"].SetValue(new Vector3(0, 0, 0.5f));
        //effect.Parameters["DiffuseTexture"].SetValue(texture);
        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            foreach (ModelMesh mesh in model.Meshes)
            foreach (ModelMeshPart part in mesh.MeshParts)
            {
                GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                GraphicsDevice.Indices = part.IndexBuffer;
                GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList, part.VertexOffset, 0,
                    part.NumVertices, part.StartIndex, part.PrimitiveCount);
            }
        }
        
        _spriteBatch.Begin();
        _spriteBatch.DrawString(font, techniqueString, new Vector2(50, 50), Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}

