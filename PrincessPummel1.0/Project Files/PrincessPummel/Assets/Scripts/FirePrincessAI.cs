using UnityEngine;
using System.Collections.Generic;
using Prime31;

[RequireComponent(typeof(CharacterController2D), typeof(AnimationController2D))]
public class FirePrincessAI : MonoBehaviour {

    public GameObject playerBlastDoor;
    public GameObject bossBlastDoor;
    public GameObject turret1;
    public GameObject turret2;
    public GameObject turret3;
    public GameObject player;
    public GameObject heatwave1;
    public GameObject heatwave2;
    public GameObject heatwave3;
    public GameObject healthBar;
    public GameObject portal;
    public GameObject bridge;
    public GameObject crow;

    private SpriteRenderer _turret1Color;
    private float _turret1HitTimer;
    private bool _turret1Hit;
    private SpriteRenderer _turret2Color;
    private float _turret2HitTimer;
    private bool _turret2Hit;
    private SpriteRenderer _turret3Color;
    private float _turret3HitTimer;
    private bool _turret3Hit;

    [Range(0, 10)]
    public float projectilePositionX = 1;
    [Range(-10, 10)]
    public float projectilePositionY = 0;
    public bool crowFlight;
    public int health = 100;
    public bool turretsActive;
    public bool turretSwitch;
    public float turretTimer;
    public float duration = 7f;
    private int _currentHealth = 0;
    private int _currentState = 0;
    private int _charge = 0;
    private SpriteRenderer _color;
    private CharacterController2D _controller;
    private AnimationController2D _animator;
    private float _hitTimer, _timer;
    private bool _hit;
    private AudioSource sfxPlayer;

    public AudioClip[] BossSFX = new AudioClip[5];

    private Dictionary<sfx, AudioClip> BossSfxLibrary = new Dictionary<sfx, AudioClip>();

    public enum sfx
    {
        IDLE,
        CHARGE,
        ATTACK,
        HIT,
        DEATH
    }

    // Use this for initialization
    void Start() {
        if (player == null) {
            Debug.LogError("Player not set. Please set player object.\nSource: " + this.gameObject.name);
            Destroy(this.gameObject);
            return;
        }
        turretTimer = 0;
        turretSwitch = false;
        turretsActive = false;
        _controller = this.GetComponent<CharacterController2D>();
        _animator = this.GetComponent<AnimationController2D>();
        _color = this.GetComponent<SpriteRenderer>();

        _turret1Color = turret1.GetComponent<SpriteRenderer>();
        _turret1HitTimer = 0;
        _turret1Hit = false;
        _turret1Color.color = Color.grey;

        _turret2Color = turret2.GetComponent<SpriteRenderer>();
        _turret2HitTimer = 0;
        _turret2Hit = false;
        _turret2Color.color = Color.grey;

        _turret3Color = turret3.GetComponent<SpriteRenderer>();
        _turret3HitTimer = 0;
        _turret3Hit = false;
        _turret3Color.color = Color.grey;

        _color.color = Color.white;
        _animator.setFacing("Left");
        _currentHealth = health;
        
        _timer = 0;
        _hit = false;
        crowFlight = false;
        _hitTimer = 0;

        sfxPlayer = this.GetComponent<AudioSource>();
        sfxPlayer.volume = GameManager.GameInstance.sfxVolume; sfx s = sfx.IDLE;
        foreach (AudioClip a in BossSFX)
        {
            BossSfxLibrary.Add(s, a);
            s++;
        }
        BossSFX = null;
    }

    // Update is called once per frame
    void Update() {
        if (!playerBlastDoor.activeSelf && bossBlastDoor.activeSelf) {
            if (!turretsActive) {
                turretsActive = true;
            }
            _currentState = 1;
        }

        else if (!bossBlastDoor.activeSelf)
            _currentState = 2;

        if (_timer >= duration) {
            crowFlight = false;
            playerBlastDoor.GetComponent<BlastDoor>().health = 100;
            bossBlastDoor.GetComponent<BlastDoor>().health = 100;
            playerBlastDoor.SetActive(true);
            bossBlastDoor.SetActive(true);
            _currentState = 0;
            _timer = 0;
        }

        if (crowFlight && _currentHealth > 0) {
            _timer += Time.fixedDeltaTime;
        }

        if (_currentState == 0)
        {
            AudioClip clip;
            BossSfxLibrary.TryGetValue(sfx.IDLE, out clip);
            PlaySound(clip);
            _animator.setAnimation("FirePrincessIdle");
        }
        else if (_currentState == 1)
        {
            AudioClip clip;
            BossSfxLibrary.TryGetValue(sfx.CHARGE, out clip);
            PlaySound(clip);
            _animator.setAnimation("FirePrincessCharge");
        }
        else if (_currentState == 2)
        {
            AudioClip clip;
            BossSfxLibrary.TryGetValue(sfx.ATTACK, out clip);
            PlaySound(clip);
            deactivateTurrets();
            _animator.setAnimation("FirePrincessAttack");
        }
    }

    private void FixedUpdate() {
        hitIndication();
        if (turretsActive) 
            turretFlash();
        else {
            _turret1Color.color = Color.grey;
            _turret2Color.color = Color.grey;
            _turret3Color.color = Color.grey; 
        }

    }

