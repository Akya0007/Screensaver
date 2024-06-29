using UnityEngine;

public class FreeMovement : MonoBehaviour
{
    public float speed = 10.0f;

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        float moveUp = 0.0f;

        if (Input.GetKey(KeyCode.Q))
        {
            moveUp = 1.0f;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            moveUp = -1.0f;
        }

        Vector3 movement = new Vector3(moveHorizontal, moveUp, moveVertical);
        transform.Translate(movement * speed * Time.deltaTime, Space.World);
    }
}
