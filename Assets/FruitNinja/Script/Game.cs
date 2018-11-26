﻿using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

// change scale based on difficulty

public class Game : MonoBehaviour {


    private KinectManager kinectManager;

    // score tracking
    public static int score;
    public static int combo;
    public static bool validCombo;

    private int totalNumberOfNotes;
    private int numberOfNotesHit;
    public Text scoreText;
    public Text difficultyText;
    public Text comboText;

    // prefabs
    public GameObject notePrefab;
    public GameObject innerHitBoxPrefab;
    public GameObject outerHitBoxPrefab;
    public GameObject triggerPrefab;
    public GameObject chargePrefab;
    public GameObject deletePrefab;

    // hand controllers
    public GameObject leftHand;
    public GameObject centerHand;
    public GameObject rightHand;

    // virtual gameobjects controllers
    HitBox innerLeftHitBox;
    HitBox innerCenterHitBox;
    HitBox innerRightHitBox;
    HitBox outerLeftHitBox;
    HitBox outerCenterHitBox;
    HitBox outerRightHitBox;

    Trigger leftTrigger;
    Trigger centerTrigger;
    Trigger rightTrigger;

    Charge leftCharge;
    Charge centerCharge;
    Charge rightCharge;

    // apex points for the hit boxes, will be dependant on user
    private Vector2 hitBoxCoordinate = new Vector2(6f, 1.5f);
    private const float spawnFactor = 10f;
    private const float triggerFactor = 5f; 
    private const float chargeFactor = 12f;
    private const float deleteFactor = 2.75f;
    private const float handFactor = 7f;
    private const float maxHandDistance = 1f;
    private const float maxHandHeight = -3.5f;
    private const float minHandHeight = -10.5f;

    public Vector2[] spawnPositions; // possible spawn positions of the notes
    private float waitTime = 2f; // cooldown for notes
    private float speed = -3f; // temp value

    private bool isPaused;

    // 0 lives
    // 1 timed
    private int gameMode; // easy, intermidiate, advanced
    private bool enableSpawn;

    public enum Hand { Left, Center, Right };

    void Start () {

        kinectManager = KinectManager.Instance;

        // generates spawn points for the notes
        spawnPositions = new Vector2[3];
        spawnPositions[0] = new Vector2(-hitBoxCoordinate.x, hitBoxCoordinate.y + spawnFactor);
        spawnPositions[1] = new Vector2(0, hitBoxCoordinate.y + spawnFactor);
        spawnPositions[2] = new Vector2(hitBoxCoordinate.x, hitBoxCoordinate.y + spawnFactor);

        // generates the hitboxes
        innerLeftHitBox = createGameComponent(-hitBoxCoordinate.x, hitBoxCoordinate.y, innerHitBoxPrefab).GetComponent<HitBox>();
        innerCenterHitBox = createGameComponent(0, hitBoxCoordinate.y, innerHitBoxPrefab).GetComponent<HitBox>();
        innerRightHitBox = createGameComponent(hitBoxCoordinate.x, hitBoxCoordinate.y, innerHitBoxPrefab).GetComponent<HitBox>();

        outerLeftHitBox = createGameComponent(-hitBoxCoordinate.x, hitBoxCoordinate.y, outerHitBoxPrefab).GetComponent<HitBox>();
        outerCenterHitBox = createGameComponent(0, hitBoxCoordinate.y, outerHitBoxPrefab).GetComponent<HitBox>();
        outerRightHitBox = createGameComponent(hitBoxCoordinate.x, hitBoxCoordinate.y, outerHitBoxPrefab).GetComponent<HitBox>();

        // generates the controls
        leftTrigger= createGameComponent(-hitBoxCoordinate.x, hitBoxCoordinate.y - triggerFactor , triggerPrefab).GetComponent<Trigger>();
        centerTrigger = createGameComponent(0, hitBoxCoordinate.y - triggerFactor, triggerPrefab).GetComponent<Trigger>();
        rightTrigger = createGameComponent(hitBoxCoordinate.x, hitBoxCoordinate.y - triggerFactor, triggerPrefab).GetComponent<Trigger>(); 

        leftCharge = createGameComponent(-hitBoxCoordinate.x, hitBoxCoordinate.y - chargeFactor, chargePrefab).GetComponent<Charge>();
        centerCharge = createGameComponent(0, hitBoxCoordinate.y - chargeFactor, chargePrefab).GetComponent<Charge>();
        rightCharge = createGameComponent(hitBoxCoordinate.x, hitBoxCoordinate.y - chargeFactor, chargePrefab).GetComponent<Charge>();

        createGameComponent(0, hitBoxCoordinate.y - deleteFactor, deletePrefab);
        createGameComponent(0, -2.25f, deletePrefab);
        createGameComponent(0, -11.75f, deletePrefab);


        leftHand.transform.position = new Vector2(-hitBoxCoordinate.x, -handFactor);
        centerHand.transform.position = new Vector2(0, -handFactor);
        rightHand.transform.position = new Vector2(hitBoxCoordinate.x, -handFactor);

        enableSpawn = true;
        validCombo = true;
    }


    public IEnumerator noteTimer()
    {
        enableSpawn = false;
        yield return new WaitForSeconds(waitTime);

        if(isPaused == true){
            StartCoroutine(noteTimer());
        } else{
            enableSpawn = true;
        }
    }


