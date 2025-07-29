using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapData))]
public class MapDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapData mapData = (MapData)target;

        if (GUILayout.Button("Validate Grid By NavMesh"))
        {
            ValidateGridByNavMesh(mapData);
        }
    }

    private void ValidateGridByNavMesh(MapData mapData)
    {
        if (mapData == null)
        {
            Debug.LogError("MapData is null.");
            return;
        }

        mapData.ValidateGridByNavMesh();
    }
}
