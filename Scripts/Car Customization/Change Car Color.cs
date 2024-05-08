using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChangeCarColor : MonoBehaviour
{
    public GameObject[] materialParts;
    public Material[] defaultColors;
    // Start is called before the first frame update

    private void OnEnable() {

        for (int i = 0; i < materialParts.Count(); i++)
        {
            GameObject part = materialParts[i];
            Material color = defaultColors[i];
            MeshRenderer renderer = part.GetComponent<MeshRenderer>();

            renderer.material = color;

        }
    }

    public void ChangeColor(Material color)
    {
        foreach (GameObject part in materialParts)
        {
            MeshRenderer renderer = part.GetComponent<MeshRenderer>();
            renderer.material = color;
        }
    }
}
