using UnityEngine;
using System.Collections;

namespace SpatialMessage.Area3Ema
{
public class OrbitCamera : MonoBehaviour {
    
    public Transform target;
	Vector3 f0Dir= Vector3.zero;
	Vector3 upVal= Vector3.zero;
	[SerializeField] float zoomDistance= 5;
	float theta= 0.0F;
	float fai= 0.0F;
	float loc_x= 0.0F;
	float loc_y= 0.0F;
	float panWeight= 0.5F;
	
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private float minDistance = 0;

    void Start(){



    }

	void Update() {

        zoomDistance = Mathf.Clamp(zoomDistance - Input.GetAxis("Mouse ScrollWheel")*5, minDistance, maxDistance);


		if( Input.GetMouseButton(0) && !Input.GetKey("left alt" ) ) {
			f0Dir=  new Vector3(Input.GetAxis("Mouse X")*5.0F, -Input.GetAxis("Mouse Y")*5.0F, 0);
			if(	Input.GetKey("left alt")) {
				loc_x= -Input.GetAxis("Mouse X")*1;
				loc_y= -Input.GetAxis("Mouse Y")*1;
				f0Dir= Vector3.zero;
			}
		} else if( Input.GetMouseButton(1)&& Input.GetKey("left alt") ) {
			zoomDistance= zoomDistance+(-Input.GetAxis("Mouse X")+Input.GetAxis("Mouse Y"))*0.1F;
		} else if( Input.GetMouseButton(2) ) {
			loc_x= -Input.GetAxis("Mouse X")*panWeight;
			loc_y= -Input.GetAxis("Mouse Y")*panWeight;
		} else {
			f0Dir= Vector3.zero;
			loc_x= 0.0F;
			loc_y= 0.0F;
		}
		theta+= Mathf.Deg2Rad*f0Dir.x*1;
		fai+=   -Mathf.Deg2Rad*f0Dir.y*1;
		
		upVal.z= zoomDistance*Mathf.Cos(theta)*Mathf.Sin(fai+Mathf.PI/2);
		upVal.x= zoomDistance*Mathf.Sin(theta)*Mathf.Sin(fai+Mathf.PI/2);
		upVal.y= zoomDistance*Mathf.Cos(fai+Mathf.PI/2);

		transform.position= upVal;
		target.transform.Translate( Camera.main.transform.up*loc_y+Camera.main.transform.right*(loc_x), Space.World);
		transform.position+= target.position;
		Camera.main.transform.LookAt(target.position);

	}
}

}