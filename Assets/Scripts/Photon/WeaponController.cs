using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public List<WeaponData> weapons = new List<WeaponData>();
    
    public int weaponId;
    public float fireRate = 10f;
    
    public GameObject[] WO;
    private GameObject WeaponObject;
    private GameObject bulletPrefab;
    private Transform bulletSpawnPoint;
    private float nextFireTime;

    public bool isGunEquipped = false;
    public bool isSwordEquipped = false;
    private PhotonView photonView;
    private float BulletDamage = 0f;


    private bool isGunDetect = false;
    private bool isSwordDetect = false;
    private bool isWeaponHeld = false;
    private GameObject DestroyweaponObj;
    private GameObject DestroyObj;
    private string WeaponName;
    private GameObject SpawnweaponObj;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        photonView.RPC("DisableWeapons", RpcTarget.AllBuffered);
        if (photonView.IsMine)
        {
            //if (weapons.Count != 0)
            //{
            //    foreach (var weapon in weapons)
            //    {
            //        weapon.Weapon.gameObject.SetActive(false);
            //    }
            //}

            WO = new GameObject[Gamemanager.instance.groundweapons.Length];
            
            for (int i = 0; i < Gamemanager.instance.groundweapons.Length; i++)
            {
                WO[i] = Gamemanager.instance.groundweapons[i];
                Debug.Log(WO[i].name + " : WObj");
            }
           
        }
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void WeaponSpawnObj(string name)
    {
        foreach (var weapon in WO)
        {
            if(weapon.name == name)
            {
                SpawnweaponObj = weapon;
            }
        }

    }

    [PunRPC]
    public void DisableWeapons()
    {
        foreach (var weapon in weapons)
        {
            weapon.Weapon.gameObject.SetActive(false);
            weapon.isWeaponEquipped = false;
           
            isGunEquipped = false;
            isSwordEquipped = false;
            BulletDamage = 0f;
        }
    }

    [PunRPC]
    public void EnableGunWeapon(int id)
    {
        foreach (var weapon in weapons)
        {
            if(weapon.ID == id)
            {
                //photonView.RPC("DisableWeapons", RpcTarget.AllBuffered);
                weapon.Weapon.gameObject.SetActive(true);
                weapon.isWeaponEquipped = true;
                weapon.isSwordEquipped = false;
                WeaponObject = weapon.Weapon;

                isGunEquipped = weapon.isWeaponEquipped;
                isSwordEquipped = weapon.isSwordEquipped;

                bulletPrefab = weapon.BulletPrefab;
                bulletSpawnPoint = weapon.BulletSpawnPoint;
                BulletDamage = weapon.BulletDamage;
                
               // Debug.Log("Weapon equipped: " + weapon.WeaponTag);
            }
        }
    }

    [PunRPC]
    public void EnableSword(int id)
    {
        foreach (var weapon in weapons)
        {
            if (weapon.ID == id)
            {
                //photonView.RPC("DisableWeapons", RpcTarget.AllBuffered);
                weapon.Weapon.gameObject.SetActive(true);
                weapon.isWeaponEquipped = false;
                weapon.isSwordEquipped = true;
                
                isGunEquipped = weapon.isWeaponEquipped;
                isSwordEquipped= weapon.isSwordEquipped;
                weapon.Weapon.gameObject.GetComponent<Sword>().UpdateDamage(weapon.SwordDamage);
                weapon.Weapon.GetComponent<Sword>().LocalPlayerObj = this.gameObject;

                // Debug.Log("Weapon equipped: " + weapon.WeaponTag);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
            if (isWeaponHeld)
            {
                return;
            }
            foreach (var weapon in weapons)
            {
                if (collision.gameObject.tag == weapon.WeaponTag)
                {
                    if (weapon.WeaponTag == "Sword")
                    {
                        weaponId = weapon.ID;
                        isSwordDetect = true;
                        isGunDetect = false;
                        DestroyweaponObj = collision.gameObject;
                        WeaponName = collision.gameObject.name;
                        WeaponSpawnObj(collision.gameObject.name);
                    }
                    else
                    {
                        weaponId = weapon.ID;
                        isGunDetect = true;
                        isSwordDetect = false;
                        DestroyweaponObj = collision.gameObject;
                        WeaponName = collision.gameObject.name;
                        WeaponSpawnObj(collision.gameObject.name);
                    }
                }
            }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        foreach (var weapon in weapons)
        {
            if (collision.gameObject.tag == weapon.WeaponTag)
            {
                isGunDetect = false;
                isSwordDetect = false;
                DestroyweaponObj = null;
                weaponId = 0;
                DestroyweaponObj = null;
            }
        }
    }

    public void EquipWeapon()
    {
        if(isWeaponHeld == false)
        {
            if (isGunDetect == true && isSwordDetect == false)
            {
                isWeaponHeld = true;
                //EnableGunWeapon(weaponId);
                Debug.Log(weaponId + "wid");
                photonView.RPC("EnableGunWeapon", RpcTarget.AllBuffered, weaponId);
                WeaponEquippedbyPlayer();
                Gamemanager.instance.SetPlayerState(2);
                //photonView.RPC("WeaponEquippedbyPlayer", RpcTarget.AllBuffered);
            }
            else if (isGunDetect == false && isSwordDetect == true)
            {
                isWeaponHeld = true;
                //EnableSword(weaponId);
                Debug.Log(weaponId + "wid");
                photonView.RPC("EnableSword", RpcTarget.AllBuffered, weaponId);
                WeaponEquippedbyPlayer();
                Gamemanager.instance.SetPlayerState(3);
                //photonView.RPC("WeaponEquippedbyPlayer", RpcTarget.AllBuffered);
            }
            else
            {
                Debug.Log("No Gun Detected");
            }
        }
        else
        {
            isGunDetect = false;
            isSwordDetect = false;
            isWeaponHeld =false;
            DestroyweaponObj = null;
            // DisableWeapons();
            photonView.RPC("DisableWeapons", RpcTarget.AllBuffered);
            Gamemanager.instance.SetPlayerState(1);
            //photonView.RPC("WeaponDroppedbyPlayer", RpcTarget.AllBuffered);
            WeaponDroppedbyPlayer();
        }
    }

    
    public void WeaponEquippedbyPlayer()
    {
        
        if(PhotonNetwork.IsMasterClient)
        {
            Gamemanager.instance.GRemoveWO(DestroyweaponObj);
            Gamemanager.instance.GRemoveOO(DestroyweaponObj);
            PhotonNetwork.Destroy(DestroyweaponObj);
        }
        else
        {
            int viewId = DestroyweaponObj.GetComponent<PhotonView>().ViewID;
            Debug.Log("ViewID of weapon: " + viewId);
            photonView.RPC("RPC_ForceMasterClientWeapon", RpcTarget.MasterClient, viewId);
        }
        Debug.Log("Weapon Destroyed and synced");
    }

    [PunRPC]
    void RPC_ForceMasterClientWeapon(int viewID)
    {
        PhotonNetwork.Destroy(PhotonView.Find(viewID).gameObject);
        
    }

    public void WeaponDroppedbyPlayer()
    {
        Vector3 PlayerPos = this.transform.position;
        PlayerPos.x += 1f;
        PlayerPos.y += 3f;
        if(SpawnweaponObj != null)
        {
            GameObject Weapon = PhotonNetwork.Instantiate(SpawnweaponObj.name, PlayerPos, Quaternion.identity);
            Gamemanager.instance.GAddWO(Weapon);
            Gamemanager.instance.GAddOO(Weapon);
            Weapon.transform.SetParent(Gamemanager.instance.ObjectContainer, false);
            Weapon.SetActive(true);
            Debug.Log("Weapon spawned and synced: " + SpawnweaponObj.name);
        }
    }

    public Vector2 GetGunDirection(Transform referenceTransform)
    {
        
        if (photonView.IsMine && isGunEquipped)
        {
            Vector3 direction = referenceTransform.position - WeaponObject.transform.position;
            return direction.normalized;
            // return nearGun.transform.right;
        }
        Vector3 dir = referenceTransform.position - WeaponObject.transform.position;
        return dir.normalized;
       // return Vector2.zero;
       
    }

    public void Shoot(bool isFacingRight)
    {
        fireRate = 10f;
        if (Time.time >= nextFireTime && isGunEquipped == true && isSwordEquipped == false)
        {
            if(bulletSpawnPoint!= null)
            {
                GameObject bullet = PhotonNetwork.Instantiate(bulletPrefab.name, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
                bullet.GetComponent<BulletFire>().UpdateDamage(BulletDamage);
                bullet.GetComponent<BulletFire>().LocalPlayerObj = this.gameObject;

                if(isFacingRight == true)
                {
                    bullet.GetComponent<PhotonView>().RPC("ChangeDirection", RpcTarget.AllBuffered);
                }
                bullet.transform.parent = null;
                nextFireTime = Time.time + 1f / fireRate;
                //Debug.Log("firieng and rate: " + fireRate + "and :" + nextFireTime);
            }
        }

        if(isGunEquipped == false && isSwordEquipped == true)
        {
            //sword Attack
        }
    }

}
