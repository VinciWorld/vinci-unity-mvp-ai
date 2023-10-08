using Unity.MLAgents.Integrations.Match3;
using UnityEngine;

public class InputControllerBasic : MonoBehaviour
{
    public bool _isMobile;
    public bool _isMouse;

    public Joystick movingJoystick;
    public Joystick aimJoystick;


    public float horizontal;

    [SerializeField]
    LayerMask groundMask;


    public Vector3 GetMoveDirection()
    {
        Vector3 direction = Vector3.zero;
        if (_isMobile)
        {
            direction.x = aimJoystick.Horizontal;
            direction.z = aimJoystick.Vertical;
        }
        else
        {
            // Convert mouse position to ray
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // Assuming Y is up
            float rayDistance;

            if (groundPlane.Raycast(ray, out rayDistance))
            {
                Vector3 pointOnPlane = ray.GetPoint(rayDistance);
                direction = pointOnPlane; // = new Vector3(pointOnPlane.x - transform.position.x, 0f, pointOnPlane.z - transform.position.z);

                //Debug.DrawLine(ray.origin, pointOnPlane, Color.green, 1f);
            }


        }

        //if (direction.magnitude > 1.0f)
        //{
        // direction.Normalize();
        //}

        return direction;
    }

    private (bool success, Vector3 position) GetMousePosition()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, groundMask))
        {
            // The Raycast hit something, return with the position.
            return (success: true, position: hitInfo.point);
        }
        else
        {
            // The Raycast did not hit anything.
            return (success: false, position: Vector3.zero);
        }
    }

    public Vector3 GetMove()
    {
        Vector3 move = Vector3.zero;

        if (_isMobile)
        {
            move.x = movingJoystick.Horizontal;
            move.z = movingJoystick.Vertical;

        }
        else
        {
            move.x = Input.GetAxis("Horizontal");
            move.z = Input.GetAxis("Vertical");
        }

        //if (move.magnitude > 1.0f)
        //{
        move.Normalize();
        //}

        return move;
    }

    public bool GetFireButton()
    {
        return Input.GetMouseButton(0);
    }

}

/*
    // Convert mouse position to ray
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    Plane groundPlane = new Plane(Vector3.up,
        new Vector3(transform.position.x, 0f, transform.position.z)); // Assuming Y is up
    float rayDistance;

    if (groundPlane.Raycast(ray, out rayDistance))
    {
        Vector3 pointOnPlane = ray.GetPoint(rayDistance);
        direction = new Vector3(pointOnPlane.x - transform.position.x, 0f,  pointOnPlane.z - transform.position.z);
    }
*/