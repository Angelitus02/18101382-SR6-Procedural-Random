using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*  
    To remove the slow generating effect
    1 Remove: yield return new WaitForSeconds(0.01f);
    2 Change CreateShape to a normal function void CreateSurface();
    3 Change StartCoroutine(CreateSurface()); to CreateSurface();
    4 Optional: to make it more efficient, move UpdateSurface() from Update to the end of Start()
    5 Optional: Remove OnDrawGizmos function
*/

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Surface : MonoBehaviour
{

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    [Range(2, 256)]
    public int width = 20;
    [Range(2, 256)]
    public int height = 20;
    //[Range(1, 999)]
    public float offsetX = 100f;
    public float offsetZ = 100f;
    [Range(-10f, 10f)]
    public float altitude = 2f;
    [Range(1,10)]
    public int resolution;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();



    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<MeshFilter>().mesh = mesh;

        CreateSurface();
        UpdateSurface();
        offsetX *= Time.deltaTime * 2;
    }

    void CreateSurface()
    {

        //+1 vertices bc each square has two vertices, in each side, so at the end is +1 
        //or another explanation would be to count the origin vertex so we add 1.
        vertices = new Vector3[(width + 1) * (height + 1)];

        //attemp at getting resolution working
        float density = 1 / resolution;
        float ytrack = 0;


        //loop through all vertices
        for (int h = 0, i = 0; h <= height; h++)
        {
            float xtrack = 0;
            for (int w = 0; w <= width; w++)
            {
                //depth(d) value to make indentations in the surface, this is the y axis
                float d = CalculatePerlin(xtrack, ytrack) * 2f;

                //giving some depth parameter
                d *= altitude;

                //initially is w,0,h but we add depth with perlin noise later w,d,h
                vertices[i] = new Vector3(xtrack, d, ytrack);
                xtrack += density;

                i++;
            }//for
            ytrack += density;
        }//for


        int vertex = 0;
        int tris = 0;
        //basic mesh to construct our surface
        triangles = new int[width * height * resolution * resolution * 6];


        //loop through our vertices to generate the mesh triangles
        for (int h = 0; h < height/*Resolution*/; h++)
        {

            for (int w = 0; w < width/*Resolution*/; w++)
            {
                //first triangle
                triangles[tris + 0] = vertex + 0;
                triangles[tris + 1] = vertex + width/*Resolution*/ + 1;
                triangles[tris + 2] = vertex + 1;
                //second triangle
                triangles[tris + 3] = vertex + 1;
                triangles[tris + 4] = vertex + width/*Resolution*/ + 1;
                triangles[tris + 5] = vertex + width/*Resolution*/ + 2;
                //square formed

                vertex++;
                tris += 6;

                //this stops the coroutine for 0.1f seconds so we see the squares being formed 1 by 1
                //yield return new WaitForSeconds(0.01f);
            }
            //this fixed the lighting issue
            vertex++;
        }




    }//createsurface()

    void UpdateSurface()
    {
        //need to clear the mesh so we dont overwrite it everytime
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        //rendering normals we need this for lightning so that it's perpendicular to the mesh
        mesh.RecalculateNormals();
    }

    private float CalculatePerlin(float i, float j)
    {
        //perlin noise takes 2 numbers and a pseudo random method to calculate random numbers that form a wave
        float number = Mathf.PerlinNoise(i * 0.3f + offsetX, j * 0.3f + offsetZ) * 2f;
        return number;
    }

    ////Gizmos class is unity for drawing
    //private void OnDrawGizmos()
    //{
    //    if (vertices == null)
    //    {
    //        return;
    //    }

    //    for (int i = 0; i < vertices.Length; i++)
    //    {
    //        //param: where, size
    //        Gizmos.DrawSphere(vertices[i], 0.1f);
    //    }

    //}//ondrawGizmos
}
