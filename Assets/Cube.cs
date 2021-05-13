using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    [SerializeField] private Material barrierMaterial;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material targetMaterial;
    [SerializeField] private Material startMaterial;
    
    public TileType type;
    // Start is called before the first frame update
    void Start()
    {
        type = TileType.normal;
    }

    private void OnMouseDown()
    {
        if (GlobalVariables.tiles == 0)
        {
            type = TileType.start;
            gameObject.GetComponent<Renderer>().material = startMaterial;
            GlobalVariables.tiles++;
        }else if (GlobalVariables.tiles ==1)
        {
            type = TileType.target;
            gameObject.GetComponent<Renderer>().material = targetMaterial;
            GlobalVariables.tiles++;
        }
        if (type == TileType.normal)
        {
            type = TileType.barrier;
            gameObject.GetComponent<Renderer>().material = barrierMaterial;
        }
        else if (type == TileType.barrier)
        {
            type = TileType.normal;
            gameObject.GetComponent<Renderer>().material = normalMaterial;
        }
    }
}
