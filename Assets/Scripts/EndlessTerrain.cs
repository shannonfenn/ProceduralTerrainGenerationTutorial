using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public const float maxViewDistance = 450;
    public Transform viewerTransform;
    public static Vector2 viewerPosition;

    public Material mapMaterial;

    static MapGenerator mapGenerator;

    int chunkSize;
    int numVisibleChunks;

    Dictionary<Vector2, TerrainChunk> terrainChunkDict= new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> chunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start() {
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.chunkSize - 1;
        numVisibleChunks = Mathf.RoundToInt(maxViewDistance / chunkSize);
    }

    void Update() {
        viewerPosition = new Vector2(viewerTransform.position.x, viewerTransform.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks() {
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int i = 0; i < chunksVisibleLastUpdate.Count; i++) {
            chunksVisibleLastUpdate[i].SetVisible(false);
        }
        chunksVisibleLastUpdate.Clear();

        for (int yOffset = - numVisibleChunks; yOffset <= numVisibleChunks; yOffset++) {
            for (int xOffset = - numVisibleChunks; xOffset <= numVisibleChunks; xOffset++) {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDict.ContainsKey(viewedChunkCoord)) {
                    terrainChunkDict[viewedChunkCoord].UpdateChunk();
                    if(terrainChunkDict[viewedChunkCoord].IsVisible()) {
                        chunksVisibleLastUpdate.Add(terrainChunkDict[viewedChunkCoord]);
                    }
                }
                else {
                    terrainChunkDict.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, mapMaterial));
                }
            }
        }
    }

    public class TerrainChunk  {
        
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        
        public TerrainChunk(Vector2 coordinate, int size, Transform parent, Material material) {
            position = coordinate * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 position3D = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            
            meshRenderer.material = material;

            meshObject.transform.position = position3D;
            meshObject.transform.parent = parent;

            SetVisible(false);

            mapGenerator.RequestMapData(OnMapDataRecieved);
        }

        void OnMapDataRecieved(MapData mapData) {
            mapGenerator.RequestMeshData(mapData, OnMeshDataRecieved);
        }

        void OnMeshDataRecieved(MeshData meshData) {
            meshFilter.mesh = meshData.CreateMesh();
        }

        public void UpdateChunk() {
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;
            SetVisible(visible);
        }

        public void SetVisible(bool visible) {
            meshObject.SetActive(visible);
        }

        public bool IsVisible() {
            return meshObject.activeSelf;
        }
    }
}
