using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
using UnityEngine.VFX;

public class SceneLogo : MonoBehaviour
{
    public Light LightData;
    public Transform LightTRS;
    public VisualEffect Particles;    
    public RawImage Render;
    public GameObject NextScene;
    public AudioSource Music;
    public Volume PPVolume;
    public TMPro.TextMeshProUGUI Txt;
    private float _Timer = 0.0f;

    private void Awake()
    {
#if !UNITY_EDITOR
        Cursor.visible = false;
#endif

        NextScene.GetComponent<SceneGameOfLife>().MyAwake();

        Render.material.SetFloat("_Deform", 0.0f);
    }

    void Start()
    {
        Txt.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        Txt.fontSize = Txt.rectTransform.rect.width*0.015f;

        LightData.intensity = 10000;

        UpdateLight();
        Particles.SetFloat("Rate", 0.0f);
        Particles.SetFloat("Strength", 0.0f);
        Render.material.SetFloat("_FadeOut", 1.0f);    
        Render.material.SetFloat("_Fade", 0.0f);

        FilmGrain grain;
        PPVolume.sharedProfile.TryGet<FilmGrain>(out grain);
        grain.intensity.value = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        _Timer += Time.deltaTime;
        UpdateLight();
    
        if (_Timer>=4.0f && !Music.isPlaying)
            Music.Play();

        if (Music.time>2.4f && Music.time<6.5f)
        {
            Render.material.SetFloat("_FadeOut", Mathf.Lerp(Render.material.GetFloat("_FadeOut"), 0.0f, Time.deltaTime*2.0f));
        }

        if (Music.time>4.5f && Music.time<20.0f)
        {
            Particles.SetFloat("Rate", 200000.0f);
        }

        if (Music.time>8.0f)
            Particles.SetFloat("Strength", Particles.GetFloat("Strength")+Time.deltaTime*6.0f);
        
        if (Music.time>=24.2f)
            Particles.SetFloat("Force", 1.0f);
        if (Music.time>=25.0f)
            Particles.SetFloat("Rate", 0.0f);
        if (Music.time>=26.0f)
        {
            LightData.intensity = Mathf.Lerp(LightData.intensity, 1000.0f, Time.deltaTime*10.0f);

            FilmGrain grain;
            PPVolume.sharedProfile.TryGet<FilmGrain>(out grain);
            grain.intensity.value = Mathf.Lerp(grain.intensity.value, 2.0f, Time.deltaTime);

            if (Music.time<32.0f)
            {
                Txt.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Lerp(Txt.color.a, 0.2f, Time.deltaTime*0.4f));
            }
            else
            {
                Txt.color = new Color(1.0f, 1.0f, 1.0f, Mathf.MoveTowards(Txt.color.a, 0.0f, Time.deltaTime));
            }
        }

        if (Music.time>=33.0f)
            Render.material.SetFloat("_FadeOut", Mathf.Lerp(Render.material.GetFloat("_FadeOut"), 1.0f, Time.deltaTime));
    
        if (Music.time>=35.0f)
        {
            FilmGrain grain;
            PPVolume.sharedProfile.TryGet<FilmGrain>(out grain);
            grain.intensity.value = 0.0f;

            gameObject.SetActive(false);
            NextScene.SetActive(true);
        }
    }

    void UpdateLight()
    {
        LightTRS.localRotation = Quaternion.Euler(new Vector3(
            LightTRS.rotation.eulerAngles.x,
            152.0f+Mathf.Sin(_Timer)*50.0f,
            LightTRS.rotation.eulerAngles.z));
    }
}
