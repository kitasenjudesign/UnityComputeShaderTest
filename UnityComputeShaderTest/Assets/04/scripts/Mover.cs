using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class Mover : MonoBehaviour{

    public Vector3 _spd;
    private Vector3 _startPos;
    private Vector3 _velocity;
    void Start(){
        
        _startPos = transform.position;
        Reset();
    }

    private void Reset(){
        _velocity =new Vector3(
            0.15f*(Random.value - 0.5f),
            0.15f*(Random.value - 0.5f),
            0.15f*(Random.value - 0.5f)
        );

        transform.position = _startPos;
        Invoke("Reset",5f);
    }

    void Update(){

        transform.position += _velocity;
        _velocity = _velocity - 0.001f*_velocity;
        _velocity += new Vector3(0,-0.0003f,0);

    }

}