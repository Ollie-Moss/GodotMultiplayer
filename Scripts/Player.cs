using Godot;

public partial class Player : CharacterBody2D
{
    // ---------------- JUMP VARIABLES -----------------
    [Export]
    private float JumpVelocity = -550.0f;
    [Export]
    private int TotalJumps = 3;
    private int CurrentJumps;

    // ---------------- DASH VARIABLES -----------------
    [Export]
    private float DashVelocity = 1200.0f;
    [Export]
    private float DashCoolDown = 0.2f;
    private bool CanDash = true;


    // ------------- BASIC MOVEMENT VARIABLES ----------
    [Export]
    private float MoveSpeed = 250f;
    [Export]
    private Vector2 TotalMaxSpeed = new Vector2(500f, 600f);
    private Vector2 CurrentMaxSpeed;

    [Export]
    private Vector2 ScalingCoeffcient = new Vector2(0.8f, 0.95f);

    private float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    // ------------- MULTIPLAYER VARIABLES -------------
    private PlayerInputSynchronizer PlayerInput;

    private int _playerId;
    public int PlayerId
    {
        get => _playerId;
        set
        {
            _playerId = value;
        }
    }

    // ------------- ANIMATION VARIABLES ---------------
    public override void _EnterTree()
    {
        GetTree().Paused = true;
        if (GameManager.Instance.isReady)
        {
            LoadPlayer();
        }
        else
        {
            GameManager.Instance.ClientReady += () => LoadPlayer();
        }
        
    }

    private void LoadPlayer()
    {
        PlayerInput = GetNode<PlayerInputSynchronizer>("PlayerInputSynchronizer");
        PlayerInput.SetMultiplayerAuthority(int.Parse(Name));
        GetNode<Label>("Name").Text = GameManager.Instance.GetPlayerInfo(int.Parse(Name)).Name;
        GD.Print("FROM ID: " + Multiplayer.GetUniqueId() + "SET AUTHORITY TO ID: " + Name);

        GetTree().Paused = false;
        GameManager.Instance.ClientReady -= () => LoadPlayer();
    }

    public override void _Ready()
    {
        // Intialize Variables
        CurrentJumps = TotalJumps;
        CurrentMaxSpeed = TotalMaxSpeed;
    }

    public override void _Process(double delta)
    {
        // get input
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;
        Vector2 direction = PlayerInput.DirectionInput;

        // Add Gravity
        if (!IsOnFloor()) velocity.Y += gravity * (float)delta;

        velocity = HandleJump(velocity);
        velocity = HandleDash(velocity, direction);

        velocity = MoveXAxis(velocity, direction);
        velocity = MoveYAxis(velocity, direction);

        velocity *= ScalingCoeffcient;
        Velocity = velocity;
        MoveAndSlide();
    }



    private Vector2 HandleJump(Vector2 velocity)
    {
        if (IsOnFloor()) CurrentJumps = TotalJumps;

        if (PlayerInput.JumpInput)
        {
            PlayerInput.JumpInput = false;
            if(CurrentJumps > 0)
            {
                velocity.Y = JumpVelocity;

                // Reduce Jumps by 1 if we have fallen off a platform
                if (!IsOnFloor() && CurrentJumps == TotalJumps) CurrentJumps--;

                CurrentJumps--;
            }
            
        }
        return velocity;
    }



    private Vector2 HandleDash(Vector2 velocity, Vector2 direction)
    {
        if(PlayerInput.DashInput)
        {
            PlayerInput.DashInput = false;
            if (CanDash)
            {
                CurrentMaxSpeed.X = DashVelocity;

                // Adjust Jump Velocity
                JumpVelocity = JumpVelocity / 1.25f;

                CanDash = false;

                velocity.X = DashVelocity * direction.X;
                ResetDash();
            }
            
        }
        

        return velocity;
    }
    public async void ResetDash()
    {
        await this.Wait(DashCoolDown * (IsOnFloor() ? 1.25f : 1f));
        CanDash = true;
        JumpVelocity = JumpVelocity * 1.25f;
    }


    // Moves Player in X Axis Accounting For Max Speeds And Adjusting Said Max Speeds
    private Vector2 MoveXAxis(Vector2 velocity, Vector2 direction)
    {
        // Move X Axis Based on MoveSpeed
        velocity.X += direction.X * MoveSpeed * 0.4f;

        // Decelerate X Axis
        if (direction.X == 0 && IsOnFloor()) velocity.X = Mathf.MoveToward(Velocity.X, 0, 100f);
        if (direction.X == 0 && !IsOnFloor()) velocity.X = Mathf.MoveToward(Velocity.X, 0, 10f);

        // Clamp X Axis
        velocity.X = Mathf.Clamp(velocity.X, CurrentMaxSpeed.X * -1, CurrentMaxSpeed.X);

        // Reduce MaxSpeed
        if (CanDash == false) CurrentMaxSpeed.X = Mathf.MoveToward(CurrentMaxSpeed.X, TotalMaxSpeed.X, 50f * (IsOnFloor() ? 1f : 0.01f));

        return velocity;
    }

    // Moves Player in Y Axis Acounting for Max Speeds and Taking Fast Fall Into Account
    private Vector2 MoveYAxis(Vector2 velocity, Vector2 direction)
    {
        // Fast fall
        if (direction.Y > 0) velocity.Y += direction.Y * MoveSpeed * 0.4f;

        // Clamp y axis
        velocity.Y = Mathf.Clamp(velocity.Y, CurrentMaxSpeed.Y * -1, CurrentMaxSpeed.Y);

        return velocity;
    }

    public void SetUpPlayer(PlayerInfo player)
    {
        
        
        //GD.Print("FROM ID: " + Multiplayer.GetUniqueId() + " Setup Player: " + player.Name + " ID: " + player.Id);
    }
}
