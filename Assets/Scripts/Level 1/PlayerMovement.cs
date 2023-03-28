using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    // Basic movement information
    private CharacterController Controller;
    [SerializeField, Range(0, 20)] private float MoveSpeed = 5f;
    [SerializeField, Range(0, 15)] private float Sensitivity = 1.0f;
    [SerializeField] private bool OnGround;
    [SerializeField] private Rigidbody Rigidbody;
    [SerializeField, Range(0f, 200f)] private float JumpForce;

    // Camera information
    [SerializeField] private GameObject CameraObject;
    private float HorizontalRotation;

    // Damage overlay
    [SerializeField] private Image DamageIndicator;
    [SerializeField] private Animator DamageUIAnimator;

    private void Awake()
    {
        // Start the game with the player's cursor locked
        Cursor.lockState = CursorLockMode.Locked;
        Controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * Sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * Sensitivity;
        // Rotating the player themselves based on the mouse's horizontal movement
        transform.rotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0f, mouseX, 0f));
        // Rotating the camera inside the player based on the mouse's vertical movement
        HorizontalRotation += -mouseY;
        // Clamping rotation to prevent camera from doing a flip
        HorizontalRotation = Mathf.Clamp(HorizontalRotation, -90f, 90f);
        CameraObject.transform.localRotation = Quaternion.Euler(new Vector3(HorizontalRotation, 0f, 0f));

        // Player movement controls
        float forwardInput = Input.GetAxisRaw("Vertical");
        float sidewaysInput = Input.GetAxisRaw("Horizontal");
        // Normalizing the input vector to keep the player a consistent movement speed
        Vector3 normalizedInput = Vector3.Normalize(new Vector3(sidewaysInput, 0f, forwardInput));
        // Direct the input vectors based on the player's current rotation
        Vector3 movement = (normalizedInput.z * transform.forward) + (normalizedInput.x * transform.right);
        // Applying movement and speed
        transform.position += movement * Time.deltaTime * MoveSpeed;

        if (Input.GetKeyDown(KeyCode.Space) && OnGround)
        {
            Rigidbody.velocity = Vector3.up * JumpForce;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.name == "Goal")
        {
            Debug.Log("You reached the goal!");

            int index = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(index + 1);
        }

        if (collision.collider.gameObject.name.Contains("Tile"))
        {
            OnGround = true;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.gameObject.name.Contains("Tile"))
        {
            OnGround = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.gameObject.name.Contains("Tile"))
        {
            OnGround = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Smoke")
        {
            Debug.Log("You are getting hurt!");

            DamageUIAnimator.SetTrigger("Start Damage");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name == "Smoke")
        {
            Debug.Log("You are still getting hurt!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Smoke")
        {
            Debug.Log("You escaped!");

            DamageUIAnimator.SetTrigger("End Damage");
        }
    }
}
