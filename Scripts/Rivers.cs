using UnityEngine;

public class Rivers
{
    private static int resolution;

    // Start is called before the first frame update
    public static TerrainData PlaceRivers(TerrainData terrainData, int numberOfRivers, Material material)
    {
        // Choose random points on the map to start a river
        // Move step by step in the direction of TerrainData.GetSteepness and that height is lower than current point
        // Lower the terrain to create a river based on that path
        // Move for the river length
        // Stop if length reached or the land flattens
        // Repeat for the number of rivers wanted
        
        resolution = terrainData.heightmapResolution;

        //using a copied variable and returns it after being modified
        float[,] copiedHeights = terrainData.GetHeights(0, 0, resolution, resolution);

        // an array of ints which stores the total length of each river
        int[] riverLengths = new int[numberOfRivers];
        // a 3d array, each is a river with a 2d map of the river path
        int[,,] riverPaths = new int[numberOfRivers, resolution, resolution];

        float normalise = resolution - 1;
        // repeat for the number of rivers wanted
        for (int i = 0; i < numberOfRivers; i++)
        {
            // pick a random point on the map
            int pointX = Random.Range(0, resolution - 1);
            int pointY = Random.Range(0, resolution - 1);

            bool river = true;
            // randomly determine how long the river will be
            int j = 0;
            int maxLength = resolution;

            while (river)
            {
                // select first valid neighbour to be the baseline that will compare against to find the steepest
                float steepest = 0;

                // where the river will flow next
                int nextX = 0;
                int nextY = 0;

                // find the steepeness and compare all the neighbours
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    for (int yOffset = -1; yOffset <= 1; yOffset++)
                    {
                        // skip the square were checking the neighbours against
                        if (xOffset == 0 && yOffset == 0)
                        {
                            continue;
                        }

                        // finding the index of the neighbour we will be comparing
                        int neighbourX = (pointX + xOffset);
                        int neighbourY = (pointY + yOffset);

                        // ensuring we don't go out of bounds
                        if (neighbourX < 0 || neighbourY < 0 || neighbourX > (resolution - 1) || neighbourY > (resolution - 1))
                        {
                            continue;
                        }

                        // get the steepeness of the current selected neighbour
                        float steep = terrainData.GetSteepness(neighbourX / normalise, neighbourY/ normalise);

                        // compare the steepness of the current neighbour to the recorded current steepest
                        // compare the height of the origin point to the selected neighbour, to ensure
                        // it's lower and the river doesn't move up a terrain
                        if (steep > steepest && copiedHeights[pointX, pointY] > copiedHeights[neighbourX, neighbourY])
                        {
                            // sets this neighbour as the steepest and the next point the river will navigate to
                            steepest = steep;
                            nextX = neighbourX;
                            nextY = neighbourY;
                        }
                    } //for
                } //for

                // if land is flat, the river stops
                if (steepest < 2f)
                {
                    break;
                }

                // the next point becomes the current point
                pointX = nextX;
                pointY = nextY;

                // increase our length tracker
                j++;

                // record the path using the legnth tracker, we can use this later when generating a mesh
                // to know the flow of the river as it will go 1,2,3...44,45 etc
                riverPaths[i, pointX, pointY] = j;

                if (j > maxLength)
                {
                    // if the length is reached break out
                    break;
                }

            }//while
            //record the length of the river so we later know how many vertices are needed to generate a mesh
            riverLengths[i] = j;
        }

