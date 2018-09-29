using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class Cubes : MonoBehaviour{

    struct CubeData
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector4 color;
        public Vector3 basePos;
        public Vector2 uv;

    }

    [SerializeField] int _num = 10000;
    [SerializeField] int _numX = 256; 
    [SerializeField] int _numY = 256;
    //[SerializeField] Texture2D _src;
    [SerializeField] float width;
    [SerializeField,Range(0.001f,1f)] float _size;//_Size ("_Size", Range(0.04,0.1)) = 0.04

    int ThreadBlockSize = 256;

    ComputeBuffer _cubeDataBuffer;
    ComputeBuffer _argsBuffer;
    private uint[] _args = new uint[5] { 0, 0, 0, 0, 0 };
    [SerializeField] private Mesh _mesh;
    [SerializeField] ComputeShader _computeShader;
    [SerializeField] private Material _material;

    private float _time = 0;

    void Start(){

        _num = _numX * _numY;

        _cubeDataBuffer = new ComputeBuffer(_num, Marshal.SizeOf(typeof(CubeData)));
        _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        var dataArr = new CubeData[_num];

        //float width = 10f;
        float height = width * (float)_numY / _numX;

        int idx = 0;
        for (int i = 0; i < _numX; ++i){
            for (int j = 0; j < _numY; ++j){

                float rx = (float)i/(_numX-1);
                float ry = (float)j/(_numY-1);
                dataArr[idx] = new CubeData();
                dataArr[idx].position = new Vector3(
                    (rx-0.5f) * width,
                    (ry-0.5f) * height,
                    0
                );
                dataArr[idx].basePos = new Vector3(
                    (rx-0.5f) * width,
                    (ry-0.5f) * height,
                    0
                );

                dataArr[idx].velocity = new Vector3(
                    0.01f * ( Random.value - 0.5f ),
                    0.01f * ( Random.value - 0.5f ),
                    0.01f * ( Random.value - 0.5f )
                );

                //var col = _src.GetPixel(
                 //   Mathf.FloorToInt(rx*(_src.width-1)),
                //    Mathf.FloorToInt(ry*(_src.height-1))
                //);

                dataArr[idx].color = new Vector4(
                    0,
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
        _cubeDataBuffer.SetData(dataArr);
        
    }


    void Update(){
        
        //computeShaderに値を渡す


            // ComputeShader
            _time += Time.deltaTime;
            if(_time>3f){
                _time = 0;
                //_computeShader.SetVector
                //_cubeDataBuffer.GetData()
            }

            int kernelId = _computeShader.FindKernel("MainCS");
            _computeShader.SetFloat("_Time", _time);
            _computeShader.SetBuffer(kernelId, "_CubeDataBuffer", _cubeDataBuffer);
            _computeShader.Dispatch(kernelId, (Mathf.CeilToInt(_num / ThreadBlockSize) + 1), 1, 1);

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