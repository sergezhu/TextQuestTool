#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Book.Tools.NarrativeEditor
{
    public class BaseActionView
    {
        protected GUIStyle _redtextStyle, _boxStyle, _tabSelectedStyle, _tabStyle, _crossButtonSmallStyle;
        protected int _labelWidth;
        protected int _lineHeight;
        protected int _fullWidth;
        protected int _halfFullWidth;
        protected int _thirdFullWidth;
        protected int _twoColumnWidth;
        protected int _borderSpacing;
        
        protected float _averageWidthPerSymbol;
        protected float _betweenElementsOffset = 8;
        protected string _stringPadding;
        protected string _stringCounterPadding;
        
        public virtual void DrawInspector(Rect rect)
        {
        }
        
        protected virtual void InitStyles(Rect rect)
        {
            _labelWidth = 160;
            _lineHeight = 24;
            _fullWidth = Screen.width;
            _borderSpacing = 20;
            _twoColumnWidth = _fullWidth - _borderSpacing * 2 - _labelWidth;
            _halfFullWidth = _fullWidth / 2 - _borderSpacing;
            _thirdFullWidth = _fullWidth / 3 - _borderSpacing;
            _averageWidthPerSymbol = 7.5f;

            _redtextStyle = new GUIStyle();
            _redtextStyle.normal.textColor = Color.red;
            _redtextStyle.fixedWidth = rect.width - _borderSpacing * 2;
            _redtextStyle.clipping = TextClipping.Clip;
            _redtextStyle.wordWrap = true;
            _redtextStyle.stretchHeight = true;

            _stringPadding = "     ";
            _stringCounterPadding = "  ";

            _boxStyle = new GUIStyle("box");
            _boxStyle.normal.textColor = Color.white * 0.8f;
            _boxStyle.margin = new RectOffset(0, 0, 0, 0);

            _tabStyle = new GUIStyle("box");
            _tabStyle.normal.background = EditorResourcesProvider.ColorTexture(Color.black * 0.05f);
            _tabStyle.normal.textColor = Color.white * 0.8f;

            _tabSelectedStyle = new GUIStyle("box");
            _tabSelectedStyle.normal.background = EditorResourcesProvider.ColorTexture(Color.black * 0.8f);
            _tabSelectedStyle.normal.textColor = Color.cyan;
            
            _crossButtonSmallStyle = new GUIStyle("label");
            _crossButtonSmallStyle.normal.background = EditorResourcesProvider.ColorTexture(Color.black * 0.5f);
            _crossButtonSmallStyle.border = new RectOffset(0, 0, 0, 0);
            _crossButtonSmallStyle.padding = new RectOffset(0, 0, 0, 0);
            _crossButtonSmallStyle.margin = new RectOffset(0, 0, 4, 0);
            _crossButtonSmallStyle.fixedWidth = 14;
            _crossButtonSmallStyle.fixedHeight = 14;
            _crossButtonSmallStyle.fontSize = 10;
            _crossButtonSmallStyle.alignment = TextAnchor.MiddleCenter;
        }

        protected void DrawID(int id)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"ID : {id}", _boxStyle);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8);
        }

        protected void DrawRedText(string text, Rect rect)
        {
            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(text, _redtextStyle, GUILayout.MaxWidth(rect.width));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }
    }
}
#endif