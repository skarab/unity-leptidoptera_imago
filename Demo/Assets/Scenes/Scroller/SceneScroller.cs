using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneScroller : MonoBehaviour
{
    public RawImage Render;
    public TMPro.TextMeshProUGUI Txt;
    public TMPro.TextMeshProUGUI Ascii;
    public AudioSource Music;

    void Start()
    {
        Render.material.SetFloat("_Deform", 0.0f);

        Ascii.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        Ascii.fontSize = Render.rectTransform.rect.height*0.012f;

        Render.material.SetFloat("_FadeOut", 0.0f);
        Txt.fontSize = Render.rectTransform.rect.height*0.045f;

        Txt.rectTransform.anchoredPosition = new Vector2(0.0f, -Render.rectTransform.rect.height);
    }

    void Update()
    {
        Txt.rectTransform.anchoredPosition = new Vector2(0.0f, Txt.rectTransform.anchoredPosition.y+Time.deltaTime*(Render.rectTransform.rect.height/16.0f));
    
        if (Music.time>=307.0f)
            Ascii.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Lerp(Ascii.color.a, 1.0f, Time.deltaTime));
        else if (Music.time>=279.0f)
            Ascii.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Lerp(Ascii.color.a, 0.15f, Time.deltaTime));
    }
}
