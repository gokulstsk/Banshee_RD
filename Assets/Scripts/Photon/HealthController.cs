using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using static UnityEngine.GraphicsBuffer;


public class HealthController : MonoBehaviour
{
    public Image fillImage;
    public float health = 1;
    public float playerHeath;

    public Rigidbody2D rb;
    public GameObject deadsprite;
    public CapsuleCollider2D collider;
    public GameObject playerCanvas;

    public BansheePlayer playerScript;

    public PhotonView photonView;
    
    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }
    public void CheckHealth()
    {
        if (photonView != null && photonView.IsMine)
        {
            Gamemanager.instance.UpdateHealth(health);
        }
        if (photonView.IsMine && health <= 0)
        {
            
            Gamemanager.instance.UpdateHealth(0f);
            Gamemanager.instance.UpdateDeathCount();
           // Gamemanager.instance.EnableRespawn();
           
            this.GetComponent<PhotonView>().RPC("death", RpcTarget.AllBuffered);
        }
    }

    public void EnableInputs()
    {
        playerScript.DisableInputs = false;
    }
    public void DisableInputs()
    {
        playerScript.DisableInputs = true;
    }


    [PunRPC]
    public void death()
    {
        playerScript.DisableInputs = true;
        rb.gravityScale = 0;
        collider.enabled = false;
        deadsprite.gameObject.SetActive(true);
        playerCanvas.SetActive(false);
    }

    [PunRPC]
    public void Revive()
    {
        health = 1;
        fillImage.fillAmount = 1f;
        rb.gravityScale = 1;
        collider.enabled = true;
        deadsprite.gameObject.SetActive(false);
        playerCanvas.SetActive(true);
    }

    [PunRPC]
    public void ShieldHealth()
    {
        health = 1;
        fillImage.fillAmount = 1f;
        CheckHealth();
    }

    [PunRPC]
    public void HealthUpdate(float damage)
    {

        fillImage.fillAmount -= damage;
        health -= damage;
        CheckHealth();
    }

    [PunRPC]
    public void YouGotKilledBy(string name)
    {
        Gamemanager.instance.UpdateYouGotKilledFeedText(name);
    }

    [PunRPC]
    public void YouKilled(string name)
    {
        Gamemanager.instance.UpdateYouKilledFeedText(name);
    }
}
