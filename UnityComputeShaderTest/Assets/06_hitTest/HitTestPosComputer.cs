using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;

public class HitTestPosComputer : MonoBehaviour
{

    public struct HitTestData
    {
        public Vector3 position;
        public Vector3 velocity;
        public int index;
        public float time;
    }

    [SerializeField] private ComputeShader _computeShader;
    [SerializeField,Range(0.1f,2f)] private float _radius = 2f;
    [SerializeField,Range(0.01f,0.1f)] private float _strength = 0.01f;

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
          
        //位置をリセットする
        if( Input.GetKeyDown(KeyCode.Space) ){
            
            Debug.Log("keydown");
            for(int i=0;i<_hitTestDataList.Length;i++){
                _hitTestDataList[i].position.x = 3f*(Random.value-0.5f);
                _hitTestDataList[i].position.y = 3f*(Random.value-0.5f);
                _hitTestDataList[i].position.z = 3f*(Random.value-0.5f);

            }
            //セットする
            _dataBuffer.SetData(_hitTestDataList);

        }

        int kernelId = _computeShader.FindKernel("MainCS");
        
        _computeShader.SetFloat("_Radius", _radius );
        _computeShader.SetFloat("_Strength",_strength);

        _computeShader.SetBuffer(kernelId, "_Result", _result);
        _computeShader.SetBuffer(kernelId, "_HitTestDataBuffer", _dataBuffer);
        _computeShader.Dispatch(kernelId, (Mathf.CeilToInt(_num / ThreadBlockSize) + 1), 1, 1);

        _dataBuffer.GetData( _hitTestDataList );


    }



}
