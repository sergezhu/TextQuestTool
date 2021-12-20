#if UNITY_EDITOR
using Book.Core;
using UnityEditor;
using UnityEngine;

namespace Book.Tools.NarrativeEditor
{
    public class SelectLineVisualDataContainerView
    {
        private readonly SelectLineVisualDataContainer _instance;
        private GUIStyle _boxStyle;
        private GUIStyle _blockHeaderStyle;
        private GUIStyle _headerTextStyle;
        private bool _inspectorStylesInitialized = false;
        
        public SelectLineVisualDataContainerView(SelectLineVisualDataContainer instance)
        {
            _instance = instance;
        }

        private void OnEnable()
        {
            ResetStyles();
        }
        
        public void DrawInspector(Rect rect)
        {
            TryInitInspectorStyles(rect);
            
            EditorGUILayout.Space(8);
            DrawID("Background ID", _instance.BackgroundID);
            DrawID("Text Animation In Effect ID", _instance.effects.textAnimationInEffectID);
            DrawID("Text Animation Out Effect ID", _instance.effects.textAnimationOutEffectID);
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
        
        protected void DrawID(string header, int id)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{header} : {id}", _boxStyle);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8);
        }
    }
}
#endif