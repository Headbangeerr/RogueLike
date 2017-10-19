using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    public float speed = 4;
    public float restTime = 0.3f;
    public float restTimer = 0;
    //音效
    public AudioClip chop1Audio;
    public AudioClip chop2Audio;
    public AudioClip stepAudio1;
    public AudioClip stepAudio2;
    public AudioClip foodAudio1;
    public AudioClip foodAudio2;
    public AudioClip sodaAudio1;
    public AudioClip sodaAudio2;
    //公共属性，在属性面板中隐藏
    [HideInInspector] public Vector2 targetPosition = new Vector2(1, 1);

    private Rigidbody2D rigidbody2d;
    private BoxCollider2D collider2d;
    private Animator animator;

    void Start() {
        rigidbody2d = GetComponent<Rigidbody2D>();
        collider2d = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
    }
    public void GetHurt(int damage)
    {
        GameManager.Instance.LossEnergy(damage);
        animator.SetTrigger("Injured");
    }
    void Update() {
        rigidbody2d.MovePosition(Vector2.Lerp(transform.position, targetPosition, speed * Time.deltaTime));
        if (GameManager.Instance.energy <= 0 || GameManager.Instance.isExit) return;
        restTimer += Time.deltaTime;
        //如果休息计时没有结束，则直接跳出
        if (restTimer < restTime ) return;
        
        //GetAxisRaw不采用平滑过渡，返回值只有-1，0,1
        //这里的Horizontal与Vertical代表wasd与上下左右四个摁键
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        //只能往一个方向移动，水平方向优先：即当同时摁下右键与下键，则人物向右移动
        if(h!=0)//如果水平方向有移动
        {
            v = 0;
        }
        if(h!=0||v!=0)
        {
            collider2d.enabled = false;
            //使用物理射线判断是否与物体发生了碰撞
            RaycastHit2D hit = Physics2D.Linecast(targetPosition, targetPosition + new Vector2(h, v));
            collider2d.enabled = true;
            //如果没有发生碰撞，则前进
            if(hit.transform==null)
            {
                AudioController.Instance.RandomPlay(new AudioClip []{stepAudio1,stepAudio2 });
                targetPosition += new Vector2(h, v);
                GameManager.Instance.LossEnergy(1);
            }
            else
            {               
                switch (hit.collider.tag)
                {
                    case "OutWall":
                        break;
                    case "Wall":
                        animator.SetTrigger("Attack");
                        AudioController.Instance.RandomPlay(new AudioClip[] { chop1Audio,chop2Audio });
                        hit.collider.SendMessage("GetHit");
                        GameManager.Instance.LossEnergy(1);
                        break;
                    case "Food":
                        AudioController.Instance.RandomPlay(new AudioClip[] { foodAudio1 , foodAudio2 });
                        GameManager.Instance.GetEnergy(10);
                        GameManager.Instance.LossEnergy(1);
                        targetPosition += new Vector2(h, v);
                        Destroy(hit.collider.gameObject);
                        break;
                    case "Soda":
                        AudioController.Instance.RandomPlay(new AudioClip[] { sodaAudio1, sodaAudio2 });
                        GameManager.Instance.GetEnergy(20);
                        GameManager.Instance.LossEnergy(1);
                        targetPosition += new Vector2(h, v);
                        Destroy(hit.collider.gameObject);
                        break;
                    case "EnemyTarget":
                        List<Transform> enemyTargetList = new List<Transform>();
                        foreach (Enemy enemy in GameManager.Instance.enemyList)
                        {
                            //将每个Enemy的占位傀儡删除
                            foreach(Transform child in enemy.transform)
                            {
                                enemyTargetList.Add(child);
                            }
                        }
                        foreach(Transform enemyTarget in enemyTargetList)
                        {
                            GameObject.Destroy(enemyTarget.gameObject);
                        }
                        collider2d.enabled = false;
                        //使用物理射线判断是否与物体发生了碰撞
                        RaycastHit2D targetHit = Physics2D.Linecast(targetPosition, targetPosition + new Vector2(h, v));
                        collider2d.enabled = true;

                        if(targetHit.collider!=null)
                        {
                            if (targetHit.collider.tag != "Enemy" && targetHit.collider.tag != "Wall"&& targetHit.collider.tag!="OutWall")
                            {
                                GameManager.Instance.LossEnergy(1);
                                targetPosition += new Vector2(h, v);
                            }
                        }
                        else
                        {
                            GameManager.Instance.LossEnergy(1);
                            targetPosition += new Vector2(h, v);
                        }
                        break;
                }
            }
            GameManager.Instance.OnPlayerMove(targetPosition);
            //无论是移动还是攻击都需要休息
            restTimer = 0;
        }
    }
}
