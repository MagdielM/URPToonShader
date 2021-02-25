using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ToonShaderURP
{
    public class ToonLitShaderGUI : ShaderGUI
    {
        private Material targetMat;
        private GUIState guiState;

        #region Shader Properties
        private MaterialProperty baseMap;
        private MaterialProperty baseMapTint;
        private MaterialProperty normalMap;
        private MaterialProperty specularMap;
        private MaterialProperty rampAtlas;
        private MaterialProperty rampArray;

        #region Shader Toggles
        private MaterialProperty useNormalMap;
        private ShaderFloatToggleMirror specularHighlighting;
        private ShaderFloatToggleMirror rimLighting;
        private ShaderFloatToggleMirror useRampArray;
        private ShaderFloatToggleMirror useAlphaClipping;
        #endregion

        #region Numeric Properties
        private List<ShaderVectorMirror> curvePoints = new List<ShaderVectorMirror>();
        private List<ShaderVectorMirror> curveValues = new List<ShaderVectorMirror>();
        #endregion

        #region Float Properties
        private ShaderFloatMirror alphaClipThreshold;
        private ShaderFloatMirror smoothness;
        private ShaderFloatMirror specularOpacity;
        private ShaderFloatMirror specularBrightening;
        private ShaderFloatMirror rimFactor;
        private ShaderFloatMirror rimLightOpacity;
        private ShaderFloatMirror rimBrightening;
        #endregion

        #region Integer Properties
        private ShaderIntMirror diffuseRampIndex;
        private ShaderIntMirror specularRampIndex;
        private ShaderIntMirror rimRampIndex;
        private ShaderIntMirror rampRowHeight;
        private ShaderIntMirror curveStepCount;
        #endregion

        #endregion

        private int rampCount;
        private int rowHeight;
        private AnimationCurve rimAngleCurve;

        private static readonly Rect rimCurveRange = new Rect(0, 0, 1, 1);

        #region GUIContent
        private readonly GUIContent settingsHeaderLabel = new GUIContent("Shader Settings");
        private readonly GUIContent propertiesHeaderLabel = new GUIContent("General Properties");
        private readonly GUIContent useNormalMapLabel = new GUIContent("Use Normal Map");
        private readonly GUIContent specularHighlightingLabel = new GUIContent("Specular Highlighting");
        private readonly GUIContent rimLightingLabel = new GUIContent("Rim Lighting");
        private readonly GUIContent useRampArrayLabel = new GUIContent("Use Ramp Array");
        private readonly GUIContent useAlphaClippingLabel = new GUIContent("Alpha Clipping");
        private readonly GUIContent alphaClipThresholdLabel = new GUIContent("Alpha Clip Threshold");
        private readonly GUIContent albedoPropertiesLabel = new GUIContent("Albedo Properties");
        private readonly GUIContent baseMapLabel = new GUIContent("Base Map");
        private readonly GUIContent normalMapLabel = new GUIContent("Normal Map");
        private readonly GUIContent specularMapLabel = new GUIContent("Specular Map");
        private readonly GUIContent specularOpacityLabel = new GUIContent("Specular Opacity");
        private readonly GUIContent specularBrighteningLabel = new GUIContent("Specular Brightening");
        private readonly GUIContent rampAtlasLabel = new GUIContent("Ramp Atlas");
        private readonly GUIContent rampArrayLabel = new GUIContent("Ramp Array");
        private readonly GUIContent smoothnessLabel = new GUIContent("Smoothness");
        private readonly GUIContent rimFactorLabel = new GUIContent("Rim Factor");
        private readonly GUIContent rimLightOpacityLabel = new GUIContent("Rim Light Opacity");
        private readonly GUIContent rimBrighteningLabel = new GUIContent("Rim Brightening");
        private readonly GUIContent diffuseRampIndexLabel = new GUIContent("Diffuse Ramp Index");
        private readonly GUIContent specularRampIndexLabel = new GUIContent("Specular Ramp Index");
        private readonly GUIContent rimRampIndexLabel = new GUIContent("Rim Ramp Index");
        private readonly GUIContent rampRowHeightLabel = new GUIContent("Ramp Row Height");
        private readonly GUIContent rimAngleCurveLabel = new GUIContent("Rim View Angle Curve");
        #endregion

        private const string indexExceptionMessage = "Index outside of vector component range.";

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            switch (guiState)
            {
                case GUIState.Init:
                    Initialize();
                    return;

                case GUIState.PostInit:
                    PostInitialize();
                    break;
            }
            //============ Feature Toggles ============//

            EditorGUILayout.LabelField(settingsHeaderLabel, EditorStyles.boldLabel);
            EditorGUILayout.Space(1);
            materialEditor.ShaderProperty(useNormalMap, useNormalMapLabel); // Normal Map Toggle
            EditorGUILayout.Space(1);
            specularHighlighting.Value = EditorGUILayout.Toggle(specularHighlightingLabel, specularHighlighting); // Specular Toggle
            EditorGUILayout.Space(1);
            rimLighting.Value = EditorGUILayout.Toggle(rimLightingLabel, rimLighting); // Rim Light Toggle
            EditorGUILayout.Space(1);
            useRampArray.Value = EditorGUILayout.Toggle(useRampArrayLabel, useRampArray); // Use Texture Ramp Array Toggle
            EditorGUILayout.Space(1);
            useAlphaClipping.Value = EditorGUILayout.Toggle(useAlphaClippingLabel, useAlphaClipping); // Texture Array Toggle
            //EditorGUILayout.Space(4);
            //Rect lineRect = EditorGUILayout.GetControlRect(false, 1);
            //lineRect.x += 4;
            //lineRect.width -= 12;
            //EditorGUI.DrawRect(lineRect, new Color(0.35f, 0.35f, 0.35f, 1));
            //EditorGUILayout.Space(4);

            //materialEditor.RenderQueueField();
            //materialEditor.DoubleSidedGIField();

            EditorGUILayout.Space(12);

            //=============== Properties ==============//

            // General Properties
            materialEditor.SetDefaultGUIWidths();
            EditorGUILayout.LabelField(propertiesHeaderLabel, EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            if (useRampArray)
            {
                materialEditor.TexturePropertySingleLine(rampArrayLabel, rampArray); // Ramp Array
            }
            else
            {
                materialEditor.TexturePropertySingleLine(rampAtlasLabel, rampAtlas); // Ramp Atlas
            }
            if (useNormalMap.floatValue > 0.5)
            {
                materialEditor.TexturePropertySingleLine(normalMapLabel, normalMap); // Normal Map
            }
            if (!useRampArray) // Checked separately so normal map and ramp texture can be grouped
            {
                rowHeight = Mathf.Clamp(EditorGUILayout.IntField(rampRowHeightLabel, rampRowHeight), 1, int.MaxValue);
            }
            if (EditorGUI.EndChangeCheck()) // Set Ramp Count
            {
                rampRowHeight.Value = rowHeight;
                SetRampCount();
            }

            EditorGUILayout.Space(12);

            // Albedo Properties
            EditorGUILayout.LabelField(albedoPropertiesLabel, EditorStyles.boldLabel);
            EditorGUILayout.Space(1);
            materialEditor.TexturePropertySingleLine(baseMapLabel, baseMap, baseMapTint); // Base Map & Tint
            EditorGUIUtility.labelWidth = 0;
            materialEditor.TextureScaleOffsetProperty(baseMap); // Base Map Scale & Offset
            materialEditor.SetDefaultGUIWidths();

            EditorGUI.BeginChangeCheck();
            EditorGUIUtility.labelWidth = 0;
            RampIndexSlider(diffuseRampIndexLabel, diffuseRampIndex); // Diffuse Ramp Index
            if (EditorGUI.EndChangeCheck())
            {
                diffuseRampIndex.property.floatValue = diffuseRampIndex.Value;
            }

            if (useAlphaClipping)
            {
                materialEditor.ShaderProperty(alphaClipThreshold, alphaClipThresholdLabel); // Alpha Clip Threshold
            }
            materialEditor.SetDefaultGUIWidths();
            EditorGUILayout.Space(12);

            // Specular Properties
            if (specularHighlighting)
            {
                EditorGUILayout.LabelField(specularHighlightingLabel, EditorStyles.boldLabel);
                EditorGUILayout.Space(1);
                materialEditor.TexturePropertySingleLine(specularMapLabel, specularMap); // Specular Map

                EditorGUIUtility.labelWidth = 0;

                RampIndexSlider(specularRampIndexLabel, specularRampIndex); // Specular Ramp Index
                materialEditor.ShaderProperty(smoothness, smoothnessLabel); // Smoothness
                materialEditor.ShaderProperty(specularOpacity, specularOpacityLabel); // Specular Opacity
                materialEditor.ShaderProperty(specularBrightening, specularBrighteningLabel); // Specular Brightening Label

                materialEditor.SetDefaultGUIWidths();
                EditorGUILayout.Space(12);
            }

            // Rim Light Properties
            if (rimLighting)
            {
                EditorGUILayout.LabelField(rimLightingLabel, EditorStyles.boldLabel);
                EditorGUILayout.Space(1);

                EditorGUIUtility.labelWidth = 0;

                RampIndexSlider(rimRampIndexLabel, rimRampIndex); // Rim Ramp Index
                materialEditor.ShaderProperty(rimFactor, rimFactorLabel); // Rim Factor
                materialEditor.ShaderProperty(rimLightOpacity, rimLightOpacityLabel); // Rim Light Opacity
                materialEditor.ShaderProperty(rimBrightening, rimBrighteningLabel); // Rim Brightening


                EditorGUI.BeginChangeCheck();
                EditorGUILayout.CurveField(rimAngleCurveLabel, rimAngleCurve, Color.white, rimCurveRange);
                if (EditorGUI.EndChangeCheck())
                {
                    for (int i = 0, j = 0; i < curvePoints.Count && j < rimAngleCurve.length; ++i)
                    {
                        for (int k = 0; k < 4; ++k)
                        {
                            if (j >= rimAngleCurve.length) { break; }
                            curvePoints[i][k] = rimAngleCurve[j].time;
                            curveValues[i][k] = rimAngleCurve[j].value;

                            SetBothTangentsToLinear(j);
                            ++j;
                        }
                    }
                    curveStepCount.Value = rimAngleCurve.length;
                }
                materialEditor.SetDefaultGUIWidths();
            }


            void Initialize()
            {
                targetMat = (Material)materialEditor.target;

                #region Shader Settings
                useNormalMap = FindProperty("_UseNormalMap", properties);
                specularHighlighting = new ShaderFloatToggleMirror(FindProperty("_SpecularHighlighting", properties));
                rimLighting = new ShaderFloatToggleMirror(FindProperty("_RimLighting", properties));
                useRampArray = new ShaderFloatToggleMirror(FindProperty("_UseRampArray", properties));
                useAlphaClipping = new ShaderFloatToggleMirror(FindProperty("_UseAlphaClipping", properties));
                #endregion

                #region Textures
                baseMap = FindProperty("_BaseMap", properties);
                baseMapTint = FindProperty("_BaseMapTint", properties);
                normalMap = FindProperty("_NormalMap", properties);
                specularMap = FindProperty("_SpecularMap", properties);
                rampAtlas = FindProperty("_RampAtlas", properties);
                rampArray = FindProperty("_RampArray", properties);
                #endregion

                #region Ramp Indices
                diffuseRampIndex = new ShaderIntMirror(FindProperty("_DiffuseRampIndex", properties));
                specularRampIndex = new ShaderIntMirror(FindProperty("_SpecularRampIndex", properties));
                rimRampIndex = new ShaderIntMirror(FindProperty("_RimRampIndex", properties));
                #endregion

                #region Specular Properties
                smoothness = new ShaderFloatMirror(FindProperty("_Smoothness", properties));
                specularOpacity = new ShaderFloatMirror(FindProperty("_SpecularOpacity", properties));
                specularBrightening = new ShaderFloatMirror(FindProperty("_SpecularBrightening", properties));
                #endregion

                #region Rim Lighting Properties
                rimFactor = new ShaderFloatMirror(FindProperty("_RimFactor", properties));
                rimLightOpacity = new ShaderFloatMirror(FindProperty("_RimLightOpacity", properties));
                rimBrightening = new ShaderFloatMirror(FindProperty("_RimBrightening", properties));
                #endregion

                alphaClipThreshold = new ShaderFloatMirror(FindProperty("_AlphaClipThreshold", properties));
                rampRowHeight = new ShaderIntMirror(FindProperty("_RampRowHeight", properties));

                #region Rim Curve
                RepopulateCurveSteps();
                PopulateRimCurve();
                #endregion

                SetRampCount();

                guiState = GUIState.PostInit;
            }

            void SetRampCount()
            {
                if (useRampArray && rampArray.textureValue is Texture2DArray array)
                {
                    rampCount = array.depth;
                }
                else if (rampAtlas.textureValue is Texture2D atlas)
                {
                    rampCount = atlas.height / rampRowHeight;
                }
            }

            void PostInitialize()
            {
                Undo.undoRedoPerformed -= undoCallback;
                Undo.undoRedoPerformed += undoCallback;

                for (int i = 0; i < rimAngleCurve.length; ++i)
                {
                    SetBothTangentsToLinear(i);
                }

                guiState = GUIState.Running;

                Selection.selectionChanged += removeUndoCallback;

                void undoCallback()
                {
                    Initialize();
                }

                void removeUndoCallback()
                {
                    Undo.undoRedoPerformed -= undoCallback;
                    Selection.selectionChanged -= removeUndoCallback;
                }
            }

            void RepopulateCurveSteps()
            {
                curveStepCount = new ShaderIntMirror(FindProperty("_rCurveStepCount", properties));

                foreach (MaterialProperty prop in properties)
                {
                    if (prop.name.Contains("_rCurvePt"))
                    {
                        curvePoints.Add(new ShaderVectorMirror(prop, Shader.PropertyToID(prop.name)));
                    }
                    else if (prop.name.Contains("_rCurveVl"))
                    {
                        curveValues.Add(new ShaderVectorMirror(prop, Shader.PropertyToID(prop.name)));
                    }
                }
            }

            void RampIndexSlider(GUIContent label, ShaderIntMirror prop)
            {
                EditorGUI.BeginChangeCheck();
                prop.Value = EditorGUILayout.IntSlider(label, prop.Value, 0, rampCount - 1);
                if (EditorGUI.EndChangeCheck())
                {
                    prop.property.floatValue = prop.Value;
                }
            }
        }


        private void PopulateRimCurve()
        {
            rimAngleCurve = new AnimationCurve();
            for (int i = 0, j = 0; i < curvePoints.Count && j < curveStepCount; ++i)
            {
                for (int k = 0; k < 4; ++k)
                {
                    if (k >= curveStepCount) { break; }
                    rimAngleCurve.AddKey(new Keyframe(curvePoints[i][k], curveValues[i][k]));
                    ++j;
                }
            }

            for (int i = 0; i < rimAngleCurve.length; ++i)
            {
                SetBothTangentsToLinear(i);
            }
        }

        private void SetBothTangentsToLinear(int index)
        {
            AnimationUtility.SetKeyLeftTangentMode(rimAngleCurve, index, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(rimAngleCurve, index, AnimationUtility.TangentMode.Linear);
        }

        private enum GUIState
        {
            Init, PostInit, Running
        }

        private class ShaderFloatMirror
        {
            public MaterialProperty property;
            public float Value
            {
                get => property.floatValue;
                set => property.floatValue = Mathf.Clamp(value, min, max);
            }
            public float min = float.MinValue;
            public float max = float.MaxValue;
            public int id;

            public ShaderFloatMirror(MaterialProperty prop, int id = default)
            {
                property = prop ?? throw new ArgumentNullException(nameof(prop));
                switch (prop.type)
                {
                    case MaterialProperty.PropType.Range:
                        min = prop.rangeLimits.x;
                        max = prop.rangeLimits.y;
                        Value = prop.floatValue;
                        break;
                    case MaterialProperty.PropType.Float:
                        Value = prop.floatValue;
                        break;
                    default:
                        throw new ArgumentException("Property is not of type PropType.Float or PropType.Range.");
                }
                this.id = id;
            }

            public static implicit operator float(ShaderFloatMirror floatMirror) => floatMirror.Value;
            public static implicit operator MaterialProperty(ShaderFloatMirror floatMirror) => floatMirror.property;
        }
        private class ShaderFloatToggleMirror
        {
            public MaterialProperty property;
            public bool Value
            {
                get => property.floatValue > 0.5;
                set => property.floatValue = value ? 1 : 0;
            }
            public int id;

            public ShaderFloatToggleMirror(MaterialProperty prop, int id = default)
            {
                property = prop ?? throw new ArgumentNullException(nameof(prop));
                if (prop.type == MaterialProperty.PropType.Float)
                {
                    Value = prop.floatValue > 0.5;
                }
                else
                {
                    throw new ArgumentException("Property is not of type PropType.Float");
                }
                this.id = id;
            }

            public static implicit operator bool(ShaderFloatToggleMirror toggleMirror) => toggleMirror.Value;
            public static implicit operator MaterialProperty(ShaderFloatToggleMirror toggleMirror) => toggleMirror.property;
        }
        private class ShaderIntMirror
        {
            public MaterialProperty property;
            public int Value
            {
                get => (int)property.floatValue;
                set => property.floatValue = value;
            }
            public int id;

            public ShaderIntMirror(MaterialProperty prop, int id = default)
            {
                property = prop ?? throw new ArgumentNullException(nameof(prop));
                Value = prop.type == MaterialProperty.PropType.Float ?
                    Mathf.RoundToInt(prop.floatValue) : throw new ArgumentException("Property is not of type PropType.Float");
                this.id = id;
            }

            public static implicit operator int(ShaderIntMirror intMirror) => intMirror.Value;
            public static implicit operator float(ShaderIntMirror intMirror) => intMirror.Value;
            public static implicit operator MaterialProperty(ShaderIntMirror intMirror) => intMirror.property;
        }
        private class ShaderVectorMirror
        {
            public MaterialProperty property;
            public Vector4 Value
            {
                get => property.vectorValue;
                set => property.vectorValue = value;
            }
            public int id;
            public float this[int i]
            {
                get
                {
                    switch (i)
                    {
                        case 0:
                            return X;
                        case 1:
                            return Y;
                        case 2:
                            return Z;
                        case 3:
                            return W;
                        default:
                            throw new IndexOutOfRangeException(indexExceptionMessage);
                    }
                }
                set
                {
                    switch (i)
                    {
                        case 0:
                            X = value;
                            break;
                        case 1:
                            Y = value;
                            break;
                        case 2:
                            Z = value;
                            break;
                        case 3:
                            W = value;
                            break;
                        default:
                            throw new IndexOutOfRangeException(indexExceptionMessage);
                    }
                }
            }

            public float X
            {
                get
                {
                    return property.vectorValue.x;
                }
                set
                {
                    Value = new Vector4(value, Value.y, Value.z, Value.w);
                }
            }
            public float Y
            {
                get
                {
                    return property.vectorValue.y;
                }
                set
                {
                    Value = new Vector4(Value.x, value, Value.z, Value.w);
                }
            }
            public float Z
            {
                get
                {
                    return property.vectorValue.z;
                }
                set
                {
                    Value = new Vector4(Value.x, Value.y, value, Value.w);
                }
            }
            public float W
            {
                get
                {
                    return property.vectorValue.w;
                }
                set
                {
                    Value = new Vector4(Value.x, Value.y, Value.z, value);
                }
            }

            public ShaderVectorMirror(MaterialProperty prop, int id = default)
            {
                property = prop ?? throw new ArgumentNullException(nameof(prop));
                Value = prop.type == MaterialProperty.PropType.Vector ?
                    property.vectorValue : throw new ArgumentException("Property is not of type PropType.Vector");
                this.id = id;
            }

            public static implicit operator Vector4(ShaderVectorMirror vectorMirror) => vectorMirror.Value;
            public static implicit operator MaterialProperty(ShaderVectorMirror vectorMirror) => vectorMirror.property;
        }
    }
}