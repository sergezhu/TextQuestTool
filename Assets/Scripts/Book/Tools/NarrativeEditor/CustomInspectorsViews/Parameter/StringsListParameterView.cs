#if UNITY_EDITOR
using Book.Core;
using UnityEditor;
using UnityEngine;

namespace Book.Tools.NarrativeEditor
{
    public class StringsListParameterView : ParameterView
    {
        protected new StringsListParameter Parameter;

        public StringsListParameterView(StringsListParameter parameter) : base(parameter)
        {
            Parameter = parameter;
        }
        
        public override void DrawAddOperationValueField()
        {
            GUILayout.BeginHorizontal();
            Parameter.CurrentIndex = EditorGUILayout.Popup("", Parameter.CurrentIndex, Parameter.OptionsList.ToArray());
            GUILayout.EndHorizontal();
        }
        public override void DrawRemoveOperationValueField()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Parameter.OptionsList[Parameter.CurrentIndex], new GUIStyle("box"),GUILayout.MinWidth(20), GUILayout.MaxWidth(200));
            GUILayout.EndHorizontal();
        }
        public void DrawSelfInspector()
        {
            EditorGUILayout.BeginVertical("box");
            
            BaseFeatureObjectView.DrawID(Parameter.ID);
            DrawTitle();
            DrawDescription();
            EditorGUILayout.Space(10);
            
            DrawSelfValueBlockHeader();
            DrawSelfValueBlockBody();
            EditorGUILayout.Space(10);
            
            EditorGUILayout.EndVertical();
        }
        protected void DrawSelfValueBlockHeader()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Список значений");
            if (GUILayout.Button("Добавить значение", GUILayout.Width(150)))
            {
                Parameter.OptionsList.Add("Новое значение");
            }

            EditorGUILayout.EndHorizontal();
        }
        public void DrawSelfValueBlockBody()
        {
            int count = Parameter.OptionsList.Count;
            int removedIndex = -1;
            
            for (int i = 0; i < count; i++)
            {
                bool isSelected = Parameter.CurrentIndex == i;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{i}. ", GUILayout.Width(20));

                Parameter.OptionsList[i] = EditorGUILayout.TextField(Parameter.OptionsList[i]);
                if (string.IsNullOrWhiteSpace(Parameter.OptionsList[i].Trim()))
                    Parameter.OptionsList[i] = "Значение не должно быть пустым";
                EditorGUI.BeginChangeCheck();
                isSelected = EditorGUILayout.Toggle(isSelected, GUILayout.Width(15));
                if (EditorGUI.EndChangeCheck() && isSelected)
                {
                    Parameter.CurrentIndex = i;
                    //Debug.Log($"new index: {instance.CurrentIndex}");
                }

                if (GUILayout.Button("X", GUILayout.Width(24), GUILayout.Height(24)))
                    removedIndex = i;

                GUILayout.EndHorizontal();
            }

            if (removedIndex != -1)
            {
                Parameter.CurrentIndex -= Parameter.CurrentIndex >= removedIndex ? 1 : 0;
                Parameter.OptionsList.RemoveAt(removedIndex);
            }
        }
    }
}
#endif