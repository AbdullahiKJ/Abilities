using System.Collections.Generic;
using UnityEngine;

public class SandDeformationManager : MonoBehaviour
{
    [Header("Terrain")]
    private Terrain terrain;

    [Header("Deformation")]
    [SerializeField] private float updatesPerSecond = 15f;

    [Tooltip("Absolute minimum terrain height in metres.")]
    [SerializeField] private float minimumHeight = 0.25f;

    private TerrainData terrainData;

    private readonly List<SandDeformation> activeDeformations = new();

    private float updateTimer;
    private float[,] heights;

    [Header("Dirty Region")]
    private int dirtyMinX;
    private int dirtyMinZ;
    private int dirtyMaxX;
    private int dirtyMaxZ;
    private bool hasDirtyRegion;

    private void Awake()
    {
        terrain = GetComponentInChildren<Terrain>();
        terrainData = terrain.terrainData;

        int resolution = terrainData.heightmapResolution;

        heights = terrainData.GetHeights(
            0,
            0,
            resolution,
            resolution
        );
    }

    private void Update()
    {
        if (activeDeformations.Count == 0)
            return;

        updateTimer += Time.deltaTime;

        float updateInterval = 1f / updatesPerSecond;

        if (updateTimer < updateInterval)
            return;

        float deltaTime = updateTimer;
        updateTimer = 0f;

        ProcessDeformations(deltaTime);
    }

    public void CreateDepression(
        Vector3 position,
        float radius,
        float depth,
        float duration)
    {
        activeDeformations.Add(
            new SandDeformation(
                SandDeformationType.Depression,
                position,
                radius,
                depth,
                duration
            )
        );
    }

    public void CreateMound(
        Vector3 position,
        float radius,
        float height,
        float duration)
    {
        activeDeformations.Add(
            new SandDeformation(
                SandDeformationType.Raise,
                position,
                radius,
                height,
                duration
            )
        );
    }

    private void ProcessDeformations(float deltaTime)
    {
        ResetDirtyRegion();

        for (int i = activeDeformations.Count - 1; i >= 0; i--)
        {
            SandDeformation deformation = activeDeformations[i];

            float deltaProgress = deformation.Advance(deltaTime);

            ApplyDeformation(
                deformation,
                deltaProgress
            );

            if (deformation.IsFinished)
            {
                activeDeformations.RemoveAt(i);
            }
        }

        UploadDirtyRegion();
    }

    private void ApplyDeformation(
        SandDeformation deformation,
        float deltaProgress)
    {
        Vector3 terrainPosition = terrain.transform.position;

        // Get the local position of the deformation relative to the terrain
        Vector3 localPosition = deformation.worldPosition - terrainPosition;

        int resolution = terrainData.heightmapResolution;

        // Get the distance between each pixel
        float spacingX = terrainData.size.x / (resolution - 1);
        float spacingZ = terrainData.size.z / (resolution - 1);

        // Get the centre of the deformation in pixel coordinates
        int centerX = Mathf.RoundToInt(localPosition.x / spacingX);
        int centerZ = Mathf.RoundToInt(localPosition.z / spacingZ);

        int radiusX = Mathf.CeilToInt(deformation.radius / spacingX);
        int radiusZ = Mathf.CeilToInt(deformation.radius / spacingZ);

        float signedAmount =
            deformation.type == SandDeformationType.Depression
                ? -deformation.amount
                : deformation.amount;

        // Only apply this tick's portion.
        float amountThisTick = signedAmount * deltaProgress;

        // Normalise the deformation amount (0-1)
        float normalizedAmount = amountThisTick / terrainData.size.y;

        // Get the normalised minimum height
        float normalizedMinimum = minimumHeight / terrainData.size.y;

        // Loop through each pixel in the square patch (center +- radius)
        for (int z = centerZ - radiusZ;
             z <= centerZ + radiusZ;
             z++)
        {
            if (z < 0 || z >= resolution)
                continue;

            for (int x = centerX - radiusX;
                 x <= centerX + radiusX;
                 x++)
            {
                if (x < 0 || x >= resolution)
                    continue;

                float worldX = x * spacingX;
                float worldZ = z * spacingZ;

                // get the distance from the centre of the deformation
                float distance = Vector2.Distance(
                    new Vector2(worldX, worldZ),
                    new Vector2(
                        localPosition.x,
                        localPosition.z
                    )
                );

                // Ignore points outside of the circle
                if (distance > deformation.radius)
                    continue;

                float t = 1f - distance / deformation.radius;

                // Smooth falloff
                float falloff = t * t * (3f - 2f * t);

                // Get the change in height
                float delta = normalizedAmount * falloff;

                heights[z, x] = Mathf.Clamp(
                    heights[z, x] + delta,
                    normalizedMinimum,
                    1f
                );

                MarkDirty(x, z);
            }
        }
    }

    private void ResetDirtyRegion()
    {
        dirtyMinX = int.MaxValue;
        dirtyMinZ = int.MaxValue;

        dirtyMaxX = int.MinValue;
        dirtyMaxZ = int.MinValue;

        hasDirtyRegion = false;
    }

    private void MarkDirty(int x, int z)
    {
        dirtyMinX = Mathf.Min(dirtyMinX, x);
        dirtyMinZ = Mathf.Min(dirtyMinZ, z);

        dirtyMaxX = Mathf.Max(dirtyMaxX, x);
        dirtyMaxZ = Mathf.Max(dirtyMaxZ, z);

        hasDirtyRegion = true;
    }

    private void UploadDirtyRegion()
    {
        if (!hasDirtyRegion)
            return;

        int width = dirtyMaxX - dirtyMinX + 1;
        int height = dirtyMaxZ - dirtyMinZ + 1;

        float[,] region = new float[height, width];

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                region[z, x] = heights[dirtyMinZ + z, dirtyMinX + x];
            }
        }

        terrainData.SetHeights(
            dirtyMinX,
            dirtyMinZ,
            region
        );
    }
}