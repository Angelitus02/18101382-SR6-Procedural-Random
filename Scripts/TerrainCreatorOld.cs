//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainCreatorOld : MonoBehaviour
{

    public int seed = 0;

    //size of the terrain, resolution will stretch
    public int width = 25, height = 25;

    //how many pixels per point, 256, 512, 768 or 1024 are good numbers. Always needs to be in 256 increments
    private int resolution = 1024;

    //how high our terrain reaches
    public int depth = 2;
    private float maxHeight = 0f;

    //parameters for our pseudorandom perlin noise calculation
    public float offsetW = 100f;
    public float offsetH = 100f;

    //Mountain and land have different scales or perlin noise
    [Range(-20f, 20f)]
    public float mountainScale = 3.3f;

    [Range(0f, 1f)]
    public float mountainPercent = 0.2f;

    [Range(-20f, 20f)]
    public float landScale = 1.3f;

    public float distorsionScale = 1f;
    public float wiggleDensity = 1f;

    private int mountainSize;
    private int mountainStartPointX;
    private int mountainStartPointY;

    //Ideally, all calculations are done at run time once. Update() is used for debugging in real time or water simulations later
    private void Start()
    {
        //All random calls will be based on this seed
        Random.InitState(seed);
        offsetW = Random.Range(0f, 9999f);
        offsetH = Random.Range(0f, 9999f);

        //my script is attached to a Terrain that we GetComponent from
        Terrain terr = GetComponent<Terrain>();
        terr.terrainData = CreateSurface(terr.terrainData);
        //terr.terrainData = TerrainSmoothing.SmoothTerrain(terr.terrainData);

        terr.terrainData.SetHeights(0, 0, DistorsionTest(terr.terrainData));

        //print(CalculateHighestPoint(terr.terrainData));


        maxHeight = CalculateHighestPoint(terr.terrainData);

        terr.terrainData = ColourTerrain.Terraintest1(terr.terrainData, maxHeight);

    }
    void Update()
    {
        
    }

    //Method to calculate the highest point of the highest mountain peak in the terrain
    private float CalculateHighestPoint(TerrainData terrainData)
    {
        float maxHeight = 0f;
        for (int x = 0; x < terrainData.heightmapResolution + 1; x++)
        {
            for (int y = 0; y < terrainData.heightmapResolution + 1; y++)
            {
                if (maxHeight < terrainData.GetHeight(x, y))
                {
                    maxHeight = terrainData.GetHeight(x, y);
                }
            }
        }
        return maxHeight;
    }

    //Method that returns terrainData and updates the terraindata of the terrain in start
    TerrainData CreateSurface(TerrainData terrdata)
    {
        // +1 because heightmap is an array
        terrdata.heightmapResolution = resolution + 1;

        //initialise the vector3 size of terrain, x,z,y
        //ideally depth should be customisable depending on the zone of the terrain and not per terrain object?
        terrdata.size = new Vector3(width, depth, height);

        //TerrainData.html: SetHeights sets an array of heightmap samples from 0,0
        //CreateMountains returns a 2D float array used to set the heights of all points in the resolution of the terrain
        terrdata.SetHeights(0, 0, CreateMountains());

        return terrdata;
    }

    // float[,] means 2D array of floats,  SetHeights uses this
    private float[,] CreateMountains()
    {
        //need to return a 2d array
        float[,] heights = new float[resolution, resolution];

        mountainSize = (int)(resolution * mountainPercent);
        mountainStartPointX = Random.Range(0, resolution);
        mountainStartPointY = Random.Range(0, resolution);

        //loop through all points, changed from h,w to x,y bc we work with resolution not width/height
        //if we're in the middle, we perform mountainScale perlin calculations to make mountains in the middle square
        //two different loops for calculating perlin, first pass, land
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                //divided by 2 because we want the land to be lower than mountains
                heights[x, y] = CalculatePerlin(x, y, landScale) / 2;
            }//for
        }//for

        int tempX, tempY;
        //second pass, mountains
        for (int x = 0; x < mountainSize; x++)
        {
            tempX = (mountainStartPointX + x) % resolution;

            for (int y = 0; y < mountainSize; y++)
            {

                tempY = (mountainStartPointY + y) % resolution;

                //if (x > quarter && x < threeQuarters && y > quarter && y < threeQuarters)
                heights[tempX, tempY] = CalculatePerlin(tempX, tempY, mountainScale);


            }//for
        }//for

        return heights;
    }

    private float[,] DistorsionTest(TerrainData terr)
    {
        //need to return a 2d array
        float[,] heights = terr.GetHeights(0, 0, resolution, resolution);

        int tempX, tempY;
        //third pass, distortion for nicer grainier mountains
        for (int x = 0; x < mountainSize; x++)
        {
            tempX = (mountainStartPointX + x) % resolution;
            for (int y = 0; y < mountainSize; y++)
            {
                tempY = (mountainStartPointY + y) % resolution;
                heights[tempX, tempY] -= CalculateDistorsion(tempX, tempY, distorsionScale, wiggleDensity);


            }//for y
        }//for x

        return heights;
    }

    private float CalculatePerlin(int w, int h, float scale)
    {
        //perlin noise takes 2 numbers and a pseudo random method to calculate random numbers that form a wave
        float wCoord = (float)w / (width) * scale + offsetW;
        float hCoord = (float)h / (height) * scale + offsetH;

        //float number = Mathf.PerlinNoise(i * 0.3f + offsetX, j * 0.3f + offsetZ) * 2f;
        return Mathf.PerlinNoise(wCoord, hCoord);
    }

    //perlin distorsion
    private float CalculateDistorsion(int w, int h, float distorsionScale, float wiggleDensity)
    {
        return distorsionScale * Mathf.PerlinNoise((w + 2.3f) * wiggleDensity, (h -4.3f) * wiggleDensity);
    }
}

