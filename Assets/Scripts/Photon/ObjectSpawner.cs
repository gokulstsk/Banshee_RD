using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using Photon.Pun;
public class ObjectSpawner : MonoBehaviour
{
    public enum ObjectType { Gem, Sheild, SprintShoes};
    public Tilemap tilemap;
    public GameObject[] ObjectPrefabs;
    public GameObject[] WeaponPrefabs;
    public float ShieldProbablity = 0.2f;
    public float SprintShoesProbablity = 0.1f;
  
    public int MaxObjects = 20;
    public int MaxWeaponObjects = 6;
    public float ShieldLifeTime = 10f;
    public float SpawnInterval = 0.5f;

    private List<Vector3> validSpawnPoints  = new List<Vector3>();
    private List<GameObject> spawnObjects = new List<GameObject>();
    private List<GameObject> weaponObjects = new List<GameObject>();
    public List<GameObject> OverallObjects = new List<GameObject>();
    public Transform ObjectContainer;
    public Transform WeaponContainer;
    public PhotonView photonView;

    private bool isOSpawning = false;
    private bool isWSpawning = false;

    public GameObject[] Gem;
    public GameObject[] Sheild;
    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        
    }

    public void SpawnGE()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GatherValidPoints();
            SpawnGameObjects();
        }
    }

    public void DestroyGE()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            DestroyGameObjects();
        }
        else
        {
            // photonView.RPC("DestroryObjectsNotMasterClient", RpcTarget.All);
            //estroryObjectsNotMasterClient();
        }
    }

   
    private void DestroryObjectsNotMasterClient()
    {
       
        Gem = GameObject.FindGameObjectsWithTag("Coin");

        if(Gem.Length!=0)
        {
            Debug.Log(Gem.Length + "gEM COUNT");
            foreach (GameObject g in Gem)
            {

               // Destroy(g);
            }
        }
        

        
        Sheild = GameObject.FindGameObjectsWithTag("Shield");

        if (Sheild.Length != 0)
        {
            Debug.Log(Gem.Length + "sHIELD COUNT");
            foreach (GameObject g in Sheild)
            {
               // Destroy(g);
            }
        }
            
    }
    
    public void SpawnGameObjects()
    {
        isOSpawning = true;
        isWSpawning = true;
        StartCoroutine(SpawnObjectsIfNeeded());
        StartCoroutine(SpawnWeaponObjectsIfNeeded());
       
    }

    
    public void DestroyGameObjects()
    {
        DestroySpawnObjects();
       // photonView.RPC("DestroySpawnObjects", RpcTarget.All);
    }
    // Update is called once per frame
    void Update()
    {
        if(!tilemap.gameObject.activeInHierarchy)
        {
            //level change
        }
        //if(!isSpawning && ActiveObjectsCount() < MaxObjects)
        //{
        //    StartCoroutine(SpawnObjectsIfNeeded());
        //}
    }

    private int ActiveObjectsCount()
    {
        spawnObjects.RemoveAll(item => item == null);
        return spawnObjects.Count;
    }

    private int ActiveWeaponObjectsCount()
    {
        weaponObjects.RemoveAll(item => item == null);
        return weaponObjects.Count;
    }
    private IEnumerator SpawnObjectsIfNeeded()
    {
        while(isOSpawning)
        {
            SpawnObjects();
            yield return new WaitForSeconds(SpawnInterval);
        }
    }
    private IEnumerator SpawnWeaponObjectsIfNeeded()
    {
        while(isWSpawning)
        {
            SpawnWeaponObjects();
            yield return new WaitForSeconds(SpawnInterval);
        }
    }

    private bool PositionHasObjects(Vector3 positionToCheck)
    {
        return OverallObjects.Any(checkObj => checkObj && Vector3.Distance(checkObj.transform.position, positionToCheck) < 5.0f);
    }

    private ObjectType RandomObjectType()
    {
        float RandomChoice = Random.value;
        if(RandomChoice <= ShieldProbablity) //0.5f
        {
            
            return ObjectType.Sheild;
        }
        if (RandomChoice <= (ShieldProbablity + SprintShoesProbablity)) //0.5+0.2
        {
            
            return ObjectType.SprintShoes;
        }
        else
        {
            
            return ObjectType.Gem;
        }
    }

    private void SpawnObjects()
    {
        
        if (ActiveObjectsCount() == MaxObjects)
        {
            isOSpawning = false;
        }

        if (validSpawnPoints.Count == 0)
            return;

        
        Vector3 SpawnPositions = Vector3.zero;
        bool isValidPositionFound  = false;

        while(!isValidPositionFound && validSpawnPoints.Count > 0)
        {
            int randomIndex = Random.Range(0, validSpawnPoints.Count);
            Vector3 potentialPositions = validSpawnPoints[randomIndex];
            Vector3 lestPosition = potentialPositions + Vector3.left;
            Vector3 rightPosition = potentialPositions + Vector3.right;

            if(!PositionHasObjects(lestPosition) && !PositionHasObjects(rightPosition))
            {
                SpawnPositions = potentialPositions;
                isValidPositionFound = true;
                
            }
            validSpawnPoints.RemoveAt(randomIndex);
        }

        if(isValidPositionFound)
        {
            ObjectType objectType = RandomObjectType();
            // int randomGunIndex = Random.Range(0, ObjectPrefabs.Length);
            //Instantiate
            //GameObject gameObject = Instantiate(ObjectPrefabs[(int)objectType], SpawnPositions, Quaternion.identity);
            GameObject gameObject = PhotonNetwork.Instantiate(ObjectPrefabs[(int)objectType].name, SpawnPositions, Quaternion.identity);
            gameObject.transform.SetParent(ObjectContainer, false);
            spawnObjects.Add(gameObject);
            OverallObjects.Add(gameObject);
            ////Destroy Shield only after time
            //if(objectType != ObjectType.Weapon)
            //{
            //    StartCoroutine(DestroyGameObjectAfterTime(gameObject, ShieldLifeTime));
            //}

        }
    }

    private void SpawnWeaponObjects()
    {
        if (validSpawnPoints.Count == 0)
            return;
        if(ActiveWeaponObjectsCount() == MaxWeaponObjects)
        {
            isWSpawning = false;
        }
        Vector3 SpawnPositions = Vector3.zero;
        bool isValidPositionFound = false;

        while (!isValidPositionFound && validSpawnPoints.Count > 0)
        {
            int randomIndex = Random.Range(0, validSpawnPoints.Count);
            Vector3 potentialPositions = validSpawnPoints[randomIndex];
            Vector3 lestPosition = potentialPositions + Vector3.left;
            Vector3 rightPosition = potentialPositions + Vector3.right;

            if (!PositionHasObjects(lestPosition) && !PositionHasObjects(rightPosition))
            {
                SpawnPositions = potentialPositions;
                isValidPositionFound = true;
            }
            validSpawnPoints.RemoveAt(randomIndex);
        }

        if (isValidPositionFound)
        {
           // ObjectType objectType = RandomObjectType();
             int randomGunIndex = Random.Range(0, WeaponPrefabs.Length);
            //Instantiate
            //GameObject gameObject = Instantiate(WeaponPrefabs[randomGunIndex], SpawnPositions, Quaternion.identity);
            GameObject gameObject = PhotonNetwork.Instantiate(WeaponPrefabs[randomGunIndex].name, SpawnPositions, Quaternion.identity);
            gameObject.transform.SetParent(WeaponContainer, false);
            weaponObjects.Add(gameObject);
            OverallObjects.Add(gameObject);
        }
    }

    
    private void DestroySpawnObjects()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (GameObject gameObject in spawnObjects)
            {
                if (gameObject != null)
                {
                    Debug.Log("spawnobject destory count" + spawnObjects.Count);
                    //PhotonNetwork.Destroy(gameObject);
                }
            }
            foreach (GameObject gameObject in weaponObjects)
            {
                if (gameObject != null)
                {
                    Debug.Log("Weaponobject destory count" + weaponObjects.Count);
                    //PhotonNetwork.Destroy(gameObject);
                }
            }
            foreach (GameObject gameObject in OverallObjects)
            {
                if (gameObject != null)
                {
                    Debug.Log("OverallObject destory count" + OverallObjects.Count);
                    PhotonNetwork.Destroy(gameObject);
                }
            }
            spawnObjects.Clear();
            weaponObjects.Clear();
            OverallObjects.Clear();
        }
        else
        {
            Debug.Log("OverallObject destory count Not a master client");
        }
    }

    public void RemoveSO(GameObject gameObject)
    {
        if (PhotonNetwork.IsMasterClient)
            spawnObjects.Remove(gameObject);
    }
    public void RemoveWO(GameObject gameObject)
    {
        if (PhotonNetwork.IsMasterClient)
            weaponObjects.Remove(gameObject);
    }
    public void RemoveOO(GameObject gameObject)
    {
        if (PhotonNetwork.IsMasterClient)
            OverallObjects.Remove(gameObject);
    }
    public void AddWO(GameObject gameObject)
    {
        if (PhotonNetwork.IsMasterClient)
            weaponObjects.Add(gameObject);
    }
    public void AddOO(GameObject gameObject)
    {
        if (PhotonNetwork.IsMasterClient)
            OverallObjects.Add(gameObject);
    }
    private IEnumerator DestroyGameObjectAfterTime(GameObject gameObject, float time)
    {
        yield return new WaitForSeconds(time);
        if(gameObject)
        {
            spawnObjects.Remove(gameObject);
            validSpawnPoints.Add(gameObject.transform.position);
            Destroy(gameObject);
        }
    }

    
    private void GatherValidPoints()
    {
        validSpawnPoints.Clear();
        BoundsInt boundsInt = tilemap.cellBounds;
        Debug.Log(boundsInt + "Bounds Int");
        TileBase[] allTiles = tilemap.GetTilesBlock(boundsInt);
        Vector3 start = tilemap.CellToWorld(new Vector3Int(boundsInt.xMin, boundsInt.yMin, 0));

        for(int x = 0; x < boundsInt.size.x; x++)
        {
            for (int y = 0; y < boundsInt.size.y; y++)
            {
                    TileBase tile = allTiles[x + y * boundsInt.size.x];
                if(tile != null)
                {
                    Vector3 place = start + new Vector3(x + 3f, y + 5f, 0);
                    validSpawnPoints.Add(place);
                }
            }
        }

        Debug.Log(validSpawnPoints.Count + "valid spawn points");

    }
}
