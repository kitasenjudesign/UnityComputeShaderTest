using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshParticle : MonoBehaviour{

    struct CubeData
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector4 color;
        public Vector3 basePos;
        public Vector2 uv;
        public float time;
    }

    [SerializeField] private Mesh _posMesh;
    [SerializeField] int _num = 10000;
    [SerializeField,Range(0.001f,1f)] float _size;//_Size ("_Size", Range(0.04,0.1)) = 0.04

    int ThreadBlockSize = 256;

    ComputeBuffer _cubeDataBuffer;
    ComputeBuffer _argsBuffer;
    private uint[] _args = new uint[5] { 0, 0, 0, 0, 0 };
    [SerializeField] private Mesh _mesh;
    [SerializeField] ComputeShader _computeShader;
    [SerializeField] private Material _material;
    private MeshRenderer _renderer;
    private float _time = 0;
    private MaterialPropertyBlock _property;
    private Vector4[] _positions;

    void Start(){

        _num = Mathf.FloorToInt( _posMesh.vertexCount );
        _property = new MaterialPropertyBlock();
        _renderer = GetComponent<MeshRenderer>();

        //コンピュートバッファ用意
        _cubeDataBuffer = new ComputeBuffer(_num, Marshal.SizeOf(typeof(CubeData)));
        //おまじない（よくわからない）
        _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        //----------初期値を設定。
        var dataArr = new CubeData[_num];
        var vertices = _posMesh.vertices;
        var uv = _posMesh.uv;
        for (int i = 0; i < _num; ++i){
            dataArr[i] = new CubeData();
            dataArr[i].position = vertices[i];
            dataArr[i].basePos = vertices[i];

            dataArr[i].velocity = new Vector3(
                0.01f * ( Random.value - 0.5f ),
                0.01f * ( Random.value - 0.5f ),
                0.01f * ( Random.value - 0.5f )
            );
            dataArr[i].time = Random.value;
            float delay = Mathf.Abs( vertices[i].x * 10f );
            //Debug.Log(delay);
            dataArr[i].color = new Vector4(
                delay,
                Random.value,
                Random.value,
                1f
            );

            dataArr[i].uv = uv[i];
        }

        //----------初期値をコンピュートバッファに入れる
        _cubeDataBuffer.SetData(dataArr);
        


        GetComponent<MeshRenderer>().enabled=false;
    }


    void Update(){
        
        //computeShaderに値を渡す

        // ComputeShader
        _time += Time.deltaTime;
        if(_time>8f){
            _time = 0;//じかん
            GetComponent<MeshRenderer>().enabled=false;
        }else if(_time>5f){
            GetComponent<MeshRenderer>().enabled=true;
        }

        int kernelId = _computeShader.FindKernel("MainCS");
        _computeShader.SetFloat("_Time", _time);
        //_computeShader.SetVectorArray("_Positions", _positions);
        _computeShader.SetBuffer(kernelId, "_CubeDataBuffer", _cubeDataBuffer);
        _computeShader.Dispatch(kernelId, (Mathf.CeilToInt(_num / ThreadBlockSize) + 1), 1, 1);

        //おまじないパラメータ
        _args[0] = (uint)_mesh.GetIndexCount(0);
        _args[1] = (uint)_num;
        _args[2] = (uint)_mesh.GetIndexStart(0);
        _args[3] = (uint)_mesh.GetBaseVertex(0);
        _argsBuffer.SetData(_args);

        // GPU Instaicing

        _material.SetBuffer("_CubeDataBuffer", _cubeDataBuffer);//データを渡す
        //_material.SetVector("_DokabenMeshScale", this._DokabenMeshScale);
        _material.SetMatrix("_modelMatrix", transform.localToWorldMatrix );
        _material.SetFloat("_Size",_size);

        //_renderer.SetPropertyBlock(_property);

        //_material.SetVector("_Num",new Vector4(_numX,_numY,0,0));
        
        Graphics.DrawMeshInstancedIndirect(
            _mesh,
            0, 
            _material, 
            new Bounds(Vector3.zero, new Vector3(32f, 32f, 32f)), 
            _argsBuffer,//Indirectには必要なんか
            0,
            null,
            ShadowCastingMode.On,
            false
        );
        
        //gameObject.transform.Rotate(new Vector3(0.01f,0.005f,0));

    }

}