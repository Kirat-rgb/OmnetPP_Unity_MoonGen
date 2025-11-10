using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ENBMovementControl : MonoBehaviour
{
    public float moveSpeed = 0.5f;
    public float rotationSpeed = 45f;
    public Transform cameraTransform;
    public Vector3 positionLocal;

    private Renderer objectRenderer;
    public Material materialIdle;
    public Material materialActive;
    public Material materialHover;
    public bool selected = false;

    private bool isHovering = false;


    private void Start()    
    {
        objectRenderer = GetComponent<Renderer>();
    }

    private void Update()
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && hit.transform == transform)
        {
            isHovering = true;
        }
        else
        {
            isHovering = false;
        }

        if (selected)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            objectRenderer.material = isHovering ? materialHover : materialActive;
        }
        else
        {
            transform.Rotate(Vector3.up, rotationSpeed * 0.3f * Time.deltaTime);
            objectRenderer.material = isHovering ? materialHover : materialIdle;
        }

        if (Input.GetMouseButtonDown(0) && isHovering)
        {
            selected = true; ;
        }
        else if (Input.GetMouseButtonDown(0) && !isHovering)
        {
            selected = false;
        }
    }

    void FixedUpdate()
    {
        positionLocal = transform.localPosition;
        Vector3 move = Vector3.zero;

        Vector3 cameraForward = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;
        Vector3 cameraRight = new Vector3(cameraTransform.right.x, 0, cameraTransform.right.z).normalized;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            move += cameraForward;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            move -= cameraForward;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            move -= cameraRight;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            move += cameraRight;
        }
        if (Input.GetKey(KeyCode.I))
        {
            move.z += 1;
        }
        if (Input.GetKey(KeyCode.K))
        {
            move.z -= 1;
        }
        if (Input.GetKey(KeyCode.J))
        {
            move.x -= 1;
        }
        if (Input.GetKey(KeyCode.L))
        {
            move.x += 1;
        }

        move.y = 0;

        if (move != Vector3.zero)
        {
            move = move.normalized;
        }

        if (selected)
        {
            transform.Translate(move * moveSpeed * Time.fixedDeltaTime, Space.World);
        }
    }
}