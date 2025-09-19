using System.Collections;
using UnityEngine;

public class BackgroundColorChanger : MonoBehaviour
{
    [Header("Flash Colors")]
    public Color player1ScoreColor = new Color(0.2f, 0.6f, 1f);
    public Color player2ScoreColor = new Color(1f, 0.4f, 0.3f);

    [Header("Timings")]
    public float flashTime = 0.15f;
    public float settleTime = 0.6f;

    Camera cam;
    Color baseColor;
    Coroutine current;

    void Awake()
    {
        cam = Camera.main;
        baseColor = cam.backgroundColor;
    }

    public void FlashForPlayer(int playerIndex)
    {
        Color target = playerIndex == 1 ? player1ScoreColor : player2ScoreColor;
        if (current != null) StopCoroutine(current);
        current = StartCoroutine(FlashRoutine(target));
    }

    IEnumerator FlashRoutine(Color target)
    {
        float t = 0f; Color start = cam.backgroundColor;
        while (t < flashTime)
        {
            t += Time.unscaledDeltaTime;
            cam.backgroundColor = Color.Lerp(start, target, t / flashTime);
            yield return null;
        }
        cam.backgroundColor = target;

        t = 0f;
        while (t < settleTime)
        {
            t += Time.unscaledDeltaTime;
            cam.backgroundColor = Color.Lerp(target, baseColor, t / settleTime);
            yield return null;
        }
        cam.backgroundColor = baseColor;
        current = null;
    }
}
