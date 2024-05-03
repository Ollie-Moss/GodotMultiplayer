using Godot;
using System;

public partial class Bullet : CharacterBody2D
{
    public const float Speed = 700f;
    public Vector2 direction = new Vector2();

    public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    public override void _Ready()
    {
        direction = new Vector2(1, 0).Rotated(Rotation);
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        velocity = Speed * direction;

        Velocity = velocity;
        MoveAndSlide();
    }
}
