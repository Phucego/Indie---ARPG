using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
public class BreakableProps : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject drops;
    
    
    public void DestroyObject()
    {
        Destroy(gameObject);

        Instantiate(drops, transform.position, Quaternion.identity);
    }

}
