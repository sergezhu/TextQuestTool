#if UNITY_EDITOR
using Book.Core;
using UnityEditor;
using UnityEngine;

namespace Book.Tools.NarrativeEditor
{
    public class NarrativePointVisualDataContainerView
    {
        private readonly NarrativePointVisualDataContainer _instance;
        private GUIStyle _boxStyle;
        private GUIStyle _blockHeaderStyle;
        private GUIStyle _headerTextStyle;
        private bool _inspectorStylesInitialized = false;
        
        public NarrativePointVisualDataContainerView(NarrativePointVisualDataContainer instance)
        {
            _instance = instance;
        }

        public void DrawInspector(Rect rect)
        {
            TryInitInspectorStyles(rect);
            
            EditorGUILayout.Space(8);
            DrawBackgroundID(_instance.BackgroundID);
        }
        
        public void ResetStyles()
        {
            _inspectorStylesInitialized = false;
        }
        protected virtual void InitGeneralStyles()
        {
        }

        protected virtual bool TryInitInspectorStyles(Rect rect)
        {
            if (_inspectorStylesInitialized) 
                return false;
            
            InitGeneralStyles();
            
            _boxStyle = new GUIStyle("box");
            _boxStyle.normal.textColor = Color.white * 0.8f;
            _boxStyle.margin = new RectOffset(0, 0, 0, 0);
            
            _blockHeaderStyle = new GUIStyle("label");
            _blockHeaderStyle.normal.textColor = Color.white * 0.8f;
            _blockHeaderStyle.normal.background = EditorResourcesProvider.ColorTexture(Color.black * 0.5f);
            
            _headerTextStyle = new GUIStyle("label");
            _headerTextStyle.fontStyle = FontStyle.Bold;
            _headerTextStyle.alignment = TextAnchor.MiddleCenter;

            _inspectorStylesInitialized = true;
            return true;
        }
        
        protected void DrawBackgroundID(int id)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Background ID : {id}", _boxStyle);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8);
        }
    }
}
#endif