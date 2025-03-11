using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using System;
using Random = UnityEngine.Random;
using MathNet.Numerics.Integration;




/*
 * neural network class
 * takes in 3 inputs (the 3 raycasts)
 * outputs 2 values (acceleration and steering)
 * 
 * 
 * 
 * To be able to multiply two matrices, the number of columns of the first matrix must be equal to the number of rows of the second matrix.
 * 
 * The input matrix has 3 columns, so the first weight matrix must have 3 rows.
 * 
 * 
*/

public class NeuralNetwork : MonoBehaviour
{
    public Matrix<float> inputLayer = Matrix<float>.Build.Dense(1, 3); //input layer has 1 rows and 3 columns

    public List<Matrix<float>> hiddenLayers = new List<Matrix<float>>(); 

    public Matrix<float> outputLayer = Matrix<float>.Build.Dense(1, 2); //output layer has 1 row and 2 columns

    public List<Matrix<float>> weights = new List<Matrix<float>>();

    public List<float> biases = new List<float>();

    public float fitness;

    public void Initialize(int hiddenLayerCount, int hiddenNeuronCount)
    {
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();
        weights.Clear();
        biases.Clear();

        for (int i = 0; i < hiddenLayerCount + 1; i++)
        {

            Matrix<float> f = Matrix<float>.Build.Dense(1, hiddenNeuronCount); //creates a specified number of hidden layers with a specified number of neurons

            hiddenLayers.Add(f); //adds the hidden layers to the list

            biases.Add(Random.Range(-1f, 1f)); //randomises biases of each neuron

            //WEIGHTS
            if (i == 0)
            {
                //creates the weights between the input layer and the first hidden layer which must have 3 rows because of the 3 inputs
                Matrix<float> inputToHiddenLayer1 = Matrix<float>.Build.Dense(3, hiddenNeuronCount); 
                weights.Add(inputToHiddenLayer1);
            }

            //creates the weights between the hidden layers, based off of the amount of neurons in the hidden layers, aka the amount of columns in the previous layer
            Matrix<float> hiddenToHiddenLayer = Matrix<float>.Build.Dense(hiddenNeuronCount, hiddenNeuronCount);
            weights.Add(hiddenToHiddenLayer);

        }

        //creates the weights and biases for the output layer
        Matrix<float> outputWeight = Matrix<float>.Build.Dense(hiddenNeuronCount, 2);
        weights.Add(outputWeight);
        biases.Add(Random.Range(-1f, 1f));

        RandomizeWeights();

    }


    //goes through the weights list, which has the matrixes of the weights between the layers and randomizes each value
    public void RandomizeWeights()
    {

        for (int i = 0; i < weights.Count; i++)
        {

            for (int x = 0; x < weights[i].RowCount; x++)
            {

                for (int y = 0; y < weights[i].ColumnCount; y++)
                {

                    weights[i][x, y] = Random.Range(-1f, 1f); 

                }
            }
        }
    }


    //runs the network with the given inputs
    //returns the output of the network
    
    public (float, float) RunNetwork(float a, float b, float c)
    {
        inputLayer[0, 0] = a;
        inputLayer[0, 1] = b;
        inputLayer[0, 2] = c;

        //tan h adjusts the value to be between -1 and 1
        inputLayer = inputLayer.PointwiseTanh();

        //multiply the input layer with the first weight matrix and add the bias, then applying tanh to adjust the value to be between -1 and 1
        hiddenLayers[0] = ((inputLayer * weights[0]) + biases[0]).PointwiseTanh();

        for (int i = 1; i < hiddenLayers.Count; i++)
        {
            hiddenLayers[i] = ((hiddenLayers[i - 1] * weights[i]) + biases[i]).PointwiseTanh();
        }

        outputLayer = ((hiddenLayers[hiddenLayers.Count - 1] * weights[weights.Count - 1]) + biases[biases.Count - 1]).PointwiseTanh();

        //sigmoid of the first output to make it always go forward (returns value between 0 and 1)
        //tanH of second value to make it between -1 and 1 so that it can turn left or right
        return (Sigmoid(outputLayer[0, 0]), (float)Math.Tanh(outputLayer[0, 1]));

    }

    //"squishes" the output to be between 0 and 1
    //works for any real number
    public float Sigmoid(float s)
    {
        return (1 / (1 + Mathf.Exp(-s)));
    }

    public NeuralNetwork InitializeCopy(int hiddenLayerCount, int hiddenNeuronCount)
    {
        {
            NeuralNetwork nn = new NeuralNetwork();

            List<Matrix<float>> newWeights = new List<Matrix<float>>();

            for (int i = 0; i < this.weights.Count; i++)
            {
                Matrix<float> currentWeight = Matrix<float>.Build.Dense(weights[i].RowCount, weights[i].ColumnCount);

                for (int x = 0; x < currentWeight.RowCount; x++)
                {
                    for (int y = 0; y < currentWeight.ColumnCount; y++)
                    {
                        currentWeight[x, y] = weights[i][x, y];
                    }
                }

                newWeights.Add(currentWeight);
            }

            List<float> newBiases = new List<float>();

            newBiases.AddRange(biases);

            nn.weights = newWeights;
            nn.biases = newBiases;

            nn.InitializeHidden(hiddenLayerCount, hiddenNeuronCount);

            return nn;
        }
    }

    private void InitializeHidden(int hiddenLayerCount, int hiddenNeuronCount)
    {
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();

        for (int i = 0; i < hiddenLayerCount + 1; i++)
        {
            Matrix<float> newHiddenLayer = Matrix<float>.Build.Dense(1, hiddenNeuronCount);
            hiddenLayers.Add(newHiddenLayer);
        }
    }
}
