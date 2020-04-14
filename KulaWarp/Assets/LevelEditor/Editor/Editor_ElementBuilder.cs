using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ElementBuilder))]
public class ElementBuilderEditor : Editor
{
    private          bool     m_showSourceList = false;
    private readonly string[] m_listNames      = { "Block List", "Interactable List", "Pickup List", "Player List" };
    private ElementBuilder.ElementType m_type  = ElementBuilder.ElementType.None;

    public override void OnInspectorGUI()
    {
        // Draw the default inspector as base to extend it.
        // It can only be extended. Not modified! If elements are to be modified,
        // they have to use [HideInInspector] and added again here. 
        // base.OnInspectorGUI();

        // SerialitedObject is a representation of the serialized data of the Editor. 
        // The Update method reads and copies the data into an internal structure 
        // (SerializedProperty). 
        // Once "updated" a SerializedObject and it's SerializedProperties 
        // represent a copy of the serialized data in memory.
        serializedObject.Update();

        // Target is provided by the Editor base class and holds the 
        // ElementBuilder component this inspector is created for 
        ElementBuilder e = target as ElementBuilder;

        /*
         * Source Lists
         */
        {
            // EditorGUILayout.Foldout creates an inspector entry with a small arrow to 
            // the left that toggles right and down to indicate a foldout or collabsable 
            // group. It does not actually collabs anything. It rather just writes in a 
            // flag <m_showSourceList> that can be used as condition to draw other elements.
            m_showSourceList = EditorGUILayout.Foldout(m_showSourceList, "Source Lists");

            if (m_showSourceList)
            {
                SerializedProperty a = serializedObject.FindProperty(nameof(e.sourceLists));

                // Indent the Array elements one level compared to the "Source Lists" 
                // foldput object.
                EditorGUI.indentLevel += 1;
                for (int i = 0; i < a.arraySize; i++) // m_listNames[i] holds displayed names 
                    EditorGUILayout.PropertyField(a.GetArrayElementAtIndex(i), new GUIContent(m_listNames[i]));

                EditorGUI.indentLevel -= 1;
            }
        }
        /*
         * Type Enum Dropdown
         */
        {
            e.type = (ElementBuilder.ElementType)EditorGUILayout.EnumPopup("Type", e.type);

            // Custom Editors don't call OnValidate autmatically so we have to send the message
            // manually when the type has changed. 
            if(m_type != e.type)
                e.SendMessage("OnValidate", null, SendMessageOptions.DontRequireReceiver);

            m_type = e.type;
        }

        /**
         * Merge Button
         */
        {
            if(GUILayout.Button("Merge Blocks"))
                LevelGenerator.mergeBlockGeometry();
        }

        // Write the modified data in the SerializedProperties back to disk. 
        serializedObject.ApplyModifiedProperties();
    }
}
