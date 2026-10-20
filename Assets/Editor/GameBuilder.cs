using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

// Monta o jogo inteiro (tags, prefabs e as 3 cenas) pelo menu "Branded to Kill".
// Tudo que ele faz também pode ser feito à mão no editor; aqui só automatiza.
public static class GameBuilder
{
    const string StudentName = "Vinicius Montes Dib";
    const string Sprites = "Assets/Sprites/";
    const string Audio = "Assets/Audio/";
    const string Prefabs = "Assets/Prefabs/";
    const string Scenes = "Assets/Scenes/";

    // Cores do tema (Tóquio à noite, neon)
    static readonly Color Background = new Color(0.04f, 0.03f, 0.1f);
    static readonly Color Street = new Color(0.1f, 0.08f, 0.17f);
    static readonly Color Building = new Color(0.16f, 0.11f, 0.26f);
    static readonly Color Pink = new Color(1f, 0.16f, 0.54f);
    static readonly Color Cyan = new Color(0.16f, 0.96f, 1f);
    static readonly Color Yellow = new Color(1f, 0.82f, 0.25f);
    static readonly Color Purple = new Color(0.65f, 0.3f, 1f);
    static readonly Color Orange = new Color(1f, 0.55f, 0.2f);

    static GameObject playerPrefab, bulletPrefab, coinPrefab, clockPrefab, patrolPrefab, chaserPrefab;

