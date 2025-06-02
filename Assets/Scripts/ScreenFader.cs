using UnityEngine;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1.5f;
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        Color c = sr.color;
        c.a = 0f;
        sr.color = c;
    }

    public void FadeIn() => StartCoroutine(Fade(0f, 1f));
    public void FadeOut() => StartCoroutine(Fade(1f, 0f));

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        Color c = sr.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            c.a = Mathf.Lerp(from, to, t);
            sr.color = c;
            yield return null;
        }

        c.a = to;
        sr.color = c;
    }
}
