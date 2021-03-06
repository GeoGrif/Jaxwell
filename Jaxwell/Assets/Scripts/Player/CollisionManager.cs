using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    public static bool isGrounded = false;
    public static bool isAgainstWallLeft = false;
    public static bool isAgainstWallRight = false;
    public static GameObject groundedObject = null;
    public static GameObject wallRightObject = null;
    public static GameObject wallLeftObject = null;

    bool lastGroundedOutput = false;

    JumpScript jumpScript;

    [Header("Settings")]
    [Tooltip("Time in seconds that the player can jump after they are no longer grounded")]
    [SerializeField] float coyoteTime = 0.1f;

    [Header("Debug")]
    [Tooltip("Draws raycasts to show where we are checking if we are grounded")]
    [SerializeField] bool drawGroundedRaycastDebug = false;
    [Tooltip("Draws raycasts to show where we are checking if we are against a wall")]
    [SerializeField] bool drawWallCheckRaycastDebug = false;

    float raycastStartOffset;
    float raycastDistance;
    Vector2 raycastStartPoint;

    float tempCoyoteTime;
    bool coyoteTimeActive = false;
    bool groundedLastFrame = false;
    public static bool jumped = false;
    public static bool preJump = false;
    private int i = 0;

    BoxCollider2D p_collider;
    Rigidbody2D p_rigidbody;
    PlayerState playerState;

    void Start()
    {
        p_collider = GetComponent<BoxCollider2D>();
        p_rigidbody = GetComponent<Rigidbody2D>();
        playerState = GetComponent<PlayerState>();
        //the distance from centre of the collider we will check left/right and top/bottom
        raycastStartOffset = p_collider.size.y * 0.4f;
        //the distance the raycast will travel (just further than the collider)
        raycastDistance = (p_collider.size.y * 0.5f) +0.05f;

        tempCoyoteTime = coyoteTime;
    }

    void Update()
    {
        if(isGrounded)
        {
            groundedLastFrame = true;
        }

        if(!isGrounded && groundedLastFrame)
        {
            //i is the buffer for the frame, increment only if we're not grounded this frame
            if(i == 1)
            {
                groundedLastFrame = false;
                i = 0;
            }
            i++;
        }

        if (coyoteTimeActive)
        {
            DebugHelper.Log("Coyote time is active");
            isGrounded = true;

            //start counting down our coyote time
            tempCoyoteTime -= Time.deltaTime;
            if(tempCoyoteTime <= 0)
            {
                DebugHelper.Log("Coyote time ended");
                coyoteTimeActive = false;
                //set isGrounded to false when coyote time is over
                isGrounded = false;
                //reset coyote time
                tempCoyoteTime = coyoteTime;
            }

            //if we jump disable coyote time straight away
            if (jumped)
            {
                DebugHelper.Log("Coyote time no longer active because we jumped during coyote time");
                coyoteTimeActive = false;
                //set isGrounded to false when coyote time is over
                isGrounded = false;
                //reset coyote time
                tempCoyoteTime = coyoteTime;
                jumped = false;
            }
        }

        if(!isGrounded && groundedObject != null)
        {
            groundedObject = null;
            lastGroundedOutput = false;
        }

        if(!isAgainstWallRight && wallRightObject != null)
        {
            wallRightObject = null;
        }

        if (!isAgainstWallLeft && wallLeftObject != null)
        {
            wallLeftObject = null;
        }

        if (groundedObject != null && !lastGroundedOutput)
        {
            DebugHelper.Log("Last Groundedobject = " + groundedObject);
            lastGroundedOutput = true;
        }
    }

    //run checks when we collide with something
    void OnCollisionEnter2D(Collision2D collision)
    {
        isGrounded = GroundedCheck();
        isAgainstWallRight = WallCheckRight();

        //if we didn't hit anything with our raycasts to the right, try left   
        if (!isAgainstWallRight)
        {
            isAgainstWallLeft = WallCheckLeft();
        }        
    }

    //run wallcheck when we stay colliding with something
    void OnCollisionStay2D(Collision2D collision)
    {
        isAgainstWallRight = WallCheckRight();

        //if we didn't hit anything with our raycasts to the right, try left   
        if (!isAgainstWallRight)
        {
            isAgainstWallLeft = WallCheckLeft();
        }

        isGrounded = GroundedCheck();
    }

    //run checks when we exit collision with something
    void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
        isAgainstWallRight = false;
        isAgainstWallLeft = false;        
    }

    private bool GroundedCheck()
    {
        bool isGroundedCheck = false;

        //make sure the raycast start point take into account the offset
        raycastStartPoint = new Vector2(transform.position.x + p_collider.offset.x, transform.position.y + p_collider.offset.y);

        //create 2D raycast fired directly down from player's centre just a tiny bit further than our player
        RaycastHit2D hit = Physics2D.Raycast(raycastStartPoint, -Vector2.up, raycastDistance);

        if (drawGroundedRaycastDebug)
        {
            //draw ray to check debug for grounded check
            Debug.DrawRay(raycastStartPoint, -Vector2.up * raycastDistance, Color.red, 0.0f);
        }

        //if the raycast hits something
        if (hit.collider != null)
        {
            //print what the raycast hit to console and where the object is
            DebugHelper.Log("Raycast to check if we're grounded from " + p_collider.gameObject + " hit " + hit.collider.gameObject + " at " + hit.point);
            isGroundedCheck = true;
            groundedObject = hit.collider.gameObject;
        }
        else
        {
            //make sure the raycast start point take into account the offset
            raycastStartPoint = new Vector2(transform.position.x + p_collider.offset.x - raycastStartOffset, transform.position.y + p_collider.offset.y);

            //fire a raycast from the left side of the player if the centre might not hit
            RaycastHit2D hitLeft = Physics2D.Raycast(raycastStartPoint, -Vector2.up, raycastDistance);

            if (drawGroundedRaycastDebug)
            {
                //draw ray to check debug for grounded check
                Debug.DrawRay(raycastStartPoint, -Vector2.up * raycastDistance, Color.red, 0.0f);
            }

            if (hitLeft.collider != null)
            {
                //print what the raycast hit to console and where the object is
                DebugHelper.Log("Left raycast to check if we're grounded from " + p_collider.gameObject + " hit " + hitLeft.collider.gameObject + " at " + hitLeft.point);
                isGroundedCheck = true;
                groundedObject = hitLeft.collider.gameObject;
            }
            else
            {
                raycastStartPoint = new Vector2(transform.position.x + p_collider.offset.x + raycastStartOffset, transform.position.y + p_collider.offset.y);


                //fire a raycast from the right side of the player if the centre or left might not hit
                RaycastHit2D hitRight = Physics2D.Raycast(raycastStartPoint, -Vector2.up, raycastDistance);

                if (drawGroundedRaycastDebug)
                {
                    //draw ray to check debug for grounded check
                    Debug.DrawRay(raycastStartPoint, -Vector2.up * raycastDistance, Color.red, 0.0f);
                }

                if (hitRight.collider != null)
                {
                    //print what the raycast hit to console and where the object is
                    DebugHelper.Log("Right raycast to check if we're grounded from " + p_collider.gameObject + " hit " + hitRight.collider.gameObject + " at " + hitRight.point);
                    isGroundedCheck = true;
                    groundedObject = hitRight.collider.gameObject;
                }
            }
        }
        //if we're not grounded or against any walls after doing a ground check enable coyote time
        if(groundedLastFrame && !isGroundedCheck && !isAgainstWallLeft && !isAgainstWallRight)
        {
            DebugHelper.Log("Raycasts to check if we're grounded from " + p_collider.gameObject + " didn't hit anything and we were grounded last frame, enabling coyote time");
            coyoteTimeActive = true;
        }
        return isGroundedCheck;
    }

    public void CheckGroundedObject()
    {
        //make sure the raycast start point take into account the offset
        raycastStartPoint = new Vector2(transform.position.x + p_collider.offset.x, transform.position.y + p_collider.offset.y);

        //create 2D raycast fired directly down from player's centre just a tiny bit further than our player
        RaycastHit2D hit = Physics2D.Raycast(raycastStartPoint, -Vector2.up, raycastDistance);

        if (drawGroundedRaycastDebug)
        {
            //draw ray to check debug for grounded check
            Debug.DrawRay(raycastStartPoint, -Vector2.up * raycastDistance, Color.red, 0.0f);
        }

        //if the raycast hits something
        if (hit.collider != null)
        {
            //print what the raycast hit to console and where the object is
            DebugHelper.Log("Raycast to check if we're grounded from " + p_collider.gameObject + " hit " + hit.collider.gameObject + " at " + hit.point);
            groundedObject = hit.collider.gameObject;
            return;
        }
        else
        {
            //make sure the raycast start point take into account the offset
            raycastStartPoint = new Vector2(transform.position.x + p_collider.offset.x - raycastStartOffset, transform.position.y + p_collider.offset.y);

            //fire a raycast from the left side of the player if the centre might not hit
            RaycastHit2D hitLeft = Physics2D.Raycast(raycastStartPoint, -Vector2.up, raycastDistance);

            if (drawGroundedRaycastDebug)
            {
                //draw ray to check debug for grounded check
                Debug.DrawRay(raycastStartPoint, -Vector2.up * raycastDistance, Color.red, 0.0f);
            }

            if (hitLeft.collider != null)
            {
                //print what the raycast hit to console and where the object is
                DebugHelper.Log("Left raycast to check if we're grounded from " + p_collider.gameObject + " hit " + hitLeft.collider.gameObject + " at " + hitLeft.point);
                groundedObject = hitLeft.collider.gameObject;
                return;
            }
            else
            {
                raycastStartPoint = new Vector2(transform.position.x + p_collider.offset.x + raycastStartOffset, transform.position.y + p_collider.offset.y);


                //fire a raycast from the right side of the player if the centre or left might not hit
                RaycastHit2D hitRight = Physics2D.Raycast(raycastStartPoint, -Vector2.up, raycastDistance);

                if (drawGroundedRaycastDebug)
                {
                    //draw ray to check debug for grounded check
                    Debug.DrawRay(raycastStartPoint, -Vector2.up * raycastDistance, Color.red, 0.0f);
                }

                if (hitRight.collider != null)
                {
                    //print what the raycast hit to console and where the object is
                    DebugHelper.Log("Right raycast to check if we're grounded from " + p_collider.gameObject + " hit " + hitRight.collider.gameObject + " at " + hitRight.point);
                    groundedObject = hitRight.collider.gameObject;
                    return;
                }
            }
        }
    }

    private bool WallCheckRight()
    {
        bool isWallCheck = false;

        //make sure the raycast start point take into account the offset
        raycastStartPoint = new Vector2(transform.position.x + p_collider.offset.x, transform.position.y + p_collider.offset.y);

        //create 2D raycast fired directly right from player's centre just a tiny bit further than our player
        RaycastHit2D grabRightHit = Physics2D.Raycast(raycastStartPoint, Vector2.right, raycastDistance);

        if (drawWallCheckRaycastDebug)
        {
            //draw ray to check debug for wall check
            Debug.DrawRay(raycastStartPoint, Vector2.right * raycastDistance, Color.cyan, 0.0f);
        }

        //if the raycast hits something
        if (grabRightHit.collider != null)
        {
            //print what the raycast hit to console and where the object is
            DebugHelper.Log("Grab raycast right from centre hit " + grabRightHit.collider.gameObject + " at " + grabRightHit.point);
            isWallCheck = true;
            wallRightObject = grabRightHit.collider.gameObject;

        }
        else
        {
            //make sure the raycast start point take into account the offset
            raycastStartPoint = new Vector2(transform.position.x + p_collider.offset.x, transform.position.y + p_collider.offset.y - raycastStartOffset);

            //fire a raycast from the bottom side of the player if the centre might not hit
            RaycastHit2D grabRightHitBottom = Physics2D.Raycast(raycastStartPoint, Vector2.right, raycastDistance);

            if (drawWallCheckRaycastDebug)
            {
                //draw ray to check debug for wall check
                Debug.DrawRay(raycastStartPoint, Vector2.right * raycastDistance, Color.cyan, 0.0f);
            }

            if (grabRightHitBottom.collider != null)
            {
                //print what the raycast hit to console and where the object is
                DebugHelper.Log("Grab raycast right from bottom hit " + grabRightHitBottom.collider.gameObject + " at " + grabRightHitBottom.point);
                isWallCheck = true;
                wallRightObject = grabRightHitBottom.collider.gameObject;

            }
            else
            {
                //make sure the raycast start point take into account the offset
                raycastStartPoint = new Vector2(transform.position.x + p_collider.offset.x, transform.position.y + p_collider.offset.y + raycastStartOffset);

                //fire a raycast from the top side of the player if the centre or bottom might not hit
                RaycastHit2D grabRightHitTop = Physics2D.Raycast(raycastStartPoint, Vector2.right, raycastDistance);

                if (drawWallCheckRaycastDebug)
                {
                    //draw ray to check debug for wall check
                    Debug.DrawRay(raycastStartPoint, Vector2.right * raycastDistance, Color.cyan, 0.0f);
                }

                if (grabRightHitTop.collider != null)
                {
                    //print what the raycast hit to console and where the object is
                    DebugHelper.Log("Grab raycast right from top hit " + grabRightHitTop.collider.gameObject + " at " + grabRightHitTop.point);
                    isWallCheck = true;
                    wallRightObject = grabRightHitTop.collider.gameObject;

                }             
            }
        }

        return isWallCheck;
    }

    private bool WallCheckLeft()
    {
        bool isWallCheck = false;

        //make sure the raycast start point take into account the offset
        raycastStartPoint = new Vector2(transform.position.x + p_collider.offset.x, transform.position.y + p_collider.offset.y);

        //create 2D raycast fired directly left from player's centre just a tiny bit further than our player
        RaycastHit2D grabLeftHit = Physics2D.Raycast(raycastStartPoint, -Vector2.right, raycastDistance);

        if (drawWallCheckRaycastDebug)
        {
            //draw ray to check debug for wall check
            Debug.DrawRay(raycastStartPoint, -Vector2.right * raycastDistance, Color.cyan, 0.0f);
        }

        //if the raycast hits something
        if (grabLeftHit.collider != null)
        {
            //print what the raycast hit to console and where the object is
            DebugHelper.Log("Grab raycast left from centre hit " + grabLeftHit.collider.gameObject + " at " + grabLeftHit.point);
            isWallCheck = true;
            wallLeftObject = grabLeftHit.collider.gameObject;

        }
        else
        {
            //make sure the raycast start point take into account the offset
            raycastStartPoint = new Vector2(transform.position.x + p_collider.offset.x, transform.position.y + p_collider.offset.y - raycastStartOffset);

            //fire a raycast from the bottom side of the player if the centre might not hit; 0.4f to avoid raycasting from the very edge, may lead to false positives when we hit something with the bottom side
            RaycastHit2D grabLeftHitBottom = Physics2D.Raycast(raycastStartPoint, -Vector2.right, raycastDistance);

            if (drawWallCheckRaycastDebug)
            {
                //draw ray to check debug for wall check
                Debug.DrawRay(raycastStartPoint, -Vector2.right * raycastDistance, Color.cyan, 0.0f);
            }

            if (grabLeftHitBottom.collider != null)
            {
                //print what the raycast hit to console and where the object is
                DebugHelper.Log("Grab raycast left from bottom hit " + grabLeftHitBottom.collider.gameObject + " at " + grabLeftHitBottom.point);
                isWallCheck = true;
                wallLeftObject = grabLeftHitBottom.collider.gameObject;
            }
            else
            {
                //make sure the raycast start point take into account the offset
                raycastStartPoint = new Vector2(transform.position.x + p_collider.offset.x, transform.position.y + p_collider.offset.y + raycastStartOffset);

                //fire a raycast from the top side of the player if the centre or bottom might not hit; 0.4f to avoid raycasting from the very edge, may lead to false positives when we hit something with the right side
                RaycastHit2D grabLeftHitTop = Physics2D.Raycast(raycastStartPoint, -Vector2.right, raycastDistance);

                if (drawWallCheckRaycastDebug)
                {
                    //draw ray to check debug for wall check
                    Debug.DrawRay(raycastStartPoint, -Vector2.right * raycastDistance, Color.cyan, 0.0f);
                }

                if (grabLeftHitTop.collider != null)
                {
                    //print what the raycast hit to console and where the object is
                    DebugHelper.Log("Grab raycast left top hit " + grabLeftHitTop.collider.gameObject + " at " + grabLeftHitTop.point);
                    isWallCheck = true;
                    wallLeftObject = grabLeftHitTop.collider.gameObject;

                }
            }
        }

        return isWallCheck;
    }
}