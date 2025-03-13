using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;
using Unity.Collections.LowLevel.Unsafe;


/*
 * this genetic algorithm is used to create a population of agents, and then evolve them over time
 * it takes a certain amount of best agents that are kept, then creates new agents by crossing over the best agents
 * both the best and the new children are kept, and the rest of the population is filled with random agents
 */


public class GeneticManager : MonoBehaviour
{

    [Header("Refrences")]
    public CarController carController;

    [Header("Controls")]
    public int initialPopulation = 85;

    [Range(0.0f, 1f)]
    public float mutationRate = 0.055f;
    [Range(0.0f, 20f)]
    public int mutationRateDivider = 7;

    [Header("Crossover Controls")]
    public int bestAgentSelection = 8;
    public int worstAgentSelection = 3;
    public int numberToCrossover = 15;

    private List<int> genePool = new List<int>();

    private int naturallySelected;



    [Header("Public View")]
    public int currentGeneration;
    public int currentGenome;
    public NeuralNetwork[] population;

    private void Start()
    {
        CreatePopulation();
    }

    private void CreatePopulation()
    {
        population = new NeuralNetwork[initialPopulation];
        FillPopulationWithRandomValues(population, 0);
        ResetToCurrentGenome();
    }

    private void ResetToCurrentGenome()
    {
        carController.ResetWithNetwork(population[currentGenome]);
        
    }

    private void FillPopulationWithRandomValues(NeuralNetwork[] newPopulation, int startingIndex)
    {
        while(startingIndex < initialPopulation) //create random values for a select amount of the population (the rest will be filled with the best agents)
        {
            newPopulation[startingIndex] = new NeuralNetwork();
            newPopulation[startingIndex].Initialize(carController.layers, carController.neurons);
            startingIndex++;
        }
    }

    private void Repopulate()
    {
        genePool.Clear();
        currentGeneration++;
        naturallySelected = 0;

        SortPopulation();

        NeuralNetwork[] newPopulation = PickBestPopulation();

        Crossover(newPopulation);
        Mutate(newPopulation);

        FillPopulationWithRandomValues(newPopulation, naturallySelected); //creates random values for the rest of the population

        population = newPopulation;
        currentGenome = 0;
        ResetToCurrentGenome();
        //currentGeneration++;
    }

    public void Crossover(NeuralNetwork[] newPopulation)
    {
        for(int i = 0; i < numberToCrossover; i+=2) //crossover the best agents, which creates two new agents
        {
            int aIndex = i;
            int bindex = i + 1;
            
            if(genePool.Count >= 1)
            {
                for(int j = 0; j < 100; j++)
                {
                    aIndex = genePool[Random.Range(0, genePool.Count)];
                    bindex = genePool[Random.Range(0, genePool.Count)];

                    if(aIndex != bindex)
                    {
                        break;
                    }
                }
            }
            NeuralNetwork child1 = new NeuralNetwork();
            NeuralNetwork child2 = new NeuralNetwork();
             
            child1.Initialize(carController.layers, carController.neurons);
            child2.Initialize(carController.layers, carController.neurons);

            child1.fitness = 0;
            child2.fitness = 0;

            //simple crossover method for weights
            for (int w = 0; w < child1.weights.Count; w++)
            {
                if (Random.Range(0.0f, 1.0f) < 0.5f) //tosses a coin to decide which parent to take the weight from
                {
                    //takes one of the parents weights matrixes, and assigns it to the child for each set of weights
                    child1.weights[w] = population[aIndex].weights[w];
                    child2.weights[w] = population[bindex].weights[w];
                }
                else
                {
                    child1.weights[w] = population[bindex].weights[w];
                    child2.weights[w] = population[aIndex].weights[w];
                }
            }

            //simple crossover for biases
            for (int w = 0; w < child1.biases.Count; w++)
            {
                if (Random.Range(0.0f, 1.0f) < 0.5f) //tosses a coin to decide which parent to take the biases from
                {
                    //takes one of the parents weights matrixes, and assigns it to the child for each set of biases
                    child1.biases[w] = population[aIndex].biases[w];
                    child2.biases[w] = population[bindex].biases[w];
                }
                else
                {
                    child1.biases[w] = population[bindex].biases[w];
                    child2.biases[w] = population[aIndex].biases[w];
                }
            }

            //adds the newly created children to the new population
            newPopulation[naturallySelected] = child1;
            naturallySelected++;
            
            newPopulation[naturallySelected] = child2;
            naturallySelected++;
        }
    }

    
    public void Mutate(NeuralNetwork[] newPopulation)
    {
        for(int i = 0; i < naturallySelected; i++)
        {
            for (int j = 0; j < newPopulation[i].weights.Count; j++)
            {
                if (Random.Range(0.0f, 1.0f) < mutationRate) //only mutates at the rate set by the mutation rate
                {
                    newPopulation[i].weights[j] = MutateMatrix(newPopulation[i].weights[j]);
                }
            }
        }
    }

    Matrix<float> MutateMatrix(Matrix<float> a)
    {
        int randomPoints = Random.Range(1, (a.RowCount * a.ColumnCount) / mutationRateDivider); //randomly selects a number of points to mutate

        Matrix<float> temp = a;

        for (int i = 0; i < randomPoints; i++)
        {
            int randomRow = Random.Range(0, a.RowCount);
            int randomColumn = Random.Range(0, a.ColumnCount);

            temp[randomRow, randomColumn] = Mathf.Clamp(temp[randomRow, randomColumn] + Random.Range(-1f, 1f), -1f, 1f); //adjusts the value up or down by a small amount
        }

        return temp;
    }

    private NeuralNetwork[] PickBestPopulation()
    {
        NeuralNetwork[] newPopulation = new NeuralNetwork[initialPopulation];

        for (int i = 0; i < bestAgentSelection; i++)
        {
            newPopulation[naturallySelected] = population[i].InitializeCopy(carController.layers, carController.neurons);
            newPopulation[naturallySelected].fitness = 0;
            naturallySelected++;

            int f = Mathf.RoundToInt(population[i].fitness * 10); //multiply the fitness to make more fit agents more likely to be selected

            for (int j = 0; j < f; j++)
            {
                genePool.Add(i); //stores the index of the agent in the gene pool
            }

        }

        for (int i = 0; i < worstAgentSelection; i++)
        {
            int last = population.Length - 1;
            last -= i;
           
            int f = Mathf.RoundToInt(population[last].fitness * 10); //multiply the fitness to make more fit agents more likely to be selected

            for (int j = 0; j < f; j++)
            {
                genePool.Add(last); //stores the index of the agent in the gene pool
            }
        }

        return newPopulation;
    }
    public void Death(float fitness, NeuralNetwork network)
    {
        if (currentGenome < population.Length - 1)
        {
            population[currentGenome].fitness = fitness;
            currentGenome++;
            ResetToCurrentGenome();
        }
        else
        {
            Repopulate();
        }
    }


    //bubble sort to sort the population by fitness
    private void SortPopulation()
    {
        for ( int i = 0; i < population.Length; i++)
        {
            for (int j = i; j < population.Length; j++)
            {
                if (population[i].fitness < population[j].fitness)
                {
                    NeuralNetwork temp = population[i];
                    population[i] = population[j];
                    population[j] = temp;
                }
            }
        }
    }

    
}
