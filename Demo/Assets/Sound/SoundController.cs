using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
using UnityEngine.VFX;

public class SoundController : MonoBehaviour
{
    public Volume PPVolume;
    public Volume PPVolumeEnd;
    public Renderer UpdateBar;
    public AudioSource Music;
    public VisualEffect ParticlesBorn;
    public VisualEffect ParticlesDeath;
    public SceneGameOfLife Gol;
    public RectTransform SoundUI;
    public RectTransform BarLow;
    public RectTransform BarHigh;
    public RectTransform Render;
    public float Bass;

    void Start()
    {
        ParticlesBorn.SetFloat("Rate", 0.0f);
        ParticlesDeath.SetFloat("Rate", 0.0f);

        float w = Render.rect.height*0.3f;
        SoundUI.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        SoundUI.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, w);
        float border = w*0.1f;
        SoundUI.anchoredPosition = new Vector2(border, border);

        ColorAdjustments c_a;
        PPVolume.sharedProfile.TryGet<ColorAdjustments>(out c_a);
        c_a.saturation.value = 0.0f;

        PaniniProjection pp;
        PPVolume.sharedProfile.TryGet<PaniniProjection>(out pp);
        LensDistortion ld;
        PPVolume.sharedProfile.TryGet<LensDistortion>(out ld);
        pp.active = true;
        ld.active = true;
    }

     
    void Update()
    {
        float[] spectrum = new float[256];
        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
        float v = 0.0f;
        for (int i=0 ; i<10 ; ++i)
            v += spectrum[i];

        Bass = v;

        ChromaticAberration ca;
        PPVolume.sharedProfile.TryGet<ChromaticAberration>(out ca);
        ca.intensity.value = v;
        PPVolumeEnd.sharedProfile.TryGet<ChromaticAberration>(out ca);
        ca.intensity.value = v;

        UpdateBar.gameObject.SetActive((Music.time>=56.9f && Music.time<82.6f)
            || (Music.time>=153.2f && Music.time<197.3f));

        UpdateBar.transform.localScale = new Vector3(UpdateBar.transform.localScale.x, UpdateBar.transform.localScale.y, Music.time>153.0f?v*10.0f:0.1f);

        BarLow.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, SoundUI.rect.height*v);
            
        if (Music.time>=43.0f && Music.time<79.5f)
        {
            ParticlesBorn.SetFloat("Rate", 0.0f);
            ParticlesDeath.SetFloat("Rate", 10000.0f);        
        }
        else if (Music.time>=125.5f && Music.time<196.35)
        {
            ParticlesBorn.SetFloat("Rate", 10000.0f);
            ParticlesDeath.SetFloat("Rate", 10000.0f);
        }
        else
        {
            ParticlesBorn.SetFloat("Rate", 0.0f);
            ParticlesDeath.SetFloat("Rate", 0.0f);
        }

        v = 0.0f;
        for (int i=210 ; i<256 ; ++i)
            v += spectrum[i];
       
        BarHigh.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, SoundUI.rect.height*v*10.0f);

        SoundUI.gameObject.SetActive(UpdateBar.gameObject.activeSelf && Music.time<153.0f);

        if (Music.time<83.5f || Music.time>199.75f)
            v = 0.0f;

        Gol.CameraJittering = v*0.5f;
    

        ColorAdjustments c_a;
        PPVolume.sharedProfile.TryGet<ColorAdjustments>(out c_a);
        
        if (Music.time>=124.90f) c_a.saturation.value = 0.0f;
        else if (Music.time>97.0f) c_a.saturation.value = Mathf.Lerp(c_a.saturation.value, -100.0f, Time.deltaTime*0.05f); 

        PaniniProjection pp;
        PPVolume.sharedProfile.TryGet<PaniniProjection>(out pp);
        LensDistortion ld;
        PPVolume.sharedProfile.TryGet<LensDistortion>(out ld);
        if (Music.time>153.0f)
        {
            pp.active = false;
            ld.active = false;
        }

#if !UNITY_EDITOR
        if (Music.time>=322.0f || Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
#endif
    }
}
