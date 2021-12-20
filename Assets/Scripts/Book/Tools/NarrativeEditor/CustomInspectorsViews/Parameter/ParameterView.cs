#if UNITY_EDITOR
using Book.Core;
using UnityEditor;
using UnityEngine;

namespace Book.Tools.NarrativeEditor
{
    public class ParameterView
    {
        protected Parameter Parameter;
        
        public ParameterView(Parameter parameter)
        {
            Parameter = parameter;
        }
        
        private int _labelWidth = 150;
        private int _textAreaHeight = 32;
        private int _borderSpacing = 20;

        public virtual void DrawInspector()
        {
            EditorGUILayout.BeginVertical("box");
            
            BaseFeatureObjectView.DrawID(Parameter.ID);
            DrawTitle();
            DrawDescription();
            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            DrawValueBlockHeader();
            DrawAddOperationValueField();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
            
            EditorGUILayout.EndVertical();
        }

        protected int GetTextAreaWidth()
        {
            return Screen.width - _labelWidth - _borderSpacing * 2;
        }

        protected void DrawTitle()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Название параметра", GUILayout.Width(_labelWidth));
            Parameter.title = EditorGUILayout.TextField(Parameter.title, GUILayout.Width(GetTextAreaWidth()));
            EditorGUILayout.EndHorizontal();
        }
        protected void DrawDescription()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Описание", GUILayout.Width(_labelWidth));
            Parameter.description = EditorGUILayout.TextArea(Parameter.description,
                GUILayout.Height(_textAreaHeight), GUILayout.Width(GetTextAreaWidth()));
            EditorGUILayout.EndHorizontal();
        }
        protected virtual  void DrawValueBlockHeader()
        {
            EditorGUILayout.LabelField("Значение", GUILayout.Width(_labelWidth));
        }
        public virtual void DrawAddOperationValueField()
        {
        }
        public virtual void DrawRemoveOperationValueField()
        {
        }
        public virtual void DrawChangeOperationValueField()
        {
        }
    }
}
#endif