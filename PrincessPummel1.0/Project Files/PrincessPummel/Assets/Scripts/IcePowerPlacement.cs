using UnityEngine;
using Prime31;
using System.Collections;

public class IcePowerPlacement : MonoBehaviour {

    public float duration = 5.0f;
    private float _timer;


    void Start () {
        _timer = 0;
    }
	
	void Update () {
        if (_timer >= duration) {
            Destroy(this.gameObject);
        }
        else {
            _timer += Time.fixedDeltaTime;
        }
    }
}
