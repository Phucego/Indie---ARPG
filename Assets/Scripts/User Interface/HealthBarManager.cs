using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarManager : MonoBehaviour
{
    public Slider healthSlider;
    public Slider easeHealthSlider;
    [SerializeField]
    private GameObject player;
    
    [SerializeField] private float lerpSpeed;
    public float playerMaxHealth;
    public float playerCurrentHealth;
    
    // Start is called before the first frame update
    void Start()
    {
        
        player = GameObject.Find("Player");
        playerCurrentHealth = player.GetComponent<PlayerHealth>().currentHealth;
        playerMaxHealth = player.GetComponent<PlayerHealth>().maxHealth;
        playerCurrentHealth = playerMaxHealth;
        Debug.Log(playerMaxHealth);
        Debug.Log(playerCurrentHealth);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (healthSlider.value != playerCurrentHealth)
        {
            healthSlider.value = playerCurrentHealth;
        }

        if (healthSlider.value != easeHealthSlider.value)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, playerCurrentHealth, lerpSpeed);
        }
        
    }

}
