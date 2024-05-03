using Godot;
using System;

public partial class PlayerEffects : AnimatedSprite2D
{
    public void _on_animation_finished()
    {
        QueueFree();
    }
}
