#if UNITY_EDITOR
using Book.Core;
using UnityEditor;
using UnityEngine;

namespace Book.Tools.NarrativeEditor
{
    public class IntParameterView : ParameterView
    {
        protected new IntParameter Parameter;

        public IntParameterView(IntParameter parameter) : base(parameter)
        {
            Parameter = parameter;
        }
        
        public override void DrawAddOperationValueField()
        {
            string stringValue = "";
            try
            {
                stringValue = EditorGUILayout.TextField(Parameter.Value.ToString(), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                Parameter.Value = int.Parse(stringValue);
            }
            catch (System.Exception e)
            {
                Debug.Log("Int Incorrect");
            }
        }
        public override void DrawRemoveOperationValueField()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Parameter.Value.ToString(), new GUIStyle("box"),GUILayout.MinWidth(20), GUILayout.MaxWidth(200));
            GUILayout.EndHorizontal();
        }
    }
}
#endif
