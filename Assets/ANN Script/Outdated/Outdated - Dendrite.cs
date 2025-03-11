using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dendrite : MonoBehaviour
{
    public Signal Input { get; set; }

    public double Weight { get; set; }

    public bool Learnable { get; set; } = true; 
}
