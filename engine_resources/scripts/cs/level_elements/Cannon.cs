using Godot;
using System;

public partial class Cannon : Node2D
{
	[Export]
	public float RotationSpeed = 180.0f; // Degrees per second
	
	[Export]
	public float FireForce = 10000.0f; // Launch force

	[Export]
	public float BarrelPivotYOffset = 20.0f; // How much lower the pivot should be
	
	[Export]
	public float BarrelRotationOffset = -1.5708f; // Rotation offset in radians (default: -π/2 to point up if barrel graphic points up)

	[Export]
	public bool UseBearMode = false; // Toggle between launch (false) and beam/raytracing (true) mode
	
	[Export]
	public bool ShowTracer = true; // Show trajectory tracer line in viewport
	
	[Export]
	public float RayMaxDistance = 2000.0f; // Maximum raycast distance for beam mode
	
	[Export]
	public Color TracerColor = new Color(0, 1, 1, 0.8f); // Yellow tracer color (adjustable in UI)

	private Player playerInside;
	private bool hasPlayerInside = false;
	private Node2D barrelPivot; // The part that rotates
	private Area2D enterArea; // Collision area to detect player entry
	
	private bool canDetectPlayer = true; // Cooldown to prevent re-catching after launch
	private float detectionCooldownTimer = 0f;
	
	private Vector2 lastRaycastHitPoint = Vector2.Zero; // Store the last raycast hit for visualization
	private bool lastRaycastHit = false; // Whether the last raycast actually hit something

    private RayCast2D rc = null;

	public override void _Ready()
	{

        rc = GetNode<RayCast2D>("BarrelPivot/Rc");

		GD.Print("=== Cannon._Ready() START ===");
		
		// Get references to child nodes
		try
		{
			barrelPivot = GetNode<Node2D>("BarrelPivot");
			GD.Print("Found BarrelPivot: " + barrelPivot.Name);
			// Lower the pivot point
			barrelPivot.Position = new Vector2(barrelPivot.Position.X, BarrelPivotYOffset);
		}
		catch (Exception e)
		{
			GD.PrintErr("Failed to find BarrelPivot: " + e.Message);
		}

		try
		{
			enterArea = GetNode<Area2D>("EnterArea");
			GD.Print("Found EnterArea: " + enterArea.Name);
			GD.Print("EnterArea GlobalPosition: " + enterArea.GlobalPosition);
			GD.Print("EnterArea Monitoring: " + enterArea.Monitoring);
			GD.Print("EnterArea Monitorable: " + enterArea.Monitorable);
		}
		catch (Exception e)
		{
			GD.PrintErr("Failed to find EnterArea: " + e.Message);
			return;
		}

		// Connect the area signals
		try
		{
			enterArea.BodyEntered += OnBodyEntered;
			enterArea.BodyExited += OnBodyExited;
			GD.Print("Signals connected successfully");
		}
		catch (Exception e)
		{
			GD.PrintErr("Failed to connect signals: " + e.Message);
		}

		GD.Print("=== Cannon._Ready() END ===");
	}

	public override void _Process(double delta)
	{
		// Handle cannon rotation
		HandleRotation((float)delta);
		
		// Update detection cooldown
		if (!canDetectPlayer)
		{
			detectionCooldownTimer -= (float)delta;
			if (detectionCooldownTimer <= 0f)
			{
				canDetectPlayer = true;
				GD.Print("[CANNON] Detection cooldown ended, ready to catch next player");
			}
		}

		// Manually check for player in area (instead of relying on signals)
		var overlappingBodies = enterArea.GetOverlappingBodies();
		
		if (overlappingBodies.Count > 0 && canDetectPlayer)
		{
			// Check each overlapping body
			foreach (var body in overlappingBodies)
			{
				if (body is Player playerBody && !hasPlayerInside)
				{
					playerInside = playerBody;
					hasPlayerInside = true;
					// Move player to barrel pivot (top of cannon barrel)
					playerBody.GlobalPosition = barrelPivot.GlobalPosition;
					playerBody.EnterCannon();
					GD.Print("[CANNON] ✓ Player entered cannon at barrel position!");
				}
			}
		}

		// Update raycast for tracer visualization (if in beam mode or showing tracer)
		if (ShowTracer || UseBearMode)
		{
			UpdateRaycast();
		}

		// Handle firing the player
		if (hasPlayerInside && playerInside != null)
		{
			HandleFiring();
		}
		
		// Draw the trajectory tracer
		if (ShowTracer && lastRaycastHit)
		{
			DebugDraw();
		}
	}

	private void HandleRotation(float delta)
	{
		// Rotate the barrel continuously
		barrelPivot.Rotation += Mathf.DegToRad(RotationSpeed * delta);
	}

	private void HandleFiring()
	{
		// Check for boost input (note: "Boost" with capital B)
		if (Input.IsActionJustPressed("Boost"))
		{
			GD.Print("[CANNON] 🔥 BOOST INPUT DETECTED! Firing...");
			FirePlayer();
		}
	}

