using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    
    [Range(0.1f, 25f)]
    public float timeScale = 1;

    public float BestSoFar = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Time.timeScale = timeScale;

    }

    public void UpdateBestSoFar(float fitness)
    {
        if(fitness > BestSoFar)
        {
            BestSoFar = fitness;
        }
    }
        
    
}
