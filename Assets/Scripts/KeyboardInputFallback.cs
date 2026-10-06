using UnityEngine;
using System.Collections;

public class KeyboardInputFallback : NativeUnityInput
{
    public KeyboardInputFallback(int playerIndex) : base(playerIndex)
    {
    }

    public override bool AButton()
    {
        return Input.GetKey(KeyCode.Space);
    }

    public override bool AnyKey()
    {
        return Input.anyKey;
    }

    public override void Dispose()
    {
        base.Dispose();
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override bool GamepadPresent()
    {
        return true;
    }

    public override Vector2 GetDPad()
    {
        return base.GetDPad();
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override Vector2 GetLeftDirection()
    {
        Vector2 stick = new Vector2();
        
        stick.x += Input.GetKey(KeyCode.Q) ? 1f : -1f;
        stick.x += Input.GetKey(KeyCode.D) ? -1f : 1f;

        stick.y += Input.GetKey(KeyCode.Z) ? -1f : 1f;
        stick.y += Input.GetKey(KeyCode.S) ? 1f : -1f;

        return stick;
    }

    public override Vector2 GetRightDirection()
    {
        Vector2 stick = new Vector2();

        stick.x += Input.GetKey(KeyCode.RightArrow) ? 1f : -1f;
        stick.x += Input.GetKey(KeyCode.LeftArrow) ? -1f : 1f;

        stick.y += Input.GetKey(KeyCode.UpArrow) ? -1f : 1f;
        stick.y += Input.GetKey(KeyCode.DownArrow) ? 1f : -1f;

        return stick;
    }

    public override bool IsPressingRS()
    {
        return base.IsPressingRS();
    }

    public override bool IsPressingSelect()
    {
        return Input.GetKey(KeyCode.Alpha2);
    }

    public override bool IsPressingStart()
    {
        return Input.GetKey(KeyCode.Alpha1);
    }

    public override float LeftTrigger()
    {
        return Input.GetKey(KeyCode.A) ? 1f : 0f;
    }

    public override void Refresh()
    {
        base.Refresh();
    }

    public override float RightTrigger()
    {
        return Input.GetKey(KeyCode.E) ? 1f : 0f;
    }

    public override string ToString()
    {
        return base.ToString();
    }

    protected override void SetVibration(float leftValue, float rightValue)
    {
        base.SetVibration(leftValue, rightValue);
    }
}
