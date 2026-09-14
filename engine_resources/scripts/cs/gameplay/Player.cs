using Godot;using System;

[GlobalClass]
public partial class Player : CharacterBody2D
{
    /************** VARIABLES/CONSTANTS ***************/
    /******************* References *******************/
    [Export] private RichTextLabel   UI_FuelIndicator;
    [Export] private Label           UI_Speed;
    [Export] private Node2D          GraphicsRoot;
    [Export] private GpuParticles2D  PT_SkidParticles;
    [Export] private GpuParticles2D  PT_WalkParticles;
    [Export] private GpuParticles2D  PT_ThrusterL;
    [Export] private GpuParticles2D  PT_ThrusterR;
    [Export] private Control         DB_RespawnLocation;
    [Export] private Node2D          DB_AngleIndicator;
    [Export] private AnimationPlayer Anim;
    [Export] private Sprite2D        PSpriteTorso;
    [Export] private Sprite2D        PSpriteLegs;
    [Export] private Sprite2D        PSpriteJetpack;
    [Export] private AudioStreamPlayer2D JumpSfx;
    [Export] private AudioStreamPlayer2D StepSfx;
    [Export] private AudioStreamPlayer2D LandSfx;
    [Export] private AudioStreamPlayer2D JetpackSfx;
    [Export] private AudioStreamPlayer2D JeckpackLaunchSfx;

    public PCam PCamRef; // Spawned in at runtime

    /**************** Camera Movement ****************/
    private const float MAX_CAMERA_DISTANCE_G   = 50.0f;
    private const float MAX_CAMERA_DISTANCE_A   = 100.0f;
    private const float BOOST_SHAKE_AMNT        = 5.0f;
    private const float BOOST_SHAKE_DAMPING     = 350.0f;


    /************** Movement Constants ***************/
    private const float WALKING_SPEED  = 400.0f;
    private const float JUMP_VELOCITY  = 300.0f;
    private const float H_ACCEL_RATE   = 650.0f;        // * Delta
    private const float H_DECEL_RATE   = 800.0f;        // * Delta
    private const float GRAVITY        = 800.0f;       // * Delta
    private const float SPEED_HARDCAP  = 750.0f;
    private const float SPEED_SOFTCAP  = 400.0f;
    private const float AIR_CONTROL    = 0.5f;
    private const float AIR_ANGVEL_LOSS_RATE = 1.0f;
    private const float LAND_SHAKE_AMNT      = 2.2f;
    private const float LAND_SHAKE_DAMPING   = 350.0f;


    /*************** Jetpack Variables ***************/
    private const float THRUST_FORCE   = 950.0f;
    private const float ONETHRUSTER_DIV = 2.0f;
    private const float ANGULAR_VELOCITY_ONETHRUSTER = 2.0f;
    private const float BOOST_FORCE    = 950.0f;        // Floor boost
    private const float BOOST_CAM_LAG_AMNT = 0.6f;
    private const float FLBOOST_MAX_ANGLE = (float)Mathf.Pi / 4.0f;
    private const float FLBOOST_WALK_SPEED_MAXANGLE = WALKING_SPEED * 0.75f;

    private bool canBoost = false;                      // Whether we can boost or not
    private bool boostedThisFrame = false;

    /*************** Recovery Variables **************/
    private bool isRecovering = false;
    private const float RECOVERY_TIME = 0.3f;
    private float currentRecoveryTimer = 0.0f;

  	/******* Ice Vars *****/
	private bool isOnSlide = false;
    private bool onSlipperySlope = false;

    /**************** Fuel Variables *****************/
    private const float MAX_FUEL = 100.0f;              // * Delta
    private const float FUEL_DRAIN_RATE = 50.0f;        // * Delta
    private const float FUEL_REGEN_RATE = 100.0f;       // * Delta
    private const float BOOST_COST      = 30.0f;

    private float currentFuel = 100.0f;

    /************** Animation Varaibles **************/
    private const float IDLE_SPEED_TRHESH = 115.0f;
    private const float RUN_SPEED_DIVIDER = 300.0f;
    private const float FUEL_ICON_ALPHA_WEIGHT = 15.0f;
    private String curAnim = "";
    private bool cutoffEn = false;
    private float fuelTargetAlpha = 0.0f;

