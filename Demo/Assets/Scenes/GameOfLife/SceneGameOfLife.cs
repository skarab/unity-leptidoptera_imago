using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
using UnityEngine.VFX;

// 0: slow particles
// 1: slow bar
// 2: slow
// 3: medium
// 4: fast

public class SceneGameOfLife : MonoBehaviour
{
    public Transform Root;
    public GameObject NodePrefab;
    public VisualEffect ParticlesBorn;
    public VisualEffect ParticlesDeath;
    public VisualEffect CubeSmoke;
    public RectTransform GridUI;
    public GameObject NodeUI;
    public RectTransform PositionUI;
    public TMPro.TextMeshProUGUI CountTxt;
    public Transform CubeRoot;
    public GameObject CubeNodePrefab; 
    public RawImage Render;
    public Transform CubeUpdateBar;
    public Volume PPVolume;
    public AudioSource Music;
    public TMPro.TextMeshProUGUI Title;
    public TMPro.TextMeshProUGUI Subtitle;
    public GameObject NextScene;
    private const int _GridSize = 32;
    private bool[,,] _Grids;
    private int _CurrentGrid = 0;
    private int _Count = 0;
    private GameObject[,] _Nodes;
    private GameObject[,] _NodesUI;
    private GameObject[,] _NodesCube;
    private int _UpdateStep = 0;
    private const float _UpdateTime = 0.05f;
    private float _UpdateTimer = 0.0f;
    private const float _NodeSpacing = 0.20f;
    private int _Created = 0;
    private int _Destroyed = 0;
    private int _Gen = 0;
    private int _Rnd = 0;
    private float _Timer = 0.0f;
    private float[] _Timings = { 35.00f, 44.0f, 49.15f, 54.68f, 57.00f, 64.00f, 69.53f, 71.73f, 76.06f, 82.82f, 89.46f, 109.64f, 111.69f, 117.59f, 124.90f, 136.98f, 139.83f, 143.50f, 148.90f, 153.0f, 158.26f, 163.09f, 168.00f, 174.58f, 180.81f, 186.50f, 192.50f, 197.22f, 199.75f, 203.00f, 210.10f, 216.35f, 225.36f, 230.15f, 999.0f };
    private int[] _States = { 2, 0, 0, 2, 1, 4, 4, 4, 4, 2, 2, 2, 2, 2, 0, 2, 3, 3, 0, 4, 4, 4, 4, 4, 4, 3, 3, 2, 3, 3, 2, 2, 2, 2 };
    private float[] _Speeds = { 0.1f, 0.15f, 0.2f, 0.5f, 1.0f };
    private int _CameraID = -1;
    public Transform Cam;
    public Transform CamCube;
    public Transform[] States;
    private float _CameraSpeed = 0.0f;
    private Vector3 _CameraPosition = Vector3.zero;
    private Vector3 _CameraDirection = Vector3.zero;
    private Quaternion _CameraRotation = Quaternion.identity;
    public float CameraJittering = 0.0f;
    private float _GridWidth;
    private int _OldState = -1;
    private int _OldN = -1;
    public Transform RayTriangle;
    public Transform UnknownObject;
    public SoundController SoundControl;
    public Texture2D[] ScreenDeforms;
    private float _ScreenDeformsRandom = 0.0f;
    private float[] _ScreenDeformTimers = { 56.9f, 75.68f, 78.09f, 80.53f, 86.32f, 88.7f, 91.31f, 95.94f, 172.48f };
    private float _ScreenDeformTimer = 0.0f;

    public void MyAwake()
    {
        Random.InitState((int)System.DateTime.Now.Ticks);
        
        DepthOfField dof;
        PPVolume.sharedProfile.TryGet<DepthOfField>(out dof);
        dof.nearFocusEnd.value = 400.0f;
    
        CubeSmoke.gameObject.SetActive(false);

        Title.enabled = false;
        Subtitle.enabled = false;
        CreateGrid();
    }

