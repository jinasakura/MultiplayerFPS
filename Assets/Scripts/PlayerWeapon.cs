using UnityEngine;
using System.Collections;

[System.Serializable]
public class PlayerWeapon{

    public string name = "Glock";

    public int damage = 10;
    public float range = 100f;

    public float fireRate = 0f;

    //武器模型
    public GameObject graphics;
}
