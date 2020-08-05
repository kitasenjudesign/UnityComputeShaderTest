using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class DepthPosition : MonoBehaviour{

    struct CubeData
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector4 color;
        public Vector3 basePos;
        public Vector2 uv;
        public float time;
    }

    //[SerializeField] private Mesh[] _meshes;
    [SerializeField]private int _num = 10000;
    int ThreadBlockSize = 64;//256;
    int _index=0;
    ComputeBuffer _cubeDataBuffer;
    ComputeBuffer _result;
    ComputeBuffer _argsBuffer;
    private uint[] _args = new uint[5] { 0, 0, 0, 0, 0 };
    [SerializeField] ComputeShader _computeShader;
    //[SerializeField] private Material _material;
    private MeshRenderer _renderer;
    private float _time = 0;
    private MaterialPropertyBlock _property;
    //private Vector4[] _positions;

    [SerializeField,Space(10)] private Texture _stencilTex;
    [SerializeField] private Texture _colorTex;
    [SerializeField] private Texture _depthTex;
    [SerializeField,Space(10)] private Camera _camera;
    [SerializeField,Range(0,1),Space(10)] private float _velocityRatio = 1;
    [SerializeField] private float _duration = 4f;
    [SerializeField] private Vector3[] _rawResult;
    
    [Space(20)]

    [SerializeField] private GameObject _prefab;
    [SerializeField] private int _numPrefab=200;
    private GameObject[] _dummyPrefabs;
    private bool _isInit = false;
    



    void Start(){

        
        _dummyPrefabs = new GameObject[_numPrefab];
        for(int i=0;i<_numPrefab;i++){
            _dummyPrefabs[i] = Instantiate(_prefab,transform,false);
            _dummyPrefabs[i].transform.localScale=Vector3.one*(Random.value*0.05f+0.02f);
            _dummyPrefabs[i].transform.rotation=Quaternion.Euler(
                360f * Random.value,
                360f * Random.value,
                360f * Random.value
            );
            _dummyPrefabs[i].gameObject.SetActive(true);

        }

        //Params.Init();
        Init();

    }

    public void Init(){

        /////////////////////////
        if(_isInit)return;
        _isInit = true;

        if(_camera==null){
            _camera = Camera.main;
        }

        //_num = 10000;//Mathf.FloorToInt( _posMesh.vertexCount );
        _property = new MaterialPropertyBlock();
        _renderer = GetComponent<MeshRenderer>();

        _result = new ComputeBuffer(100,Marshal.SizeOf(typeof(Vector3)));
        _rawResult = new Vector3[100];

        //コンピュートバッファ用意
        _cubeDataBuffer = new ComputeBuffer(_num, Marshal.SizeOf(typeof(CubeData)));
        
        //----------初期値を設定。
        var dataArr = new CubeData[_num];
        
        for (int i = 0; i < _num; ++i){
            dataArr[i] = new CubeData();
            dataArr[i].position = new Vector3(
                0,100f,0
            );
            dataArr[i].basePos = new Vector3(
                Random.value,
                Random.value,
                Random.value
            );

            dataArr[i].velocity = new Vector3(
                0.01f * ( Random.value - 0.5f ),
                0.01f * ( Random.value - 0.5f ),
                0.01f * ( Random.value - 0.5f )
            );
            dataArr[i].time = Random.value*4f;
            dataArr[i].color = new Vector4(
                Random.value,
                Random.value,
                Random.value,
                1f
            );

            dataArr[i].uv = new Vector2(
                Random.value,
                Random.value
            );
        }

        //----------初期値をコンピュートバッファに入れる
        _cubeDataBuffer.SetData(dataArr);
    
    }

    public void SetTextures(Texture depthTex, Texture stencilTex){
        
        _depthTex = depthTex;
        _stencilTex = stencilTex;
        if(_stencilTex){
            //Params.SetStencilAspect(_stencilTex.width,_stencilTex.height);
            //Params.SetRotation();
        }
        
    }

    void Update(){
        
        //computeShaderに値を渡す

        // ComputeShader
        _time += Time.deltaTime;
        
        int kernelId = _computeShader.FindKernel("MainCS");
        //_computeShader.SetFloat("_Time", _time);

        // camera
        var cam = _camera;
        
        var camToWorld = cam.cameraToWorldMatrix;
		var projection = GL.GetGPUProjectionMatrix (cam.projectionMatrix, false);
		var inverseP = projection.inverse;

        _computeShader.SetFloat("_Near", cam.nearClipPlane );
        _computeShader.SetFloat("_Far", cam.farClipPlane );
        _computeShader.SetMatrix("_InvProjMat", inverseP);
		_computeShader.SetMatrix("_InvViewMat", camToWorld);

        _computeShader.SetFloat("_Duration",_duration);
        _computeShader.SetFloat("_VelocityRatio",_velocityRatio);

        if(_stencilTex!=null){
            _computeShader.SetTexture(0,"_StencilTex", _stencilTex);
            _computeShader.SetVector("_StencilTexSize", new Vector4(_stencilTex.width,_stencilTex.height));
        }
        if(_colorTex!=null){        
            _computeShader.SetTexture(0,"_ColorTex", _colorTex);
            _computeShader.SetVector("_ColorTexSize", new Vector4(_colorTex.width,_colorTex.height));
        }
        if(_depthTex!=null){
            _computeShader.SetTexture(0,"_DepthTex", _depthTex);
            _computeShader.SetVector("_DepthTexSize", new Vector4(_depthTex.width,_depthTex.height));
        }

        //_computeShader.SetVectorArray("_Positions", _positions);
        _computeShader.SetBuffer(kernelId, "_Result", _result);
        _computeShader.SetBuffer(kernelId, "_CubeDataBuffer", _cubeDataBuffer);
        _computeShader.Dispatch(kernelId, (Mathf.CeilToInt(_num / ThreadBlockSize) + 1), 1, 1);

        //Vector3[] rawData = new Vector3[100];
        _result.GetData(_rawResult);
        
        for(int i=0;i<_dummyPrefabs.Length;i++){
            
            var pos = _rawResult[i%_rawResult.Length];
            //pos.y = -pos.y;
            if(Random.value<0.03f){
                _dummyPrefabs[i%_dummyPrefabs.Length].transform.position 
                = pos;//
            }

        }
        //_index++;
    }


    public Vector3[] GetPosition(){
        return _rawResult;
    }

}