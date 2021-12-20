#if UNITY_EDITOR
using Book.Core;
using UnityEditor;
using UnityEngine;

namespace Book.Tools.NarrativeEditor
{
    public abstract class BaseFeatureObjectView
    {
        protected static GUIStyle _boxStyle;
        
        private static GUIStyle CreateBoxStyle()
        {
            if (_boxStyle != null) return _boxStyle;
            
            _boxStyle = new GUIStyle("box");
            _boxStyle.normal.textColor = Color.white * 0.8f;
            _boxStyle.margin = new RectOffset(0, 0, 0, 0);
            return _boxStyle;
        }

        public static void DrawID(int id)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"ID : {id}", CreateBoxStyle());
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8);
        }
    }
}
#endif