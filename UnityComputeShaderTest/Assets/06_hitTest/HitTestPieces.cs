using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class HitTestPieces : MonoBehaviour {

    [SerializeField] protected Mesh _mesh;
    [SerializeField] protected Material _mat;
    [SerializeField] protected HitTestComputer _posComputer;
    protected Matrix4x4[] _matrices;
    protected Vector4[] _colors;
    protected MaterialPropertyBlock _propertyBlock;
    protected int _count = 400;
    private HitTestPieceData[] _data;
    public const int MAX = 1023;

    void Start(){

        _propertyBlock = new MaterialPropertyBlock();
        _matrices = new Matrix4x4[MAX];
        _data = new HitTestPieceData[MAX];
        _colors = new Vector4[MAX];
        //_uvs = new Vector4[MAX];

        _count=0;
        for(int i=0;i<MAX;i++){

            _matrices[_count] = Matrix4x4.identity;
            _data[_count] = new HitTestPieceData();

            _data[_count].pos.x = 2f * ( Random.value-0.5f );
            _data[_count].pos.y = 2f * ( Random.value-0.5f );
            _data[_count].pos.z = 2f * ( Random.value-0.5f );
            _data[_count].rot = Quaternion.Euler(
                Random.value * 360f,
                Random.value * 360f,
                Random.value * 360f
            );

            _data[_count].scale.x = 0.5f;
            _data[_count].scale.y = 0.5f;
            _data[_count].scale.z = 0.5f;


            _colors[_count] = new Vector4(
                Random.value,
                Random.value,
                Random.value,
                1f
            );
            _count++;
            
        }

    }


    void Update(){

        for (int i = 0; i < _count; i++)
        {
            //_data[i].pos = _posComputer.Positions[i];
            _data[i].pos = _posComputer.PosDataList[i].position;

            _matrices[i].SetTRS( 
                _data[i].pos,
                _data[i].rot,
                _data[i].scale
            );
            _matrices[i] = transform.localToWorldMatrix * _matrices[i];

        }

        _propertyBlock.SetVectorArray("_Color", _colors);
        //_propertyBlock.SetVectorArray("_Uv", _uvs);

        Graphics.DrawMeshInstanced(
            _mesh, 
            0, 
            _mat, 
            _matrices, 
            _count, 
            _propertyBlock, 
            ShadowCastingMode.On, 
            false, 
            gameObject.layer
        );

    }

}