﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    DashScript dashScript;
    EarthDash earthDash;
    JumpScript jumpScript;
    PlayerState playerState;
    WallClimb wallClimb;

    // Start is called before the first frame update
    void Start()
    {
        dashScript = GetComponent<DashScript>();
        earthDash = GetComponent<EarthDash>();
        jumpScript = GetComponent<JumpScript>();
        playerState = GetComponent<PlayerState>();    
        wallClimb = GetComponent<WallClimb>();
    }

    // Update is called once per frame
    void Update()
    {
        //manage input when we press fire
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            //if we aren't in fire already, dash
            if(playerState.element != Elements.elements.fire)
            {
                dashScript.pressedDash = true;
            }
            playerState.pressedFire = true;
        }

        //manage imput when we press water
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            playerState.pressedWater = true;
        }

        //manage input when we press earth
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            //if we aren't in earth already, earth dash
            if (playerState.element != Elements.elements.earth)
            {
                EarthDash.pressedDashToEarth = true;
            }

            playerState.pressedEarth = true;
        }

        //manage input when we press air
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (playerState.element != Elements.elements.air)
            {
                if (!jumpScript.usedAirJump)
                {
                    jumpScript.pressedAirJump = true;
                }
            }

            playerState.pressedAir = true;
        }

        //manage input when we press jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //if we're grounded and not earth form, jump
            if (CollisionManager.isGrounded == true && playerState.element != Elements.elements.earth)
            {
                jumpScript.pressedJump = true;
            }

            //if grabbing use walljump
            if (WallClimb.grabbing)
            {
                wallClimb.pressedWallJump = true;
            }
        }
    }
}
