using System.Threading.Tasks;
using Godot;

namespace CPlatformerController;

/// <summary>
/// PlatformerController2D - A comprehensive 2D platformer character controller for Godot 4.6
/// IMPORTANT: MAKE SURE TO ASSIGN 'left', 'right', 'jump', 'dash', 'up', 'down' in the project settings input map.
/// 
/// Usage tips:
/// 1. Hover over each toggle and variable to read what it does and to make sure nothing bugs.
/// 2. Animations are very primitive. To make full use of your custom art, you may want to slightly change the code for the animations.
/// </summary>
public partial class PlatformerController : CharacterBody2D
{
    #region README and Documentation

    [Export] public string Readme =
        "IMPORTANT: MAKE SURE TO ASSIGN 'left' 'right' 'jump' 'dash' 'up' 'down' in the project settings input map." +
        " Usage tips. 1. Hover over each toggle and variable to read what it does and to make sure nothing bugs. 2." +
        " Animations are very primitive. To make full use of your custom art, you may want to slightly change the " +
        "code for the animations";

    #endregion

    #region Necessary Child Nodes

    [ExportGroup("Necessary Child Nodes")] [Export]
    public AnimatedSprite2D PlayerSprite;

    [Export] public CollisionShape2D PlayerCollider;

    #endregion

    #region Horizontal Movement

    /// <summary>The max speed your player will move</summary>
    [ExportGroup("L & R Movement")] [Export(PropertyHint.Range, "50, 500")]
    public float MaxSpeed = 150.0f;

    /// <summary>How fast your player will reach max speed from rest (in seconds)</summary>
    [Export] public float TimeToReachMaxSpeed = 0.2f;

    /// <summary>How fast your player will reach zero speed from max speed (in seconds)</summary>
    [Export] public float TimeToReachZeroSpeed = 0.2f;

    /// <summary>If true, player will instantly move and switch directions. Overrides the "timeToReach" variables</summary>
    [Export] public bool DirectionalSnap;

    /// <summary>If enabled, the default movement speed will be 1/2 of the maxSpeed and the player must hold a "run" button to accelerate to max speed</summary>
    [Export] public bool RunningModifier;

    #endregion

    #region Jumping and Gravity

    /// <summary>The peak height of your player's jump</summary>
    [ExportGroup("Jumping and Gravity")] [Export(PropertyHint.Range, "0,20")]
    public float JumpHeight = 1.75f;

    /// <summary>How many jumps your character can do before needing to touch the ground again. More than 1 disables jump buffering and coyote time</summary>
    [Export] public int Jumps = 1;

    /// <summary>The strength at which your character will be pulled to the ground</summary>
    [Export] public float GravityScale = 20.0f;

    /// <summary>The fastest your player can fall</summary>
    [Export] public float TerminalVelocity = 500.0f;

    /// <summary>Your player will move this amount faster when falling providing a less floaty jump curve</summary>
    [Export(PropertyHint.Range, "0.5f, 3")]
    public float DescendingGravityFactor = 1.0f;

    /// <summary>When the player releases the jump key while ascending, their vertical velocity will cut in half</summary>
    [Export] public bool ShortHopAkaVariableJumpHeight = true;

    /// <summary>How much extra time (in seconds) your player will be given to jump after falling off an edge</summary>
    [Export(PropertyHint.Range, "0, 0.5f")]
    public float CoyoteTime = 0.2f;

    /// <summary>The window of time (in seconds) that your player can press the jump button before hitting the ground and still have their input registered</summary>
    [Export(PropertyHint.Range, "0, 0.5f")]
    public float JumpBuffering = 0.2f;

    #endregion

    #region Wall Jumping

    /// <summary>Allows your player to jump off of walls</summary>
    [ExportGroup("Wall Jumping")] [Export] public bool WallJump;

    /// <summary>How long the player's movement input will be ignored after wall jumping</summary>
    [Export(PropertyHint.Range, "0, 0.5f")]
    public float InputPauseAfterWallJump = 0.1f;

    /// <summary>The angle at which your player will jump away from the wall. 0 is straight away, 90 is straight up</summary>
    [Export(PropertyHint.Range, "0, 90")] public float WallKickAngle = 60.0f;

    /// <summary>The player's gravity will be divided by this number when touching a wall and descending</summary>
    [Export(PropertyHint.Range, "1,20")] public float WallSliding = 1.0f;

