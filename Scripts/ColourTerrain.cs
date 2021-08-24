using System.Linq;
using UnityEngine;

public class ColourTerrain
{
    public static TerrainData Terraintest1(TerrainData t, float maxHeight)
    {
        //Instead of calculating maxHeight here we get it as a parameter from the terraincreator.cs

        //This is the # of textures you have added in the terrain editor
        int texturesAdded = t.alphamapLayers;

        //pass these as parameters for it to be public
        float maxAltitude = 1f;
        float minAltitude = 0.8f;
        float minSteepness = 0f;
        float maxSteepness = 0.65f;

        //3D array of alphamap layers, the x and y(positions in terrain) and the z(z here is the correspondent texture)
        float[,,] map = new float[t.alphamapWidth, t.alphamapHeight, t.alphamapLayers];

        //unity always needs a default texture or it will be filled with black. 
        //To avoid this we make sure there's always an else statement that uses at least one texture.

        // Looping through every point of the alphamap in the terrain
        for (int y = 0; y < t.alphamapHeight; y++)
        {
            for (int x = 0; x < t.alphamapWidth; x++)
            {
                //normalisation of x and y. Normalisation makes the points go from for example 0-1024 to 0-1
                float ynorm = (float)y / (float)t.alphamapHeight;
                float xnorm = (float)x / (float)t.alphamapWidth;

                //get height from this x and y point. used for rules that depend on height later
                float height = t.GetHeight(Mathf.RoundToInt(ynorm * t.heightmapResolution), Mathf.RoundToInt(xnorm * t.heightmapResolution));

                //normalise the previous height 
                float hnorm = height / maxHeight;

                // Calculate the steepness of the terrain at this location
                float steepness = t.GetSteepness(ynorm, xnorm);

                // Normalise the steepness by dividing it by the maximum steepness: 90 degrees
                float normSteepness = steepness / 90.0f;

                //array to record texture weights. 1f means 100% of one texture. 0.5f means 50% of one texture.
                float[] mapWeights = new float[t.alphamapLayers];

                //loop through the textures, assign the texture as 1f to apply full texture or 0.5f for a blend of both
                for (int i = 0; i < texturesAdded; i++)
                {
                    //rule for water, under a certain height threshold? 
                    //more advanced: shadow areas are sandy, flatter big areas have grass
                    //different water than mapmagic: create water on high height terrains with a setting.

                    //Rules go here. Snow takes priority if there is a certain height.
                    //if high, snow.
                    if (hnorm >= minAltitude && hnorm <= maxAltitude)
                    {

                        mapWeights[0] = 0.9f;
                        mapWeights[1] = 0.1f;
                    }
                    //if not very steep, mix of snow and cliff
                    else if (normSteepness >= minSteepness && normSteepness <= maxSteepness)
                    {
                        mapWeights[2] = 1f;
                    }
                    //else if (hnorm >= (minAltitude - 0.2f) && hnorm <= (maxAltitude - 0.4f))
                    //{
                    //    mapWeights[2] = 0.9f;
                    //    mapWeights[0] = 0.1f;
                    //}
                    //default, steepness must be between 0.7f and 1f. Cliff texture
                    else
                    {
                        mapWeights[1] = 1f;
                    }
                }

                //max used to later normalise
                float z = mapWeights.Sum();

            	//They cycle through all x and y points of the terrain and assign a texture in the z axis
                //with a respective weight retrieved from the mapWeights array that is set up upon the rules coded.
                for (int i = 0; i < t.alphamapLayers; i++)
                {
                    //normalise, z is sum of all weights (1f)
                    mapWeights[i] /= z;

                    //texture assigning
                    map[x, y, i] = mapWeights[i];
                }

            }
        }
        t.SetAlphamaps(0, 0, map);

        return t;
    }
}