using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagement : MonoBehaviour
{
    [SerializeField] Vector3 _gravity = default;
    public Vector3 Gravity => _gravity;

    [Header("General")]
    public static GameManagement instance;
    public Transform player;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        } else
        {
            instance = this;
        }
    }
}
