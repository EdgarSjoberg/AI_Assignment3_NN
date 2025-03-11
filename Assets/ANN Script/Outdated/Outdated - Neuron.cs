using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neuron : MonoBehaviour
{
    public List<Dendrite> Dendrites {  get; set; }
    public Signal Output { get; set; }
    private double myWeight;

    public Neuron()
    {
        Dendrites = new List<Dendrite>();
        Output = new Signal();
    }

    public void Fire()
    {
        Output.Value = Sum();
        Output.Value = Activation(Output.Value);
        return;
    }
    public void Conpute(double learningRate, double delta)
    {
        myWeight += learningRate * delta;
        foreach(var dendrite in Dendrites)
        {
            dendrite.Weight = myWeight;
        }
        return;
    }
    private double Sum()
    {
        double sum = 0.0f;

        foreach (var incoming in Dendrites)
        {
            sum += incoming.Input.Value * incoming.Weight;
        }
        //can add bias here
        return 0f;
    }
    private double Activation(double input)
    {
        double threshold = 1;
        return input >= threshold ? input : threshold;
    }
}