    private bool isSkidding = false;
    /**************** Other Variables ****************/
    [Export] private bool startLevelWithJetpack = true;

    private const float STANDUP_SPEED = 15.0f;          // * Delta
    private const float RESPAWN_OFFSET_Y = -32.0f;

    private const float JETPACK_PITCH_RATE = 0.6f;
    private const float JETPACK_PITCH_THRESHH = 0.75f;

    private float currentAngle = (float)Math.PI / -2.0f;// Facing up
    private Vector2 curVel = Vector2.Zero;              // See _PhysicsProcess() for more info
    public Vector2 respawnPosition = Vector2.Zero;

    // Reference to the phyiscs delta in case non-process functions need it
    // Also saves typing (float)delta over and over again
    private float deltaRef = 1.0f;      
    private float angularVelocity = 0.0f;       
    private bool wasInAir = false;     
    private bool isSafe = false;
    private float launchBoostTimer = 0f; // Duration of launch momentum (no clamping)
    private const float LAUNCH_BOOST_DURATION = 0.5f; // 0.5 seconds of unrestricted velocity
    private bool isExitingCannon = false; // Prevents movement/jetpack after exiting cannon
    private bool exitedCannonThisFrame = false;
    private float jetpackPitch = 0.0f;
    public bool controlEnabled = true;

    /*************** Slime/Bounce Variables ***************/
    private const float SLIME_AMPLIFIER = 1.5f;        // How much slime amplifies your velocity
    private const float SLIME_MIN_SPEED = 400.0f;


    /******************** SIGNALS ********************/

    [Signal] public delegate void PlayerNowSafeEventHandler();


    /******************** METHODS ********************/
    public override void _Ready()
    {
        Global.i.PlayerRef = this;
        PCamRef = new PCam();
        GetParent().CallDeferred(Node.MethodName.AddChild, PCamRef);
        PSpriteTorso.FrameChanged += _TorsoFrameUpdated;
        Anim.AnimationFinished += _AnimationFinished;

        respawnPosition = this.GlobalPosition;
    }

    public override void _ExitTree()
    {
        Global.i.PlayerRef = null;
        base._ExitTree();
    }