    void Start()
    {
        Title.enabled = true;
        Title.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        Title.fontSize = Render.rectTransform.rect.height*0.10f;

        Subtitle.fontSize = Render.rectTransform.rect.height*0.04f;

        _GridWidth = Render.rectTransform.rect.height*0.3f;
        GridUI.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _GridWidth);
        GridUI.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _GridWidth);
        float border = _GridWidth*0.1f;
        GridUI.anchoredPosition = new Vector2(Render.rectTransform.rect.width-_GridWidth-border, border);
        CountTxt.fontSize = _GridWidth*0.08f;
        CountTxt.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        RectTransform trs = CountTxt.GetComponent<RectTransform>();
        trs.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _GridWidth);
        trs.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CountTxt.fontSize*2.0f);

        Render.material.SetFloat("_Fade", 1.0f);

        FilmGrain grain;
        PPVolume.sharedProfile.TryGet<FilmGrain>(out grain);
        grain.intensity.value = 0.5f;

        Update();
    }

    // Update is called once per frame
    void Update()
    {
        Render.material.SetFloat("_Deform", Mathf.MoveTowards(Render.material.GetFloat("_Deform"), 0.0f, Time.deltaTime*0.8f));
        
        for (int i=0 ; i<_ScreenDeformTimers.Length ; ++i)
        {
            if (Music.time>=_ScreenDeformTimers[i] && _ScreenDeformTimer<_ScreenDeformTimers[i])
                Render.material.SetFloat("_Deform", 1.0f);
        }

        if (Music.time>110.0f && Music.time<124.0f) 
            Render.material.SetFloat("_Deform", Mathf.Lerp(0.0f, 1.0f, (Music.time-110.0f)/(124.0f-110.0f)));

        _ScreenDeformTimer = Music.time;
        
        _ScreenDeformsRandom +=Time.deltaTime;
        if (_ScreenDeformsRandom>0.4f)
        {
            Render.material.SetFloat("_DeformInvert", Random.value);
            _ScreenDeformsRandom = 0.0f;
        }
        Render.material.SetTexture("_DeformTex", ScreenDeforms[((int)(Music.time*20.0f))%ScreenDeforms.Length]);

        if (Music.time>=37.8f && Title.enabled)
        {
            if (Music.time<40.0f)
            {
                Title.color = new Color(Title.color.r, Title.color.g, Title.color.b, Mathf.Lerp(Title.color.a, 0.8f, Time.deltaTime*5.0f));
                Title.fontSize = Render.rectTransform.rect.height*0.08f;
            }
            else if (Music.time<42.60f)
            {
                Title.fontSize = Render.rectTransform.rect.height*0.10f;
            }
            else
            {
                Title.color = new Color(Title.color.r, Title.color.g, Title.color.b, Mathf.MoveTowards(Title.color.a, 0.0f, Time.deltaTime*4.0f));
                if (Title.color.a<=0.0f)
                    Title.enabled = false;
            }
        }

        if (Music.time>103.0f && Music.time<124.90f)
        {
            FilmGrain grain;
            PPVolume.sharedProfile.TryGet<FilmGrain>(out grain);
            grain.intensity.value = Mathf.Lerp(0.0f, 4.0f, (Music.time-103.0f)/(124.90f/103.0f));
        }
        if (Music.time>=54.66f && Music.time<56.922f)
        {
            FilmGrain grain;
            PPVolume.sharedProfile.TryGet<FilmGrain>(out grain);
            grain.intensity.value = 1.0f;
        }
        else if (_Timer<12.0f)
        {
            FilmGrain grain;
            PPVolume.sharedProfile.TryGet<FilmGrain>(out grain);
            grain.intensity.value = 1.0f;
        }
        else
        {
            FilmGrain grain;
            PPVolume.sharedProfile.TryGet<FilmGrain>(out grain);
            grain.intensity.value = Mathf.MoveTowards(grain.intensity.value, 0.0f, Time.deltaTime*0.3f);
        }

        if (Music.time>=_Timings[_CameraID+1])
        {
            if (_CameraID+1<_Timings.Length)
               ++_CameraID;

            if (_CameraID<_States.Length)
            {
                int state = _States[_CameraID];
                int n = _Count%States[state].childCount;

                if ((state==_OldState) && (n==_OldN))
                    n = (n+1)%States[state].childCount;

                
                _OldState = state;
                _OldN = n;

                //Debug.Log("Cam: "+state.ToString()+" "+n.ToString());

                Transform trs = States[state].GetChild(n);
                _CameraPosition = trs.position;
                Cam.rotation = trs.rotation;
                _CameraSpeed = _Speeds[state];

                _CameraDirection = (trs.GetChild(0).position-trs.position).normalized;
                _CameraRotation = trs.GetChild(0).rotation;
            }
        }

        Cam.rotation = Quaternion.Lerp(Cam.rotation, _CameraRotation, Time.deltaTime*_CameraSpeed*0.2f);
        _CameraPosition += _CameraDirection*Time.deltaTime*_CameraSpeed;
        Cam.position = _CameraPosition+Cam.transform.right*Mathf.Sin(Music.time*20.0f)*CameraJittering;
        
        CamCube.position = Cam.position;
        CamCube.rotation = Cam.rotation;

        _Timer += Time.deltaTime;
        
        Subtitle.enabled = false;
        if (Music.time>=54.66f && Music.time<56.922f)
        {
            Subtitle.enabled = true;
            GridUI.gameObject.SetActive(true);
            GridUI.anchoredPosition = new UnityEngine.Vector2(
                (Render.rectTransform.rect.width-GridUI.rect.width)/2.0f,
                (Render.rectTransform.rect.height-GridUI.rect.height)/2.0f);
        
            Render.material.SetFloat("_FadeOut", 1.0f);        
        }
        else if (_Timer>2.0f)
        {
            float border = GridUI.rect.width*0.1f;
            Vector2 p = new Vector2(Render.rectTransform.rect.width-GridUI.rect.width-border, border);;
            GridUI.anchoredPosition = new Vector2(Mathf.Lerp(GridUI.anchoredPosition.x, p.x, Time.deltaTime*5.0f),
                Mathf.Lerp(GridUI.anchoredPosition.y, p.y, Time.deltaTime*5.0f));
        
            DepthOfField dof;
            PPVolume.sharedProfile.TryGet<DepthOfField>(out dof);
            dof.nearFocusEnd.value = Mathf.Lerp(dof.nearFocusEnd.value, 1.0f, Time.deltaTime*0.5f);

            Render.material.SetFloat("_FadeOut", Mathf.Lerp(Render.material.GetFloat("_FadeOut"), 0.0f, Time.deltaTime*0.1f));
        }
        
        float t = Mathf.PingPong(Music.time/10.0f, 1.0f);
        if (t<0.30f) t = 0.0f;
        else if (t>0.70f) t = 1.0f;
        else t = (t-0.30f)/0.40f;

        bool enable_butterfly_cam = true;
        bool enable_cubes_cam = true;

        if (Music.time<153.0f)
        {
            t = 1.0f;
            enable_butterfly_cam = false;
        }
        else if (Music.time<203.0f)
        {
            t = 0.0f;
            enable_cubes_cam = false;
        }
        else if (Music.time>240.0f)
        {
            t = 0.0f;
        }

        //if (Music.time>=109.64f && GridUI.gameObject.activeInHierarchy)
        //    GridUI.gameObject.SetActive(false);

        Cam.GetComponent<Camera>().enabled = enable_butterfly_cam;
        CamCube.GetComponent<Camera>().enabled = enable_cubes_cam;

        Render.material.SetFloat("_Fade", t);
    
        UpdateGrid();

        if (Music.time>=254.0f)
        {
            Render.material.SetFloat("_FadeOut", Mathf.MoveTowards(Render.material.GetFloat("_FadeOut"), 1.0f, Time.deltaTime*0.33f));

            if (Render.material.GetFloat("_FadeOut")==1.0f)
            {
                GridUI.gameObject.SetActive(false);
                gameObject.SetActive(false);
                NextScene.SetActive(true);
            }    
        }

        bool sphere_enabled = (Music.time>94.0f) && (Music.time<126.5f);
        if (sphere_enabled!=UnknownObject.gameObject.activeSelf)
            UnknownObject.gameObject.SetActive(sphere_enabled);

        UnknownObject.position = new Vector3(Mathf.Lerp(0.0f, _NodeSpacing*_GridSize, (Music.time-93.0f)/(122.5f-93.0f)), UnknownObject.position.y, _NodeSpacing*_GridSize*0.5f);
        float scale = Mathf.MoveTowards(0.01f, 1.0f, (Music.time-94.0f)/(120.5f-94.0f));
        if (Music.time>122.5f)
            scale = Mathf.Lerp(1.0f, 0.0f, (Music.time-122.5f)/(126.5f-122.5f));
        scale *= 1.8f;
        scale *= 1.0f+SoundControl.Bass;
        scale = Mathf.Clamp01(scale);
        UnknownObject.localScale = new Vector3(scale, scale, scale);

        if (!CubeSmoke.gameObject.activeSelf && Music.time>203.0f)
            CubeSmoke.gameObject.SetActive(true);
    }

    void CreateGrid()
    {
        _Grids = new bool[2, _GridSize, _GridSize];
        _Nodes = new GameObject[_GridSize, _GridSize];
        _NodesUI = new GameObject[_GridSize, _GridSize];
        _NodesCube = new GameObject[_GridSize, _GridSize];

        for (int x=0 ; x<_GridSize ; ++x)
        for (int y=0 ; y<_GridSize ; ++y)
        {
            _Nodes[x, y] = null;
            _NodesUI[x, y] = null;
            _NodesCube[x, y] = null;
        
            if (Random.value>0.8f)
            {
                _Grids[0, x, y] = true;
                CreateNode(x, y);
            }
            else
            {
                _Grids[0, x, y] = false;                
            }
        }

        CubeUpdateBar.localScale = new Vector3(0.2f, _NodeSpacing*_GridSize*1.4f, CubeUpdateBar.localScale.z);
    }

    void UpdateGrid()
    {
        _UpdateTimer += Time.deltaTime;
        if (_UpdateTimer<_UpdateTime)
            return;
        _UpdateTimer = 0.0f;

        int next_grid = (_CurrentGrid+1)%2;
        int x = _UpdateStep;

        _Created = 0;
        _Destroyed = 0;

        for (int y=0 ; y<_GridSize ; ++y)
        {
            int alive = 0;

            for (int i=x-1 ; i<=x+1 ; ++i)
            {
                if (i>=0 && i<_GridSize)
                {
                    for (int j=y-1 ; j<=y+1 ; ++j)
                    {
                        if (j>=0 && j<_GridSize && !(i==x && j==y))
                        {
                            if (_Grids[_CurrentGrid, i, j])
                                ++alive;
                        }
                    }
                }
            }

            _Grids[next_grid, x, y] = _Grids[_CurrentGrid, x, y];

            if (!_Grids[_CurrentGrid, x, y] && alive==3)
                _Grids[next_grid, x, y] = true;

            if (_Grids[_CurrentGrid, x, y] && (alive<2 || alive>3))
                _Grids[next_grid, x, y] = false;

            if (!_Grids[_CurrentGrid, x, y] && _Grids[next_grid, x, y])
                CreateNode(x, y);
            if (_Grids[_CurrentGrid, x, y] && !_Grids[next_grid, x, y])
                DestroyNode(x, y);   
            
            if (_Count<80)
            {
                for (int i=0 ; i<10 ; ++i)
                {
                    int nx = (int)(Random.value*(_GridSize-1.0f));
                    int ny = (int)(Random.value*(_GridSize-1.0f));
                    if (!_Grids[next_grid, nx, ny])
                    {
                        _Grids[_CurrentGrid, nx, ny] = true;
                        _Grids[next_grid, nx, ny] = true;
                        CreateNode(nx, ny);
                    }
                }

                if (_Rnd<999)
                    ++_Rnd;
            }
        }

        ++_UpdateStep;

        CubeUpdateBar.position = new Vector3(Mathf.MoveTowards(CubeUpdateBar.position.x, _UpdateStep*_NodeSpacing, Time.deltaTime/_UpdateTime), -0.53f, 3.93f); //_NodeSpacing*_GridSize/2.0f);

        if (_UpdateStep>=_GridSize)
        {
            CubeUpdateBar.position = new Vector3(0.0f, -0.53f, _NodeSpacing*_GridSize/2.0f);

            _UpdateStep = 0;
            _CurrentGrid = next_grid;

            if (_Gen<999)
                ++_Gen;
        }

        RayTriangle.position = new Vector3(4.07f, 1.73f, 3.93f);
        
        RayTriangle.localScale = new Vector3(
            Vector3.Distance(RayTriangle.position, CubeUpdateBar.position)/1.8f,
            2.0f,
            _NodeSpacing*_GridSize*0.8f/1.2f);

        Vector3 a_ray = new Vector3(-1.0f, 0.0f, 0.0f);
        Vector3 a_vec = (CubeUpdateBar.position-RayTriangle.position).normalized;
        RayTriangle.rotation = Quaternion.Euler(0.0f, 0.0f, Vector3.Angle(a_ray, a_vec));
    
        bool enabled = CubeUpdateBar.gameObject.activeSelf && Music.time>172.48f;

        if (RayTriangle.gameObject.activeSelf != enabled)
            RayTriangle.gameObject.SetActive(enabled);

        PositionUI.anchoredPosition = new Vector2(
            5.0f+(GridUI.rect.width-10.0f)*_UpdateStep/(_GridSize-1.0f),
            PositionUI.anchoredPosition.y);

        CountTxt.text = "<color=#00F000>g"+_Gen.ToString()+"</color> <color=#282828>r"+_Rnd.ToString()+"</color> <color=#5D99FF>c"+_Count.ToString()+"</color> <color=#FFFFFF>+"+_Created.ToString()+"</color> <color=#282828>-"+_Destroyed.ToString()+"</color>";
    }

    void CreateNode(int x, int y)
    {
        if (_Nodes[x, y]!=null)
        {
            //Debug.LogError("node already exists");
        }
        else
        {
            ParticlesBorn.SetVector3("Position", new Vector3(x*_NodeSpacing, 0.0f, y*_NodeSpacing));

            _Nodes[x, y] = Object.Instantiate<GameObject>(
                NodePrefab,
                new Vector3(x*_NodeSpacing, 0.0f, y*_NodeSpacing),
                Quaternion.Euler(new Vector3(Random.value*360.0f, Random.value*360.0f, Random.value*360.0f)),
                Root);
            float scale = 1.0f+(Random.value-0.5f);
            _Nodes[x, y].transform.localScale = new Vector3(scale, scale, scale);

            _NodesUI[x, y] = Object.Instantiate<GameObject>(NodeUI);
            _NodesUI[x, y].transform.SetParent(GridUI, false);
    
            float size = _GridWidth/(_GridSize+2.0f);
            RectTransform trs = _NodesUI[x, y].GetComponent<RectTransform>();
            trs.pivot = Vector2.zero;
            trs.anchorMin = Vector2.zero;
            trs.anchorMax = Vector2.zero;
            trs.anchoredPosition = new Vector2(size*(1.0f+x), size*(1.0f+y));
            trs.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size*0.9f);
            trs.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size*0.9f);

            scale *= 0.1f;
            _NodesCube[x, y] = Object.Instantiate<GameObject>(
                CubeNodePrefab,
                new Vector3(x*_NodeSpacing, 0.0f, y*_NodeSpacing),
                Quaternion.identity,
                CubeRoot);
            _NodesCube[x, y].transform.localScale = new Vector3(scale, scale, scale);

            if (Music.time>203.0f)
            {
                _NodesCube[x, y].GetComponent<Renderer>().enabled = false;
                _NodesCube[x, y].transform.GetChild(0).gameObject.SetActive(true);
            }

            ++_Count;
            ++_Created;
        }
    }

    void DestroyNode(int x, int y)
    {
        if (_Nodes[x, y]==null)
        {
            //Debug.LogError("node doesnt exists");
        }
        else
        {            
            ParticlesDeath.SetVector3("Position", new Vector3(x*_NodeSpacing, 0.0f, y*_NodeSpacing));

            GameObject.Destroy(_Nodes[x, y]);
            _Nodes[x, y] = null;

            GameObject.Destroy(_NodesUI[x, y]);
            _NodesUI[x, y] = null;

            GameObject.Destroy(_NodesCube[x, y]);
            _NodesCube[x, y] = null;

            --_Count;
            ++_Destroyed;
        }
    }
}
