using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainCreator))]
public class TerrainCreatorEditor : Editor
{
    override public void OnInspectorGUI()
    {
        //serializedObject.Update
        TerrainCreator terrainCreator = (TerrainCreator)target;
        if (GUILayout.Button("Build Terrain"))
        {
            terrainCreator.BuildTerrain();
        }
        DrawDefaultInspector();
    }
}