    public override void _PhysicsProcess(double delta)
    {
    	// If in cannon, disable normal movement
    	if (isInCannon)
    		return;
    
    	// If exiting cannon, don't allow movement or input
        if (isExitingCannon)
        {
            _Move(curVel); // Just apply gravity, no input
            return;
        }

        DB_AngleIndicator.Rotation = currentAngle;
        DB_RespawnLocation.GlobalPosition = respawnPosition;

        currentAngle += angularVelocity * deltaRef;

        PT_SkidParticles.Emitting = false; isSkidding = false;
        PT_WalkParticles.Emitting = false;

        UI_Speed.Text = "X: " + Mathf.RoundToInt(curVel.X).ToString() + "\nY: " + Mathf.RoundToInt(curVel.Y).ToString();

        GraphicsRoot.Rotation = Mathf.LerpAngle(GraphicsRoot.Rotation, currentAngle, Global.i.GetClampedDelta_PH() * STANDUP_SPEED);

        deltaRef = (float)delta;
        float accel = H_ACCEL_RATE;
		float decel = H_DECEL_RATE;

		if (isOnSlide)
		{
			decel *= 0.08f; // ~8% of normal decel — tweak to taste
			accel *= 0.5f;  // Also makes it harder to accelerate, feels more "slippery"
		}

        // Handle Recovery Timer (Stun State)
        if (isRecovering)
        {
            currentRecoveryTimer -= deltaRef;
            curVel.X = Mathf.MoveToward(curVel.X, 0.0f, decel * deltaRef);

            if (currentRecoveryTimer <= 0.0f)
            {
                isRecovering = false;
                canBoost = true;
            }

            _Move(curVel);
            return;
        }
        

        // Floor stuff
        if (this.IsOnFloor()) {

            GD.Print(GetFloorAngle()); GD.Print(GetFloorNormal());

            if (wasInAir)
            {
                wasInAir = false;
                if (!isOnSlide)
                {
                    curVel.Y = GRAVITY * deltaRef;
                }
                angularVelocity = 0.0f;
                isExitingCannon = false; // Re-enable controls when landing
            }

            PCamRef.SetVelocityPan(new Vector2(Mathf.Min(curVel.X / SPEED_SOFTCAP, 1.0f) * (MAX_CAMERA_DISTANCE_G / 2.0f), -16.0f));
            PCamRef.SetAnglePan   (new Vector2(Mathf.Min(curVel.X / SPEED_SOFTCAP, 1.0f) * (MAX_CAMERA_DISTANCE_G / 2.0f), -16.0f));

            currentAngle = Mathf.LerpAngle(currentAngle, (float)Math.PI / -2.0f, STANDUP_SPEED * Global.i.GetClampedDelta_PH());

            if (isSafe)
            {
                respawnPosition = this.GlobalPosition;

                // Fuel regen
                currentFuel = Mathf.MoveToward(currentFuel, MAX_FUEL, FUEL_REGEN_RATE * deltaRef);

            }

            // If we boosted, stun a bit
            if (!canBoost)
            {
                _TriggerRecovery();
                return;
            }

            GD.Print($"El anglei! {GetFloorAngle()}");

            // Ground Movement
            float xInput = controlEnabled ? Input.GetAxis("MoveLeft", "MoveRight") : 0.0f;
            if (Mathf.Abs(xInput) >= Global.CONTROLLER_DEADZONE)
            {
                // Give a turnaround boost in the event that you want to go in the
    			// opposite direction of where you're currently going (ground only)
    			if (Mathf.Sign(xInput) != Mathf.Sign(curVel.X))
    			{
    				PT_SkidParticles.Emitting = true;
                    isSkidding = true;
    				curVel.X = Mathf.MoveToward(curVel.X, WALKING_SPEED * Mathf.Sign(xInput), decel * deltaRef);
    				curVel.X = Mathf.MoveToward(curVel.X, WALKING_SPEED * Mathf.Sign(xInput), accel * deltaRef);
    			} 
    			else
    			{
    				PT_WalkParticles.Emitting = true;
    				if (Mathf.Abs(curVel.X) <= WALKING_SPEED)
    				{
    					curVel.X = Mathf.MoveToward(curVel.X, WALKING_SPEED * Mathf.Sign(xInput), accel * deltaRef);
    				}
    			}
    		} 
            else if (isOnSlide && GetFloorAngle() > 0.7)
    		{
                GD.Print("Slippery!");
                onSlipperySlope = true;
    			xInput = 0.0f;
                curVel += Gravity() * deltaRef;
    			curVel = curVel.MoveToward(new Vector2(WALKING_SPEED * GetFloorNormal().X, SPEED_HARDCAP), decel * 25.0f * deltaRef);
    		}
            else
            {
                GD.Print("Avg!");
                onSlipperySlope = false;
                xInput = 0.0f;
    			curVel.X = Mathf.MoveToward(curVel.X, 0.0f, decel * deltaRef);
            }
    

    		if (Input.IsActionJustPressed("Jump") && controlEnabled)
    		{
                if (onSlipperySlope)
                {
                    onSlipperySlope = false;
                    curVel.X = this.Velocity.X;
                }
    			curVel.Y = -JUMP_VELOCITY;
    			OneshotParticleManager.i.SpawnParticleAsChild(OneshotParticleManager.ParticleTypes.LAND_JUMP, this, new Vector2(0.0f, 11.0f), 0.0f, 5);
    		}
    	} 

    	// Air Stuff
    	else
    	{

    		if (!wasInAir)
    		{
    			wasInAir = true;
    			angularVelocity = curVel.X / (WALKING_SPEED / 2.0f);
    		}
    		if (!boostedThisFrame)
    		{
    			curVel += Gravity() * deltaRef;
    		}

    		PCamRef.SetAnglePan(Vector2.Right.Rotated(currentAngle) * (MAX_CAMERA_DISTANCE_A / 2.0f));
    		PCamRef.SetVelocityPan(curVel.Normalized() * (MAX_CAMERA_DISTANCE_A / 2.0f));

    		// Gradually lose angular velocity in the air
    		angularVelocity = Math.Sign(angularVelocity) * Math.Clamp(Math.Abs(angularVelocity) - AIR_ANGVEL_LOSS_RATE * deltaRef, 0.0f, 10000.0f);

    		// Air Control
    		float xInput = Input.GetAxis("MoveLeft", "MoveRight"); 
    		if (Mathf.Abs(xInput) >= Global.CONTROLLER_DEADZONE)
    		{
    			if ((Mathf.Sign(xInput) == Mathf.Sign(curVel.X)) && Mathf.Abs(curVel.X) <= WALKING_SPEED)
    			{
    				curVel.X = Mathf.MoveToward(curVel.X, WALKING_SPEED * Mathf.Sign(xInput), accel * deltaRef * AIR_CONTROL);
    			}
    		}
    	}

    	boostedThisFrame = false;

        PT_ThrusterL.Emitting = false;
        PT_ThrusterR.Emitting = false;

    	// Jetpack Stuff
    	if (HasJetpack() && currentFuel > 0.0f && controlEnabled)
    	{
    		// Boost
    		if ((currentFuel >= BOOST_COST) && canBoost && Input.IsActionJustPressed("Boost") && !isInCannon && !exitedCannonThisFrame)
    		{
    			canBoost = false;
    			if (this.IsOnFloor()) 
    			{
    				currentAngle = FLBOOST_MAX_ANGLE * Mathf.Clamp(Mathf.Abs(curVel.X) / FLBOOST_WALK_SPEED_MAXANGLE, 0.0f, 1.0f) * Mathf.Sign(curVel.X) - (float)Mathf.Pi / 2.0f;
    			} 
    			curVel = (Vector2.Right * BOOST_FORCE).Rotated(currentAngle);
    			currentFuel -= BOOST_COST;
    			boostedThisFrame = true;

    			OneshotParticleManager.i.SpawnParticleAsChild(OneshotParticleManager.ParticleTypes.BOOST_EXPLOSION, GraphicsRoot, new Vector2(0.0f, 0.0f), 0.0f, 5);
    			PCamRef.Lag(BOOST_CAM_LAG_AMNT);
    			PCamRef.SetShake(BOOST_SHAKE_AMNT, BOOST_SHAKE_DAMPING);
    		}
    		else
    		{

    			if (Input.IsActionPressed("JetpackLeft") && Input.IsActionPressed("JetpackRight"))
    			{
    				currentFuel = Mathf.MoveToward(currentFuel, 0.0f, FUEL_DRAIN_RATE * deltaRef);
    				angularVelocity = 0.0f;
    				_ApplyJetpackImpulse((Vector2.Right * THRUST_FORCE * deltaRef).Rotated(currentAngle));
    
    				boostedThisFrame = true;

                    PT_ThrusterL.Emitting = true;
                    PT_ThrusterR.Emitting = true;
    			}
    			// Note: Using a single thruster only uses 1/2 of the fuel you'd normally use
                else if (Input.IsActionPressed("JetpackLeft"))
                {
                    currentFuel = Mathf.MoveToward(currentFuel, 0.0f, FUEL_DRAIN_RATE * 0.5f * deltaRef);
                    angularVelocity = -ANGULAR_VELOCITY_ONETHRUSTER;
                    _ApplyJetpackImpulse((Vector2.Right * (THRUST_FORCE / ONETHRUSTER_DIV) * deltaRef).Rotated(currentAngle));

                    boostedThisFrame = true;
                    PT_ThrusterL.Emitting = true;
                }
                else if (Input.IsActionPressed("JetpackRight"))
                {
                    currentFuel = Mathf.MoveToward(currentFuel, 0.0f, FUEL_DRAIN_RATE * 0.5f * deltaRef);
                    angularVelocity = ANGULAR_VELOCITY_ONETHRUSTER;
                    _ApplyJetpackImpulse((Vector2.Right * (THRUST_FORCE / ONETHRUSTER_DIV) * deltaRef).Rotated(currentAngle));

                    boostedThisFrame = true;
                    PT_ThrusterR.Emitting = true;
                }
            }
        } else // No fuel
        {
        }

        exitedCannonThisFrame = false;

        _Move(curVel);
        _Animate();
    }


