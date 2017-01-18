using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using Prime31;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {

    public float walkSpeed = 3;
    public float gravity = -35;
    public float jumpHeight = 4;

    public GameObject healthBar;
    public GameObject bossDoor;
    public GameObject staminaBar;
    public GameObject gameOverPanel;
    public GameObject powerUpExplanation;
    public GameObject pausePanel;
    public GameObject iceBlockPrefab;
    public GameObject whirlwind;
    public GameObject fireDragon;
    public GameObject basePowerIndicator;
    public GameObject icePowerIndicator;
    public GameObject firePowerIndicator;
    public GameObject windPowerIndicator;
    public GameObject earthPowerIndicator;
    public CircleCollider2D punchCollider;
    public CircleCollider2D earthCollider;

    [Range(0, 10)]
    public float projectilePositionX = 1;
    [Range(-10, 10)]
    public float projectilePositionY = 0;

    public bool devMode;

    public int direction;

    public GameObject gameCamera;

    public AudioClip[] playerSFX = new AudioClip[14];

    // Structure to better find all SFX files for the player.
    private Dictionary<sfx, AudioClip> PlayerSfxLibrary = new Dictionary<sfx, AudioClip>();
    
	public enum sfx {
		IDLE,
		WALK,
		JUMP,
        DASH,
		ATTACK,
		ITEM_PICKUP,
        POWER_CHANGE,
		FIRE_POWER,
		EARTH_POWER,
		ICE_POWER,
		WIND_POWER,
		FATIGUE,
		HIT,
		DEATH
	}

    private CharacterController2D _controller;
    private AnimationController2D _animator;
    private SpriteRenderer _color;
    private int currentHealth = 0;
    private int currentPower = 0;
    private double currentStamina = 0;
    private float timer = 0;
    private float exhaustTimer = 0;
    private bool inNotification;
    private bool hitLeft;
    private bool hitRight;
    private bool playerControl;
    private bool canDoubleJump;
    private bool isBlocking;
    private bool inMaelstrom;
    private bool isPunching;
    private bool isPaused;
    private bool canBeDamaged;
    private bool isDashing;
    private bool earthShatter;
    private bool iceBlock;
    private bool whirl;
    private bool morningPeacock;
    private bool whirlwindWalkers;
    private bool superLiftingBelt;
    private bool iceCrystalCrown;
    private bool igneousBracers;
    private bool hurt;
    private bool exhausted;
    private bool changingPower;
    private Color hitColor;
    private Color normalColor;
    private AudioSource sfxPlayer;
    //private bool relativityEnabled;

	// Use this for initialization
	void Start () {
        _controller = gameObject.GetComponent<CharacterController2D>();
        _animator = gameObject.GetComponent<AnimationController2D>();
        //gameCamera.GetComponent<CameraFollow_v2>().startCameraFollow(this.gameObject);
		currentHealth = GameManager.GameInstance.playerHealth;
		currentStamina = GameManager.GameInstance.playerStamina;
        playerControl = true;
        canDoubleJump = false;
        direction = 1;
        isPaused = false;
        isPunching = false;
        canBeDamaged = true;
        isBlocking = false;
        isDashing = false;
        inMaelstrom = false;
        earthShatter = false;
        inNotification = false;
        whirl = false;
        morningPeacock = false;
        iceBlock = false;
        hurt = false;
        exhausted = false;
        changingPower = false;

        basePowerIndicator.SetActive(false);
        earthPowerIndicator.SetActive(false);
        icePowerIndicator.SetActive(false);
        windPowerIndicator.SetActive(false);
        firePowerIndicator.SetActive(false);

        PlayerDamage(0);


        if (devMode) {
            iceCrystalCrown = true;
            superLiftingBelt = true;
            whirlwindWalkers = true;
            igneousBracers = true;
            currentPower = 0;
        }
        else {
            iceCrystalCrown = GameManager.GameInstance.playerIcePower;
            superLiftingBelt = GameManager.GameInstance.playerEarthPower;
            whirlwindWalkers = GameManager.GameInstance.playerWindPower;
            igneousBracers = GameManager.GameInstance.playerFirePower;
        }

        currentPower = GameManager.GameInstance.playerPower;
        if (currentPower == 0)
            basePowerIndicator.SetActive(true);
        else if (currentPower == 1)
            earthPowerIndicator.SetActive(true);
        else if (currentPower == 2)
            icePowerIndicator.SetActive(true);
        else if (currentPower == 3)
            windPowerIndicator.SetActive(true);
        else
            firePowerIndicator.SetActive(true);
        bossDoor.SetActive(false);
        _color = this.GetComponent<SpriteRenderer>();
        normalColor = _color.color;
        hitColor = Color.red;
        Time.timeScale = 1;
        sfxPlayer = this.GetComponent<AudioSource>();
        sfxPlayer.volume = GameManager.GameInstance.sfxVolume;
        sfx s = sfx.IDLE;
        foreach (AudioClip a in playerSFX)
        {
            PlayerSfxLibrary.Add(s, a);
            s++;
        }
        playerSFX = null;
    }
	
	// Update is called once per frame
	void Update () {
        if (playerControl) {
            Vector3 velocity = PlayerInput();
            _controller.move(velocity * Time.deltaTime);
        }
        if (isPaused && !pausePanel.activeSelf)
            isPaused = false;
        if (inNotification) {
            if (Input.GetButtonDown("A")) {
                powerUpExplanation.SetActive(false);
                allowControl();
                Time.timeScale = 1;
                SoundManager.AudioInstance.MusicVol(GameManager.GameInstance.musicVolume);
            }
        }
        punchCollider.transform.position = this.transform.position;
        earthCollider.transform.position = this.transform.position;
        if (_controller.isGrounded)
            canDoubleJump = true;
        UseStamina(.4);
    }

    private Vector3 PlayerInput() {

        Vector3 velocity = _controller.velocity;
        timer += Time.deltaTime * walkSpeed;

        if (_controller.isGrounded && _controller.ground != null && _controller.ground.tag == "MovingPlatform") {
            this.transform.parent = _controller.ground.transform;
        }
        else {
            if (this.transform.parent != null)
                transform.parent = null;
        }

        velocity.x = 0;
        if (Input.GetAxis("Horizontal") < 0 && (Input.GetMouseButtonDown(0) || Input.GetButtonDown("X")) && currentStamina > 10 && !isPunching && !isBlocking && !hurt && !earthShatter && !changingPower && !whirl && !iceBlock && !morningPeacock && !exhausted && !isPaused && !inNotification) {
            velocity.x = -walkSpeed;
            _animator.setFacing("Left");
            direction = 0;
            punchCollider.GetComponent<CircleCollider2D>().offset = new Vector2(0.5f, punchCollider.offset.y);
            earthCollider.GetComponent<CircleCollider2D>().offset = new Vector2(0.5f, earthCollider.offset.y);
            if (_controller.isGrounded) {
                //Running Punch
                _animator.setAnimation("Single Punch");
                AudioClip clip;
                PlayerSfxLibrary.TryGetValue(sfx.ATTACK, out clip);
                PlaySound(clip);
                isPunching = true;
                UseStamina(-20);
            }
            //else
            //_animator.setAnimation("Jump Kick");
            //isPunching = true;
            //UseStamina(-20);
        }

        else if (Input.GetAxis("Horizontal") < 0 && (Input.GetKey(KeyCode.LeftControl) || Input.GetButtonDown("B")) && currentStamina > 70 && !isPunching && !isBlocking && !hurt && !earthShatter && !changingPower && !whirl && !iceBlock && !morningPeacock && !exhausted && !isPaused && !inNotification) {
            velocity.x = -walkSpeed;
            _animator.setFacing("Left");
            direction = 0;
            punchCollider.GetComponent<CircleCollider2D>().offset = new Vector2(0.5f, punchCollider.offset.y);
            earthCollider.GetComponent<CircleCollider2D>().offset = new Vector2(0.5f, earthCollider.offset.y);
            _animator.setAnimation("Dash");
            AudioClip clip;
            PlayerSfxLibrary.TryGetValue(sfx.DASH, out clip);
            PlaySound(clip);
            isDashing = true;
            UseStamina(-70);
        }

        else if (Input.GetAxis("Horizontal") > 0 && (Input.GetKey(KeyCode.LeftControl) || Input.GetButtonDown("B")) && currentStamina > 70 && !isPunching && !isBlocking && !hurt && !earthShatter && !changingPower && !whirl && !iceBlock && !morningPeacock && !exhausted && !isPaused && !inNotification) {
            velocity.x = walkSpeed;
            _animator.setFacing("Right");
            direction = 1;
            punchCollider.GetComponent<CircleCollider2D>().offset = new Vector2(0.5f, punchCollider.offset.y);
            earthCollider.GetComponent<CircleCollider2D>().offset = new Vector2(0.5f, earthCollider.offset.y);
            _animator.setAnimation("Dash");

            AudioClip clip;
            PlayerSfxLibrary.TryGetValue(sfx.DASH, out clip);
            PlaySound(clip);
            isDashing = true;
            UseStamina(-70);
        }

        else if ((Input.GetButtonDown("LB") || Input.GetKeyDown(KeyCode.S)) && _controller.isGrounded && !isPunching && !isBlocking && !isDashing && !earthShatter && !whirl && (Input.GetAxis("Horizontal") == 0) && !changingPower && !iceBlock && !morningPeacock && !exhausted && !isPaused && !inNotification && superLiftingBelt) {
            changingPower = true;
            _animator.setAnimation("ChangePower");
            AudioClip clip;
            PlayerSfxLibrary.TryGetValue(sfx.POWER_CHANGE, out clip);
            PlaySound(clip);
            currentPower--;
            if (currentPower < 0) {
                if (igneousBracers) {
                    basePowerIndicator.SetActive(false);
                    firePowerIndicator.SetActive(true);
                    currentPower = 4;
                }
                else if (whirlwindWalkers) {
                    basePowerIndicator.SetActive(false);
                    windPowerIndicator.SetActive(true);
                    currentPower = 3;
                }
                else if (iceCrystalCrown) {
                    basePowerIndicator.SetActive(false);
                    icePowerIndicator.SetActive(true);
                    currentPower = 2;
                }
                else if (superLiftingBelt) {
                    basePowerIndicator.SetActive(false);
                    earthPowerIndicator.SetActive(true);
                    currentPower = 1;
                }
                else
                    currentPower = 0;
            }

            else if (currentPower == 0) {
                earthPowerIndicator.SetActive(false);
                basePowerIndicator.SetActive(true);
            }
            else if (currentPower == 1) {
                icePowerIndicator.SetActive(false);
                earthPowerIndicator.SetActive(true);
            }
            else if (currentPower == 2) {
                windPowerIndicator.SetActive(false);
                icePowerIndicator.SetActive(true);
            }
            else if (currentPower == 3) {
                firePowerIndicator.SetActive(false);
                windPowerIndicator.SetActive(true);
            }
        }

        else if ((Input.GetButtonDown("RB") || Input.GetKeyDown(KeyCode.W)) && _controller.isGrounded &&!isPunching && !isBlocking && !isDashing && !earthShatter && !whirl && (Input.GetAxis("Horizontal") == 0) && !changingPower && !iceBlock && !morningPeacock && !exhausted && !isPaused && !inNotification && superLiftingBelt) {
            changingPower = true;
            _animator.setAnimation("ChangePower");
            currentPower++;
            AudioClip clip;
            PlayerSfxLibrary.TryGetValue(sfx.POWER_CHANGE, out clip);
            PlaySound(clip);
            if (currentPower == 0) {
                if (igneousBracers)
                    firePowerIndicator.SetActive(false);
                else if (whirlwindWalkers)
                    windPowerIndicator.SetActive(false);
                else if (iceCrystalCrown)
                    icePowerIndicator.SetActive(false);
                else if (superLiftingBelt)
                    earthPowerIndicator.SetActive(false);
                basePowerIndicator.SetActive(true);
            }
            else if(currentPower == 1) {
                if (superLiftingBelt) {
                    basePowerIndicator.SetActive(false);
                    earthPowerIndicator.SetActive(true);
                }
                else {
                    currentPower = 0;
                }
            }
            else if (currentPower == 2) {
                if (iceCrystalCrown) {
                    earthPowerIndicator.SetActive(false);
                    icePowerIndicator.SetActive(true);
                }
                else {
                    earthPowerIndicator.SetActive(false);
                    basePowerIndicator.SetActive(true);
                    currentPower = 0;
                }
            }
            else if (currentPower == 3) {
                if (whirlwindWalkers) {
                    icePowerIndicator.SetActive(false);
                    windPowerIndicator.SetActive(true);
                }
                else {
                    icePowerIndicator.SetActive(false);
                    basePowerIndicator.SetActive(true);
                    currentPower = 0;
                }
            }
            else if (currentPower == 4) {
                if (igneousBracers) {
                    windPowerIndicator.SetActive(false);
                    firePowerIndicator.SetActive(true);
                }
                else {
                    windPowerIndicator.SetActive(false);
                    basePowerIndicator.SetActive(true);
                    currentPower = 0;
                }
            }
            else if (currentPower == 5) {
                firePowerIndicator.SetActive(false);
                basePowerIndicator.SetActive(true);
                currentPower = 0;
            }
        }

        else if (Input.GetAxis("Horizontal") > 0 && (Input.GetMouseButtonDown(0) || Input.GetButtonDown("X")) && currentStamina > 10 && !isPunching && !isBlocking && !hurt && !earthShatter && !changingPower && !whirl && !iceBlock && !morningPeacock && !exhausted && !isPaused && !inNotification) {
            velocity.x = walkSpeed;
            _animator.setFacing("Right");
            direction = 1;
            punchCollider.GetComponent<CircleCollider2D>().offset = new Vector2(0.5f, punchCollider.offset.y);
            earthCollider.GetComponent<CircleCollider2D>().offset = new Vector2(0.5f, earthCollider.offset.y);
            if (_controller.isGrounded) {
                //Running Punch
                _animator.setAnimation("Single Punch");
                AudioClip clip;
                PlayerSfxLibrary.TryGetValue(sfx.ATTACK, out clip);
                PlaySound(clip);
                isPunching = true;
                UseStamina(-20);
            }
            //else
            //_animator.setAnimation("Jump Kick");
            //isPunching = true;
            //UseStamina(-20);
        }

        else if ((Input.GetMouseButtonDown(1) || Input.GetButtonDown("Y")) && !isPunching && currentStamina > 49 && !isBlocking && _controller.isGrounded && !hurt && !earthShatter && !changingPower && !whirl && !iceBlock && !morningPeacock && !exhausted && !isPaused && !inNotification) {
            if (currentPower == 0) {
                isBlocking = true;
                UseStamina(-50);
                _animator.setAnimation("Block");
            }
            else if (currentPower == 1) {
                earthShatter = true;
                UseStamina(-50);
                _animator.setAnimation("EarthPower");
                AudioClip clip;
                PlayerSfxLibrary.TryGetValue(sfx.EARTH_POWER, out clip);
                PlaySound(clip);
            }
            else if (currentPower == 2) {
                iceBlock = true;
                _animator.setAnimation("IcePower");
            }

            else if (currentPower == 3) {
                whirl = true;
                whirlwind.SetActive(true);
                UseStamina(-50);
                _animator.setAnimation("WindPower");

                AudioClip clip;
                PlayerSfxLibrary.TryGetValue(sfx.WIND_POWER, out clip);
                PlaySound(clip);
            }

            else if (currentPower == 4) {
                morningPeacock = true;
                UseStamina(-50);
                _animator.setAnimation("FirePower");

                AudioClip clip;
                PlayerSfxLibrary.TryGetValue(sfx.FIRE_POWER, out clip);
                PlaySound(clip);
            }

        }


        else if (Input.GetAxis("Horizontal") < 0 && !isPunching && !isBlocking && !isDashing && !hurt && !earthShatter && !changingPower && !whirl && !iceBlock && !morningPeacock && !exhausted && !isPaused && !inNotification) {
            velocity.x = -walkSpeed;
            _animator.setFacing("Left");
            direction = 0;
            punchCollider.GetComponent<CircleCollider2D>().offset = new Vector2(0.5f, punchCollider.offset.y);
            earthCollider.GetComponent<CircleCollider2D>().offset = new Vector2(0.5f, earthCollider.offset.y);
            if (_controller.isGrounded)
            {
                _animator.setAnimation("Run");
                AudioClip clip;
                PlayerSfxLibrary.TryGetValue(sfx.WALK, out clip);
                PlaySound(clip);
            }
        }
        else if (Input.GetAxis("Horizontal") > 0 && !isPunching && !isBlocking && !isDashing && !hurt && !earthShatter && !changingPower && !whirl && !iceBlock && !morningPeacock && !exhausted && !isPaused && !inNotification) {
            velocity.x = walkSpeed;
            _animator.setFacing("Right");
            direction = 1;
            punchCollider.GetComponent<CircleCollider2D>().offset = new Vector2(0.5f, punchCollider.offset.y);
            earthCollider.GetComponent<CircleCollider2D>().offset = new Vector2(0.5f, earthCollider.offset.y);
            if (_controller.isGrounded)
            {
                _animator.setAnimation("Run");
                AudioClip clip;
                PlayerSfxLibrary.TryGetValue(sfx.WALK, out clip);
                PlaySound(clip);
            }
        }
        /*
        else if (Input.GetAxis("Vertical") < 0 && isPunching == false && _controller.isGrounded) {
            _animator.setAnimation("Crouch");
        }*/

        else if ((Input.GetMouseButtonDown(0) || Input.GetButtonDown("X")) && currentStamina > 10 && !isPunching && !hurt && !earthShatter && !changingPower && !whirl && !iceBlock && !morningPeacock && !exhausted && !isPaused && !inNotification) {
            if (_controller.isGrounded) {
                _animator.setAnimation("Single Punch");
                AudioClip clip;
                PlayerSfxLibrary.TryGetValue(sfx.ATTACK, out clip);
                PlaySound(clip);
                isPunching = true;
                UseStamina(-20);
            }
            //else {
            //punchCollider.GetComponent<CircleCollider2D>().offset = new Vector2(punchCollider.offset.x, 0);
            //_animator.setAnimation("Jump Kick");
            //}
            //isPunching = true;
            //UseStamina(-20);
        }

        /*else if(Input.GetKeyDown(KeyCode.P) && _controller.isGrounded == false && currentStamina > 10) {
            _animator.setAnimation("Jump Kick");
            punchCollider.GetComponent<CircleCollider2D>().offset = new Vector2(punchCollider.offset.x, 0);

            isPunching = true;
            UseStamina(-20);
        }*/
        else {
            if (_controller.isGrounded && !isPunching && !isBlocking && !isDashing && !hurt && !earthShatter && !changingPower && !whirl && !iceBlock && !morningPeacock && !exhausted && !isPaused && !inNotification) {
                _animator.setAnimation("Idle");
                disablePunch();
            }
        }

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("A")) && _controller.isGrounded && !hurt && !isDashing && !earthShatter && !isPunching && !isBlocking && !changingPower && !whirl && !iceBlock && !morningPeacock && !exhausted && !isPaused && !inNotification) {
            velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
            _animator.setAnimation("Jump");
            AudioClip clip;
            PlayerSfxLibrary.TryGetValue(sfx.JUMP, out clip);
            PlaySound(clip);
        }
       
        else if ((Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("A")) && !_controller.isGrounded && canDoubleJump && !hurt && !isDashing && !earthShatter && !isPunching && !isBlocking && !changingPower && !whirl && !iceBlock && !morningPeacock && !exhausted && !isPaused && !inNotification) {
            velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
            _animator.setAnimation("Jump");
            AudioClip clip;
            PlayerSfxLibrary.TryGetValue(sfx.JUMP, out clip);
            PlaySound(clip);
            canDoubleJump = false;
        }
        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Start")) && isPaused == false && gameOverPanel.activeSelf == false) {
            if (!inNotification)
                Time.timeScale = 0;
			SoundManager.AudioInstance.MusicVol (GameManager.GameInstance.musicVolume / 2);
            pausePanel.SetActive(true);
            isPaused = true;
            GameManager.GameInstance.PlayPauseSound();
        }

        else if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Start")) && isPaused == true && gameOverPanel.activeSelf == false) {
            if (!inNotification)
                Time.timeScale = 1;
			SoundManager.AudioInstance.MusicVol (GameManager.GameInstance.musicVolume);
            pausePanel.SetActive(false);
            isPaused = false;
            GameManager.GameInstance.PlayPauseSound();
        }
        if (isDashing)
            if (direction == 1) {
                velocity.x += 15;
                velocity.y = 0;
            }
            else {
                velocity.x -= 15;
                velocity.y = 0;
            }
        else if (inMaelstrom) {
            velocity.x -= 12;
            velocity.y += gravity * Time.deltaTime;
        }
        else if (exhausted) {
            exhaustTimer += Time.deltaTime;
            if (exhaustTimer >= 1.25) {
                Debug.Log("RECOVERED");
                exhausted = false;
                exhaustTimer = 0;
                _animator.setAnimation("Idle");
                AudioClip clip;
                PlayerSfxLibrary.TryGetValue(sfx.IDLE, out clip);
                PlaySound(clip);
            }
        }
        else if (iceBlock) {
            velocity.y += .4f;
        }
        else {
            velocity.y += gravity * Time.deltaTime;
        }

        return velocity;
    }
    void OnCollisionEnter2D(Collision2D col) {
        if (col.collider.tag == "KillZ") {
            PlayerFallDeath();
        }

        else if (col.collider.tag == "Damaging" && !whirl) {
            Vector3 enemyLoc = col.gameObject.transform.position;
           
            if (enemyLoc.x > _controller.gameObject.transform.position.x)
                hitRight = true;
            else
                hitLeft = true;
            if ((isPunching || earthShatter) && enemyLoc.x > _controller.gameObject.transform.position.x && direction == 1) {
                
            }
            else if ((isPunching || earthShatter) && enemyLoc.x < _controller.gameObject.transform.position.x && direction == 0) {
               
            }
            else {
                if (canBeDamaged && !isBlocking) {
                    PlayerDamage(25);
                    hurt = true;
                    canBeDamaged = false;
                    isPunching = false;
                    earthShatter = false;
                    StartCoroutine(DamageFlash());
                    timer = 0;

                }
                else if (canBeDamaged && isBlocking && enemyLoc.x > _controller.gameObject.transform.position.x) {
                    PlayerDamage(5);
                    canBeDamaged = false;
                    StartCoroutine(DamageFlash());
                    timer = 0;
                }
            }
        }
        else if (col.collider.tag == "Victory") {
            LoadNextLevel();
        }
        else if (col.collider.tag == "NextScene") {
            LoadNextScene();
        }
        else if (col.collider.tag == "Health") {
            PlayerHeal(50);
            Destroy(col.gameObject);
        }
        else if (col.collider.tag == "Notification") {
            Time.timeScale = 0;
            inNotification = true;
            powerUpExplanation.SetActive(true);
            Destroy(col.gameObject);
        }
        else if (col.collider.tag == "PowerPickUp") {
            AudioClip clip;
            PlayerSfxLibrary.TryGetValue(sfx.ITEM_PICKUP, out clip);
            SoundManager.AudioInstance.PlaySound(clip);
            Time.timeScale = 0;
            inNotification = true;
            powerUpExplanation.SetActive(true);
            Destroy(col.gameObject);
            if (!superLiftingBelt) {
                Debug.Log("New Item");
                superLiftingBelt = true;
            }
            else if (!iceCrystalCrown)
                iceCrystalCrown = true;
            else if (!whirlwindWalkers)
                whirlwindWalkers = true;
            else if (!igneousBracers)
                igneousBracers = true;
        }
        else if (col.collider.tag == "BossTrigger") {
            if (!bossDoor.activeSelf)
                bossDoor.SetActive(true);
        }
    }
    void OnCollisionStay2D(Collision2D col) {
        if (col.collider.tag == "Damaging" && timer >= 5 && !whirl) {
            if (isPunching == true)
                Debug.Log("PUNCH STAY");
            else {
                Vector3 enemyLoc = col.gameObject.transform.position;
                if (enemyLoc.x > _controller.gameObject.transform.position.x)
                    hitRight = true;
                else
                    hitLeft = true;
                if ((isPunching || earthShatter) && enemyLoc.x > _controller.gameObject.transform.position.x && direction == 1) {

                }
                else if ((isPunching || earthShatter) && enemyLoc.x < _controller.gameObject.transform.position.x && direction == 0) {

                }
                else
                    PlayerDamage(25);
                timer = 0;
            }
        }
    }

    void OnCollisionExit2D(Collision2D col) {
        if (col.collider.tag == "Damaging") {
            if (isPunching == true)
                Debug.Log("PUNCH EXIT");
            else
                timer = 0;
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col.tag == "KillZ") {
            PlayerFallDeath();
        }
        else if (col.tag == "Maelstrom") {
            inMaelstrom = true;
            PlayerDamage(25);
            isDashing = false;
            hurt = true;
            canBeDamaged = false;
            isPunching = false;
            earthShatter = false;
            whirl = false;
            StartCoroutine(DamageFlash());
            timer = 0;
        }
        else if (col.tag == "Heatwave") {
            PlayerDamage(100);
            isDashing = false;
            hurt = true;
            canBeDamaged = false;
            isPunching = false;
            earthShatter = false;
            whirl = false;
            StartCoroutine(DamageFlash());
            timer = 0;
        }
        else if (col.tag == "MidnightCrow" && !whirl) {
            PlayerDamage(25);
            hurt = true;
            canBeDamaged = false;
            isDashing = false;
            isPunching = false;
            earthShatter = false;
            whirl = false;
            StartCoroutine(DamageFlash());
            timer = 0;
        }
        else if (col.tag == "Damaging" && !whirl) {
            Vector3 enemyLoc = col.gameObject.transform.position;
            if (enemyLoc.x > _controller.gameObject.transform.position.x)
                hitRight = true;
            else
                hitLeft = true;
            if ((isPunching || earthShatter) && enemyLoc.x > _controller.gameObject.transform.position.x && direction == 1) {

            } else if ((isPunching || earthShatter) && enemyLoc.x < _controller.gameObject.transform.position.x && direction == 0) {

            } else {
                if (canBeDamaged && !isBlocking) {
                    PlayerDamage(25);
                    hurt = true;
                    canBeDamaged = false;
                    isPunching = false;
                    earthShatter = false;
                    StartCoroutine(DamageFlash());
                    timer = 0;

                } else if (canBeDamaged && isBlocking && enemyLoc.x > _controller.gameObject.transform.position.x) {
                    PlayerDamage(5);
                    canBeDamaged = false;
                    StartCoroutine(DamageFlash());
                    timer = 0;
                }
            }
        }

        else if (col.tag == "SuperFireball" && !whirl) {
            Vector3 enemyLoc = col.gameObject.transform.position;
            if (enemyLoc.x > _controller.gameObject.transform.position.x)
                hitRight = true;
            else
                hitLeft = true;
            if ((isPunching || earthShatter) && enemyLoc.x > _controller.gameObject.transform.position.x && direction == 1) {

            }
            else if ((isPunching || earthShatter) && enemyLoc.x < _controller.gameObject.transform.position.x && direction == 0) {

            }
            else {
                if (canBeDamaged && !isBlocking) {
                    PlayerDamage(75);
                    hurt = true;
                    canBeDamaged = false;
                    isPunching = false;
                    earthShatter = false;
                    StartCoroutine(DamageFlash());
                    timer = 0;
                }
                else if (canBeDamaged && isBlocking && enemyLoc.x > _controller.gameObject.transform.position.x) {
                    PlayerDamage(35);
                    canBeDamaged = false;
                    StartCoroutine(DamageFlash());
                    timer = 0;
                }
            }
        }

        else if (col.tag == "Victory") {
            LoadNextLevel();
        } else if (col.tag == "NextScene") {
            LoadNextScene();
        }
        else if (col.tag == "Health") {
            PlayerHeal(50);
            Destroy(col.gameObject);
        }
        else if (col.tag == "BossTrigger") {
            Debug.Log("BOSST");
            GameManager.GameInstance.PlayBossMusic();
            if (!bossDoor.activeSelf)
                bossDoor.SetActive(true);
        }
    }

    void OnTriggerStay2D(Collider2D col) {
        if (col.tag == "Damaging" && timer >= 5 && !whirl) {
            if (isPunching == true)
                Debug.Log("PUNCH STAY");
            else {
                PlayerDamage(25);
                timer = 0;
            }

        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == "Damaging")
        {
            if (isPunching == true)
                Debug.Log("PUNCH EXIT");
            else
                timer = 0;
        }
    }


    public int getDirection () { return direction; }

    public Vector3 getPosition() {
        return this.transform.position;
    }

    public void enablePunch() {
        if (!earthShatter)
            punchCollider.enabled = true;
    }

    public void disablePunch() {
        punchCollider.enabled = false;
    }

    public void endPeacock() {
        morningPeacock = false;
        Vector3 ballPosition;
        ballPosition = this.transform.position;
        if (_animator.getFacing().Equals("Left")) {
            ballPosition.x = ballPosition.x - projectilePositionX;
        }
        else {
            ballPosition.x = ballPosition.x + projectilePositionX;
        }
        GameObject fireball = (GameObject)Instantiate(fireDragon, ballPosition, this.transform.rotation);
        fireball.GetComponent<AnimationController2D>().setFacing(_animator.getFacing());
    }

    public void enableEarthShatter() {
        if (earthShatter)
            earthCollider.enabled = true;
    }
    public void stopEarthShatter() {
        earthShatter = false;
        earthCollider.enabled = false;
        PlayerExhausted();
    }

    public void stopWhirlwind() {
        Debug.Log(whirl);
        if (whirl) {
            whirlwind.SetActive(false);
            Debug.Log("STOP");
            whirl = false;
        }
    }
    public void stopBlock() {
        isBlocking = false;
    }

    public void dash() {
        Debug.Log("Is Dashing");
    }

    public void stopDash() {
        isDashing = false;
    }

    private void PlayerFallDeath() {
        _animator.setAnimation("Death");
        currentHealth = 0;
        healthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 32);
        this.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        //gameCamera.GetComponent<CameraFollow2D>().stopCameraFollow();
        playerControl = false;
        //Debug.Log(currentHealth);
        AudioClip clip;
        PlayerSfxLibrary.TryGetValue(sfx.DEATH, out clip);
        PlaySound(clip);
    }

    private void StopPunching() {
        punchCollider.enabled = false;
        isPunching = false;
    }

    private void PlayerDeath() {
        AudioClip clip;
        PlayerSfxLibrary.TryGetValue(sfx.DEATH, out clip);
        PlaySound(clip);
        playerControl = false;
        this.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        _animator.setAnimation("Death");
    }

    public void GameOverScreen() {
        gameOverPanel.SetActive(true);
    }
    private void deletePlayer() {
        _controller.enabled = false;
    }
    private void PlayerDamage(int damage) {
        currentHealth -= damage;
        punchCollider.enabled = false;
        earthCollider.enabled = false;
        float normalizedHealth = (float)currentHealth / 100;
        healthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(normalizedHealth*173,17);
        if (changingPower)
            endChange();
        if (exhausted) {
            exhausted = false;
        }
        if (!isBlocking && !isDashing && damage > 0)
            _animator.setAnimation("Hurt");
        if (_controller.isGrounded) {
            if (canBeDamaged) {
                if (hitLeft) {
                    _controller.move(new Vector3(.5f, 0f, 0f));
                    hitLeft = false;
                }
                else if (hitRight) {
                    _controller.move(new Vector3(-.5f, 0f, 0f));
                    hitRight = false;
                }
            }
        }
        else {
            _controller.move(new Vector3(0, -.2f, 0));
            hurt = false;
        }
        AudioClip clip;
        PlayerSfxLibrary.TryGetValue(sfx.HIT, out clip);
        PlaySound(clip);
        if (currentHealth <= 0)
            PlayerDeath();
    }

    public void stopHurt() {
        if (hurt)
            hurt = false;
        if (earthShatter)
            earthShatter = false;
    }

    private void PlayerHeal(int amount) {
        currentHealth += amount;
        if (currentHealth > 100)
            currentHealth = 100;
        float normalizedHealth = (float)currentHealth / 100;
        healthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(normalizedHealth * 173, 17);
        AudioClip clip;
        PlayerSfxLibrary.TryGetValue(sfx.ITEM_PICKUP, out clip);
        PlaySound(clip);
    }

    private void PlayerExhausted() {
        AudioClip clip;
        PlayerSfxLibrary.TryGetValue(sfx.FATIGUE, out clip);
        PlaySound(clip);
        _animator.setAnimation("Fatigue");
        exhausted = true;
    }

    private void UseStamina(double staminaUsed) {
        currentStamina += staminaUsed;
        if (currentStamina > 100)
            currentStamina = 100;
        float normalizedStamina = (float)currentStamina / 100;
        staminaBar.GetComponent<RectTransform>().sizeDelta = new Vector2(normalizedStamina * 173, 16);
        //if (currentStamina <= 0)
        //    PlayerExhausted();
    }

    private void icePower() {
        Vector3 iceBlockPosition = _controller.transform.position;
        //_controller.move(new Vector3(0f, .25f, 0f));
        iceBlockPosition.y -= .2f;
        UseStamina(-50);
        GameObject ice_Block = (GameObject) Instantiate(iceBlockPrefab, iceBlockPosition, this.transform.rotation);
        AudioClip clip;
        PlayerSfxLibrary.TryGetValue(sfx.ICE_POWER, out clip);
        PlaySound(clip);
    }

    private void stopIcePower() {
        iceBlock = false;
    }

    public void endChange() {
        changingPower = false;
    }

    public void allowControl() {
        inNotification = false;
    }

    private IEnumerator DamageFlash() {
        if (currentHealth > 0) {
            for (int i = 0; i < 5; i++) {
                _color.material.color = hitColor;
                yield return new WaitForSeconds(.1f);
                _color.material.color = normalColor;
                yield return new WaitForSeconds(.1f);
            }
            canBeDamaged = true;
            if (inMaelstrom)
                inMaelstrom = false;
        }
    }
    private void LoadNextScene() {
		GameManager.GameInstance.playerHealth = currentHealth;
		GameManager.GameInstance.playerStamina = currentStamina;
		GameManager.GameInstance.playerPower = currentPower;
		GameManager.GameInstance.playerEarthPower = superLiftingBelt;
		GameManager.GameInstance.playerIcePower = iceCrystalCrown;
        GameManager.GameInstance.playerWindPower = whirlwindWalkers;
        GameManager.GameInstance.playerFirePower = igneousBracers;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
    }

	private void LoadNextLevel() {
		GameManager.GameInstance.playerHealth = 100;
		GameManager.GameInstance.playerStamina = 100;
		GameManager.GameInstance.playerPower = 0;
        GameManager.GameInstance.playerEarthPower = superLiftingBelt;
        GameManager.GameInstance.playerIcePower = iceCrystalCrown;
        GameManager.GameInstance.playerWindPower = whirlwindWalkers;
        GameManager.GameInstance.playerFirePower = igneousBracers;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
	}

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        sfxPlayer.Stop();
        sfxPlayer.clip = clip;
        sfxPlayer.Play();
    }
}
