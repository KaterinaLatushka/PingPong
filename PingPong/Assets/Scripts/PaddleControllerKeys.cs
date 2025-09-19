using UnityEngine;

public class PaddleControllerKeys : MonoBehaviour
{
    public float speed = 10f;
    public KeyCode upKey = KeyCode.W;
    public KeyCode downKey = KeyCode.S;

    private float topLimit;
    private float bottomLimit;

    void Start()
    {
        // find top and bottom walls
        var topWall = GameObject.Find("TopWall").GetComponent<BoxCollider2D>();
        var bottomWall = GameObject.Find("BottomWall").GetComponent<BoxCollider2D>();

        float halfPaddle = GetComponent<BoxCollider2D>().bounds.extents.y;

        // define clamp limits based on wall colliders
        topLimit = topWall.bounds.min.y - halfPaddle;
        bottomLimit = bottomWall.bounds.max.y + halfPaddle;
    }

    void Update()
    {
        float dir = 0f;
        if (Input.GetKey(upKey)) dir += 1f;
        if (Input.GetKey(downKey)) dir -= 1f;

        // move paddle
        transform.Translate(0f, dir * speed * Time.deltaTime, 0f);

        // clamp paddle position
        var p = transform.position;
        p.y = Mathf.Clamp(p.y, bottomLimit, topLimit);
        transform.position = p;
    }
}
