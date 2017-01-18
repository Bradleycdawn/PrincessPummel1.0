using UnityEngine;
using System.Collections.Generic;
using Prime31;

//This will tell Unity that these components are required for this script to function properly.
[RequireComponent(typeof(CharacterController2D), typeof(AnimationController2D))]
public class EnemyController : MonoBehaviour
{

    public enum enemyType
    {
        NONE,
        TURRET,
        GROUND,
        FLYING,
        DROP,
        TURRET_TRACKING,
        GROUND_TRACKING,
        FLYING_TRACKING,
        DROP_TRACKING,
    }

    //Region of local variables.
    #region Local_Vars
    public enemyType enemy_type = enemyType.NONE;
    public float moveSpeed = 2;
    public float moveDistance = 4;
    public float gravity = -9.8f;
    //TODO: Implement jumping enemies if we want them.
    //public float jumpHeight = 4;
    //public bool canDoubleJump = false;
    public bool canShoot = false;
    public GameObject fireballPrefab;
    public float shootInterval = 1;
    public float projectileSpeed = 4;
    public float projectileDuration = 2;
	[Range(0, 10)]
	public float projectilePositionX = 1;
	[Range(-10, 10)]
	public float projectilePositionY = 0;
    public int health = 100;
    public float knockbackDistance = 1;
    public GameObject player;
    public float detectionRange = 5;
    public float stoppingRange = 0.5f;

    private CharacterController2D _controller;
    private AnimationController2D _animator;
    private SpriteRenderer _color;
    private Vector3 _playerDetector;
    private int _currentHealth = 0;
    //TODO: Implement jumping enemies if we want them.
    //private bool _doubleJump;
	private float _tempTime, _shootTime, _hitTimer;
	private bool _isDropping, _isShooting, _hit, _isDead;
    private AudioSource sfxPlayer;
    private float deathTimer;
    #endregion

	#region Sound_Items
	public AudioClip[] enemySFX = new AudioClip[5];

    //Add map or dictionary sfx structure here.
    private Dictionary<sfx, AudioClip> EnemySfxLibrary = new Dictionary<sfx, AudioClip>();

	// Used to find appropriate sound.
	public enum sfx {
		IDLE,
		WALK,
		ATTACK,
		HIT,
		DEATH
	}
	#endregion