    [MenuItem("Branded to Kill/Montar jogo")]
    public static void BuildAll()
    {
        CreateTags("Coletavel", "Relogio", "Inimigo", "Parede");
        PlayerSettings.productName = "Branded to Kill";
        PlayerSettings.companyName = "Insper";
        PlayerSettings.defaultWebScreenWidth = 600;  // resolução pedida na entrega
        PlayerSettings.defaultWebScreenHeight = 480;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true; // necessário para funcionar no itch.io
        PlayerSettings.WebGL.template = "APPLICATION:Minimal"; // só o jogo, sem o rodapé do Unity (fica exatamente 600x480)
        AssetDatabase.Refresh();

        BuildPrefabs();
        BuildMenuScene();
        BuildGameScene();
        BuildSecretScene();
        BuildEndScene();

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(Scenes + "MainMenu.unity", true),
            new EditorBuildSettingsScene(Scenes + "Game.unity", true),
            new EditorBuildSettingsScene(Scenes + "FaseSecreta.unity", true),
            new EditorBuildSettingsScene(Scenes + "EndGame.unity", true),
        };
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(Scenes + "MainMenu.unity");
        Debug.Log("[BrandedToKill] Jogo montado.");
    }

    // ================================================================ prefabs (tutorial 13)
    static void BuildPrefabs()
    {
        Directory.CreateDirectory(Prefabs);

        // Bala: círculo pequeno ciano, trigger.
        var bullet = Shape("Bullet", "Circle", Cyan, null, Vector2.zero, Vector2.one * 0.22f, 9);
        var brb = bullet.AddComponent<Rigidbody2D>();
        brb.gravityScale = 0f;
        brb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        bullet.AddComponent<CircleCollider2D>().isTrigger = true;
        bullet.AddComponent<Bullet>().enemyDeathSound = Clip("sfx_enemy_death");
        bulletPrefab = Save(bullet);

        // Moeda de ouro (coletável do tutorial 12, com tag "Coletavel").
        var coin = new GameObject("Coin") { tag = "Coletavel" };
        Renderer(coin, "Coin", Color.white, 5).transform.localScale = Vector3.one;
        coin.transform.localScale = Vector3.one * 1.4f;
        var cc = coin.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius = 0.22f;
        coinPrefab = Save(coin);

        // Relógio: o triângulo do tutorial, roxo, dá +10 segundos.
        var clock = Shape("Clock", "Triangle", Purple, null, Vector2.zero, Vector2.one * 0.6f, 5);
        clock.tag = "Relogio";
        clock.AddComponent<PolygonCollider2D>().isTrigger = true;
        clockPrefab = Save(clock);

        // Inimigos
        patrolPrefab = Save(Enemy("EnemyPatrol", Color.white, true));
        chaserPrefab = Save(Enemy("EnemyChaser", new Color(1f, 0.65f, 0.35f), false));

        // Jogador
        var player = new GameObject("Player") { tag = "Player" };
        Renderer(player, "Player", Color.white, 10);
        player.transform.localScale = Vector3.one * 1.5f;
        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // tutorial 08: sem gravidade, movimento só pelo input
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        player.AddComponent<CircleCollider2D>().radius = 0.35f;
        var audio = player.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        var move = player.AddComponent<PlayerMovement>();
        move.audioSource = audio;
        move.coinSound = Clip("sfx_coin");
        move.clockSound = Clip("sfx_clock");
        move.hurtSound = Clip("sfx_hurt");
        var shoot = player.AddComponent<PlayerShoot>();
        shoot.bulletPrefab = bulletPrefab;
        shoot.audioSource = audio;
        shoot.shootSound = Clip("sfx_shoot_1");
        playerPrefab = Save(player);
    }

    static GameObject Enemy(string name, Color tint, bool patrol)
    {
        var enemy = new GameObject(name) { tag = "Inimigo" };
        Renderer(enemy, "Enemy", tint, 8);
        enemy.transform.localScale = Vector3.one * 1.5f;
        var rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.mass = 5f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        enemy.AddComponent<CircleCollider2D>().radius = 0.38f;
        if (patrol) enemy.AddComponent<EnemyPatrol>();
        else enemy.AddComponent<EnemyChaser>();
        return enemy;
    }

    // ================================================================ cena do jogo
    static void BuildGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Random.InitState(7); // janelas sempre iguais
        Camera(new Vector3(0f, 0.5f), 32.5f, 19.5f);
        GlobalLight();

        // Chão da rua e marcações
        Shape("Rua", "Square", Street, null, Vector2.zero, new Vector2(31f, 17f), -10);
        for (float x = -13f; x <= 13f; x += 2f)
            Shape("Faixa", "Square", new Color(1f, 0.82f, 0.25f, 0.35f), null, new Vector2(x, 1.8f), new Vector2(0.9f, 0.12f), -9);

        // Paredes de fora da arena
        Wall(new Vector2(0f, 8.5f), new Vector2(32f, 1f), Pink);
        Wall(new Vector2(0f, -8.5f), new Vector2(32f, 1f), Pink);
        Wall(new Vector2(-15.5f, 0f), new Vector2(1f, 18f), Pink);
        Wall(new Vector2(15.5f, 0f), new Vector2(1f, 18f), Pink);

        // Prédios de Kabukichō (com contorno neon e janelas)
        BuildingBlock(new Vector2(-9f, 4f), new Vector2(5f, 3f), Pink);
        BuildingBlock(new Vector2(0f, 5f), new Vector2(6f, 2f), Cyan);
        BuildingBlock(new Vector2(9f, 4f), new Vector2(5f, 3f), Yellow);
        BuildingBlock(new Vector2(-9f, -3f), new Vector2(4f, 4f), Cyan);
        BuildingBlock(new Vector2(0f, -1f), new Vector2(4f, 3f), Purple);
        BuildingBlock(new Vector2(9f, -3f), new Vector2(4f, 4f), Pink);

        // Moedas (o objetivo é pegar todas)
        Vector2[] coins =
        {
            new Vector2(-13f, 7f), new Vector2(-4.5f, 7.2f), new Vector2(4.5f, 7.2f), new Vector2(13f, 7f),
            new Vector2(-13f, 0.5f), new Vector2(-5f, 1.8f), new Vector2(5f, 1.8f), new Vector2(13f, 0.5f),
            new Vector2(-5f, -5f), new Vector2(0f, -4f), new Vector2(5f, -5f), new Vector2(13f, -7f),
        };
        foreach (var p in coins) Place(coinPrefab, p);

        // Relógios (+10 s)
        Place(clockPrefab, new Vector2(-13f, -7f));
        Place(clockPrefab, new Vector2(0f, 2.5f));

        // Inimigos: 3 em patrulha e 2 que perseguem
        Patrol(new Vector2(-10f, 7.2f), new Vector2(10f, 7.2f));
        Patrol(new Vector2(-5f, 3f), new Vector2(-5f, -7f));
        Patrol(new Vector2(5f, -7f), new Vector2(5f, 3f));
        Place(chaserPrefab, new Vector2(12.5f, -6f));
        Place(chaserPrefab, new Vector2(-13.5f, 4f));

        // Segredo: bueiro quase invisível no beco da direita -> leva à fase secreta
        var manhole = Shape("Bueiro", "Circle", new Color(0.16f, 0.13f, 0.24f), null, new Vector2(13.3f, -3.5f), Vector2.one * 0.9f, -8);
        Shape("Tampa", "Circle", new Color(0.12f, 0.1f, 0.19f), null, new Vector2(13.3f, -3.5f), Vector2.one * 0.6f, -7);
        manhole.AddComponent<CircleCollider2D>().isTrigger = true;
        manhole.AddComponent<SecretEntrance>();

        // Jogador
        Place(playerPrefab, new Vector2(0f, -7f));

        // GameManager + música de fundo
        var gm = new GameObject("GameManager");
        gm.AddComponent<GameManager>();
        Music(gm, "music_level", 0.35f);

        // HUD
        var canvas = Canvas();
        var hud = canvas.gameObject.AddComponent<HUD>();
        hud.scoreText = Label(canvas.transform, "PONTOS 0", 28, Yellow, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(20, -10), new Vector2(320, 36));
        hud.coinsText = Label(canvas.transform, "MOEDAS 0 / 12", 28, Yellow, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(20, -44), new Vector2(320, 36));
        hud.timeText = Label(canvas.transform, "01:30", 48, Color.white, TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0, -8), new Vector2(240, 56));
        hud.livesText = Label(canvas.transform, "VIDAS 3", 28, Pink, TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(-20, -10), new Vector2(240, 36));
        EventSystem();

        EditorSceneManager.SaveScene(scene, Scenes + "Game.unity");
    }

    static void BuildingBlock(Vector2 center, Vector2 size, Color neon)
    {
        Shape("Neon", "Square", neon, null, center, size + Vector2.one * 0.25f, -2);
        var b = Shape("Predio", "Square", Building, null, center, size, -1);
        b.tag = "Parede";
        b.AddComponent<BoxCollider2D>();
        // janelas acesas (só decoração)
        for (float x = -size.x / 2f + 0.6f; x < size.x / 2f - 0.3f; x += 1f)
            for (float y = -size.y / 2f + 0.6f; y < size.y / 2f - 0.3f; y += 1f)
                if (Random.value < 0.5f)
                    Shape("Janela", "Square", new Color(1f, 0.85f, 0.5f, 0.5f), b.transform.parent, center + new Vector2(x, y), Vector2.one * 0.35f, 0);
    }

    static void Wall(Vector2 center, Vector2 size, Color neon)
    {
        var w = Shape("Parede", "Square", Building, null, center, size, -1);
        w.tag = "Parede";
        w.AddComponent<BoxCollider2D>();
        Shape("NeonParede", "Square", neon, null, center, size - Vector2.one * 0.8f, 0);
    }

    static void Patrol(Vector2 a, Vector2 b)
    {
        var e = Place(patrolPrefab, a);
        var patrol = e.GetComponent<EnemyPatrol>();
        patrol.pointA = a;
        patrol.pointB = b;
    }

    // ================================================================ fase secreta: o cofre da yakuza
    static void BuildSecretScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Random.InitState(11);
        Camera(new Vector3(0f, 0.5f), 23f, 19f);
        GlobalLight();

        // Sala do cofre: menor, toda em dourado
        Shape("Chao", "Square", new Color(0.13f, 0.1f, 0.05f), null, Vector2.zero, new Vector2(21f, 14f), -10);
        Wall(new Vector2(0f, 7.5f), new Vector2(22f, 1f), Yellow);
        Wall(new Vector2(0f, -7.5f), new Vector2(22f, 1f), Yellow);
        Wall(new Vector2(-11f, 0f), new Vector2(1f, 16f), Yellow);
        Wall(new Vector2(11f, 0f), new Vector2(1f, 16f), Yellow);
        // Pilhas de ouro que servem de obstáculo
        BuildingBlock(new Vector2(-5f, 2f), new Vector2(2f, 2f), Yellow);
        BuildingBlock(new Vector2(5f, 2f), new Vector2(2f, 2f), Yellow);
        BuildingBlock(new Vector2(0f, -2f), new Vector2(3f, 1.5f), Orange);

        // 8 moedas valendo 50 pontos cada
        Vector2[] coins =
        {
            new Vector2(-8.5f, 5.5f), new Vector2(0f, 5.5f), new Vector2(8.5f, 5.5f), new Vector2(-8.5f, -1f),
            new Vector2(8.5f, -1f), new Vector2(-5f, -5.5f), new Vector2(5f, -5.5f), new Vector2(0f, 2f),
        };
        foreach (var p in coins) Place(coinPrefab, p);

        // Guardas do cofre
        Patrol(new Vector2(-8.5f, 4.3f), new Vector2(8.5f, 4.3f));
        Place(chaserPrefab, new Vector2(-8f, -5f));
        Place(chaserPrefab, new Vector2(8f, -5f));

        Place(playerPrefab, new Vector2(0f, -5.5f));

        var gm = new GameObject("GameManager");
        var manager = gm.AddComponent<GameManager>();
        manager.newGame = false;   // continua com pontos, vidas e tempo da fase principal
        manager.coinValue = 50;
        manager.victoryMessage = "Você achou o cofre secreto da yakuza!";
        Music(gm, "music_level", 0.35f);

        var canvas = Canvas();
        var hud = canvas.gameObject.AddComponent<HUD>();
        hud.scoreText = Label(canvas.transform, "PONTOS 0", 28, Yellow, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(20, -10), new Vector2(320, 36));
        hud.coinsText = Label(canvas.transform, "MOEDAS 0 / 8", 28, Yellow, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(20, -44), new Vector2(320, 36));
        hud.timeText = Label(canvas.transform, "01:30", 48, Color.white, TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0, -8), new Vector2(240, 56));
        hud.livesText = Label(canvas.transform, "VIDAS 3", 28, Pink, TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(-20, -10), new Vector2(240, 36));
        Label(canvas.transform, "FASE SECRETA  ·  O COFRE DA YAKUZA", 26, Yellow, TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(0, 22), new Vector2(900, 40));
        EventSystem();
        EditorSceneManager.SaveScene(scene, Scenes + "FaseSecreta.unity");
    }

    // ================================================================ menu (tutorial 16)
    static void BuildMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Camera(Vector3.zero, 12f, 10f);
        GlobalLight();
        // Fundo: silhueta de prédios com neon
        for (int i = 0; i < 9; i++)
        {
            if (i == 4) continue; // deixa o meio livre para os botões
            float x = -8f + i * 2f;
            float h = 2f + (i * 37 % 5);
            Color neon = new[] { Pink, Cyan, Yellow, Purple }[i % 4];
            Shape("Neon", "Square", neon * 0.8f, null, new Vector2(x, -5f + h / 2f), new Vector2(1.8f, h + 0.15f), -2);
            Shape("Predio", "Square", Building, null, new Vector2(x, -5f + h / 2f), new Vector2(1.65f, h), -1);
        }
        var musicHolder = new GameObject("Musica");
        Music(musicHolder, "music_menu", 0.4f);

        var canvas = Canvas();
        var menu = canvas.gameObject.AddComponent<MainMenu>();
        var main = Panel(canvas.transform, "MainPanel");
        menu.mainPanel = main;
        var title = Label(main.transform, "BRANDED TO KILL", 92, Pink, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -130), new Vector2(960, 120));
        title.gameObject.AddComponent<Outline>().effectColor = new Color(0.16f, 0.96f, 1f, 0.6f);
        Label(main.transform, "Tóquio, 2h da manhã. Pegue o ouro da yakuza antes do tempo acabar.", 26, Cyan, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -215), new Vector2(900, 70));
        menu.playButton = Button(main.transform, "JOGAR", new Vector2(0, -10), Cyan);
        menu.howToButton = Button(main.transform, "COMO JOGAR", new Vector2(0, -95), Yellow);
        menu.quitButton = Button(main.transform, "SAIR", new Vector2(0, -180), Pink);
        Label(main.transform, StudentName + "  ·  Insper 2026", 22, new Color(0.7f, 0.7f, 0.85f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(900, 30));

        var how = Panel(canvas.transform, "HowToPanel");
        menu.howToPanel = how;
        Label(how.transform, "COMO JOGAR", 60, Yellow, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -70), new Vector2(900, 80));
        Label(how.transform,
            "Pegue as 12 moedas de ouro antes do tempo acabar.\n" +
            "Encostar num capanga tira 1 vida. Você tem 3 vidas.\n" +
            "Triângulos roxos dão +10 segundos. Cada capanga derrotado dá +3 segundos.\n" +
            "Dizem que um bueiro de Kabukichō leva ao cofre secreto da yakuza...\n\n" +
            "TECLADO: WASD ou setas para andar · ESPAÇO ou clique para atirar\n" +
            "CONTROLE XBOX: analógico esquerdo para andar · A ou RT para atirar",
            25, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(920, 470));
        menu.backButton = Button(how.transform, "VOLTAR", new Vector2(0, -320), Cyan);
        EventSystem();
        EditorSceneManager.SaveScene(scene, Scenes + "MainMenu.unity");
    }

    // ================================================================ fim de jogo (tutorial 17)
    static void BuildEndScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Camera(Vector3.zero, 12f, 10f);
        GlobalLight();
        var holder = new GameObject("Som");
        var audio = holder.AddComponent<AudioSource>();
        audio.playOnAwake = false;

        var canvas = Canvas();
        var end = canvas.gameObject.AddComponent<EndGame>();
        end.audioSource = audio;
        end.victorySound = Clip("sfx_victory");
        end.gameOverSound = Clip("sfx_game_over");
        end.titleText = Label(canvas.transform, "FIM DE JOGO", 88, Pink, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -130), new Vector2(960, 110));
        end.messageText = Label(canvas.transform, "", 30, Cyan, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -235), new Vector2(900, 80));
        end.statsText = Label(canvas.transform, "", 32, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(800, 200));
        end.playAgainButton = Button(canvas.transform, "JOGAR DE NOVO", new Vector2(-200, -290), Cyan);
        end.menuButton = Button(canvas.transform, "MENU", new Vector2(200, -290), Pink);
        EventSystem();
        EditorSceneManager.SaveScene(scene, Scenes + "EndGame.unity");
    }

    // ================================================================ ajudantes
    static void CreateTags(params string[] tags)
    {
        var manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var list = manager.FindProperty("tags");
        foreach (var tag in tags)
        {
            bool exists = false;
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).stringValue == tag) exists = true;
            if (exists) continue;
            list.InsertArrayElementAtIndex(list.arraySize);
            list.GetArrayElementAtIndex(list.arraySize - 1).stringValue = tag;
        }
        manager.ApplyModifiedPropertiesWithoutUndo();
    }

    static Sprite LoadSprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Sprites + name + ".png");
    static AudioClip Clip(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>(Audio + name + ".wav");
    static Font MainFont => AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Rajdhani-Bold.ttf");

    static SpriteRenderer Renderer(GameObject go, string sprite, Color color, int order)
    {
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(sprite);
        sr.color = color;
        sr.sortingOrder = order;
        return sr;
    }

    static GameObject Shape(string name, string sprite, Color color, Transform parent, Vector2 position, Vector2 size, int order)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent);
        go.transform.position = position;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        Renderer(go, sprite, color, order);
        return go;
    }

    static GameObject Save(GameObject go)
    {
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, Prefabs + go.name + ".prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject Place(GameObject prefab, Vector2 position)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.position = position;
        return go;
    }

    static void Camera(Vector3 position, float width, float height)
    {
        var go = new GameObject("Main Camera") { tag = "MainCamera" };
        go.transform.position = new Vector3(position.x, position.y, -10f);
        var cam = go.AddComponent<UnityEngine.Camera>();
        cam.orthographic = true;
        var fit = go.AddComponent<CameraFit>(); // a área inteira cabe em qualquer resolução
        fit.width = width;
        fit.height = height;
        cam.aspect = 600f / 480f;
        fit.Fit();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Background;
        go.AddComponent<AudioListener>();
        go.AddComponent<UniversalAdditionalCameraData>();
    }

    static void GlobalLight()
    {
        var light = new GameObject("Global Light 2D").AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.intensity = 1f;
    }

    static void Music(GameObject holder, string clip, float volume)
    {
        var music = holder.AddComponent<AudioSource>();
        music.clip = Clip(clip);
        music.loop = true;
        music.playOnAwake = true;
        music.volume = volume;
    }

    static Canvas Canvas()
    {
        var go = new GameObject("Canvas");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1000, 800);
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    static void EventSystem()
    {
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<InputSystemUIInputModule>(); // permite navegar no menu com o controle
    }

    static GameObject Panel(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }

    static Text Label(Transform parent, string text, int size, Color color, TextAnchor align, Vector2 anchor, Vector2 position, Vector2 box)
    {
        var go = new GameObject("Texto", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(align == TextAnchor.UpperLeft ? 0f : align == TextAnchor.UpperRight ? 1f : 0.5f, anchor.y);
        rt.anchoredPosition = position;
        rt.sizeDelta = box;
        var t = go.AddComponent<Text>();
        t.text = text;
        t.font = MainFont;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Wrap; // quebra linha se não couber
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    static Button Button(Transform parent, string text, Vector2 position, Color color)
    {
        var go = new GameObject("Botao " + text, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(360, 64);
        var image = go.AddComponent<Image>();
        image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        image.type = Image.Type.Sliced;
        var button = go.AddComponent<Button>();
        var colors = button.colors;
        colors.normalColor = new Color(color.r, color.g, color.b, 0.35f);
        colors.highlightedColor = color;
        colors.selectedColor = color;
        colors.pressedColor = Color.white;
        button.colors = colors;
        Label(go.transform, text, 30, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, rt.sizeDelta);
        return button;
    }

    // ================================================================ build e capturas
    [MenuItem("Branded to Kill/Build WebGL")]
    public static void BuildWebGL()
    {
        PlayerSettings.WebGL.template = "APPLICATION:Minimal";
        PlayerSettings.defaultWebScreenWidth = 600;
        PlayerSettings.defaultWebScreenHeight = 480;
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { Scenes + "MainMenu.unity", Scenes + "Game.unity", Scenes + "FaseSecreta.unity", Scenes + "EndGame.unity" },
            locationPathName = "Builds/WebGL",
            target = BuildTarget.WebGL,
        });
        Debug.Log("[BrandedToKill] Build WebGL: " + report.summary.result);
        if (Application.isBatchMode && report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }

    // Usado pela linha de comando para conferir o visual das cenas.
    public static void BuildAndScreenshots()
    {
        BuildAll();
        Directory.CreateDirectory("Tools/preview");
        foreach (var name in new[] { "MainMenu", "Game", "FaseSecreta", "EndGame" })
        {
            EditorSceneManager.OpenScene(Scenes + name + ".unity");
            var cam = UnityEngine.Camera.main;
            foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                c.renderMode = RenderMode.ScreenSpaceCamera;
                c.worldCamera = cam;
                c.planeDistance = 1f;
                c.sortingOrder = 100;
            }
            var how = GameObject.Find("HowToPanel");
            if (how != null) how.SetActive(false);
            var rt = new RenderTexture(600, 480, 24);
            cam.targetTexture = rt;
            cam.aspect = 600f / 480f;
            cam.GetComponent<CameraFit>().Fit();
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(600, 480, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 600, 480), 0, 0);
            File.WriteAllBytes("Tools/preview/" + name + ".png", tex.EncodeToPNG());
            RenderTexture.active = null;
            cam.targetTexture = null;
        }
    }
}
