using UnityEngine;
using System.Collections.Generic;
using Prime31;

[RequireComponent(typeof(CharacterController2D), typeof(AnimationController2D))]
public class IcePrincessAI : MonoBehaviour {

    public GameObject iceBlock;
    public GameObject player;
    public GameObject maelstrom;
    public GameObject healthBar;
    public GameObject portal;
    public bool iceWallAlive;
    public bool maelstromActive;
    public int health = 100;
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
    void Start () {
        if (player == null) {
            Debug.LogError("Player not set. Please set player object.\nSource: " + this.gameObject.name);
            Destroy(this.gameObject);
            return;
        }

        _controller = this.GetComponent<CharacterController2D>();
        _animator = this.GetComponent<AnimationController2D>();
        _color = this.GetComponent<SpriteRenderer>();
        _color.color = Color.white;
        _animator.setFacing("Left");
        _currentHealth = health;
        _timer = 0;
        _hit = false;
        maelstromActive = false;
        _hitTimer = 0;
        iceWallAlive = false;
        sfxPlayer = this.GetComponent<AudioSource>();
        sfxPlayer.volume = GameManager.GameInstance.sfxVolume;
        sfx s = sfx.IDLE;
        foreach (AudioClip a in BossSFX)
        {
            BossSfxLibrary.Add(s, a);
            s++;
        }
        BossSFX = null;
    }

    // Update is called once per frame
    void Update () {
        if (!iceBlock.activeSelf) {
            _currentState = 1;
        }

        if (_timer >= duration) {
            maelstromActive = false;
            maelstrom.SetActive(false);
            iceBlock.SetActive(true);
            _currentState = 0;
            _timer = 0;
        }

        if (maelstromActive) {
            _timer += Time.fixedDeltaTime;
        }

        if (_currentState == 0)
        {
            _animator.setAnimation("IcePrincessIdle");
            AudioClip clip;
            BossSfxLibrary.TryGetValue(sfx.IDLE, out clip);
            PlaySound(clip);
        }
        else if (_currentState == 1)
        {
            _animator.setAnimation("IcePrincessFocus");
            AudioClip clip;
            BossSfxLibrary.TryGetValue(sfx.CHARGE, out clip);
            PlaySound(clip);
        }
        else if (_currentState == 2)
        {
            _animator.setAnimation("IcePrincessAttack");
            AudioClip clip;
            BossSfxLibrary.TryGetValue(sfx.ATTACK, out clip);
            PlaySound(clip);
        }
	}

    private void FixedUpdate() {
        hitIndication();
    }

    private void OnCollisionEnter2D(Collision2D col) {
        if (col.collider.tag == "EarthShatter" && _currentHealth > 0) {
            _hitTimer = 0;
            _hit = true;
            bossDamage(100);
        }

        if (col.collider.tag == "Attack" && _currentHealth > 0) {
            _hitTimer = 0;
            _hit = true;
            bossDamage(50);
        }
    }

    private void charge() {
        if (_charge < 3)
            _charge++;
        else {
            _currentState = 2;
            _charge = 0;
            _animator.setAnimation("IcePrincessAttack");
        }
    }

    private void bossDamage(int damage) {
        AudioClip clip;
        BossSfxLibrary.TryGetValue(sfx.HIT, out clip);
        PlaySound(clip);
        _currentHealth -= damage;
        float normalizedHealth = (float)_currentHealth / health;
        healthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(normalizedHealth * 173, 17);
        if (_currentHealth <= 0) {
            BossSfxLibrary.TryGetValue(sfx.DEATH, out clip);
            PlaySound(clip);
            _controller.GetComponent<BoxCollider2D>().enabled = false;
            GameManager.GameInstance.PlayBossVictoryMusic();
            _animator.setAnimation("IcePrincessDefeat");
        }
    }

    private void maelstromActivate() {
        maelstromActive = true;
        maelstrom.SetActive(true);
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
    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        sfxPlayer.Stop();
        sfxPlayer.clip = clip;
        sfxPlayer.Play();
    }

    public void cleanUp() {
        //Destroy(this.gameObject);
        portal.SetActive(true);
    }
}
