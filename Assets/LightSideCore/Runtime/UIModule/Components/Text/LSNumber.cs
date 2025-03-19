﻿using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LSCore
{
    public class LSNumber : LSText
    {
        [field: SerializeField] public int Number { get; private set; }
        public static implicit operator int(LSNumber number) => number.Number;

        public override string text
        {
            get => base.text;
            set
            {
                base.text = value;
                Number = TextToNumber(value);
            }
        }

        public static int TextToNumber(string text)
        {
            var strVal = Regex.Replace(text, "<.*?>", string.Empty);
            var intVal = Regex.Replace(strVal, "[^0-9]", string.Empty);
            int.TryParse(intVal, out var result);
            return result;
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(LSNumber), true), CanEditMultipleObjects]
    public class LSNumberEditor : LSTextEditor
    {
        SerializedProperty Number;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            Number = serializedObject.FindBackingField("Number");
        }

        public override void OnInspectorGUI()
        {
            if (IsMixSelectionTypes()) return;

            serializedObject.Update();

            Draw();

            if (serializedObject.ApplyModifiedProperties() || m_HavePropertiesChanged)
            {
                m_TextComponent.havePropertiesChanged = true;
                m_HavePropertiesChanged = false;
                EditorUtility.SetDirty(target);
            }
        }

        protected virtual void Draw()
        {
            var oldEnabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUILayout.PropertyField(Number);
            GUI.enabled = oldEnabled;
            
            DrawTextInput();
            
            Number.intValue = LSNumber.TextToNumber(m_TextProp.stringValue);

            DrawMainSettings();

            DrawExtraSettings();

            EditorGUILayout.Space();
        }


        [MenuItem("GameObject/LSCore/Number")]
        private static void CreateButton()
        {
            new GameObject(nameof(LSNumber)).AddComponent<LSNumber>();
        }
    }
#endif
}