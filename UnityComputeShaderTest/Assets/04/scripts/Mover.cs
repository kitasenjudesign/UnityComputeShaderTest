using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class Mover : MonoBehaviour{

    public Vector3 _spd;
    private Vector3 _startPos;
    void Start(){
        _startPos = transform.position;
        Reset();
    }

    private void Reset(){
        _spd = new Vector3(
            Random.value - 0.5f,
            Random.value - 0.5f,
            Random.value - 0.5f
        );
        transform.position = _startPos;
        Invoke("Reset",5f);
    }

    void Update(){

        transform.Translate(0.05f*_spd);

    }

}