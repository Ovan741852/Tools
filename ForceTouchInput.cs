using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class ForceTouchInput : BaseInput
{
    private bool _hasFakeTouch;
    private Touch _fakeTouch;
    private Queue<SimulateTouchCommand> _queue;
    private Coroutine _coroutine;

    public bool HasSimulateTouchCommand => _queue.Count != 0;

    protected override void Awake()
    {
        base.Awake();
        _queue = new Queue<SimulateTouchCommand>();
    }

    public override int touchCount
    {
        get
        {
            if (_hasFakeTouch)
                return 1;
            return Input.touchCount;
        }
    }

    public override Touch GetTouch(int index)
    {
        if (_hasFakeTouch)
        {
            return _fakeTouch;
        }
        else
        {
            return base.GetTouch(index);
        }
    }

    public void AddSimulateTouch(SimulateTouchCommand command)
    {
        _queue.Enqueue(command);
    }

    public void BeforeProcess()
    {
        if (_coroutine != null)
        {
            _hasFakeTouch = true;
        }
        else
        {
            if (_queue.Count != 0)
            {
                _hasFakeTouch = true;
                var command = _queue.Dequeue();
                _coroutine = StartCoroutine(DoSimulateTouch(command));
            }
            else
            {
                MouseSimulateTouch();
            }
        }
    }

    public void AfterProcess()
    {
        _hasFakeTouch = false;
    }

    private IEnumerator DoSimulateTouch(SimulateTouchCommand command)
    {
        float lifeTime = command.duration;

        _fakeTouch.phase = TouchPhase.Began;
        _fakeTouch.position = command.from;
        _fakeTouch.deltaPosition = Vector3.zero;
        yield return null;

        if (!command.isTap)
        {
            while (lifeTime >= 0)
            {
                lifeTime -= Time.deltaTime;
                _fakeTouch.phase = TouchPhase.Moved;
                var next = command.GetInterpolate(command.duration - lifeTime);
                _fakeTouch.deltaPosition = _fakeTouch.position - next;
                _fakeTouch.position = next;
                yield return null;
            }

            _fakeTouch.phase = TouchPhase.Ended;
            _fakeTouch.deltaPosition = command.to - _fakeTouch.position;
            _fakeTouch.position = command.to;
        }
        else
        {
            _fakeTouch.phase = TouchPhase.Ended;
            _fakeTouch.position = command.to;
            _fakeTouch.deltaPosition = Vector3.zero;
        }
        
        yield return null;
        _coroutine = null;
    }

    private void MouseSimulateTouch()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            _hasFakeTouch = true;
            _fakeTouch.phase = TouchPhase.Began;
            _fakeTouch.position = Input.mousePosition;
            _fakeTouch.deltaPosition = Vector3.zero;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _hasFakeTouch = true;
            _fakeTouch.phase = TouchPhase.Ended;
            _fakeTouch.deltaPosition = (Vector2)Input.mousePosition - _fakeTouch.position;
            _fakeTouch.position = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            _hasFakeTouch = true;
            _fakeTouch.deltaPosition = (Vector2)Input.mousePosition - _fakeTouch.position;
            _fakeTouch.position = Input.mousePosition;
            if (_fakeTouch.deltaPosition.magnitude > EventSystem.current.pixelDragThreshold)
                _fakeTouch.phase = TouchPhase.Moved;
            else
                _fakeTouch.phase = TouchPhase.Stationary;
        }
#endif
    }

#if DEBUG
    private void OnGUI()
    {
        var position = _fakeTouch.position;
        var rect = new Rect(position.x, Screen.height - position.y, 50, 50);
        GUI.Box(rect, "Fake");
    }
#endif
}

public struct SimulateTouchCommand
{
    public SimulateTouchCommand(Vector2 screenPos)
    {
        isTap = true;
        from = screenPos;
        to = screenPos;
        duration = 0.1f;
    }

    public SimulateTouchCommand(Vector2 from, Vector2 to, float duration)
    {
        isTap = false;
        this.from = from;
        this.to = to;
        this.duration = duration;
    }

    public bool isTap { get; private set; }

    public Vector2 from { get; private set; }

    public Vector2 to { get; private set; }

    public float duration { get; private set; }

    public Vector2 GetInterpolate(float elapsed)
    {
        if (elapsed <= 0)
        {
            return from;
        }
        else if (elapsed >= duration)
        {
            return to;
        }
        else
        {
            var time = elapsed / duration;
            return Vector2.Lerp(from, to, time);
        }
    }
}
