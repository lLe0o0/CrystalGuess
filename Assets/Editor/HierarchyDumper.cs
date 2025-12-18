using UnityEngine;
using UnityEditor;
using System.Text;

public class HierarchyDumper : MonoBehaviour
{
    [MenuItem("Tools/Dump Hierarchy (Inspector Mode)")]
    static void DumpHierarchy()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("Zaznacz obiekt w hierarchii!");
            return;
        }

        StringBuilder sb = new StringBuilder();
        DumpRecursive(selected, sb, "");
        
        // Wypisanie wyniku
        Debug.Log($"--- DUMP START: {selected.name} ---\n" + sb.ToString());
    }

    static void DumpRecursive(GameObject obj, StringBuilder sb, string indent)
    {
        sb.AppendLine($"{indent}► {obj.name} (Active: {obj.activeSelf})");

        Component[] components = obj.GetComponents<Component>();
        foreach (var component in components)
        {
            if (component == null) continue;

            string compName = component.GetType().Name;
            sb.AppendLine($"{indent}   [{compName}]");

            // Używamy SerializedObject - to czyta dane tak jak Inspektor
            SerializedObject so = new SerializedObject(component);
            SerializedProperty prop = so.GetIterator();

            // Przejdź przez wszystkie widoczne właściwości
            bool enterChildren = true;
            while (prop.NextVisible(enterChildren))
            {
                enterChildren = false; // Wchodzimy głębiej tylko przy pierwszym elemencie, potem NextVisible sam ogarnia strukturę

                // Pomijamy "m_Script", bo to tylko referencja do samego pliku skryptu
                if (prop.name == "m_Script") continue;

                string valueStr = GetValueString(prop);
                sb.AppendLine($"{indent}     - {prop.displayName}: {valueStr}");
            }
        }

        foreach (Transform child in obj.transform)
        {
            DumpRecursive(child.gameObject, sb, indent + "  ");
        }
    }

    // Pomocnicza funkcja do wyciągania wartości w formie tekstu
    static string GetValueString(SerializedProperty prop)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer: return prop.intValue.ToString();
            case SerializedPropertyType.Boolean: return prop.boolValue.ToString();
            case SerializedPropertyType.Float: return prop.floatValue.ToString("F2");
            case SerializedPropertyType.String: return $"\"{prop.stringValue}\"";
            case SerializedPropertyType.Color: return prop.colorValue.ToString();
            case SerializedPropertyType.ObjectReference:
                return prop.objectReferenceValue ? $"[{prop.objectReferenceValue.name}]" : "null";
            case SerializedPropertyType.Enum:
                // Próbujemy pobrać nazwę enuma, jeśli się da
                if (prop.enumValueIndex >= 0 && prop.enumValueIndex < prop.enumDisplayNames.Length)
                    return prop.enumDisplayNames[prop.enumValueIndex];
                return prop.enumValueIndex.ToString();
            case SerializedPropertyType.Vector2: return prop.vector2Value.ToString();
            case SerializedPropertyType.Vector3: return prop.vector3Value.ToString();
            case SerializedPropertyType.Rect: return prop.rectValue.ToString();
            default: return "(Complex/Other)";
        }
    }
}