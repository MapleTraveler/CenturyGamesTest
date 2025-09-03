using Core.Collections;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        private const float lineHeight = 20f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var keysProp = property.FindPropertyRelative("keys");
            var valuesProp = property.FindPropertyRelative("values");

            EditorGUI.BeginProperty(position, label, property);
            position.height = lineHeight;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                int count = Mathf.Max(keysProp.arraySize, valuesProp.arraySize);

                // 绘制表头
                position.y += lineHeight;
                EditorGUI.LabelField(new Rect(position.x, position.y, position.width / 2, lineHeight), "Key");
                EditorGUI.LabelField(new Rect(position.x + position.width / 2, position.y, position.width / 2, lineHeight), "Value");

                // 绘制元素
                for (int i = 0; i < count; i++)
                {
                    position.y += lineHeight;
                    if (i >= keysProp.arraySize) keysProp.InsertArrayElementAtIndex(i);
                    if (i >= valuesProp.arraySize) valuesProp.InsertArrayElementAtIndex(i);

                    var keyProp = keysProp.GetArrayElementAtIndex(i);
                    var valueProp = valuesProp.GetArrayElementAtIndex(i);

                    EditorGUI.PropertyField(new Rect(position.x, position.y, position.width / 2, lineHeight), keyProp, GUIContent.none);
                    EditorGUI.PropertyField(new Rect(position.x + position.width / 2, position.y, position.width / 2, lineHeight), valueProp, GUIContent.none);
                }

                // 添加按钮
                position.y += lineHeight;
                if (GUI.Button(new Rect(position.x, position.y, position.width / 2, lineHeight), "Add"))
                {
                    keysProp.arraySize++;
                    valuesProp.arraySize++;
                }
                if (GUI.Button(new Rect(position.x + position.width / 2, position.y, position.width / 2, lineHeight), "Remove Last"))
                {
                    if (keysProp.arraySize > 0) keysProp.arraySize--;
                    if (valuesProp.arraySize > 0) valuesProp.arraySize--;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return lineHeight;
            var keysProp = property.FindPropertyRelative("keys");
            var valuesProp = property.FindPropertyRelative("values");
            int count = Mathf.Max(keysProp.arraySize, valuesProp.arraySize);
            return lineHeight * (count + 3); // foldout + 表头 + 元素 + 按钮
        }
    }
}
