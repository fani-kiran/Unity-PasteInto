using UnityEngine;
using UnityEditor;

public class PasteIntoEditor : EditorWindow
{
    // Acts as our custom internal clipboard
    private static GameObject customCopiedObject;

    /// <summary>
    /// Step 1: Copy the object you want to duplicate.
    /// Added shortcut: Ctrl+Alt+C (Cmd+Option+C on Mac)
    /// </summary>
    [MenuItem("Edit/Copy for Paste Into %&c")]
    public static void CopyObject()
    {
        if (Selection.activeGameObject != null)
        {
            customCopiedObject = Selection.activeGameObject;
            Debug.Log($"<b>[Paste Into]</b> Copied '{customCopiedObject.name}' to custom clipboard.");
        }
    }

    /// <summary>
    /// Validation: Only enable the Copy menu item if an object is actually selected.
    /// </summary>
    [MenuItem("Edit/Copy for Paste Into %&c", true)]
    public static bool ValidateCopyObject()
    {
        return Selection.activeGameObject != null;
    }

    /// <summary>
    /// Step 2: Paste the copied object into all currently selected objects.
    /// Added shortcut: Ctrl+Alt+V (Cmd+Option+V on Mac)
    /// </summary>
    [MenuItem("Edit/Paste Into %&v")]
    public static void PasteInto()
    {
        if (customCopiedObject == null)
        {
            Debug.LogWarning("[Paste Into] No object copied! Use 'Edit > Copy for Paste Into' first.");
            return;
        }

        GameObject[] selectedTargets = Selection.gameObjects;

        if (selectedTargets.Length == 0)
        {
            Debug.LogWarning("[Paste Into] No target objects selected to paste into.");
            return;
        }

        foreach (GameObject target in selectedTargets)
        {
            // Attempt to keep prefab connections if the copied object is a prefab
            GameObject pastedObject = null;
            if (PrefabUtility.IsPartOfAnyPrefab(customCopiedObject))
            {
                Object prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(customCopiedObject);
                if (prefabAsset != null)
                {
                    pastedObject = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
                }
            }

            // Fallback to standard instantiation if it's a normal scene object
            if (pastedObject == null)
            {
                pastedObject = Instantiate(customCopiedObject);
            }

            // Clean up the name (removes the "(Clone)" suffix)
            pastedObject.name = customCopiedObject.name;

            // Parent it to the selected object and reset its local transform
            pastedObject.transform.SetParent(target.transform);
            pastedObject.transform.localPosition = Vector3.zero;
            pastedObject.transform.localRotation = Quaternion.identity;
            pastedObject.transform.localScale = customCopiedObject.transform.localScale;

            // Register this action with Unity's Undo system so Ctrl+Z works safely
            Undo.RegisterCreatedObjectUndo(pastedObject, "Paste Into");
        }
        
        Debug.Log($"<b>[Paste Into]</b> Successfully pasted '{customCopiedObject.name}' into {selectedTargets.Length} object(s).");
    }

    /// <summary>
    /// Validation: Only enable the Paste menu item if we have targets selected AND something in the clipboard.
    /// </summary>
    [MenuItem("Edit/Paste Into %&v", true)]
    public static bool ValidatePasteInto()
    {
        return Selection.gameObjects.Length > 0 && customCopiedObject != null;
    }
}