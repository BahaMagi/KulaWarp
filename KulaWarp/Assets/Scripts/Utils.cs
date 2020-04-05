using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;


/**
* Base class used for all objects that are supposed to be reset when the level restarts. 
*/
public abstract class ObjectBase : MonoBehaviour
{
    public abstract void Reset();
}

/**
 * Holds a variaty of interpolations to expand what is provided by Unity.
 */
public class MyInterps
{
    /**
     * Interpolation from [0, 1] to [0, 1] with an Ease-Out.
     */
    public static Vector3 Sinerp(Vector3 start, Vector3 end, float t)
    {
        t = Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI * 0.5f);

        return new Vector3(
            start.x + (end.x - start.x) * t,
            start.y + (end.y - start.y) * t,
            start.z + (end.z - start.z) * t
            );
    }

    /**
     * Interpolation from [0, 1] to [0, 1] with an Ease-In.
     */
    public static Vector3 Coserp(Vector3 start, Vector3 end, float t)
    {
        t = 1 - Mathf.Cos(Mathf.Clamp01(t) * Mathf.PI * 0.5f);

        return new Vector3(
            start.x + (end.x - start.x) * t,
            start.y + (end.y - start.y) * t,
            start.z + (end.z - start.z) * t
            );
    }

    /**
     * Interpolation from [0, inf] -> [@start, @target] where @start and @target are vectors.
     * Eases in with a quadratic function for @ease_in/2 seconds and then interpolates linearly. 
     * The curve is continuous, i.e. the deriative of the functions at the junctions is the same. 
     * No Ease out. 
     * The value of @t can be >1, as the movement velocity is bound by @speed. 
     * The interpolation will always stop at @target, regardless of @t. 
     * The value of target is allowed to change during interpolation.
     * 
     * @start  : Point in space where the interpolation starts. This should not change.
     * @target : Point in space where the interpolation ends. This can change during interpolation.
     * @done   : out variable that is true if the interpolation reached @target. 
     * @t      : Time since the interpolation started in seconds.
     * @ease_in: Time of the quadratic Ease-In. The Ease-In takes @ease_in/2 seconds.  
     * @speed  : Velocity of the interpolation in units/second. The interpolation will never move faster than this. 
     * 
     * @return A position on the line between @start and @target for @t seconds given by an interpolation function.
     */
    public static Vector3 QuadEaseIn(Vector3 start, Vector3 target, out bool done, float t, float ease_in, float speed)
    {
        Vector3 direction = (target - start);
        float t_, mag = direction.magnitude; // @TODO Move this calculation to when the target is set to calculate the sqrt only once!
        direction = direction.normalized;

        if (t <= ease_in * 0.5f && ease_in > 0)
            t_ = speed * t * t / ease_in;
        else
            t_ = speed * (t - ease_in * 0.25f);

        t_ = Mathf.Min(t_, mag);
        done = t_ == mag;

        return new Vector3(
            start.x + direction.x * t_,
            start.y + direction.y * t_,
            start.z + direction.z * t_
            );
    }
}

/**
 * Extend existing classes with utility functions. 
 */
static class ExtensionMethods
{//@TODO Take care of the "ref" vs none "ref" Extensions. Might lead to unintended sideeffects. 
    // Should unify the behaviour at some point.


    public static string ToMyString<T>(this List<T> L, string separator = ", ")
    {
        return string.Join(separator, L.Select(i => i.ToString()));
    }

    /**
     * Round the components of vec for which axis is != 0. 
     * For example: 
     * 
     * Vector3 a = new Vector3(1.2, 1.6, 1.8).Round(Vector3.up); // Vector3.up = (0, 1, 0);
     * 
     * will yield a = (1.2, 2, 1.8);
     */
    public static Vector3 Round(this Vector3 vec, Vector3 axis)
    {
        Vector3Int a = Vector3Int.RoundToInt(axis);
        return new Vector3(
            a.x != 0 ? Mathf.RoundToInt(vec.x) : vec.x,
            a.y != 0 ? Mathf.RoundToInt(vec.y) : vec.y,
            a.z != 0 ? Mathf.RoundToInt(vec.z) : vec.z);
    }

    /**
     * Floors the components of vec for which axis is != 0. 
     * For example: 
     * 
     * Vector3 a = new Vector3(1.2, 1.6, 1.8).Floor(Vector3.up); // Vector3.up = (0, 1, 0);
     * 
     * will yield a = (1.2, 1, 1.8);
     */
    public static Vector3 Floor(this ref Vector3 vec, Vector3 axis)
    {
        Vector3Int a = Vector3Int.RoundToInt(axis);
        return new Vector3(
            a.x != 0 ? Mathf.FloorToInt(vec.x) : vec.x,
            a.y != 0 ? Mathf.FloorToInt(vec.y) : vec.y,
            a.z != 0 ? Mathf.FloorToInt(vec.z) : vec.z);
    }

