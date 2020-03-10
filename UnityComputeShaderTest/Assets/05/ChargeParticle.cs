//using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace Kitasenju {
public class ChargeParticle : MonoBehaviour{

    struct CubeData
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector4 color;
        public float timeRatio;
        public Vector2 uv;
        public float time;
    }
    
    [SerializeField] int _num = 10000;
    //[SerializeField] Texture2D _src;
    [SerializeField,Range(0.001f,1f)] float _size;//_Size ("_Size", Range(0.04,0.1)) = 0.04

    int ThreadBlockSize = 64;

    ComputeBuffer _cubeDataBuffer;
    ComputeBuffer _argsBuffer;
    private uint[] _args = new uint[5] { 0, 0, 0, 0, 0 };
    [SerializeField] private Mesh _mesh;
    [SerializeField] ComputeShader _computeShader;
    [SerializeField] private Material _material;
    [SerializeField,Range(0,1f)] private float _Distance=0.5f;
    private float _time = 0;


    private CubeData[] _dataArr;
    private int _frame=0;
    [SerializeField] private float _duration;
    private MaterialPropertyBlock _block;
    private Vector3 _pastPos;
    private bool _isInit = false;
    private bool _isEmit = false;
    private float _strength=0;
    private float _speed = 0;


    void Start(){
        Show(1f,3f);
    }


    private void Init(){

        if(_isInit) return;
        _isInit=true;

        _block = new MaterialPropertyBlock();
        _pastPos = Vector3.zero;

        //コンピュートバッファ用意
        _cubeDataBuffer = new ComputeBuffer(_num, Marshal.SizeOf(typeof(CubeData)));
        //おまじない（よくわからない）
        _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        //----------初期値を設定。
        var dataArr = new CubeData[_num];
        for (int i = 0; i < _num; i++){

                dataArr[i] = new CubeData();
                dataArr[i].position = new Vector3(
                    99999f,
                    99999f,
                    99999f
                );//Vector3.zero;

                dataArr[i].timeRatio=Random.value;
                dataArr[i].velocity = new Vector3(
                    0.01f * ( Random.value - 0.5f ),
                    0.01f * ( Random.value - 0.5f ),
                    0.01f * ( Random.value - 0.5f )
                );
                dataArr[i].color = new Vector4(
                    Random.value,
                    Random.value,
                    Random.value,
                    Random.value
                );

                dataArr[i].time = Random.value*_duration;
                //dataArr[i].time = Random.value * _duration;
                //Debug.Log(dataArr[i].time);
        
                dataArr[i].uv = new Vector2(0,0);
               
        }

        //----------初期値をコンピュートバッファに入れる
        _dataArr=dataArr;
        _cubeDataBuffer.SetData(dataArr);
        
        _setShaderParams();
    }



    void Update(){
        
        if(enabled && _isInit){
            _setShaderParams();
            _draw();
        }
    }

   
    private void _setShaderParams(){

        // ComputeShader

        int kernelId = _computeShader.FindKernel("MainCS");
        _computeShader.SetFloat("_Duration", _duration);
        _computeShader.SetFloat("_DeltaTime",Time.deltaTime);
        _computeShader.SetFloat("_Emitting", _isEmit ? 1f : 0 );
        _computeShader.SetFloat("_Strength",_strength);
        _computeShader.SetVector("_Position", transform.position);
        //_computeShader.SetVector("_Velocity", transform.position-_pastPos);
        _computeShader.SetFloat("_Distance",_Distance);
        
        //_pastPos=transform.position;
        
        _computeShader.SetBuffer(kernelId, "_CubeDataBuffer", _cubeDataBuffer);
        _computeShader.Dispatch(kernelId, (Mathf.CeilToInt(_num / ThreadBlockSize) + 1), 1, 1);

        //おまじないパラメータ
        _args[0] = (uint)_mesh.GetIndexCount(0);
        _args[1] = (uint)_num;
        _args[2] = (uint)_mesh.GetIndexStart(0);
        _args[3] = (uint)_mesh.GetBaseVertex(0);
        _argsBuffer.SetData(_args);

        // GPU Instaicing
        _block.SetFloat("_Duration",_duration);
        _block.SetBuffer("_CubeDataBuffer", _cubeDataBuffer);//データを渡す
        _block.SetFloat("_Size",_size);
        
    }

    public void Show(float strength, float duration){

        
        _strength = strength;
        _duration = duration;
        _isEmit = true;
        enabled=true;
        Init();
        
    }
    public void Hide(){
        Debug.Log("Hide");
        _isEmit=false;
    }

    public void Reset(){

        int kernelId = _computeShader.FindKernel("Reset");
        _computeShader.SetBuffer(kernelId, "_CubeDataBuffer", _cubeDataBuffer);
        _computeShader.Dispatch(kernelId, (Mathf.CeilToInt(_num / ThreadBlockSize) + 1), 1, 1);

    }

    private void _draw(){

        Graphics.DrawMeshInstancedIndirect(
            _mesh,
            0, 
            _material, 
            new Bounds(Vector3.zero, new Vector3(32f, 32f, 32f)), 
            _argsBuffer,//Indirectには必要なんか
            0,
            _block,
            ShadowCastingMode.Off,
            false
        );        

    }

    //
    void OnDestroy()
    {
        // バッファを破棄
        ReleaseBuffer();
    }

    private void ReleaseBuffer()
    {
        if (_cubeDataBuffer != null)
        {
            _cubeDataBuffer.Release();
            _cubeDataBuffer = null;
        }
    }


}

}