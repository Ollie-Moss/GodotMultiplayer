using Godot;
using System;

public partial class PlayerInputSynchronizer : MultiplayerSynchronizer
{
    [Export]
    public bool JumpInput = false;
    [Export]
    public bool DashInput = false;
    [Export]
    public bool LightAttackInput = false;
    [Export]
    public Vector2 DirectionInput = new Vector2();

    public override void _Ready()
    {
        SetProcess(GetMultiplayerAuthority() == Multiplayer.GetUniqueId());
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void Jump()
    {
        JumpInput = true;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void Dash()
    {
        DashInput = true;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void LightAttack()
    {
        LightAttackInput = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsMultiplayerAuthority()) return;
        DirectionInput = Input.GetVector("Left", "Right", "Up", "Down");

        if (Input.IsActionJustPressed("Up")) Rpc(nameof(Jump));

        if (Input.IsActionJustPressed("Shift")) Rpc(nameof(Dash));

        if (Input.IsActionJustPressed("Light Attack")) Rpc(nameof(LightAttack));
    }
}
