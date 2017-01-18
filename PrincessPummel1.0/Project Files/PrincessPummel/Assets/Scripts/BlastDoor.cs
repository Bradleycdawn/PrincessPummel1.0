using UnityEngine;
using System.Collections;

public class BlastDoor : MonoBehaviour {

    public float health;
    private SpriteRenderer _color;
    private float _hitTimer;
    private bool _hit;

    // Use this for initialization
    void Start() {

        _color = this.GetComponent<SpriteRenderer>();
        _color.color = Color.grey;
        _hit = false;
        _hitTimer = 0;
    }
    private void FixedUpdate() {
        hitIndication();
    }
    // Use this for initialization
    void OnTriggerEnter2D(Collider2D _col) {
        if (_col.tag == "MorningPeacock") {
            health -= 25;
            _hitTimer = 0;
            _hit = true;
            if (health <= 0) {
                this.gameObject.SetActive(false);
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.collider.tag == "EarthShatter") {
            health -= 100;
            _hitTimer = 0;
            _hit = true;
            if (health <= 0) {
                this.gameObject.SetActive(false);
            }
        }
    }

    public void hitIndication() {
        if (_hit) {
            _hitTimer += Time.fixedDeltaTime;
            if (_hitTimer < 0.5f && _color.color == Color.red) {
                _color.color = Color.grey;
            }
            else if (_hitTimer < 0.5f && _color.color == Color.grey) {
                _color.color = Color.red;
            }
            else if (_hitTimer >= 0.5f) {
                _color.color = Color.grey;
                _hit = false;
                _hitTimer = 0;
            }
        }
    }
}
