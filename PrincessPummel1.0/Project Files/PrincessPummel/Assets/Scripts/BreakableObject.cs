using UnityEngine;

public class BreakableObject : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D _col) {
        if (_col.tag == "EarthShatter") {
            this.gameObject.SetActive(false);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.collider.tag == "EarthShatter") {
            this.gameObject.SetActive(false);
        }
    }
}
