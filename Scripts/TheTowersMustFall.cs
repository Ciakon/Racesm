using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheTowersMustFall : MonoBehaviour
{
    Animator animator;
    KeyCode hitTowersButton = KeyCode.L;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(hitTowersButton))
            animator.SetBool("Fall", true);
    }
}
