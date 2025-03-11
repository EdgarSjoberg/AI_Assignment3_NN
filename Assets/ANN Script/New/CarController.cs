using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class CarController : MonoBehaviour
{
    private Vector3 startPosition, startRotation;

    NeuralNetwork network;

    [Range(-1f, 1f)]
    public float acceleration, turning;

    public float timeSinceStart;

    [Header("Car Movement")]
    public float speedMultiplier = 11.4f;
    public float smoothness = 0.02f;

    [Header("Network Options")]
    public int layers = 1;
    public int neurons = 10;


    [Header("Fitness")]
    public float overallFitness;
    public float distanceMultiplier = 1.4f; //how important the fitness is to the fitness function
    public float averageSpeedMultiplier = 0.2f; //how important the speed is
    public float sensorMultiplier = 0.1f; //how important the sensors are

    private Vector3 lastPosition;
    private float totalDistanceTravelled;
    private float averageSpeed;

    [Header("Sensors")]
    private float aSensor, bSensor, cSensor;

    [Header("Win/Lose Condition")]
    public float winCondition = 1000f; 
    public float loseConditon = 40f;
    public float loseTime = 20f;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;

        network = GetComponent<NeuralNetwork>();

        //network.Initialize(layers, neurons);

    }

    //reset when the next car will start
    public void Reset() 
    {
        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        averageSpeed = 0f;
        lastPosition = startPosition;
        overallFitness = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;



        //network.Initialize(layers, neurons); //randomizes the weights to try again

    }


    //resets the car when it hits a wall
    public void OnCollisionEnter(Collision collision)
    {
        Death();
    }

    
    private void FixedUpdate()
    {
        InputSensors();
        lastPosition = transform.position;

        (acceleration, turning) = network.RunNetwork(aSensor, bSensor, cSensor); //runs the network to get the two outputs, which are the acceleration and steering

        MoveCar(acceleration, turning);

        timeSinceStart += Time.deltaTime;
        CalculateFitness();
    }
    private void CalculateFitness()
    {
        totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition); //find distance traveled since last calculation
        averageSpeed = totalDistanceTravelled / timeSinceStart;

        overallFitness = 
            (totalDistanceTravelled * distanceMultiplier)
            + (averageSpeed * averageSpeedMultiplier) 
            + (((aSensor + bSensor + cSensor)/3) * sensorMultiplier); //calculate the fitness based off of the values we care about, and their multipliers to weigh the importance

        if (timeSinceStart > loseTime && overallFitness < loseConditon) //kill the car if it is not doing well after 20 seconds
        {
            Death();
        }

        if(overallFitness >= winCondition) //win condition
        { 
            Death();
        }
    }


    //3 raycasts to detect the distance to the walls infront of it
    private void InputSensors()
    {
        //create 3 raycasts directions
        Vector3 a = transform.forward + transform.right;
        Vector3 b = transform.forward;
        Vector3 c = transform.forward - transform.right;

        Ray r = new Ray(transform.position, a);
        RaycastHit hit;

        if(Physics.Raycast(r, out hit))
        {
            Debug.DrawLine(r.origin, hit.point, Color.red);
            aSensor = hit.distance; 
        }

        r = new Ray(transform.position, b);
        if (Physics.Raycast(r, out hit))
        {
            Debug.DrawLine(r.origin, hit.point, Color.red);
            bSensor = hit.distance; 
        }

        r = new Ray(transform.position, c);

        if (Physics.Raycast(r, out hit))
        {
            Debug.DrawLine(r.origin, hit.point, Color.red);
            cSensor = hit.distance; 
        }

        Vector3 sensors = new Vector3 (aSensor, bSensor, cSensor); //normalize the values to get it between -1 and 1 for the neural network
        sensors.Normalize(); //gives better results for the neural network (my own testing)

        aSensor = sensors.x;
        print("A: " + aSensor);
        bSensor = sensors.y;
        print("B: " + bSensor);
        cSensor = sensors.z;
        print("C: " + cSensor);


    }

    private Vector3 input;
    public void MoveCar(float vertical, float horizontal)
    {
        input = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, vertical * speedMultiplier), smoothness); //lerp the vertical movement for smooth acceleration
        input = transform.TransformDirection(input); //rotate the vector to be relative to the car

        transform.position += input; //move the car

        transform.eulerAngles += new Vector3(0, horizontal * 90 * smoothness, 0); //rotate the car smoothly

    }

    public void ResetWithNetwork(NeuralNetwork net)
    {
        network = net;
        Reset();
    }

    private void Death()
    {
        GameObject.FindObjectOfType<GameManager>().UpdateBestSoFar(overallFitness);
        GameObject.FindObjectOfType<GeneticManager>().Death(overallFitness, network);
    }

}
