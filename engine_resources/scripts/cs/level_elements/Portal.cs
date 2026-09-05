using Godot;
using System;

public partial class Portal : VariableWall
{
	/// <summary>
	/// Used to delete collision data that isn't needed.
	/// </summary>
	[Export] CollisionObject2D UnusedBody;
	/// <summary>
	/// The target for where to teleport the player upon contact.
	/// </summary>
	[Export] Node2D            TeleportTarget;

	/// <summary>
	/// Whether or not the player should have their velocity reset upon teleport.
	/// </summary>
	[Export] bool resetVelocity;
	/// <summary>
	/// Whether or not the player should have their fuel reset upon teleport.
	/// </summary>
	[Export] bool resetFuel;

	public override void _Ready()
	{
		base._Ready();
		if (UnusedBody != null)
		{
			UnusedBody.QueueFree();
		}
		if (Body is Area2D a)
		{
			a.BodyEntered += BodyDetected;
		} 
		else
		{
			GD.PrintErr("ERR in Portal.cs | _Ready() | Body was not assigned to an Area2D. Portal cannot work. Please check to make sure you did not change where the export variable \"Body\" points to.");
			this.QueueFree();
		}
	}

	private void BodyDetected(Node2D body)
	{
		if (body is Player p)
		{
			p.GlobalPosition = TeleportTarget.GlobalPosition;
			if (resetVelocity)
			{
				p.OverrideVelocity(Vector2.Zero);
			}
			if (resetFuel)
			{
				p.OverrideFuel(0.0f);
			}
		}
	}

}
