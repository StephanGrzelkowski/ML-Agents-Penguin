using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class PenguinAgent : Agent
{
    [Tooltip("How fast the agent moves forward")]
    public float moveSpeed = 5f;

    [Tooltip("Turning speed of the agent")]
    public float turnSpeed = 180f;

    [Tooltip("Heat prefab when the baby is fed")]
    public GameObject heartPrefab;

    [Tooltip("Fish regurgitated appears after baby is fed")]
    public GameObject regurgitatedFishPrefab;

    private PenguinArea penguinArea;
    private PenguinAcademy penguinAcademy;
    new private Rigidbody rigidbody;
    private GameObject baby;

    private bool isFull;
    private float feedRadius = 0f;

    /// <summary>
    /// Initialization of the agent 
    /// </summary>
    public override void InitializeAgent()
    {
        base.InitializeAgent();
        penguinArea = GetComponentInParent<PenguinArea>();
        penguinAcademy = FindObjectOfType<PenguinAcademy>();
        baby = penguinArea.penguinBaby;
        rigidbody = GetComponent<Rigidbody>(); 
    }

    /// <summary>
    /// Perform actions based on a vector of numbers 
    /// </summary>
    /// <param name="vectorAction">The list of actions to take</param>
    public override void AgentAction(float[] vectorAction)
    {
        // convert forward action into movement 
        float forwardAmount = vectorAction[0];

        //convert second action into turning 
        float turnAmount = 0f; 
        if (vectorAction[1] == 1f)
        {
            turnAmount = -1f; 
        }
        else if (vectorAction[1] == 2f)
        {
            turnAmount = 1f;
        }

        //apply the movement 
        rigidbody.MovePosition(transform.position + transform.forward * forwardAmount * moveSpeed * Time.fixedDeltaTime);
        transform.Rotate(transform.up * turnAmount * turnSpeed * Time.fixedDeltaTime);

        //negative reward so actions cost
        AddReward(-1f / agentParameters.maxStep);
    }

    /// <summary>
    /// Read keyboard inputs from the player to control the penguin. Behavior Type needs to be set to "heuristic Only" in the Behavior parameters inspector. 
    /// </summary>
    /// <returns>an vector action that is passed to <see cref="AgentAction(float[])"/></returns>
    public override float[] Heuristic()
    {
        float forwardAction = 0f;
        float turnAction = 0f;
        if (Input.GetKey(KeyCode.W))
        {
            forwardAction = 1f;
        }

        if (Input.GetKey(KeyCode.A))
        {
            turnAction = 1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {

            turnAction = 2f;
        }

        // put the actions into an array and return 
        return new float[] { forwardAction, turnAction }; 
    }

    /// <summary>
    /// reset agent and area 
    /// </summary>
    public override void AgentReset()
    {
        isFull = false;
        penguinArea.ResetArea();
        feedRadius = penguinAcademy.FeedRadius;
    }

    /// <summary>
    /// collect non-Raycast observations
    /// </summary>
    public override void CollectObservations()
    {
        //Whether the penguin has eaten the fish (1 float = 1 value)
        AddVectorObs(isFull);

        //Distance to the baby (1 float = 1 value)
        AddVectorObs(Vector3.Distance(baby.transform.position, transform.position));

        //Direction to baby (1 Vector3 = 3 values)
        AddVectorObs((baby.transform.position - transform.position).normalized);

        // Direction penguin is facing (1 vector3 =3 values)
        AddVectorObs(transform.forward); 

        // 1 + 1 + 3 + 3 = 8 total values
    }
    
    private void FixedUpdate()
    {
        //Test if the agent is close to the baby to feed it 
        if (Vector3.Distance(transform.position, baby.transform.position) < feedRadius)
        {
            // Close enough, try to feed the baby 
            RegurgitateFish();
        }
    }

   /// <summary>
   /// Take collision action
   /// </summary>
   /// <param name="collision">collision info</param>
   private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("fish"))
        {
            // Munch Munch Munch
            EatFish(collision.gameObject);
        }
        else if (collision.transform.CompareTag("baby"))
        {
            // goo goo feed the baby
            RegurgitateFish();
        }
    }

    /// <summary>
    /// If fish no fish go fish get points fo fish, yo fish
    /// </summary>
    /// <param name="fishObject"></param>
    private void EatFish(GameObject fishObject)
    {
        if (isFull) return;
        isFull = true;

        penguinArea.RemoveSpecificFish(fishObject);

        AddReward(1f);
    }

    /// <summary>
    /// Check if agent is full and then feed the baby
    /// </summary>
    private void RegurgitateFish()
    {
        if (!isFull) return;
        isFull = false;

        //spawn the fish to feed the babyt 
        GameObject regurgitatedFish = Instantiate<GameObject>(regurgitatedFishPrefab);
        regurgitatedFish.transform.parent = transform.parent;
        regurgitatedFish.transform.position = baby.transform.position;
        Destroy(regurgitatedFish, 4f);

        //Spawn a hearty so happy
        GameObject heart = Instantiate<GameObject>(heartPrefab);
        heart.transform.parent = transform.parent;
        heart.transform.position = baby.transform.position + Vector3.up;
        Destroy(heart, 4f);

        AddReward(1f); 

        if (penguinArea.FishRemaining <= 0)
        {
            Done();
        }
    }
}

