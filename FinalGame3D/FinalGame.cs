using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using CPI311.GameEngine;
using CPI311.GameEngine.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalGame3D;

public class FinalGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Random random;
    private GameObject player;
    private Camera camera;
    private Light light;
    private Model playerModel;
    private Plane plane;
    private Model enemyModel;
    private List<GameObject> enemies;
    private List<GameObject> playerBullets;
    private List<GameObject> itemsOnField;
    private float bulletSpeed = 10;

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
        ScreenManager.Initialize(_graphics);
        random = new Random();
        enemies = new List<GameObject>();
        playerBullets = new List<GameObject>();
        itemsOnField = new List<GameObject>();
        camera = new Camera();
        camera.Transform = new Transform();
        camera.Transform.LocalPosition = Vector3.Up * 25 + Vector3.Backward * 10;
        camera.Transform.Rotate(Vector3.Right, MathHelper.ToRadians(-80));
        light = new Light();
        Transform lightTransform = new Transform();
        lightTransform.LocalPosition = Vector3.Backward * 10 + Vector3.Right * 5;
        light.Transform = lightTransform;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        ScreenManager.Setup(false, 1920, 1080); 

        playerModel = Content.Load<Model>("Helicopter");
        enemyModel = Content.Load<Model>("Sphere");

        player = new GameObject();
        player.Transform.Position = new Vector3(0, 0, 0);
        PlayerController controller = new PlayerController(new Vector3(20, 0, 20));
        controller.TurnSpeed = 10;
        controller.MoveSpeed = 10;
        controller.TimeToShoot = 2.5f;
        player.Add<PlayerController>(controller);
        Renderer playerRenderer = new Renderer(playerModel, player.Transform, camera, Content, GraphicsDevice, light, null, 2, 20f, Content.Load<Texture2D>("HelicopterTexture"));
        player.Add<Renderer>(playerRenderer);
        SphereCollider sphereCollider = new SphereCollider();
        sphereCollider.Radius = 1.0f * player.Transform.LocalScale.Y;
        player.Add<Collider>(sphereCollider);

        spawnEnemy(new Vector3(-10, 0, 0));
        spawnEnemy(new Vector3(10, 0, 0));

        plane = new Plane(new Vector3(0, 0, 0), Vector3.Up);
        ThreadPool.QueueUserWorkItem(new WaitCallback(BulletEnemyCollision));
        ThreadPool.QueueUserWorkItem(new WaitCallback(PlayerCollisions));
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        InputManager.Update();
        Time.Update(gameTime);

        Ray ray = camera.ScreenPointToWorldRay(InputManager.GetMousePosition());
        float? p = ray.Intersects(plane);
        
        if (p != null)
        {
            if (InputManager.isMouseRightClicked())
            {
                Vector3 worldPoint = ray.Position + p.Value * ray.Direction;
                player.Get<PlayerController>().target = worldPoint;
                player.Get<PlayerController>().distanceToTarget = 0.5f;
                player.Get<PlayerController>().CurrentState = PlayerController.State.Turning;
            }
        }

        List<float?> eHits = new List<float?>();
        Hashtable enemyHits = new Hashtable();
        foreach (GameObject e in enemies)
        {
            //Console.WriteLine(e + " " + e.Get<Collider>().Intersects(ray));
            // ReSharper disable once HeapView.BoxingAllocation
            enemyHits.Add(e, e.Get<Collider>().Intersects(ray));
            eHits.Add(e.Get<Collider>().Intersects(ray));
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            if (eHits[i] != null)
            {
                //(enemies[i].Renderer.ObjectModel.Meshes[0].Effects[0] as BasicEffect).DiffuseColor =
                enemies[i].Renderer.color =
                    Color.DarkRed.ToVector3();
                if (InputManager.isMouseRightClicked())
                {
                    Vector3 worldPoint = ray.Position + p.Value * ray.Direction;
                    player.Get<PlayerController>().target = worldPoint;
                    player.Get<PlayerController>().distanceToTarget = 15f;
                    player.Get<PlayerController>().CurrentState = PlayerController.State.Turning;
                }
            }
            else
            {
                //(enemies[i].Renderer.ObjectModel.Meshes[0].Effects[0] as BasicEffect).DiffuseColor =
                enemies[i].Renderer.color =
                    Color.Red.ToVector3();
            } 
        }

        if (player.Get<PlayerController>().CurrentState == PlayerController.State.Shooting)
        {
            playerShoot();
        }

        player.Update();
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null)
            {
                enemies.Remove(enemy);
            } 
            else
            {
                enemy.Update();
            }
        }

        foreach (GameObject bullet in playerBullets)
        {
            bullet.Update();
        }
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        GraphicsDevice.DepthStencilState = new DepthStencilState();
        GraphicsDevice.Viewport = camera.Viewport;

        //player.Renderer.Material.Diffuse = Color.White.ToVector3();
        player.Draw();

        //plane.Renderer.Material.Diffuse = Color.White.ToVector3();
        foreach (GameObject enemy in enemies)
        {
            enemy.Draw();
        }

        foreach (GameObject bullet in playerBullets)
        {
            bullet.Draw();
        }

        foreach (GameObject item in itemsOnField)
        {
            item.Draw();
        }
        
        base.Draw(gameTime);
    }

    private void spawnEnemy(Vector3 enemyPos)
    {
        GameObject enemy = new GameObject();
        enemy.Transform.Position = enemyPos;
        Renderer enemyRenderer = new Renderer(Content.Load<Model>("Sphere"), enemy.Transform, camera, Content,
            GraphicsDevice, light, null, 0, 20f, null);
        enemy.Add<Renderer>(enemyRenderer);
        SphereCollider sphereCollider = new SphereCollider();
        sphereCollider.Radius = 1.0f * enemy.Transform.LocalScale.Y;
        enemy.Add<Collider>(sphereCollider);
        Health h = new Health(100);
        enemy.Add(h);
        enemies.Add(enemy);
    }

    private void playerShoot()
    {
        GameObject bullet = new GameObject();
        bullet.Transform.Position = player.Transform.Position;
        bullet.Transform.Scale = new Vector3(0.25f, 0.25f, 0.25f);
        Renderer bulletRenderer = new Renderer(Content.Load<Model>("Sphere"), bullet.Transform, camera, Content,
            GraphicsDevice, light, null, 0, 20f, null);
        bulletRenderer.color = Color.Black.ToVector3();
        bullet.Add<Renderer>(bulletRenderer);
        SphereCollider sphereCollider = new SphereCollider();
        sphereCollider.Radius = 1.0f * bullet.Transform.LocalScale.Y;
        bullet.Add<Collider>(sphereCollider);
        RigidBody rigidbody = new RigidBody();
        rigidbody.Mass = 1;
        Vector3 direction = player.Get<PlayerController>().target - player.Transform.Position;
        direction.Normalize();
        rigidbody.Velocity = direction * bulletSpeed;
        bullet.Add<RigidBody>(rigidbody);
        playerBullets.Add(bullet);
    }

    private void spawnItem(Vector3 pos)
    {
        GameObject item = new GameObject();
        pos.X += (random.NextSingle() - 0.5f) * 3;
        pos.Z += (random.NextSingle() - 0.5f) * 3;
        item.Transform.Position = pos;
        item.Transform.Scale = new Vector3(0.5f, 0.5f, 0.5f);
        Renderer itemRenderer = new Renderer(Content.Load<Model>("Sphere"), item.Transform, camera, Content,
            GraphicsDevice, light, null, 0, 20f, null);
        item.Add(itemRenderer);
        SphereCollider sphereCollider = new SphereCollider();
        sphereCollider.Radius = 1.0f * item.Transform.LocalScale.Y;
        item.Add<Collider>(sphereCollider);
        Item itemType = new Item(random.Next(0, 101), random.Next(0, 101));
        item.Add(itemType);
        item.Renderer.color = itemType.CurrentRarity switch
        {
            Item.Rarity.Common => Color.White.ToVector3(),
            Item.Rarity.Uncommon => Color.Green.ToVector3(),
            Item.Rarity.Rare => Color.MediumBlue.ToVector3(),
            Item.Rarity.Epic => Color.Purple.ToVector3(),
            Item.Rarity.Legendary => Color.Gold.ToVector3()
        };
        Console.WriteLine("Spawned " + itemType.CurrentRarity + " " + itemType.CurrentSlot);
        itemsOnField.Add(item);
    }
    
    private void BulletEnemyCollision(Object obj)
    {
        while (true)
        {
            Vector3 normal;
            for (int i = 0; i < enemies.Count; i++)
            {
                for (int j = 0; j < playerBullets.Count; j++)
                {
                    if (enemies[i].Get<Collider>().Collides(playerBullets[j].Get<Collider>(), out normal))
                    {
                        if (enemies[i].Get<Health>().TakeDamage(25))
                        {
                            spawnItem(enemies[i].Transform.Position);
                            enemies[i] = null;
                            enemies.RemoveAt(i);
                        }
                        Console.WriteLine("Enemy took damage!");
                        playerBullets[j] = null;
                        playerBullets.RemoveAt(j);
                    }
                }
            }
            Thread.Sleep(16);
        }
    }
    
    private void PlayerCollisions(Object obj)
    {
        while (true)
        {
            Vector3 normal;
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i].Get<Collider>().Collides(player.Get<Collider>(), out normal))
                    {
                        // player takes damage
                        Console.WriteLine("Player Damaged!");
                    }
                }

                for (int i = 0; i < itemsOnField.Count; i++)
                {
                    if (itemsOnField[i].Get<Collider>().Collides(player.Get<Collider>(), out normal))
                    {
                        // equip item
                        itemsOnField[i].Get<Item>().isEquipped = true;
                        itemsOnField[i].Remove<Collider>();
                        Console.WriteLine("Equipped " + itemsOnField[i].Get<Item>().CurrentRarity + " " + itemsOnField[i].Get<Item>().CurrentSlot);
                        itemsOnField.RemoveAt(i);
                    }
                }
            Thread.Sleep(16);
        }
    }

}

