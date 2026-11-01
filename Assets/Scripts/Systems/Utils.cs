using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class Utils
{
    #region MÉTODOS NUMÉRICOS

    private static System.Random rnd = new System.Random();

    // RANDOM
    public static Func<int, int, int> GetRandom = (min, max) => rnd.Next(min, max);
    public static Func<int, int, int> GetRandomInclusive = (min, max) => rnd.Next(min, max + 1);
    
    public static Func<float, float, float> GetRandomFloat = (min, max) => (float) (rnd.NextDouble() * (max - min) + min);
    public static Func<double, double, double> GetRandomDouble = (min, max) => rnd.NextDouble() * (max - min) + min;

    // CLAMP
    public static int Clamp(int val, int min, int max) => val < min ? min : (val > max ? max : val);
    public static float Clamp(float val, float min, float max) => Mathf.Clamp(val, min, max);
    public static double Clamp(double val, double min, double max) => Math.Clamp(val, min, max);

    // MAX
    public static int Max(int x, int y) => x > y ? x : y;
    public static float Max(float x, float y) => x > y ? x : y;
    public static double Max(double x, double y) => x > y ? x : y;

    // MIN
    public static int Min(int x, int y) => x < y ? x : y;
    public static float Min(float x, float y) => x < y ? x : y;
    public static double Min(double x, double y) => x < y ? x : y;

    #endregion

    #region ENTRADA DE USUARIO
    public static bool KeyPressed(Key key) => Keyboard.current != null && key != Key.None && Keyboard.current[key].wasPressedThisFrame;
    public static bool KeyHeld(Key key) => Keyboard.current != null && key != Key.None && Keyboard.current[key].isPressed;

    public static bool MouseActive() => Mouse.current != null;
    public static Vector2 GetMousePos() => Mouse.current.position.ReadValue();

    public static bool MouseClicked() => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    public static bool MouseClicked(ButtonControl button) => Mouse.current != null && button.wasPressedThisFrame;

    public static bool MouseReleased() => Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
    public static bool MouseReleased(ButtonControl button) => Mouse.current != null && button.wasReleasedThisFrame;

    public static ButtonControl LeftClick => Mouse.current.leftButton;
    public static ButtonControl MiddleClick => Mouse.current.middleButton;
    public static ButtonControl RightClick => Mouse.current.rightButton;
    #endregion
}