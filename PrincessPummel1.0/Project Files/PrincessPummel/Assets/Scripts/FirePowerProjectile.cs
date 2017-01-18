using UnityEngine;
using Prime31;
using System.Collections;

public class FirePowerProjectile : MonoBehaviour {

    // Use this for initialization
    public float speed = 4;
    public float duration = 2;
    private float _timer;
    private bool _deflected;
    private AnimationController2D _animator;
    private CharacterController2D _move;

    void Start() {
        _timer = 0;
        _deflected = false;
        _animator = this.GetComponent<AnimationController2D>();
        _move = this.GetComponent<CharacterController2D>();
    }

    void FixedUpdate() {
        if (_timer >= duration || _move.collisionState.hasCollision()) {
            Destroy(this.gameObject);
        }
        else {
            _timer += Time.fixedDeltaTime;
            if (_deflected) {
                if (_animator.getFacing().Equals("Right")) {
                    _move.move(new Vector3(Time.fixedDeltaTime * speed, 0, 0));
                }
                else {
                    _move.move(new Vector3(Time.fixedDeltaTime * -speed, 0, 0));
                }
            }
            else {
                if (_animator.getFacing().Equals("Left")) {
                    _move.move(new Vector3(Time.fixedDeltaTime * -speed, 0, 0));
                }
                else {
                    _move.move(new Vector3(Time.fixedDeltaTime * speed, 0, 0));
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D _col) {

        if (_col.tag == "Damaging") {
            Destroy(this.gameObject);
        }

        else if (_col.tag == "Breakable") {
            Destroy(this.gameObject);
        }

        else if (_col.tag == "Player") {
            Destroy(this.gameObject);
        }

        else if (_col.tag == "Whirlwind") {
            if (!_deflected) {
                Debug.Log("DEFLECTED");
                if (_animator.getFacing().Equals("Left"))
                    _animator.setFacing("Right");
                else
                    _animator.setFacing("Left");
                _deflected = true;
            }
        }
    }
}
