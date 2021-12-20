#if UNITY_EDITOR
using Book.Core;
using Book.Tools.NarrativeEditor.ViewUtils;
using UnityEditor;
using UnityEngine;

namespace Book.Tools.NarrativeEditor
{
    public class BaseCharacterPropertyActionView : BaseActionView
    {
        private BaseCharacterPropertyAction Instance;

        public BaseCharacterPropertyActionView(BaseCharacterPropertyAction instance)
        {
            Instance = instance;
        }

        public override void DrawInspector(Rect rect)
        {
            InitStyles(rect);

            ValidateOperationType();
            GUILayout.Space(10);

            DrawOperationButtons(rect);
            EditorGUILayout.Separator();

            DrawID(Instance.ID);
            DrawCharacterBlock(ref Instance.character, ref Instance.ShowMoreData, rect);
            EditorGUILayout.Separator();
        }

        protected virtual void DrawOperationButtons(Rect rect)
        {
            GUILayout.BeginHorizontal();
            DrawAddingTabButton(rect.width / 2 - _borderSpacing);
            DrawRemovingTabButton(rect.width / 2 - _borderSpacing);
            GUILayout.EndHorizontal();
        }

        protected virtual void DrawAddingTabButton(float width)
        {
            GUIStyle style;
            style = Instance.isAddingOperation ? _tabSelectedStyle : _tabStyle;
            if (GUILayout.Button(GetAddingTabHeader(), style, GUILayout.Width(width)))
                SetAddingOperation();
        }

        protected virtual void DrawRemovingTabButton(float width)
        {
            GUIStyle style;
            style = Instance.isRemovingOperation ? _tabSelectedStyle : _tabStyle;
            if (GUILayout.Button(GetRemovingTabHeader(), style, GUILayout.Width(width)))
                SetRemovingOperation();
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
                EditorGUILayout.BeginVertical();
                
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
                
                if (needClearCharacterField)
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

        protected virtual void ValidateOperationType()
        {
            if (Instance.isAddingOperation == Instance.isRemovingOperation)
            {
                Instance.isAddingOperation = true;
                Instance.isRemovingOperation = false;
            }
        }

        protected virtual void SetAddingOperation()
        {
            if (Instance.isAddingOperation == false)
            {
                Instance.isAddingOperation = true;
                Instance.isRemovingOperation = false;
            }
        }

        protected virtual void SetRemovingOperation()
        {
            if (Instance.isRemovingOperation == false)
            {
                Instance.isAddingOperation = false;
                Instance.isRemovingOperation = true;
            }
        }
        
        protected virtual string GetAddingTabHeader()
        {
            return "";
        }
        protected virtual string GetRemovingTabHeader()
        {
            return "";
        }
    }
}
#endif