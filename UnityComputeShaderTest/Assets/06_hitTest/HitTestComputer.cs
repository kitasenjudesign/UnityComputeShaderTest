using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;

public class HitTestComputer : MonoBehaviour
{

    public struct HitTestData
    {
        public Vector3 position;
        public Vector3 velocity;
        public int index;
        public float time;
    }

    [SerializeField] private ComputeShader _computeShader;

    int ThreadBlockSize = 32;//64;//256;
    ComputeBuffer _result;
    [SerializeField] private Vector3[] _rawResult;//出力よう


    private int _num = 1024;
    private ComputeBuffer _dataBuffer;
    private HitTestData[] _hitTestDataList;

    
    public Vector3[] Positions
    {
        get
        {
            return _rawResult;
        }
    }

    public HitTestData[] PosDataList
    {
        get
        {
            return _hitTestDataList;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

        _hitTestDataList=new HitTestData[_num];

        //result
        _result = new ComputeBuffer(_num,Marshal.SizeOf(typeof(Vector3)));
        _rawResult = new Vector3[_num];

        //コンピュートバッファ用意
        _dataBuffer = new ComputeBuffer(_num, Marshal.SizeOf(typeof(HitTestData)));
        
        //----------初期値を設定。
        
        for (int i = 0; i < _num; ++i){
            _hitTestDataList[i] = new HitTestData();
            _hitTestDataList[i].time = Random.value * 100f;
            _hitTestDataList[i].position = new Vector3(
                5f*(Random.value-0.5f),
                5f*(Random.value-0.5f),
                5f*(Random.value-0.5f)
            );
            _hitTestDataList[i].velocity = new Vector3(
                0.01f * ( Random.value - 0.5f ),
                0.01f * ( Random.value - 0.5f ),
                0.01f * ( Random.value - 0.5f )
            );
        }

        //----------初期値をコンピュートバッファに入れる
        _dataBuffer.SetData(_hitTestDataList);
        
    }

    // Update is called once per frame
    void Update()
    {
          
        int kernelId = _computeShader.FindKernel("MainCS");
        
        _computeShader.SetFloat("_Near", 100f );
        _computeShader.SetFloat("_VelocityRatio",1f);

        //_computeShader.SetVectorArray("_Positions", _positions);
        _computeShader.SetBuffer(kernelId, "_Result", _result);
        _computeShader.SetBuffer(kernelId, "_HitTestDataBuffer", _dataBuffer);
        _computeShader.Dispatch(kernelId, (Mathf.CeilToInt(_num / ThreadBlockSize) + 1), 1, 1);

        //Vector3[] rawData = new Vector3[100];
        //_result.GetData( _rawResult );
        _dataBuffer.GetData( _hitTestDataList );


    }



}
