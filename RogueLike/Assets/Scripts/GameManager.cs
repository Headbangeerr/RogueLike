using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    //关卡数
    public int level = 1;
    public int energy = 100;
    //public List<Vector2> lockPosList = new List<Vector2>();

    //获取所有的Enemy集合
    public List<Enemy> enemyList = new List<Enemy>();
    //通过该标志位实现Player走两步，Enemy走一步
    private bool enemySleep = true;

    //UI
    private Text energyText;
    private Text failText;
    private Image dayImage;
    private Text dayText;
    private Player player;
    private MapManager mapManager;
    //音效
    public AudioClip dieAudio;
    //判断是否到达出口位置
    [HideInInspector] public bool isExit=false;
    //单例模式
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            return _instance;
        }
    }
    private void Awake()
    {
        _instance = this;
        InitGame();
        DontDestroyOnLoad(this.gameObject);
    }
    void InitGame()
    {
        Debug.Log("InitGame");
        energyText = GameObject.Find("EnergyText").GetComponent<Text>();
        failText = GameObject.Find("FailText").GetComponent<Text>();
        dayImage = GameObject.Find("DayImage").GetComponent<Image>();
        dayText = GameObject.Find("DayText").GetComponent<Text>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        dayText.text = "Day:" + level;
        Invoke("HideDayImage", 1);
        mapManager = GetComponent<MapManager>();
        //初始化地图
        mapManager.InitMap();
        failText.enabled = false;
        UpdateEnergyText();
        isExit = false;
    }
    void UpdateEnergyText()
    {
        energyText.text = "Energy:" + energy;
    }
    public void GetEnergy(int number)
    {
        energy += number;
        UpdateEnergyText();
    }
    public void LossEnergy(int number)
    {
        energy -= number;
        UpdateEnergyText();
        if (energy <= 0)
        {
            AudioController.Instance.RandomPlay(new AudioClip[] { dieAudio });
            AudioController.Instance.StopBGM();
            failText.enabled = true;
        }
    }
    //public void LockPosition(Vector2 pos)
    //{
    //    this.lockPosList.Add(pos);
    //}
    
    public void OnPlayerMove(Vector2 playerTargetPos)
    {
        //在玩家移动之前先将锁定位置集合清空
        //this.lockPosList.Clear();
        if (enemyList.Count != 0)//如果地图中有敌人的情况
        {
            foreach (Enemy enemy in enemyList)
            {
                enemy.AttackCheck(playerTargetPos);
            }
            if (enemySleep)//第一次先修改标志位
            {
                enemySleep = false;
            }
            else//第二次进行移动
            {
                //Vector2 enemyTargetPos = new Vector2();
                //foreach (Enemy enemy in enemyList)
                //{

                //    //由Enemy判断自身下一步是否可以移动
                //    enemyTargetPos = enemy.MoveCheck(playerTargetPos);
                //    enemy.checkPosition = enemyTargetPos;
                //    //如果Enemy可以移动，则将目标位置锁定
                //    if (enemyTargetPos!=Vector2.zero)
                //    {
                //        this.LockPosition(enemyTargetPos);
                //    }
                //}

                foreach (Enemy enemy in enemyList)
                {
                    //将每个Enemy的占位傀儡删除
                    foreach (Transform children in enemy.transform)
                    {
                        Destroy(children.gameObject);
                    }
                    enemy.MoveCheck(playerTargetPos);
                }
                enemySleep = true;
              
            }
        }
         if (player.targetPosition == new Vector2(mapManager.columns - 2, mapManager.rows - 2))
         {
            isExit = true;
            //重新载入关卡
            SceneManager.LoadScene("Main");
            this.enemyList.Clear();
         }
    }
    private void OnLevelWasLoaded(int scenelevel)
    {
        this.level++;
        InitGame();
    }
    private void HideDayImage()
    {
        dayImage.gameObject.SetActive(false);
    }
}
