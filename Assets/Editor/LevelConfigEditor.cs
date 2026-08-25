using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelConfig))]
public class LevelConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        LevelConfig config = (LevelConfig)target;
        serializedObject.Update();

        GUILayout.Label("Paramètres Principaux", EditorStyles.boldLabel);
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("largeur"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hauteur"));
        
        GUILayout.Space(5);
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("endCoords"), new GUIContent("Arrivée (X, Y)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnPoints"), new GUIContent("Points de Départ"), true);

        GUILayout.Space(15);

        if (GUILayout.Button("Mettre à jour / Reset les tableaux", GUILayout.Height(30)))
        {
            config.GenererGrilleVide();
            EditorUtility.SetDirty(config);
        }

        if (config.elevationMap == null || config.chunkMap == null || config.elevationMap.Length != config.hauteur || config.chunkMap.Length != config.hauteur)
        {
            EditorGUILayout.HelpBox("Cliquez sur le bouton ci-dessus pour générer les grilles.", MessageType.Warning);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        GUILayout.Space(15);

        GUILayout.Label("Élévation Map (0 = Chemin, 1+ = Montagne)", EditorStyles.boldLabel);
        DessinerGrilleExcel(config.elevationMap, config.largeur, config.hauteur);

        GUILayout.Space(15);

        GUILayout.Label("Chunk Map (1 = Bloc Départ, 2+ = Vagues Suivantes, 0 = Vide)", EditorStyles.boldLabel);
        DessinerGrilleExcel(config.chunkMap, config.largeur, config.hauteur);

        if (GUI.changed) EditorUtility.SetDirty(config);
        serializedObject.ApplyModifiedProperties();
    }

    private void DessinerGrilleExcel(GridRow[] grid, int largeur, int hauteur)
    {
        GUIStyle cellStyle = new GUIStyle(GUI.skin.textField) { alignment = TextAnchor.MiddleCenter };

        for (int y = hauteur - 1; y >= 0; y--)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Y " + y, GUILayout.Width(30));

            for (int x = 0; x < largeur; x++)
            {
                if (grid[y].cells == null || grid[y].cells.Length != largeur) break;
                grid[y].cells[x] = EditorGUILayout.IntField(grid[y].cells[x], cellStyle, GUILayout.Width(25));
            }
            GUILayout.EndHorizontal();
        }
    }
}