    public static float L1Norm(this Vector3 vec)
    {
        return vec.x + vec.y + vec.z;
    }

    public static float getComponent(this Vector3 vec, Vector3 axis)
    {
        Vector3Int a = Vector3Int.RoundToInt(axis);
        return (a.x != 0 ? vec.x : a.y != 0 ? vec.y : vec.z);
    }

    public static Vector3 setComponent(this ref Vector3 vec, Vector3 component, float value)
    {
        Vector3Int c = Vector3Int.RoundToInt(component);

        if (c.x != 0)
            vec.x = value;
        else if (c.y != 0)
            vec.y = value;
        else
            vec.z = value;

        return vec;
    }

    /**
     * This rounds the up component of @vec to valid multiples of m_boxsize/2 +/- m_sphere Radius.
     * 
     * This is mainly necessary because the Idle animation introduces small numerical changes in the
     * up component. To avoid jumping of the ball this has to be countered. 
     */
    public static Vector3 SnapToGridUp(this Vector3 vec, Vector3 up)
    {
        return vec.Round(up) - (LevelController.lc.boxSize * 0.5f - PlayerController.pc.sphereRadius) * up;
    }

    public static Vector3 SnapToGridAll(this Vector3 vec, Vector3 up)
    {
        return vec.Round(Vector3.one) - (LevelController.lc.boxSize * 0.5f - PlayerController.pc.sphereRadius) * up;
    }

    public static Vector3 Abs(this Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    /**
     * This allows to search for (grand-)children recursively rather than 
     * only for direct children. 
     */
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(aParent);
        while (queue.Count > 0)
        {
            Transform c = queue.Dequeue();
            if (c.name == aName)
                return c;
            foreach (Transform t in c)
                queue.Enqueue(t);
        }
        return null;
    }

    /**
     * Instantiates a prefab as prefab regardless of whether <prefab> is an 
     * isntance of a prefab or the asset object from which an instance should be created. 
     * Note, that a call to Instantiate creates a gameobject that is not 
     * linked to the prefab anymore, thus not taking any changes. 
     */
    public static GameObject InstantiatePrefabAsPrefab(GameObject prefab)
    {
        GameObject newObject, p = prefab;
        PrefabAssetType      pType       = PrefabUtility.GetPrefabAssetType(p);
        PrefabInstanceStatus pInstStatus = PrefabUtility.GetPrefabInstanceStatus(p);

        // Check if the object in elementPrefab is part of a prefab
        // If elementPrefab is a prefab thats been dragged into place from the project window,
        // i.e. its not an instance, the instantiation has to be differnet than if the
        // instance of a prefab was used. Both are flagged prefabs and look the same
        // but cannot be instantiated the same way. 
        if (pType == PrefabAssetType.NotAPrefab)
        {
            Debug.LogError("The passed parameter has to be a prefab.");
            newObject = null;
        }
        else
        {
            // PrefabInstanceStatus.NotAPrefab is missleading but it means that the object is not
            // an instance(!) of a prefab but rather the asset object from which a prefab instance
            // was instantiated. 
            // If the object i elementPrefab is an instance we need to get the asset object first. 
            if (pInstStatus != PrefabInstanceStatus.NotAPrefab)
                p = PrefabUtility.GetCorrespondingObjectFromSource(p) as GameObject;

            newObject = (PrefabUtility.InstantiatePrefab(p) as GameObject);
        }

        return newObject;
    }
}


public class ReadOnlyAttribute : PropertyAttribute { }
public class ReadOnlyWhenPlayingAttribute : PropertyAttribute { }

#if UNITY_EDITOR   
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    // Override Height to prevent overlapping property fields in the inspector
    public override float GetPropertyHeight(SerializedProperty property,
                                            GUIContent label)
    { return EditorGUI.GetPropertyHeight(property, label, true); }

    // Disable input
    public override void OnGUI(Rect position,
                               SerializedProperty property,
                               GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}

[CustomPropertyDrawer(typeof(ReadOnlyWhenPlayingAttribute))]
public class ReadOnlyWhenPlayingAttributeDrawer : PropertyDrawer
{
    // Necessary since some properties tend to collapse smaller than their content
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    { return EditorGUI.GetPropertyHeight(property, label, true); }

    // Draw a disabled property field
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = !Application.isPlaying;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}

public class LockMenu : Editor
{
    // Adds a menu item to the Tools menu called "Toggle Inspector Lock" with a shortcut "Ctrl + l"
    [MenuItem("Tools/Toggle Inspector Lock %l")]
    public static void ToggleInspectorLock() 
    {
        // Toggle lock of current inspector and force unity to redraw the inspector window
        // to make changes immediately visible.
        ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
        ActiveEditorTracker.sharedTracker.ForceRebuild();
    }
}
#endif