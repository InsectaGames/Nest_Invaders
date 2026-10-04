using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Utils
{
    #region MÉTODOS NUMÉRICOS
    private static System.Random rnd = new System.Random();

    public static Func<int, int, int> GetRandom = (min, max) => rnd.Next(min, max);

    public static Func<int, int, int, int> Clamp = (val, min, max) => val < min ? min : (val > max ? max : val);

    public static Func<int, int, int> Max = (x, y) => (x > y) ? x : y;
    
    public static Func<int, int, int> Min = (x, y) => (x < y) ? x : y;
    #endregion

    #region ENTRADA DE USUARIO
    public static bool KeyPressed(Key key) => Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame;

    public static bool MouseClicked() => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

    public static Vector2 GetMousePos() => Mouse.current.position.ReadValue();
    #endregion
}