using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class Star : MonoBehaviour{

    struct CubeData
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector4 color;
        public Vector3 basePos;
        public Vector2 uv;
        public float time;
    }
    [SerializeField] private Mover _prefab;
    [SerializeField] private Mover[] _targets;
    [SerializeField] private Mesh _posMesh;
    [SerializeField] int _num = 10000;
    [SerializeField] int _numX = 256; 
    [SerializeField] int _numY = 256;
    //[SerializeField] Texture2D _src;
    [SerializeField] float width;
    [SerializeField,Range(0.001f,1f)] float _size;//_Size ("_Size", Range(0.04,0.1)) = 0.04

    int ThreadBlockSize = 64;

    ComputeBuffer _cubeDataBuffer;
    ComputeBuffer _argsBuffer;
    private uint[] _args = new uint[5] { 0, 0, 0, 0, 0 };
    [SerializeField] private Mesh _mesh;
    [SerializeField] ComputeShader _computeShader;
    [SerializeField] private Material _material;

    private float _time = 0;

    private Vector4[] _positions;
    private Vector4[] _velosities;

    private CubeData[] _dataArr;
    void Start(){

        _targets = new Mover[100];
        for(int i=0;i<100;i++){
            _targets[i] = Instantiate(_prefab,transform,false);
            _targets[i].transform.position = Vector3.zero;
        }
        
        _prefab.gameObject.SetActive(false);

        _num = _numX * _numY;
        float height = width * (float)_numY / _numX;


        //コンピュートバッファ用意
        _cubeDataBuffer = new ComputeBuffer(_num, Marshal.SizeOf(typeof(CubeData)));
        //おまじない（よくわからない）
        _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        //----------初期値を設定。
        var dataArr = new CubeData[_num];
        int idx = 0;
        for (int i = 0; i < _numX; ++i){
            for (int j = 0; j < _numY; ++j){

                float rx = (float)i/(_numX-1);
                float ry = (float)j/(_numY-1);
                dataArr[idx] = new CubeData();
                dataArr[idx].position = _targets[
                    idx % _targets.Length
                ].transform.position;

                dataArr[idx].basePos = _targets[
                    idx % _targets.Length
                ].transform.position;

                dataArr[idx].velocity = new Vector3(
                    0.01f * ( Random.value - 0.5f ),
                    0.01f * ( Random.value - 0.5f ),
                    0.01f * ( Random.value - 0.5f )
                );

                dataArr[idx].time = Random.value*5f;

                dataArr[idx].color = new Vector4(
                    Random.value,
                    0,
                    0,
                    1f
                );

                dataArr[idx].uv = new Vector2(
                    (float) i / (_numX),
                    (float) j / (_numY)
                );
                idx++;
            }
        }

        //----------初期値をコンピュートバッファに入れる
        _dataArr=dataArr;
        _cubeDataBuffer.SetData(dataArr);
        


        
        
        //
        _positions = new Vector4[100];
        _velosities = new Vector4[100];
    }


    void Update(){
        
        //computeShaderに値を渡す

        //base position wo kurikaesu
        for(var i=0;i<_positions.Length;i++){
            _positions[i] = _targets[i%_targets.Length].transform.position;
            _velosities[i] = _targets[i%_targets.Length]._spd;
        }

        // ComputeShader
        _time += Time.deltaTime;
        if(_time>8f){
            _time = 0;//じかん
        }

        int kernelId = _computeShader.FindKernel("MainCS");
        _computeShader.SetFloat("_Time", _time);
        _computeShader.SetVectorArray("_Positions", _positions);
        _computeShader.SetVectorArray("_Velocities", _velosities);
        
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
        _material.SetVector("_Num",new Vector4(_numX,_numY,0,0));
        
        Graphics.DrawMeshInstancedIndirect(
            _mesh,
            0, 
            _material, 
            new Bounds(Vector3.zero, new Vector3(32f, 32f, 32f)), 
            _argsBuffer,//Indirectには必要なんか
            0,
            null,
            ShadowCastingMode.Off,
            false
        );
        
        //gameObject.transform.Rotate(new Vector3(0.01f,0.005f,0));

    }

}