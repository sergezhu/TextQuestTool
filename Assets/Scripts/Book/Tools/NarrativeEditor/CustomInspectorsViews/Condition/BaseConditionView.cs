#if UNITY_EDITOR
using Book.Core;
using UnityEditor;
using UnityEngine;
using Book.Tools.NarrativeEditor.ViewUtils;

namespace Book.Tools.NarrativeEditor
{
    public abstract class BaseConditionView
    {
        protected GUIStyle _redtextStyle, _yellowTextStyle, _boxStyle, _tabSelectedStyle, _tabStyle, _crossButtonSmallStyle;
        protected int _labelWidth;
        protected int _lineHeight;
        protected int _fullWidth;
        protected int _halfFullWidth;
        protected int _thirdFullWidth;
        protected int _twoColumnWidth;
        protected int _borderSpacing;
        
        protected float _betweenElementsOffset = 8;
        protected string _stringPadding;
        protected string _stringCounterPadding;

        public virtual void DrawInspector(Rect rect)
        {
        }
        
        protected virtual void InitStyles()
        {
            _labelWidth = 160;
            _lineHeight = 24;
            _fullWidth = Screen.width;
            _borderSpacing = 20;
            _twoColumnWidth = _fullWidth - _borderSpacing * 2 - _labelWidth;
            _halfFullWidth = _fullWidth / 2 - _borderSpacing;
            _thirdFullWidth = _fullWidth / 3 - _borderSpacing;

            _redtextStyle = new GUIStyle();
            _redtextStyle.normal.textColor = Color.red;
            
            _yellowTextStyle = new GUIStyle();
            _yellowTextStyle.normal.textColor = Color.yellow * 0.9f;

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
        
        
        protected void DrawCharacterBlock(ref Character targetCharacter, ref bool showMoreData, Rect rect)
        {
            bool needClearCharacterField = false;
            
            EditorGUILayout.BeginHorizontal();
            
            if (targetCharacter is null)
            {
                EditorGUILayout.LabelField("Выберите персонажа: ", GUILayout.Width(_labelWidth));
                targetCharacter = EditorGUILayout.ObjectField(targetCharacter, typeof(Character), false,
                    GUILayout.Width(rect.width - _labelWidth - _lineHeight), GUILayout.Height(_lineHeight)) as Character;
            }
            else
            {
                GUILayout.BeginVertical();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Выбранный персонаж: ", GUILayout.Width(_labelWidth));
                EditorGUILayout.LabelField(_stringPadding + $"{targetCharacter.Name}" + _stringPadding, _boxStyle);
                EditorGUILayout.BeginVertical();
                needClearCharacterField = GUILayout.Button("X", _crossButtonSmallStyle);
                EditorGUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(GetShowListHeader(targetCharacter), 
                    GUILayout.MaxWidth(StringUtil.CalculateStringWidth(GetShowListHeader(targetCharacter), 0)));
                showMoreData = EditorGUILayout.Toggle("", showMoreData, GUILayout.Width(_lineHeight));
                EditorGUILayout.LabelField("", GUILayout.Width(_lineHeight));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();

                if (showMoreData)
                {
                    ShowCharacterMoreData(targetCharacter);
                }
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                
                GUILayout.FlexibleSpace();

                if(needClearCharacterField) 
                    targetCharacter = null;
            }

            EditorGUILayout.EndHorizontal();
            
            if (targetCharacter is null)
            {
                EditorGUILayout.LabelField("Не выбран персонаж.",
                    _redtextStyle, GUILayout.MaxWidth(_fullWidth - _lineHeight*3));
            }
        }

        protected virtual void ShowCharacterMoreData(Character targetCharacter)
        {
        }

        protected virtual string GetShowListHeader(Character character)
        {
            return "";
        }

        public void DrawReceiveableElement(BaseFeatureObject element, ref float calculatedWidth)
        {
            float currentWidth = ViewUtils.StringUtil.CalculateStringWidth(element.title, _stringPadding.Length);
            calculatedWidth += currentWidth + _betweenElementsOffset;

            if (calculatedWidth > _fullWidth - _lineHeight * 4)
            {
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                calculatedWidth = currentWidth + _betweenElementsOffset;
            }

            EditorGUILayout.LabelField(_stringPadding + $"{element.title}" + _stringPadding, new GUIStyle("box"),
                GUILayout.Width(currentWidth));
        }
        
        protected void DrawConditionTypeHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Тип сравнения: ", GUILayout.Width(_labelWidth));
            DrawConditionEnumPopup();
            GUILayout.EndHorizontal();
        }
        
        protected void ShowParameterContainWarning()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Если выбранный параметр отсутствует у персонажа,\nрезультат выражения будет равен FALSE ", 
                _yellowTextStyle, GUILayout.Width(_labelWidth));
            GUILayout.EndHorizontal();
        }

        protected virtual void DrawConditionEnumPopup()
        {
        }
    }
}
#endif