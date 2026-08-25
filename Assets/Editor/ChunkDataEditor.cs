using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ChunkData))]
public class ChunkDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ChunkData chunk = (ChunkData)target;

        serializedObject.Update();

        GUILayout.Label("Paramètres du Chunk", EditorStyles.boldLabel);
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("category"), new GUIContent("Catégorie"));
        
        GUILayout.Space(5);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("largeur"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hauteur"));

        GUILayout.Space(10);
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("sockets"), new GUIContent("Points d'Intérêt (Portes, Spawns, Base)"), true);

        GUILayout.Space(15);

        if (GUILayout.Button("Mettre à jour / Reset la grille locale", GUILayout.Height(30)))
        {
            chunk.GenererGrilleVide();
            EditorUtility.SetDirty(chunk);
        }

        if (chunk.elevationMap == null || chunk.elevationMap.Length != chunk.hauteur)
        {
            EditorGUILayout.HelpBox("Cliquez sur le bouton ci-dessus pour générer la grille de ce Chunk.", MessageType.Warning);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        GUILayout.Space(15);

        GUILayout.Label("Élévation Locale (0 = Chemin, 1+ = Montagne)", EditorStyles.boldLabel);
        DessinerGrilleExcel(chunk.elevationMap, chunk.largeur, chunk.hauteur);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(chunk);
        }
        
        serializedObject.ApplyModifiedProperties();
    }

    private void DessinerGrilleExcel(ChunkRow[] grid, int largeur, int hauteur)
    {
        GUIStyle cellStyle = new GUIStyle(GUI.skin.textField)
        {
            alignment = TextAnchor.MiddleCenter
        };

        for (int y = hauteur - 1; y >= 0; y--) // Affichage de haut en bas
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