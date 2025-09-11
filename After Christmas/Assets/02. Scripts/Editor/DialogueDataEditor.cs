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

        // participants 목록
        SerializedProperty participantsProp = serializedObject.FindProperty("participants");
        EditorGUILayout.PropertyField(participantsProp, true);

        GUILayout.Space(10);

        // dialogueLines 배열의 토글과 크기 조절 필드
        bool isExpanded = EditorGUILayout.PropertyField(dialogueLinesProp);

        if (isExpanded)
        {
            EditorGUI.indentLevel++;
            // 배열 내의 각 요소를 순회하며 그리기
            for (int i = 0; i < dialogueLinesProp.arraySize; i++)
            {
                SerializedProperty lineProp = dialogueLinesProp.GetArrayElementAtIndex(i);

                // 각 대화 라인마다 토글로
                bool lineExpanded = EditorGUILayout.PropertyField(lineProp, new GUIContent($"Line {i}"));

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

    // 설정된 캐릭터들을 드롭다운으로 표시하는 메서드
    private void DrawCharacterDropdown(SerializedProperty lineProp)
    {
        SerializedProperty characterProp = lineProp.FindPropertyRelative("characterData");

        // 드롭다운에 표시할 옵션 목록
        string[] options = new string[dialogueData.participants.Length + 1];
        options[0] = "None";
        int selectedIndex = 0;

        for (int i = 0; i < dialogueData.participants.Length; i++)
        {
            if (dialogueData.participants[i] != null)
            {
                options[i + 1] = dialogueData.participants[i].characterName;
                if (characterProp.objectReferenceValue == dialogueData.participants[i])
                {
                    selectedIndex = i + 1;
                }
            }
        }

        // 드롭다운 메뉴
        int newIndex = EditorGUILayout.Popup("Character Data", selectedIndex, options);

        // 선택이 변경되었을 때만 할당
        if (newIndex != selectedIndex)
        {
            if (newIndex == 0)
            {
                characterProp.objectReferenceValue = null;
            }
            else
            {
                characterProp.objectReferenceValue = dialogueData.participants[newIndex - 1];
            }
        }
    }

    // 문장 텍스트 영역을 그리는 메서드
    private void DrawSentenceTextArea(SerializedProperty lineProp)
    {
        SerializedProperty sentenceProp = lineProp.FindPropertyRelative("sentence");
        EditorGUILayout.PropertyField(sentenceProp, true);
    }
}