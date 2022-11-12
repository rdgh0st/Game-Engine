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
    private SpriteFont font;
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
    private List<GameObject> inventory;
    private Dictionary<Item.Slot, GameObject> currentlyEquipped;
    private float bulletSpeed = 10;
    private int luck = 0;
    private int bulletDamage = 25;

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
        currentlyEquipped = new Dictionary<Item.Slot, GameObject>();
        inventory = new List<GameObject>();
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
        font = Content.Load<SpriteFont>("font");

        player = new GameObject();
        player.Transform.Position = new Vector3(0, 0, 0);
        PlayerController controller = new PlayerController(new Vector3(20, 0, 20));
        controller.TurnSpeed = 10;
        controller.MoveSpeed = 10;
        controller.TimeToShoot = 1.5f;
        player.Add<PlayerController>(controller);
        Renderer playerRenderer = new Renderer(playerModel, player.Transform, camera, Content, GraphicsDevice, light, null, 2, 20f, Content.Load<Texture2D>("HelicopterTexture"));
        player.Add<Renderer>(playerRenderer);
        SphereCollider sphereCollider = new SphereCollider();
        sphereCollider.Radius = 1.0f * player.Transform.LocalScale.Y;
        player.Add<Collider>(sphereCollider);
        Health health = new Health(100);
        player.Add(health);

        spawnEnemy(new Vector3(-10, 0, 0));
        spawnEnemy(new Vector3(10, 0, 0));
        spawnEnemy(new Vector3(10, 0, 10));
        spawnEnemy(new Vector3(10, 0, -10));

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
        for (int i = 0; i < enemies.Count; i++)
        {
            GameObject e = enemies[i];
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
        
        List<float?> iHits = new List<float?>();
        Hashtable itemHits = new Hashtable();
        for (int i = 0; i < itemsOnField.Count; i++)
        {
            GameObject e = itemsOnField[i];
            //Console.WriteLine(e + " " + e.Get<Collider>().Intersects(ray));
            // ReSharper disable once HeapView.BoxingAllocation
            itemHits.Add(e, e.Get<Collider>().Intersects(ray));
            iHits.Add(e.Get<Collider>().Intersects(ray));
        }

        for (int i = 0; i < itemsOnField.Count; i++)
        {
            if (iHits[i] != null)
            {
                //(enemies[i].Renderer.ObjectModel.Meshes[0].Effects[0] as BasicEffect).DiffuseColor =
                itemsOnField[i].Get<Item>().drawName = true;
                itemsOnField[i].Get<Item>().setDrawCoords(InputManager.GetMousePosition() + new Vector2(-80, -40));
                if (InputManager.isMouseRightClicked())
                {
                    Vector3 worldPoint = ray.Position + p.Value * ray.Direction;
                    player.Get<PlayerController>().target = worldPoint;
                    player.Get<PlayerController>().distanceToTarget = 0.5f;
                    player.Get<PlayerController>().CurrentState = PlayerController.State.Turning;
                }
            }
            else
            {
                //(enemies[i].Renderer.ObjectModel.Meshes[0].Effects[0] as BasicEffect).DiffuseColor =
                itemsOnField[i].Get<Item>().drawName = false;
            } 
        }

        if (player.Get<PlayerController>().CurrentState == PlayerController.State.Shooting)
        {
            playerShoot();
        }

        player.Update();
        for (int i = 0; i < enemies.Count; i++)
        {
            GameObject enemy = enemies[i];
            if (enemy == null)
            {
                enemies.Remove(enemy);
            } 
            else
            {
                enemy.Update();
            }
        }

        for (int i = 0; i < playerBullets.Count; i++)
        {
            GameObject bullet = playerBullets[i];
            bullet.Update();
        }
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        
        _spriteBatch.Begin();
        GraphicsDevice.Clear(Color.CornflowerBlue);
        GraphicsDevice.DepthStencilState = new DepthStencilState();
        GraphicsDevice.Viewport = camera.Viewport;

        //player.Renderer.Material.Diffuse = Color.White.ToVector3();
        player.Draw();

        //plane.Renderer.Material.Diffuse = Color.White.ToVector3();
        for (int i = 0; i < enemies.Count; i++)
        {
            GameObject enemy = enemies[i];
            enemy.Draw();
        }

        for (int i = 0; i < playerBullets.Count; i++)
        {
            GameObject bullet = playerBullets[i];
            bullet.Draw();
        }

        for (int i = 0; i < itemsOnField.Count; i++)
        {
            GameObject item = itemsOnField[i];
            item.Draw();
            _graphics.GraphicsDevice.Viewport.Project(item.Transform.Position, camera.Projection, camera.View,
                item.Transform.World);
            Vector3 screenView = camera.Viewport.Project(item.Transform.Position, camera.Projection,
                camera.View, item.Transform.World);
            if (item.Get<Item>().drawName)
                _spriteBatch.DrawString(font, item.Get<Item>().name, item.Get<Item>().drawCoords, item.Get<Item>().color);
        }
        
        if (currentlyEquipped.ContainsKey(Item.Slot.Hat) && currentlyEquipped[Item.Slot.Hat] != null)
            _spriteBatch.DrawString(font, "Current Hat: " + currentlyEquipped[Item.Slot.Hat].Get<Item>().name, new Vector2(30, 30), currentlyEquipped[Item.Slot.Hat].Get<Item>().color);
        
        if (currentlyEquipped.ContainsKey(Item.Slot.Armor) && currentlyEquipped[Item.Slot.Armor] != null)
            _spriteBatch.DrawString(font, "Current Armor: " + currentlyEquipped[Item.Slot.Armor].Get<Item>().name, new Vector2(30, 50), currentlyEquipped[Item.Slot.Armor].Get<Item>().color);
        
        if (currentlyEquipped.ContainsKey(Item.Slot.Weapon) && currentlyEquipped[Item.Slot.Weapon] != null)
            _spriteBatch.DrawString(font, "Current Weapon: " + currentlyEquipped[Item.Slot.Weapon].Get<Item>().name, new Vector2(30, 70), currentlyEquipped[Item.Slot.Weapon].Get<Item>().color);
        
        if (currentlyEquipped.ContainsKey(Item.Slot.Legs) && currentlyEquipped[Item.Slot.Legs] != null)
            _spriteBatch.DrawString(font, "Current Legs: " + currentlyEquipped[Item.Slot.Legs].Get<Item>().name, new Vector2(30, 90), currentlyEquipped[Item.Slot.Legs].Get<Item>().color);
        
        if (currentlyEquipped.ContainsKey(Item.Slot.Boots) && currentlyEquipped[Item.Slot.Boots] != null)
            _spriteBatch.DrawString(font, "Current Boots: " + currentlyEquipped[Item.Slot.Boots].Get<Item>().name, new Vector2(30, 110), currentlyEquipped[Item.Slot.Boots].Get<Item>().color);
        
        _spriteBatch.End();
        
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
        Item itemType = new Item(random.Next(0, 101), random.Next(luck, 101));
        item.Add(itemType);
        item.Renderer.color = itemType.color.ToVector3();
        Console.WriteLine("Spawned " + itemType.CurrentRarity + " " + itemType.CurrentSlot);
        itemsOnField.Add(item);
    }

    private void ApplyUpgrades()
    {
        foreach (Item.Slot slot in currentlyEquipped.Keys)
        {
            switch (slot)
            {
                case Item.Slot.Armor:
                    switch (currentlyEquipped[slot].Get<Item>().CurrentRarity)
                    {
                        case Item.Rarity.Common:
                            player.Get<Health>().MaxHealth = 110;
                            player.Get<Health>().Heal(25);
                            break;
                        case Item.Rarity.Uncommon:
                            player.Get<Health>().MaxHealth = 125;
                            player.Get<Health>().Heal(25);
                            break;
                        case Item.Rarity.Rare:
                            player.Get<Health>().MaxHealth = 150;
                            player.Get<Health>().Heal(25);
                            break;
                        case Item.Rarity.Epic:
                            player.Get<Health>().MaxHealth = 200;
                            player.Get<Health>().Heal(25);
                            break;
                        case Item.Rarity.Legendary:
                            player.Get<Health>().MaxHealth = 300;
                            player.Get<Health>().Heal(25);
                            break;
                    }
                    break;
                case Item.Slot.Boots:
                    switch (currentlyEquipped[slot].Get<Item>().CurrentRarity)
                    {
                        case Item.Rarity.Common:
                            player.Get<PlayerController>().TurnSpeed = 11;
                            break;
                        case Item.Rarity.Uncommon:
                            player.Get<PlayerController>().TurnSpeed = 12;
                            break;
                        case Item.Rarity.Rare:
                            player.Get<PlayerController>().TurnSpeed = 15;
                            break;
                        case Item.Rarity.Epic:
                            player.Get<PlayerController>().TurnSpeed = 20;
                            break;
                        case Item.Rarity.Legendary:
                            player.Get<PlayerController>().TurnSpeed = 30;
                            break;
                    }
                    break;
                case Item.Slot.Hat:
                    switch (currentlyEquipped[slot].Get<Item>().CurrentRarity)
                    {
                        case Item.Rarity.Common:
                            luck = 10;
                            break;
                        case Item.Rarity.Uncommon:
                            luck = 20;
                            break;
                        case Item.Rarity.Rare:
                            luck = 35;
                            break;
                        case Item.Rarity.Epic:
                            luck = 50;
                            break;
                        case Item.Rarity.Legendary:
                            luck = 75;
                            break;
                    }
                    break;
                case Item.Slot.Legs:
                    switch (currentlyEquipped[slot].Get<Item>().CurrentRarity)
                    {
                        case Item.Rarity.Common:
                            player.Get<PlayerController>().MoveSpeed = 11;                            
                            break;
                        case Item.Rarity.Uncommon:
                            player.Get<PlayerController>().MoveSpeed = 12;
                            break;
                        case Item.Rarity.Rare:
                            player.Get<PlayerController>().MoveSpeed = 15;
                            break;
                        case Item.Rarity.Epic:
                            player.Get<PlayerController>().MoveSpeed = 20;
                            break;
                        case Item.Rarity.Legendary:
                            player.Get<PlayerController>().MoveSpeed = 30;
                            break;
                    }
                    break;
                case Item.Slot.Weapon:
                    switch (currentlyEquipped[slot].Get<Item>().CurrentRarity)
                    {
                        case Item.Rarity.Common:
                            bulletDamage = 30;
                            break;
                        case Item.Rarity.Uncommon:
                            bulletDamage = 40;
                            break;
                        case Item.Rarity.Rare:
                            bulletDamage = 50;
                            break;
                        case Item.Rarity.Epic:
                            bulletDamage = 75;
                            break;
                        case Item.Rarity.Legendary:
                            bulletDamage = 100;
                            break;
                    }
                    break;
            }
        }
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
                        if (enemies[i].Get<Health>().TakeDamage(bulletDamage))
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
                        if (!currentlyEquipped.ContainsKey(itemsOnField[i].Get<Item>().CurrentSlot))
                        {
                            currentlyEquipped.Add(itemsOnField[i].Get<Item>().CurrentSlot, itemsOnField[i]);
                        }
                        else
                        {
                            inventory.Add(itemsOnField[i]);
                        }
                        itemsOnField[i].Remove<Collider>();
                        Console.WriteLine("Equipped " + itemsOnField[i].Get<Item>().CurrentRarity + " " + itemsOnField[i].Get<Item>().CurrentSlot);
                        itemsOnField.RemoveAt(i);
                        ApplyUpgrades();
                    }
                }
            Thread.Sleep(16);
        }
    }

}

