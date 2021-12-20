#if UNITY_EDITOR
using System.Collections.Generic;
using Book.Core;
using Book.Tools.NarrativeEditor.ViewUtils;
using UnityEditor;
using UnityEngine;

namespace Book.Tools.NarrativeEditor
{
    public class ChangeAtributeLineActionView : BaseCharacterPropertyActionView
    {
        protected new ChangeAtributeLineAction Instance;
        
        public ChangeAtributeLineActionView(ChangeAtributeLineAction instance) : base (instance)
        {
            Instance = instance;
        }

        public override void DrawInspector(Rect rect)
        {
            base.DrawInspector(rect);
            DrawAtributeBlock(ref Instance.character, ref Instance.atribute);
            
            EditorGUILayout.Space(10);
        }
        
        protected override void InitStyles(Rect rect)
        {
            base.InitStyles(rect);
        }

        protected override void ShowCharacterMoreData(Character targetCharacter)
        {
            List<Atribute> atributes;
            float calculatedWidth = 0;
            foreach (var x in targetCharacter.Atributes)
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

        protected void DrawAtributeBlock(ref Character targetCharacter, ref Atribute targetAtribute)
        {
            if (targetCharacter is null)
                return;
            
            bool needClearField = false;
            
            GUILayout.BeginHorizontal();
            
            if (targetAtribute is null)
            {
                GUILayout.BeginVertical();
                DrawAtributeSelectField(ref targetAtribute);
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical();
                
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Выбранный атрибут: ", GUILayout.Width(_labelWidth));
                EditorGUILayout.LabelField(_stringPadding + $"{targetAtribute.title}" + _stringPadding, _boxStyle);
                needClearField = GUILayout.Button("X", _crossButtonSmallStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                DrawCheckInfo(targetAtribute, targetCharacter.Atributes);

                GUILayout.EndVertical();
                
                GUILayout.FlexibleSpace();
                
                if (needClearField)
                    targetAtribute = null;
            }

            GUILayout.EndHorizontal();
        }

        private void DrawCheckInfo(Atribute targetAtribute, List<Atribute> atributes)
        {
            if (Instance.isAddingOperation)
                DrawAddingInfo(targetAtribute, atributes);
            else if (Instance.isRemovingOperation)
                DrawRemovingInfo(targetAtribute, atributes);
        }

        private void DrawAddingInfo(Atribute targetAtribute, List<Atribute> atributes)
        {
            Character targetCharacter;
            if (atributes.Contains(targetAtribute))
            {
                DrawRedText("Внимание! Выбранный атрибут уже получен персонажем и добавлен не будет",
                    new Rect(0,0, _fullWidth - _lineHeight * 3, 0));
            }
        }
        private void DrawRemovingInfo(Atribute targetAtribute, List<Atribute> atributes)
        {
            if (atributes.Contains(Instance.atribute) == false)
            {
                DrawRedText("Внимание! Выбранный атрибут отсутствует у персонажа и не может быть удален.",
                    new Rect(0,0, _fullWidth - _lineHeight * 3, 0));
            }
        }

        private void DrawAtributeSelectField(ref Atribute targetAtribute)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Выберите атрибут: ", GUILayout.Width(_labelWidth));
            targetAtribute = EditorGUILayout.ObjectField(targetAtribute, typeof(Atribute), false,
                GUILayout.MaxWidth(200), GUILayout.MinWidth(20), GUILayout.Height(_lineHeight)) as Atribute;
            GUILayout.EndHorizontal();

            DrawRedText("Не выбран атрибут.",
                new Rect(0,0, _fullWidth - _lineHeight * 3, 0));
        }

        protected override string GetAddingTabHeader()
        {
            return "Добавить атрибут";
        }
        protected override string GetRemovingTabHeader()
        {
            return "Удалить атрибут";
        }
        protected override string GetShowListHeader(Character character)
        {
            return $"Показывать список атрибутов [{character.Atributes.Count}]:";
        }
    }
}
#endif