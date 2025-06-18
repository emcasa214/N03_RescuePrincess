using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public PlayerDataWithDash Data;
    public static PlayerMovement Instance { get; private set; }
    private PlayerControl playerControls;
    private PlayerSound playerSound;

    #region COMPONENTS
    [SerializeField] public Rigidbody2D RB;
    [SerializeField] private Animator playeranim;
    [SerializeField] private GameObject shadowDash;
    [SerializeField] private ParticleSystem ImpactEffect;
    [SerializeField] private SpriteRenderer sr;

    [SerializeField] private Camera mainCamera;
    #endregion

    #region STATE PARAMETERS
    public bool IsFacingRight { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsWallJumping { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsSliding { get; private set; }

    private bool _isLeftPressed;
    private bool _isRightPressed;
    private int _lastPressedDirection;

    public float LastOnGroundTime { get; private set; }
    public float LastOnWallTime { get; private set; }
    public float LastOnWallRightTime { get; private set; }
    public float LastOnWallLeftTime { get; private set; }

    private bool _isJumpCut;
    private bool _isJumpFalling;

    private float _wallJumpStartTime;
    private int _lastWallJumpDir;

    private float _dashMoveInputBufferTime;
    private int _dashesLeft;
    private bool _dashRefilling;
    private Vector2 _lastDashDir;
    private bool _isDashAttacking;
    private Coroutine dashEffectCoroutine;

    private bool _isFacingLocked;
    public int facingLockFrames;
    private int _facingLockFrames;

    // Thêm biến để lưu trạng thái chạm đất của khung hình trước
    private bool _wasOnGroundLastFrame;
    #endregion

    #region INPUT PARAMETERS
    private Vector2 _moveInput;
    private bool isJumpPressed;
    private bool isDashPressed;
    private bool isLeftressed;
    private bool isRightPressed;
    private bool isUpPressed;
    private bool isDowmPressed;

    public float LastPressedJumpTime { get; private set; }
    public float LastPressedDashTime { get; private set; }
    #endregion

    #region CHECK PARAMETERS
    [Header("Checks")]
    [SerializeField] public Transform _groundCheckPoint;
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
    [Space(5)]
    [SerializeField] private Transform _frontWallCheckPoint;
    [SerializeField] private Transform _backWallCheckPoint;
    [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);
    #endregion

    #region LAYERS & TAGS
    [Header("Layers & Tags")]
    [SerializeField] private LayerMask _groundLayer;
    #endregion

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        playerSound = GetComponent<PlayerSound>();

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Hủy nếu đã có instance khác
        }
        playerControls = new PlayerControl();
        playerControls.Enable();
    }



    private void OnDestroy()
    {
        playerControls.Disable();
    }
    private void Start()
    {
        SetGravityScale(Data.gravityScale);
        IsFacingRight = true;
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    public void OnJump(InputAction.CallbackContext ctxt)
    {
        if (ctxt.started)
        {
            OnJumpInput();
        }
        if (ctxt.canceled)
        {
            OnJumpUpInput();
        } 
    }

    public void OnDash(InputAction.CallbackContext ctxt)
    {
        if (ctxt.started)
        {
            OnDashInput();
        }
    }
    public void OnLeft(InputAction.CallbackContext ctxt)
    {
        if (ctxt.started)
        {
            isLeftressed = true;
        }
        if (ctxt.canceled)
        {
            isLeftressed = false;
        } 
    }

    public void OnRight(InputAction.CallbackContext ctxt)
    {
        if (ctxt.started)
        {
            isRightPressed = true;
        }
        if (ctxt.canceled)
        {
            isRightPressed = false;
        }
    }

    public void OnUp(InputAction.CallbackContext ctxt)
    {
        if (ctxt.started)
        {
            isUpPressed = true;
        }
        if (ctxt.canceled)
        {
            isUpPressed = false;
        } 
    }

    public void OnDown(InputAction.CallbackContext ctxt)
    {
        if (ctxt.started)
        {
            isDowmPressed = true;
        }
        if (ctxt.canceled)
        {
            isDowmPressed = false;
        } 
    }
    private void Update()
    {
        #region TIMERS
        LastOnGroundTime -= Time.deltaTime;
        LastOnWallTime -= Time.deltaTime;
        LastOnWallRightTime -= Time.deltaTime;
        LastOnWallLeftTime -= Time.deltaTime;

        LastPressedJumpTime -= Time.deltaTime;
        LastPressedDashTime -= Time.deltaTime;
        #endregion

        #region INPUT HANDLER
        bool leftPressed = isLeftressed;
        bool rightPressed = isRightPressed;

        if (leftPressed && !_isLeftPressed)
        {
            _lastPressedDirection = -1;
        }
        if (rightPressed && !_isRightPressed)
        {
            _lastPressedDirection = 1;
        }

        if (!leftPressed && _isLeftPressed)
        {
            if (_lastPressedDirection == -1)
            {
                _lastPressedDirection = rightPressed ? 1 : 0;
            }
        }
        if (!rightPressed && _isRightPressed)
        {
            if (_lastPressedDirection == 1)
            {
                _lastPressedDirection = leftPressed ? -1 : 0;
            }
        }

        _isLeftPressed = leftPressed;
        _isRightPressed = rightPressed;

        float verticalInput = 0f;
        if (isUpPressed)
        {
            verticalInput = 1f;
        }
        else if (isDowmPressed)
        {
            verticalInput = -1f;
        }

        _moveInput.x = _lastPressedDirection;
        _moveInput.y = verticalInput;

        if (_moveInput.x != 0)
            CheckDirectionToFace(_moveInput.x > 0);
        #endregion

        #region COLLISION CHECKS
        if (!IsDashing && !IsJumping)
        {
            if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer) && !IsJumping)
            {
                LastOnGroundTime = Data.coyoteTime;
            }

            if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)
                    || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)) && !IsWallJumping)
                LastOnWallRightTime = Data.coyoteTime;

            if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)
                || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)) && !IsWallJumping)
                LastOnWallLeftTime = Data.coyoteTime;

            LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
        }
        #endregion

        #region JUMP CHECKS
        if (IsJumping && RB.velocity.y < 0)
        {
            IsJumping = false;
            if (!IsWallJumping)
                _isJumpFalling = true;
        }

        if (IsWallJumping && Time.time - _wallJumpStartTime > Data.wallJumpTime)
        {
            IsWallJumping = false;
        }

        if (LastOnGroundTime > 0 && !IsJumping && !IsWallJumping)
        {
            _isJumpCut = false;
            if (!IsJumping)
                _isJumpFalling = false;
        }

        if (!IsDashing)
        {
            if (CanJump() && LastPressedJumpTime > 0)
            {
                IsJumping = true;
                IsWallJumping = false;
                _isJumpCut = false;
                _isJumpFalling = false;
                Jump();
            }
            else if (CanWallJump() && LastPressedJumpTime > 0)
            {
                IsWallJumping = true;
                IsJumping = false;
                _isJumpCut = false;
                _isJumpFalling = false;
                _wallJumpStartTime = Time.time;
                _lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1;
                WallJump(_lastWallJumpDir);
            }
        }
        #endregion

        #region DASH CHECKS
        if (CanDash() && LastPressedDashTime > 0)
        {
            Sleep(Data.dashSleepTime);
            _dashMoveInputBufferTime = Data.dashInputBufferTime; // Bắt đầu thời gian chờ input di chuyển
            if (_moveInput != Vector2.zero)
            {
                // Ưu tiên hướng chéo nếu cả hai trục được nhấn
                _lastDashDir = _moveInput.normalized; // Chuẩn hóa để đảm bảo hướng chéo
            }
            else
            {
                _lastDashDir = IsFacingRight ? Vector2.right : Vector2.left; // Mặc định theo hướng nhân vật
            }

            IsDashing = true;
            IsJumping = false;
            IsWallJumping = false;
            _isJumpCut = false;
            StartCoroutine(nameof(StartDash), _lastDashDir);
        }

        // Cập nhật hướng dash trong thời gian buffer
        if (_dashMoveInputBufferTime > 0)
        {
            _dashMoveInputBufferTime -= Time.deltaTime;
            if (_moveInput != Vector2.zero)
            {
                _lastDashDir = _moveInput.normalized; // Cập nhật hướng dựa trên input mới, ưu tiên chéo
            }
        }
        #endregion

        #region SLIDE CHECKS
        if (CanSlide() && ((LastOnWallLeftTime > 0 && _moveInput.x < 0) || (LastOnWallRightTime > 0 && _moveInput.x > 0)))
            IsSliding = true;
        else
            IsSliding = false;
        #endregion

        #region GRAVITY
        if (!_isDashAttacking)
        {
            if (IsSliding)
            {
                SetGravityScale(0);
            }
            else if (RB.velocity.y < 0 && _moveInput.y < 0)
            {
                SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
                RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFastFallSpeed));
            }
            else if (_isJumpCut)
            {
                SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
                RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
            }
            else if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
            {
                SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
            }
            else if (RB.velocity.y < 0)
            {
                SetGravityScale(Data.gravityScale * Data.fallGravityMult);
                RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
            }
            else
            {
                SetGravityScale(Data.gravityScale);
            }
        }
        else
        {
            SetGravityScale(0);
        }
        #endregion

        #region Fall and Land Animation
        // Kiểm tra trạng thái "vừa chạm đất"
        if (!_wasOnGroundLastFrame && LastOnGroundTime > 0)
        {
            playeranim.SetTrigger("land"); // Kích hoạt animation "land" khi vừa chạm đấp
            playerSound.PlayLand();
            PlayImpactEffect();
        }

        if (!_wasOnGroundLastFrame && LastOnGroundTime == 0)
        {
            playeranim.SetBool("fall", true);
        }
        else
        {
            playeranim.SetBool("fall", false);
        }

        if (LastOnGroundTime > 0)
        {
            playeranim.SetBool("ground", true);
        }
        else
        {
            playeranim.SetBool("ground", false);
        }

        if (LastOnGroundTime <= 0 && RB.velocity.y < 0)
        {
            playeranim.SetBool("fall", true);
        }
        else
        {
            playeranim.SetBool("fall", false);
        }

        // Lưu trạng thái chạm đất cho khung hình tiếp theo
        _wasOnGroundLastFrame = LastOnGroundTime > 0;
        #endregion

    }

    private void FixedUpdate()
    {
        if (!IsDashing)
        {
            if (IsWallJumping)
                Run(Data.wallJumpRunLerp);
            else
                Run(1);
        }
        else if (_isDashAttacking)
        {
            Run(Data.dashEndRunLerp);
        }

        if (IsSliding)
            Slide();

        if (_isFacingLocked)
        {
            _facingLockFrames--;
            if (_facingLockFrames <= 0)
            {
                _isFacingLocked = false;
            }
        }
    }

    #region INPUT CALLBACKS
    public void OnJumpInput()
    {
        LastPressedJumpTime = Data.jumpInputBufferTime;
    }

    public void OnJumpUpInput()
    {
        if (CanJumpCut() || CanWallJumpCut())
            _isJumpCut = true;
    }

    public void OnDashInput()
    {
        LastPressedDashTime = Data.dashInputBufferTime;
    }
    #endregion

    #region GENERAL METHODS
    public void SetGravityScale(float scale)
    {
        RB.gravityScale = scale;
    }

    private void Sleep(float duration)
    {
        StartCoroutine(nameof(PerformSleep), duration);
    }

    private IEnumerator PerformSleep(float duration)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
    }
    #endregion

    #region RUN METHODS
    private void Run(float lerpAmount)
    {
        float targetSpeed = _moveInput.x * Data.runMaxSpeed;
        targetSpeed = Mathf.Lerp(RB.velocity.x, targetSpeed, lerpAmount);
        if (targetSpeed != 0)
        {
            playeranim.SetBool("run", true);
        }
        else
        {
            playeranim.SetBool("run", false);
        }

        float accelRate;

        if (LastOnGroundTime > 0)
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;

        if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
        {
            accelRate *= Data.jumpHangAccelerationMult;
            targetSpeed *= Data.jumpHangMaxSpeedMult;
        }

        if (Data.doConserveMomentum && Mathf.Abs(RB.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(RB.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
        {
            accelRate = 0;
        }

        float speedDif = targetSpeed - RB.velocity.x;
        float movement = speedDif * accelRate;

        RB.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    private void Turn()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        IsFacingRight = !IsFacingRight;
    }
    #endregion

    #region JUMP METHODS

    public void JumpPadState()
    {
        IsJumping = true;
        _isJumpCut = false;
        _isJumpFalling = false;
        playeranim.SetTrigger("jump");
        playerSound.PlayJump();
    }
    private void Jump()
    {
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;

        float force = Data.jumpForce;
        if (RB.velocity.y < 0)
            force -= RB.velocity.y;
        playeranim.SetTrigger("jump");
        playerSound.PlayJump();
        RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    private void WallJump(int dir)
    {
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;
        LastOnWallRightTime = 0;
        LastOnWallLeftTime = 0;

        bool shouldFaceRight = dir > 0;
        if (shouldFaceRight != IsFacingRight)
        {
            Turn();
        }

        _isFacingLocked = true;
        _facingLockFrames = facingLockFrames;

        Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);
        force.x *= dir;

        if (Mathf.Sign(RB.velocity.x) != Mathf.Sign(force.x))
            force.x -= RB.velocity.x;

        if (RB.velocity.y < 0)
            force.y -= RB.velocity.y;
        playeranim.SetTrigger("jump");
        playerSound.PlayJump();
        PlayImpactEffect();
        RB.AddForce(force, ForceMode2D.Impulse);
    }

    //landed effect
    void PlayImpactEffect()
    {
        ImpactEffect.Play();
        
    }

    #endregion

    #region DASH METHODS
    private IEnumerator StartDash(Vector2 dir)
    {
        LastOnGroundTime = 0;
        LastPressedDashTime = 0;

        float startTime = Time.time;

        _dashesLeft--;
        _isDashAttacking = true;
        StartDashEffect();
        playerSound.PlayDash();
        SetGravityScale(0);
        while (Time.time - startTime <= Data.dashAttackTime)
        {
            RB.velocity = _lastDashDir.normalized * Data.dashSpeed; // Sử dụng _lastDashDir
            yield return null;
        }

        startTime = Time.time;

        _isDashAttacking = false;

        SetGravityScale(Data.gravityScale);
        RB.velocity = Data.dashEndSpeed * _lastDashDir.normalized; // Sử dụng _lastDashDir

        while (Time.time - startTime <= Data.dashEndTime)
        {
            yield return null;
        }
        if (dashEffectCoroutine != null) StopCoroutine(dashEffectCoroutine);

        IsDashing = false;
    }

    private IEnumerator RefillDash(int amount)
    {
        _dashRefilling = true;
        yield return new WaitForSeconds(Data.dashRefillTime);
        _dashRefilling = false;
        _dashesLeft = Mathf.Min(Data.dashAmount, _dashesLeft + 1);
    }

    private void StartDashEffect()
    {
        if (dashEffectCoroutine != null) StopCoroutine(dashEffectCoroutine);
        dashEffectCoroutine = StartCoroutine(DashEffectCoroutine());
    }

    private IEnumerator DashEffectCoroutine()
    {
        // FindObjectOfType<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));
        RippleEffect.Instance.Emit(Camera.main.WorldToViewportPoint(transform.position));
        GetComponent<Cinemachine.CinemachineImpulseSource>().GenerateImpulse(_lastDashDir.normalized * 0.15f);
        while (true)
        {
            GameObject shadow = Instantiate(shadowDash, transform.position, transform.rotation);
            shadow.transform.localScale = transform.localScale;
            shadow.GetComponent<SpriteRenderer>().sprite = sr.sprite;
            Destroy(shadow, 0.5f);

            yield return new WaitForSeconds(Data.shadowDashDelaySecond);
        }
    }
    #endregion

    #region OTHER MOVEMENT METHODS
    private void Slide()
    {
        float speedDif = Data.slideSpeed - RB.velocity.y;
        float movement = speedDif * Data.slideAccel;
        movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

        RB.AddForce(movement * Vector2.up);
    }
    #endregion

    #region CHECK METHODS
    public void CheckDirectionToFace(bool isMovingRight)
    {
        if (_isFacingLocked)
            return;

        if (isMovingRight != IsFacingRight)
            Turn();
    }

    private bool CanJump()
    {
        return LastOnGroundTime > 0 && !IsJumping;
    }

    private bool CanWallJump()
    {
        return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && (!IsWallJumping ||
                (LastOnWallRightTime > 0 && _lastWallJumpDir == 1) || (LastOnWallLeftTime > 0 && _lastWallJumpDir == -1));
    }

    private bool CanJumpCut()
    {
        return IsJumping && RB.velocity.y > 0;
    }

    private bool CanWallJumpCut()
    {
        return IsWallJumping && RB.velocity.y > 0;
    }

    private bool CanDash()
    {
        if (!IsDashing && _dashesLeft < Data.dashAmount && LastOnGroundTime > 0 && !_dashRefilling)
        {
            StartCoroutine(nameof(RefillDash), 1);
        }

        return _dashesLeft > 0;
    }

    public bool CanSlide()
    {
        if (LastOnWallTime > 0 && !IsJumping && !IsWallJumping && !IsDashing && LastOnGroundTime <= 0)
            return true;
        else
            return false;
    }
    #endregion

    #region EDITOR METHODS
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
        Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
    }
    #endregion
}