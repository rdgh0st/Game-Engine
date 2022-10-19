using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CPI311.GameEngine;
using CPI311.GameEngine.Physics;
using Microsoft.Xna.Framework.Audio;

namespace Lab8;

public class Lab08 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    SoundEffect soundEffect;
    SoundEffectInstance soundInstance;
    Model model;
    Light light;
    Camera camera, topDownCamera;
    List<Transform> transforms;
    List<Collider> colliders;
    List<Camera> cameras;
    Effect effect;
    Texture texture;

    public Lab08()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        InputManager.Initialize();
        Time.Initialize();
        ScreenManager.Initialize(_graphics);
        transforms = new List<Transform>();
        colliders = new List<Collider>();
        cameras = new List<Camera>();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        soundEffect = Content.Load<SoundEffect>("Gun");
        model = Content.Load<Model>("Sphere");
        effect = Content.Load<Effect>("Shader");
        texture = Content.Load<Texture2D>("Square");
        // *** Lab 8 Item ***********************
        ScreenManager.Setup(true, 1920, 1080); 
        //***************************************
        camera = new Camera();
        camera.Transform = new Transform();
        camera.Transform.LocalPosition = Vector3.Backward * 5;
        camera.Position = new Vector2(0f, 0f);
        camera.Size = new Vector2(0.5f, 1f);
        camera.AspectRatio = camera.Viewport.AspectRatio;
        topDownCamera = new Camera();
        topDownCamera.Transform = new Transform();
        topDownCamera.Transform.LocalPosition = Vector3.Up * 10;
        topDownCamera.Transform.Rotate(Vector3.Right, -MathHelper.PiOver2);
        topDownCamera.Position = new Vector2(0.5f, 0f);
        topDownCamera.Size = new Vector2(0.5f, 1f);
        topDownCamera.AspectRatio = topDownCamera.Viewport.AspectRatio;
        cameras.Add(topDownCamera);
        cameras.Add(camera);
        Transform modelTransform = new Transform();
        RigidBody rigidBody = new RigidBody();
        rigidBody.Transform = modelTransform;
        SphereCollider sphereCollider = new SphereCollider();
        sphereCollider.Radius = 1.0f * modelTransform.LocalScale.Y;
        sphereCollider.Transform = modelTransform;
        light = new Light();
        Transform lightTransform = new Transform();
        lightTransform.LocalPosition = Vector3.Backward * 10 + Vector3.Right * 5;
        light.Transform = lightTransform;
        colliders.Add(sphereCollider);
        transforms.Add(modelTransform);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        InputManager.Update();
        Time.Update(gameTime);

        foreach (Camera camera in cameras)
        {
            Ray ray = camera.ScreenPointToWorldRay
                (InputManager.GetMousePosition());
            float nearest = Single.MaxValue; // Start with highest value
            float? p;
            Collider target = null; // Assume no intersection
            foreach (Collider collider in colliders)
                if ((p = collider.Intersects(ray)) != null)
                {
                    float q = (float) p;
                    if (q < nearest)
                        nearest = q;
                    target = collider;
                }

            if (target != null && nearest < camera.FarPlane)
            {

            }

            foreach (Collider collider in colliders)
            {
                if (collider.Intersects(ray) != null)
                {
                    effect.Parameters["DiffuseColor"].SetValue(Color.Red.ToVector3());
                    (model.Meshes[0].Effects[0] as BasicEffect).DiffuseColor =
                        Color.Blue.ToVector3();
                    if (InputManager.isMouseLeftClicked())
                    {
                        SoundEffectInstance instance = soundEffect.CreateInstance();
                        AudioListener listener = new AudioListener();
                        listener.Position = camera.Transform.Position;
                        listener.Forward = camera.Transform.Forward;
                        AudioEmitter emitter = new AudioEmitter();
                        emitter.Position = target.Transform.Position;
                        instance.Apply3D(listener, emitter);
                        instance.Play();
                    }
                }
                else
                {
                    effect.Parameters["DiffuseColor"].SetValue(Color.Blue.ToVector3());
                    (model.Meshes[0].Effects[0] as BasicEffect).DiffuseColor =
                        Color.Red.ToVector3();
                }
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        foreach (Camera camera in cameras)
        {
            GraphicsDevice.DepthStencilState = new DepthStencilState();
            GraphicsDevice.Viewport = camera.Viewport;
            Matrix view = camera.View;
            Matrix projection = camera.Projection;
            effect.CurrentTechnique = effect.Techniques[1];
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["LightPosition"].SetValue(light.Transform.Position);
            effect.Parameters["CameraPosition"].SetValue(camera.Transform.Position);
            effect.Parameters["Shininess"].SetValue(20f);
            //effect.Parameters["AmbientColor"].SetValue(Color.White.ToVector3());
            effect.Parameters["SpecularColor"].SetValue(Color.White.ToVector3());
            effect.Parameters["DiffuseTexture"].SetValue(texture);
            foreach (Transform transform in transforms)
            {
                effect.Parameters["World"].SetValue(transform.World);
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    foreach (ModelMesh mesh in model.Meshes)
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;
                        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, 0,
                            part.PrimitiveCount);
                    }
                }
            }
        }

        base.Draw(gameTime);
    }
}

