using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeTime : MonoBehaviour
{
    [SerializeField] private float _lifeTime = 1f;
    private void Start() 
    {
        Destroy(gameObject, _lifeTime);
    }
}
