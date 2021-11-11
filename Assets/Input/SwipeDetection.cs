using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeDetection : MonoBehaviour
{
    [SerializeField]
    private float minimunDistance = .2f;
    [SerializeField]
    private float maximunTime = 1f;
    [SerializeField, Range(0f, 1f)]
    private float directionThreshold = .9f;
    [SerializeField]
    private GameObject trail;
    [SerializeField]
    private TrailRenderer trailRenderer;
    [SerializeField]
    private GameObject line;
    [SerializeField]
    private LineRenderer lineRenderer;
    [SerializeField]
    private float maxJumpVector = 1f;

    private Material lineMaterial;
    private InputManager inputManager;

    private Vector2 startPosition;
    private float startTime;
    private Vector2 endPosition;
    private float endTime;

    private Coroutine trailCoroutine;
    private Coroutine lineCoroutine;

    private bool swiping;

    private void Awake()
    {
        inputManager = InputManager.Instance;
        lineMaterial = lineRenderer.material;
        swiping = false;
    }

    private void OnEnable()
    {
        inputManager.OnStartPrimaryTouch += SwipeStart;
        inputManager.OnEndPrimaryTouch += SwipeEnd;
    }

    private void OnDisable()
    {
        inputManager.OnStartPrimaryTouch -= SwipeStart;
        inputManager.OnEndPrimaryTouch -= SwipeEnd;
    }

    private void Update()
    {
        trailRenderer.enabled = swiping;
        
    }

    private void SwipeStart(Vector2 position, float time)
    {
        lineRenderer.enabled = true;
        startPosition = position;
        startTime = time;    
        trailCoroutine = StartCoroutine(Trail());
        lineCoroutine = StartCoroutine(Line());
    }

    private IEnumerator Trail()
    {
        while (true)
        {
            trail.transform.position = inputManager.PrimaryPosition();
            yield return null;
        }
    }

    private IEnumerator Line()
    {
        while (true)
        {
            Vector2 lastPosition = inputManager.PrimaryPosition();
            Vector2 direction = lastPosition - startPosition;
            //Vector2 direction2D = new Vector2(direction.x, direction.y).normalized;
            float dist = Mathf.Clamp(Vector2.Distance(startPosition, lastPosition), 0, maxJumpVector);
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, (startPosition + (direction.normalized * dist)));
            lineMaterial.SetTextureOffset("_MainTex", new Vector2(-Time.time * dist, 0));
            yield return null;
        }
    }

    private void SwipeEnd(Vector2 position, float time)
    {
        lineRenderer.enabled = false;
        StopCoroutine(trailCoroutine);
        StopCoroutine(lineCoroutine);
        endPosition = position;
        endTime = time;
        DetectSwipe();
    }

    private void DetectSwipe()
    {
        swiping = false;
        if (Vector3.Distance(startPosition, endPosition) >= minimunDistance && (startTime - endTime) <= maximunTime)
        {
            Debug.Log("Swipe Detected");
            swiping = true;
            Vector3 direction = endPosition - startPosition;
            Vector2 direction2D = new Vector2(direction.x, direction.y).normalized;
            SwipeDirection(direction2D);
        }
    }

    private void SwipeDirection(Vector2 direction)
    {
        if (Vector2.Dot(Vector2.up, direction) > directionThreshold)
        {
            Debug.Log("Swipe Up");
        }
        if (Vector2.Dot(Vector2.down, direction) > directionThreshold)
        {
            Debug.Log("Swipe Down");
        }
        if (Vector2.Dot(Vector2.left, direction) > directionThreshold)
        {
            Debug.Log("Swipe Left");
        }
        if (Vector2.Dot(Vector2.right, direction) > directionThreshold)
        {
            Debug.Log("Swipe Right");
        }
    }
}
