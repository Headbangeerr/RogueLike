using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour {
    //预制体 
    public GameObject[] outWallArray;
    public GameObject[] floorArray;
    public GameObject[] wallArray;
    public GameObject[] foodArray;
    public GameObject[] enemyArray;
    public GameObject exit;

    private GameManager gameManager;
    private Transform mapHolder;

    //地图格子行数
    public int rows=10;
    //地图格子列数
    public int columns = 10;
    public int minWallnumber = 2;
    public int maxWallnumber = 15;
    private List<Vector2> positionList = new List<Vector2>();
	void Awake () {
        gameManager = this.GetComponent<GameManager>();
	}
	
	void Update () {
		
	}
    private Vector2 RandomPosition()
    {
        //随机获取生成范围中的一个坐标
        int maxSize = positionList.Count - 1;
        int positionIndex = Random.Range(0, maxSize);
        //Debug.Log("maxsize:" +( positionList.Count - 1));
        Vector2 pos = positionList[positionIndex];
        //利用索引值移除已经放置了障碍的坐标
        positionList.RemoveAt(positionIndex);
        return pos;
    }
    private GameObject RandomPrefabs(GameObject[] prefabs)
    {
        int arrayIndex = Random.Range(0, prefabs.Length);
        return prefabs[arrayIndex];
    }
    private void CreatePerfab(int createNum,GameObject[] prefabs)
    {
        for(int i=0;i<createNum;i++)
        {
            GameObject prefabTemp = RandomPrefabs(prefabs);
            GameObject.Instantiate(prefabTemp, RandomPosition(), Quaternion.identity).transform.SetParent(mapHolder);
        }
    }

    /// <summary>
    /// 初始化地图
    /// </summary>
    public void InitMap()
    {
        mapHolder = new GameObject("Map").transform;
        for(int x= 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                //初始化外围墙
                if(x==0||x==columns-1||y==0||y==rows-1)
                {
                    int index = Random.Range(0, outWallArray.Length);
                    GameObject.Instantiate(outWallArray[index], new Vector2(x, y), Quaternion.identity).transform.SetParent(mapHolder);
                }
                //初始化地板
                else
                {
                    int index = Random.Range(0, floorArray.Length);
                    GameObject.Instantiate(floorArray[index], new Vector2(x, y), Quaternion.identity).transform.SetParent(mapHolder) ;

                }
            }
        }
        //创建食物、怪物与障碍物
        positionList.Clear();
        for(int x = 2; x < columns-2; x++)
        {
            for(int y = 2; y < rows-2; y++)
            {
                positionList.Add(new Vector2(x, y));
            }
        }
        //随机生成障碍的个数
        int wallNumber = Random.Range(minWallnumber, maxWallnumber);
        //生成障碍
        CreatePerfab(wallNumber, wallArray);

        //生成食物个数范围：（2，当前天数*2），这里如果采用与level相关的话可能会出现下表越界的错误
        int maxFoodNum;
        if (gameManager.level*2>positionList.Count)
        {
            maxFoodNum = positionList.Count-5;
        }
        else
        {
            maxFoodNum = gameManager.level * 2;
        }
        int foodNum = Random.Range(2,maxFoodNum);
        CreatePerfab(foodNum, foodArray);

        //生成敌人 敌人个数范围：（0，level/2）
        int enemyMaxSize;
        if(gameManager.level/2>positionList.Count)
        {
            enemyMaxSize = positionList.Count;
        }
        else
        {
            enemyMaxSize = gameManager.level / 2;
        }
        int enemyNum = Random.Range(0,enemyMaxSize);
        CreatePerfab(enemyNum, enemyArray);

        //生成出口
        Instantiate(exit, new Vector2(columns - 2, rows - 2), Quaternion.identity).transform.SetParent(mapHolder);
        
    }
   
}
