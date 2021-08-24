using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainSmoothing
{

    public static TerrainData SmoothTerrain(TerrainData terrainData, int smoothingPasses)
    {
        int resolution = terrainData.heightmapResolution;

        //using a copied variable and returns it after being modified
        float[,] copiedHeights = terrainData.GetHeights(0, 0, resolution, resolution);

        //old algorithm: iterate through terrain, 
        //if difference between one point and the neighbour is more than certain height
        //add the height to the smaller points and work outwards decreasing the heights by 0.01f 

        //better approach: do an average of 5 resolution points in array in a cross pattern(+) and apply it to the array

        //the higher the resolution the more times we have to perform the smoothing
        //we will run the smoothing 10% times of the resolution so 1024 will be done 102 times or 256 will be done 25 times
        //int smoothingPasses = resolution / 10;

        //first loop does smoothing passes, second and third loop iterates through all points of the terrain(x and y)
        for (int i = 0; i < smoothingPasses; i++)
        {
            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    //neighbours in a cross + pattern (excluding diagonals) 
                    int belowX = (x - 1 + resolution) % resolution;
                    int aboveX = (x + 1) % resolution;
                    int leftY = (y - 1 + resolution) % resolution;
                    int rightY = (y + 1) % resolution; // 4+1 mod 1024 = 5 mod 1024 = 5 so it will never go outside 1024 with modulus

                    // plus(+) pattern, we add all heights and divide them by 5 to get the average, better smoothing performance wise than the previous one
                    copiedHeights[x, y] = (copiedHeights[x, y] + copiedHeights[x, rightY] + copiedHeights[x, leftY] + copiedHeights[belowX, y] + copiedHeights[aboveX, y]) / 5;

                    //if (Mathf.Abs(copiedHeights[x, y] - copiedHeights[x, rightY]) > 0.2f)
                    //if((copiedHeights[x, rightY] - copiedHeights[x, y]) > 0.1f)
                    //print("rightY height " + terrainData.GetHeight(x, rightY));
                    //print("Y height " + terrainData.GetHeight(x, y));
                    //if ((terrainData.GetHeight(x, rightY) - terrainData.GetHeight(x, y)) > 2f)
                    //{
                    //    float increase;
                    //    if (copiedHeights[x, rightY] > copiedHeights[x, y])
                    //    {

                    //        //print("triggered rightY height " + terrainData.GetHeight(x, rightY));
                    //        //print("triggered Y height " + terrainData.GetHeight(x, y));
                    //        increase = copiedHeights[x, rightY] - 0.01f;
                    //        //print("inc " + increase);
                    //        int j = y;
                    //        for (float i = 0.01f; i < 0.075f; i += 0.01f)
                    //        {

                    //            copiedHeights[x, j] = increase - i;
                    //            j--;
                    //        }
                    //        //copiedHeights[x, y] = increase;
                    //        //copiedHeights[x, y - 2] = increase - 0.02f;
                    //        //copiedHeights[x, y - 3] = increase - 0.03f;

                }//for
            }//for
        }

        terrainData.SetHeights(0, 0, copiedHeights);
        return terrainData;
    }

}