    // Dereference the player reference in Global.cs and queue
    // free for the player.
    // If you need to call Free() as opposed to QueueFree(), just
    // set the "instant" argument to true
    public void DespawnPlayer(bool instant = false)
    {
        Global.i.PlayerRef = null;
        if (instant)
        {
            this.Free();
            return;
        }
        this.QueueFree();
    }

    /// <summary>
    /// Returns the current gravity.
    /// As opposed to gravity being a set value, it is instead a scalar that
    /// can be modified by an Area2D. It typically is set at 1 px/sec^2, which
    /// is really small. This is because it is being used as a multiplier for
    /// the actual gravity.
    /// Ex. if it was 0.5 px/sec^2, it would be half-as-strong gravity,
    /// if it were 2.0 px/sec^2, it would be doubly strong.
    /// The actual value of gravity on the player is defined in the GRAVITY
    /// constant above.
    /// </summary>
    /// <returns></returns>
    public Vector2 Gravity()
    {
        return this.GetGravity() * GRAVITY;
    }


    /// <summary>
    /// Contains all movement code for the player once velocity has been calculated.
    /// </summary>
    /// <param name="vel"></param>
    private void _Move(Vector2 vel)
    {

        // Update launch boost timer
        if (launchBoostTimer > 0)
            launchBoostTimer -= deltaRef;

    	// If launched from cannon, don't clamp velocity while timer is active
    	if (launchBoostTimer > 0)
    	{
    		curVel = vel;
    		this.Velocity = vel;
    	}
    	else
    	{
    		curVel        = new Vector2(Mathf.Clamp(vel.X, -SPEED_HARDCAP, SPEED_HARDCAP), Mathf.Clamp(vel.Y, -SPEED_HARDCAP, SPEED_HARDCAP));
    		this.Velocity = new Vector2(Mathf.Clamp(vel.X, -SPEED_SOFTCAP, SPEED_SOFTCAP), Mathf.Clamp(vel.Y, -SPEED_SOFTCAP, SPEED_SOFTCAP));
    	}

    	// Ugly hack to get MoveAndSlide()'s velocity modifications on curVel
        Vector2 preVel = this.Velocity;
        Vector2 preVelSlam = curVel;

        /******** SLIME/BOUNCE - Arturo ********/
        // Loop through every surface the player is touching this frame
        for (int i = 0; i < this.GetSlideCollisionCount(); i++)
        {
        	// Get the current collision info
        	KinematicCollision2D collision = this.GetSlideCollision(i);

        	// Get whatever object we collided with
        	GodotObject collider = collision.GetCollider();

        	// Check if that object belongs to the "bounce" group
        	if (collider is Node node && node.IsInGroup("bounce"))
        	{
        		// Get the direction the surface is facing (used to calculate bounce angle)
        		Vector2 normal = collision.GetNormal();

        		// Reflect the player's velocity off the surface, then multiply it
        		// to make the launch stronger than a normal bounce
        		// Example: hit it going up = launched back down harder
        		//          hit it going left = launched back right harder
                this.Velocity = this.Velocity.Bounce(normal) * SLIME_AMPLIFIER * 1.2f;
        	}
            else if (collider is Node n && n.IsInGroup("Walls"))
        	{
                isExitingCannon = false;
        	}
        }

        if (onSlipperySlope)
        {
            FloorSnapLength = onSlipperySlope ? 128.0f : 8.0f;
            this.Velocity = new Vector2(this.Velocity.X, SPEED_SOFTCAP - 1.0f);
            this.MoveAndSlide();
        }
        else
        {
            this.MoveAndSlide();
        }
        
        /******** ICE/SLIDE  ********/
		// Loop through every surface the player is touching this frame
		isOnSlide = false;
		for (int i = 0; i < this.GetSlideCollisionCount(); i++)
		{
			KinematicCollision2D collision = this.GetSlideCollision(i);
			GodotObject collider = collision.GetCollider();

			if (collider is Node node && node.IsInGroup("Ice"))
			{
				isOnSlide = true;
			}
		}

        // Reset launch boost if velocity drops below 500 (clean transfer point)
        if (launchBoostTimer > 0 && curVel.Length() < 500f)
        {
            launchBoostTimer = 0f;
            //GD.Print("[LAUNCH] Velocity dropped below 500, ending launch boost");
        }

        if (preVel.X != 0.0f)
        {
            curVel.X *= this.Velocity.X / preVel.X;
        }
        if (preVel.Y != 0.0f)
        {
            curVel.Y *= this.Velocity.Y / preVel.Y;
        }

        if (Mathf.Abs(curVel.X - preVelSlam.X) > SPEED_SOFTCAP)
        {
            PCamRef.SetShake(LAND_SHAKE_AMNT, LAND_SHAKE_DAMPING);
            OneshotParticleManager.i.SpawnParticleAsChild(OneshotParticleManager.ParticleTypes.LAND_JUMP, this, new Vector2(8.0f * Mathf.Sign(preVelSlam.X), 0.0f), GetWallNormal().Angle() - Mathf.Pi / 2.0f, 5);

        }
        if (Mathf.Abs(curVel.Y - preVelSlam.Y) > SPEED_SOFTCAP - 1.0f)
        {
            PCamRef.SetShake(LAND_SHAKE_AMNT, LAND_SHAKE_DAMPING);
            if (preVelSlam.Y > 0.0f) // Floor
            {
                //GD.Print(OneshotParticleManager.i);
                OneshotParticleManager.i.SpawnParticleAsChild(OneshotParticleManager.ParticleTypes.LAND_JUMP, this, new Vector2(0.0f, 8.0f), GetFloorAngle(), 5);
            }
            else
            {
                // TODO: Angle ceiling dust
                OneshotParticleManager.i.SpawnParticleAsChild(OneshotParticleManager.ParticleTypes.LAND_JUMP, this, new Vector2(0.0f, -8.0f), 0.0f, 5);
            }

        }

    }


