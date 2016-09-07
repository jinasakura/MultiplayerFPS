using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent(typeof(PlayerSetup))]
public class Player : NetworkBehaviour
{

    [SyncVar]
    private bool _isDead = false;
    public bool isDead
    {
        get { return _isDead; }
        protected set { _isDead = value; }
    }

    [SerializeField]
    private int maxHealth = 100;

    [SyncVar]
    private int currentHealth;

    [SerializeField]
    private Behaviour[] disableOnDead;
    private bool[] wasEnabled;

    [SerializeField]
    private GameObject[] disableGameObjectOnDead;

    [SerializeField]
    private GameObject deathEffect;

    [SerializeField]
    private GameObject spawnEffect;

    private bool firstSetup = true;

    public void SetupPlayer()
    {
        //switch camera
        GameManager.instance.setSceneCameraActive(false);
        GetComponent<PlayerSetup>().playerUIInstance.SetActive(true);

        CmdBroadCastNewPlayerSetup();
    }

    [Command]
    private void CmdBroadCastNewPlayerSetup()
    {
        RpcSetupPlayerOnAllClient();
    }

    [ClientRpc]
    private void RpcSetupPlayerOnAllClient()
    {
        if (firstSetup)
        {
            wasEnabled = new bool[disableOnDead.Length];
            for (int i = 0; i < wasEnabled.Length; i++)
            {
                wasEnabled[i] = disableOnDead[i].enabled;
            }

            firstSetup = false;
        }

        setDefault();

    }

    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            RpcTakeDamage(10000);
        }
    }

    [ClientRpc]
    public void RpcTakeDamage(int _amount)
    {
        if (isDead) return;

        currentHealth -= _amount;

        Debug.Log(transform.name + " now has " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        //Disable component
        for (int i = 0; i < disableOnDead.Length; i++)
        {
            disableOnDead[i].enabled = false;
        }

        for (int i = 0; i < disableGameObjectOnDead.Length; i++)
        {
            disableGameObjectOnDead[i].SetActive(false);
        }

        //Disable the collider
        Collider _col = GetComponent<Collider>();
        if (_col != null)
        {
            _col.enabled = false;
        }

        //create a death effect
        GameObject _gfxIns = (GameObject)Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(_gfxIns, 3f);

        if (isLocalPlayer)
        {
            GameManager.instance.setSceneCameraActive(true);
            GetComponent<PlayerSetup>().playerUIInstance.SetActive(false);
        }

        Debug.Log(transform.name + " is Dead!");

        //Call respawn method
        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(GameManager.instance.matchSettings.respawnTime);

        Transform _spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = _spawnPoint.position;
        transform.rotation = _spawnPoint.rotation;

        yield return new WaitForSeconds(0.01f);//不太明白为什么要加

        SetupPlayer();

        Debug.Log(transform.name + " respawn!");
    }

    public void setDefault()
    {
        isDead = false;

        currentHealth = maxHealth;

        for (int i = 0; i < disableOnDead.Length; i++)
        {
            disableOnDead[i].enabled = wasEnabled[i];
        }

        for (int i = 0; i < disableGameObjectOnDead.Length; i++)
        {
            disableGameObjectOnDead[i].SetActive(true);
        }

        Collider _col = GetComponent<Collider>();
        if (_col != null)
        {
            _col.enabled = true;
        }



        GameObject _gfxIns = (GameObject)Instantiate(spawnEffect, transform.position, Quaternion.identity);
        Destroy(_gfxIns, 3f);
    }
}