    void Update()
    {
        // run code to detect the user --> detected user in 3..2..1
        // if(user can't be detected, pause the game)

        /*
         * for all values in the queue, dequeue and set velocity to zero
         * all the dequeues values to pauseQueue, when user is detected again
         * re add all the pauseQueue values into the noteQueue
         */

        // if user is detected again, game continues in 3..2..1

        // every x-y seconds, depending on difficulty, generate a note at a random location
        // different note types for different difficulties

        /*
        // moves controls vertically relative to hands
          if (kinectManager && kinectManager.IsInitialized())
            {
                if (kinectManager.IsUserDetected())
                {
                    long userId = kinectManager.GetPrimaryUserID();
                    int jointType = (int)KinectInterop.JointType.HandRight;
                    if (kinectManager.IsJointTracked(userId, jointType))
                    {
                        Vector3 rHandPosition = kinectManager.GetJointKinectPosition(userId, jointType);
                        rightHand.transform.position = new Vector3(rightHand.transform.position.x, rHandPosition.y, rightHand.transform.position.z);
                    }

                    jointType = (int)KinectInterop.JointType.HandLeft;
                    if (kinectManager.IsJointTracked(userId, jointType))
                    {
                        Vector3 lHandPosition = kinectManager.GetJointKinectPosition(userId, jointType);
                        leftHand.transform.position = new Vector3(leftHand.transform.position.x, lHandPosition.y, leftHand.transform.position.z);
                    }
                }
            }
        */

        scoreText.text = "Score: " + score;
        comboText.text = "Combo: " + combo;


        if (enableSpawn == true)
        {
            createNote();
            StartCoroutine(noteTimer());
        }

        // on trigger, can't move up anymore

        float handDifference = Mathf.Abs(leftHand.transform.position.y - rightHand.transform.position.y);
        float centerY = (leftHand.transform.position.y + rightHand.transform.position.y) / 2;
        centerHand.transform.position = new Vector2(0, centerY);


        if (handDifference >= 0 && handDifference <= maxHandDistance)
        {
            centerHand.GetComponent<SpriteRenderer>().enabled = true;
            leftTrigger.GetComponent<SpriteRenderer>().enabled = false;
            centerTrigger.GetComponent<SpriteRenderer>().enabled = true;
            rightTrigger.GetComponent<SpriteRenderer>().enabled = false;
        }
        else
        {
            centerHand.GetComponent<SpriteRenderer>().enabled = false;
            leftTrigger.GetComponent<SpriteRenderer>().enabled = true;
            centerTrigger.GetComponent<SpriteRenderer>().enabled = false;
            rightTrigger.GetComponent<SpriteRenderer>().enabled = true;
        }
    

        controlGameComponent(leftTrigger, leftCharge, innerLeftHitBox, outerLeftHitBox);
        controlGameComponent(centerTrigger, centerCharge, innerCenterHitBox, outerCenterHitBox);
        controlGameComponent(rightTrigger, rightCharge, innerRightHitBox, outerRightHitBox);

    }

    private bool validateHandHeight(GameObject hand)
    {
        if(hand.transform.position.y >= maxHandHeight || hand.transform.position.y <= minHandHeight)
        {
            return false;
        }

        return true;
    }

    private void createNote()
    {
        int hitBoxCoordinatePosition = Random.Range(0, spawnPositions.Length);
        Vector2 tempSpawnCoordinate = spawnPositions[hitBoxCoordinatePosition];

        Vector2 newNotePosition = new Vector2(tempSpawnCoordinate.x, tempSpawnCoordinate.y); // based on a random spawn point
        GameObject newNoteObject = (GameObject)Instantiate(notePrefab, newNotePosition, notePrefab.transform.rotation);

        newNoteObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0, speed);
    }


    private GameObject createGameComponent(float x, float y, GameObject prefab)
    {
        Vector2 newPosition = new Vector2(x, y);
        GameObject newPrefab = (GameObject)Instantiate(prefab, newPosition, prefab.transform.rotation);
        return newPrefab;
    }


    private void controlGameComponent(Trigger trigger, Charge charge, HitBox innerHitBox, HitBox outerHitBox)
    {
        if (trigger.getIsDetected())
        {
            // only valid if trigger is charged
            if (charge.getIsCharged())
            {
                charge.setNotCharged();
                trigger.setTriggered();

                if (innerHitBox.getNoteIsTouching() && !outerHitBox.getNoteIsTouching())
                {
                    Debug.Log("FULL");
                    score = score + 100;
                    Destroy(innerHitBox.getNoteObject());

                    if(validCombo == true)
                    {
                        combo = combo + 1;
                    }
                }
                else if (innerHitBox.getNoteIsTouching() && outerHitBox.getNoteIsTouching())
                {
                    Debug.Log("PARTIAL");
                    score = score + 50;
                    Destroy(outerHitBox.getNoteObject());

                    if (validCombo == true)
                    {
                        combo = combo + 1;
                    }
                }
            }
        }

        if (charge.getIsDetected() == true)
        {
            charge.setCharged();
            trigger.setNotTriggered();
        }


        if (validCombo == false)
        {
            validCombo = true;
            combo = 0;
        }
    }

   
    public void pauseGame(){

        isPaused = true;
        enableSpawn = false;

        foreach (GameObject note in GameObject.FindObjectsOfType(typeof(GameObject)))
        {
            if (note.name == "Note(Clone)")
            {
                note.GetComponent<Note>().GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
            }
        }
    }


    public void resumeGame(){

        isPaused = false;
        noteTimer();

        foreach (GameObject note in GameObject.FindObjectsOfType(typeof(GameObject)))
        {
            if (note.name == "Note(Clone)")
            {
                note.GetComponent<Note>().GetComponent<Rigidbody2D>().velocity = new Vector2(0, speed);
            }
        }
    }
}
