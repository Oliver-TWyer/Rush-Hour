using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeManagerScript : MonoBehaviour
{

    [SerializeField] CanvasGroup cg;
    public void LoadFadePanel(CanvasGroup canvasGroup, float fadeDuration)
    {
        StartCoroutine(Fade(canvasGroup, fadeDuration));
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float fadeDuration)
    {
        float timePassed = 0;

        while (timePassed < fadeDuration)
        {
            timePassed = timePassed + Time.deltaTime; //fade timer
            cg.alpha = Mathf.Lerp(0f, 1f, timePassed / fadeDuration);

            yield return null;
        }

        cg.alpha = 1f;
    }
}