    /// <summary>If enabled, the player's gravity will be set to 0 when touching a wall and descending</summary>
    [Export] public bool WallLatching;

    /// <summary>If enabled, the player must hold down the "latch" key to wall latch</summary>
    [Export] public bool WallLatchingModifer;

    #endregion

    #region Dashing

    /// <summary>The type of dashes the player can do. 0=None, 1=Horizontal, 2=Vertical, 3=Four Way, 4=Eight Way</summary>
    [ExportGroup("Dashing")] [Export] public int DashType;

    /// <summary>How many dashes your player can do before needing to hit the ground</summary>
    [Export] public int Dashes = 1;

    /// <summary>If enabled, pressing the opposite direction of a dash will zero the player's velocity</summary>
    [Export] public bool DashCancel = true;

    /// <summary>How far the player will dash</summary>
    [Export(PropertyHint.Range, "1.5f, 4")]
    public float DashLength = 2.5f;

    #endregion

    #region Corner Cutting

    /// <summary>If the player's head is blocked by a jump but only by a little, the player will be nudged in the right direction</summary>
    [ExportGroup("Corner Cutting/Jump Correct")] [Export]
    public bool CornerCutting;

    /// <summary>How many pixels the player will be pushed (per frame) if corner cutting is needed</summary>
    [Export(PropertyHint.Range, "1, 5")] public float CorrectionAmount = 1.5f;

    /// <summary>Raycast used for corner cutting calculations. Place above and to the left of the player's head</summary>
    [Export] public RayCast2D LeftRaycast;

    /// <summary>Raycast used for corner cutting calculations. Place above the player's head</summary>
    [Export] public RayCast2D MiddleRaycast;

    /// <summary>Raycast used for corner cutting calculations. Place above and to the right of the player's head</summary>
    [Export] public RayCast2D RightRaycast;

    #endregion

    #region Down Input

    /// <summary>Holding down will crouch the player</summary>
    [ExportGroup("Down Input")] [Export] public bool Crouch;

    /// <summary>Holding down and pressing the input for "roll" will execute a roll if the player is grounded</summary>
    [Export] public bool CanRoll;

    /// <summary>How far the player will roll</summary>
    [Export(PropertyHint.Range, "1.25f, 2")]
    public float RollLength = 2.0f;

    /// <summary>If enabled, the player will stop all horizontal movement midair and slam down into the ground when down is pressed</summary>
    [Export] public bool GroundPound;

    /// <summary>The amount of time the player will hover in the air before completing a ground pound (in seconds)</summary>
    [Export(PropertyHint.Range, "0.05f, 0.75f")]
    public float GroundPoundPause = 0.25f;

    /// <summary>If enabled, pressing up will end the ground pound early</summary>
    [Export] public bool UpToCancel;

    #endregion

    #region Animations

    /// <summary>Animations must be named "run" all lowercase</summary>
    [ExportGroup("Animations (Check Box if has animation)")] [Export]
    public bool Run;

    /// <summary>Animations must be named "jump" all lowercase</summary>
    [Export] public bool Jump;

    /// <summary>Animations must be named "idle" all lowercase</summary>
    [Export] public bool Idle;

    /// <summary>Animations must be named "walk" all lowercase</summary>
    [Export] public bool Walk;

    /// <summary>Animations must be named "slide" all lowercase</summary>
    [Export] public bool Slide;

    /// <summary>Animations must be named "latch" all lowercase</summary>
    [Export] public bool Latch;

    /// <summary>Animations must be named "falling" all lowercase</summary>
    [Export] public bool Falling;

    /// <summary>Animations must be named "crouch_idle" all lowercase</summary>
    [Export] public bool CrouchIdle;

    /// <summary>Animations must be named "crouch_walk" all lowercase</summary>
    [Export] public bool CrouchWalk;

    /// <summary>Animations must be named "roll" all lowercase</summary>
    [Export] public bool Roll;

    #endregion

    #region Internal Physics Variables

    private float _appliedGravity;
    private float _maxSpeedLock;
    private float _appliedTerminalVelocity;

    private float _friction;
    private float _acceleration;
    private float _deceleration;
    private bool _instantAccel;
    private bool _instantStop;

    private float _jumpMagnitude = 500.0f;
    private int _jumpCount;
    private bool _jumpWasPressed;
    private bool _coyoteActive;
    private float _dashMagnitude;
    private bool _gravityActive = true;
    private bool _dashing;
    private int _dashCount;
    private bool _rolling;

