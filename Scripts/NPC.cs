using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.SideChannels;
using UnityEngine;

public class NPC : MonoBehaviour
{
    private Rigidbody[] RBchildren;
    // Start is called before the first frame update
    void Start()
    {
        
        foreach (Transform g in transform.GetComponentsInChildren<Transform>())
        {
            try
            {
                var rb = g.GetComponent<Rigidbody>();
                rb.useGravity = false;
            }
            catch (System.Exception)
            {
                continue;
            }
        }
        


        // for (int i = 0; i < RBchildren.Length; i++)
        // {
        //     RBchildren[i].useGravity = false;
        // }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        

        if (collision.gameObject.tag == "Player")
        {
            foreach (Transform g in transform.GetComponentsInChildren<Transform>())
            {
                try
                {
                    var rb = g.GetComponent<Rigidbody>();
                    rb.useGravity = true;
                    rb.isKinematic = false;
                    
                }
                catch (System.Exception)
                {
                    continue;
                }
            }
        }

        // for (int i = 0; i < RBchildren.Length; i++)
        // {
        //     RBchildren[i].useGravity = true;
        //     RBchildren[i].isKinematic = false;
            
        // }
    }
    
}
