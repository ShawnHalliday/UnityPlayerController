
using UnityEngine;
public class Movement : MonoBehaviour
{
    [SerializeField] private float groundSpeed = 5.0f;
    [SerializeField] private float groundSpeedSprint = 7.5f;
    [SerializeField] private float airSpeed = 8.0f;
    [SerializeField] private float kpGround = 0.1f;
    [SerializeField] private float kpAir = 0.01f;
    [SerializeField] private float accel = 1.0f;
    [SerializeField] private float friction = 15.0f;
    [SerializeField] private float jumpValue = 10.0f;
    [SerializeField] private float gravity = 15.0f;
    private CharacterController cc;
    private Vector3 playerVelocity = Vector3.zero;
    private float horizontal;
    private float vertical;
    private Vector3 normal = Vector3.zero;
    private bool JumpQueue= false;
    
    void Start()
    {
        cc = GetComponent<CharacterController>();
    }
    
    void Update()
    {
        vertical = Input.GetAxisRaw("Vertical");
        horizontal = Input.GetAxisRaw("Horizontal");
        float scrollWheelInput = Input.GetAxis("Mouse ScrollWheel");

        if (cc.isGrounded)
        {
            GroundMovement();
            
            if (Input.GetKeyDown(KeyCode.Space) || scrollWheelInput != 0 || JumpQueue)
            {
                jump();
                JumpQueue = false;
            }
        }

        else if (!cc.isGrounded)
        {
            AirMovement();
            //Check if the player can queue a jump, makes bhopping easier and more enjoyable
            if ((Input.GetKeyDown(KeyCode.Space) || scrollWheelInput != 0) && Physics.Raycast(transform.position, Vector3.down, 1.3f) && playerVelocity.y < 0)
            {
                JumpQueue = true;
            }
        }

        //Apply calculated Velocity
        cc.Move(playerVelocity*Time.deltaTime);
        
    }

    //Gets the perpendicular component of the velocity when colliding with a wall to get rid of all velocity facing the wall after a collision
    private void OnControllerColliderHit(ControllerColliderHit hit) {
        //Check if it's the ground first
        if (hit.gameObject.layer == 3)
        {
            return;
        }
        else
        {
            normal = hit.normal;
            playerVelocity.x = Vector3.ProjectOnPlane(new Vector3(playerVelocity.x, 0 ,playerVelocity.z), normal).x;
            playerVelocity.z = Vector3.ProjectOnPlane(new Vector3(playerVelocity.x, 0 ,playerVelocity.z), normal).z;
            Debug.DrawLine(transform.position, transform.position+Vector3.ProjectOnPlane(new Vector3(playerVelocity.x, 0 ,playerVelocity.z), normal)*3, Color.cyan);
        }
        
    }
    //Movement for when the player is grounded
    private void GroundMovement()
    {
        float speed;
        Vector3 wishDir = new Vector3(horizontal, 0, vertical);
        wishDir = transform.TransformDirection(wishDir);
        wishDir.Normalize();
        if (Input.GetKey(KeyCode.LeftShift)){
            speed = groundSpeedSprint;
        }
        else {
            speed = groundSpeed;
        }
        ProportionalAccel(wishDir, speed, kpGround);
        AddFriction(1.0f);
        playerVelocity.y = -1f;
    }
    //Movement for when the player is in the air
    private void AirMovement()
    {
        Vector3 wishDir = new Vector3(horizontal, 0, vertical);
        wishDir = transform.TransformDirection(wishDir);
        wishDir.Normalize();
        ProportionalAccel(wishDir, airSpeed, kpAir);
        playerVelocity.y -= gravity*Time.deltaTime;
    }

    //Self explanitory
    private void jump()
    {
        playerVelocity.y = jumpValue;
    }

    //A proportiopnal acceleration that gets applied to the player
    private void ProportionalAccel(Vector3 wishDir, float wishSpeed, float kp)
    {
        //By calculating for movement speed this way it allows the player to gain speed while strafing in air
        float currentSpeed = Vector3.Dot(playerVelocity, wishDir);

        float speedError = (wishSpeed-currentSpeed)*kp;
        playerVelocity.x += speedError*wishDir.x;
        playerVelocity.z += speedError*wishDir.z;
        Vector3 clamped = Vector3.ClampMagnitude(new Vector3(playerVelocity.x, 0, playerVelocity.z), 16);
        playerVelocity.x = clamped.x;
        playerVelocity.z = clamped.z;
    }

    //Adds friction to the player so they stop after they release their keyboard keys
    private void AddFriction(float amount)
    {
        Vector3 vec = playerVelocity; 
        vec.y = 0;
        float speed = vec.magnitude;
        float control;
        if (speed < accel)
        {
            control = accel;
        }
        else
        {
            control = speed;
        }
        float drop = control * friction * Time.deltaTime * amount;
        float newSpeed = speed - drop;
        if (newSpeed < 0)
        {
            newSpeed = 0;
        }

        if (speed > 0)
        {
            newSpeed /= speed;
        }
        playerVelocity.x *= newSpeed;
        playerVelocity.z *= newSpeed;
    }
}
