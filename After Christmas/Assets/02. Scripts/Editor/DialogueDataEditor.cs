using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogueData))]
public class DialogueDataEditor : Editor
{
    private DialogueData dialogueData;
    private SerializedProperty dialogueLinesProp;

    private void OnEnable()
    {
        dialogueData = (DialogueData)target;
        dialogueLinesProp = serializedObject.FindProperty("dialogueLines");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 대화 논리 및 분기 설정
        EditorGUILayout.LabelField("▶ 대화 논리 및 분기 설정", EditorStyles.boldLabel);

        // Dialogue ID
        SerializedProperty dialogueIDProp = serializedObject.FindProperty("dialogueID");
        EditorGUILayout.PropertyField(dialogueIDProp, new GUIContent("Dialogue ID (0: No Record)"));

        // Prerequisite Condition
        SerializedProperty conditionProp = serializedObject.FindProperty("prerequisiteCondition");
        EditorGUILayout.PropertyField(conditionProp, new GUIContent("Prerequisite Condition"));

        // Fail-Safe Dialogue
        if (conditionProp.objectReferenceValue != null)
        {
            SerializedProperty failSafeProp = serializedObject.FindProperty("failSafeDialogue");
            EditorGUILayout.PropertyField(failSafeProp, new GUIContent("Fail-Safe Dialogue (Optional)"));
        }

        EditorGUILayout.Space(15);

        // 참여 캐릭터 목록
        EditorGUILayout.LabelField("▶ 참여 캐릭터 목록", EditorStyles.boldLabel);
        SerializedProperty participantsProp = serializedObject.FindProperty("participants");
        EditorGUILayout.PropertyField(participantsProp, true);

        GUILayout.Space(10);

        // 대화 라인
        EditorGUILayout.LabelField("▶ 대화 라인", EditorStyles.boldLabel);

        bool isExpanded = EditorGUILayout.PropertyField(dialogueLinesProp);

        if (isExpanded)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < dialogueLinesProp.arraySize; i++)
            {
                SerializedProperty lineProp = dialogueLinesProp.GetArrayElementAtIndex(i);

                bool lineExpanded = EditorGUILayout.PropertyField(lineProp, new GUIContent($"Line {i + 1}"));

                if (lineExpanded)
                {
                    EditorGUI.indentLevel++;
                    DrawCharacterDropdown(lineProp);
                    DrawSentenceTextArea(lineProp);
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    // 설정된 캐릭터들을 드롭다운으로 표시
    private void DrawCharacterDropdown(SerializedProperty lineProp)
    {
        SerializedProperty speakerProp = lineProp.FindPropertyRelative("speaker");

        string[] options = new string[dialogueData.participants.Length + 1];
        options[0] = "None";
        int selectedIndex = 0;

        for (int i = 0; i < dialogueData.participants.Length; i++)
        {
            if (dialogueData.participants[i] != null)
            {
                options[i + 1] = dialogueData.participants[i].characterName;

                if (speakerProp.objectReferenceValue == dialogueData.participants[i])
                {
                    selectedIndex = i + 1;
                }
            }
        }

        int newIndex = EditorGUILayout.Popup("Speaker (CharacterData)", selectedIndex, options);

        // 선택된 값 할당
        if (newIndex != selectedIndex)
        {
            if (newIndex == 0)
            {
                speakerProp.objectReferenceValue = null;
            }
            else
            {
                speakerProp.objectReferenceValue = dialogueData.participants[newIndex - 1];
            }
        }
    }

    // 문장 텍스트 영역 표시
    private void DrawSentenceTextArea(SerializedProperty lineProp)
    {
        SerializedProperty sentenceProp = lineProp.FindPropertyRelative("sentence");
        EditorGUILayout.PropertyField(sentenceProp, new GUIContent("Sentence"), true);
    }
}
