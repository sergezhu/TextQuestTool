#if UNITY_EDITOR
using System.Collections.Generic;
using Book.Core;
using Book.Tools.NarrativeEditor.ViewUtils;
using UnityEditor;
using UnityEngine;

namespace Book.Tools.NarrativeEditor
{
    public abstract class ChangeParameterLineActionView : BaseCharacterPropertyActionView
    {
        private ChangeParameterLineAction Instance;
        
        public ChangeParameterLineActionView(ChangeParameterLineAction instance) : base (instance)
        {
            Instance = instance;
        }

        public override void DrawInspector(Rect rect) 
        {
            base.DrawInspector(rect);
            DrawParameterBlock(ref Instance.character);
            
            EditorGUILayout.Space(10);
        }
        
        protected override void InitStyles(Rect rect)
        {
            base.InitStyles(rect);
        }
        
        protected override void DrawOperationButtons(Rect rect)
        {
            var thirdFullWidth = rect.width / 3 - _borderSpacing;
            GUILayout.BeginHorizontal();
            DrawAddingTabButton(thirdFullWidth);
            DrawChangingTabButton(thirdFullWidth);
            DrawRemovingTabButton(thirdFullWidth);
            GUILayout.EndHorizontal();
        }
        
        private void DrawChangingTabButton(float width)
        {
            GUIStyle style;
            style = Instance.isChangingOperation ? _tabSelectedStyle : _tabStyle;
            if (GUILayout.Button(GetChangingTabHeader(), style, GUILayout.Width(width)))
                SetChangingOperation();
        }
        
        protected override void SetAddingOperation()
        {
            if (Instance.isAddingOperation == false)
            {
                Instance.isAddingOperation = true;
                Instance.isChangingOperation = false;
                Instance.isRemovingOperation = false;
            }
        }

        private void SetChangingOperation()
        {
            if (Instance.isChangingOperation == false)
            {
                Instance.isAddingOperation = false;
                Instance.isChangingOperation = true;
                Instance.isRemovingOperation = false;
            }
        }

        protected override void SetRemovingOperation()
        {
            if (Instance.isRemovingOperation == false)
            {
                Instance.isAddingOperation = false;
                Instance.isChangingOperation = false;
                Instance.isRemovingOperation = true;
            }
        }
        
        protected void DrawChangeTypeButtons()
        {
            GUIStyle style;
            var header1 = "Установить значение";
            var header2 = "Установить разницу";
            GUILayout.BeginHorizontal();
            style = Instance.changedValueIsDelta == false ? _tabSelectedStyle : _tabStyle;
            if (GUILayout.Button(header1, style, GUILayout.MinWidth(20), GUILayout.MaxWidth(200)))
                SetChangingTypeAsTarget();
            style = Instance.changedValueIsDelta == true ? _tabSelectedStyle : _tabStyle;
            if (GUILayout.Button(header2, style, GUILayout.MinWidth(20), GUILayout.MaxWidth(200)))
                SetChangingTypeAsDifference();
            GUILayout.EndHorizontal();
        }
        protected void SetChangingTypeAsTarget()
        {
            if (Instance.changedValueIsDelta == true)
            {
                Instance.changedValueIsDelta = false;
            }
        }
        protected void SetChangingTypeAsDifference()
        {
            if (Instance.changedValueIsDelta == false)
            {
                Instance.changedValueIsDelta = true;
            }
        }
                
        
        protected override void ValidateOperationType()
        {
            int trueCount = 0;

            if (Instance.isAddingOperation) trueCount++;
            if (Instance.isChangingOperation) trueCount++;
            if (Instance.isRemovingOperation) trueCount++;
            
            if (trueCount != 1)
            {
                Instance.isAddingOperation = true;
                Instance.isChangingOperation = false;
                Instance.isRemovingOperation = false;
            }
        }

        protected override void ShowCharacterMoreData(Character targetCharacter)
        {
            List<Parameter> parameters;
            float calculatedWidth = 0;
            foreach (var p in targetCharacter.Parameters)
            {
                if (p is null) continue;
                if (ParameterTypePredicat(p) == false) continue;

                //var strTest = $"{p.title} [{p.GetType().Name}]";
                //Debug.Log(strTest);
                float currentWidth = StringUtil.CalculateStringWidth(p.title, _stringPadding.Length);
                calculatedWidth += currentWidth + _betweenElementsOffset;

                if (calculatedWidth > _fullWidth - _lineHeight * 3)
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    calculatedWidth = currentWidth;
                }

                EditorGUILayout.LabelField(_stringPadding + $"{p.title}" + _stringPadding, new GUIStyle("box"),
                    GUILayout.Width(currentWidth));
                EditorGUILayout.LabelField("", GUILayout.Width(_betweenElementsOffset));
            }

