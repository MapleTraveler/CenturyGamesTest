using Core.Collections;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Core.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        private const float LineVSpace = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var so = property.serializedObject;
            so.Update(); // ★ 先更新

            var keysProp = property.FindPropertyRelative("keys");
            var valuesProp = property.FindPropertyRelative("values");

            EditorGUI.BeginProperty(position, label, property);
            var foldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldRect, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                position.y += EditorGUIUtility.singleLineHeight + LineVSpace;

                // ---- 保证两边长度一致 ----
                int count = Mathf.Max(keysProp.arraySize, valuesProp.arraySize);
                if (keysProp.arraySize != count) keysProp.arraySize = count;
                if (valuesProp.arraySize != count) valuesProp.arraySize = count;

                // ---- 表头 ----
                var headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(new Rect(headerRect.x, headerRect.y, headerRect.width * 0.5f, headerRect.height), "Key");
                EditorGUI.LabelField(new Rect(headerRect.x + headerRect.width * 0.5f, headerRect.y, headerRect.width * 0.5f, headerRect.height), "Value");
                position.y += EditorGUIUtility.singleLineHeight + LineVSpace;

                // ---- 元素 ----
                for (int i = 0; i < count; i++)
                {
                    var keyProp = keysProp.GetArrayElementAtIndex(i);
                    var valProp = valuesProp.GetArrayElementAtIndex(i);

                    float kh = EditorGUI.GetPropertyHeight(keyProp, GUIContent.none, true);
                    float vh = EditorGUI.GetPropertyHeight(valProp, GUIContent.none, true);
                    float rowH = Mathf.Max(kh, vh);

                    var keyRect = new Rect(position.x, position.y, position.width * 0.5f - 2, rowH);
                    var valRect = new Rect(position.x + position.width * 0.5f + 2, position.y, position.width * 0.5f - 2, rowH);

                    EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none, true);
                    EditorGUI.PropertyField(valRect, valProp, GUIContent.none, true);

                    position.y += rowH + LineVSpace;
                }

                // ---- 按钮 ----
                var btnRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                var halfW = btnRect.width * 0.5f;

                if (GUI.Button(new Rect(btnRect.x, btnRect.y, halfW - 2, btnRect.height), "Add"))
                {
                    Undo.RecordObject(so.targetObject, "Add Dictionary Entry");

                    keysProp.InsertArrayElementAtIndex(keysProp.arraySize);
                    valuesProp.InsertArrayElementAtIndex(valuesProp.arraySize);

                    // 初始化新键/值默认内容，降低“重复键”造成的困扰
                    InitDefault(keysProp.GetArrayElementAtIndex(keysProp.arraySize - 1));
                    InitDefault(valuesProp.GetArrayElementAtIndex(valuesProp.arraySize - 1));

                    EditorUtility.SetDirty(so.targetObject);
                }

                if (GUI.Button(new Rect(btnRect.x + halfW + 2, btnRect.y, halfW - 2, btnRect.height), "Remove Last"))
                {
                    Undo.RecordObject(so.targetObject, "Remove Dictionary Entry");
                    if (keysProp.arraySize > 0) keysProp.DeleteArrayElementAtIndex(keysProp.arraySize - 1);
                    if (valuesProp.arraySize > 0) valuesProp.DeleteArrayElementAtIndex(valuesProp.arraySize - 1);
                    EditorUtility.SetDirty(so.targetObject);
                }

                position.y += EditorGUIUtility.singleLineHeight + LineVSpace;
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();

            // ★ 统一提交（包括行内编辑的改动）
            so.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = EditorGUIUtility.singleLineHeight; // foldout
            if (!property.isExpanded) return h;

            var keysProp = property.FindPropertyRelative("keys");
            var valuesProp = property.FindPropertyRelative("values");
            int count = Mathf.Max(keysProp.arraySize, valuesProp.arraySize);

            h += LineVSpace; // space
            h += EditorGUIUtility.singleLineHeight + LineVSpace; // header

            for (int i = 0; i < count; i++)
            {
                var keyProp = keysProp.GetArrayElementAtIndex(Mathf.Min(i, keysProp.arraySize - 1));
                var valProp = valuesProp.GetArrayElementAtIndex(Mathf.Min(i, valuesProp.arraySize - 1));
                float kh = keyProp != null ? EditorGUI.GetPropertyHeight(keyProp, GUIContent.none, true) : EditorGUIUtility.singleLineHeight;
                float vh = valProp != null ? EditorGUI.GetPropertyHeight(valProp, GUIContent.none, true) : EditorGUIUtility.singleLineHeight;
                h += Mathf.Max(kh, vh) + LineVSpace;
            }

            h += EditorGUIUtility.singleLineHeight + LineVSpace; // buttons
            return h;
        }

        private static void InitDefault(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.String:
                    if (string.IsNullOrEmpty(prop.stringValue)) prop.stringValue = "NewKey";
                    break;
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = null;
                    break;
                case SerializedPropertyType.Integer:
                    // 给个 0 起步
                    break;
                case SerializedPropertyType.Enum:
                    // 保持当前
                    break;
                // 其他类型按需处理
            }
        }
    }
}
