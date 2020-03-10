using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kitasenju{
public class ChargeTest : MonoBehaviour
{
    [SerializeField] private ChargeParticle particle;
    [SerializeField,Range(0,1f)] private float _Strength;
    [SerializeField] private float _Duration;
    //[SerializeField,Range(0,1f)] private float _Speed;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        #if UNITY_EDITOR

        _Duration = 2f - 1f * _Strength;
        particle.Show( _Strength,_Duration );

        #endif

    }
}
}
