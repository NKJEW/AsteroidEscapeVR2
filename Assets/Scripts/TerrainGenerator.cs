using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {
    public static TerrainGenerator instance;

    public float terrainStep;
    public float beltRadius;
    public float chunkSize;
    public int maxNumChunks;

    [Space(15)]
    public float launchRadius;
    public float launchBufferOffset;
    public float minChaosLaunchWait;
    public float maxChaosLaunchWait;
    public float launchWaitVariance;
    public float launchConeAngle;
    public float minLaunchSpeed;
    public float maxLaunchSpeed;
    public float minChaosFlamingChance;
    public float maxChaosFlamingChance;
    public float minFlamingSpeed;
    public float maxFlamingSpeed;
    public float chaosIncreaseRate;
    float nextLaunch;

    [Space(15)]
    public GameObject asteroidPrefab;
    public GameObject asteroidExplosion;
    public GameObject flamingAsteroidPrefab;
    public AsteroidData[] asteroidDatas;
    Dictionary<int, List<Mesh>> asteroidsBySize = new Dictionary<int, List<Mesh>>();
    int maxAsteroidSize;

    int curChunkId;
    List<GameObject> chunks = new List<GameObject>();

    Rigidbody playerRb;

    void Awake() {
        instance = this;
        SetupAsteroidDatas();
    }

    void Start() {
        playerRb = FindObjectOfType<PlayerController>().GetComponent<Rigidbody>();
        nextLaunch = Time.time + Random.Range(maxChaosLaunchWait, minChaosLaunchWait);
        UpdateTerrain();
    }

    void SetupAsteroidDatas() {
        int curMaxSize = 0;

        foreach (AsteroidData data in asteroidDatas) {
            if (!asteroidsBySize.ContainsKey(data.size)) {
                asteroidsBySize.Add(data.size, new List<Mesh>());
                if (data.size > curMaxSize) {
                    curMaxSize = data.size;
                }
            }
            asteroidsBySize[data.size].Add(data.mesh);
        }

        maxAsteroidSize = curMaxSize + 1;
    }

    void UpdateTerrain() {
        int nextChunkId = curChunkId + 1; // currently chunks only load forwards
        float chunkStart = GetPosForChunkId(curChunkId);
        float chunkEnd = GetPosForChunkId(nextChunkId);
        float curSpawnPos = chunkStart;

        GameObject newChunk = new GameObject("Chunk" + curChunkId);

        while (curSpawnPos < chunkEnd) {
            Vector2 circlePos = Random.insideUnitCircle * beltRadius;
            Vector3 spawnPos = new Vector3(circlePos.x, circlePos.y, curSpawnPos);

            CreateRandomAsteroid(spawnPos, newChunk.transform);

            curSpawnPos += terrainStep;
        }

        // delete farthest back chunk
        if (chunks.Count == maxNumChunks) {
            Destroy(chunks[0]);
            chunks.RemoveAt(0);
        }
        chunks.Add(newChunk);

        curChunkId = nextChunkId;
    }

    void Update() {
        if (Time.time > nextLaunch) {
            nextLaunch = Time.time + Mathf.Max(maxChaosLaunchWait, Mathf.Lerp(minChaosLaunchWait, maxChaosLaunchWait, GetChaosFactor()) + Random.Range(-launchWaitVariance, launchWaitVariance));
            LaunchAsteroid();
        }
    }

    GameObject CreateRandomAsteroid(Vector3 spawnPos, Transform parentChunk) {
        GameObject newAsteroid = Instantiate(asteroidPrefab, spawnPos, Quaternion.identity, parentChunk);
        newAsteroid.GetComponent<AstriodController>().Init(Random.Range(1, maxAsteroidSize));
        return newAsteroid;
    }

    void LaunchAsteroid() {
        bool isFlaming = Random.value > Mathf.Lerp(minChaosFlamingChance, maxChaosFlamingChance, GetChaosFactor());

        float launchSpeed = (!isFlaming) ? Random.Range(minLaunchSpeed, maxLaunchSpeed) : Random.Range(minFlamingSpeed, maxFlamingSpeed);
        float buffer = Mathf.Max(launchRadius * playerRb.velocity.z / launchSpeed, 50f) + Random.Range(-launchBufferOffset, launchBufferOffset); //z position to spawn at, include this arbitrary minimum distance so that it can't be too close or spawn behind us

        float randomAngle = Random.Range(0, Mathf.PI); // a random angle on the top of the unit circle
        Vector3 spawnPos = new Vector3(Mathf.Cos(randomAngle) * launchRadius, Mathf.Sin(randomAngle) * launchRadius, playerRb.position.z + buffer);

        GameObject newMovingAsteroid;
		Quaternion launchAngle = Quaternion.Euler(0, Random.Range(-launchConeAngle, launchConeAngle), (randomAngle * Mathf.Rad2Deg) - 180f + Random.Range(-launchConeAngle, launchConeAngle));

        if (!isFlaming) {
            newMovingAsteroid = CreateRandomAsteroid(spawnPos, chunks[chunks.Count - 1].transform); //include it in the farthest forward chunk so that it unloads last
        } else {
            newMovingAsteroid = Instantiate(flamingAsteroidPrefab, spawnPos, launchAngle, chunks[chunks.Count - 1].transform);
        }
        Rigidbody newAsteroidRb = newMovingAsteroid.GetComponent<Rigidbody>();
        newAsteroidRb.drag = 0;

        newAsteroidRb.velocity = launchAngle * Vector3.right * launchSpeed;
    }

    void LateUpdate() {
        if (playerRb.transform.position.z + chunkSize > GetPosForChunkId(curChunkId - 1)) { // if we are close to the "next chunk" we should make a new one
            UpdateTerrain();
        }
    }

    float GetPosForChunkId(int id) {
        return id * chunkSize + transform.position.z;
    }

    float GetChaosFactor() { //chaos factor (0 to 1) is based on distance and affects the rate of moving and flaming asteroids
        float curPlayerDistance = playerRb.transform.position.z;
        return 1 - (1 / Mathf.Sqrt(chaosIncreaseRate * Mathf.Max(curPlayerDistance,0) + 1));
    }

    public Mesh GetMeshOfSize(int size) {
        List<Mesh> datas = asteroidsBySize[size];
        return datas[Random.Range(0, datas.Count)];
    }

    [System.Serializable]
    public struct AsteroidData {
        public Mesh mesh;
        public int size;
    }
}
