using UnityEngine;
using UnityEditor;
using System;

[Serializable]
public struct SettingContainer
{
    public bool  fullScreen;
    public float musicVolume;
    public float sfxVolume;
}

[ExecuteInEditMode]
public class Settings : MonoBehaviour
{
    public static Settings s;

    [ReadOnly]
    public SettingContainer settings;

    private void Awake()
    {
        // Make this a public singelton
        if (s == null) s = this;
        else if (s != this) Destroy(gameObject);

        // Initialize settings
        settings.fullScreen  = false;
        settings.musicVolume = 1.0f;
        settings.sfxVolume   = 1.0f;
    }

    public void ToggleFullScreen()
    {
#if UNITY_EDITOR
        EditorWindow window = EditorWindow.focusedWindow;
        // Assume the game view is focused
        window.maximized = !window.maximized;

        // Store settings
        settings.fullScreen = window.maximized;
#else
        Screen.fullScreen = !Screen.fullScreen;

        // Store settings
        settings.fullScreen = Screen.fullScreen;
#endif
    }
}
