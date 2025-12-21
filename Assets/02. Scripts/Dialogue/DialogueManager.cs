using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    Canvas canvas;
    
    public TMP_Text dialogueText;
    public TMP_Text nameText;
    public Image characterStandingImage;
    
    private string currentText; //현재 입력 중인 string
    
    public List<Dialogue> dialogues; // 현재 출력되어야할 전체 다이얼로그
    private int currentDialogueIndex=0; // 위 dialogues에서 현재 출력할 인덱스
    Dialogue currentDialogue; //현재 타이핑 해야하는 다이얼로그 정보
    bool isDialogueTyping = false; //현재 타이핑 중인지(타이핑 중에 클릭 시, 모든 텍스트 띄워주기 위함)
    
    [System.Serializable]
    public struct Dialogue
    {
        public string name;
        [TextArea]public string text;
        public Sprite sprite;
    }
    
    void Start()
    {
        canvas=GetComponent<Canvas>();
        StartDialogue(dialogues); //test only
    }

    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NextDialogue();
        }
    }

    public void StartDialogue(List<Dialogue> newDialogues)
    {
        canvas.enabled = true;
        this.dialogues = newDialogues;
        currentDialogueIndex = 0;
        NextDialogue();
    }

    public void NextDialogue()
    {
        if (isDialogueTyping)
        {
            StopCoroutine("TypeDialogue");
            nameText.text = dialogues[currentDialogueIndex].name; // 이름 즉시 반영
            characterStandingImage.sprite = dialogues[currentDialogueIndex].sprite; // 스프라이트 즉시 반영
            dialogueText.text = dialogues[currentDialogueIndex].text; // 현재 텍스트 전체 띄우기
            
            isDialogueTyping = false; //타이핑 다 된 상태
            currentDialogueIndex++; //현재 인덱스 ++
            
            return; //리턴 (한번 더 클릭해야 다음꺼 타이핑)
        }

        if (currentDialogueIndex >= dialogues.Count)
        {
            currentDialogueIndex = 0;
            canvas.enabled = false;
            return;
        }
        StartCoroutine("TypeDialogue", (dialogues[currentDialogueIndex]));
    }

    IEnumerator TypeDialogue(Dialogue dialogue)
    {
        isDialogueTyping = true;
        currentText = "";
        int currentIndex = 0;
        nameText.text = dialogue.name;
        
        while (currentText.Length < dialogue.text.Length)
        {
            currentText += dialogue.text[currentIndex];
            currentIndex++;
            dialogueText.text=currentText;
            yield return new WaitForSeconds(0.06f);
        }
        
        isDialogueTyping = false;
        currentDialogueIndex++;
    }
    
}
