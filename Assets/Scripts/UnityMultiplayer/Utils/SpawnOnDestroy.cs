using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOnDestroy : MonoBehaviour
{
    [SerializeField] private GameObject _prefab;
    private void OnDestroy()
    {
        if(!gameObject.scene.isLoaded) { return; }
        Instantiate(_prefab, transform.position, Quaternion.identity);
    }
}
