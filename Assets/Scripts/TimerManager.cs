using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TimerManager : MonoBehaviour
{
    public TMP_Text timerText;
    private Canvas timerCanvas;
    private PlayerController playerController;
    private Enemy enemy;
    public float timeLeft;
    public float bonusTimeFromCoins = 20f;
    public bool canAutoConvertScoreToTime = true;

    private void Awake()
    {
        timerCanvas = GameObject.FindGameObjectWithTag("TimerCanvas").GetComponent<Canvas>();
        if (GetComponent<PlayerController>() != null)
        {
            playerController = GetComponent<PlayerController>();
            timeLeft = 60f;
        }
        else
        {
            enemy = GetComponent<Enemy>();
            timeLeft = enemy.spawnTime;
        }
    }

    private void Start()
    {
        timerText = Instantiate(timerText, timerCanvas.transform).GetComponent<TMP_Text>();
    }

    private void Update()
    {
        timerText.transform.position = Camera.main.WorldToScreenPoint(transform.position);
        if (playerController != null) timeLeft -= Time.deltaTime * Mathf.Sqrt(Mathf.Pow(playerController.playerMovement.x, 2) + Mathf.Pow(playerController.playerMovement.y, 2));
        else timeLeft -= Time.deltaTime;
        if(timeLeft <= 0)
        {
            if(playerController != null)
            {
                if (playerController.score > 0 && canAutoConvertScoreToTime)
                {
                    playerController.score--;
                    timeLeft += bonusTimeFromCoins;
                    return;
                }
            }
            else if(enemy.health > 1)
            {
                enemy.health--;
                timeLeft += 15f;
                return;
            }
            Destroy(timerText.gameObject);
            Destroy(gameObject);
        }
        timerText.text = Mathf.Round(timeLeft).ToString();
    }
}