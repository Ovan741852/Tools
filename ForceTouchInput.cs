using UnityEngine;
using UnityEngine.EventSystems;

public class ForceTouchInput : BaseInput
{
    private bool _hasFakeTouch;
    private Touch _fakeTouch;

    public override int touchCount
    {
        get
        {
            if (_hasFakeTouch)
            {
                _hasFakeTouch = false;
                return 1;
            }
            return Input.touchCount;
        }
    }

    public override Touch GetTouch(int index)
    {
        Debug.Log("touch");
        if (_hasFakeTouch)
        {
            return _fakeTouch;
        }
        else
        {
            return base.GetTouch(index);
        }
    }

#if !UNITY_ANDROID && !UNITY_IOS
    private void Update()
    {
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
            if (_fakeTouch.deltaPosition.sqrMagnitude > 20)
                _fakeTouch.phase = TouchPhase.Moved;
            else
                _fakeTouch.phase = TouchPhase.Stationary;
        }

    }
#endif 
}