    // Use this for initialization
    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player not set. Please set player object.\nSource: " + this.gameObject.name);
            Destroy(this.gameObject);
            return;
        }
        _controller = this.GetComponent<CharacterController2D>();
        _animator = this.GetComponent<AnimationController2D>();
        _color = this.GetComponent<SpriteRenderer>();
        _color.color = Color.white;
        _animator.setFacing("Left");
        _animator.setAnimation("Idle");
        _currentHealth = health;
        _playerDetector = getPlayerPos();
        //TODO: Implement jumping enemies if we want them.
        //_doubleJump = true;
        _tempTime = 0;
        _hit = false;
        _hitTimer = 0;
        _isDead = false;
        deathTimer = 0;

        sfxPlayer = this.GetComponent<AudioSource>();
        sfxPlayer.volume = GameManager.GameInstance.sfxVolume;
        sfx s = sfx.IDLE;
        foreach (AudioClip a in enemySFX)
        {
            EnemySfxLibrary.Add(s, a);
            s++;
        }
        enemySFX = null;

        switch (enemy_type)
        {
            case (enemyType.DROP):
            case (enemyType.DROP_TRACKING):
                _isDropping = true;
                knockbackDistance = 0;
                break;
            case (enemyType.TURRET):
            case (enemyType.TURRET_TRACKING):
                canShoot = true;
                knockbackDistance = 0;
                break;
            case (enemyType.NONE):
                canShoot = false;
                break;
        }
        if (canShoot)
        {
            _shootTime = 0;
            _isShooting = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Currently this update method removes the warnings on variables not beeing used. This will change when we implement proper AI controls and pathfinding.
        _playerDetector = getPlayerPos();
        //TODO: Implement jumping enemies if we want them.
        //if (_doubleJump) Debug.Log("Double Jump is enabled. Disable Double Jump to turn this message off.");
    }

    void FixedUpdate()
    {
        if (_isDead)
        {
            _color.color = Color.white;
            this.gameObject.tag = "Dead";
            if (!_animator.getAnimation().Equals("Death"))
                _animator.setAnimation("Death");
            deathTimer += Time.fixedDeltaTime;
            if (deathTimer >= 0.5)
            {
                Destroy(this.gameObject);
            }
            return;
        }
        switch (enemy_type)
        {
            // Invoke turret enemy AI.
            case (enemyType.TURRET):
                canShoot = true;
                turretAI();
                break;

            // Invoke the ground enemy AI.
            case (enemyType.GROUND):
                groundMovement();
                break;

            // Invoke the flying enemy AI.
            case (enemyType.FLYING):
                flyingMovement();
                break;

            // Invoke drop enemy AI.
            case (enemyType.DROP):
                dropMovement();
                break;

            // Invoke turret enemy AI with player tracking.
            case (enemyType.TURRET_TRACKING):
                canShoot = true;
                turretTracking();
                break;

            // Invoke the ground enemy AI with player tracking.
            case (enemyType.GROUND_TRACKING):
                trackingGround();
                break;

            // Invoke the flying enemy AI with player tracking.
            case (enemyType.FLYING_TRACKING):
                trackingFlying();
                break;

            // Invoke the drop enemy AI with player tracking.
            case (enemyType.DROP_TRACKING):
                dropTracking();
                break;

            // Enemy has no movement or attacks. It is only idle, looking back and forth. 
            default:
                canShoot = false;
                idleEnemy();
                break;
        }
        hitIndication();
    }

    void OnTriggerEnter2D(Collider2D _col)
    {
        if (!_hit && _col.tag == "Attack")
        {
            _hit = true;
            _hitTimer = 0;
            _currentHealth -= 50;
            AudioClip clip;
            EnemySfxLibrary.TryGetValue(sfx.HIT, out clip);
            PlaySound(clip);
            if (player.GetComponent<PlayerController>().direction == 0 && knockbackDistance != 0)
            {
                _controller.move(new Vector3(-knockbackDistance, 0, 0));
            }
            else if (knockbackDistance != 0)
            {
                _controller.move(new Vector3(knockbackDistance, 0, 0));
            }
            if (_currentHealth <= 0)
            {
                EnemySfxLibrary.TryGetValue(sfx.DEATH, out clip);
                PlaySound(clip);
                _isDead = true;
            }
        }
        else if (_col.tag == "SuperFireball") {
            _hit = true;
            _hitTimer = 0;
            _currentHealth -= 400;
            AudioClip clip;
            EnemySfxLibrary.TryGetValue(sfx.HIT, out clip);
            PlaySound(clip);
            if (player.GetComponent<PlayerController>().direction == 0 && knockbackDistance != 0) {
                _controller.move(new Vector3(-knockbackDistance, 0, 0));
            }
            else if (knockbackDistance != 0) {
                _controller.move(new Vector3(knockbackDistance, 0, 0));
            }
            if (_currentHealth <= 0) {
                EnemySfxLibrary.TryGetValue(sfx.DEATH, out clip);
                PlaySound(clip);
                _isDead = true;
            }
        }
        else if (_col.tag == "MorningPeacock") {
            _hit = true;
            _hitTimer = 0;
            _currentHealth -= 100;
            AudioClip clip;
            EnemySfxLibrary.TryGetValue(sfx.HIT, out clip);
            PlaySound(clip);
            if (player.GetComponent<PlayerController>().direction == 0 && knockbackDistance != 0) {
                _controller.move(new Vector3(-knockbackDistance, 0, 0));
            }
            else if (knockbackDistance != 0) {
                _controller.move(new Vector3(knockbackDistance, 0, 0));
            }
            if (_currentHealth <= 0) {
                EnemySfxLibrary.TryGetValue(sfx.DEATH, out clip);
                PlaySound(clip);
                _isDead = true;
            }
        }
        else if (_col.tag == "KillZ")
        {
            AudioClip clip;
            EnemySfxLibrary.TryGetValue(sfx.DEATH, out clip);
            PlaySound(clip);
            _isDead = true;
        }
        else if (_col.tag == "EarthShatter") {
            _hit = true;
            _hitTimer = 0;
            _currentHealth -= 100;
            AudioClip clip;
            EnemySfxLibrary.TryGetValue(sfx.HIT, out clip);
            PlaySound(clip);
            if (player.GetComponent<PlayerController>().direction == 0 && knockbackDistance != 0) {
                _controller.move(new Vector3(-knockbackDistance, 0, 0));
            }
            else if (knockbackDistance != 0) {
                _controller.move(new Vector3(knockbackDistance, 0, 0));
            }
            if (_currentHealth <= 0) {
                EnemySfxLibrary.TryGetValue(sfx.DEATH, out clip);
                PlaySound(clip);
                _isDead = true;
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D _col) {
        if (_col.collider.tag == "Attack") {
            _hit = true;
            _hitTimer = 0;
            _currentHealth -= 50;
            AudioClip clip;
            EnemySfxLibrary.TryGetValue(sfx.HIT, out clip);
            PlaySound(clip);
            if (player.GetComponent<PlayerController>().direction == 0 && knockbackDistance != 0) {
                _controller.move(new Vector3(-knockbackDistance, 0, 0));
            }
            else if (knockbackDistance != 0) {
                _controller.move(new Vector3(knockbackDistance, 0, 0));
            }
            if (_currentHealth <= 0) {
                EnemySfxLibrary.TryGetValue(sfx.DEATH, out clip);
                PlaySound(clip);
                _isDead = true;
            }
        }
        else if (_col.collider.tag == "KillZ") {
            AudioClip clip;
            EnemySfxLibrary.TryGetValue(sfx.DEATH, out clip);
            PlaySound(clip);
            _isDead = true;
        }
        else if (_col.collider.tag == "EarthShatter") {
            _hit = true;
            _hitTimer = 0;
            _currentHealth -= 100;
            AudioClip clip;
            EnemySfxLibrary.TryGetValue(sfx.HIT, out clip);
            PlaySound(clip);
            if (player.GetComponent<PlayerController>().direction == 0 && knockbackDistance != 0) {
                _controller.move(new Vector3(-knockbackDistance, 0, 0));
            }
            else if (knockbackDistance != 0) {
                _controller.move(new Vector3(knockbackDistance, 0, 0));
            }
            if (_currentHealth <= 0) {
                EnemySfxLibrary.TryGetValue(sfx.DEATH, out clip);
                PlaySound(clip);
                _isDead = true;
            }
        }
    }

    public void hitIndication()
    {
        //TODO: Edit damage visual indicator. Can change the length of time enemy flashes, or try to the change rate it flashes.
        if (_hit)
        {
            _hitTimer += Time.fixedDeltaTime;
            if (_hitTimer < 0.5f && _color.color == Color.red)
            {
                _color.color = Color.white;
            }
            else if (_hitTimer < 0.5f && _color.color == Color.white)
            {
                _color.color = Color.red;
            }
            else if (_hitTimer >= 0.5f)
            {
                _color.color = Color.white;
                _hit = false;
                _hitTimer = 0;
            }
        }
    }

    Vector3 getPlayerPos()
    {
        return player.GetComponent<PlayerController>().getPosition();
    }

    private void PlaySound (AudioClip clip)
    {
        if (clip == null) return;
        sfxPlayer.Stop();
        sfxPlayer.clip = clip;
        sfxPlayer.Play();
    }

    #region AI_Methods
    /// <summary>
    /// Actions of turret style enemies.
    /// </summary>
    private void turretAI()
    {
        _tempTime += (Time.fixedDeltaTime);
        if (_tempTime >= (moveDistance/moveSpeed) && _animator.getFacing().Equals("Left"))
        {
            _animator.setFacing("Right");
            _tempTime = 0;
        }
        else if (_tempTime >= (moveDistance/moveSpeed) && _animator.getFacing().Equals("Right"))
        {
            _animator.setFacing("Left");
            _tempTime = 0;
        }
        animate();
    }

    /// <summary>
    /// Movement of Ground enemies.
    /// </summary>
    private void groundMovement()
    {

		Vector3 _vector = Vector3.zero;
		if (!_isShooting) {
			_tempTime += (Time.fixedDeltaTime);

			if (_animator.getFacing ().CompareTo ("Right") == 0 && _tempTime >= (moveDistance / moveSpeed)) {
				_animator.setFacing ("Left");
				_tempTime = 0;
			} else if (_animator.getFacing ().CompareTo ("Right") == 0 && _tempTime < (moveDistance / moveSpeed)) {
				_animator.setFacing ("Right");
				_vector.x = moveSpeed * Time.fixedDeltaTime;
			} else if (_animator.getFacing ().CompareTo ("Left") == 0 && _tempTime >= (moveDistance / moveSpeed)) {
				_animator.setFacing ("Right");
				_tempTime = 0;
			} else if (_animator.getFacing ().CompareTo ("Left") == 0 && _tempTime < (moveDistance / moveSpeed)) {
				_animator.setFacing ("Left");
				_vector.x = -(moveSpeed * Time.fixedDeltaTime);
			}
		}
		_vector.y = (gravity * Time.fixedDeltaTime);
		_controller.move (_vector);
        animate();
    }

    /// <summary>
    /// Movement of flying enemies.
    /// </summary>
    private void flyingMovement()
    {
        //TODO: Update for better flight movement. Currently basic back and forth movement.
        Vector3 _vector = Vector3.zero;

        _tempTime += (Time.fixedDeltaTime);

        if (_animator.getFacing().CompareTo("Right") == 0 && _tempTime >= (moveDistance/moveSpeed))
        {
            _animator.setFacing("Left");
            _tempTime = 0;
        }
        else if (_animator.getFacing().CompareTo("Right") == 0 && _tempTime < (moveDistance / moveSpeed))
        {
            _animator.setFacing("Right");
            _vector.x = moveSpeed * Time.fixedDeltaTime;
        }
        else if (_animator.getFacing().CompareTo("Left") == 0 && _tempTime >= (moveDistance / moveSpeed))
        {
            _animator.setFacing("Right");
            _tempTime = 0;
        }
        else if (_animator.getFacing().CompareTo("Left") == 0 && _tempTime < (moveDistance / moveSpeed))
        {
            _animator.setFacing("Left");
            _vector.x = -(moveSpeed * Time.fixedDeltaTime);
        }

        _controller.move(_vector);
        animate();
    }

    /// <summary>
    /// Movement of drop style enemies.
    /// </summary>
    private void dropMovement()
    {
        Vector3 _vector = Vector3.zero;

        _tempTime += (Time.fixedDeltaTime);
        // End of drop. Start rising again.
        if (_isDropping && _tempTime >= (moveDistance / moveSpeed)) // Change "(moveDistance / moveSpeed)" if we want a wait time at the bottom of the drop.
        {
            _isDropping = false;
            _tempTime = 0;
        }
        // Currently dropping. continue until traveled full distance.
        else if (_isDropping && _tempTime < (moveDistance / moveSpeed))
        {
            _vector.y = -(moveSpeed * Time.fixedDeltaTime);
        }
        // Ready to drop again. begin dropping.
        else if (!_isDropping && _tempTime >= 2*(moveDistance / moveSpeed)) // Change "(moveDistance / moveSpeed)" to vary wait time at top.
        {
            _isDropping = true;
            _tempTime = 0;
        }
        // Rising from drop state.
        else if (!_isDropping && _tempTime < (moveDistance / moveSpeed))
        {
            _vector.y = moveSpeed * Time.fixedDeltaTime;
        }

        _controller.move(_vector);
        animate();
    }

    /// <summary>
    /// Actions of turret style enemies with player tracking.
    /// </summary>
    private void turretTracking()
    {
        _playerDetector = getPlayerPos();

        float xDist = Mathf.Pow(_playerDetector.x - this.transform.position.x, 2f);
        float yDist = Mathf.Pow(_playerDetector.y - this.transform.position.y, 2f);
        bool playerInRange = Mathf.Sqrt(xDist + yDist) <= detectionRange;

        if (playerInRange)
        {
            if (this.transform.position.x > _playerDetector.x)
                _animator.setFacing("Left");
            else if (this.transform.position.x < _playerDetector.x)
                _animator.setFacing("Right");
            animate();
        }
        else
        {
            _animator.setAnimation("Idle");
            _tempTime += (Time.fixedDeltaTime);
            _shootTime = 0;
            _isShooting = true;
            if (_tempTime >= (moveDistance / moveSpeed) && _animator.getFacing().Equals("Left"))
            {
                _animator.setFacing("Right");
                _tempTime = 0;
            }
            else if (_tempTime >= (moveDistance / moveSpeed) && _animator.getFacing().Equals("Right"))
            {
                _animator.setFacing("Left");
                _tempTime = 0;
            }
        }
    }

    /// <summary>
    /// Movement of ground enemies with player tracking.
    /// </summary>
    private void trackingGround()
    {
        _playerDetector = getPlayerPos();

        float xDist = Mathf.Pow(_playerDetector.x - this.transform.position.x, 2f);
        float yDist = Mathf.Pow(_playerDetector.y - this.transform.position.y, 2f);
        bool playerInRange = Mathf.Sqrt(xDist + yDist) <= detectionRange;
        Vector3 _vector = Vector3.zero;

		_tempTime += (Time.fixedDeltaTime);

        //Player is in range, move to player.
        if (playerInRange)
        {
            float dist = _playerDetector.x - this.transform.position.x;

            // Face the player.
            if (this.transform.position.x > _playerDetector.x)
                _animator.setFacing("Left");
            else if (this.transform.position.x < _playerDetector.x)
                _animator.setFacing("Right");

			if (dist < -stoppingRange && !_isShooting)
            {
                _vector.x = -(moveSpeed * Time.fixedDeltaTime);
            }
			else if (dist > stoppingRange && !_isShooting)
            {
                _vector.x = (moveSpeed * Time.fixedDeltaTime);
            }
            _tempTime = 0;
            animate(dist);
        }

        //Player is not in range, walk back and forth.
        else
        {
            if (_animator.getFacing().CompareTo("Right") == 0 && _tempTime >= (moveDistance / moveSpeed))
            {
                _animator.setFacing("Left");
                _animator.setAnimation("Run");
                _tempTime = 0;
            }
            else if (_animator.getFacing().CompareTo("Right") == 0 && _tempTime < (moveDistance / moveSpeed))
            {
                _animator.setAnimation("Run");
                _animator.setFacing("Right");
                _vector.x = (moveSpeed * Time.fixedDeltaTime);
            }
            else if (_animator.getFacing().CompareTo("Left") == 0 && _tempTime >= (moveDistance / moveSpeed))
            {
                _animator.setFacing("Right");
                _animator.setAnimation("Run");
                _tempTime = 0;
            }
            else if (_animator.getFacing().CompareTo("Left") == 0 && _tempTime < (moveDistance / moveSpeed))
            {
                _animator.setAnimation("Run");
                _animator.setFacing("Left");
                _vector.x = -(moveSpeed * Time.fixedDeltaTime);
            }
            _shootTime = 0;
        }
        _vector.y += (gravity * Time.fixedDeltaTime);
        _controller.move(_vector);
    }

    /// <summary>
    /// Movement of flying enemies with player tracking.
    /// </summary>
    private void trackingFlying()
    {
        //TODO: Update for better flight movement. Currently basic tracking and basic back and forth movement.
        _playerDetector = getPlayerPos();

        float xDist = Mathf.Pow(_playerDetector.x - this.transform.position.x, 2f);
        float yDist = Mathf.Pow(_playerDetector.y - this.transform.position.y, 2f);
        bool playerInRange = Mathf.Sqrt(xDist + yDist) <= detectionRange;
        Vector3 _vector = Vector3.zero;

        _tempTime += (Time.fixedDeltaTime);

        //Player is in range, move to player.
        if (playerInRange)
        {
            float dist = _playerDetector.x - this.transform.position.x;

            // Face the player.
            if (this.transform.position.x > _playerDetector.x)
                _animator.setFacing("Left");
            else if (this.transform.position.x < _playerDetector.x)
                _animator.setFacing("Right");

            if (dist < -stoppingRange)
            {
                _vector.x = -(moveSpeed * Time.fixedDeltaTime);
            }
            else if (dist > stoppingRange)
            {
                _vector.x = (moveSpeed * Time.fixedDeltaTime);
            }
            _tempTime = 0;
            animate(dist);
        }

        //Player is not in range, move back and forth.
        else
        {
            if (_animator.getFacing().CompareTo("Right") == 0 && _tempTime >= (moveDistance / moveSpeed))
            {
                _animator.setFacing("Left");
                _animator.setAnimation("Fly"); //TODO: change to appropriate animation.
                _tempTime = 0;
            }
            else if (_animator.getFacing().CompareTo("Right") == 0 && _tempTime < (moveDistance / moveSpeed))
            {
                _animator.setAnimation("Fly"); //TODO: change to appropriate animation.
                _animator.setFacing("Right");
                _vector.x = (moveSpeed * Time.fixedDeltaTime);
            }
            else if (_animator.getFacing().CompareTo("Left") == 0 && _tempTime >= (moveDistance / moveSpeed))
            {
                _animator.setFacing("Right");
                _animator.setAnimation("Fly"); //TODO: change to appropriate animation.
                _tempTime = 0;
            }
            else if (_animator.getFacing().CompareTo("Left") == 0 && _tempTime < (moveDistance / moveSpeed))
            {
                _animator.setAnimation("Fly"); //TODO: change to appropriate animation.
                _animator.setFacing("Left");
                _vector.x = -(moveSpeed * Time.fixedDeltaTime);
            }
            _shootTime = 0;
        }
        _controller.move(_vector);
    }

    /// <summary>
    /// Movement of drop style enemies with player tracking.
    /// </summary>
    private void dropTracking()
    {
        _playerDetector = getPlayerPos();
        
        float xDist = Mathf.Abs(_playerDetector.x - this.transform.position.x);
        float yDist = _playerDetector.y - this.transform.position.y;
        bool playerInRange = (xDist <= detectionRange && yDist <= 0 && yDist >= -(moveDistance+1));
        Vector3 _vector = Vector3.zero;

        if (playerInRange)
        {
            // Face the player.
            if (this.transform.position.x > _playerDetector.x)
                _animator.setFacing("Left");
            else if (this.transform.position.x < _playerDetector.x)
                _animator.setFacing("Right");
            _tempTime += (Time.fixedDeltaTime);
            // End of drop. Start rising again.
            if (_isDropping && _tempTime >= (moveDistance / moveSpeed))
            {
                _isDropping = false;
                _tempTime = 0;
            }
            // Currently dropping. continue until traveled full distance.
            else if (_isDropping && _tempTime < (moveDistance / moveSpeed))
            {
                _vector.y = -(moveSpeed * Time.fixedDeltaTime);
            }
            // Ready to drop again. begin dropping.
            else if (!_isDropping && _tempTime >= (moveDistance / moveSpeed))
            {
                _isDropping = true;
                _tempTime = 0;
            }
            // Rising from drop state.
            else if (!_isDropping && _tempTime < (moveDistance / moveSpeed))
            {
                _vector.y = moveSpeed * Time.fixedDeltaTime;
            }

            _controller.move(_vector);
            animate();
        }
        else
        {
            //This section resets the enemy to the original position if it is currently in motion.

            // End of drop. Start rising again.
            if (_isDropping && _tempTime >= (moveDistance / moveSpeed))
            {
                _isDropping = false;
                _tempTime = 0;
            }
            // Currently dropping. continue until traveled full distance.
            else if (_isDropping && _tempTime < (moveDistance / moveSpeed))
            {
                _tempTime += (Time.fixedDeltaTime);
                _vector.y = -(moveSpeed * Time.fixedDeltaTime);
            }
            // Rising from drop state.
            else if (!_isDropping && _tempTime < (moveDistance / moveSpeed))
            {
                _tempTime += (Time.fixedDeltaTime);
                _vector.y = moveSpeed * Time.fixedDeltaTime;
            }

            _animator.setAnimation("Idle");
            _controller.move(_vector);
            _shootTime = 0;
        }
    }

    /// <summary>
    /// Idle enemy looking back and forth at a set interval.
    /// </summary>
    void idleEnemy()
    {
        _animator.setAnimation("Idle");
        _tempTime += (Time.fixedDeltaTime);
        if (_tempTime >= (moveDistance / moveSpeed) && _animator.getFacing().Equals("Left"))
        {
            _animator.setFacing("Right");
            _tempTime = 0;
        }
        else if (_tempTime >= (moveDistance / moveSpeed) && _animator.getFacing().Equals("Right"))
        {
            _animator.setFacing("Left");
            _tempTime = 0;
        }
        _controller.move(new Vector3(0, (gravity * Time.fixedDeltaTime), 0));
    }

    /// <summary>
    /// Animation selection controls for the enemy. Also handles projectiles for shooting enemies.
    /// </summary>
    private void animate()
    {
        if (!canShoot) {
            switch (enemy_type)
            {
                case (enemyType.GROUND):
                    if (!_animator.getAnimation().Equals("Run"))
                        _animator.setAnimation("Run");
                    break;
                case (enemyType.FLYING):
                    if (!_animator.getAnimation().Equals("Fly"))
                        _animator.setAnimation("Fly");
                    break;
                default:
                    if (!_animator.getAnimation().Equals("Idle"))
                        _animator.setAnimation("Idle");
                    break;
            }
            return;
        }
        _shootTime += (Time.fixedDeltaTime);
		if (_shootTime >= shootInterval) {
			if (_isShooting) {
				if (!_animator.getAnimation ().Equals ("Shoot"))
					_animator.setAnimation ("Shoot");
				_isShooting = false;
				_shootTime = 0;
			}
			else {
				switch (enemy_type) {
				case (enemyType.GROUND):
					if (!_animator.getAnimation ().Equals ("Run"))
						_animator.setAnimation ("Run");
					break;
				case (enemyType.FLYING):
					if (!_animator.getAnimation ().Equals ("Fly"))
						_animator.setAnimation ("Fly");
					break;
				default:
					if (!_animator.getAnimation ().Equals ("Idle"))
						_animator.setAnimation ("Idle");
					break;
				}
				_isShooting = true;
				_shootTime = 0;
			}
		} else if (_shootTime < 0.2 && _isShooting)
		{
			if (_shootTime > 0.185)
				projectile ();
			if (!_animator.getAnimation ().Equals ("Shoot"))
				_animator.setAnimation ("Shoot");
		}
		else if (_shootTime >= 0.2 && _isShooting) 
		{
			if (!_animator.getAnimation ().Equals ("Shoot"))
				_animator.setAnimation ("Shoot");
			_isShooting = false;
		}
		else
        {
            switch (enemy_type)
            {
                case (enemyType.GROUND):
                    if (!_animator.getAnimation().Equals("Run"))
                        _animator.setAnimation("Run");
                    break;
                case (enemyType.FLYING):
                    if (!_animator.getAnimation().Equals("Fly"))
                        _animator.setAnimation("Fly");
                    break;
                default:
                    if (!_animator.getAnimation().Equals("Idle"))
                        _animator.setAnimation("Idle");
                    break;
            }
        }
    }

    /// <summary>
	/// Animation selection controls for the enemy. Also handles projectiles for shooting enemies. This method in particular is used with tracking enemy types.
    /// </summary>
    private void animate(float _dist)
    {
        _dist = Mathf.Abs(_dist);
        if (!canShoot) {
            switch (enemy_type)
            {
                case (enemyType.GROUND_TRACKING):
                    if (_dist <= stoppingRange)
                    {
                        if (!_animator.getAnimation().Equals("Idle"))
                            _animator.setAnimation("Idle");
                    }
                    else
                    {
                        if (!_animator.getAnimation().Equals("Run"))
                            _animator.setAnimation("Run");
                    }
                    break;
                case (enemyType.FLYING_TRACKING):
                    if (_dist <= stoppingRange)
                    {
                        if (!_animator.getAnimation().Equals("Run"))
                            _animator.setAnimation("Run");
                    }
                    else
                    {
                        if (!_animator.getAnimation().Equals("Fly"))
                            _animator.setAnimation("Fly");
                    }
                    break;
            }
            return;
        }
        _shootTime += (Time.fixedDeltaTime);
        if (_shootTime >= shootInterval)
        {
            if (_isShooting)
            {
                if (!_animator.getAnimation().Equals("Shoot"))
                    _animator.setAnimation("Shoot");
                _isShooting = false;
                _shootTime = 0;
            }
            else
            {
                switch (enemy_type)
                {
                    case (enemyType.GROUND_TRACKING):
                        if (_dist <= stoppingRange)
                        {
                            if (!_animator.getAnimation().Equals("Idle"))
                                _animator.setAnimation("Idle");
                        }
                        else
                        {
                            if (!_animator.getAnimation().Equals("Run"))
                                _animator.setAnimation("Run");
                        }
                        break;
                    case (enemyType.FLYING_TRACKING):
                        if (_dist <= stoppingRange)
                        {
                            if (!_animator.getAnimation().Equals("Idle"))
                                _animator.setAnimation("Idle");
                        }
                        else
                        {
                            if (!_animator.getAnimation().Equals("Fly"))
                                _animator.setAnimation("Fly");
                        }
                        break;
                }
                _isShooting = true;
                _shootTime = 0;
            }
        }
        else if (_shootTime < 0.2 && _isShooting)
        {
            if (_shootTime > 0.185)
                projectile();
            if (!_animator.getAnimation().Equals("Shoot"))
                _animator.setAnimation("Shoot");
        }
		else if (_shootTime >= 0.2 && _isShooting)
		{
			if (!_animator.getAnimation().Equals("Shoot"))
				_animator.setAnimation("Shoot");
			_isShooting = false;
		}
        else
        {
            switch (enemy_type)
            {
                case (enemyType.GROUND_TRACKING):
                    if (_dist <= stoppingRange)
                    {
                        if (!_animator.getAnimation().Equals("Idle"))
                            _animator.setAnimation("Idle");
                    }
                    else
                    {
                        if (!_animator.getAnimation().Equals("Run"))
                            _animator.setAnimation("Run");
                    }
                    break;
			case (enemyType.FLYING_TRACKING):
				if (_dist <= stoppingRange) {
					if (!_animator.getAnimation ().Equals ("Idle"))
						_animator.setAnimation ("Idle");
				} else {
					if (!_animator.getAnimation ().Equals ("Fly"))
						_animator.setAnimation ("Fly");
				}
                    break;
            }
        }
    }

    /// <summary>
    /// Creates/Calls in-game projectiles.
    /// </summary>
    private void projectile()
    {
        Vector3 ballPosition;
		ballPosition = this.transform.position;
		ballPosition.y = ballPosition.y + projectilePositionY;
        if (_animator.getFacing().Equals("Left"))
        {
			ballPosition.x = ballPosition.x - projectilePositionX;
        }
        else
        {
			ballPosition.x = ballPosition.x + projectilePositionX;
        }
        GameObject fireball = (GameObject)Instantiate(fireballPrefab, ballPosition, this.transform.rotation);
        fireball.GetComponent<AnimationController2D>().setFacing(_animator.getFacing());
        fireball.GetComponent<FireballMovement>().speed = projectileSpeed;
        fireball.GetComponent<FireballMovement>().duration = projectileDuration;
        AudioClip clip;
        EnemySfxLibrary.TryGetValue(sfx.ATTACK, out clip);
        PlaySound(clip);
    }
    #endregion

    void OnDrawGizmos()
    {
        switch (enemy_type)
        {
            case (enemyType.GROUND):
            case (enemyType.FLYING):
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(this.transform.position, new Vector3(this.transform.position.x - moveDistance, this.transform.position.y));
                break;
            case (enemyType.DROP):
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(this.transform.position, new Vector3(this.transform.position.x, this.transform.position.y - moveDistance));
                break;
            case (enemyType.TURRET_TRACKING):
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(this.transform.position, detectionRange);
                break;
            case (enemyType.GROUND_TRACKING):
            case (enemyType.FLYING_TRACKING):
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(this.transform.position, detectionRange);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(this.transform.position, stoppingRange);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(this.transform.position, new Vector3(this.transform.position.x - moveDistance, this.transform.position.y));
                break;
            case (enemyType.DROP_TRACKING):
                Vector3 center = new Vector3(this.transform.position.x, this.transform.position.y - (moveDistance / 2)-0.5f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(center, new Vector3(2*detectionRange, moveDistance+1));
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(this.transform.position, new Vector3(this.transform.position.x, this.transform.position.y - moveDistance));
                break;
            default:
                break;

        }
		if (canShoot || enemy_type == enemyType.TURRET || enemy_type == enemyType.TURRET_TRACKING)
		{
			Vector3 placement = new Vector3 (this.transform.position.x + projectilePositionX, this.transform.position.y + projectilePositionY);
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere (placement, 0.25f);
		}
    }
}