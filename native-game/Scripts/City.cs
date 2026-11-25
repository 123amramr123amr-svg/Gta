using Godot;
using System;
using System.Collections.Generic;

// Full 3D Third-Person Android Edition (Jason & Lucia, 3D Vehicles, Story NPCs, Over-The-Shoulder Camera)
public partial class City : Node3D
{
    private CharacterBody3D player = null!;
    private Node3D jasonMesh = null!;
    private Node3D luciaMesh = null!;
    private bool isJason = true;

    private List<Node3D> cars = new();
    private Node3D? activeCar = null;
    private Camera3D camera = null!;
    private Label status = null!;
    private Label hint = null!;

    private bool driving, brake;
    private float speed, heading;
    private float camYaw = Mathf.Pi;
    private float camPitch = 0.22f; // True 3rd-person behind-shoulder angle (not top-down)
    private int camMode = 0;

    private Vector2 touchMove;
    private int joystickFinger = -1;
    private int lookFinger = -1;
    private Vector2 joystickOrigin;
    private Control joystick = null!;
    private const float Limit = 140f;

    public override void _Ready()
    {
        GD.Print("CITY_START_ENV");
        BuildEnvironment();
        GD.Print("CITY_START_GEOMETRY");
        BuildCity();
        GD.Print("CITY_START_PLAYER");
        BuildPlayer();
        BuildStoryNpcs();
        GD.Print("CITY_START_CAR");
        BuildVehicles();
        GD.Print("CITY_START_UI");
        BuildUi();
        camera = new Camera3D { Current = true, Fov = 60, Far = 500 };
        AddChild(camera);
        camera.GlobalPosition = new Vector3(0, 2.2f, 13);
        GD.Print("COASTAL_CITY_READY");
    }

    private void BuildEnvironment()
    {
        var env = new Godot.Environment
        {
            BackgroundMode = Godot.Environment.BGMode.Color,
            BackgroundColor = new Color("1a0c30"),
            AmbientLightSource = Godot.Environment.AmbientSource.Color,
            AmbientLightColor = new Color("ff9e80"),
            AmbientLightEnergy = 0.85f,
            TonemapMode = Godot.Environment.ToneMapper.Filmic
        };
        AddChild(new WorldEnvironment { Environment = env });
        var sun = new DirectionalLight3D
        {
            LightColor = new Color("ff7a3d"),
            LightEnergy = 1.4f,
            ShadowEnabled = true,
            RotationDegrees = new Vector3(-35, -35, 0)
        };
        AddChild(sun);
    }

    private static StandardMaterial3D Mat(Color c, float roughness = 0.65f, float metallic = 0.1f) =>
        new() { AlbedoColor = c, Roughness = roughness, Metallic = metallic };

