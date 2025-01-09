using UnityEngine;
using UnityEngine.Rendering;


/// <summary>
/// Creates a procedural cylinder mesh designed for wrapping UI/textures with correct proportions.
/// Automatically rebuilds when properties are changed in the editor.
/// </summary>
/// <remarks>
/// Usage:
/// 1. Attach to a GameObject that will display your curved UI
/// 2. Set the radius and subdivisions based on your needs
/// 3. Set textureWidth and textureHeight to match your UI texture dimensions
/// 
/// The cylinder height is automatically calculated to maintain proper texture proportions
/// based on the radius and texture dimensions.
/// </remarks>
[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FuiCylinder : MonoBehaviour
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct FuiVertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;

        public FuiVertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            this.position = position;
            this.normal = normal;
            this.uv = uv;
        }
    }

    [Header("Mesh Properties")]
    [SerializeField, Tooltip("Number of segments around the cylinder. Higher numbers create a smoother curve but use more triangles")]
    private int subdivisions = 10;

    [SerializeField, Min(0.1f), Tooltip("Radius of the cylinder in world units")]
    private float radius = 10.0f;

    [Header("Texture Properties")]
    [SerializeField, Min(1f), Tooltip("Width of the texture in pixels. Used to calculate proper cylinder proportions")]
    private float textureWidth = 5000f;

    [SerializeField, Min(1f), Tooltip("Height of the texture in pixels. Used to calculate proper cylinder proportions")]
    private float textureHeight = 252f;

    private Mesh currentMesh;

    private MeshFilter meshFilter;

    private bool rebuildRequested;


    private void OnEnable()
    {
        BuildMesh();
    }

    private void OnDestroy()
    {
        // Clean up the generated mesh when the component is destroyed
        if (currentMesh != null)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                Destroy(currentMesh);
            }
            else
            {
                DestroyImmediate(currentMesh);
            }
#else
                Destroy(currentMesh);
#endif
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Ensure values are valid
        radius = Mathf.Max(0.1f, radius);
        textureWidth = Mathf.Max(1f, textureWidth);
        textureHeight = Mathf.Max(1f, textureHeight);

        //BuildMesh(); <- We can't call this in OnValidate. Unity logs an error if we do.
    }

    private void Update()
    {
        // Only rebuild in editor when requested
        if (rebuildRequested && !Application.isPlaying)
        {
            rebuildRequested = false;
            BuildMesh();
        }
    }
#endif

    /// <summary>
    /// Regenerates the cylinder mesh using current properties.
    /// </summary>
    private void BuildMesh()
    {
        if (textureWidth <= 0f || textureHeight <= 0f)
        {
            Debug.LogWarning($"Invalid texture dimensions on {gameObject.name}. Width: {textureWidth}, Height: {textureHeight}");
            return;
        }

        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        float inc = Mathf.PI * 2.0f / subdivisions;
        float angle = 0;
        float circumference = 2.0f * Mathf.PI * radius;
        // Find ratio with texture circumference to compute world space height.
        float height = circumference / textureWidth * textureHeight;

        // Extra one for final UV
        var vertexCount = (subdivisions + 1) * 2;
        var indexCount = subdivisions * 2 * 3 * 2; // x 2 for front and back

        Mesh.MeshDataArray dataArray = Mesh.AllocateWritableMeshData(1);
        var meshData = dataArray[0];

        meshData.SetVertexBufferParams(vertexCount, new[]
        {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
            });

        var vertices = meshData.GetVertexData<FuiVertex>();
        meshData.SetIndexBufferParams(indexCount, IndexFormat.UInt16);
        var triangleIndices = meshData.GetIndexData<ushort>();

        var vertexIndex = 0;
        var triangleIndex = 0;

        for (int i = 0; i <= subdivisions; i++)
        {
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            var bottom = new Vector3(x, 0, z);
            var top = new Vector3(x, height, z);
            var normal = Vector3.Normalize(new Vector3(x, 0, z));
            var u = angle / (Mathf.PI * 2.0f);

            vertices[vertexIndex] = new FuiVertex(top, normal, new Vector2(u, 0));
            vertices[vertexIndex + 1] = new FuiVertex(bottom, normal, new Vector2(u, 1));

            angle += inc;

            // Add faces only for subdivisions (final iteration is just for final vertex above).
            if (i < subdivisions)
            {
                // Front face triangles
                triangleIndices[triangleIndex++] = (ushort)vertexIndex;
                triangleIndices[triangleIndex++] = (ushort)(vertexIndex + 2);
                triangleIndices[triangleIndex++] = (ushort)(vertexIndex + 1);

                triangleIndices[triangleIndex++] = (ushort)(vertexIndex + 2);
                triangleIndices[triangleIndex++] = (ushort)(vertexIndex + 3);
                triangleIndices[triangleIndex++] = (ushort)(vertexIndex + 1);

                // Back face triangles (reversed winding)
                int from = triangleIndex - 1;
                for (int j = 0; j < 6; j++)
                {
                    triangleIndices[triangleIndex++] = triangleIndices[from--];
                }
            }
            vertexIndex += 2;
        }

        meshData.subMeshCount = 1;
        meshData.SetSubMesh(0, new SubMeshDescriptor(0, indexCount));

        var bounds = new Bounds();
        bounds.SetMinMax(new Vector3(-radius, 0, -radius), new Vector3(radius, height, radius));

        // Clean up old mesh
        if (currentMesh != null)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                Destroy(currentMesh);
            }
            else
            {
                DestroyImmediate(currentMesh);
            }
#else
                Destroy(currentMesh);
#endif
        }

        currentMesh = new Mesh
        {
            name = "FuiCylinder Mesh",
            bounds = bounds
        };

        Mesh.ApplyAndDisposeWritableMeshData(dataArray, currentMesh);
        meshFilter.mesh = currentMesh;
    }
}