using UnityEngine;
using UnityEngine.AI;

public class MeshParticleSystem : MonoBehaviour
{
    private Mesh mesh;

    private Vector3[] vertices;
    private Vector2[] uv;
    private int[] triangles;

    private void Awake()
    {
        mesh = new Mesh();

        vertices = new Vector3[0];
        uv = new Vector2[0];
        triangles = new int[0];

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
