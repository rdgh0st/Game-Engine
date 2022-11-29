using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Xml;
using CPI311.GameEngine;
using CPI311.GameEngine.GUI;
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
    private SpriteFont titleFont;
    private Random random;
    private GameObject player;
    private Camera camera;
    private Light light;
    private Model playerModel;
    private Plane plane;
    private Sprite background;
    private List<BasicEnemy> enemies;
    private List<FinalBullet> playerBullets;
    private List<GameObject> itemsOnField;
    private List<GameObject> inventory;
    private Dictionary<Item.Slot, GameObject> currentlyEquipped;
    private List<GUIElement> guiElements;
    private float bulletSpeed = 10;
    private float harpoonSpeed = 20;
    private int luck = 0;
    private int bulletDamage = 25;
    private AOEAttack currentAOE;
    private Harpoon harpoon;
    ParticleManager particleManager;
    Texture2D particleTex;
    Effect particleEffect;
    private float timeToSpawn = 20;
    private enum GameState
    {
        Gameplay,
        Title,
        Controls,
        GameOver
    }
    private GameState currentState = GameState.Title;
    private Button ToGame;
    private Button ToControls;

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
        enemies = new List<BasicEnemy>();
        playerBullets = new List<FinalBullet>();
        itemsOnField = new List<GameObject>();
        guiElements = new List<GUIElement>();
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
        ScreenManager.SpriteBatch = _spriteBatch;

        playerModel = Content.Load<Model>("shipNew");
        background = new Sprite(Content.Load<Texture2D>("ocean"));
        background.Layer = 0.1f;
        background.Scale = new Vector2(3, 2);
        font = Content.Load<SpriteFont>("font");
        titleFont = Content.Load<SpriteFont>("titlefont");
        particleManager = new ParticleManager(GraphicsDevice, 100);
        particleEffect = Content.Load<Effect>("ParticleShader-complete");
        particleTex = Content.Load<Texture2D>("fire");
        ToGame = new Button();
        ToGame.Texture = Content.Load<Texture2D>("Square");
        ToGame.Text = "PLAY";
        ToGame.fontColor = Color.Red;
        ToGame.Bounds = new Rectangle(2 * GraphicsDevice.Viewport.Width / 3, GraphicsDevice.Viewport.Height / 2, 120, 70);
        ToGame.Action += sender =>
        {
            currentState = GameState.Gameplay;
            ThreadPool.QueueUserWorkItem(new WaitCallback(BulletEnemyCollision));
            ThreadPool.QueueUserWorkItem(new WaitCallback(PlayerCollisions));
            ThreadPool.QueueUserWorkItem(new WaitCallback(SpawnWave));
        };

        player = new Player(playerModel, null, Content, camera, GraphicsDevice,
            light);

        plane = new Plane(new Vector3(0, 0, 0), Vector3.Up);
        currentAOE = new AOEAttack(Content.Load<Model>("Sphere"), null, player.Transform.Position, Content, camera,
            GraphicsDevice, light, enemies);
        harpoon = new Harpoon(Content.Load<Model>("Sphere"), null, player.Transform.Position, Content, camera,
            GraphicsDevice, light, enemies, player);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        InputManager.Update();
        if (currentState == GameState.Title)
        {
            ToGame.Update();
        }
        if (currentState != GameState.Gameplay) return;
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

            if (InputManager.IsKeyPressed(Keys.W) && player.Get<PlayerController>().canHarpoon())
            {
                Vector3 worldPoint = ray.Position + p.Value * ray.Direction;
                Vector3 direction = worldPoint - player.Transform.Position;
                direction.Normalize();
                harpoon.Transform.Position = player.Transform.Position;
                harpoon.Get<RigidBody>().Velocity = direction * harpoonSpeed;
                Console.WriteLine("Shooting from " + player.Transform.Position + " to " + worldPoint);
                harpoon.activeSelf = true;
            }

            if (InputManager.IsKeyPressed(Keys.E) && player.Get<PlayerController>().canRush())
            {
                Vector3 worldPoint = ray.Position + p.Value * ray.Direction;
                Vector3 direction = worldPoint - player.Transform.Position;
                direction.Normalize();
                player.Rigidbody.Velocity = direction * harpoonSpeed;
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

        bool setOthers = false;
        for (int i = 0; i < enemies.Count; i++)
        {
            if (eHits[i] != null)
            {
                //(enemies[i].Renderer.ObjectModel.Meshes[0].Effects[0] as BasicEffect).DiffuseColor =
                enemies[i].hovered = true;
                Vector3 worldPoint = ray.Position + p.Value * ray.Direction;
                if (InputManager.isMouseRightClicked())
                {
                    enemies[i].selected = true;
                    setOthers = true;
                    player.Get<PlayerController>().target = worldPoint;
                    player.Get<PlayerController>().distanceToTarget = 15f;
                    player.Get<PlayerController>().CurrentState = PlayerController.State.Turning;
                }

                if (InputManager.IsKeyPressed(Keys.R) && player.Get<PlayerController>().canUlt(worldPoint))
                {
                    spawnItem(enemies[i].Transform.Position);
                    enemies[i] = null;
                    enemies.RemoveAt(i);
                    player.Get<PlayerController>().target = worldPoint;
                    player.Transform.Position = worldPoint;
                }
            }
            else
            {
                //(enemies[i].Renderer.ObjectModel.Meshes[0].Effects[0] as BasicEffect).DiffuseColor =
                if (setOthers) enemies[i].selected = false;
                enemies[i].hovered = false;
            } 
        }
        
        List<float?> iHits = new List<float?>();
        Hashtable itemHits = new Hashtable();
        for (int i = 0; i < itemsOnField.Count; i++)
        {
            GameObject e = itemsOnField[i];
            try
            {
                // ReSharper disable once HeapView.BoxingAllocation
                itemHits.Add(e, e.Get<Collider>().Intersects(ray));
                iHits.Add(e.Get<Collider>().Intersects(ray));
            }
            catch (NullReferenceException exception)
            {
                Console.WriteLine(exception);
            }
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

        if (InputManager.IsKeyPressed(Keys.Q) && player.Get<PlayerController>().canAOE())
        {
            currentAOE.SetPosition(player.Transform.Position);
            currentAOE.timer = 0;
            /*
            Particle particle = particleManager.getNext();
            particle.Position = currentAOE.Transform.Position;
            particle.Velocity = new Vector3(
                2, 2, 2);
            particle.Acceleration = new Vector3(3, 3, 3);
            particle.MaxAge = 6;
            particle.Init();
            */
        }

        player.Update();
        for (int i = 0; i < enemies.Count; i++)
        {
            BasicEnemy enemy = (BasicEnemy) enemies[i];
            if (enemy == null)
            {
                enemies.Remove(enemy);
            } 
            else
            {
                enemy.otherEnemies = enemies;
                enemy.Update();
            }
        }

        for (int i = 0; i < playerBullets.Count; i++)
        {
            GameObject bullet = playerBullets[i];
            bullet.Update();
        }

        for (int i = 0; i < guiElements.Count; i++)
        {
            guiElements[i].Update();
        }
        
        currentAOE?.Update();
        particleManager.Update();
        harpoon.Update();
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        
        _spriteBatch.Begin();
        GraphicsDevice.Clear(Color.CornflowerBlue);
        GraphicsDevice.DepthStencilState = new DepthStencilState();
        GraphicsDevice.Viewport = camera.Viewport;
        background.Draw(_spriteBatch);
        if (currentState == GameState.GameOver)
        {
            _spriteBatch.DrawString(titleFont, "GAME OVER", new Vector2(_graphics.GraphicsDevice.Viewport.Width / 2, _graphics.GraphicsDevice.Viewport.Height / 2), Color.Red);
            _spriteBatch.End();
            return;
        } 
        if (currentState == GameState.Title)
        {
            _spriteBatch.DrawString(titleFont, "SALTY SEAS", new Vector2(_graphics.GraphicsDevice.Viewport.Width / 2 - 135, _graphics.GraphicsDevice.Viewport.Height / 2 - 150), Color.Red);
            ToGame.Draw(_spriteBatch, titleFont);
            _spriteBatch.End();
            return;
        }

        for (int i = 0; i < itemsOnField.Count; i++)
        {
            GameObject item = itemsOnField[i];
            if (item.Get<Item>().drawName)
                _spriteBatch.DrawString(font, item.Get<Item>().name, item.Get<Item>().drawCoords, item.Get<Item>().color);
        }

        if (currentlyEquipped.ContainsKey(Item.Slot.Hat) && currentlyEquipped[Item.Slot.Hat] != null)
            _spriteBatch.DrawString(font, "Current Flag: " + currentlyEquipped[Item.Slot.Hat].Get<Item>().name, new Vector2(30, 30), currentlyEquipped[Item.Slot.Hat].Get<Item>().color);
        
        if (currentlyEquipped.ContainsKey(Item.Slot.Armor) && currentlyEquipped[Item.Slot.Armor] != null)
            _spriteBatch.DrawString(font, "Current Hull: " + currentlyEquipped[Item.Slot.Armor].Get<Item>().name, new Vector2(30, 50), currentlyEquipped[Item.Slot.Armor].Get<Item>().color);
        
        if (currentlyEquipped.ContainsKey(Item.Slot.Weapon) && currentlyEquipped[Item.Slot.Weapon] != null)
            _spriteBatch.DrawString(font, "Current Cannons: " + currentlyEquipped[Item.Slot.Weapon].Get<Item>().name, new Vector2(30, 70), currentlyEquipped[Item.Slot.Weapon].Get<Item>().color);
        
        if (currentlyEquipped.ContainsKey(Item.Slot.Legs) && currentlyEquipped[Item.Slot.Legs] != null)
            _spriteBatch.DrawString(font, "Current Sail: " + currentlyEquipped[Item.Slot.Legs].Get<Item>().name, new Vector2(30, 90), currentlyEquipped[Item.Slot.Legs].Get<Item>().color);
        
        if (currentlyEquipped.ContainsKey(Item.Slot.Boots) && currentlyEquipped[Item.Slot.Boots] != null)
            _spriteBatch.DrawString(font, "Current Rudder: " + currentlyEquipped[Item.Slot.Boots].Get<Item>().name, new Vector2(30, 110), currentlyEquipped[Item.Slot.Boots].Get<Item>().color);

        for (int i = 0; i < guiElements.Count; i++)
        {
            guiElements[i].Draw(_spriteBatch, font);
        }
        _spriteBatch.End();
        
        player.Draw();
        harpoon.Draw();
        //player.Renderer.Material.Diffuse = Color.White.ToVector3();

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
        }
        
        currentAOE?.Draw();
        //particle draw
        /*
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
        */
        base.Draw(gameTime);
    }

    private void spawnEnemy(Vector3 enemyPos)
    {
        BasicEnemy enemy = new BasicEnemy(Content.Load<Model>("Sphere"), null, enemyPos, Content, camera,
            GraphicsDevice, light, player, enemies);
        enemies.Add(enemy);
    }

    private void playerShoot()
    {
        FinalBullet bullet = new FinalBullet(Content.Load<Model>("Sphere"), null, Content, camera,
            GraphicsDevice, light);
        bullet.Transform.Position = player.Transform.Position;
        Vector3 direction = player.Get<PlayerController>().target - player.Transform.Position;
        direction.Normalize();
        bullet.Get<RigidBody>().Velocity = direction * bulletSpeed;
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
    
    void EquipItemfromGUI(GUIElement element)
    {
        // find index of guiElement, find corresponding name in inventory list
        Predicate<GameObject> finder = (GameObject p) => { return p.Get<Item>().name == element.Text; };
        GameObject toEquip = inventory[inventory.FindIndex(finder)];
        // swap equipped item with inventory
        string newName = currentlyEquipped[toEquip.Get<Item>().CurrentSlot].Get<Item>().name;
        Color newColor = currentlyEquipped[toEquip.Get<Item>().CurrentSlot].Get<Item>().color;
        inventory[inventory.FindIndex(finder)] = currentlyEquipped[toEquip.Get<Item>().CurrentSlot];
        currentlyEquipped[toEquip.Get<Item>().CurrentSlot] = toEquip;
        // update button
        element.Text = newName;
        element.fontColor = newColor;
        ApplyUpgrades();
    }

    void RemoveFromInventory(GUIElement element)
    {
        Predicate<GUIElement> buttonFinder = (GUIElement b) =>
        {
            return b.Bounds.Y == element.Bounds.Y && b.Text != element.Text;
        };
        GUIElement itemButton = guiElements[guiElements.FindIndex(buttonFinder)];
        Predicate<GameObject> finder = (GameObject p) => { return p.Get<Item>().name == itemButton.Text; };
        int i = inventory.FindIndex(finder);
        inventory[i] = null;
        inventory.RemoveAt(i);
        int j = guiElements.FindIndex(buttonFinder);
        guiElements[j] = null;
        guiElements.RemoveAt(j);
        int k = guiElements.IndexOf(element);
        guiElements[k] = null;
        guiElements.RemoveAt(k);
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
        while (currentState == GameState.Gameplay)
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

                if (currentAOE.isActive())
                {
                    //if (currentAOE.Get<Collider>().Collides(enemies[i].Collider, out normal) &&
                    if (Vector3.Distance(currentAOE.Transform.Position, enemies[i].Transform.Position) <= 4.75f)
                    {
                        //enemies[i].Tag = "iFrames";
                        if (enemies[i].Get<Health>().TakeDamage(50))
                        {
                            spawnItem(enemies[i].Transform.Position);
                            enemies[i] = null;
                            enemies.RemoveAt(i);
                        }
                    }
                }

                if (harpoon.activeSelf && !harpoon.retracting)
                {
                    if (enemies[i].Get<Collider>().Collides(harpoon.Get<Collider>(), out normal))
                    {
                        harpoon.retracting = true;
                        Vector3 direction = player.Transform.Position - harpoon.Transform.Position;
                        direction.Normalize();
                        harpoon.Get<RigidBody>().Velocity = direction * 20;
                        enemies[i].Get<RigidBody>().Velocity = harpoon.Get<RigidBody>().Velocity;
                    }
                }
            }
            Thread.Sleep(16);
        }
    }
    
    private void PlayerCollisions(Object obj)
    {
        while (currentState == GameState.Gameplay)
        {
            Vector3 normal;
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i].Get<Collider>().Collides(player.Get<Collider>(), out normal))
                    {
                        if (player.Rigidbody.Velocity != Vector3.Zero)
                        {
                            if (enemies[i].Get<Health>().TakeDamage(50))
                            {
                                spawnItem(enemies[i].Transform.Position);
                                enemies[i] = null;
                                enemies.RemoveAt(i);
                            }
                        }
                        else
                        {
                            if (player.Get<Health>().TakeDamage(10))
                            {
                                currentState = GameState.GameOver;
                            }
                        }
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
                            Button itemButton = new Button();
                            itemButton.Texture = Content.Load<Texture2D>("Square");
                            itemButton.Text = itemsOnField[i].Get<Item>().name;
                            itemButton.fontColor = itemsOnField[i].Get<Item>().color;
                            Predicate<GameObject> finder = (GameObject p) => { return p == itemsOnField[i]; };
                            itemButton.Bounds = new Rectangle(30, 130 + (30 * inventory.FindIndex(finder)), 300, 30);
                            itemButton.Action += EquipItemfromGUI;
                            guiElements.Add(itemButton);
                            Button trashButton = new Button();
                            trashButton.Texture = Content.Load<Texture2D>("Square");
                            trashButton.Text = "( X )";
                            trashButton.fontColor = Color.Red;
                            trashButton.Bounds = new Rectangle(340, 130 + (30 * inventory.FindIndex(finder)), 30, 30);
                            trashButton.Action += RemoveFromInventory;
                            guiElements.Add(trashButton);
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

    private void SpawnWave(Object obj)
    {
        while (currentState == GameState.Gameplay)
        {
            spawnEnemy(new Vector3(-10, 0, 0));
            spawnEnemy(new Vector3(10, 0, 0));
            spawnEnemy(new Vector3(10, 0, 10));
            spawnEnemy(new Vector3(10, 0, -10));
            spawnEnemy(new Vector3(-10, 0, 10));
            spawnEnemy(new Vector3(-10, 0, -10));
            float newTime = (timeToSpawn - 1) * 1000;
            Thread.Sleep((int) MathF.Max(0, newTime));
        }
    }

}

