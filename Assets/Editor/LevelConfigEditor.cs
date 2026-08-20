using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelConfig))]
public class LevelConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        LevelConfig config = (LevelConfig)target;
        EditorGUI.BeginChangeCheck();

        GUILayout.Label("Paramètres Principaux", EditorStyles.boldLabel);
        config.largeur = EditorGUILayout.IntField("Largeur", config.largeur);
        config.hauteur = EditorGUILayout.IntField("Hauteur", config.hauteur);
        config.startCoords = EditorGUILayout.Vector2IntField("Départ (X, Y)", config.startCoords);
        config.endCoords = EditorGUILayout.Vector2IntField("Arrivée (X, Y)", config.endCoords);

        GUILayout.Space(15);
        GUILayout.Label("Éditeur de Carte (façon Excel)", EditorStyles.boldLabel);

        if (GUILayout.Button("Mettre à jour la taille du tableau", GUILayout.Height(30)))
        {
            config.GenererGrilleVide();
        }

        GUILayout.Space(10);

        if (config.elevationMap != null && config.elevationMap.Length == config.hauteur)
        {
            for (int y = config.hauteur - 1; y >= 0; y--)
            {
                GUILayout.BeginHorizontal(); 
                GUILayout.Label($"Y {y}", GUILayout.Width(25)); 

                for (int x = 0; x < config.largeur; x++)
                {
                    if (config.elevationMap[y].cells != null && config.elevationMap[y].cells.Length == config.largeur)
                    {
                        config.elevationMap[y].cells[x] = EditorGUILayout.IntField(config.elevationMap[y].cells[x], GUILayout.Width(20));
                    }
                }
                GUILayout.EndHorizontal(); 
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Cliquez sur le bouton ci-dessus pour générer le tableau.", MessageType.Warning);
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(config);
        }
    }
}