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
    private EnemyBehavior enemy;
    public float timeLeft;
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
            enemy = GetComponent<EnemyBehavior>();
            timeLeft = enemy.enemyStats.spawnTime;
        }
    }

    private void Start()
    {
        timerText = Instantiate(timerText, timerCanvas.transform).GetComponent<TMP_Text>();
    }

    private void Update()
    {
        timerText.transform.position = Camera.main.WorldToScreenPoint(transform.position);
        if (playerController != null)
        { 
            if (playerController.playerMovement != Vector2.zero) timeLeft -= Time.deltaTime * playerController.timeConsumeSpeed; 
        }
        else if (!enemy.enemyStats.isBoss) timeLeft -= Time.deltaTime;
        if(timeLeft <= 0)
        {
            if(playerController != null)
            {
                if (playerController.score > 0 && canAutoConvertScoreToTime)
                {
                    playerController.ChangeScore(-1);
                    timeLeft += 20f;
                    return;
                }
            }
            else if(enemy.enemyStats.health > 1)
            {
                enemy.enemyStats.health--;
                timeLeft += 15f;
                return;
            }
            Destroy(timerText.gameObject);
            Destroy(gameObject);
        }
        timerText.text = Mathf.Round(timeLeft).ToString();
    }
}