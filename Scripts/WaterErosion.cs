using UnityEngine;

public class WaterErosion
{
    public static TerrainData Erosion(TerrainData terrainData, int numOfRain, float inertia, float minumSedimentCapacity,
            float sedimentCapacityFactor, float depositionSpeed, float erosionSpeed, float evaporationSpeed, float grav, int maxPath,
            float startingSpeed, float startingWater) {

        int resolution = terrainData.heightmapResolution;
        float[,] copiedHeights = terrainData.GetHeights(0, 0, resolution, resolution);

        for (int i = 0; i < numOfRain; i++) {
            // pick a random point on the map
            int pointX = Random.Range(1, resolution - 2);
            int pointY = Random.Range(1, resolution - 2);
            float speed = startingSpeed;
            float water = startingWater;
            float directionX = 0;
            float directionY = 0;
            float sediment = 0;

            for (int path = 0; path < maxPath; path++) {
                int oldX = pointX;
                int oldY = pointY;


                // Get the rains direction of flow
                float[] gradients = GetGradient(copiedHeights, pointX, pointY);
                float currentHeight = terrainData.GetHeight(pointX, pointY);

                // Get the direction of the rain
                directionX = (directionX * inertia - gradients[0] * (1 - inertia));
                directionY = (directionY * inertia - gradients[1] * (1 - inertia));

                // move in direction by 1
                // since we can only move in the direction of neighbours need to find if the number is positive or negative and
                // convert it to int, to move a single square
                pointX += (directionX == 0) ? 0 : (directionX > 0) ? 1 : -1;
                pointY += (directionY == 0) ? 0 : (directionY > 0) ? 1 : -1;

                // ensuring we don't go out of bounds and that the rain is still moving
                if ((directionX == 0 && directionY == 0) || pointX < 1 || pointY < 1 || pointX > (resolution - 2) || pointY > (resolution - 2))
                {
                    break;
                }

                // Find the new height and get the heightChange
                float newHeight = terrainData.GetHeight(pointX, pointY);
                // a negative heightchange means we move downwards, if it's positive it moves up the slope
                float heightChange = newHeight - currentHeight;

                // Get the rain sediment capacity, this capacity is higher when moving fast down a slope as well as containing a lot of water
                float sedimentCapacity = Mathf.Max(-heightChange * speed * water * sedimentCapacityFactor, minumSedimentCapacity);

                // If the rain is carrying more sediment than the sedimentCapacity, or if the rain is going up a slope
                if (sediment > sedimentCapacity || heightChange > 0) {
                    // If going up a slope (heightChange > 0) then fill it to the newHeight so there is no pit left behind,
                    // in the other case that sediment is more than the capacity, then deposit some of the excess sediment onto the old point
                    float depositAmount = (heightChange > 0) ? Mathf.Min(heightChange, sediment) : (sediment - sedimentCapacity) * depositionSpeed;
                    // remove the amount were depositing from our current sediment
                    sediment -= depositAmount;
                    // the depositAmount is in world amounts, however heighmap is between 0-1 so need to normalise to scale
                    copiedHeights[oldX, oldY] += depositAmount / terrainData.size.y;
                } else {
                    // If there is sedimentCapacity available, apply Erosion
                    // It erodes a set amount thats also determined by the erosionSpeed

                    // Need to clamp to a minimum, as we can't erode more than the height difference, if we did, would be
                    // able to dig big holes into the terrain in the old position
                    float erodeAmount = Mathf.Min((sedimentCapacity - sediment) * erosionSpeed, -heightChange);

                    // Get by how much we will erode, can't erode more than the current height of the point. If so return the height of the point
                    // Otherwise the change will be by the erodeAmount
                    float sedimentChange = (copiedHeights[oldX, oldY] < (erodeAmount/terrainData.size.y)) ? copiedHeights[oldX, oldY] : (erodeAmount / terrainData.size.y);

                    // apply the erosion on the old point
                    copiedHeights[oldX, oldY] -= sedimentChange;
                    // update the current sediment by the eroded amount, added to sediment
                    sediment += sedimentChange;
                }

                // After one cycle update the water content and speed of rain
                speed = Mathf.Sqrt(speed * speed + heightChange * grav);
                water *= (1 - evaporationSpeed);
            }
        }
        terrainData.SetHeights(0, 0, copiedHeights);
        return terrainData;
    }

    private static float[] GetGradient(float[,] copiedHeights, int x, int y) {
        // to get the gradient of the slope, in which direction it's going
        // we get all the four neighbouring cells of the point
        float heightTopLeft = copiedHeights[x - 1, y + 1];
        float heightTopRight = copiedHeights[x + 1, y + 1];
        float heightBottomLeft = copiedHeights[x - 1, y - 1];
        float heightBottomRight = copiedHeights[x + 1, y - 1];

        // Then calculate the gradient along the x axis and the y axis. Can do this by subtracting the heights
        // of the points
        float gradientX = (heightTopRight - heightTopLeft) + (heightBottomRight - heightBottomLeft);
        float gradientY = (heightBottomLeft - heightTopLeft) + (heightBottomRight - heightTopRight);

        // return this gradient
        return new float[] { gradientX, gradientY };
    }
}
