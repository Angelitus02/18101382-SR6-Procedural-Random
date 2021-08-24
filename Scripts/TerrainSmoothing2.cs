using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainSmoothing2
{

    public static TerrainData SmoothTerrain(TerrainData terrainData, int smoothingPasses)
    {
        int resolution = terrainData.heightmapResolution;

        //using a copied variable and returns it after being modified
        float[,] copiedHeights = terrainData.GetHeights(0, 0, resolution, resolution);

        //old algorithm: iterate through terrain, 
        //if difference between one point and the neighbour is more than certain height
        //add the height to the smaller points and work outwards decreasing the heights by 0.01f 

        //better approach: do an average of 9 resolution points in array in neighbouring point

        //first loop does smoothing passes, second and third loop iterates through all points of the terrain(x and y)
        //then few more loops to check the neighbours
        for (int i = 0; i < smoothingPasses; i++)
        {
            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    //keep track of the total height to average
                    float averageHeight = 0f;
                    //track neighbours used to divide for average
                    int neighbours = 0;
                    //neighbours
                    for (int xOffset = -1; xOffset <= 1; xOffset++)
                        {
                            for (int yOffset = -1; yOffset <= 1; yOffset++)
                            {
                                // finding the index of the neighbour we will be comparing
                                int tempX = (x + xOffset);
                                int tempY = (y + yOffset);

                                // ensuring we don't wrap around and go out of bounds
                                if (tempX < 0 || tempY < 0 || tempX > (resolution - 1) || tempY > (resolution - 1))
                                {
                                    continue;
                                }

                                averageHeight += copiedHeights[tempX, tempY];
                                neighbours++;
                            } //for
                    } //for

                    // dividing and setting the new average height
                    copiedHeights[x , y] = averageHeight / neighbours;
                }//for
            }//for
        }

        terrainData.SetHeights(0, 0, copiedHeights);
        return terrainData;
    }

}
