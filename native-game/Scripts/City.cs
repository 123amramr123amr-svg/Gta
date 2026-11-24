using Godot;
using System;

// Small standalone native prototype, not a port of every feature in the web demo.
public partial class City : Node3D
{
    private CharacterBody3D player = null!;
    private Node3D car = null!;
    private Camera3D camera = null!;
    private Label status = null!;
    private Label hint = null!;
    private bool driving, brake;
    private float speed, heading;
    private Vector2 touchMove;
    private int joystickFinger = -1;
    private Vector2 joystickOrigin;
    private Control joystick = null!;
    private const float Limit = 95f;

    public override void _Ready()
    {
        BuildEnvironment();
        BuildCity();
        BuildPlayer();
        BuildCar();
        BuildUi();
        camera = new Camera3D { Current = true, Fov = 58, Far = 350 };
        AddChild(camera);
        camera.GlobalPosition = new Vector3(0, 7, 16);
    }

    private void BuildEnvironment()
    {
        var env = new Godot.Environment
        {
            BackgroundMode = Godot.Environment.BGMode.Color,
            BackgroundColor = new Color(0.55f, 0.73f, 0.82f),
            AmbientLightSource = Godot.Environment.AmbientSource.Color,
            AmbientLightColor = new Color(0.78f, 0.84f, 0.91f),
            AmbientLightEnergy = 0.75f,
            TonemapMode = Godot.Environment.ToneMapper.Filmic
        };
        AddChild(new WorldEnvironment { Environment = env });
        var sun = new DirectionalLight3D { LightEnergy = 1.1f, ShadowEnabled = true, RotationDegrees = new Vector3(-48, -30, 0) };
        AddChild(sun);
    }

    private static StandardMaterial3D Mat(Color c, float roughness = 0.8f, float metallic = 0f) =>
        new() { AlbedoColor = c, Roughness = roughness, Metallic = metallic };

