using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTriggers : MonoBehaviour
{
    private AudioManager _audioManager;


    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("Torch"))
        {
           // AudioManager.Instance.PlaySoundEffect("Torch_SFX");
        }
    }
}
