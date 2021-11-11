using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float maxJumpVector;
    [SerializeField]
    private float maxJumpForce;

    private InputManager inputManager;
    private Camera mainCamera;
    private Rigidbody rb;
    private bool aimingJump;
    private LineRenderer lineRenderer;
    private Material lineMaterial;
    private Coroutine lineCoroutine;
    private Vector3 lineStartPosition;
    private Vector3 lineEndPosition;
    private Vector3 direction;
    private float distance;

    private void Awake()
    {
        Physics.gravity = new Vector3(0, -30, 0);
        inputManager = InputManager.Instance;
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        lineRenderer = GetComponentInChildren<LineRenderer>();
        lineMaterial = lineRenderer.material;
        aimingJump = false;
    }

    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        lineRenderer.SetPosition(0, transform.position);
    }

    private void OnEnable()
    {
        inputManager.OnStartTouch += TouchStart;
        inputManager.OnMoveTouch += TouchMove;
        inputManager.OnEndTouch += TouchEnd;
    }

    private void OnDisable()
    {
        inputManager.OnStartTouch -= TouchStart;
        inputManager.OnMoveTouch -= TouchMove;
        inputManager.OnEndTouch -= TouchEnd;
    }

    private void TouchStart(Vector2 position, float time)
    {
        aimingJump = CheckTouching(position);
        lineRenderer.enabled = aimingJump;
        if (aimingJump)
        {
            lineCoroutine = StartCoroutine(Line());
        }
    }

    private void TouchMove(Vector2 position, float time)
    {
        Debug.Log("TOUCH MOVING");
        // VALUTARE SE SPOSTARE IL CODICE DELLA COROUTINE IN QUESTO EVENTO
    }

    private void TouchEnd(Vector2 position, float time)
    {
        lineRenderer.enabled = false;
        if (aimingJump)
        {
            aimingJump = false;
            StopCoroutine(lineCoroutine);
            rb.AddForce(direction * distance * maxJumpForce, ForceMode.Impulse);
        }
    }

    private IEnumerator Line()
    {
        while (true)
        {
            lineEndPosition.x = inputManager.PrimaryPosition().x;
            lineEndPosition.y = inputManager.PrimaryPosition().y;
            lineEndPosition.z = transform.position.z - .5f;
            lineStartPosition.x = transform.position.x;
            lineStartPosition.y = transform.position.y + 1;
            lineStartPosition.z = transform.position.z - .5f;
            direction = (lineEndPosition - lineStartPosition).normalized;
            //Vector2 direction2D = new Vector2(direction.x, direction.y).normalized;
            distance = Mathf.Clamp(Vector2.Distance(lineStartPosition, lineEndPosition), 0, maxJumpVector);
            lineRenderer.SetPosition(0, lineStartPosition);
            lineRenderer.SetPosition(1, (lineStartPosition + (direction * distance)));
            lineMaterial.SetTextureOffset("_MainTex", new Vector2(-Time.time * distance, 0));
            yield return null;
        }
    }

    // Check if the Player is touched
    private bool CheckTouching(Vector2 touchPosition)
    {
        Debug.Log(touchPosition);
        Debug.Log(mainCamera.ScreenPointToRay(touchPosition));

        Ray ray = mainCamera.ScreenPointToRay(touchPosition);


        RaycastHit hit;
        Vector3 origin = new Vector3(touchPosition.x, touchPosition.y, mainCamera.transform.position.z);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.blue, 10);
            Debug.Log("Did Hit");
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("Player Touched");
                return true;
            }
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.green, 10);
            Debug.Log("Did not Hit");
        }   
        
        return false;
    }
}
