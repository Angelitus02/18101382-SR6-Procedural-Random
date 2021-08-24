using UnityEngine;

// Needs a gameobject with a terrain component attached for it to work
[RequireComponent(typeof(Terrain))]
public class TerrainCreator : MonoBehaviour
{

    public int seed = 0;

    //size of the terrain, resolution will stretch
    public int width = 25, height = 25;

    //limited to 12 as heightmap resolution can be a maximm of 4096, which is 2^12
    [Range(0, 12)]
    public int detail = 8;

    //how many pixels per point, 256, 512, 768 or 1024 are good numbers. Always needs to be in 256 increments
    private int resolution = 1024;

    //how high our terrain reaches
    public int maxHeight = 2;

    private float highestPoint = 0f;

    //number of high points
    [Range(0, 100)]
    public int highPoints = 1;

    // Initial ssettings for land
    public float landWavelength = 1f;
    [Range(-1f, 1f)]
    public float landFrequency = 1f;

    // Will create multiple passes to create realistic land
    public float[] distortionWavelength = new float[] {5f};
    [Range(-1f, 1f)]
    public float[] distortionFrequency = new float[] {0f};

    public int numberOfRain = 2;
    public float inertia = 0.1f; // With inertia, which is from 0-1, at 0 means the direction of the rain droplet will not change
    // and follow from the old direction. At 1 it means it will always change direction to go downhill
    public float minumSedimentCapacity = .01f; // Needed to stop reaching 0 capacity of flat terrain
    public float sedimentCapacityFactor = 4; // Multiplier for how much sediment a droplet can carry
    public float depositionSpeed = 0.2f;
    public float erosionSpeed = 0.2f;
    public float evaporationSpeed = .01f;
    public float grav = 4f;
    public int maxPath = 30;
    public float startingSpeed = 1;
    public float startingWater = 1;

    [Range(0f, 500f)]
    public int smoothing = 0;

        //number of rivers to form
    [Range(0, 500)]
    public int numberOfRivers = 2;
    public Material waterMaterial;

    //parameters for our pseudorandom perlin noise calculation
    private float offsetW;
    private float offsetH;

    public void BuildTerrain()
    {
        //delete any previous existing rivers
        foreach(GameObject gameObj in FindObjectsOfType<GameObject>())
        {
            if(gameObj.name == "River")
            {
                Object.DestroyImmediate(gameObj);
            }
        }

        //can use left shift to calculate anything to the power of 2
        //will use this to set heighmap resolution
        // +1 because heightmap is an array
        resolution = (1 << detail) + 1;
        //All random calls will be based on this seed
        Random.InitState(seed);
        offsetW = Random.Range(0f, 9999f);
        offsetH = Random.Range(0f, 9999f);

        //my script is attached to a Terrain that we GetComponent from
        Terrain terr = GetComponent<Terrain>();

        //Creates a terrain and performs first pass of perlin noise with the centre being the highest point
        terr.terrainData = CreateSurface(terr.terrainData);

        //second pass of perlin noise using Perlin Distorsion, makes a more realistic texture on mountains
        terr.terrainData = Distorsion(terr.terrainData);

        terr.terrainData = WaterErosion.Erosion(terr.terrainData, numberOfRain, inertia, minumSedimentCapacity, sedimentCapacityFactor,
            depositionSpeed, erosionSpeed, evaporationSpeed, grav, maxPath, startingSpeed, startingWater);

        //smoothes the terrain by calculating averages of neighbours 
        terr.terrainData = TerrainSmoothing2.SmoothTerrain(terr.terrainData, smoothing);

        //place rivers on the map
        terr.terrainData = Rivers.PlaceRivers(terr.terrainData, numberOfRivers, waterMaterial);

        //needed for assigning textures based on height
        highestPoint = CalculateHighestPoint(terr.terrainData);

        //assigns textures depending on height and steepness
        terr.terrainData = ColourTerrain.Terraintest1(terr.terrainData, highestPoint);
    }

