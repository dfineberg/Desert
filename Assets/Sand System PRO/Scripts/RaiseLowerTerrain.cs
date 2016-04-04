using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Terrain))]
[AddComponentMenu("Mesh/SSTerrain/Raise Lower Terrain")]
public class RaiseLowerTerrain : MonoBehaviour
{
    bool TestWithMouse = false;
    public Terrain targetTerrain;
    TerrainData TerrainDataBuf;
    public GameObject player;
    public float maxDepth = 0.0005f;
    public float depthOfStep = 0.0003f;
    public float step = 0.9f;
    Vector3 lastPlayerPosition;
    public int SmoothArea = 0;

    private int xResolution;
    private int zResolution;
    private float[,] heights;
    private float[,] heightMapBackup;
    protected const float DEPTH_METER_CONVERT = 0.05f;
    protected const float TEXTURE_SIZE_MULTIPLIER = 1.25f;

    public int DeformationTextureNum = 1;
    
    protected int alphaMapWidth;
    protected int alphaMapHeight;
    protected int numOfAlphaLayers;
    private float[, ,] alphaMapBackup;


    void Start()
    {
        xResolution = targetTerrain.terrainData.heightmapWidth;
        zResolution = targetTerrain.terrainData.heightmapHeight;
        alphaMapWidth = targetTerrain.terrainData.alphamapWidth;
        alphaMapHeight = targetTerrain.terrainData.alphamapHeight;
        numOfAlphaLayers = targetTerrain.terrainData.alphamapLayers;

        TerrainDataBuf = Object.Instantiate(targetTerrain.terrainData);        

        /*if (Debug.isDebugBuild)
        {*/
            heights = targetTerrain.terrainData.GetHeights(0, 0, xResolution, zResolution);
            heightMapBackup = targetTerrain.terrainData.GetHeights(0, 0, xResolution, zResolution);
            alphaMapBackup = targetTerrain.terrainData.GetAlphamaps(0, 0, alphaMapWidth, alphaMapHeight);
        //}

        if (player != null)
            lastPlayerPosition = player.transform.position;

       /* GameObject go = Terrain.CreateTerrainGameObject(Terrain.activeTerrain.terrainData);
        go.hideFlags = HideFlags.HideAndDontSave;
        terrainBackup = new Terrain();
        terrainBackup = go.GetComponent<Terrain>();        
        Object.Destroy(go);*/
    }

    void OnApplicationQuit()
    {
        /*if (Debug.isDebugBuild)
        {*/
            targetTerrain.terrainData.SetHeights(0, 0, heightMapBackup);
            targetTerrain.terrainData.SetAlphamaps(0, 0, alphaMapBackup);
        //}
         //   Object.Destroy(terrainBackup);
    }


