using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BreakableProps : MonoBehaviour, IBreakables
{
    [SerializeField] private GameObject drops;
    public void Destroy()
    {
        Destroy(gameObject);
    }

}
