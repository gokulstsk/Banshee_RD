using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;
using TMPro;
using Photon.Pun.UtilityScripts;
using UnityEngine.SocialPlatforms.Impl;


public class BansheePlayer : MonoBehaviourPun
{
    public GameObject PlayerCam;
    public GameObject PlayerCanvas;
    //public GameObject Character;

    private bool isfacingRight = false;
   
    [HideInInspector] public float HorizontalInput;
    [HideInInspector] public bool Jump;
    [HideInInspector] public bool JumpHeld;
    [HideInInspector] public bool Attack;
    [HideInInspector] public bool InputChanged;
    private PlayerMovementController playerMovementController;

    public WeaponController gunEquipController;
    public HealthController healthController;

    public bool DisableInputs = false;

    public Animator anim;

    public PlayerProfileData playerProfileData;

    private GameObject coin;

    public TextMeshProUGUI PlayerNameText;
    public string PlayerName;
    public GameObject PlayerObj;

    public bool isGunEquipped =  false;
    public bool isIdle = true;
    public bool isSwordEquipped =  false;
    private void Awake()
    {
        if (photonView.IsMine)
        {
            Gamemanager.instance.LocalPlayer = this.gameObject;
           // Gamemanager.instance.SetPlayerState(1);
            PlayerCam.SetActive(true);
            PlayerNameText.text = PhotonNetwork.NickName;
            PlayerName = PhotonNetwork.NickName;
            playerProfileData.username = PlayerName;
            PlayerNameText.color = Color.white;
            PlayerObj = this.gameObject;
        }
        else
        {
            PlayerNameText.text = photonView.Owner.NickName;
            PlayerNameText.color = Color.red;
        }
    }



    private void Start()
    {
        gunEquipController = GetComponent<WeaponController>();
        playerMovementController = GetComponent<PlayerMovementController>();
        healthController = GetComponent<HealthController>();
    }

   

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine && DisableInputs == false)
        {
            PlayerMovementControl();
            GunEquipControl();
            Shoot();
            // CheckInputs(); 
        }
    }

    public void SetPlayerAnimator()
    {
        playerMovementController.SetAnimator();
    }

    //Player Movement
    private void PlayerMovementControl()
    {

        // Get the current input states.
        var horizontalInput = Input.GetAxisRaw("Horizontal");
        var attack = Input.GetButtonDown("Fire1");

        // Set a boolean (true/false) value to indicate if any input state has changed since the last frame.
        InputChanged = (horizontalInput != HorizontalInput || attack != Attack);

        // Cache the new input states in public variables that can be read elsewhere.
        HorizontalInput = horizontalInput;
        Attack = attack;

        if(Input.GetKeyDown(KeyCode.Space))
        {
            Jump = true;
            JumpHeld = true;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            Jump = false;
            JumpHeld = false;
        }
       
            // Set inputs on Player Controllers.
        playerMovementController.SetHorizontalMovement(HorizontalInput);
        playerMovementController.SetJump(Jump);
        playerMovementController.SetJumpHeld(JumpHeld);

        photonView.RPC("Flip", RpcTarget.AllBuffered);
        
    }

    [PunRPC]
    private void Flip()
    {
        if (isfacingRight && HorizontalInput < 0f || !isfacingRight && HorizontalInput > 0f)
        {
            isfacingRight = !isfacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;

            Vector3 CanvasLocalScale = PlayerCanvas.transform.localScale;
            CanvasLocalScale.x = 2f;
            PlayerCanvas.transform.localScale = CanvasLocalScale;

            if(transform.localScale.x < 0f)
            {
                CanvasLocalScale.x *= -1f;
                PlayerCanvas.transform.localScale = CanvasLocalScale;
            }
        }
    }

    //Gun Equip
    private void GunEquipControl()
    {
        if (Input.GetKeyDown(KeyCode.E) )
        {
            gunEquipController.EquipWeapon();
        }
    }

    //Shoot
    private void Shoot()
    {
        playerMovementController.Shoot();
        if (Input.GetKeyDown(KeyCode.F))
        {
            gunEquipController.Shoot(isfacingRight);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Coin")
        {
            ScoreUpdate();
            coin = collision.gameObject;
            PhotonView photonView = PhotonView.Get(this);
            if (PhotonNetwork.IsMasterClient)
            {
                GemCollected(coin);
            }
            else
            {
                int viewID = collision.GetComponent<PhotonView>().ViewID;
                Debug.Log("ViewID of gem: " + viewID);
                photonView.RPC("RPC_GemCollected", RpcTarget.MasterClient, viewID);

            }
        }

        //if(collision.gameObject.tag == "Shield")
        //{
        //    coin = collision.gameObject;
        //    PhotonView photonView = PhotonView.Get(this);
        //    healthController.photonView.RPC("ShieldHealth", RpcTarget.AllBuffered);
        //    photonView.RPC("RPC_GemCollected", RpcTarget.MasterClient);
        //}
    }

    public void GemCollected(GameObject Gem)
    {
        Gamemanager.instance.GRemoveSO(Gem);
        Gamemanager.instance.GRemoveOO(Gem);
        PhotonNetwork.Destroy(Gem);
    }

    [PunRPC]
    public void RPC_GemCollected(int viewId)
    {
        if(PhotonView.Find(viewId).gameObject != null)
        {
            PhotonNetwork.Destroy(PhotonView.Find(viewId).gameObject);
        }
        
        //Debug.Log("Coin Destroyed and synced");
    }

    public void ScoreUpdate()
    {
        if (photonView.IsMine)
        {
            Gamemanager.instance.UpdateScore();
        }
    }

    


   


}
