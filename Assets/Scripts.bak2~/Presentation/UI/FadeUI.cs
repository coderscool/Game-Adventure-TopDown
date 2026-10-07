using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FadeUI : MonoBehaviour
{
    public static FadeUI Instance;

    public Image fadeImage;
    public float duration = 0.5f;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator FadeOut()
    {
        yield return Fade(0, 1);
    }

    public IEnumerator FadeIn()
    {
        yield return Fade(1, 0);
    }

    IEnumerator Fade(float from, float to)
    {
        float time = 0;

        while (time < duration)
        {
            float alpha = Mathf.Lerp(from, to, time / duration);
            fadeImage.color = new Color(0, 0, 0, alpha);

            time += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, to);
    }
}
