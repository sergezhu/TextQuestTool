#if UNITY_EDITOR
using System.Collections.Generic;
using Book.Core;
using Book.Tools.NarrativeEditor.ViewUtils;
using UnityEditor;
using UnityEngine;

namespace Book.Tools.NarrativeEditor
{
    public class ChangeAchievmentLineActionView : BaseCharacterPropertyActionView
    {
        protected new ChangeAchievmentLineAction Instance;
        
        public ChangeAchievmentLineActionView(ChangeAchievmentLineAction instance) : base (instance)
        {
            Instance = instance;
        }

        public override void DrawInspector(Rect rect) 
        {
            base.DrawInspector(rect);
            DrawAchievmentBlock(ref Instance.character, ref Instance.achievment);
            
            EditorGUILayout.Space(10);
        }
        
        protected override void InitStyles(Rect rect)
        {
            base.InitStyles(rect);
        }
        
        protected override void DrawOperationButtons(Rect rect)
        {
            GUILayout.BeginHorizontal();
            DrawAddingTabButton(rect.width - _borderSpacing * 2);
            GUILayout.EndHorizontal();
        }

        protected override void ShowCharacterMoreData(Character targetCharacter)
        {
            List<Achievment> achievments;
            float calculatedWidth = 0;

            foreach (var x in targetCharacter.Achievments)
            {
                if (x is null) continue;

                float currentWidth = StringUtil.CalculateStringWidth(x.title, _stringPadding.Length, _averageWidthPerSymbol);
                calculatedWidth += currentWidth + _betweenElementsOffset;

                if (calculatedWidth > _fullWidth - _lineHeight * 3)
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    calculatedWidth = currentWidth;
                }

                EditorGUILayout.LabelField(_stringPadding + $"{x.title}" + _stringPadding, new GUIStyle("box"),
                    GUILayout.Width(currentWidth));
                EditorGUILayout.LabelField("", GUILayout.Width(_betweenElementsOffset));
            }

            GUILayout.FlexibleSpace();
        }

        protected void DrawAchievmentBlock(ref Character targetCharacter, ref Achievment targetAchievment)
        {
            if (targetCharacter is null)
                return;

            bool needClearField = false;
            
            GUILayout.BeginHorizontal();
            
            if (targetAchievment is null)
            {
                GUILayout.BeginVertical();
                DrawAchievmentSelectField(ref targetAchievment);
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical();
                
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Выбранная ачивка: ", GUILayout.Width(_labelWidth));
                EditorGUILayout.LabelField(_stringPadding + $"{targetAchievment.title}" + _stringPadding, _boxStyle);
                needClearField = GUILayout.Button("X", _crossButtonSmallStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                DrawCheckInfo(targetAchievment, targetCharacter.Achievments);

                GUILayout.EndVertical();
                
                GUILayout.FlexibleSpace();
                
                if (needClearField)
                    targetAchievment = null;
            }

            GUILayout.EndHorizontal();
        }

        private void DrawCheckInfo(Achievment targetAchievment, List<Achievment> achievments)
        {
            if (Instance.isAddingOperation)
                DrawAddingInfo(targetAchievment, achievments);
            else if (Instance.isRemovingOperation)
                DrawRemovingInfo(targetAchievment, achievments);
        }

        private void DrawAddingInfo(Achievment targetAchievment, List<Achievment> achievments)
        {
            Character targetCharacter;
            if (achievments.Contains(targetAchievment))
            {
                DrawRedText("Внимание! Выбранная ачивка уже получена персонажем и добавлена не будет",
                    new Rect(0,0, _fullWidth - _lineHeight * 3, 0));
            }
        }
        private void DrawRemovingInfo(Achievment targetAchievment, List<Achievment> achievments)
        {
            if (achievments.Contains(Instance.achievment) == false)
            {
                DrawRedText("Внимание! Выбранная ачивка отсутствует у персонажа и не может быть удалена.",
                    new Rect(0,0, _fullWidth - _lineHeight * 3, 0));
            }
        }

        private void DrawAchievmentSelectField(ref Achievment targetAchievment)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Выберите ачивку: ", GUILayout.Width(_labelWidth));
            targetAchievment = EditorGUILayout.ObjectField(targetAchievment, typeof(Achievment), false,
                GUILayout.MaxWidth(200), GUILayout.MinWidth(20), GUILayout.Height(_lineHeight)) as Achievment;
            GUILayout.EndHorizontal();

            DrawRedText("Не выбрана ачивка.",
                new Rect(0,0, _fullWidth - _lineHeight * 3, 0));
        }

        protected override string GetAddingTabHeader()
        {
            return "Добавить ачивку";
        }
        protected override string GetRemovingTabHeader()
        {
            return "Удалить ачивку";
        }
        protected override string GetShowListHeader(Character character)
        {
            return $"Показывать список ачивок [{character.Achievments.Count}]:";
        }
    }
}
#endif