    private MeshInstance3D Box(Node parent, Vector3 size, Vector3 pos, Color color, float metallic = 0f)
    {
        var mesh = new MeshInstance3D { Mesh = new BoxMesh { Size = size }, Position = pos,
            MaterialOverride = Mat(color, metallic > 0 ? 0.35f : 0.85f, metallic) };
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

    private void BuildCity()
    {
        StaticBox(new Vector3(210, 0.2f, 210), new Vector3(0, -0.2f, 0), new Color("758469"));
        Box(this, new Vector3(210, 0.03f, 21), new Vector3(0, -0.07f, 0), new Color("333b40"));
        Box(this, new Vector3(21, 0.03f, 210), new Vector3(0, -0.06f, 0), new Color("333b40"));
        Box(this, new Vector3(210, 0.04f, 22), new Vector3(0, -0.05f, -88), new Color("d3bf91"));
        Box(this, new Vector3(210, 0.04f, 20), new Vector3(0, -0.04f, -108), new Color("397e90"));
        for (int i = -9; i <= 9; i++)
        {
            if (Math.Abs(i) < 2) continue;
            Box(this, new Vector3(4, 0.04f, 0.15f), new Vector3(i * 10, -0.03f, 0), new Color("e7d9aa"));
            Box(this, new Vector3(0.15f, 0.04f, 4), new Vector3(0, -0.03f, i * 10), new Color("e7d9aa"));
        }
        var rng = new Random(62);
        for (int x = -3; x <= 3; x++) for (int z = -2; z <= 3; z++)
        {
            if (x == 0 || z == 0) continue;
            float height = rng.Next(8, 23);
            float bx = x * 26, bz = z * 26;
            var color = new Color(0.72f + (float)rng.NextDouble() * 0.15f, 0.70f, 0.62f);
            StaticBox(new Vector3(15, height, 14), new Vector3(bx, height / 2, bz), color);
            for (int floor = 2; floor < height - 2; floor += 3)
                for (int w = -1; w <= 1; w++)
                    Box(this, new Vector3(2.2f, 1.35f, 0.08f), new Vector3(bx + w * 4, floor, bz + 7.05f), new Color("486270"));
        }
        for (int i = -4; i <= 4; i++)
        {
            float x = i * 21;
            var trunk = new MeshInstance3D { Mesh = new CylinderMesh { TopRadius = .26f, BottomRadius = .4f, Height = 6 },
                Position = new Vector3(x, 3, -74), MaterialOverride = Mat(new Color("806449")) };
            AddChild(trunk);
            for (int leaf = 0; leaf < 6; leaf++)
            {
                float angle = leaf * Mathf.Tau / 6f;
                var blade = Box(this, new Vector3(.6f, .12f, 4), new Vector3(x + Mathf.Sin(angle) * 1.5f, 6.1f, -74 + Mathf.Cos(angle) * 1.5f), new Color("397c48"));
                blade.Rotation = new Vector3(.16f, angle, 0);
            }
        }
    }

    private void BuildPlayer()
    {
        player = new CharacterBody3D { Position = new Vector3(0, 0, 8), FloorSnapLength = 0.2f };
        AddChild(player);
        player.AddChild(new CollisionShape3D { Shape = new CapsuleShape3D { Radius = .35f, Height = 1.8f }, Position = new Vector3(0, .9f, 0) });
        Box(player, new Vector3(.7f, .85f, .35f), new Vector3(0, 1.12f, 0), new Color("d8d4c6"));
        Box(player, new Vector3(.45f, .5f, .42f), new Vector3(0, 1.82f, 0), new Color("bc8e6e"));
        Box(player, new Vector3(.28f, .7f, .3f), new Vector3(-.19f, .35f, 0), new Color("364657"));
        Box(player, new Vector3(.28f, .7f, .3f), new Vector3(.19f, .35f, 0), new Color("364657"));
    }

    private void BuildCar()
    {
        car = new Node3D { Position = new Vector3(5, 0, 4) };
        AddChild(car);
        Box(car, new Vector3(2, .65f, 4), new Vector3(0, .65f, 0), new Color("a93f45"), .35f);
        Box(car, new Vector3(1.65f, .55f, 2), new Vector3(0, 1.22f, -.15f), new Color("293d48"), .1f);
        foreach (int x in new[] { -1, 1 }) foreach (int z in new[] { -1, 1 })
            Box(car, new Vector3(.3f, .7f, .8f), new Vector3(x * 1.03f, .4f, z * 1.3f), new Color("202326"));
        Box(car, new Vector3(1.5f, .15f, .06f), new Vector3(0, .7f, -2.03f), new Color("fff1c2"));
    }

    private void BuildUi()
    {
        var layer = new CanvasLayer(); AddChild(layer);
        var root = new Control { AnchorRight = 1, AnchorBottom = 1, MouseFilter = Control.MouseFilterEnum.Ignore };
        layer.AddChild(root);
        status = new Label { Position = new Vector2(20, 18), Text = "COASTAL CITY  •  DEMO" };
        status.AddThemeFontSizeOverride("font_size", 24); root.AddChild(status);
        hint = new Label { Position = new Vector2(20, 52), Text = "WASD / arrows: move   •   E: enter or exit   •   Space: brake" };
        hint.AddThemeFontSizeOverride("font_size", 16); root.AddChild(hint);
        joystick = new ColorRect { Color = new Color(1, 1, 1, .22f), AnchorTop = 1, AnchorBottom = 1,
            OffsetLeft = 20, OffsetRight = 180, OffsetTop = -180, OffsetBottom = -20, MouseFilter = Control.MouseFilterEnum.Ignore };
        root.AddChild(joystick);
        var enter = new Button { Text = "ENTER / EXIT", AnchorLeft = 1, AnchorRight = 1, AnchorTop = 1, AnchorBottom = 1,
            OffsetLeft = -185, OffsetRight = -20, OffsetTop = -180, OffsetBottom = -115 };
        enter.Pressed += ToggleCar; root.AddChild(enter);
        var handbrake = new Button { Text = "BRAKE", AnchorLeft = 1, AnchorRight = 1, AnchorTop = 1, AnchorBottom = 1,
            OffsetLeft = -185, OffsetRight = -20, OffsetTop = -100, OffsetBottom = -25 };
        handbrake.ButtonDown += () => brake = true; handbrake.ButtonUp += () => brake = false; root.AddChild(handbrake);
    }

    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.E) ToggleCar();
        if (e is InputEventScreenTouch touch)
        {
            if (touch.Pressed && joystickFinger == -1 && joystick.GetGlobalRect().HasPoint(touch.Position))
            { joystickFinger = touch.Index; joystickOrigin = touch.Position; touchMove = Vector2.Zero; }
            else if (!touch.Pressed && touch.Index == joystickFinger)
            { joystickFinger = -1; touchMove = Vector2.Zero; }
        }
        if (e is InputEventScreenDrag drag && drag.Index == joystickFinger)
            touchMove = ((drag.Position - joystickOrigin) / 55f).LimitLength(1f);
    }

    private void ToggleCar()
    {
        if (driving)
        {
            if (Math.Abs(speed) > 2f) { hint.Text = "Slow down before leaving the car"; return; }
            driving = false; player.Visible = true;
            player.GlobalPosition = car.GlobalPosition + new Vector3(2.5f, 0, 0);
            player.Velocity = Vector3.Zero; speed = 0;
        }
        else if (player.GlobalPosition.DistanceTo(car.GlobalPosition) < 5f)
        { driving = true; player.Visible = false; speed = 0; }
        else hint.Text = "Walk closer to the car";
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = Math.Min((float)delta, .05f);
        float x = touchMove.X + (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right) ? 1 : 0) - (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left) ? 1 : 0);
        float y = touchMove.Y + (Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down) ? 1 : 0) - (Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up) ? 1 : 0);
        if (driving)
        {
            speed = Driving.Step(speed, -Math.Clamp(y, -1, 1), dt, brake || Input.IsKeyPressed(Key.Space));
            heading -= Math.Clamp(x, -1, 1) * dt * 1.3f * Mathf.Clamp(Mathf.Abs(speed) / 5f, 0, 1) * (speed < 0 ? -1 : 1);
            car.Rotation = new Vector3(0, heading, 0);
            var forward = new Vector3(-Mathf.Sin(heading), 0, -Mathf.Cos(heading));
            car.Position += forward * speed * dt;
            car.Position = new Vector3(Mathf.Clamp(car.Position.X, -Limit, Limit), 0, Mathf.Clamp(car.Position.Z, -Limit, Limit));
            status.Text = $"DRIVING  •  {Mathf.RoundToInt(Mathf.Abs(speed) * 3.6f)} km/h";
        }
        else
        {
            var move = new Vector2(x, y).LimitLength(1);
            var velocity = new Vector3(move.X * 5, player.Velocity.Y - 20f * dt, move.Y * 5);
            player.Velocity = velocity;
            player.MoveAndSlide();
            player.Position = new Vector3(Mathf.Clamp(player.Position.X, -Limit, Limit), player.Position.Y, Mathf.Clamp(player.Position.Z, -Limit, Limit));
            if (move.Length() > .05f) player.Rotation = new Vector3(0, Mathf.Atan2(-move.X, -move.Y), 0);
            status.Text = "COASTAL CITY  •  DEMO";
        }
        Vector3 target = driving ? car.GlobalPosition : player.GlobalPosition;
        var desired = target + new Vector3(0, 7, 12);
        camera.GlobalPosition = camera.GlobalPosition.Lerp(desired, 1f - Mathf.Exp(-5f * dt));
        camera.LookAt(target + Vector3.Up * 1.1f, Vector3.Up);
    }
}