    private void OnCollisionEnter2D(Collision2D col) {
        if (col.collider.tag == "MorningPeacock" && _currentHealth > 0) {
            _hitTimer = 0;
            _hit = true;
            bossDamage(50);
        }

        if (col.collider.tag == "MidnightCrow" && _currentHealth > 0) {
            _hitTimer = 0;
            _hit = true;
            bossDamage(100);
        }
    }

    private void OnTriggerEnter2D(Collider2D col) {
        if (col.tag == "MidnightCrow" && _currentHealth > 0) {
            _hitTimer = 0;
            _hit = true;
            bossDamage(250);
        }

        else if (col.tag == "MorningPeacock" && _currentHealth > 0) {
            _hitTimer = 0;
            _hit = true;
            bossDamage(50);
        }
    }

    private void bossDamage(int damage) {
        AudioClip clip;
        BossSfxLibrary.TryGetValue(sfx.HIT, out clip);
        PlaySound(clip);
        _currentHealth -= damage;
        float normalizedHealth = (float)_currentHealth / 1000;
        healthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(normalizedHealth * 173, 17);
        if (_currentHealth <= 0) {
            BossSfxLibrary.TryGetValue(sfx.DEATH, out clip);
            PlaySound(clip);
            _controller.GetComponent<BoxCollider2D>().enabled = false;
            GameManager.GameInstance.PlayBossVictoryMusic();
            _animator.setAnimation("FirePrincessDefeat");
        }
    }

    private void midnightCrow() {
        crowFlight = true;
        Vector3 ballPosition;
        ballPosition = this.transform.position;
        if (_animator.getFacing().Equals("Left")) {
            ballPosition.x = ballPosition.x - projectilePositionX;
        }
        else {
            ballPosition.x = ballPosition.x + projectilePositionX;
        }
        GameObject fireball = (GameObject)Instantiate(crow, ballPosition, this.transform.rotation);
        fireball.GetComponent<AnimationController2D>().setFacing(_animator.getFacing());
    }

    public void hitIndication() {
        if (_hit) {
            _hitTimer += Time.fixedDeltaTime;
            if (_hitTimer < 0.5f && _color.color == Color.red) {
                _color.color = Color.white;
            }
            else if (_hitTimer < 0.5f && _color.color == Color.white) {
                _color.color = Color.red;
            }
            else if (_hitTimer >= 0.5f) {
                _color.color = Color.white;
                _hit = false;
                _hitTimer = 0;
            }
        }
    }

    public void turretFlash() {
        if (!heatwave1.activeSelf) {
            _turret1HitTimer += Time.fixedDeltaTime;
            if (_turret1HitTimer < 3f && _turret1Color.color == Color.red) {
                _turret1Color.color = Color.grey;
            }
            else if (_turret1HitTimer < 3f && _turret1Color.color == Color.grey) {
                _turret1Color.color = Color.red;
            }
            else if (_turret1HitTimer >= 3f) {
                _turret1Color.color = Color.grey;
                _turret1Hit = false;
                _turret1HitTimer = 0;
                heatwave1.SetActive(true);
            }
        }
        else if (!heatwave2.activeSelf) {
            _turret2HitTimer += Time.fixedDeltaTime;
            if (_turret2HitTimer < 3f && _turret2Color.color == Color.red) {
                _turret2Color.color = Color.grey;
            }
            else if (_turret2HitTimer < 3f && _turret2Color.color == Color.grey) {
                _turret2Color.color = Color.red;
            }
            else if (_turret2HitTimer >= 3f) {
                _turret2Color.color = Color.grey;
                _turret2Hit = false;
                _turret2HitTimer = 0;
                heatwave2.SetActive(true);
            }
        }
        else if (!heatwave3.activeSelf) {
            _turret3HitTimer += Time.fixedDeltaTime;
            if (_turret3HitTimer < 3.5f && _turret3Color.color == Color.red) {
                _turret3Color.color = Color.grey;
            }
            else if (_turret3HitTimer < 3.5f && _turret3Color.color == Color.grey) {
                _turret3Color.color = Color.red;
            }
            else if (_turret3HitTimer >= 3.5f) {
                _turret3Color.color = Color.grey;
                _turret3Hit = false;
                _turret3HitTimer = 0;
                heatwave3.SetActive(true);
            }
        }
    }

    public void activateTurrets() {
        if (!heatwave1.activeSelf) {
            heatwave1.SetActive(true);
        }
        else if (!heatwave2.activeSelf) {
            heatwave2.SetActive(true);
        }
        else {
            heatwave3.SetActive(true);
        }
    }

    public void deactivateTurrets() {
        turretSwitch = false;
        turretsActive = false;
        heatwave1.SetActive(false);
        heatwave2.SetActive(false);
        heatwave3.SetActive(false);
        _turret3HitTimer = 0;
        _turret2HitTimer = 0;
        _turret1HitTimer = 0;
        _turret1Color.color = Color.grey;
        _turret2Color.color = Color.grey;
        _turret3Color.color = Color.grey;
    }
    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        sfxPlayer.Stop();
        sfxPlayer.clip = clip;
        sfxPlayer.Play();
    }

    public void cleanUp() {
        portal.SetActive(true);
        bridge.SetActive(true);
    }
}
