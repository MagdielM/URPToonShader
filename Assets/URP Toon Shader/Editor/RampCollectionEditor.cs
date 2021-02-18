using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace ToonShaderURP
{
    public class RampCollectionEditor : EditorWindow
    {
        private const int verticalWindowPadding = 8;
        private const int horizontalWindowPadding = 12;
        private const int defaultRampResolution = 256;
        private const int defaultRowHeight = 3;

        #region Ramp Collection Fields
        private RampCollectionData rampCollectionRaw;
        private SerializedObject rampCollection;
        private SerializedProperty resolution;
        private SerializedProperty height;
        private SerializedProperty ramps;
        #endregion

        #region GUIContent Fields
        private GUIContent rampCollectionLabel = new GUIContent("Ramp Collection",
            "The ramp array asset to be edited.");
        private GUIContent rampCollectionNameLabel = new GUIContent("Ramp Collection Name",
            "The name to give the new ramp collection asset. If left blank, the new asset " +
            "will be given a default name.");
        private GUIContent createNewCollectionLabel = new GUIContent("Create New Collection",
            "Create a new ramp collection asset with the given name.");
        private GUIContent rampResolutionLabel = new GUIContent("Ramp Resolution",
            "The pixel width of all ramps in the generated texture array. Higher values will " +
            "yield higher precision, but larger textures.");
        private GUIContent rowHeightLabel = new GUIContent("Row Height",
            "The height of each ramp row, in pixels.");
        private GUIContent saveRampTextureArrayLabel = new GUIContent("Save Ramp Texture Array",
            "Generate a new Texture2DArray asset from all the ramps in the collection.");
        private GUIContent saveRampTextureAtlasLabel = new GUIContent("Save Ramp Texture Atlas",
            "Generate a new Texture2D asset from all the ramps in the collection, each taking " +
            "up one pixel row.");
        #endregion

        private string rampCollectionName;
        private const string defaultRampTextureArrayName = "New Ramp Texture Array";
        private const string defaultRampTextureAtlasName = "New Ramp Texture Atlas";
        private string rampCollectionPath;

        [MenuItem("Window/Ramp Collection Editor")]
        private static RampCollectionEditor Init()
        {
            RampCollectionEditor window = (RampCollectionEditor)GetWindow(typeof(RampCollectionEditor));
            window.titleContent = new GUIContent("Ramp Collection Editor");
            window.minSize = new Vector2(600, 600);
            return window;

        }

        [OnOpenAsset]
        public static bool OpenWindowWithAsset(int instanceID, int line)
        {
            if (EditorUtility.InstanceIDToObject(instanceID) is RampCollectionData collection)
            {
                RampCollectionEditor editor = Init();
                editor.rampCollectionRaw = collection;
                return true;
            }
            return false;
        }

        private void OnGUI()
        {
            GUILayout.Space(verticalWindowPadding);
            using (var c = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(horizontalWindowPadding);
                using (var a = new EditorGUILayout.VerticalScope())
                {
                    // Ramp Collection Field
                    EditorGUI.BeginChangeCheck();
                    rampCollectionRaw = (RampCollectionData)EditorGUILayout.ObjectField(rampCollectionLabel,
                        rampCollectionRaw, typeof(RampCollectionData), false);
                    if (rampCollectionRaw != null)
                    {
                        // Create serialized asset mirror if missing
                        if (EditorGUI.EndChangeCheck() || rampCollection == null)
                        {
                            rampCollection = new SerializedObject(rampCollectionRaw);
                            resolution = rampCollection.FindProperty(nameof(RampCollectionData.resolution));
                            height = rampCollection.FindProperty(nameof(RampCollectionData.height));
                            ramps = rampCollection.FindProperty(nameof(RampCollectionData.ramps));
                        }
                    }
                    else
                    {
                        EditorGUI.EndChangeCheck();
                        EditorGUILayout.Space(8);
                        using (var b = new EditorGUILayout.HorizontalScope())
                        {
                            rampCollectionName = EditorGUILayout.TextField(rampCollectionNameLabel, rampCollectionName);
                            GUILayout.Space(32);
                            if (GUILayout.Button(createNewCollectionLabel))
                            {
                                rampCollectionPath = "Assets/Toon Shader URP/Ramp Collections";
                                rampCollectionRaw = CreateInstance<RampCollectionData>();
                                if (rampCollectionName == null || rampCollectionName.Length == 0)
                                {
                                    rampCollectionName = "New Ramp Collection";
                                    rampCollectionRaw.name = rampCollectionName;
                                }
                                Directory.CreateDirectory($"{Application.dataPath}/Toon Shader URP/Ramp Collections");
                                AssetDatabase.CreateAsset(rampCollectionRaw,
                                    $"{rampCollectionPath}/{rampCollectionName}.asset");
                                rampCollection = new SerializedObject(rampCollectionRaw);
                                SerializedProperty name = rampCollection.FindProperty(nameof(RampCollectionData.arrayName));
                                resolution = rampCollection.FindProperty(nameof(RampCollectionData.resolution));
                                height = rampCollection.FindProperty(nameof(RampCollectionData.height));
                                ramps = rampCollection.FindProperty(nameof(RampCollectionData.ramps));

                                name.stringValue = rampCollectionName;
                                resolution.intValue = defaultRampResolution;
                                height.intValue = defaultRowHeight;
                                rampCollection.ApplyModifiedProperties();
                            }
                            else
                            {
                                rampCollection = null;
                                ramps = null;
                            }
                        }
                        EditorGUILayout.Space(8);
                    }

                    if (rampCollection != null && ramps != null)
                    {
                        rampCollection.UpdateIfRequiredOrScript();
                        resolution.intValue = EditorGUILayout.IntField(rampResolutionLabel, resolution.intValue);
                        height.intValue = Mathf.Clamp(EditorGUILayout.IntField(rowHeightLabel, height.intValue), 1, int.MaxValue);
                        EditorGUILayout.PropertyField(ramps);
                        rampCollection.ApplyModifiedProperties();

                        if (Event.current.type == EventType.MouseUp
                            || (Event.current.type == EventType.KeyUp
                                && Event.current.keyCode == KeyCode.Return))
                        {
                            AssetDatabase.SaveAssets();
                        }

                        using (var d = new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button(saveRampTextureArrayLabel))
                            {
                                Texture2DArray rasterizedRampArray = new Texture2DArray(
                                    resolution.intValue, 1, ramps.arraySize, TextureFormat.RGBA32, false) {
                                    wrapMode = TextureWrapMode.Clamp,
                                    filterMode = FilterMode.Bilinear,
                                    anisoLevel = 0
                                };
                                for (int i = 0; i < ramps.arraySize; ++i)
                                {
                                    Color32[] arrayRamp = new Color32[resolution.intValue];
                                    for (int j = 0; j < resolution.intValue; ++j)
                                    {
                                        arrayRamp[j] = rampCollectionRaw.ramps[i].Evaluate((float)j / resolution.intValue);
                                    }
                                    rasterizedRampArray.SetPixels32(arrayRamp, i);
                                }
                                rasterizedRampArray.Apply();
                                string path = EditorUtility.SaveFilePanelInProject(
                                    "Save Texture Array", defaultRampTextureArrayName, "asset",
                                    "Select a location to save your generated texture array in.");

                                if (path.Length != 0)
                                {
                                    AssetDatabase.CreateAsset(rasterizedRampArray, path);
                                }
                            }
                            else if (GUILayout.Button(saveRampTextureAtlasLabel))
                            {
                                Texture2D rasterizedRampAtlas = new Texture2D(
                                    resolution.intValue, ramps.arraySize * height.intValue, TextureFormat.RGBA32, false) {
                                    wrapMode = TextureWrapMode.Clamp,
                                    filterMode = FilterMode.Bilinear,
                                    anisoLevel = 0,
                                };
                                for (int i = 0; i < ramps.arraySize; ++i)
                                {
                                    Color32[] rampRow = new Color32[resolution.intValue];
                                    for (int j = 0; j < resolution.intValue; ++j)
                                    {
                                        rampRow[j] = rampCollectionRaw.ramps[i].Evaluate((float)j / resolution.intValue);
                                    }
                                    for (int k = 0; k < height.intValue; ++k)
                                    {
                                        rasterizedRampAtlas.SetPixels32(0, k + (i * height.intValue), resolution.intValue, 1, rampRow);
                                    }
                                }
                                rasterizedRampAtlas.Apply();
                                string path = EditorUtility.SaveFilePanelInProject(
                                    "Save Texture Atlas", defaultRampTextureAtlasName, "png",
                                    "Select a location to save your generated texture atlas in.");

                                if (path.Length != 0)
                                {
                                    string localPath = path.Substring("Assets".Length);
                                    RampAtlasPreprocessor.isRampTexture = true;
                                    File.WriteAllBytes($"{Application.dataPath}/{localPath}", rasterizedRampAtlas.EncodeToPNG());
                                }
                            }
                        }
                    }
                }
                GUILayout.Space(horizontalWindowPadding);
            }
            GUILayout.Space(verticalWindowPadding);
        }

        private void OnDestroy()
        {
            EditorUtility.UnloadUnusedAssetsImmediate();
        }
    }
}