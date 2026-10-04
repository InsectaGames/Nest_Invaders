using UnityEngine;
using UnityEngine.InputSystem;

public class InputUtils
{
    public static bool KeyPressed(Key key) => Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame;
    public static bool MouseClicked() => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    public static Vector2 GetMousePos() => Mouse.current.position.ReadValue();
}