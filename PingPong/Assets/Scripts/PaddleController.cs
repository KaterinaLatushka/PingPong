using UnityEngine;

public class PaddleController : MonoBehaviour
{
    public float speed = 10f;
    public string inputAxis = "Vertical"; // set to "Vertical" or "Vertical2" per paddle

    void Update()
    {
        float move = Input.GetAxisRaw(inputAxis) * speed * Time.deltaTime;
        transform.Translate(0f, move, 0f);

        // Keep paddles on-screen (adjust bounds for your camera size)
        Vector3 p = transform.position;
        p.y = Mathf.Clamp(p.y, -4f, 4f);
        transform.position = p;
    }
}
