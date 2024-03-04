using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun.UtilityScripts;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class Gamemanager : MonoBehaviourPunCallbacks
{
    public GameObject PlayerPrefab;
    public GameObject StartScreen;
    public GameObject SceneCam;

    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI PingRateText;
    public TextMeshProUGUI HealthText;

    public GameObject[] groundweapons;


    public static Gamemanager instance = null;

    //player respawn
    private bool b_Level;
    private float LevelTimeAmount = 5;
    public TextMeshProUGUI LevelTimer;
    public TextMeshProUGUI LevelCountText;
    public GameObject LevelUI;
    [HideInInspector]
    public int LevelCount = 0;

    public int maxLevels = 5;
    //player respawn
    private bool startRespawn;
    private float TimeAmount = 3;
    public TextMeshProUGUI spawnTimer;
    public GameObject respawnUI;

    [HideInInspector]
    public GameObject LocalPlayer;

    [HideInInspector]
    public int KillCount;
    [HideInInspector]
    public int Deathcount;
    [HideInInspector]
    public int GemsCount;
    [HideInInspector]
    public int isPlayerDead;

    public TextMeshProUGUI KillCountText;
    public TextMeshProUGUI KillFeedtext;

    public GameObject LeaderboardUI;
    public Leaderboard leaderboard;
    public GameObject GameOverUI;
    public TextMeshProUGUI PlayerWintext;

    private bool GS;
    public string[] x;
    private int d;
    public ObjectSpawner randomObjectSpawn;
    public Transform ObjectContainer;
    

    private void Awake()
    {
        //PhotonNetwork.OfflineMode = true;
        instance = this;
        StartScreen.gameObject.SetActive(true);
        HealthText.text = "100";
        KillCountText.text = "0";
    }
    // Start is called before the first frame update
    void Start()
    {
        //SpawnPlayer();
        GameStart();
        leaderboard = GetComponent<Leaderboard>();
        randomObjectSpawn = GetComponent<ObjectSpawner>();
        
    }

    void GameStart()
    {
        GS = true;
        maxLevels = 5;
        LevelCount = 0;
        KillCount = 0;
        Deathcount = 0;
        GemsCount = 0;
        isPlayerDead = 0;
        d = 0;
        SetHashes();
        EnableLevel();
        
    }


    // Update is called once per frame
    void Update()
    {
        //Player Spawn Timer
        if (startRespawn)
        {
            StartRespawn();
        }
        //Level Loading Timer
        if (b_Level)
        {
            LevelStart();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LeaveRoom();
        }
        PingRateText.text = PhotonNetwork.GetPing().ToString();
    }

    public void StartRespawn()
    {
        TimeAmount -= Time.deltaTime;
        //spawnTimer.text = "Respawn in : " + TimeAmount.ToString("F0");

        if (TimeAmount <= 0)
        {
            HealthText.text = "100";
            respawnUI.SetActive(false);
            startRespawn = false;
            LocalPlayer.GetComponent<HealthController>().EnableInputs();
            LocalPlayer.GetComponent<PhotonView>().RPC("Revive", RpcTarget.AllBuffered);
            LocalPlayer.GetComponent<WeaponController>().EquipWeapon();

            LocalPlayer.GetComponent<BansheePlayer>().playerProfileData.isDead = false;
            float randomSpawn = Random.Range(-20, 50);
            LocalPlayer.transform.position = new Vector2(PlayerPrefab.transform.position.x + randomSpawn, PlayerPrefab.transform.position.y);
            LocalPlayer.transform.rotation = Quaternion.identity;
            isPlayerDead = 0;
            SetHashes();
        }
    }

    public void SetPlayerState(int x)
    {
        switch (x)
        {
            case 1: // player without weapons
                LocalPlayer.GetComponent<BansheePlayer>().isIdle = true;
                LocalPlayer.GetComponent<BansheePlayer>().isGunEquipped = false;
                LocalPlayer.GetComponent<BansheePlayer>().isSwordEquipped = false;
                LocalPlayer.GetComponent<BansheePlayer>().SetPlayerAnimator();
                break;
            case 2: // player with Gun
                LocalPlayer.GetComponent<BansheePlayer>().isIdle = false;
                LocalPlayer.GetComponent<BansheePlayer>().isGunEquipped = true;
                LocalPlayer.GetComponent<BansheePlayer>().isSwordEquipped = false;
                LocalPlayer.GetComponent<BansheePlayer>().SetPlayerAnimator();
                break;
            case 3: // player with Sword
                LocalPlayer.GetComponent<BansheePlayer>().isIdle = false;
                LocalPlayer.GetComponent<BansheePlayer>().isGunEquipped = false;
                LocalPlayer.GetComponent<BansheePlayer>().isSwordEquipped = true;
                LocalPlayer.GetComponent<BansheePlayer>().SetPlayerAnimator();
                break;
            default:
                LocalPlayer.GetComponent<BansheePlayer>().isIdle = true;
                LocalPlayer.GetComponent<BansheePlayer>().isGunEquipped = false;
                LocalPlayer.GetComponent<BansheePlayer>().isSwordEquipped = false;
                LocalPlayer.GetComponent<BansheePlayer>().SetPlayerAnimator();
                break;
        }
    }
    public void EnableRespawn()
    {
        LevelUI.SetActive(false);
        TimeAmount = 2;
        startRespawn = true;
        //respawnUI.SetActive(true);
    }


    public void SpawnPlayer()
    {

        LevelUI.SetActive(false);
        float randomSpawn = Random.Range(-20, 50);
        // PhotonNetwork.Instantiate(PlayerPrefab.name, new Vector2(PlayerPrefab.transform.position.x * randomSpawn, PlayerPrefab.transform.position.y), Quaternion.identity, 0);
        PhotonNetwork.Instantiate(PlayerPrefab.name, new Vector2(PlayerPrefab.transform.position.x + randomSpawn, PlayerPrefab.transform.position.y), Quaternion.identity, 0);
        StartScreen.gameObject.SetActive(false);
        SceneCam.gameObject.SetActive(false);
        LevelCount = 1;
        KillCount = 0;
        Deathcount = 0;
        GemsCount = 0;
        isPlayerDead = 0;
        SetHashes();
    }

    public void SpawnGameEssentials()
    {

        if (Data.instance.isPlayerMasterClient == true)
        {
            randomObjectSpawn.SpawnGE();
            Debug.Log("It's a master client");
        }
        else
        {
            randomObjectSpawn.SpawnGE();
            Debug.Log("It's not a master client");
        }

    }

    public void DestroyGameEssentials()
    {

        if (Data.instance.isPlayerMasterClient == true)
        {
            randomObjectSpawn.DestroyGE();
            Debug.Log("It's a master client");
        }
        else
        {
            Debug.Log("It's not a master client");
        }
    }

    public void LevelStart()
    {
        LevelTimeAmount -= Time.deltaTime;
        LevelTimer.text = "Level Starts in : " + LevelTimeAmount.ToString("F0");

        if (LevelTimeAmount <= 0)
        {
            b_Level = false;
            if (GS == true)
            {
                LevelTimer.text = "Level Started!!!";
                SpawnGameEssentials();
                SpawnPlayer();
                InvokeRepeating(nameof(IsPlayerDeadCounts), 5f, 5f);
                
                GS = false;
            }
            else
            {
                SpawnGameEssentials();
                EnableRespawn();
                InvokeRepeating(nameof(IsPlayerDeadCounts), 5f, 5f);

            }
        }
    }

    public void EnableLevel()
    {
        if(LevelCount == maxLevels)
        {
            CancelInvoke("IsPlayerDeadCounts");
            DestroyGameEssentials();
            GameOver();
            //Game Over
        }
        else
        {
            CancelInvoke("IsPlayerDeadCounts");
            DestroyGameEssentials();
            LevelUI.SetActive(true);
            LevelTimeAmount = 5;
            b_Level = true;
            LevelCount += 1;
            LevelCountText.text = "Level : " + LevelCount;
        }
    }

    //GameOver
    public void GameOver()
    {
        LocalPlayer.GetComponent<HealthController>().DisableInputs();
        LeaderboardUI.SetActive(true);
        GameOverUI.SetActive(true);
        leaderboard.PlayerWins();
    }

    //Player Wins
    public void IsPlayerWins(string player)
    {
        PlayerWintext.text = player + " WINS THE GAME!!";
    }

    public void GRemoveSO(GameObject gameObject)
    {
        randomObjectSpawn.RemoveSO(gameObject);
    }
    public void GRemoveWO(GameObject gameObject)
    {
        randomObjectSpawn.RemoveWO(gameObject);
    }
    public void GRemoveOO(GameObject gameObject)
    {
        randomObjectSpawn.RemoveOO(gameObject);
    }

    public void GAddWO(GameObject gameObject)
    {
        randomObjectSpawn.AddWO(gameObject);
    }
    public void GAddOO(GameObject gameObject)
    {
        randomObjectSpawn.AddOO(gameObject);
    }





    public void IsPlayerDeadCounts()
    {
        var sortedPlayerList = (from player in PhotonNetwork.PlayerList orderby player.UserId descending select player).ToList();

        int i = 0;
        
        foreach (var player in sortedPlayerList)
        {
            if (player.CustomProperties["IsplayerDead"] != null)
            {
                x[i] = player.CustomProperties["IsplayerDead"].ToString();
                if (x[i] == "1")
                {
                    d++;
                    Debug.Log(d + "total player died");
                }
                Debug.Log(x[i] + player.NickName + i);
            }
            else
            {
                x[i] = "0";
            }
            i++;
        }

        if(d == 3)
        {
            //next level load
            //cancel invoke
            d = 0;
            EnableLevel();
        }
        else
        {
            d= 0;
        }
        
    }

    

    public void UpdateScore()
    {
        GemsCount += 10;
        ScoreText.text = GemsCount.ToString();
        SetHashes();
    }

    public void UpdateHealth(float x)
    {
        x = x * 100;
        LocalPlayer.GetComponent<BansheePlayer>().playerProfileData.health = x;
        HealthText.text = x.ToString();
    }
    public void UpdateKillCount()
    {
        KillCount += 1;
        LocalPlayer.GetComponent<BansheePlayer>().playerProfileData.kills = KillCount;
        SetHashes();
        KillCountText.text = KillCount.ToString();
    }

    public void UpdateYouKilledFeedText(string name)
    {
        KillFeedtext.text = "You Killed: " + name;
        KillFeedtext.color = Color.green;
    }
    public void UpdateDeathCount()
    {
        Deathcount += 1;
        isPlayerDead = 1;
        LocalPlayer.GetComponent<BansheePlayer>().playerProfileData.deaths = Deathcount;
        LocalPlayer.GetComponent<BansheePlayer>().playerProfileData.isDead = true;
        SetHashes();
    }
    public void UpdateYouGotKilledFeedText(string name)
    {
        KillFeedtext.text = "You Got Killed By: " + name;
        KillFeedtext.color = Color.red;
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel(0);
    }

    public void SetHashes()
    {
        try
        {
            Hashtable Hash = PhotonNetwork.LocalPlayer.CustomProperties;
            Hash["Kills"] = KillCount;
            Hash["Deaths"] = Deathcount;
            Hash["Gems"] = GemsCount;
            Hash["IsplayerDead"] = isPlayerDead;
            PhotonNetwork.LocalPlayer.SetCustomProperties(Hash);
        }
        catch
        {
            //Do Nothing
        }
    }

}
