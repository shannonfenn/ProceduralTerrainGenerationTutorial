using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator {
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightScale, AnimationCurve baseHeightCurve, int levelOfDetail) {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        // for centering on the origin
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;
        AnimationCurve heightCurve = new AnimationCurve(baseHeightCurve.keys);

        int meshSimplificationIncrement = (levelOfDetail == 0)? 1 : 2 * levelOfDetail;
        int verticesPerLine = 1 + (width - 1) / meshSimplificationIncrement;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);

        for (int y = 0; y < height; y += meshSimplificationIncrement) {
            for (int x = 0; x < width; x += meshSimplificationIncrement) {
                // no triangles are created for right and bottom edge vertices
                if (x < width - 1 && y < height - 1) {
                    int tl = meshData.GetCurrentVertexIndex();
                    int tr = tl + 1;
                    int bl = tl + verticesPerLine;
                    int br = tl + verticesPerLine + 1;
                    meshData.AddTriangle(tl, br, bl);  // clockwise vertices for lower-left half
                    meshData.AddTriangle(br, tl, tr);  // clockwise vertices for upper-right half
                }
                meshData.AddVertexUV(new Vector3(topLeftX + x,
                                                heightCurve.Evaluate(heightMap[x, y]) * heightScale,
                                                topLeftZ - y),
                                    new Vector2(x / (float)width,
                                                y / (float)height));
            }
        }

        return meshData;
    }
}


public class MeshData {
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;

    private int currentTriangleIndex;
    private int currentVertexIndex;

    public MeshData(int meshWidth, int meshHeight) {
        currentTriangleIndex = 0;
        currentVertexIndex = 0;

        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 3 * 2]; // two triangles per quad, 3 vertices per triangle
    }

    public void AddTriangle(int a, int b, int c) {
        triangles[currentTriangleIndex] = a;
        triangles[currentTriangleIndex + 1] = b;
        triangles[currentTriangleIndex + 2] = c;
        currentTriangleIndex += 3;
    }

    public void AddVertexUV(Vector3 vertex, Vector2 uv) {
        vertices[currentVertexIndex] = vertex;
        uvs[currentVertexIndex] = uv;
        currentVertexIndex += 1;
    }

    public int GetCurrentVertexIndex() {
        return currentVertexIndex;
    }

    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}