using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator animator;
    bool IsE;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
         if (Input.GetKeyDown(KeyCode.E))
        {
            IsE=true;
        }

        if(IsE)
        {OpenDoor();}
    }
    void OpenDoor()
    {
     animator.Play("Door_Animation");
    }
}