    void Update()
    {
        if (player.GetComponent<TerrainCollDetection>().haveCollision)
        {
            if (TestWithMouse == true)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit))
                    {
                        // area middle point x and z, area width, area height, smoothing distance, area height adjust
                        raiselowerTerrainArea(hit.point, 10, 10, SmoothArea, 0.01f);
                        // area middle point x and z, area size, texture ID from terrain textures
                        TextureDeformation(hit.point, 10 * 2f, DeformationTextureNum);
                    }
                }
                if (Input.GetMouseButtonDown(1))
                {
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit))
                    {
                        // area middle point x and z, area width, area height, smoothing distance, area height adjust
                        raiselowerTerrainArea(hit.point, 1, 1, SmoothArea, -0.01f);
                        // area middle point x and z, area size, texture ID from terrain textures
                        TextureDeformation(hit.point, 1 * 2f, 0);
                    }
                }
            }
            else
            {
                if (player != null)
                {
                    if (Mathf.Abs(player.transform.position.x - lastPlayerPosition.x) > step &&
                        Mathf.Abs(player.transform.position.z - lastPlayerPosition.z) > step)
                    {
                        raiselowerTerrainArea(lastPlayerPosition, 1, 1, SmoothArea, -depthOfStep);
                        // area middle point x and z, area size, texture ID from terrain textures
                        TextureDeformation(lastPlayerPosition, 1 * 2f, DeformationTextureNum);
                        lastPlayerPosition = player.transform.position;
                    }
                }
            }
        }
    }


    private void raiselowerTerrainArea(Vector3 point, int lenx, int lenz, int smooth, float incdec)
    {
        point -= GetComponent<Transform>().localPosition;
        int areax;
        int areaz;
        smooth += 1;
        float smoothing;
        int terX = (int)((point.x / targetTerrain.terrainData.size.x) * xResolution);
        int terZ = (int)((point.z / targetTerrain.terrainData.size.z) * zResolution);
        lenx += smooth;
        lenz += smooth;
        terX -= (lenx / 2);
        terZ -= (lenz / 2);
        if (terX < 0) terX = 0;
        if (terX > xResolution) terX = xResolution;
        if (terZ < 0) terZ = 0;
        if (terZ > zResolution) terZ = zResolution;
        float[,] heights = targetTerrain.terrainData.GetHeights(terX, terZ, lenx, lenz);
        float[,] heightsB = TerrainDataBuf.GetHeights(terX, terZ, lenx, lenz);

        for (smoothing = 1; smoothing < smooth + 1; smoothing++)
        {
            float multiplier = smoothing / smooth;
            for (areax = (int)(smoothing / 2); areax < lenx - (smoothing / 2); areax++)
                for (areaz = (int)(smoothing / 2); areaz < lenz - (smoothing / 2); areaz++)
                {
                    // Debug.Log((heightsB[areax, areaz] - maxDepth <= heights[areax, areaz] - depthOfStep).ToString() + " " + heightsB[areax, areaz].ToString() + "<=" + (heights[areax, areaz] - depthOfStep).ToString());
                    if ((areax > -1) && (areaz > -1) && (areax < xResolution) && (areaz < zResolution))
                    {
                        if (heightsB[areax, areaz] - maxDepth <= heights[areax, areaz] - depthOfStep)
                        {
                            heights[areax, areaz] = heights[areax, areaz] - depthOfStep;
                        }
                        else
                        {
                            heights[areax, areaz] = heightsB[areax, areaz] - maxDepth;
                        }
                    }
                    //Debug.Log(heights[areax, areaz]);
                }
        }
        targetTerrain.terrainData.SetHeights(terX, terZ, heights);
    }
    
    protected void TextureDeformation(Vector3 pos, float craterSizeInMeters, int textureIDnum)
    {
        Vector3 alphaMapTerrainPos = GetRelativeTerrainPositionFromPos(pos, targetTerrain, alphaMapWidth, alphaMapHeight);
        int alphaMapCraterWidth = (int)(craterSizeInMeters * (alphaMapWidth / targetTerrain.terrainData.size.x));
        int alphaMapCraterLength = (int)(craterSizeInMeters * (alphaMapHeight / targetTerrain.terrainData.size.z));
        int alphaMapStartPosX = (int)(alphaMapTerrainPos.x - (alphaMapCraterWidth / 2));
        int alphaMapStartPosZ = (int)(alphaMapTerrainPos.z - (alphaMapCraterLength / 2));
        float[, ,] alphas = targetTerrain.terrainData.GetAlphamaps(alphaMapStartPosX, alphaMapStartPosZ, alphaMapCraterWidth, alphaMapCraterLength);
        float circlePosX;
        float circlePosY;
        float distanceFromCenter;
        for (int i = 0; i < alphaMapCraterLength; i++) //width
        {
            for (int j = 0; j < alphaMapCraterWidth; j++) //height
            {
                circlePosX = (j - (alphaMapCraterWidth / 2)) / (alphaMapWidth / targetTerrain.terrainData.size.x);
                circlePosY = (i - (alphaMapCraterLength / 2)) / (alphaMapHeight / targetTerrain.terrainData.size.z);
                distanceFromCenter = Mathf.Abs(Mathf.Sqrt(circlePosX * circlePosX + circlePosY * circlePosY));
                if (distanceFromCenter < (craterSizeInMeters / 2.0f))
                {
                    for (int layerCount = 0; layerCount < numOfAlphaLayers; layerCount++)
                    {
                        //could add blending here in the future
                        if (layerCount == textureIDnum)
                        {
                            alphas[i, j, layerCount] = 1;
                        }
                        else
                        {
                            alphas[i, j, layerCount] = 0;
                        }
                    }
                }
            }
        }
        targetTerrain.terrainData.SetAlphamaps(alphaMapStartPosX, alphaMapStartPosZ, alphas);
    }

    protected Vector3 GetNormalizedPositionRelativeToTerrain(Vector3 pos, Terrain terrain)
    {
        Vector3 tempCoord = (pos - terrain.gameObject.transform.position);
        Vector3 coord;
        coord.x = tempCoord.x / targetTerrain.terrainData.size.x;
        coord.y = tempCoord.y / targetTerrain.terrainData.size.y;
        coord.z = tempCoord.z / targetTerrain.terrainData.size.z;
        return coord;
    }

    protected Vector3 GetRelativeTerrainPositionFromPos(Vector3 pos, Terrain terrain, int mapWidth, int mapHeight)
    {
        Vector3 coord = GetNormalizedPositionRelativeToTerrain(pos, terrain);
        return new Vector3((coord.x * mapWidth), 0, (coord.z * mapHeight));
    }
}
