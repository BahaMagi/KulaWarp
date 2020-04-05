using UnityEngine;
using System;

[ExecuteInEditMode]
public class ElementBuilder : MonoBehaviour
{
    public enum ElementType {None = -1, Block, Interactable, Pickup, Player };

    public ElementListSO[] sourceLists = new ElementListSO[Enum.GetNames(typeof(ElementType)).Length - 1];

    public ElementType type = ElementType.None;

    private ElementType   m_type = ElementType.None;
    private GameObject    m_curElement;

    public ElementType GetElementType() { return m_type; }
    public void SetElementType(ElementType type) 
    { 
        this.type = type; 
        OnValidate();
    }

    void OnTypeChanged(ElementType type)
    {
        m_type = type;

        // If the type was ElementType.None before, m_curElement is null
        if (m_curElement != null)
        {
            DestroyImmediate(m_curElement);
            m_curElement = null;
        }

        // If the new type is ElementType.None, set the current element to null 
        // after it was just destroyed. 
        if (m_type == ElementType.None)
        {
            m_curElement = null;
            return;
        }

        // SourceLists contains the lists for the scritable object templates of 
        // each type. If the list of the selected type is empty return.
        if(sourceLists[(int)m_type].elementList.Length == 0)
        {
            Debug.Log("Element list for type " + m_type + " is empty.");
            return;
        }

        // Now we know that the element list of the selected type is not empty, 
        // the selected type is something other than ElementType.None and
        // m_curElement is set to null. Next we create a new Element by using the 
        // method provided by the Scriptable object of the class corresponding to
        // the type m_type. 
        m_curElement = sourceLists[(int)m_type].elementList[0].CreateElement();
        m_curElement.transform.position = transform.position;
        m_curElement.transform.SetParent(transform);
        m_curElement.AddComponent<SelectParent>();
    }

    private void OnValidate()
    {
        if (type != m_type)
        {
            OnTypeChanged(type);
        }
    }
}
