using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Division.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        private Canvas canvas;

        public TMP_Text dialogueText;
        public TMP_Text nameText;
        public Image characterStandingImage;

        private string currentText; //현재 입력 중인 string

        public List<Dialogue> dialogues; // 현재 출력되어야할 전체 다이얼로그
        private int currentDialogueIndex; // 위 dialogues에서 현재 출력할 인덱스
        private Dialogue currentDialogue; //현재 타이핑 해야하는 다이얼로그 정보
        private bool isDialogueTyping; //현재 타이핑 중인지(타이핑 중에 클릭 시, 모든 텍스트 띄워주기 위함)
        private bool isChoiceMode;

        public GameObject choiceWhole;
        public List<TMP_Text> choiceTextList;
        private List<int> choiceIndexList;

        [Serializable]
        public struct Dialogue
        {
            public string name;
            [TextArea] public string text;
            public Sprite sprite;
        }

        private void Start()
        {
            canvas = GetComponent<Canvas>();
            CloseChoiceMode();
            StartDialogue(dialogues); //test only
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isChoiceMode)
                {
                }
                else
                {
                    NextDialogue();
                }
            }
        }

        private void StartDialogue(List<Dialogue> newDialogues)
        {
            canvas.enabled = true;
            dialogues = newDialogues;
            currentDialogueIndex = 0;
            NextDialogue();
        }

        private void NextDialogue()
        {
            //여기 복잡한 if문은 나중에 가능하면 리팩터링하기...
            if (isDialogueTyping)
            {
                StopCoroutine("TypeDialogue");
                nameText.text = dialogues[currentDialogueIndex].name; // 이름 즉시 반영
                characterStandingImage.sprite = dialogues[currentDialogueIndex].sprite; // 스프라이트 즉시 반영
                dialogueText.text = dialogues[currentDialogueIndex].text; // 현재 텍스트 전체 띄우기

                isDialogueTyping = false; //타이핑 다 된 상태
                currentDialogueIndex++; //현재 인덱스 ++

                if (dialogues[currentDialogueIndex].name.Equals("선택지")) StartChoiceMode();
                return; //리턴 (한번 더 클릭해야 다음꺼 타이핑)
            }

            if (currentDialogueIndex >= dialogues.Count)
            {
                EndDialogue();
                return;
            }


            if (dialogues[currentDialogueIndex].name.Equals("선택지"))
            {
                StartChoiceMode();
                //현재 다이얼로그 인덱스 등을 절대 초기화하지 않는다.
                return;
            }

            if (dialogues[currentDialogueIndex].name.Equals("종료"))
            {
                EndDialogue();
                return;
            }

            StartCoroutine("TypeDialogue", dialogues[currentDialogueIndex]);
        }

        private void EndDialogue()
        {
            currentDialogueIndex = 0;
            canvas.enabled = false;
        }

        private IEnumerator TypeDialogue(Dialogue dialogue)
        {
            isDialogueTyping = true;
            currentText = "";
            var currentIndex = 0;
            nameText.text = dialogue.name;

            while (currentText.Length < dialogue.text.Length)
            {
                currentText += dialogue.text[currentIndex];
                currentIndex++;
                dialogueText.text = currentText;
                yield return new WaitForSeconds(0.06f);
            }

            isDialogueTyping = false;
            currentDialogueIndex++;
            if (dialogues[currentDialogueIndex].name.Equals("선택지")) NextDialogue();
        }

        /*
         [ 사실 걱정되는 거지? ]:0
         [ 책임…? ]:0
         */

        private void StartChoiceMode()
        {
            choiceWhole.SetActive(true);
            var choices = dialogues[currentDialogueIndex].text.Split('\n');
            Debug.Log(choices.Length + "개의 선택지 발견됨!");
            choiceIndexList = new List<int>();

            for (var i = 0; i < choices.Length; i++)
            {
                //예시    [ 사실 걱정되는 거지? ]:0
                var choiceSplit = choices[i].Split(" ]:");
                var choiceName = choiceSplit[0].Substring(1, choiceSplit[0].Length - 1);
                var index = int.Parse(choiceSplit[1]);
                choiceIndexList.Add(index); //예시: 0
                choiceTextList[i].text = choiceName; //예시: 
            }

            StartCoroutine(ChoiceOn(choices.Length));
        }

        private IEnumerator ChoiceOn(int choiceLength)
        {
            for (var i = 0; i < choiceIndexList.Count; i++)
                if (i < choiceLength)
                {
                    choiceTextList[i].transform.parent.gameObject.SetActive(true);
                    yield return new WaitForSeconds(0.25f);
                }
                else
                {
                    choiceTextList[i].transform.parent.gameObject.SetActive(false);
                }
        }

        private IEnumerator ChoiceOff(int choiceSelected)
        {
            for (var i = 0; i < choiceIndexList.Count; i++)
                if (i == choiceSelected)
                    choiceTextList[i].color = Color.yellow;
                else
                    choiceTextList[i].transform.parent.GetComponent<Animator>().SetTrigger("off");

            yield return new WaitForSeconds(0.5f);
            choiceTextList[choiceSelected].transform.parent.GetComponent<Animator>().SetTrigger("off");
            yield return new WaitForSeconds(1f);
            CloseChoiceMode();
        }


        public void ChoiceSelect(int index)
        {
            currentDialogueIndex = choiceIndexList[index];
            StartCoroutine(ChoiceOff(index));
            //CloseChoiceMode();
            NextDialogue();
        }

        private void CloseChoiceMode()
        {
            isChoiceMode = false;

            for (var i = 0; i < choiceTextList.Count; i++)
            {
                choiceTextList[i].color = Color.white;
                choiceTextList[i].transform.parent.gameObject.SetActive(false);
            }

            choiceWhole.SetActive(false);
        }
    }
}