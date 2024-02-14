using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using com.kakunvr.parameter_smoother.runtime;
using VRC.SDK3.Avatars.Components;

// ReSharper disable once CheckNamespace
namespace com.kakunvr.parameter_smoother.editor
{
    [CustomEditor(typeof(ParameterSmoother))]
    public class SettingsEditor : Editor
    {
        private SerializedProperty _controllerProp;
        private SerializedProperty _layerTypeProp;
        private SerializedProperty _smoothedSuffixProp;
        private SerializedProperty _configsProp;
        private ReorderableList _configsList;
        
        private bool _foldout=false;
        private float _localSmoothness = 0.1f;
        private float _remoteSmoothness = 0.1f;
        
        private void OnEnable()
        {
            var so = serializedObject;
            _layerTypeProp = so.FindProperty("layerType");
            _smoothedSuffixProp = so.FindProperty("smoothedSuffix");
            _configsProp = so.FindProperty("configs");
            _configsList = new ReorderableList(so, _configsProp);
            _configsList.drawHeaderCallback += rect =>
            {
                EditorGUI.LabelField(rect, "Target Parameters");
            };
            _configsList.elementHeight = (EditorGUIUtility.singleLineHeight + 2) * 3;
            _configsList.drawElementCallback += (rect, index, selected, focused) =>
            {
                var prop = _configsList.serializedProperty.GetArrayElementAtIndex(index);
                var h = EditorGUIUtility.singleLineHeight;
                var lw = EditorGUIUtility.labelWidth;
                var sourceRect = new Rect(rect.x, rect.y + (h + 2) * 0 + 1, rect.width, h);
                EditorGUI.PropertyField(sourceRect, prop.FindPropertyRelative("parameterName"));
                var localSmoothnessRect = new Rect(rect.x, rect.y + (h + 2) * 1 + 1, rect.width, h);
                EditorGUI.PropertyField(localSmoothnessRect, prop.FindPropertyRelative("localSmoothness"));
                var remoteSmoothnessRect = new Rect(rect.x, rect.y + (h + 2) * 2 + 1, rect.width, h);
                EditorGUI.PropertyField(remoteSmoothnessRect, prop.FindPropertyRelative("remoteSmoothness"));
            };
        }

        public override void OnInspectorGUI()
        {
            var so = serializedObject;
            EditorGUILayout.PropertyField(_layerTypeProp, new GUIContent("Layer Type"));
            EditorGUILayout.PropertyField(_smoothedSuffixProp, new GUIContent("Smoothed Parameter Suffix"));

            _foldout = EditorGUILayout.Foldout(_foldout, "Import From Avatar Animator");

            if (_foldout)
            {
                // 読み込み時のオプション
                GUILayout.BeginVertical("Box");
                GUILayout.Label("Import Options");
                _localSmoothness = EditorGUILayout.FloatField("Local Smoothness", _localSmoothness);
                _remoteSmoothness = EditorGUILayout.FloatField("Remote Smoothness", _remoteSmoothness);
                GUILayout.EndVertical();
                if (GUILayout.Button("Import"))
                {
                    ImportFromAnimator();
                }
            }

            _configsList.DoLayoutList();
            so.ApplyModifiedProperties();
        }

        private void ImportFromAnimator()
        {
            var smoother = (ParameterSmoother)target;
            var avatarDescriptor = smoother.GetComponentInParent<VRCAvatarDescriptor>();

            if (avatarDescriptor == null)
            {
                Debug.LogError("No avatar descriptor found in parent hierarchy");
                return;
            }

            if (!avatarDescriptor.customizeAnimationLayers)
            {
                Debug.LogError("Avatar does not have animation layers enabled");
                return;
            }

            // 指定されているレイヤー情報を取得
            var enumIndex = _layerTypeProp.enumValueIndex;
            VRCAvatarDescriptor.AnimLayerType layerType = (VRCAvatarDescriptor.AnimLayerType)enumIndex;
            var anim = avatarDescriptor.baseAnimationLayers.First(x => x.type == layerType);

            if (anim.isDefault)
            {
                Debug.LogError("Layer type is not set to custom");
                return;
            }

            // パラメータ情報を追加する
            var controller = anim.animatorController;
            // パスを取得しAnimatorとして読み込む
            var path = AssetDatabase.GetAssetPath(controller);
            var animator = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(path);

            var parameters = animator.parameters.Select(x => x.name).ToList();
            var newConfigs = new List<SmoothingConfig>();
            var suffix = _smoothedSuffixProp.stringValue;
            foreach (var parameter in parameters)
            {
                var param = parameter.Replace(suffix, "");
                // 既に同名のがある場合はスキップ
                if (smoother.configs.Any(x => x.parameterName == param))
                {
                    continue;
                }

                newConfigs.Add(new SmoothingConfig
                {
                    parameterName = param,
                    localSmoothness = _localSmoothness,
                    remoteSmoothness = _remoteSmoothness
                });
            }

            smoother.configs.AddRange(newConfigs);
        }
    }
}