    //Method to calculate the highest point of the highest mountain peak in the terrain
    private float CalculateHighestPoint(TerrainData terrainData)
    {
        float highestPoint = 0f;
        for (int x = 0; x < terrainData.heightmapResolution + 1; x++)
        {
            for (int y = 0; y < terrainData.heightmapResolution + 1; y++)
            {
                if (highestPoint < terrainData.GetHeight(x, y))
                {
                    highestPoint = terrainData.GetHeight(x, y);
                }
            }
        }
        return highestPoint;
    }

    //Method that returns terrainData and updates the terraindata of the terrain in start
    TerrainData CreateSurface(TerrainData terrdata)
    {
        terrdata.heightmapResolution = resolution;

        //initialise the vector3 size of terrain, x,z,y
        //ideally maxHeight should be customisable depending on the zone of the terrain and not per terrain object?
        //maxHeight is the maximum height of the map
        terrdata.size = new Vector3(width, maxHeight, height);

        //TerrainData.html: SetHeights sets an array of heightmap samples from 0,0
        //CreateMountains returns a 2D float array used to set the heights of all points in the resolution of the terrain
        terrdata.SetHeights(0, 0, CreateMountains());

        return terrdata;
    }

    // float[,] means 2D arrayd of floats,  SetHeights uses this
    private float[,] CreateMountains()
    {
        //need to return a 2d array
        float[,] heights = new float[resolution, resolution];

        //centre point of the terrain
        float[,] highPoints = Gradients();

        //loop through all points
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                //we substract the distance to centre so that the centre has the highest height
                heights[x, y] = CalculatePerlin(x, y, landWavelength, landFrequency) - highPoints[x,y];

            }//for
        }//for

        return heights;
    }

    private float[,] Gradients()
    {
        float[,] gradient = new float[resolution, resolution];

        for (int i = 0; i < highPoints; i++)
        {
            //random point on map which is the high point we will use to
            //calculate gradient from and assign values
            int pointX = Random.Range(0, resolution - 1);
            int pointY = Random.Range(0, resolution - 1);

            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    //distance formula d=squareroot((x1-x2)^2 + (y1-y2)^2)
                    //coordinates of first point
                    float distanceX = (pointX - x) * (pointX - x);
                    //coordinates of second point
                    float distanceY = (pointY - y) * (pointY - y);

                    //distance formula
                    float distanceToPoint = Mathf.Sqrt(distanceX + distanceY);
                    //normalise
                    distanceToPoint /= resolution;

                    //check that the distanceToPoint is smaller than what is currently stored as we are
                    //using multiple gradients and don't want to override
                    if (distanceToPoint < gradient[x, y] || i == 0)
                    {
                        gradient[x, y] = distanceToPoint;
                    }

                }//for
            }//for
        }//for

        return gradient;
    }

    private TerrainData Distorsion(TerrainData terr)
    {
        // to add create distortion to the terrain, the user needs to have provided parameters to use
        // as there are no values to loop through, so return
        if(distortionWavelength.Length == 0 || distortionFrequency.Length == 0) return terr;
        // the legnth of both arrays needs to be the same otherwise will get an out of bounds error when applying parameters
        if(distortionWavelength.Length != distortionFrequency.Length) return terr;
        //need to return a 2d array
        float[,] heights = terr.GetHeights(0, 0, resolution, resolution);

        for(int i = 0; i < distortionWavelength.Length; i++)
        {
            //x passes, distortion for nicer grainier mountains and more realistic mountains
            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++) { 

                    // we can substract, multiply or add, they all work to make distorsion, two parameters scale and frequency
                    // prefer to subtract as hitting the maximum height will make very weird mountains, however 0 height is just
                    // flat terrain
                    heights[x, y] -= CalculatePerlin(x, y, distortionWavelength[i], distortionFrequency[i]);
                }// for y
            }// for x
        } //for i

        terr.SetHeights(0, 0, heights);

        return terr;
    }

    //perlin distortion
    private float CalculatePerlin(int w, int h, float distortionWavelength, float distortionFrequency)
    {
        //second pass of perlin for distorsion
        //distortionFrequency is the frequency of the second perlin noise added
        //distortionWavelength is how strong we want the changes of the noise waves of the perlin noise
        return distortionWavelength * Mathf.PerlinNoise(w * distortionFrequency + offsetW, h * distortionFrequency + offsetH);
    }
}

