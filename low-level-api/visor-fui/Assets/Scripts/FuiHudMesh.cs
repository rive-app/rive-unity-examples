using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

using LoadAction = UnityEngine.Rendering.RenderBufferLoadAction;
using StoreAction = UnityEngine.Rendering.RenderBufferStoreAction;

namespace Rive
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct VisorVertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;

        public VisorVertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            this.position = position;
            this.normal = normal;
            this.uv = uv;
        }
    }

    [ExecuteInEditMode, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class FuiHudMesh : MonoBehaviour
    {
        public int subdivisions = 10;
        public float radius = 10.0f;

        private void OnEnable()
        {
            buildMesh();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            buildMesh();
        }
#endif

        private void buildMesh()
        {
            // Circumference of our cylinder in texture space = 2*pi*r
            float textureWidth = 1920 * 2;
            // Height
            float textureHeight = 1080 * 2;

            float inc = Mathf.PI / subdivisions;
            float angle = 0;
            float circumference = Mathf.PI * radius;
            // Find ratio with texture circumference to compute world space height.
            float height = circumference / (textureWidth * 2) * textureHeight;

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

            var vertices = meshData.GetVertexData<VisorVertex>();

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
                var normal = Vector3.Normalize(top);
                var u = angle / (Mathf.PI);

                vertices[vertexIndex] = new VisorVertex(top, normal, new Vector2(u, 0));
                vertices[vertexIndex + 1] = new VisorVertex(bottom, normal, new Vector2(u, 1));

                angle += inc;

                // Add faces only for subdivisions (final iteration is just for final vertex above).
                if (i < subdivisions)
                {
                    triangleIndices[triangleIndex++] = (ushort)vertexIndex;
                    triangleIndices[triangleIndex++] = (ushort)(vertexIndex + 2);
                    triangleIndices[triangleIndex++] = (ushort)(vertexIndex + 1);

                    triangleIndices[triangleIndex++] = (ushort)(vertexIndex + 2);
                    triangleIndices[triangleIndex++] = (ushort)(vertexIndex + 3);
                    triangleIndices[triangleIndex++] = (ushort)(vertexIndex + 1);

                    // send em backwards
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
            var mesh = new Mesh
            {
                name = "Fui Mesh",
                bounds = bounds
            };
            Mesh.ApplyAndDisposeWritableMeshData(dataArray, mesh);

#if !UNITY_EDITOR
            GetComponent<MeshFilter>().mesh = mesh;
#endif
        }
    }
}