    /// <summary>
    /// Applies a force to the player for the jetpack.
    /// Assumes the value has already been scaled by some frame delta.
    /// </summary>
    /// <param name="impulse"></param>
    private void _ApplyJetpackImpulse(Vector2 impulse)
    {
        Vector2 multiplier = Vector2.One;

        // Apply a bonus acceleration to improve turnaound speed
        if (Mathf.Sign(curVel.X) != Mathf.Sign(impulse.X) && Mathf.Sign(curVel.X) != 0.0f)
        {
            multiplier.X = 2.0f * Mathf.Clamp(Mathf.Abs(curVel.X) / SPEED_SOFTCAP, 0.5f, 1.0f);
        }
        if (Mathf.Sign(curVel.Y) != Mathf.Sign(impulse.Y) && Mathf.Sign(curVel.Y) != 0.0f)
        {
            multiplier.Y = 2.0f * Mathf.Clamp(Mathf.Abs(curVel.Y) / SPEED_SOFTCAP, 0.5f, 1.0f);
        }

        curVel += impulse * multiplier;
    }

    private void _Animate()
    {

        if (isSafe && IsOnFloor() && controlEnabled)
        {
            GetNode<GpuParticles2D>("ChargeParticles")      .Emitting   = true;
            GetNode<PointLight2D>("ChargeParticles/Light")  .Enabled    = true;
        }
        else
        {
            GetNode<GpuParticles2D>("ChargeParticles")      .Emitting   = false;
            GetNode<PointLight2D>("ChargeParticles/Light")  .Enabled    = false;
        }

        if (jetpackPitch >= JETPACK_PITCH_THRESHH)
        {
            JetpackSfx.PitchScale = jetpackPitch;
            JetpackSfx.Play();
        } else
        {
            JetpackSfx.Stop();
        }
        if (IsOnFloor())
        {
            PSpriteLegs.Rotation = -currentAngle;


            if (Mathf.Sign(curVel.X) == -1.0f)
            {
                PSpriteTorso.FlipH = true;
                PSpriteLegs.FlipH  = true;
                PSpriteJetpack.FlipH = true;
            }
            else if (Mathf.Sign(curVel.X) == 1.0f)
            {
                PSpriteTorso.FlipH = false;
                PSpriteLegs.FlipH  = false;
                PSpriteJetpack.FlipH = false;
            }


            if (isSkidding && Mathf.Abs(curVel.X) <= IDLE_SPEED_TRHESH && isOnSlide)
            {
                _SetAnim("Skid", true);
            }
            else if (Mathf.Abs(curVel.X) <= IDLE_SPEED_TRHESH)
            {
                Anim.SpeedScale = 1.0f;
                _SetAnim("Idle", false);
            }
            else if (isSkidding)
            {
                _SetAnim("Skid", true);
            }
            else
            {
                Anim.SpeedScale = Mathf.Abs(curVel.X) / RUN_SPEED_DIVIDER;
                _SetAnim("Run", true);
            }
        }
        else
        {
            _SetAnim("Jump", true);
            PSpriteLegs.Rotation = Mathf.LerpAngle(PSpriteLegs.Rotation, Mathf.DegToRad(180) + currentAngle, 2.0f * Global.i.GetClampedDelta_PH());
        }


        string color;
        if (isSafe && IsOnFloor() && controlEnabled)
        {
            color = "[color=#FF54C1]";
        }
        else if (currentFuel > 50.0f)
        {
            color = "";
        }
        else if (currentFuel > BOOST_COST)
        {
            color = "[color=yellow]";
        }
        else
        {
            color = "[color=red]";
        }
        UI_FuelIndicator.Text = color + Mathf.RoundToInt(currentFuel).ToString();
        UI_FuelIndicator.Scale = Vector2.One / PCamRef.Zoom;

        if (currentFuel <= 0.2f)
        {
            fuelTargetAlpha = 0.0f;
        }
        else if (boostedThisFrame)
        {
            fuelTargetAlpha = 1.0f;
        }
        else
        {
            if (isSafe && IsOnFloor() && controlEnabled)
            {
                fuelTargetAlpha = 1.0f;
            }
            else if (currentFuel >= 99.5f)
            {
                fuelTargetAlpha = 0.0f;
            }
            else
            {
                if (!IsOnFloor())
                {
                    fuelTargetAlpha = 0.8f;
                }
                else
                {
                    fuelTargetAlpha = 0.4f;
                }
            }    
        }
        UI_FuelIndicator.SelfModulate = new Color(UI_FuelIndicator.SelfModulate.R, UI_FuelIndicator.SelfModulate.G, UI_FuelIndicator.SelfModulate.B, Mathf.Lerp(UI_FuelIndicator.SelfModulate.A, fuelTargetAlpha, FUEL_ICON_ALPHA_WEIGHT * Global.i.GetClampedDelta_PH()));
        
    }


