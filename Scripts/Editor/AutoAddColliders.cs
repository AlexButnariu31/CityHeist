using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AutoAddColliders : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Add Colliders to All Objects")]
    static void AddCollidersToAll()
    {
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int count = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.GetComponent<Collider>() == null && obj.GetComponent<Rigidbody>() == null)
            {
                MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    MeshCollider meshCollider = obj.AddComponent<MeshCollider>();
                    meshCollider.convex = false;
                    count++;
                }
                else if (obj.GetComponent<Renderer>() != null)
                {
                    obj.AddComponent<BoxCollider>();
                    count++;
                }
            }
        }
        
        Debug.Log($"Adaugate collidere la {count} obiecte!");
    }
    
    [MenuItem("Tools/Add Colliders to Selected")]
    static void AddCollidersToSelected()
    {
        int count = 0;
        
        foreach (GameObject obj in Selection.gameObjects)
        {
            if (obj.GetComponent<Collider>() == null && obj.GetComponent<Rigidbody>() == null)
            {
                MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    MeshCollider meshCollider = obj.AddComponent<MeshCollider>();
                    meshCollider.convex = false; 
                    count++;
                }
                else if (obj.GetComponent<Renderer>() != null)
                {
                    obj.AddComponent<BoxCollider>(); 
                    count++;
                }
            }
            
            foreach (Transform child in obj.GetComponentsInChildren<Transform>())
            {
                if (child == obj.transform) continue; 
                if (child.GetComponent<Collider>() == null && child.GetComponent<Rigidbody>() == null)
                {
                    MeshFilter meshFilter = child.GetComponent<MeshFilter>();
                    
                    if (meshFilter != null && meshFilter.sharedMesh != null)
                    {
                        MeshCollider meshCollider = child.gameObject.AddComponent<MeshCollider>();
                        meshCollider.convex = false;
                        count++;
                    }
                    else if (child.GetComponent<Renderer>() != null)
                    {
                        child.gameObject.AddComponent<BoxCollider>();
                        count++;
                    }
                }
            }
        }
        
        Debug.Log($"Adaugate collidere la {count} obiecte selectate (si copiii lor)!");
    }
    
    [MenuItem("Tools/Remove All Colliders")]
    static void RemoveAllColliders()
    {
        if (EditorUtility.DisplayDialog("Confirmare", 
            "Sigur vrei sa stergi TOATE colliderele din scena?", 
            "Da", "Nu"))
        {
            Collider[] allColliders = Object.FindObjectsByType<Collider>(FindObjectsSortMode.None);
            int count = allColliders.Length;
            
            foreach (Collider col in allColliders)
            {
                DestroyImmediate(col);
            }
            
            Debug.Log($"Sterse {count} collidere!");
        }
    }
#endif
}