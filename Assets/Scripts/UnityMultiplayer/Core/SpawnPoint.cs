using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    private static List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();

    private void OnEnable()
    {
        _spawnPoints.Add(this);
    }
    private void OnDisable()
    {
        _spawnPoints.Remove(this);
    }
    public static Vector3 GetRandomSpawnPosition()
    {
        if (_spawnPoints.Count == 0) return Vector3.zero;

        int rand = Random.Range(0, _spawnPoints.Count);
        return _spawnPoints[rand].transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 1);
    }
}
