using Godot;
using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Linq;

public partial class main_character : CharacterBody2D
{
	[Export]
	public PackedScene PlayerEffects;

	private PlayerInputSynchronizer _input;

	private int _playerId;

	public const float Speed = 250.0f;
	public const int Jumps = 3;
	public int currentJumps = Jumps;

	public const float MaxXSpeed = 500f;
	public const float MaxYSpeed = 600f;

	public float CurrentMaxSpeed = 500f;

	public float JumpVelocity = -550.0f;

	public const float DashVelocity = 1200.0f;
	public const float DashCoolDown = 0.2f;
	private bool canDash = true;

	private bool isLeft;

	private string playerAnim;
	private bool isAnimating = false;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	AnimatedSprite2D playerSprite;

    public override void _EnterTree()
    {
        _input = GetNode<PlayerInputSynchronizer>("PlayerInputSynchronizer");
        _input.SetMultiplayerAuthority(int.Parse(Name));
        GetNode<Label>("Name").Text = GameManager.Instance.GetPlayerInfo(int.Parse(Name)).Name;
    }

    public override void _Ready()
    {
        playerSprite = GetNode<AnimatedSprite2D>("Sprite2D");
    }

    public override void _Process(double delta)
    {
        if (_input.LightAttackInput)
		{
            _input.LightAttackInput = false;
            ChangeAnim(playerSprite, "Light Attack", true);
			Rpc(nameof(CreateEffect), "Attack", new Vector2(70f, -17f), 3f, isLeft);

			GetChild<Area2D>(2).Monitorable = true;
        }

        // handle animation
        if (isAnimating == false) 
		{
			ChangeAnim(playerSprite, playerAnim);
        }
    }


    public override void _PhysicsProcess(double delta)
	{
        Vector2 velocity = Velocity;
		Vector2 direction = _input.DirectionInput;

        if (direction.X < 0)
        {
            isLeft = true;
        }
        else if (direction.X > 0)
        {
            isLeft = false;
        }
        playerSprite.FlipH = isLeft;

        if (direction.X == 0)
		{
            playerAnim = "Idle";
		}
		else
		{
            playerAnim = "Run";
		}

		// Add the gravity.
		if (!IsOnFloor())
		{
			if (velocity.Y < 0)
			{
                playerAnim = "JumpUp";
			}
			else 
			{
                playerAnim = "Fall";	
			}

			velocity.Y += gravity * (float)delta;
		}
		if (IsOnFloor())
		{
			currentJumps = Jumps;
		}

		// Handle Jump.
		if (_input.JumpInput && currentJumps > 0)
		{
            _input.JumpInput = false;
            velocity.Y = JumpVelocity;
			if(!IsOnFloor() && currentJumps == Jumps)
			{
				currentJumps--;
			}
			if (!IsOnFloor()) Rpc(nameof(CreateEffect), "Jump", new Vector2(1, -14), 1.25f, false);

			currentJumps--;
		}
		// Handle Dash
		if (_input.DashInput && canDash)
		{
			CurrentMaxSpeed = DashVelocity;
			JumpVelocity = JumpVelocity / 1.25f;
			canDash = false;
			_input.DashInput = false;
			velocity.X = DashVelocity * direction.X;
			ResetDash();

			if (IsOnFloor() && direction.X != 0)
			{
                Rpc(nameof(CreateEffect), "Dash", new Vector2(-16f, -17f), 1f, isLeft);
            }
		}

		// move x axis
		velocity.X += direction.X * Speed * 0.4f;

		// decelerate x axis
		if (direction.X == 0 && IsOnFloor()) velocity.X = Mathf.MoveToward(Velocity.X, 0, 100f);
		if (direction.X == 0 && !IsOnFloor()) velocity.X = Mathf.MoveToward(Velocity.X, 0, 10f);

		// fast fall
		if (direction.Y > 0) velocity.Y += direction.Y * Speed * 0.4f;

		// Clamp x axis
		velocity.X = Mathf.Clamp(velocity.X, CurrentMaxSpeed * -1, CurrentMaxSpeed);

		// Clamp y axis
		velocity.Y = Mathf.Clamp(velocity.Y, MaxYSpeed * -1, MaxYSpeed);

		// Reduce MaxSpeed
		if (canDash == false) CurrentMaxSpeed = Mathf.MoveToward(CurrentMaxSpeed, MaxXSpeed, 50f * (IsOnFloor() ? 1f : 0.01f)) ;

		velocity.X *= 0.8f;
		velocity.Y *= 0.95f;
		Velocity = velocity;
		
		MoveAndSlide();	
	}

	public void _on_sprite_2d_animation_looped()
	{
		isAnimating = false;
	}

	private void ChangeAnim(AnimatedSprite2D sprite, string anim, bool priority = false, bool overideAnim = false)
	{
		if (isAnimating == true && overideAnim == false) return;
		sprite.Play(anim);
		isAnimating = priority;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	public void CreateEffect(string anim, Vector2 pos, float scale = 1, bool flip = false)
	{
        Node2D p = PlayerEffects.Instantiate<Node2D>();
		p.Position = this.Position;
		AnimatedSprite2D pa = p.GetChild<AnimatedSprite2D>(0);
		pa.Scale = new Vector2(scale, scale);
		pos.X = (flip ? pos.X * -1 : pos.X);
        pa.Position = pos;
        pa.FlipH = flip;
        pa.Play(anim);
        GetTree().Root.AddChild(p);
    }

    public async void ResetDash()
	{
		await this.Wait(DashCoolDown * (IsOnFloor() ? 1.25f : 1f));
		canDash = true;
		JumpVelocity = JumpVelocity * 1.25f;
	}
}
