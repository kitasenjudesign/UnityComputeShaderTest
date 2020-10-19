using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelGetter : MonoBehaviour
{

    [SerializeField] private Texture2D _textTex;
    [SerializeField] public List<Vector2> Positions;
    [SerializeField,Range(1,10)] private int _reduction;
    // Start is called before the first frame update
    void Awake()
    {
        Positions = new List<Vector2>();
    
        if(_reduction<=0)_reduction=1;

        for(int i=0;i<_textTex.width;i+=_reduction){
            for(int j=0;j<_textTex.height;j+=_reduction){
                
                var col = _textTex.GetPixel(i,j);
                if(col.r>0.5f){
                    Positions.Add(
                        new Vector2((float)i/_textTex.width-0.5f,(float)j/_textTex.height-0.5f)
                    );
                }

            }
        }

    }

    public Vector2 GetRandomPos(){
        return Positions[Mathf.FloorToInt(Random.value*Positions.Count)];
    }


    // Update is called once per frame
    void Update()
    {

    }
}
