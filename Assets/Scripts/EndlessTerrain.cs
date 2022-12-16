using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static TerrainChunk;

public class EndlessTerrain : MonoBehaviour
{
    const float scale = 5f;

    public LODInfo[] detailsLevels;
    public static float maxViewDist;
    
    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    static MapGenerator mapGenerator;

    int chunkSize;
    int ChunkVisibleInViewDist;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionnary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();

        maxViewDist = detailsLevels[detailsLevels.Length - 1].visbleDistTreshhold;
        chunkSize = MapGenerator.mapChunkSize - 1;
        ChunkVisibleInViewDist = Mathf.RoundToInt(maxViewDist / chunkSize);
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z)/scale;
        UpdateVisibleChunk();
    }

    void UpdateVisibleChunk()
    {
        for(int i = 0; i< terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);
        
        for(int yOffset= -ChunkVisibleInViewDist; yOffset<= ChunkVisibleInViewDist; yOffset++)
        {
            for (int xOffset = -ChunkVisibleInViewDist; xOffset <= ChunkVisibleInViewDist; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if(terrainChunkDictionnary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionnary[viewedChunkCoord].UpdateTerrainChunk(maxViewDist,viewerPosition);
                    if (terrainChunkDictionnary[viewedChunkCoord].IsVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionnary[viewedChunkCoord]);
                    }
                }
                else
                {
                    terrainChunkDictionnary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord,chunkSize,detailsLevels,transform,mapMaterial,mapGenerator));
                }
            }
        }
    }
}

public class TerrainChunk
{
    GameObject meshObject;
    Vector2 position;
    Bounds bounds;

    MapGenerator mapGenerator;
    const float scale = 5f;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    LODInfo[] detailsLevels;
    LODMesh[] lODMeshes;

    MapData mapData;
    bool mapDataReceived;
    int previousLODIndex = -1;


    public TerrainChunk(Vector2 coord, int size, LODInfo[] detailsLevels, Transform parent, Material material, MapGenerator mapGenerator)
    {
        position = coord * size;
        this.mapGenerator = mapGenerator;
        this.detailsLevels= detailsLevels;
        

        bounds = new Bounds(position, Vector2.one * size);
        Vector3 positionV3 = new Vector3(position.x, 0, position.y+94);

        meshObject = new GameObject("Terrain Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();
        meshRenderer.material = material;

        meshObject.transform.position = positionV3 * scale;
        meshObject.transform.parent = parent;
        meshObject.transform.localScale = Vector3.one * scale;
        SetVisible(false);

        lODMeshes= new LODMesh[detailsLevels.Length];
        for(int i = 0; i < detailsLevels.Length; i++)
        {
            lODMeshes[i]= new LODMesh(detailsLevels[i].lod,mapGenerator);
        }

        mapGenerator.RequestMapData(position,OnMapDataReceived);
    }

    void OnMapDataReceived(MapData mapData)
    {
        this.mapData = mapData;
        mapDataReceived = true;

        Texture2D texture= TextureGenerator.TextureFromColourMap(mapData.colourMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
        meshRenderer.material.mainTexture= texture;
    }


    public void UpdateTerrainChunk(float maxViewDist, Vector2 viewerPosition)
    {
        if (mapDataReceived)
        {
            float viewerDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDistFromNearestEdge <= maxViewDist;

            if (visible)
            {
                int lodIndex = 0;
                for (int i = 0; i < detailsLevels.Length - 1; i++)
                {
                    if (viewerDistFromNearestEdge > detailsLevels[i].visbleDistTreshhold)
                    {
                        lodIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }
                if (lodIndex != previousLODIndex)
                {
                    LODMesh lodMesh = lODMeshes[lodIndex];
                    if (lodMesh.hasMesh)
                    {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                        meshCollider.sharedMesh = lodMesh.mesh;
                    }
                    else if (!lodMesh.hasRequestedMesh)
                    {
                        lodMesh.RequestMesh(mapData);
                    }
                }
            }
            SetVisible(visible);
        }
    }

    public void SetVisible(bool visible)
    {
        meshObject.SetActive(visible);
    }

    public bool IsVisible()
    {
        return meshObject.activeSelf;
    }

    class LODMesh{
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        MapGenerator mapGenerator;

        public LODMesh(int lod, MapGenerator mapGenerator)
        {
            this.lod = lod;
            this.mapGenerator = mapGenerator;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh= true;
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh= true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visbleDistTreshhold;
    }
}
