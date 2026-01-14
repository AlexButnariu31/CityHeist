using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class RemoveDuplicatesTool : EditorWindow
{
    // Adaugam un meniu in Unity: Tools -> Remove Duplicates
    [MenuItem("Tools/Remove Duplicates")]
    public static void ShowWindow()
    {
        GetWindow<RemoveDuplicatesTool>("Remove Duplicates");
    }

    // Variabile pentru setari
    private float positionTolerance = 0.001f; // Marja de eroare pentru pozitie
    private bool checkRotation = true;
    private bool checkScale = true;

    void OnGUI()
    {
        GUILayout.Label("Setari Curatare", EditorStyles.boldLabel);

        positionTolerance = EditorGUILayout.FloatField("Toleranta Pozitie", positionTolerance);
        checkRotation = EditorGUILayout.Toggle("Verifica Rotatia", checkRotation);
        checkScale = EditorGUILayout.Toggle("Verifica Scara (Scale)", checkScale);

        GUILayout.Space(20);

        if (GUILayout.Button("STERGE DUBLURILE (Cu Undo)"))
        {
            RemoveDuplicates();
        }

        GUILayout.Space(10);
        GUILayout.Label("NOTA: Aceasta actiune suporta Undo (Ctrl+Z).", EditorStyles.miniLabel);
    }

    void RemoveDuplicates()
    {
        // Gasim toate obiectele din scena activa
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        // Listele pentru procesare
        List<GameObject> objectsToDestroy = new List<GameObject>();
        // Folosim un Dictionar pentru a tine minte obiectele unice
        // Cheia va fi o semnatura unica (Nume + Pozitie)
        HashSet<string> seenSignatures = new HashSet<string>();

        int count = 0;

        foreach (GameObject obj in allObjects)
        {
            // Ignoram obiectele care sunt Asset-uri (nu sunt in scena) sau sunt deja marcate
            if (obj.scene.name == null) continue;

            // Generam o "semnatura" unica pentru obiect
            // Rotunjim pozitia pentru a evita erorile de float (0.00001 vs 0.00000)
            string signature = obj.name;

            // Adaugam Pozitia la semnatura
            signature += $"_P:{Round(obj.transform.position.x)}_{Round(obj.transform.position.y)}_{Round(obj.transform.position.z)}";

            // Adaugam Rotatia (optional)
            if (checkRotation)
                signature += $"_R:{Round(obj.transform.rotation.eulerAngles.x)}_{Round(obj.transform.rotation.eulerAngles.y)}_{Round(obj.transform.rotation.eulerAngles.z)}";

            // Adaugam Scara (optional)
            if (checkScale)
                signature += $"_S:{Round(obj.transform.localScale.x)}_{Round(obj.transform.localScale.y)}_{Round(obj.transform.localScale.z)}";

            // VERIFICARE
            if (seenSignatures.Contains(signature))
            {
                // Daca semnatura exista deja, inseamna ca acest obiect e o dublura
                objectsToDestroy.Add(obj);
            }
            else
            {
                // Daca nu, il adaugam la lista celor "vazute"
                seenSignatures.Add(signature);
            }
        }

        // Executam stergerea cu Undo
        if (objectsToDestroy.Count > 0)
        {
            foreach (GameObject obj in objectsToDestroy)
            {
                // Verificam daca nu cumva a fost sters deja (fiind copilul altui obiect sters)
                if (obj != null)
                {
                    Undo.DestroyObjectImmediate(obj);
                    count++;
                }
            }
            Debug.Log($"<color=green>Succes!</color> Au fost sterse {count} dubluri.");
        }
        else
        {
            Debug.Log("Nu au fost gasite dubluri.");
        }
    }

    // Functie ajutatoare pentru rotunjire
    float Round(float value)
    {
        return Mathf.Round(value / positionTolerance) * positionTolerance;
    }
}