            GUILayout.FlexibleSpace();
        }

        protected virtual bool ParameterTypePredicat(Parameter p)
        {
            return true;
        }

        protected virtual void DrawParameterBlock(ref Character targetCharacter)
        {
            if (targetCharacter is null)
                return;
            
            bool needClearField = false;
            
            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();
        }
        
        protected bool DrawSelectedParameter()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Выбранный параметр: ", GUILayout.Width(_labelWidth));
            ShowSelectedParameterTitle();
            var needClearField = GUILayout.Button("X", _crossButtonSmallStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(8);

            if (Instance.isChangingOperation)
            {
                DrawChangeTypeButtons();
                GUILayout.Space(8);
            }

            GUILayout.BeginHorizontal();
            string headerStr = GetFieldHeader();
            EditorGUILayout.LabelField(headerStr, GUILayout.Width(_labelWidth));
                
            ShowSelectedParameterValue();
                    
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            return needClearField;
        }

        protected virtual string GetFieldHeader()
        {
            if (Instance.isChangingOperation == false)
                return $"Текущее значение:{_stringPadding}";
            
            return Instance.changedValueIsDelta ? $"Изменить значения на:{_stringPadding}" : $"Установить значение:{_stringPadding}";
        }

        protected virtual string GetValueText()
        {
            //return (Instance.isRemovingOperation || Instance.isChangingOperation) ? $"  {targetParameter.GetStringedValue()}" : "";
            return "";
        }

        protected virtual void ShowSelectedParameterTitle()
        {
            //EditorGUILayout.LabelField(_stringPadding + $"{targetParameter.title}" + _stringPadding, _boxStyle);
        }
        protected virtual void ShowSelectedParameterValue()
        {
            
        }

        protected void DrawCheckInfo(Parameter targetParameter, List<Parameter> parameters)
        {
            if (Instance.isAddingOperation)
                DrawAddingInfo(targetParameter, parameters);
            else if (Instance.isRemovingOperation)
                DrawRemovingInfo(targetParameter, parameters);
            else if (Instance.isChangingOperation)
                DrawChangingInfo(targetParameter, parameters);
        }

        private void DrawAddingInfo(Parameter targetParameter, List<Parameter> parameters)
        {
            Character targetCharacter;
            if (parameters.Contains(targetParameter))
            {
                DrawRedText("Внимание! Выбранный параметр уже получен персонажем и добавлен не будет",
                    new Rect(0,0, _fullWidth - _lineHeight * 3, 0));
            }
        }
        private void DrawChangingInfo(Parameter targetParameter, List<Parameter> parameters)
        {
            if (parameters.Contains(targetParameter) == false)
            {
                DrawRedText("Внимание! Выбранный параметр отсутствует у персонажа и не может быть изменен.",
                    new Rect(0,0, _fullWidth - _lineHeight * 3, 0));
            }
        }
        private void DrawRemovingInfo(Parameter targetParameter, List<Parameter> parameters)
        {
            if (parameters.Contains(targetParameter) == false)
            {
                DrawRedText("Внимание! Выбранный параметр отсутствует у персонажа и не может быть удален.",
                    new Rect(0,0, _fullWidth - _lineHeight * 3, 0));
            }
        }

        protected void DrawParameterSelectFieldBlock()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Выберите параметр: ", GUILayout.Width(_labelWidth));
            DrawParameterSelectField();
            GUILayout.EndHorizontal();

            DrawRedText("Не выбран параметр.",
                new Rect(0,0, _fullWidth - _lineHeight * 3, 0));
        }

        protected virtual void DrawParameterSelectField()
        {
            //targetParameter = EditorGUILayout.ObjectField(targetParameter, typeof(Parameter), false,
            //    GUILayout.MaxWidth(200), GUILayout.MinWidth(20), GUILayout.Height(_lineHeight)) as Parameter;
        }

        protected override string GetAddingTabHeader()
        {
            return "Добавить параметр";
        }
        protected string GetChangingTabHeader()
        {
            return "Изменить параметр";
        }
        protected override string GetRemovingTabHeader()
        {
            return "Удалить параметр";
        }
        protected override string GetShowListHeader(Character character)
        {
            return "Показывать список параметров:";
        }
    }
}
#endif