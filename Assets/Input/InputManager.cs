using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

[DefaultExecutionOrder(-1)]
public class InputManager : Singleton<InputManager>
{
    #region "events"
    public delegate void StartTouchEvent(Vector2 position, float time);
    public event StartTouchEvent OnStartTouch;
    public delegate void MoveTouchEvent(Vector2 position, float time);
    public event MoveTouchEvent OnMoveTouch;
    public delegate void EndTouchEvent(Vector2 position, float time);
    public event EndTouchEvent OnEndTouch;

    public delegate void StartPrimaryTouchEvent(Vector2 position, float time);
    public event StartPrimaryTouchEvent OnStartPrimaryTouch;
    public delegate void EndPrimaryTouchEvent(Vector2 position, float time);
    public event EndPrimaryTouchEvent OnEndPrimaryTouch;
    #endregion

    private TouchControls touchControls;
    private Camera cameraMain;

    private void Awake()
    {
        touchControls = new TouchControls();
        cameraMain = Camera.main;
    }

    private void OnEnable()
    {
        touchControls.Enable();
        EnhancedTouchSupport.Enable();
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += FingerDown;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove += FingerMove;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp += FingerUp;
    }

    private void OnDisable()
    {
        touchControls.Disable();
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= FingerDown;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove -= FingerMove;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp -= FingerUp;
        EnhancedTouchSupport.Disable();
    }

    private void Start()
    {
        //touchControls.Touch.TouchPress.started += context => StartTouch(context);
        //touchControls.Touch.TouchPress.canceled += context => EndTouch(context);
        touchControls.Touch.PrimaryContact.started += context => StartTouchPrimary(context);
        touchControls.Touch.PrimaryContact.canceled += context => EndTouchPrimary(context);
    }

    private void StartTouch(InputAction.CallbackContext context)
    {
        Debug.Log("Touch Started " + touchControls.Touch.TouchPosition.ReadValue<Vector2>());
        if (OnStartTouch != null) OnStartTouch(touchControls.Touch.TouchPosition.ReadValue<Vector2>(), (float)context.startTime);
    }

    private void EndTouch(InputAction.CallbackContext context)
    {
        Debug.Log("Touch Ended " + touchControls.Touch.TouchPosition.ReadValue<Vector2>());
        if (OnEndTouch != null) OnEndTouch(touchControls.Touch.TouchPosition.ReadValue<Vector2>(), (float)context.time);
    }

    private void StartTouchPrimary(InputAction.CallbackContext context)
    {
        if (OnStartPrimaryTouch != null) OnStartPrimaryTouch(PrimaryPosition(), (float)context.startTime);
    }

    private void EndTouchPrimary(InputAction.CallbackContext context)
    {
        if (OnEndPrimaryTouch != null) OnEndPrimaryTouch(PrimaryPosition(), (float)context.time);
    }

    public Vector2 PrimaryPosition()
    {
        return Utils.ScreenToWorld(cameraMain, touchControls.Touch.PrimaryPosition.ReadValue<Vector2>());
    }

    private void FingerDown(Finger finger)
    {
        if (OnStartTouch != null) OnStartTouch(finger.screenPosition, Time.time);
    }

    private void FingerMove(Finger finger)
    {
        if (OnMoveTouch != null) OnMoveTouch(finger.screenPosition, Time.time);
    }

    private void FingerUp(Finger finger)
    {
        if (OnEndTouch != null) OnEndTouch(finger.screenPosition, Time.time);
    }

    private void Update()
    {
        if (EnhancedTouchSupport.enabled)
        {
            UnityEngine.InputSystem.Utilities.ReadOnlyArray<UnityEngine.InputSystem.EnhancedTouch.Touch> touches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
            //Debug.Log(touches);
            foreach (UnityEngine.InputSystem.EnhancedTouch.Touch touch in touches)
            {
                //Debug.Log(touch.phase);
            }
        }
    }
}