    private bool _twoWayDashHorizontal;
    private bool _twoWayDashVertical;
    private bool _eightWayDash;

    private bool _wasMovingR;
    private bool _wasPressingR;

    private Vector2
        _movementInputMonitoring = new(1, 1); // .X addresses right direction, .Y addresses left direction

    private float _gdelta = 1.0f;
    private bool _dset;

    private float _colliderScaleLockY;
    private float _colliderPosLockY;

    private bool _latched;
    private bool _wasLatched;
    private bool _crouching;
    private bool _groundPounding;

    private AnimatedSprite2D _anim;
    private CollisionShape2D _col;
    private Vector2 _animScaleLock;

    #endregion

    #region Input Variables

    private bool _upHold;
    private bool _downHold;
    private bool _leftHold;
    private bool _leftTap;
    private bool _leftRelease;
    private bool _rightHold;
    private bool _rightTap;
    private bool _rightRelease;
    private bool _jumpTap;
    private bool _jumpRelease;
    private bool _runHold;
    private bool _latchHold;
    private bool _dashTap;
    private bool _rollTap;
    private bool _downTap;
    private bool _twirlTap;

    #endregion

    public override void _Ready()
    {
        _wasMovingR = true;
        _anim = PlayerSprite;
        _col = PlayerCollider;

        UpdateData();
    }

    private void UpdateData()
    {
        _acceleration = MaxSpeed / TimeToReachMaxSpeed;
        _deceleration = -MaxSpeed / TimeToReachZeroSpeed;

        _jumpMagnitude = 10.0f * JumpHeight * GravityScale;
        _jumpCount = Jumps;

        _dashMagnitude = MaxSpeed * DashLength;
        _dashCount = Dashes;

        _maxSpeedLock = MaxSpeed;

        _animScaleLock = _anim.Scale.Abs();
        _colliderScaleLockY = _col.Scale.Y;
        _colliderPosLockY = _col.Position.Y;

        if (TimeToReachMaxSpeed == 0)
        {
            _instantAccel = true;
            TimeToReachMaxSpeed = 1;
        }
        else if (TimeToReachMaxSpeed < 0)
        {
            TimeToReachMaxSpeed = Mathf.Abs(TimeToReachMaxSpeed);
            _instantAccel = false;
        }
        else
        {
            _instantAccel = false;
        }

        if (TimeToReachZeroSpeed == 0)
        {
            _instantStop = true;
            TimeToReachZeroSpeed = 1;
        }
        else if (TimeToReachMaxSpeed < 0)
        {
            TimeToReachMaxSpeed = Mathf.Abs(TimeToReachMaxSpeed);
            _instantStop = false;
        }
        else
        {
            _instantStop = false;
        }

        if (Jumps > 1)
        {
            JumpBuffering = 0;
            CoyoteTime = 0;
        }

        CoyoteTime = Mathf.Abs(CoyoteTime);
        JumpBuffering = Mathf.Abs(JumpBuffering);

        if (DirectionalSnap)
        {
            _instantAccel = true;
            _instantStop = true;
        }

        _twoWayDashHorizontal = false;
        _twoWayDashVertical = false;
        _eightWayDash = false;

        switch (DashType)
        {
            case 0:
                break;
            case 1:
                _twoWayDashHorizontal = true;
                break;
            case 2:
                _twoWayDashVertical = true;
                break;
            case 3:
                _twoWayDashHorizontal = true;
                _twoWayDashVertical = true;
                break;
            case 4:
                _eightWayDash = true;
                break;
        }
    }

