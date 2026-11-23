using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ChickenShooter.EditorTools
{
    /// <summary>
    /// One-shot deterministic project generator. Rebuilds art, prefabs,
    /// ScriptableObject data, UI, scenes and build settings in place.
    /// </summary>
    public static partial class ProjectSetup
    {
        private const string PrefabDir = "Assets/Prefabs";
        private const string DataDir = "Assets/Data";
        private const string WaveDir = DataDir + "/Waves";
        private const string LevelDir = DataDir + "/Levels";
        private const string EnemyDir = DataDir + "/Enemies";
        private const string WeaponDir = DataDir + "/Weapons";
        private const string SceneDir = "Assets/Scenes";
        private const string VfxDir = PrefabDir + "/Vfx";

        private const string PlayerPrefabPath = PrefabDir + "/Player.prefab";
        private const string BulletPrefabPath = PrefabDir + "/Bullet.prefab";
        private const string EggPrefabPath = PrefabDir + "/Egg.prefab";
        private const string FoodPrefabPath = PrefabDir + "/Food.prefab";
        private const string GiftPrefabPath = PrefabDir + "/Gift.prefab";
        private const string AsteroidPrefabPath = PrefabDir + "/Asteroid.prefab";
        private const string ChickenPrefabPath = PrefabDir + "/Chicken_{0}.prefab";
        private const string GameManagerPrefabPath = "Assets/Resources/GameManager.prefab";
        private const string InputAssetPath = "Assets/Resources/GameInput.inputactions";
        private const string DatabasePath = "Assets/Resources/LevelDatabase.asset";
        private const string VolumeProfilePath = "Assets/Settings/GameVolumeProfile.asset";

        [MenuItem("Tools/Chicken Shooter/Setup Project")]
        public static void SetupAll()
        {
            try
            {
                EnsureTmpEssentials();
                EnsureFolders();
                SetupTagsAndLayers();
                CloseOrphanScenes();

                InputActionAsset input = CreateInputAsset();
                ArtLibrary.SpriteSet sprites = ArtLibrary.Build();

                GameObject bulletPrefab = CreateBulletPrefab(sprites.Laser);
                GameObject eggPrefab = CreateEggPrefab(sprites.Egg);
                GameObject foodPrefab = CreatePickupPrefab("Food", sprites.Food, PickupType.Food);
                GameObject giftPrefab = CreatePickupPrefab("Gift", sprites.Gift, PickupType.Gift);
                GameObject asteroidPrefab = CreateAsteroidPrefab(sprites);

                WeaponDataSO[] weapons = CreateWeapons(bulletPrefab);
                GameObject playerPrefab = CreatePlayerPrefab(sprites.Ship, input, weapons);

                EnemyDataSO basicData = CreateEnemyData(
                    "Basic", "Basic Chicken", sprites.ChickenBasic[0], 2, 10, 3.2f,
                    new Vector2(6f, 10f), 4.2f, 0.035f, 5.5f, 0.16f, 0.03f,
                    eggPrefab, foodPrefab, giftPrefab);

                EnemyDataSO poisonData = CreateEnemyData(
                    "Poison", "Poison Chicken", sprites.ChickenPoison[0], 3, 20, 2.9f,
                    new Vector2(5f, 8.5f), 4.8f, 0.07f, 6f, 0.18f, 0.035f,
                    eggPrefab, foodPrefab, giftPrefab);

                EnemyDataSO alienData = CreateEnemyData(
                    "Alien", "Cosmic Chicken", sprites.ChickenAlien[0], 4, 30, 3.4f,
                    new Vector2(4f, 7f), 5.2f, 0.1f, 7f, 0.2f, 0.04f,
                    eggPrefab, foodPrefab, giftPrefab);

                GameObject basicChicken = CreateChickenPrefab("Basic", sprites.ChickenBasic);
                GameObject poisonChicken = CreateChickenPrefab("Poison", sprites.ChickenPoison);
                GameObject alienChicken = CreateChickenPrefab("Alien", sprites.ChickenAlien);

                LevelDataSO[] levels = CreateLevels(
                    basicData, poisonData, alienData,
                    basicChicken, poisonChicken, alienChicken,
                    asteroidPrefab);

                LevelDatabaseSO database = CreateDatabase(levels);
                CreateGameManagerPrefab(database, sprites);

                CreateLevelScene("Level1", levels[0], playerPrefab, sprites);
                CreateLevelScene("Level2", levels[1], playerPrefab, sprites);
                CreateLevelScene("Level3", levels[2], playerPrefab, sprites);
                CreateMainMenuScene(sprites);

                SetBuildSettings();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("[ChickenShooter] Project setup complete: HD art, UI, backgrounds, VFX, scenes and build settings are ready.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        // ------------------------------------------------------------------
        // Folders / project settings
        // ------------------------------------------------------------------

        private static void EnsureTmpEssentials()
        {
            if (TMP_Settings.defaultFontAsset != null)
            {
                return;
            }

            TMP_PackageResourceImporter.ImportResources(true, false, false);
            AssetDatabase.Refresh();
        }

        private static void EnsureFolders()
        {
            EnsureFolder(ArtLibrary.ArtDir);
            EnsureFolder(PrefabDir);
            EnsureFolder(VfxDir);
            EnsureFolder(DataDir);
            EnsureFolder(WaveDir);
            EnsureFolder(LevelDir);
            EnsureFolder(EnemyDir);
            EnsureFolder(WeaponDir);
            EnsureFolder("Assets/Resources");
            EnsureFolder(SceneDir);
            EnsureFolder("Assets/Settings");
        }

        private static void EnsureFolder(string path)
        {
            if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(parent))
            {
                return;
            }

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static void SetupTagsAndLayers()
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0)
            {
                Debug.LogWarning("[ChickenShooter] Could not open TagManager.asset.");
                return;
            }

            SerializedObject tagManager = new SerializedObject(assets[0]);
            AddTag(tagManager.FindProperty("tags"), "Enemy");

            SerializedProperty layers = tagManager.FindProperty("layers");
            SetLayer(layers, GameLayers.Player, "Player");
            SetLayer(layers, GameLayers.PlayerProjectile, "PlayerProjectile");
            SetLayer(layers, GameLayers.Enemy, "Enemy");
            SetLayer(layers, GameLayers.EnemyProjectile, "EnemyProjectile");
            SetLayer(layers, GameLayers.Pickup, "Pickup");

            tagManager.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
        }

        private static void AddTag(SerializedProperty tags, string tag)
        {
            if (tags == null)
            {
                return;
            }

            for (int i = 0; i < tags.arraySize; i++)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    return;
                }
            }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        }

        private static void SetLayer(SerializedProperty layers, int index, string name)
        {
            if (layers == null || index < 0 || index >= layers.arraySize)
            {
                return;
            }

            layers.GetArrayElementAtIndex(index).stringValue = name;
        }

        // ------------------------------------------------------------------
        // Input
        // ------------------------------------------------------------------

        private static InputActionAsset CreateInputAsset()
        {
            InputActionAsset asset = ScriptableObject.CreateInstance<InputActionAsset>();
            asset.name = "GameInput";

            InputActionMap player = asset.AddActionMap("Player");

            InputAction move = player.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            move.AddBinding("<Gamepad>/leftStick");

            InputAction fire = player.AddAction("Fire", InputActionType.Button);
            fire.AddBinding("<Keyboard>/space");
            fire.AddBinding("<Gamepad>/buttonSouth");

            InputAction bomb = player.AddAction("Bomb", InputActionType.Button);
            bomb.AddBinding("<Keyboard>/x");
            bomb.AddBinding("<Gamepad>/buttonWest");

            InputAction pause = player.AddAction("Pause", InputActionType.Button);
            pause.AddBinding("<Keyboard>/escape");
            pause.AddBinding("<Gamepad>/start");

            File.WriteAllText(InputAssetPath, asset.ToJson());
            UnityEngine.Object.DestroyImmediate(asset);
            AssetDatabase.ImportAsset(InputAssetPath, ImportAssetOptions.ForceUpdate);

            return AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputAssetPath);
        }

        // ------------------------------------------------------------------
        // Prefabs
        // ------------------------------------------------------------------

        private static GameObject CreatePlayerPrefab(Sprite[] shipFrames, InputActionAsset input, WeaponDataSO[] weapons)
        {
            GameObject go = new GameObject("Player");
            go.layer = GameLayers.Player;
            go.tag = "Player";

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = shipFrames[0];
            renderer.sortingOrder = 10;

            SpriteAnimator animator = go.AddComponent<SpriteAnimator>();
            animator.target = renderer;
            animator.frames = shipFrames;
            animator.framesPerSecond = 7f;
            animator.randomizeStart = false;

            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CircleCollider2D collider = go.AddComponent<CircleCollider2D>();
            collider.radius = 0.28f;
            collider.isTrigger = true;

            PlayerInputReader reader = go.AddComponent<PlayerInputReader>();
            reader.actions = input;

            PlayerShip ship = go.AddComponent<PlayerShip>();
            ship.spriteRenderer = renderer;
            ship.body = body;
            ship.input = reader;

            PlayerWeapon weapon = go.AddComponent<PlayerWeapon>();
            weapon.input = reader;
            weapon.weaponLevels = weapons;
            ship.weapon = weapon;

            BombSystem bomb = go.AddComponent<BombSystem>();
            bomb.input = reader;
            ship.bombSystem = bomb;

            PauseController pause = go.AddComponent<PauseController>();
            pause.input = reader;

            return SavePrefab(go, PlayerPrefabPath);
        }

        private static GameObject CreateChickenPrefab(string variant, Sprite[] frames)
        {
            GameObject go = new GameObject($"Chicken_{variant}");
            go.layer = GameLayers.Enemy;
            SetTagSafe(go, "Enemy");

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = frames[0];
            renderer.sortingOrder = 5;

            SpriteAnimator animator = go.AddComponent<SpriteAnimator>();
            animator.target = renderer;
            animator.frames = frames;
            animator.framesPerSecond = 4.5f;
            animator.randomizeStart = true;

            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;

            BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.7f, 0.7f);
            collider.isTrigger = true;

            EnemyUnit unit = go.AddComponent<EnemyUnit>();
            unit.spriteRenderer = renderer;

            go.AddComponent<ChickenBrain>();

            return SavePrefab(go, string.Format(ChickenPrefabPath, variant));
        }

        private static GameObject CreateAsteroidPrefab(ArtLibrary.SpriteSet sprites)
        {
            GameObject go = new GameObject("Asteroid");
            go.layer = GameLayers.Enemy;
            SetTagSafe(go, "Enemy");

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprites.Asteroid[0];
            renderer.sortingOrder = 4;

            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;

            CircleCollider2D collider = go.AddComponent<CircleCollider2D>();
            collider.radius = 0.55f;
            collider.isTrigger = true;

            EnemyUnit unit = go.AddComponent<EnemyUnit>();
            unit.spriteRenderer = renderer;

            Asteroid asteroid = go.AddComponent<Asteroid>();
            asteroid.unit = unit;
            asteroid.variants = sprites.Asteroid;

            return SavePrefab(go, AsteroidPrefabPath);
        }

        private static GameObject CreateEggPrefab(Sprite sprite)
        {
            GameObject go = new GameObject("Egg");
            go.layer = GameLayers.EnemyProjectile;

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 20;

            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            CircleCollider2D collider = go.AddComponent<CircleCollider2D>();
            collider.radius = 0.13f;
            collider.isTrigger = true;

            EggProjectile egg = go.AddComponent<EggProjectile>();
            egg.body = body;

            return SavePrefab(go, EggPrefabPath);
        }

        private static GameObject CreateBulletPrefab(Sprite sprite)
        {
            GameObject go = new GameObject("Bullet");
            go.layer = GameLayers.PlayerProjectile;

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 21;

            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.12f, 0.5f);
            collider.isTrigger = true;

            Projectile projectile = go.AddComponent<Projectile>();
            projectile.body = body;

            return SavePrefab(go, BulletPrefabPath);
        }

        private static GameObject CreatePickupPrefab(string name, Sprite sprite, PickupType type)
        {            GameObject go = new GameObject(name);
            go.layer = GameLayers.Pickup;

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 8;

            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            CircleCollider2D collider = go.AddComponent<CircleCollider2D>();
            collider.radius = 0.24f;
            collider.isTrigger = true;

            Pickup pickup = go.AddComponent<Pickup>();
            pickup.type = type;
            pickup.scoreValue = type == PickupType.Gift ? 40 : 20;

            string path = type == PickupType.Gift ? GiftPrefabPath : FoodPrefabPath;
            return SavePrefab(go, path);
        }

        private static GameObject SavePrefab(GameObject temporary, string path)
        {
            EnsureFolder(Path.GetDirectoryName(path)?.Replace('\\', '/'));
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temporary, path);
            UnityEngine.Object.DestroyImmediate(temporary);
            return prefab;
        }

        private static void SetTagSafe(GameObject go, string tag)
        {
            try
            {
                go.tag = tag;
            }
            catch
            {
                // The tag is optional for gameplay; ignore a missing tag entry.
            }
        }

        // ------------------------------------------------------------------
        // Visual effects
        // ------------------------------------------------------------------

        private static void CreateVfxPrefabs(ArtLibrary.SpriteSet sprites, out GameObject explosion,
            out GameObject smallExplosion, out GameObject muzzle, out GameObject shockwave,
            out GameObject sparkle, out GameObject damage)
        {
            explosion = CreateExplosionPrefab("Explosion", sprites.Explosion, 0.55f, 30);
            smallExplosion = CreateExplosionPrefab("ExplosionSmall", sprites.ExplosionSmall, 0.35f, 29);
            muzzle = CreateBurstPrefab("Muzzle", sprites.Muzzle, 22, 0.1f, 0.45f, 1.15f, 0f, Color.white);
            shockwave = CreateBurstPrefab("Shockwave", sprites.Shockwave, 25, 0.55f, 0.2f, 3.2f, 40f, new Color(0.65f, 0.95f, 1f, 0.9f));
            sparkle = CreateBurstPrefab("Sparkle", sprites.Sparkle, 26, 0.38f, 0.35f, 1.25f, 90f, Color.white);
            damage = CreateBurstPrefab("DamageBurst", sprites.Muzzle, 26, 0.3f, 0.5f, 1.9f, 0f, new Color(1f, 0.35f, 0.35f, 0.9f));
        }

        private static GameObject CreateExplosionPrefab(string name, Sprite[] frames, float lifetime, int order)
        {
            GameObject go = new GameObject(name);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = frames != null && frames.Length > 0 ? frames[0] : null;
            renderer.sortingOrder = order;

            ExplosionEffect effect = go.AddComponent<ExplosionEffect>();
            effect.target = renderer;
            effect.frames = frames;
            effect.lifetime = lifetime;
            effect.framesPerSecond = frames != null ? frames.Length / lifetime : 18f;
            effect.spin = true;

            return SavePrefab(go, $"{VfxDir}/{name}.prefab");
        }

        private static GameObject CreateBurstPrefab(string name, Sprite sprite, int order,
            float lifetime, float startScale, float endScale, float spin, Color color)
        {
            GameObject go = new GameObject(name);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            renderer.color = color;

            AutoFade fade = go.AddComponent<AutoFade>();
            fade.target = renderer;
            fade.lifetime = lifetime;
            fade.startScale = startScale;
            fade.endScale = endScale;
            fade.spinSpeed = spin;

            return SavePrefab(go, $"{VfxDir}/{name}.prefab");
        }

        // ------------------------------------------------------------------
        // ScriptableObject data
        // ------------------------------------------------------------------

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                EnsureFolder(Path.GetDirectoryName(path)?.Replace('\\', '/'));
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }

            return asset;
        }

        private static WeaponDataSO[] CreateWeapons(GameObject bulletPrefab)
        {
            WeaponDataSO level1 = LoadOrCreate<WeaponDataSO>(WeaponDir + "/Weapon_Level1.asset");
            level1.displayName = "Single Laser";
            level1.powerLevel = 1;
            level1.projectilePrefab = bulletPrefab;
            level1.damage = 1;
            level1.projectileSpeed = 15f;
            level1.fireInterval = 0.22f;
            level1.pattern = FirePattern.Single;
            level1.projectileCount = 1;
            level1.spreadAngle = 0f;
            level1.muzzleOffsets = new[] { new Vector2(0f, 0.55f) };
            EditorUtility.SetDirty(level1);

            WeaponDataSO level2 = LoadOrCreate<WeaponDataSO>(WeaponDir + "/Weapon_Level2.asset");
            level2.displayName = "Twin Laser";
            level2.powerLevel = 2;
            level2.projectilePrefab = bulletPrefab;
            level2.damage = 1;
            level2.projectileSpeed = 15f;
            level2.fireInterval = 0.23f;
            level2.pattern = FirePattern.Parallel;
            level2.projectileCount = 2;
            level2.spreadAngle = 0f;
            level2.muzzleOffsets = new[] { new Vector2(-0.22f, 0.52f), new Vector2(0.22f, 0.52f) };
            EditorUtility.SetDirty(level2);

            WeaponDataSO level3 = LoadOrCreate<WeaponDataSO>(WeaponDir + "/Weapon_Level3.asset");
            level3.displayName = "Spread Laser";
            level3.powerLevel = 3;
            level3.projectilePrefab = bulletPrefab;
            level3.damage = 1;
            level3.projectileSpeed = 14f;
            level3.fireInterval = 0.25f;
            level3.pattern = FirePattern.Spread;
            level3.projectileCount = 3;
            level3.spreadAngle = 13f;
            level3.muzzleOffsets = new[]
            {
                new Vector2(-0.3f, 0.48f),
                new Vector2(0f, 0.56f),
                new Vector2(0.3f, 0.48f)
            };
            EditorUtility.SetDirty(level3);

            return new[] { level1, level2, level3 };
        }

        private static EnemyDataSO CreateEnemyData(
            string assetName, string displayName, Sprite sprite,
            int health, int score, float moveSpeed,
            Vector2 eggInterval, float eggSpeed, float diveChance, float diveSpeed,
            float foodChance, float giftChance,
            GameObject eggPrefab, GameObject foodPrefab, GameObject giftPrefab)
        {
            EnemyDataSO data = LoadOrCreate<EnemyDataSO>($"{EnemyDir}/Enemy_{assetName}.asset");
            data.displayName = displayName;
            data.sprite = sprite;
            data.tint = Color.white;
            data.maxHealth = health;
            data.score = score;
            data.moveSpeed = moveSpeed;
            data.eggInterval = eggInterval;
            data.eggSpeed = eggSpeed;
            data.eggDamage = 1;
            data.eggPrefab = eggPrefab;
            data.diveChance = diveChance;
            data.diveInterval = new Vector2(8f, 14f);
            data.diveSpeed = diveSpeed;
            data.foodPrefab = foodPrefab;
            data.giftPrefab = giftPrefab;
            data.foodChance = foodChance;
            data.giftChance = giftChance;
            EditorUtility.SetDirty(data);
            return data;
        }

        private static LevelDataSO[] CreateLevels(
            EnemyDataSO basic, EnemyDataSO poison, EnemyDataSO alien,
            GameObject basicChicken, GameObject poisonChicken, GameObject alienChicken,
            GameObject asteroidPrefab)
        {
            Vector2[] leftPath = { new Vector2(-2.5f, 3.5f), new Vector2(-1f, 1.8f), Vector2.zero };
            Vector2[] rightPath = { new Vector2(2.5f, 3.6f), new Vector2(1f, 1.9f), Vector2.zero };

            WaveDataSO l1Grid = CreateChickenWave("L1_Grid", "Earth Grid", basicChicken, basic,
                15, FormationType.Grid, 3, 5, new Vector2(1.25f, 0.85f), 1.9f,
                0.55f, 1.1f, 0.02f, leftPath, 1.7f, 0.08f, 1f, 0.8f);

            WaveDataSO l1V = CreateChickenWave("L1_V", "Earth V", basicChicken, basic,
                14, FormationType.VShape, 4, 7, new Vector2(1f, 0.7f), 1.7f,
                0.7f, 1.3f, 0.025f, rightPath, 1.55f, 0.075f, 1.05f, 0.9f);

            WaveDataSO l1Dense = CreateChickenWave("L1_Dense", "Earth Dense", basicChicken, basic,
                24, FormationType.DenseGrid, 4, 6, new Vector2(1.05f, 0.75f), 1.9f,
                0.8f, 1.4f, 0.03f, leftPath, 1.45f, 0.07f, 1.1f, 1f);

            WaveDataSO l1Asteroids = CreateAsteroidWave("L1_Asteroids", "Earth Debris", asteroidPrefab,
                6, 3, 1.4f, new Vector2(0.8f, 1.5f), 15);

            LevelDataSO level1 = CreateLevelAsset(
                "Level1", 1, "Earth Orbit", "Level1", LevelTheme.EarthOrbit,
                new Color(0.015f, 0.035f, 0.09f, 1f),
                new[]
                {
                    MakeRound(l1Grid, 1f),
                    MakeRound(l1V, 1.05f),
                    MakeRound(l1Asteroids, 1f),
                    MakeRound(l1Dense, 1.18f),
                    MakeRound(l1Grid, 1.3f)
                });

            Vector2[] l2Left = { new Vector2(-3f, 4f), new Vector2(-1.2f, 2.1f), Vector2.zero };
            Vector2[] l2Right = { new Vector2(3f, 4f), new Vector2(1.2f, 2.1f), Vector2.zero };

            WaveDataSO l2Grid = CreateChickenWave("L2_Grid", "Belt Grid", poisonChicken, poison,
                21, FormationType.Grid, 3, 7, new Vector2(1.1f, 0.78f), 2f,
                0.75f, 1.35f, 0.03f, l2Right, 1.5f, 0.07f, 1.15f, 1.1f);

            WaveDataSO l2V = CreateChickenWave("L2_V", "Belt V", poisonChicken, poison,
                18, FormationType.VShape, 5, 9, new Vector2(0.95f, 0.68f), 1.8f,
                0.9f, 1.5f, 0.035f, l2Left, 1.35f, 0.065f, 1.2f, 1.25f);

            WaveDataSO l2Dense = CreateChickenWave("L2_Dense", "Belt Dense", poisonChicken, poison,
                30, FormationType.DenseGrid, 5, 6, new Vector2(0.98f, 0.72f), 2f,
                1f, 1.6f, 0.04f, l2Right, 1.3f, 0.06f, 1.3f, 1.4f);

            WaveDataSO l2Diamond = CreateChickenWave("L2_Diamond", "Belt Diamond", poisonChicken, poison,
                22, FormationType.Diamond, 5, 7, new Vector2(0.95f, 0.75f), 1.9f,
                0.85f, 1.45f, 0.03f, l2Left, 1.4f, 0.065f, 1.2f, 1.2f);

            WaveDataSO l2Asteroids = CreateAsteroidWave("L2_Asteroids", "Belt Rocks", asteroidPrefab,
                8, 4, 1.6f, new Vector2(0.8f, 1.6f), 20);

            LevelDataSO level2 = CreateLevelAsset(
                "Level2", 2, "Asteroid Belt", "Level2", LevelTheme.AsteroidBelt,
                new Color(0.04f, 0.025f, 0.04f, 1f),
                new[]
                {
                    MakeRound(l2Grid, 1.1f),
                    MakeRound(l2V, 1.2f),
                    MakeRound(l2Asteroids, 1.1f),
                    MakeRound(l2Diamond, 1.25f),
                    MakeRound(l2Dense, 1.35f),
                    MakeRound(l2Asteroids, 1.3f),
                    MakeRound(l2V, 1.5f)
                });

            Vector2[] l3Left = { new Vector2(-3.5f, 4.5f), new Vector2(-1.5f, 2.4f), new Vector2(-0.2f, 0.6f), Vector2.zero };
            Vector2[] l3Right = { new Vector2(3.5f, 4.5f), new Vector2(1.5f, 2.4f), new Vector2(0.2f, 0.6f), Vector2.zero };

            WaveDataSO l3Dense = CreateChickenWave("L3_Dense", "Galaxy Dense", alienChicken, alien,
                36, FormationType.DenseGrid, 5, 8, new Vector2(0.9f, 0.68f), 2.1f,
                1.1f, 1.7f, 0.05f, l3Left, 1.2f, 0.05f, 1.4f, 1.6f);

            WaveDataSO l3V = CreateChickenWave("L3_V", "Galaxy V", alienChicken, alien,
                26, FormationType.VShape, 6, 13, new Vector2(0.85f, 0.62f), 1.9f,
                1.15f, 1.8f, 0.05f, l3Right, 1.15f, 0.05f, 1.5f, 1.7f);

            WaveDataSO l3Arc = CreateChickenWave("L3_Arc", "Galaxy Arc", alienChicken, alien,
                30, FormationType.Arc, 6, 10, new Vector2(0.85f, 0.68f), 1.8f,
                1.2f, 1.7f, 0.05f, l3Left, 1.25f, 0.055f, 1.45f, 1.65f);

            WaveDataSO l3Asteroids = CreateAsteroidWave("L3_Asteroids", "Galaxy Rocks", asteroidPrefab,
                10, 6, 1.8f, new Vector2(0.8f, 1.7f), 25);

            LevelDataSO level3 = CreateLevelAsset(
                "Level3", 3, "Deep Galaxy", "Level3", LevelTheme.DeepGalaxy,
                new Color(0.045f, 0.01f, 0.075f, 1f),
                new[]
                {
                    MakeRound(l3Dense, 1.2f),
                    MakeRound(l3V, 1.3f),
                    MakeRound(l3Arc, 1.35f),
                    MakeRound(l3Asteroids, 1.35f),
                    MakeRound(l3Dense, 1.5f),
                    MakeRound(l3V, 1.6f),
                    MakeRound(l3Arc, 1.7f),
                    MakeRound(l3Asteroids, 1.6f),
                    MakeRound(l3Dense, 1.85f),
                    MakeRound(l3V, 2f)
                });

            return new[] { level1, level2, level3 };
        }

        private static WaveDataSO CreateChickenWave(
            string assetName, string label, GameObject enemyPrefab, EnemyDataSO enemyData,
            int count, FormationType formation, int rows, int columns,
            Vector2 spacing, float centerY, float swayAmplitude, float swaySpeed,
            float descentSpeed, Vector2[] waypoints, float entryDuration, float entryStagger,
            float fireRateMultiplier, float diveChanceMultiplier)
        {
            WaveDataSO wave = LoadOrCreate<WaveDataSO>($"{WaveDir}/{assetName}.asset");
            wave.waveName = label;
            wave.kind = WaveKind.ChickenFormation;
            wave.enemyPrefab = enemyPrefab;
            wave.enemyData = enemyData;
            wave.count = count;
            wave.formation = formation;
            wave.rows = rows;
            wave.columns = columns;
            wave.spacing = spacing;
            wave.formationCenter = new Vector2(0f, centerY);
            wave.swayAmplitude = swayAmplitude;
            wave.swaySpeed = swaySpeed;
            wave.descentSpeed = descentSpeed;
            wave.fireRateMultiplier = fireRateMultiplier;
            wave.diveChanceMultiplier = diveChanceMultiplier;

            if (wave.entryPath == null)
            {
                wave.entryPath = new EntryPathConfig();
            }

            wave.entryPath.waypoints = waypoints;
            wave.entryPath.duration = entryDuration;
            wave.entryPath.stagger = entryStagger;
            wave.entryPath.spreadX = 0.45f;

            EditorUtility.SetDirty(wave);
            return wave;
        }

        private static WaveDataSO CreateAsteroidWave(
            string assetName, string label, GameObject asteroidPrefab,
            int count, int health, float fallSpeed, Vector2 sizeRange, int score)
        {
            WaveDataSO wave = LoadOrCreate<WaveDataSO>($"{WaveDir}/{assetName}.asset");
            wave.waveName = label;
            wave.kind = WaveKind.AsteroidField;
            wave.asteroidPrefab = asteroidPrefab;
            wave.asteroidCount = count;
            wave.asteroidHealth = health;
            wave.asteroidFallSpeed = fallSpeed;
            wave.asteroidSizeRange = sizeRange;
            wave.asteroidScore = score;
            EditorUtility.SetDirty(wave);
            return wave;
        }

        private static RoundDefinition MakeRound(WaveDataSO wave, float difficulty)
        {
            return new RoundDefinition { wave = wave, difficulty = difficulty };
        }

        private static LevelDataSO CreateLevelAsset(
            string assetName, int number, string displayName, string sceneName,
            LevelTheme theme, Color color, RoundDefinition[] rounds)
        {
            LevelDataSO level = LoadOrCreate<LevelDataSO>($"{LevelDir}/{assetName}.asset");
            level.levelNumber = number;
            level.displayName = displayName;
            level.sceneName = sceneName;
            level.theme = theme;
            level.themeColor = color;
            level.rounds = rounds;
            EditorUtility.SetDirty(level);
            return level;
        }

        private static LevelDatabaseSO CreateDatabase(LevelDataSO[] levels)
        {
            LevelDatabaseSO database = LoadOrCreate<LevelDatabaseSO>(DatabasePath);
            database.levels = levels;
            EditorUtility.SetDirty(database);
            return database;
        }

        private static void CreateGameManagerPrefab(LevelDatabaseSO database, ArtLibrary.SpriteSet sprites)
        {
            CreateVfxPrefabs(sprites, out GameObject explosion, out GameObject smallExplosion,
                out GameObject muzzle, out GameObject shockwave, out GameObject sparkle, out GameObject damage);

            GameObject go = new GameObject("GameManager");
            GameManager manager = go.AddComponent<GameManager>();
            manager.database = database;
            go.AddComponent<PoolManager>();

            VfxManager vfx = go.AddComponent<VfxManager>();
            vfx.explosionPrefab = explosion;
            vfx.smallExplosionPrefab = smallExplosion;
            vfx.muzzlePrefab = muzzle;
            vfx.shockwavePrefab = shockwave;
            vfx.sparklePrefab = sparkle;
            vfx.damagePrefab = damage;

            SavePrefab(go, GameManagerPrefabPath);
        }

        // ------------------------------------------------------------------
        // Camera, post processing and backgrounds
        // ------------------------------------------------------------------

        private static Camera CreateCamera(string name, Color background)
        {
            GameObject go = new GameObject(name);
            go.tag = "MainCamera";

            Camera camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.4f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = background;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            go.transform.position = new Vector3(0f, 0f, -10f);

            UniversalAdditionalCameraData data = go.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.None;
            data.renderShadows = false;

            go.AddComponent<AudioListener>();
            go.AddComponent<CameraShake>();
            return camera;
        }

        private static Volume CreatePostProcessing()
        {
            GameObject go = new GameObject("PostFX");
            Volume volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;
            volume.sharedProfile = LoadOrCreateVolumeProfile();
            return volume;
        }

        private static VolumeProfile LoadOrCreateVolumeProfile()
        {
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
            if (profile != null)
            {
                return profile;
            }

            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, VolumeProfilePath);

            Bloom bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(0.85f);
            bloom.threshold.Override(0.95f);
            bloom.scatter.Override(0.62f);
            bloom.tint.Override(new Color(1f, 0.96f, 0.92f));
            AssetDatabase.AddObjectToAsset(bloom, profile);

            Vignette vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.22f);
            vignette.smoothness.Override(0.5f);
            vignette.color.Override(new Color(0.02f, 0.03f, 0.08f));
            AssetDatabase.AddObjectToAsset(vignette, profile);

            ColorAdjustments adjustments = profile.Add<ColorAdjustments>(true);
            adjustments.saturation.Override(8f);
            adjustments.contrast.Override(4f);
            AssetDatabase.AddObjectToAsset(adjustments, profile);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        private static void CreateBackground(int theme, ArtLibrary.SpriteSet sprites)
        {
            GameObject root = new GameObject("Background");
            CreateParallaxLayer(root.transform, "BG_Far", sprites.BackgroundFar[theme], -100, 0.05f, 0.004f, 1f);
            CreateParallaxLayer(root.transform, "BG_Mid", sprites.BackgroundMid[theme], -90, 0.13f, 0.01f, 1f);
            CreateParallaxLayer(root.transform, "BG_Near", sprites.BackgroundNear[theme], -80, 0.26f, 0.018f, 0.92f);
        }

        private static void CreateParallaxLayer(
            Transform parent, string name, Sprite sprite, int order,
            float speed, float horizontalFactor, float tint)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            renderer.color = new Color(tint, tint, tint, 1f);

            ParallaxLayer parallax = go.AddComponent<ParallaxLayer>();
            parallax.scrollSpeed = speed;
            parallax.horizontalFactor = horizontalFactor;
            parallax.tileHeight = ArtLibrary.BackgroundHeight / ArtLibrary.BackgroundPixelsPerUnit;
        }

        // ------------------------------------------------------------------
        // Scenes
        // ------------------------------------------------------------------

        private static void CreateMainMenuScene(ArtLibrary.SpriteSet sprites)
        {
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = BeginScene(SceneDir + "/MainMenu.unity");

            Camera camera = CreateCamera("Main Camera", new Color(0.01f, 0.02f, 0.06f, 1f));
            camera.transform.position = new Vector3(0f, 0f, -10f);

            CreateBackground(0, sprites);
            CreatePostProcessing();
            CreateDriftingSprite("MenuShip", sprites.Ship[0], -1.2f, 0.9f, 0.34f, 1.6f, 2.2f);
            CreateDriftingSprite("MenuChicken", sprites.ChickenBasic[0], 1.3f, -1.6f, 0.3f, 2.2f, 1.8f);

            Canvas canvas = CreateCanvas("MainMenuCanvas", 10);
            BuildMainMenu(canvas.transform, sprites);

            CreateEventSystem();

            GameObject controllerGo = new GameObject("MainMenuController");
            MainMenuController controller = controllerGo.AddComponent<MainMenuController>();
            controller.newGameButton = FindButton(canvas.transform, "NewGameButton");
            controller.quitButton = FindButton(canvas.transform, "QuitButton");
            controller.uiModule = UnityEngine.Object.FindAnyObjectByType<InputSystemUIInputModule>();

            EditorSceneManager.MarkSceneDirty(scene);
            EndScene(scene, SceneDir + "/MainMenu.unity", previous);
        }

        private static void CreateLevelScene(string sceneName, LevelDataSO level, GameObject playerPrefab, ArtLibrary.SpriteSet sprites)
        {
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = BeginScene($"{SceneDir}/{sceneName}.unity");

            Camera camera = CreateCamera("Main Camera", level != null ? level.themeColor : Color.black);
            int theme = level != null ? (int)level.theme : 0;

            CreateBackground(theme, sprites);
            CreatePostProcessing();

            GameObject player = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
            if (player != null)
            {
                player.transform.position = new Vector3(0f, -4.3f, 0f);
            }

            LevelBanner banner = CreateBanner();

            CreateEventSystem();

            GameObject controllerGo = new GameObject("LevelController");
            LevelController controller = controllerGo.AddComponent<LevelController>();
            controller.level = level;
            controller.mainCamera = camera;
            controller.banner = banner;
            controller.fallbackLevelIndex = Mathf.Max(0, sceneName switch
            {
                "Level1" => 0,
                "Level2" => 1,
                "Level3" => 2,
                _ => 0
            });

            EditorSceneManager.MarkSceneDirty(scene);
            EndScene(scene, $"{SceneDir}/{sceneName}.unity", previous);

            Debug.Log($"[ChickenShooter] Built {sceneName} (theme={theme}).");
        }

        private static void CreateDriftingSprite(string name, Sprite sprite, float startX, float y,
            float speed, float bobAmplitude, float bobSpeed)
        {
            GameObject go = new GameObject(name);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 2;
            renderer.color = new Color(1f, 1f, 1f, 0.55f);
            go.transform.position = new Vector3(startX, y, 0f);
            go.transform.localScale = Vector3.one * 1.5f;

            DriftingSprite drift = go.AddComponent<DriftingSprite>();
            drift.speed = speed;
            drift.bobAmplitude = bobAmplitude;
            drift.bobSpeed = bobSpeed;
        }

        private static void EndScene(Scene scene, string path, Scene previous)
        {
            EditorSceneManager.SaveScene(scene, path);

            if (previous.IsValid() && previous != scene)
            {
                SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static Scene BeginScene(string path)
        {
            Scene existing = SceneManager.GetSceneByPath(path);
            if (existing.IsValid() && existing.isLoaded)
            {
                SceneManager.SetActiveScene(existing);
                foreach (GameObject root in existing.GetRootGameObjects())
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }

                return existing;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            return scene;
        }

        private static void CloseOrphanScenes()
        {
            for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && string.IsNullOrEmpty(scene.path))
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static void SetBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(SceneDir + "/MainMenu.unity", true),
                new EditorBuildSettingsScene(SceneDir + "/Level1.unity", true),
                new EditorBuildSettingsScene(SceneDir + "/Level2.unity", true),
                new EditorBuildSettingsScene(SceneDir + "/Level3.unity", true)
            };
        }
    }
}
