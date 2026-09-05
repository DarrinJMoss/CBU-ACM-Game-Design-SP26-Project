using Godot;
using System;

public partial class FuelPickup : Node2D
{

	[Export] Node2D Graphic;

	[Export] float restoreAmount = 15.0f;

	private bool isDisabled = false;
	private float sineVal = 0.0f;


	public override void _Ready()
	{
		Global.i.PlayerRef.PlayerNowSafe += Enable;
		Disable();
	}


	public override void _PhysicsProcess(double delta)
	{
		sineVal += (float)delta;
		Graphic.Position = new Vector2((float)Mathf.Sin(sineVal * 2.0f) * 3.0f, (float)MathF.Cos(sineVal * 2.0f) * 3.0f);
	}


	private void _BodyDetected(Node2D body)
	{
		if (body is Player p && !isDisabled)
		{
			p.AwardFuel(restoreAmount);
			Disable();
		}
	}


	public void Disable()
	{
		isDisabled = true;
		Graphic.Hide();
	}
	public void Enable()
	{
		sineVal = (float)GD.RandRange(0.0, Mathf.Tau);
		isDisabled = false;
		Graphic.Show();
	}

}