    private void _HandleHazardDetector(Node2D body)
    {
        Die();
    }

    private void _HandleSafetyDetectorEnter(Node2D body)
    {
        EmitSignal(SignalName.PlayerNowSafe);
        isSafe = true;
    }
    private void _HandleSafetyDetectorExit(Node2D body)
    {
        isSafe = false;
    }

    private void _TriggerRecovery()
    {
        isRecovering = true;
        currentRecoveryTimer = RECOVERY_TIME;
        curVel.X /= 2.0f;

        // TODO: Soft camera kick
    }


    public void Die()
    {
        this.GlobalPosition = respawnPosition + new Vector2(0.0f, RESPAWN_OFFSET_Y);
        curVel = Vector2.Down * GRAVITY * deltaRef;
        currentFuel = 0.0f;
        currentAngle = 0.0f;
        angularVelocity = 0.0f;
        canBoost = false;
        isRecovering = false;
        currentRecoveryTimer = 0.0f;

    }


    public void AwardFuel(float amount)
    {
        currentFuel = Mathf.Clamp(currentFuel + amount, 0.0f, MAX_FUEL);
    }

    private bool isInCannon = false;

    public void EnterCannon()
    {
        isInCannon = true;
        // Hide the player graphics and particles
        if (GraphicsRoot != null)
            GraphicsRoot.Visible = false;
        if (PT_ThrusterL != null)
            PT_ThrusterL.Emitting = false;
        if (PT_ThrusterR != null)
            PT_ThrusterR.Emitting = false;
        if (PT_SkidParticles != null)
            PT_SkidParticles.Emitting = false;
        if (PT_WalkParticles != null)
            PT_WalkParticles.Emitting = false;
        //GD.Print("[CANNON] Player hidden and particles stopped");
    }

