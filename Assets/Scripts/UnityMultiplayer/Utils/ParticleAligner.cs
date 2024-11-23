using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(ParticleSystem))]

public class ParticleAligner : MonoBehaviour
{
    private ParticleSystem.MainModule _psMain;
    private void Start()
    {
        _psMain = GetComponent<ParticleSystem>().main;
    }

    private void Update()
    {
        //StartRotation is in radians and rotation.eulerAngles is in degrees 
        //Thats why we make the conversion from degrees to radians
        _psMain.startRotation = -transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
    }
}
