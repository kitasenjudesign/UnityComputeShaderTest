using UnityEngine;
using System.Collections;

namespace Utils {

    public class RandomPointOnMesh {

        public Vector3 v;
        public Vector2 uv;

        public static Vector3 Sample (Mesh mesh) {
            int[] triangles = mesh.triangles;
            int index = Mathf.FloorToInt(Random.value * (triangles.Length / 3));
            Vector3 v0 = mesh.vertices[triangles[index * 3 + 0]];
            Vector3 v1 = mesh.vertices[triangles[index * 3 + 1]];
            Vector3 v2 = mesh.vertices[triangles[index * 3 + 2]];
            return Vector3.Lerp(v0, Vector3.Lerp(v1, v2, Random.value), Random.value);
        }
        
        public static RandomPointOnMesh Sample2 (Mesh mesh) {
            int[] triangles = mesh.triangles;
            int index = Mathf.FloorToInt(Random.value * (triangles.Length / 3));
            
            Vector3 v0 = mesh.vertices[triangles[index * 3 + 0]];
            Vector3 v1 = mesh.vertices[triangles[index * 3 + 1]];
            Vector3 v2 = mesh.vertices[triangles[index * 3 + 2]];
            
            Vector2 uv0 = mesh.uv[triangles[index * 3 + 0]];
            Vector2 uv1 = mesh.uv[triangles[index * 3 + 1]];
            Vector2 uv2 = mesh.uv[triangles[index * 3 + 2]];
            
            float rand1 = Random.value;
            float rand2 = Random.value;

            RandomPointOnMesh m = new RandomPointOnMesh();
            m.v     = Vector3.Lerp(v0, Vector3.Lerp(v1, v2, rand1), rand2);
            m.uv    = Vector2.Lerp(uv0, Vector2.Lerp(uv1, uv2, rand1), rand2);

            return  m;
        }
        


    }
    
}