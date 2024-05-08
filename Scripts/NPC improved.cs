using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCimproved : MonoBehaviour
{
    public Animator animator;
    public GameObject npcPrefab;
    public AudioSource audio;
    private bool walking = true;
    Vector3 rotation;

    private void Start() {
        rotation = transform.right;
    }


    void Update()
    {
        // if (walking)
        // {
        //     npcPrefab.transform.position += new Vector3(0,0,0.005f);
        //     npcPrefab.transform.position += transform.forward * Time.deltaTime;
        //     npcPrefab.transform.position = new Vector3(npcPrefab.transform.position.x, 0, npcPrefab.transform.position.z);
        // }

        //animator.SetBool("IsTurning", false);
        
    }

    private void FixedUpdate() {
        if (walking)
        {
            npcPrefab.transform.position += rotation * 0.07f;
            npcPrefab.transform.position = new Vector3(npcPrefab.transform.position.x, 0, npcPrefab.transform.position.z);
        }
        
    }


    // Disabels the animation controller, to simulate getting hit by car
    void OnCollisionEnter(Collision col) {
        if (col.gameObject.tag == "Player")
        {
            if (walking)
            {
                audio.Play();
                animator.runtimeAnimatorController = Resources.Load("m_Controller") as RuntimeAnimatorController;
                walking = false;
            }
        }
        if (col.gameObject.tag == "Wall")
        {
            rotation = Quaternion.AngleAxis(120, Vector3.up) * rotation;
            npcPrefab.transform.eulerAngles += new Vector3(0,120,0);
            
        }
    }
}
