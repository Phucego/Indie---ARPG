using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarbarianMenu : MonoBehaviour
{
    public Animator anim;
    public static BarbarianMenu Instance;
    // Start is called before the first frame update

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