    public void ExitCannon()
    {
        isInCannon = false;
        isExitingCannon = true; // Freeze player after exiting
        // Show the player graphics again
        if (GraphicsRoot != null)
            GraphicsRoot.Visible = true;
        //GD.Print("[CANNON] Player shown again - FROZEN until landing");
    }

    public void LaunchFromCannon(Vector2 launchVelocity)
    {
        isInCannon = false; exitedCannonThisFrame = true;
        PCamRef.SetShake(BOOST_SHAKE_AMNT, BOOST_SHAKE_DAMPING);
        curVel = launchVelocity;
        this.Velocity = launchVelocity;
        launchBoostTimer = LAUNCH_BOOST_DURATION; // 0.5 seconds of unrestricted velocity
        // Show player graphics when launched
        if (GraphicsRoot != null)
            GraphicsRoot.Visible = true;
        //GD.Print("[CANNON] Player launched with velocity: " + launchVelocity);
    }
    public void OverrideVelocity(Vector2 amount)
    {
        curVel = amount;
    }
    public void OverrideFuel(float amount)
    {
        currentFuel = amount;
    }

    public void ApplyBounceImpulse(Vector2 amount)
    {
	    curVel = amount;
    }

    public float GetFuelCount()
    {
        return this.currentFuel;
    }

