using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class SandParticleController : MonoBehaviour
{
    [Header("Particles")]
    [SerializeField] private int particleCount = 10000;

    [Header("Recovery")]
    [SerializeField] private float springStrength = 60f;
    [SerializeField] private float damping = 12f;
    [SerializeField] private float displacedDrag = 2f;
    [SerializeField] private float recoveryDistance = 0.03f;

    private NativeArray<SandParticle> particles;
    private NativeArray<float3> meshTargets;
    private NativeArray<float3> previousMeshTargets;

    private readonly List<SandHitData> pendingHits = new();

    // Particle mesh settings
    private NativeArray<float3> vertices;
    private NativeArray<int> triangles;
    private NativeArray<SandSurfacePoint> surfacePoints;
    private List<Vector3> bakedVertices;
    private NativeArray<float> particleSizes;
    private NativeArray<Matrix4x4> matrices;


    [Header("Mesh")]
    [SerializeField] SkinnedMeshRenderer skinnedMeshRenderer;
    private Mesh bakedMesh;

    // Script References
    private SandParticleRenderer particleRenderer;

    private void Awake()
    {
        // Get the particle renderer script
        particleRenderer = GetComponent<SandParticleRenderer>();

        particles = new NativeArray<SandParticle>(
            particleCount,
            Allocator.Persistent
        );

        meshTargets = new NativeArray<float3>(
            particleCount,
            Allocator.Persistent
        );

        previousMeshTargets = new NativeArray<float3>(
            particleCount,
            Allocator.Persistent
        );

        for (int i = 0; i < particleCount; i++)
        {
            particles[i] = new SandParticle
            {
                position = float3.zero,
                velocity = float3.zero,
                recoveryTimer = 0f,
                state = SandParticleState.Attached
            };
        }

        bakedMesh = new Mesh();
        bakedMesh.MarkDynamic();

        // Original mesh topology
        Mesh sourceMesh = skinnedMeshRenderer.sharedMesh;

        // Allocate persistent arrays
        vertices = new NativeArray<float3>(
            sourceMesh.vertexCount,
            Allocator.Persistent
        );

        triangles = new NativeArray<int>(
            sourceMesh.triangles,
            Allocator.Persistent
        );

        surfacePoints = new NativeArray<SandSurfacePoint>(
            particleCount,
            Allocator.Persistent
        );
        InitializeSurfacePoints();

        bakedVertices = new List<Vector3>(sourceMesh.vertexCount);

        // Create the particle sizes and matrices arrays
        particleSizes = new NativeArray<float>(
            particleCount,
            Allocator.Persistent
        );

        var random = new Unity.Mathematics.Random(1u);
        for (int i = 0; i < particleSizes.Length; i++)
            particleSizes[i] = random.NextFloat(particleRenderer.MinParticleSize, particleRenderer.MaxParticleSize);

        matrices = new NativeArray<Matrix4x4>(
            particleCount,
            Allocator.Persistent
        );
    }

    public void RegisterHit(SandHitData hit)
    {
        pendingHits.Add(hit);
    }

    private void LateUpdate()
    {
        // Bake the mesh and update the vertices
        skinnedMeshRenderer.BakeMesh(bakedMesh);
        bakedMesh.GetVertices(bakedVertices);

        for (int i = 0; i < bakedVertices.Count; i++)
        {
            vertices[i] = bakedVertices[i];
        }

        // Schedule the sand mesh target job
        var meshJob = new SandMeshTargetJob
        {
            vertices = vertices,
            triangles = triangles,
            surfacePoints = surfacePoints,
            meshTargets = meshTargets,

            localToWorld = (float4x4)skinnedMeshRenderer.localToWorldMatrix
        };

        JobHandle targetHandle = meshJob.Schedule(particles.Length, 64);

        JobHandle hitHandle = default;

        foreach (SandHitData hit in pendingHits)
        {
            hitHandle = new SandHitJob
            {
                particles = particles,
                hit = hit
            }.Schedule(particles.Length, 64, hitHandle);
        }

        pendingHits.Clear();

        // Combine the mesh target and hit jobs
        JobHandle handle = JobHandle.CombineDependencies(
            targetHandle,
            hitHandle
        );

        var updateJob = new SandParticleUpdateJob
        {
            particles = particles,
            meshTargets = meshTargets,
            previousMeshTargets = previousMeshTargets,

            deltaTime = Time.deltaTime,
            springStrength = springStrength,
            damping = damping,
            displacedDragFactor = math.exp(-displacedDrag * Time.deltaTime),
            recoveryDistance = recoveryDistance
        };

        handle = updateJob.Schedule(
            particles.Length,
            64,
            handle
        );

        var renderJob = new SandRendererMatrixJob
        {
            particles = particles,
            particleSizes = particleSizes,
            matrices = matrices,
        };

        handle = renderJob.Schedule(
            particles.Length,
            64,
            handle
        );

        handle.Complete();

        // Render particles
        particleRenderer.Render(matrices);
    }

    private void OnDestroy()
    {
        if (particles.IsCreated)
            particles.Dispose();
        if (meshTargets.IsCreated)
            meshTargets.Dispose();
        if (previousMeshTargets.IsCreated)
            previousMeshTargets.Dispose();

        if (vertices.IsCreated)
            vertices.Dispose();
        if (triangles.IsCreated)
            triangles.Dispose();
        if (surfacePoints.IsCreated)
            surfacePoints.Dispose();

        if (particleSizes.IsCreated)
            particleSizes.Dispose();
        if (matrices.IsCreated)
            matrices.Dispose();

        Destroy(bakedMesh);
    }

    private void InitializeSurfacePoints()
    {
        Mesh mesh = skinnedMeshRenderer.sharedMesh;

        Vector3[] meshVertices = mesh.vertices;
        int[] meshTriangles = mesh.triangles;

        int triangleCount = meshTriangles.Length / 3;

        // Calculate cumulative triangle areas.
        float[] cumulativeAreas = new float[triangleCount];

        float totalArea = 0f;

        for (int i = 0; i < triangleCount; i++)
        {
            int index = i * 3;

            Vector3 a = meshVertices[meshTriangles[index]];
            Vector3 b = meshVertices[meshTriangles[index + 1]];
            Vector3 c = meshVertices[meshTriangles[index + 2]];

            float area = Vector3.Cross(b - a, c - a).magnitude * 0.5f;

            totalArea += area;
            cumulativeAreas[i] = totalArea;
        }

        if (totalArea <= 0f)
        {
            Debug.LogError("Mesh has no valid surface area.");
            return;
        }

        // Generate one stable surface point per particle.
        for (int i = 0; i < particleCount; i++)
        {
            // Select a triangle weighted by its area.
            float randomArea = UnityEngine.Random.value * totalArea;

            int triangleIndex = System.Array.BinarySearch(
                cumulativeAreas,
                randomArea
            );

            if (triangleIndex < 0)
                triangleIndex = ~triangleIndex;

            triangleIndex = Mathf.Clamp(
                triangleIndex,
                0,
                triangleCount - 1
            );

            // Generate uniform barycentric coordinates.
            float r1 = UnityEngine.Random.value;
            float r2 = UnityEngine.Random.value;

            float sqrtR1 = Mathf.Sqrt(r1);

            float3 barycentric = new float3(
                1f - sqrtR1,
                sqrtR1 * (1f - r2),
                sqrtR1 * r2
            );

            surfacePoints[i] = new SandSurfacePoint
            {
                triangleIndex = triangleIndex,
                barycentric = barycentric
            };
        }
    }
}