        terrainData.SetHeights(0, 0, riverDip(terrainData, riverPaths, copiedHeights));
        CreateMeshes(terrainData, riverLengths, riverPaths, material);
        return terrainData;
    }

    private static float[,] riverDip(TerrainData terrainData, int[,,] riverPaths, float[,] copiedHeights)
    {
        // of the 3d array, for each river, go through the 2D array of the map and check that
        // there is a river at that point
        // if the value is more than 0, means the river goes through the point and the height
        // is reduced at that point
        for (int i = 0; i < riverPaths.GetLength(0); i++)
        {
            for (int x = 0; x < copiedHeights.GetLength(0); x++)
            {
                for (int y = 0; y < copiedHeights.GetLength(1); y++)
                {
                    if (riverPaths[i, x, y] > 0)
                    {
                        copiedHeights[x, y] -= 0.05f;
                    }
                }
            }
        }

        return copiedHeights;
    }

    private static void CreateMeshes(TerrainData terrainData, int[] riverLengths, int[,,] riverPaths, Material material)
    {
        // used to make raycasts to the sides of the river for positions of the vertices
        RaycastHit hit;

        // will create meshes for every single river, which will be the length of riverLengths as it is an array
        // which holds how long each river is
        for (int i = 0; i < riverLengths.Length; i++){
            // the number of vertices needed for the river will be the length of the river plus 1, as the end also has to have
            // vertices, times two since the mesh will be a single square-like shape between each point
            Vector3[] vertices = new Vector3[2 * (riverLengths[i] + 1)];
            // each polygon needs 3 triangle positions, since a square is made from two polygons, means we need
            // 6 triangle positions to create one section
            int[] triangles = new int[6 * (riverLengths[i])];

//Debug.Log(vertices.Length);
//Debug.Log(triangles.Length);
//break;

            // need to track the flow of the river, riverPaths contains the path starting from 1 ascending
            int currentPosition = 1;
            // moving direction is a multiplier, as when we reach the last point of the river
            int movingDirection = 1;

            for (int j = 0; j < riverLengths[i] + 1; j++)
            {
                int currentX = 0;
                int currentY = 0;

                int nextX = 0;
                int nextY = 0;
                
                //find start of river, which is 1
                for (int x = 0; x < riverPaths.GetLength(1); x++)
                {
                    for (int y = 0; y < riverPaths.GetLength(2); y++)
                    {
                        if (riverPaths[i, x, y] == currentPosition)
                        {
                            currentX = x;
                            currentY = y;
                        }
                    } //for
                } //for

                // reached the end of the river, there is no next point
                // to get the direction instead, go to previous point
                if (currentPosition == riverLengths[i])
                {
                    movingDirection = -1;
                }
                    for (int x = 0; x < riverPaths.GetLength(1); x++)
                    {
                        for (int y = 0; y < riverPaths.GetLength(2); y++)
                        {
                            if (riverPaths[i, x, y] == currentPosition + movingDirection)
                            {
                                nextX = x;
                                nextY = y;
                            }
                        } //for
                    } //for

                Vector3 origin = new Vector3(terrainData.size.x * (currentX / resolution), terrainData.GetHeight(currentX, currentY), terrainData.size.z * (currentY / resolution));
                Vector3 destination = new Vector3(terrainData.size.x * (nextX / resolution), terrainData.GetHeight(nextX, nextY), terrainData.size.z * (nextY / resolution));
                Vector3 direction = (destination - origin).normalized;

                Vector3 left = Vector3.Cross(direction, Vector3.up).normalized;
                Vector3 right = -left;


                Physics.Raycast((origin + new Vector3(0,3,0)), left, out hit, 5);
                vertices[j] = hit.point;
                
                j++;
                Physics.Raycast((origin + new Vector3(0,3,0)), right, out hit, 5);
                vertices[j] = hit.point;

                currentPosition++;

            } //for

            //int vertex = 0;
            int triangle = 0;

            for (int vertex = 0; triangle < triangles.Length - 1; vertex+=2)
            {
                //first triangle
                triangles[triangle + 0] = vertex + 0;
                triangles[triangle + 1] = vertex + 2;
                triangles[triangle + 2] = vertex + 3;
                //second triangle
                triangles[triangle + 3] = vertex + 0;
                triangles[triangle + 4] = vertex + 3;
                triangles[triangle + 5] = vertex + 1;
                //square formed

                //increment triangles as done 6 points
                triangle += 6;

            } //for

// Debug.Log(triangles.Length);
// Debug.Log(triangle);
// break;

            Mesh mesh = new Mesh();

            mesh.vertices = vertices;
            mesh.triangles = triangles;

            GameObject gameObject = new GameObject("River", typeof(MeshFilter), typeof(MeshRenderer));

            gameObject.transform.localScale = new Vector3(30, 30, 1);

            gameObject.GetComponent<MeshFilter>().mesh = mesh;
            gameObject.GetComponent<MeshRenderer>().material = material;

        } //for
    }
}