    public bool HasJetpack()
    {
        return startLevelWithJetpack;
    }
    public void GiveJetpack()
    {
        startLevelWithJetpack = true;
    }

    private void _TorsoFrameUpdated()
    {
        PSpriteLegs.Frame = PSpriteTorso.Frame;
        PSpriteJetpack.Frame = PSpriteTorso.Frame;
    }
    private void _SetAnim(String newName, bool cutoff)
    {
        curAnim = newName;
        cutoffEn = cutoff;
        if (Anim.IsPlaying() == false)
        {
            _AnimationFinished("Idle");
            return;
        }
        StringName animation = Anim.CurrentAnimation;
        //GD.Print(_GetAnimSuffix(animation));
        if (_GetAnimPrefix(animation) != curAnim)
        {
            if (cutoffEn)
            {
                if (_GetAnimSuffix(animation) == "INTRO")
                {
                    return;
                }
                if (Anim.HasAnimation(curAnim + "_INTRO"))
                    Anim.Play(curAnim + "_INTRO");
                else
                    Anim.Play(curAnim);
                return;
            }
            else
            {
                if (_GetAnimSuffix(animation) == "END")
                {
                    return;
                }
                if (Anim.HasAnimation(animation + "_END"))
                    Anim.Play(animation + "_END");
                else
                    Anim.Play(curAnim);
                cutoffEn = false;
                return;
            }
        }
    }

    public void ExitElevator(Node levelRoot)
    {
        this.Reparent(levelRoot);
        PCamRef.Reparent(levelRoot);

        Global.i.PlayerRef = this;
    }

    private void _AnimationFinished(StringName animation)
    {
        //GD.Print(_GetAnimSuffix(animation));
        //GD.Print(_GetAnimPrefix(animation));

        if (_GetAnimSuffix(animation) == "END")
        {
            if (Anim.HasAnimation(curAnim + "_INTRO"))
                Anim.Play(curAnim + "_INTRO");
            else
                Anim.Play(curAnim);
            return;
        }
        if (_GetAnimSuffix(animation) == "INTRO")
        {
            Anim.Play(curAnim);
            return;
        }
        Anim.Play(animation);
    }

    /// <summary>
    /// Strip the animation name at the first underscore.
    /// </summary>
    /// <param name="inputStr"></param>
    /// <returns></returns>
    private String _GetAnimPrefix(StringName inputStrname)
    {
        String str = inputStrname.ToString();
        char[] characters = str.ToCharArray();
        int idx;
        for (idx = 0; idx < characters.Length; idx++)
        {
            if (characters[idx] == '_')
            {
                break;
            }
        }
        return str.Substring(0, idx);
    }

    private String _GetAnimSuffix(StringName inputStrname)
    {
        String str = inputStrname.ToString();
        char[] characters = str.ToCharArray();
        int idx;
        for (idx = 0; idx < characters.Length; idx++)
        {
            if (characters[idx] == '_')
            {
                idx += 1;
                break;
            }
        }
        return str.Substring(idx);
    }

}