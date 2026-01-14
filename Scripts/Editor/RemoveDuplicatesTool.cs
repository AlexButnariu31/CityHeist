using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GeometryDuplicateRemover : EditorWindow
{
    [MenuItem("Tools/Geometry Duplicate Remover")]
    public static void ShowWindow()
    {
        GetWindow<GeometryDuplicateRemover>("Geo Cleaner");
    }

    private float tolerance = 0.001f; // Marja de eroare (1 milimetru)
    private bool checkMesh = true;    // Verifica daca au aceeasi forma 3D

    void OnGUI()
    {
        GUILayout.Label("Curatare Bazata pe Geometrie", EditorStyles.boldLabel);
        GUILayout.Label("Ignora numele. Verifica doar pozitia si forma.", EditorStyles.miniLabel);

        GUILayout.Space(10);
        tolerance = EditorGUILayout.FloatField("Toleranta Pozitie (m)", tolerance);
        checkMesh = EditorGUILayout.Toggle("Verifica Mesh (Forma)", checkMesh);

        GUILayout.Space(20);

        if (GUILayout.Button("DETECTEAZA SI STERGE (Strict)"))
        {
            RemoveDuplicates();
        }
        GUILayout.Label("Suporta Undo (Ctrl+Z)", EditorStyles.centeredGreyMiniLabel);
    }

    void RemoveDuplicates()
    {
        // 1. Luam TOATE obiectele din scena care au Renderer (sunt vizibile)
        // Daca vrei sa verifici si obiecte invizibile (lumini, sunet), scoate filtrarea asta
        MeshFilter[] allMeshes = FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);

        List<GameObject> toDelete = new List<GameObject>();

        // Folosim o lista pentru a marca obiectele deja procesate ca sa nu le verificam de 2 ori
        HashSet<GameObject> processedObjects = new HashSet<GameObject>();

        int count = 0;

        // Trecem prin fiecare obiect (Bucla A)
        for (int i = 0; i < allMeshes.Length; i++)
        {
            GameObject objA = allMeshes[i].gameObject;
            if (toDelete.Contains(objA) || processedObjects.Contains(objA)) continue;

            // Il comparam cu restul obiectelor (Bucla B)
            for (int j = i + 1; j < allMeshes.Length; j++)
            {
                GameObject objB = allMeshes[j].gameObject;
                if (toDelete.Contains(objB)) continue;

                // --- TESTUL SUPREM ---
                if (AreIdentical(objA, objB, allMeshes[i], allMeshes[j]))
                {
                    // Am gasit o dublura perfecta!
                    // Il marcam pe B pentru stergere si il adaugam la procesate
                    toDelete.Add(objB);
                    processedObjects.Add(objB);
                }
            }
            processedObjects.Add(objA);
        }

        // Executam stergerea
        if (toDelete.Count > 0)
        {
            foreach (GameObject obj in toDelete)
            {
                Undo.DestroyObjectImmediate(obj);
                count++;
            }
            Debug.Log($"<color=red><b>Curatenie Gata!</b></color> S-au sters {count} obiecte care se suprapuneau perfect.");
        }
        else
        {
            Debug.Log("Nu s-au gasit obiecte suprapuse.");
        }
    }

    bool AreIdentical(GameObject a, GameObject b, MeshFilter meshA, MeshFilter meshB)
    {
        // 1. Verificare Distanta (Pozitie)
        if (Vector3.Distance(a.transform.position, b.transform.position) > tolerance) return false;

        // 2. Verificare Rotatie (Unghi)
        if (Quaternion.Angle(a.transform.rotation, b.transform.rotation) > 1.0f) return false;

        // 3. Verificare Scara (Marime)
        if (Vector3.Distance(a.transform.localScale, b.transform.localScale) > tolerance) return false;

        // 4. Verificare Forma (Mesh) - CRUCIAL
        // Asta previne stergerea unui Cub daca e in acelasi loc cu o Sfera
        if (checkMesh)
        {
            if (meshA.sharedMesh != meshB.sharedMesh) return false;
        }

        return true;
    }
}