    private MeshInstance3D Box(Node parent, Vector3 size, Vector3 pos, Color color, float metallic = 0f)
    {
        var mesh = new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = size },
            Position = pos,
            MaterialOverride = Mat(color, metallic > 0 ? 0.25f : 0.7f, metallic)
        };
        parent.AddChild(mesh);
        return mesh;
    }

    private void StaticBox(Vector3 size, Vector3 pos, Color color)
    {
        var body = new StaticBody3D { Position = pos };
        AddChild(body);
        Box(body, size, Vector3.Zero, color);
        body.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = size } });
    }

    private Node3D CreateHumanoid3D(Color topColor, Color skinColor, Color pantsColor, Color hairColor)
    {
        var g = new Node3D();
        Box(g, new Vector3(.56f, .72f, .32f), new Vector3(0, 1.18f, 0), topColor);
        Box(g, new Vector3(.54f, .22f, .30f), new Vector3(0, .74f, 0), pantsColor);
        Box(g, new Vector3(.38f, .42f, .38f), new Vector3(0, 1.82f, 0), skinColor);
        Box(g, new Vector3(.40f, .14f, .40f), new Vector3(0, 2.04f, 0), hairColor);
        // Sunglasses
        Box(g, new Vector3(.36f, .08f, .05f), new Vector3(0, 1.86f, -.19f), new Color("111118"), 0.8f);
        // Arms & Legs
        Box(g, new Vector3(.16f, .65f, .16f), new Vector3(-.38f, 1.15f, 0), skinColor);
        Box(g, new Vector3(.16f, .65f, .16f), new Vector3(.38f, 1.15f, 0), skinColor);
        Box(g, new Vector3(.21f, .68f, .21f), new Vector3(-.15f, .34f, 0), pantsColor);
        Box(g, new Vector3(.21f, .68f, .21f), new Vector3(.15f, .34f, 0), pantsColor);
        return g;
    }

    private void BuildCity()
    {
        StaticBox(new Vector3(300, 0.2f, 300), new Vector3(0, -0.2f, 0), new Color("243428"));
        // Roads
        for (int i = -3; i <= 3; i++)
        {
            Box(this, new Vector3(280, 0.03f, 14), new Vector3(0, -0.07f, i * 38), new Color("1c1e29"));
            Box(this, new Vector3(14, 0.03f, 280), new Vector3(i * 38, -0.06f, 0), new Color("1c1e29"));
        }
        // Vice Beach & Ocean
        Box(this, new Vector3(300, 0.04f, 32), new Vector3(0, -0.05f, -110), new Color("f4d58d"));
        Box(this, new Vector3(300, 0.04f, 60), new Vector3(0, -0.04f, -150), new Color("0096c7"));

        var rng = new Random(62);
        Color[] neonPalette = { new Color("ff2e88"), new Color("00e5ff"), new Color("ffd93d"), new Color("8b5cf6") };
        for (int x = -3; x <= 3; x++) for (int z = -2; z <= 3; z++)
        {
            if (x == 0 && z == 0) continue;
            float height = rng.Next(12, 32);
            float bx = x * 38 + 19, bz = z * 38 + 19;
            StaticBox(new Vector3(16, height, 16), new Vector3(bx, height / 2, bz), new Color("17192c"));
            Color neon = neonPalette[Math.Abs(x + z) % neonPalette.Length];
            Box(this, new Vector3(16.4f, 0.8f, 16.4f), new Vector3(bx, height - 0.6f, bz), neon);
            for (int floor = 3; floor < height - 2; floor += 4)
                Box(this, new Vector3(13f, 1.1f, 16.2f), new Vector3(bx, floor, bz), new Color("fef08a"));
        }
    }

    private void BuildPlayer()
    {
        player = new CharacterBody3D { Position = new Vector3(0, 0, 8), FloorSnapLength = 0.2f };
        AddChild(player);
        player.AddChild(new CollisionShape3D { Shape = new CapsuleShape3D { Radius = .38f, Height = 1.85f }, Position = new Vector3(0, .92f, 0) });

        jasonMesh = CreateHumanoid3D(new Color("f5f5f7"), new Color("e8b896"), new Color("284166"), new Color("3b2618"));
        luciaMesh = CreateHumanoid3D(new Color("ff2e7e"), new Color("d69e72"), new Color("2b3a2e"), new Color("1a110b"));
        luciaMesh.Visible = false;

        player.AddChild(jasonMesh);
        player.AddChild(luciaMesh);
    }

    private void BuildStoryNpcs()
    {
        (Vector3 pos, Color top, Color skin)[] npcs = {
            (new Vector3(-18, 0, -40), new Color("00b4d8"), new Color("d9a17c")), // Brian Heder
            (new Vector3(-24, 0, 22), new Color("ffd93d"), new Color("5c3824")),  // Boobie Ike
            (new Vector3(24, 0, -18), new Color("dc2626"), new Color("d19a72")),  // Raul Bautista
            (new Vector3(32, 0, 32), new Color("1d4ed8"), new Color("c68b63"))    // Captain Mendez
        };
        foreach (var n in npcs)
        {
            var m = CreateHumanoid3D(n.top, n.skin, new Color("1e293b"), new Color("111111"));
            m.Position = n.pos;
            AddChild(m);
        }
    }

    private Node3D CreateVehicleMesh(Vector3 pos, Color bodyColor, bool isSuper)
    {
        var c = new Node3D { Position = pos };
        AddChild(c);
        Box(c, new Vector3(2.1f, .62f, 4.6f), new Vector3(0, .62f, 0), bodyColor, .65f);
        Box(c, new Vector3(1.75f, .52f, 2.1f), new Vector3(0, 1.18f, -.18f), new Color("0f172a"), .85f);
        if (isSuper)
            Box(c, new Vector3(2.1f, .08f, .36f), new Vector3(0, 1.15f, 2.05f), new Color("111111"), .5f);
        foreach (int x in new[] { -1, 1 }) foreach (int z in new[] { -1, 1 })
            Box(c, new Vector3(.32f, .78f, .78f), new Vector3(x * 1.08f, .4f, z * 1.4f), new Color("141418"));
        Box(c, new Vector3(1.7f, .16f, .08f), new Vector3(0, .7f, -2.32f), new Color("fffbeb"));
        Box(c, new Vector3(1.8f, .16f, .08f), new Vector3(0, .72f, 2.32f), new Color("ff1e42"));
        return c;
    }

    private void BuildVehicles()
    {
        cars.Add(CreateVehicleMesh(new Vector3(6, 0, 5), new Color("ff2e55"), true));   // Grotti Cheetah '95
        cars.Add(CreateVehicleMesh(new Vector3(-8, 0, 6), new Color("ff8c00"), false)); // Vapid Dominator
        cars.Add(CreateVehicleMesh(new Vector3(16, 0, -14), new Color("00e5ff"), true));// Coquette D10
        cars.Add(CreateVehicleMesh(new Vector3(-18, 0, -12), new Color("1e3a8a"), false)); // Police Interceptor
    }

    private void BuildUi()
    {
        var layer = new CanvasLayer(); AddChild(layer);
        var root = new Control { AnchorRight = 1, AnchorBottom = 1, MouseFilter = Control.MouseFilterEnum.Ignore };
        layer.AddChild(root);
        status = new Label { Position = new Vector2(20, 16), Text = "GTA VI • LEONIDA 3D (Jason Duval)" };
        status.AddThemeFontSizeOverride("font_size", 22); root.AddChild(status);
        hint = new Label { Position = new Vector2(20, 48), Text = "3rd-Person View • Drag Right Screen to Rotate Camera 360°" };
        hint.AddThemeFontSizeOverride("font_size", 15); root.AddChild(hint);

        joystick = new ColorRect
        {
            Color = new Color(1, 1, 1, .22f),
            AnchorTop = 1, AnchorBottom = 1,
            OffsetLeft = 20, OffsetRight = 175, OffsetTop = -175, OffsetBottom = -20,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        root.AddChild(joystick);

        var enter = new Button
        {
            Text = "CAR (ENTER/EXIT)", AnchorLeft = 1, AnchorRight = 1, AnchorTop = 1, AnchorBottom = 1,
            OffsetLeft = -190, OffsetRight = -20, OffsetTop = -230, OffsetBottom = -175
        };
        enter.Pressed += ToggleCar; root.AddChild(enter);

        var switchBtn = new Button
        {
            Text = "SWITCH JASON/LUCIA", AnchorLeft = 1, AnchorRight = 1, AnchorTop = 1, AnchorBottom = 1,
            OffsetLeft = -190, OffsetRight = -20, OffsetTop = -165, OffsetBottom = -110
        };
        switchBtn.Pressed += SwitchCharacter; root.AddChild(switchBtn);

        var camBtn = new Button
        {
            Text = "CAMERA VIEW", AnchorLeft = 1, AnchorRight = 1, AnchorTop = 1, AnchorBottom = 1,
            OffsetLeft = -190, OffsetRight = -20, OffsetTop = -100, OffsetBottom = -55
        };
        camBtn.Pressed += () => camMode = (camMode + 1) % 3; root.AddChild(camBtn);

        var handbrake = new Button
        {
            Text = "DRIFT / BRAKE", AnchorLeft = 1, AnchorRight = 1, AnchorTop = 1, AnchorBottom = 1,
            OffsetLeft = -190, OffsetRight = -20, OffsetTop = -50, OffsetBottom = -10
        };
        handbrake.ButtonDown += () => brake = true; handbrake.ButtonUp += () => brake = false; root.AddChild(handbrake);
    }

    private void SwitchCharacter()
    {
        if (driving) return;
        isJason = !isJason;
        jasonMesh.Visible = isJason;
        luciaMesh.Visible = !isJason;
        status.Text = isJason ? "GTA VI • LEONIDA 3D (Jason Duval)" : "GTA VI • LEONIDA 3D (Lucia Caminos)";
    }

    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey key && key.Pressed && !key.Echo)
        {
            if (key.Keycode == Key.E) ToggleCar();
            if (key.Keycode == Key.Tab) SwitchCharacter();
            if (key.Keycode == Key.C) camMode = (camMode + 1) % 3;
        }
        if (e is InputEventScreenTouch touch)
        {
            if (touch.Pressed && joystickFinger == -1 && joystick.GetGlobalRect().HasPoint(touch.Position))
            {
                joystickFinger = touch.Index;
                joystickOrigin = touch.Position;
                touchMove = Vector2.Zero;
            }
            else if (touch.Pressed && lookFinger == -1 && touch.Position.X > GetViewport().GetVisibleRect().Size.X * 0.45f)
            {
                lookFinger = touch.Index;
            }
            else if (!touch.Pressed)
            {
                if (touch.Index == joystickFinger) { joystickFinger = -1; touchMove = Vector2.Zero; }
                if (touch.Index == lookFinger) { lookFinger = -1; }
            }
        }
        if (e is InputEventScreenDrag drag)
        {
            if (drag.Index == joystickFinger)
                touchMove = ((drag.Position - joystickOrigin) / 55f).LimitLength(1f);
            else if (drag.Index == lookFinger)
            {
                camYaw -= drag.Relative.X * 0.006f;
                camPitch = Mathf.Clamp(camPitch + drag.Relative.Y * 0.004f, 0.06f, 0.52f);
            }
        }
    }

    private void ToggleCar()
    {
        if (driving && activeCar != null)
        {
            driving = false;
            player.Visible = true;
            player.GlobalPosition = activeCar.GlobalPosition + new Vector3(2.6f, 0, 0);
            player.Velocity = Vector3.Zero;
            speed = 0;
            activeCar = null;
            return;
        }

        Node3D? best = null;
        float bestDist = 999f;
        foreach (var c in cars)
        {
            float d = player.GlobalPosition.DistanceTo(c.GlobalPosition);
            if (d < bestDist) { bestDist = d; best = c; }
        }
        if (best != null && bestDist < 6.5f)
        {
            activeCar = best;
            driving = true;
            player.Visible = false;
            speed = 0;
        }
        else hint.Text = "Walk closer to any 3D vehicle to enter";
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = Math.Min((float)delta, .05f);
        float x = touchMove.X + (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right) ? 1 : 0) - (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left) ? 1 : 0);
        float y = touchMove.Y + (Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down) ? 1 : 0) - (Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up) ? 1 : 0);

        if (driving && activeCar != null)
        {
            speed = Driving.Step(speed, -Math.Clamp(y, -1, 1), dt, brake || Input.IsKeyPressed(Key.Space));
            heading -= Math.Clamp(x, -1, 1) * dt * 1.6f * Mathf.Clamp(Mathf.Abs(speed) / 5f, 0, 1) * (speed < 0 ? -1 : 1);
            activeCar.Rotation = new Vector3(0, heading, 0);
            var forward = new Vector3(-Mathf.Sin(heading), 0, -Mathf.Cos(heading));
            activeCar.Position += forward * speed * dt;
            activeCar.Position = new Vector3(Mathf.Clamp(activeCar.Position.X, -Limit, Limit), 0, Mathf.Clamp(activeCar.Position.Z, -Limit, Limit));
            player.GlobalPosition = activeCar.GlobalPosition;
            camYaw = Mathf.LerpAngle(camYaw, heading, dt * 4f);
            status.Text = $"DRIVING 3D  •  {Mathf.RoundToInt(Math.Abs(speed) * 3.8f)} KM/H";
        }
        else
        {
            var input = new Vector2(x, -y).LimitLength(1);
            if (input.Length() > 0.08f)
            {
                var forward = new Vector3(-Mathf.Sin(camYaw), 0, -Mathf.Cos(camYaw));
                var right = new Vector3(Mathf.Cos(camYaw), 0, -Mathf.Sin(camYaw));
                var moveDir = (forward * input.Y + right * input.X).Normalized();
                player.Velocity = new Vector3(moveDir.X * 6.5f, player.Velocity.Y - 20f * dt, moveDir.Z * 6.5f);
                player.Rotation = new Vector3(0, Mathf.Atan2(-moveDir.X, -moveDir.Z), 0);
            }
            else
            {
                player.Velocity = new Vector3(0, player.Velocity.Y - 20f * dt, 0);
            }
            player.MoveAndSlide();
            player.Position = new Vector3(Mathf.Clamp(player.Position.X, -Limit, Limit), player.Position.Y, Mathf.Clamp(player.Position.Z, -Limit, Limit));
        }

        // True Third-Person Behind-Back / Over-The-Shoulder Camera (NOT top-down!)
        Vector3 target = driving && activeCar != null ? activeCar.GlobalPosition : player.GlobalPosition;
        float dist = camMode == 1 ? 3.2f : camMode == 2 ? 7.8f : (driving ? 6.8f : 4.4f);
        float height = camMode == 1 ? 1.75f : (driving ? 2.35f : 1.95f);
        var backOffset = new Vector3(Mathf.Sin(camYaw) * dist, height + Mathf.Sin(camPitch) * 1.4f, Mathf.Cos(camYaw) * dist);
        var desiredPos = target + backOffset;
        camera.GlobalPosition = camera.GlobalPosition.Lerp(desiredPos, 1f - Mathf.Exp(-10f * dt));
        var lookAhead = target + new Vector3(-Mathf.Sin(camYaw) * 4f, 1.55f - Mathf.Sin(camPitch) * 1.5f, -Mathf.Cos(camYaw) * 4f);
        camera.LookAt(lookAhead, Vector3.Up);
    }
}
