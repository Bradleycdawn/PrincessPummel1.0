using UnityEngine;
using System.Collections;

public class CameraFollow_v2 : MonoBehaviour {

    public Transform Player;

    public Vector2 Margin, Smoothing;

    public BoxCollider2D Bounds;

    public Camera localCamera;

    private Vector3 _min, _max;

    public bool isFollowing { get; set; }

    public void Start() {
        _min = Bounds.bounds.min;
        _max = Bounds.bounds.max;

        isFollowing = true;
    }

    public void Update() {
        var x = transform.position.x;
        var y = transform.position.y;

        if (isFollowing) {
            if (Mathf.Abs(x - Player.position.x) > Margin.x)
                x = Mathf.Lerp(x, Player.position.x, Smoothing.x * Time.deltaTime);

            if (Mathf.Abs(y = Player.position.y) > Margin.y)
                y = Mathf.Lerp(y, Player.position.y, Smoothing.y * Time.deltaTime);
        }

        var cameraHalfWidth = localCamera.orthographicSize * ((float)Screen.width / Screen.height);

        x = Mathf.Clamp(x, _min.x + cameraHalfWidth, _max.x - cameraHalfWidth);
        y = Mathf.Clamp(y, _min.y + localCamera.orthographicSize, _max.y - localCamera.orthographicSize);

        transform.position = new Vector3(x, y, transform.position.z);
    }

}
