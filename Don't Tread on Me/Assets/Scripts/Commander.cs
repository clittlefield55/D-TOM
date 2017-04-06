﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class Commander : MonoBehaviour {

    public GameObject target;
    public GameObject crate;
    public GameObject reticle;

    public float translateSpeed = 10.0f;
    public float crateHeight = 100.0f;
    public float timerCap = 1.0f;

    // Gun Properties
    public float rotateSpeed = 1;
    public float deadZone = 0.1f;
    public GameObject bullet;
    public GameObject tracer;
    public GameObject firePoint;
    public float bulletSpeed = 20.0f;
    public float reloadTime = 0.5f;
    public int tracerCap = 4;
    public GameObject gun;

    private float timeLast = 0.0f;
    private float oldX = 0;
    private float oldY = 0;
    private int tracerCounter = 0;


    private Vector3 oldTargetPosition;
    private float timerLast = 0.0f;

    protected GameObject inputMngr;
    public PlayerID playerID;

    // Use this for initialization
    void Start () {

        oldTargetPosition = target.transform.position;

        inputMngr = GameObject.Find("InputManager");
        playerID = inputMngr.GetComponent<PlayerRoles>().commander;
    }
	
	// Update is called once per frame
	void Update () {

        if (playerID != inputMngr.GetComponent<PlayerRoles>().commander) return;

        // follow the tank
        Vector3 follow = target.transform.position - oldTargetPosition;
        reticle.transform.Translate(follow, Space.World);

        // movement input
        if (InputManager.GetAxis("Left Stick Vertical", playerID) != 0.0f)
        {
            reticle.transform.Translate(new Vector3(1, 0, 1) * -InputManager.GetAxis("Left Stick Vertical", playerID) * translateSpeed  * Time.deltaTime, Space.World);
        }
        if (InputManager.GetAxis("Left Stick Horizontal", playerID) != 0.0f)
        {
            reticle.transform.Translate(new Vector3(-1, 0, 1) * -InputManager.GetAxis("Left Stick Horizontal", playerID) * translateSpeed * Time.deltaTime, Space.World);
        }

        // update tank's old position
        oldTargetPosition = target.transform.position;

        if(InputManager.GetAxis("Left Trigger", playerID) > 0)
        {
            if (Time.time - timerLast > timerCap)
            {
                Vector3 crateStart = reticle.transform.position;
                crateStart.y = crateHeight;
                Instantiate(crate, crateStart, Quaternion.identity);
                timerLast = Time.time;
            }
        }

        // machine gun
        UpdateGun();

        if (InputManager.GetAxis("DPAD Vertical", playerID) == 1)
        {
            Gunner gunner = GetComponent<Gunner>();
            gunner.playerID = playerID;
            playerID = inputMngr.GetComponent<PlayerRoles>().gunner;

            inputMngr.GetComponent<PlayerRoles>().gunner = gunner.playerID;
            inputMngr.GetComponent<PlayerRoles>().commander = playerID;
        }
        else if (InputManager.GetAxis("DPAD Horizontal", playerID) == -1)
        {
            Engineer engineer = GetComponent<Engineer>();
            engineer.playerID = playerID;
            playerID = inputMngr.GetComponent<PlayerRoles>().engineer;

            inputMngr.GetComponent<PlayerRoles>().engineer = engineer.playerID;
            inputMngr.GetComponent<PlayerRoles>().commander = playerID;
        }
    }

    // Update Commander Gun
    void UpdateGun()
    {
        float x = InputManager.GetAxis("Right Stick Horizontal", playerID);
        float y = InputManager.GetAxis("Right Stick Vertical", playerID);

        float angle;

        if (Mathf.Abs(x) < deadZone && Mathf.Abs(y) < deadZone)
        {
            angle = Mathf.Atan2(oldX, -oldY);
        }
        else
        {
            oldX = x;
            oldY = y;
            angle = Mathf.Atan2(x, -y);
            Fire();
        }

        Quaternion destination = Quaternion.EulerAngles(0, angle, 0) * Quaternion.AngleAxis(45, Vector3.up);
        gun.transform.rotation = Quaternion.Lerp(gun.transform.rotation, destination, Time.deltaTime * rotateSpeed);
    }

    // Fire function
    void Fire()
    {
        if (Time.time - timeLast > reloadTime)
        {
            GameObject newBullet;

            if (tracerCounter == tracerCap)
            {
                newBullet = Instantiate(tracer, firePoint.transform.position, gun.transform.rotation);
                tracerCounter = 0;
            }
            else
            {
                newBullet = Instantiate(bullet, firePoint.transform.position, gun.transform.rotation);
                tracerCounter++;
            }

            newBullet.GetComponent<Rigidbody>().AddForce(gun.transform.forward * bulletSpeed);
            timeLast = Time.time;
        }
    }
}