    public override void _Process(double delta)
    {
        // Wall latching and animation directions
        if (IsOnWall() && !IsOnFloor() && Latch && WallLatching &&
            ((WallLatchingModifer && _latchHold) || !WallLatchingModifer))
        {
            _latched = true;
        }
        else
        {
            _latched = false;
            _wasLatched = true;
            SetLatch(0.2f, false);
        }

        if (_rightHold && !_latched)
        {
            _anim.Scale = new Vector2(_animScaleLock.X, _anim.Scale.Y);
        }

        if (_leftHold && !_latched)
        {
            _anim.Scale = new Vector2(_animScaleLock.X * -1, _anim.Scale.Y);
        }

        // Run animation
        if (Run && Idle && !_dashing && !_crouching)
        {
            if (Mathf.Abs(Velocity.X) > 0.1f && IsOnFloor() && !IsOnWall())
            {
                _anim.SpeedScale = Mathf.Abs(Velocity.X / 150);
                _anim.Play("run");
            }
            else if (Mathf.Abs(Velocity.X) < 0.1f && IsOnFloor())
            {
                _anim.SpeedScale = 1;
                _anim.Play("idle");
            }
        }
        else if (Run && Idle && Walk && !_dashing && !_crouching)
        {
            if (Mathf.Abs(Velocity.X) > 0.1f && IsOnFloor() && !IsOnWall())
            {
                _anim.SpeedScale = Mathf.Abs(Velocity.X / 150);
                if (Mathf.Abs(Velocity.X) < _maxSpeedLock)
                {
                    _anim.Play("walk");
                }
                else
                {
                    _anim.Play("run");
                }
            }
            else if (Mathf.Abs(Velocity.X) < 0.1f && IsOnFloor())
            {
                _anim.SpeedScale = 1;
                _anim.Play("idle");
            }
        }

        // Jump animation
        if (Velocity.Y < 0 && Jump && !_dashing)
        {
            _anim.SpeedScale = 1;
            _anim.Play("jump");
        }

        if (Velocity.Y > 40 && Falling && !_dashing && !_crouching)
        {
            _anim.SpeedScale = 1;
            _anim.Play("falling");
        }

        // Wall slide, latch, dash, crouch, roll animations
        if (Latch && Slide)
        {
            if (_latched && !_wasLatched)
            {
                _anim.SpeedScale = 1;
                _anim.Play("latch");
            }

            if (IsOnWall() && Velocity.Y > 0 && Slide && /*anim.CurrentAnimation != "slide" &&*/ WallSliding != 1)
            {
                _anim.SpeedScale = 1;
                _anim.Play("slide");
            }

            if (_dashing)
            {
                _anim.SpeedScale = 1;
                _anim.Play("dash");
            }

            if (_crouching && !_rolling)
            {
                if (Mathf.Abs(Velocity.X) > 10)
                {
                    _anim.SpeedScale = 1;
                    _anim.Play("crouch_walk");
                }
                else
                {
                    _anim.SpeedScale = 1;
                    _anim.Play("crouch_idle");
                }
            }

            if (_rollTap && CanRoll && Roll)
            {
                _anim.SpeedScale = 1;
                _anim.Play("roll");
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        var fDelta = (float)delta;

        if (!_dset)
        {
            _gdelta = fDelta;
            _dset = true;
        }

        // Input Detection
        _leftHold = Input.IsActionPressed("left");
        _rightHold = Input.IsActionPressed("right");
        _upHold = Input.IsActionPressed("up");
        _downHold = Input.IsActionPressed("down");
        _leftTap = Input.IsActionJustPressed("left");
        _rightTap = Input.IsActionJustPressed("right");
        _leftRelease = Input.IsActionJustReleased("left");
        _rightRelease = Input.IsActionJustReleased("right");
        _jumpTap = Input.IsActionJustPressed("jump");
        _jumpRelease = Input.IsActionJustReleased("jump");
        _runHold = Input.IsActionPressed("run");
        _latchHold = Input.IsActionPressed("latch");
        _dashTap = Input.IsActionJustPressed("dash");
        _rollTap = Input.IsActionJustPressed("roll");
        _downTap = Input.IsActionJustPressed("down");
        _twirlTap = Input.IsActionJustPressed("twirl");

        // Left and Right Movement
        if (_rightHold && _leftHold && _movementInputMonitoring != Vector2.Zero)
        {
            if (!_instantStop)
            {
                Decelerate(fDelta, false);
            }
            else
            {
                Velocity = Velocity with { X = -0.1f };
            }
        }
        else if (_rightHold && _movementInputMonitoring.X != 0)
        {
            if (Velocity.X > MaxSpeed || _instantAccel)
            {
                Velocity = Velocity with { X = MaxSpeed };
            }
            else
            {
                Velocity = Velocity with { X = Velocity.X + _acceleration * fDelta };
            }

            if (Velocity.X < 0)
            {
                if (!_instantStop)
                {
                    Decelerate(fDelta, false);
                }
                else
                {
                    Velocity = Velocity with { X = -0.1f };
                }
            }
        }
        else if (_leftHold && _movementInputMonitoring.Y != 0)
        {
            if (Velocity.X < -MaxSpeed || _instantAccel)
            {
                Velocity = Velocity with { X = -MaxSpeed };
            }
            else
            {
                Velocity = Velocity with { X = Velocity.X - _acceleration * fDelta };
            }

            if (Velocity.X > 0)
            {
                if (!_instantStop)
                {
                    Decelerate(fDelta, false);
                }
                else
                {
                    Velocity = Velocity with { X = 0.1f };
                }
            }
        }

        if (Velocity.X > 0)
        {
            _wasMovingR = true;
        }
        else if (Velocity.X < 0)
        {
            _wasMovingR = false;
        }

        if (_rightTap)
        {
            _wasPressingR = true;
        }

        if (_leftTap)
        {
            _wasPressingR = false;
        }

        if (RunningModifier && !_runHold)
        {
            MaxSpeed = _maxSpeedLock / 2;
        }
        else if (IsOnFloor())
        {
            MaxSpeed = _maxSpeedLock;
        }

        if (!(_leftHold || _rightHold))
        {
            if (!_instantStop)
            {
                Decelerate(fDelta, false);
            }
            else
            {
                Velocity = Velocity with { X = 0 };
            }
        }

        // Crouching
        if (Crouch)
        {
            if (_downHold && IsOnFloor())
            {
                _crouching = true;
            }
            else if (!_downHold && ((_runHold && RunningModifier) || !RunningModifier) && !_rolling)
            {
                _crouching = false;
            }
        }

        if (!IsOnFloor())
        {
            _crouching = false;
        }

        if (_crouching)
        {
            MaxSpeed = _maxSpeedLock / 2;
            _col.Scale = new Vector2(_col.Scale.X, _colliderScaleLockY / 2);
            _col.Position = new Vector2(_col.Position.X, _colliderPosLockY + 8 * _colliderScaleLockY);
        }
        else
        {
            MaxSpeed = _maxSpeedLock;
            _col.Scale = new Vector2(_col.Scale.X, _colliderScaleLockY);
            _col.Position = new Vector2(_col.Position.X, _colliderPosLockY);
        }

        // Rolling
        if (CanRoll && IsOnFloor() && _rollTap && _crouching)
        {
            RollingTime(0.75f);
            if (_wasPressingR && !_upHold)
            {
                Velocity = Velocity with { Y = 0 };
                Velocity = Velocity with { X = _maxSpeedLock * RollLength };
                _dashCount += -1;
                _movementInputMonitoring = Vector2.Zero;
                InputPauseReset(RollLength * 0.0625f);
            }
            else if (!_upHold)
            {
                Velocity = Velocity with { Y = 0 };
                Velocity = Velocity with { X = -_maxSpeedLock * RollLength };
                _dashCount += -1;
                _movementInputMonitoring = Vector2.Zero;
                InputPauseReset(RollLength * 0.0625f);
            }
        }

        if (CanRoll && _rolling)
        {
            // Add immunity or other effects here
        }

        // Jump and Gravity
        if (Velocity.Y > 0)
        {
            _appliedGravity = GravityScale * DescendingGravityFactor;
        }
        else
        {
            _appliedGravity = GravityScale;
        }

        if (IsOnWall() && !_groundPounding)
        {
            _appliedTerminalVelocity = TerminalVelocity / WallSliding;
            if (WallLatching && ((WallLatchingModifer && _latchHold) || !WallLatchingModifer))
            {
                _appliedGravity = 0;

                if (Velocity.Y < 0)
                {
                    Velocity = Velocity with { Y = Velocity.Y + 50 };
                }

                if (Velocity.Y > 0)
                {
                    Velocity = Velocity with { Y = 0 };
                }

                if (WallLatchingModifer && _latchHold && _movementInputMonitoring == Vector2.Zero)
                {
                    Velocity = Velocity with { X = 0 };
                }
            }
            else if (WallSliding != 1 && Velocity.Y > 0)
            {
                _appliedGravity = _appliedGravity / WallSliding;
            }
        }
        else if (!IsOnWall() && !_groundPounding)
        {
            _appliedTerminalVelocity = TerminalVelocity;
        }

        if (_gravityActive)
        {
            if (Velocity.Y < _appliedTerminalVelocity)
            {
                Velocity = Velocity with { Y = Velocity.Y + _appliedGravity };
            }
            else if (Velocity.Y > _appliedTerminalVelocity)
            {
                Velocity = Velocity with { Y = _appliedTerminalVelocity };
            }
        }

        if (ShortHopAkaVariableJumpHeight && _jumpRelease && Velocity.Y < 0)
        {
            Velocity = Velocity with { Y = Velocity.Y / 2 };
        }

        // Jump logic
        if (Jumps == 1)
        {
            if (!IsOnFloor() && !IsOnWall())
            {
                if (CoyoteTime > 0)
                {
                    _coyoteActive = true;
                    HandleCoyoteTime();
                }
            }

            if (_jumpTap && !IsOnWall())
            {
                if (_coyoteActive)
                {
                    _coyoteActive = false;
                    HandleJump();
                }

                if (JumpBuffering > 0)
                {
                    _jumpWasPressed = true;
                    BufferJump();
                }
                else if (JumpBuffering == 0 && CoyoteTime == 0 && IsOnFloor())
                {
                    HandleJump();
                }
            }
            else if (_jumpTap && IsOnWall() && !IsOnFloor())
            {
                if (WallJump && !_latched)
                {
                    HandleWallJump();
                }
                else if (WallJump && _latched)
                {
                    HandleWallJump();
                }
            }
            else if (_jumpTap && IsOnFloor())
            {
                HandleJump();
            }

            if (IsOnFloor())
            {
                _jumpCount = Jumps;
                _coyoteActive = true;
                if (_jumpWasPressed)
                {
                    HandleJump();
                }
            }
        }
        else if (Jumps > 1)
        {
            if (IsOnFloor())
            {
                _jumpCount = Jumps;
            }

            if (_jumpTap && _jumpCount > 0 && !IsOnWall())
            {
                Velocity = Velocity with { Y = -_jumpMagnitude };
                _jumpCount -= 1;
                EndGroundPound();
            }
            else if (_jumpTap && IsOnWall() && WallJump)
            {
                HandleWallJump();
            }
        }

        // Dashing
        if (IsOnFloor())
        {
            _dashCount = Dashes;
        }

        if (_eightWayDash && _dashTap && _dashCount > 0 && !_rolling)
        {
            var inputDirection = Input.GetVector("left", "right", "up", "down");
            var dTime = 0.0625f * DashLength;
            DashingTime(dTime);
            PauseGravity(dTime);
            Velocity = _dashMagnitude * inputDirection;
            _dashCount += -1;
            _movementInputMonitoring = Vector2.Zero;
            InputPauseReset(dTime);
        }

        if (_twoWayDashVertical && _dashTap && _dashCount > 0 && !_rolling)
        {
            var dTime = 0.0625f * DashLength;

            if (_upHold && _downHold)
            {
                PlaceHolder();
            }
            else if (_upHold)
            {
                DashingTime(dTime);
                PauseGravity(dTime);
                Velocity = Velocity with { X = 0 };
                Velocity = Velocity with { Y = -_dashMagnitude };
                _dashCount += -1;
                _movementInputMonitoring = Vector2.Zero;
                InputPauseReset(dTime);
            }
            else if (_downHold && _dashCount > 0)
            {
                DashingTime(dTime);
                PauseGravity(dTime);
                Velocity = Velocity with { X = 0 };
                Velocity = Velocity with { Y = _dashMagnitude };
                _dashCount += -1;
                _movementInputMonitoring = Vector2.Zero;
                InputPauseReset(dTime);
            }
        }

        if (_twoWayDashHorizontal && _dashTap && _dashCount > 0 && !_rolling)
        {
            var dTime = 0.0625f * DashLength;
            if (_wasPressingR && !(_upHold || _downHold))
            {
                Velocity = Velocity with { Y = 0 };
                Velocity = Velocity with { X = _dashMagnitude };
                PauseGravity(dTime);
                DashingTime(dTime);
                _dashCount += -1;
                _movementInputMonitoring = Vector2.Zero;
                InputPauseReset(dTime);
            }
            else if (!(_upHold || _downHold))
            {
                Velocity = Velocity with { Y = 0 };
                Velocity = Velocity with { X = -_dashMagnitude };
                PauseGravity(dTime);
                DashingTime(dTime);
                _dashCount += -1;
                _movementInputMonitoring = Vector2.Zero;
                InputPauseReset(dTime);
            }
        }

        if (_dashing && Velocity.X > 0 && _leftTap && DashCancel)
        {
            Velocity = Velocity with { X = 0 };
        }

        if (_dashing && Velocity.X < 0 && _rightTap && DashCancel)
        {
            Velocity = Velocity with { X = 0 };
        }

        // Corner Cutting
        if (CornerCutting)
        {
            if (Velocity.Y < 0 && LeftRaycast.IsColliding() && !RightRaycast.IsColliding() &&
                !MiddleRaycast.IsColliding())
            {
                Position = new Vector2(Position.X + CorrectionAmount, Position.Y);
            }

            if (Velocity.Y < 0 && !LeftRaycast.IsColliding() && RightRaycast.IsColliding() &&
                !MiddleRaycast.IsColliding())
            {
                Position = new Vector2(Position.X - CorrectionAmount, Position.Y);
            }
        }

        // Ground Pound
        if (GroundPound && _downTap && !IsOnFloor() && !IsOnWall())
        {
            _groundPounding = true;
            _gravityActive = false;
            Velocity = Velocity with { Y = 0 };
            GroundPoundAsync();
        }

        if (IsOnFloor() && _groundPounding)
        {
            EndGroundPound();
        }

        MoveAndSlide();

        if (UpToCancel && _upHold && GroundPound)
        {
            EndGroundPound();
        }
    }

    // Async helper methods
    private async void GroundPoundAsync()
    {
        await Task.Delay((int)(GroundPoundPause * 1000));
        HandleGroundPound();
    }

    private async void BufferJump()
    {
        await Task.Delay((int)(JumpBuffering * 1000));
        _jumpWasPressed = false;
    }

    private async void HandleCoyoteTime()
    {
        await Task.Delay((int)(CoyoteTime * 1000));
        _coyoteActive = false;
        _jumpCount += -1;
    }

    private void HandleJump()
    {
        if (_jumpCount > 0)
        {
            Velocity = Velocity with { Y = -_jumpMagnitude };
            _jumpCount += -1;
            _jumpWasPressed = false;
        }
    }

    private void HandleWallJump()
    {
        var horizontalWallKick = Mathf.Abs(_jumpMagnitude * Mathf.Cos(WallKickAngle * (Mathf.Pi / 180)));
        var verticalWallKick = Mathf.Abs(_jumpMagnitude * Mathf.Sin(WallKickAngle * (Mathf.Pi / 180)));
        Velocity = Velocity with { Y = -verticalWallKick };

        var dir = 1;
        if (WallLatchingModifer && _latchHold)
        {
            dir = -1;
        }

        if (_wasMovingR)
        {
            Velocity = Velocity with { X = -horizontalWallKick * dir };
        }
        else
        {
            Velocity = Velocity with { X = horizontalWallKick * dir };
        }

        if (InputPauseAfterWallJump != 0)
        {
            _movementInputMonitoring = Vector2.Zero;
            InputPauseReset(InputPauseAfterWallJump);
        }
    }

    private async void SetLatch(float delay, bool setBool)
    {
        await Task.Delay((int)(delay * 1000));
        _wasLatched = setBool;
    }

    private async void InputPauseReset(float time)
    {
        await Task.Delay((int)(time * 1000));
        _movementInputMonitoring = new Vector2(1, 1);
    }

    private void Decelerate(float delta, bool vertical)
    {
        if (!vertical)
        {
            if (Velocity.X > 0)
            {
                Velocity = Velocity with { X = Velocity.X + _deceleration * delta };
            }
            else if (Velocity.X < 0)
            {
                Velocity = Velocity with { X = Velocity.X - _deceleration * delta };
            }
        }
        else if (Velocity.Y > 0)
        {
            Velocity = Velocity with { Y = Velocity.Y + _deceleration * delta };
        }
    }

    private async void PauseGravity(float time)
    {
        _gravityActive = false;
        await Task.Delay((int)(time * 1000));
        _gravityActive = true;
    }

    private async void DashingTime(float time)
    {
        _dashing = true;
        await Task.Delay((int)(time * 1000));
        _dashing = false;
    }

    private async void RollingTime(float time)
    {
        _rolling = true;
        await Task.Delay((int)(time * 1000));
        _rolling = false;
    }

    private void HandleGroundPound()
    {
        _appliedTerminalVelocity = TerminalVelocity * 10;
        Velocity = Velocity with { Y = _jumpMagnitude * 2 };
    }

    private void EndGroundPound()
    {
        _groundPounding = false;
        _appliedTerminalVelocity = TerminalVelocity;
        _gravityActive = true;
    }

    private void PlaceHolder()
    {
        GD.Print("");
    }
}