	private void FirePlayer()
	{
		if (playerInside == null)
		{
			GD.PrintErr("[CANNON] FirePlayer: playerInside is null!");
			return;
		}

		// Get the direction the barrel is facing (with rotation offset applied)
		Vector2 fireDirection = Vector2.FromAngle(barrelPivot.Rotation + BarrelRotationOffset);
		
		if (UseBearMode && lastRaycastHit)
		{
			// BEAM MODE: Teleport player to raycast hit point
			GD.Print("[CANNON] 🌟 BEAM MODE! Teleporting player to: " + lastRaycastHitPoint);
			playerInside.GlobalPosition = lastRaycastHitPoint;
			playerInside.LaunchFromCannon(Vector2.Zero); // Call with zero velocity to indicate beam teleport
		}
		else
		{
			// LAUNCH MODE: Traditional velocity-based launch
			Vector2 launchVelocity = fireDirection * FireForce;
			GD.Print("[CANNON] 🔥 LAUNCH MODE! Barrel angle: " + Mathf.RadToDeg(barrelPivot.Rotation) + "° | Fire direction: " + fireDirection + " | Launch Velocity: " + launchVelocity);
			playerInside.LaunchFromCannon(launchVelocity);
		}
		
		playerInside = null;
		hasPlayerInside = false;
		
		// Start cooldown to prevent re-catching the player as they escape
		canDetectPlayer = false;
		detectionCooldownTimer = 0.2f; // 0.2 seconds should be enough to escape
		GD.Print("[CANNON] ✓ Player fired from cannon! Detection cooldown started.");
	}

	private void OnBodyEntered(Node body)
	{
		GD.Print("### OnBodyEntered called! Body: " + body.Name + " (Type: " + body.GetType().Name + ")");
		
		// Try direct cast
		if (body is Player player && !hasPlayerInside)
		{
			playerInside = player;
			hasPlayerInside = true;
			player.EnterCannon();
			GD.Print("Player entered cannon via direct cast");
			return;
		}
		
		// Try finding Player in parents
		Player playerParent = body.GetParent() as Player;
		if (playerParent != null && !hasPlayerInside)
		{
			playerInside = playerParent;
			hasPlayerInside = true;
			playerParent.EnterCannon();
			GD.Print("Player entered cannon via parent search");
		}
	}

	private void OnBodyExited(Node body)
	{
		if (body is Player player && hasPlayerInside && playerInside == player)
		{
			playerInside = null;
			hasPlayerInside = false;
			player.ExitCannon();
			
			GD.Print("Player exited cannon");
		}
	}

	/// <summary>
	/// Performs a raycast from the barrel in the fire direction to find where the beam would land.
	/// Updates lastRaycastHitPoint and lastRaycastHit accordingly.
	/// </summary>
	private void UpdateRaycast()
	{
        /*
		// Get the direction the barrel is facing (with rotation offset applied)
		Vector2 fireDirection = Vector2.FromAngle(barrelPivot.Rotation + BarrelRotationOffset);
		Vector2 rayStart = barrelPivot.GlobalPosition;
		Vector2 rayEnd = rayStart + (fireDirection * RayMaxDistance);

		// Create raycast parameters
		var rayQuery = PhysicsRayQueryParameters2D.Create(rayStart, rayEnd);
		rayQuery.CollisionMask = 0xFFFFFFFF; // Check all collision layers
		
		// Perform the raycast
		var space = GetWorld2D().DirectSpaceState;
		var result = space.IntersectRay(rayQuery);

		while (result.Count > 0)
		{
			lastRaycastHitPoint = (Vector2)result["position"];
			lastRaycastHit = true;
			
			if (UseBearMode && hasPlayerInside)
			{
				GD.Print("[CANNON BEAM] Would teleport to: " + lastRaycastHitPoint);
			}
		}
		else
		{
			// No hit - ray goes to max distance
			lastRaycastHitPoint = rayEnd;
			lastRaycastHit = false;
		}
        */

        rc.ForceRaycastUpdate();
        if (rc.GetCollider() != null)
        {
            lastRaycastHitPoint = rc.GetCollisionPoint();
            lastRaycastHit = true;
        }
        else
        {
            lastRaycastHitPoint = rc.TargetPosition + rc.GlobalPosition;
            lastRaycastHit = false;
        }
        


	}

	/// <summary>
	/// Draws a debug line showing the trajectory/beam path from barrel to hit point.
	/// This is visible in the Godot viewport as a yellow line (color adjustable via TracerColor).
	/// </summary>
	private void DebugDraw()
	{
		// Queue a redraw so _Draw() gets called
		QueueRedraw();
	}

	public override void _Draw()
	{
		// Only draw if tracer is enabled and we have a valid raycast hit
		if (ShowTracer && lastRaycastHit)
		{
			// Draw a line from barrel pivot to the hit point
			DrawLine(barrelPivot.GlobalPosition - GlobalPosition, lastRaycastHitPoint - GlobalPosition, TracerColor, 3.0f);
			
			// Draw a small circle at the hit point
			DrawCircle(lastRaycastHitPoint - GlobalPosition, 8.0f, TracerColor);
		}
	}
}
