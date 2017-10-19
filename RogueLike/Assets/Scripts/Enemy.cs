using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
    public float smoothing = 3;
    //攻击力
    public int damage = 10;
    //占位傀儡预制体
    public GameObject EnemyTarget;

    //音效
    public AudioClip enemyAttack;

    private Transform player;
    private Vector2 targetPosition;
    private Rigidbody2D rigidbody2d;
    private Animator animator;
    private BoxCollider2D boxCollider2d;


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        targetPosition = transform.position;
        rigidbody2d = gameObject.GetComponent<Rigidbody2D>();
        boxCollider2d = gameObject.GetComponent<BoxCollider2D>();
        animator = gameObject.GetComponent<Animator>();
        //enemyList初始化
        GameManager.Instance.enemyList.Add(this);
    }

    void Update()
    {
        rigidbody2d.MovePosition(Vector2.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime));   
    }
    public void AttackCheck(Vector3 playerPosition)
    {
        if(this.transform.position==null)
        {
            Debug.Log("attackCheck" + this.transform.position);
        }
        
        Vector2 offset = playerPosition - this.transform.position;
        if (offset.magnitude < 1.1f)//如果与玩家相接触则攻击
        {
            AudioController.Instance.RandomPlay(new AudioClip[] { enemyAttack });
            animator.SetTrigger("Attack");
            player.SendMessage("GetHurt", damage);
        }       
    }
    public void AttackCheck(Vector3 playerPosition,Vector3 enemyPosition)
    {
        Vector2 offset = playerPosition - enemyPosition;
        if (offset.magnitude < 1.1f)//如果与玩家相接触则攻击
        {
            animator.SetTrigger("Attack");
            player.SendMessage("GetHurt", damage);
        }
    }
  
    private void Move(Vector2 targetPosition)
    {
        float x = (float)Math.Round(targetPosition.x);
        float y =(float) Math.Round(targetPosition.y); 
        //在目标地点摆放一个占位傀儡
        GameObject puppet = GameObject.Instantiate(EnemyTarget, new Vector2(x,y), Quaternion.identity);
        puppet.transform.SetParent(this.transform);
        this.targetPosition = targetPosition;
    }
    public Vector2 MoveCheck(Vector2 playerPosition)
    {
        Vector2 checkPosition=Vector2.zero;
        //是否可以移动
        //bool canMove=false;
        //获取自身与Player的偏移向量
        Vector2 offset = player.position - transform.position;
        if (offset.magnitude > 1.1f)//否则进行移动接近玩家
        {
            int x=0, y=0;
            if(Mathf.Abs(offset.y)>Mathf.Abs(offset.x))//如果偏移向量的纵坐标大于横坐标，则纵向移动
            {
                if (offset.y > 0)
                {
                    y = 1;
                }
                else
                {
                    y = -1;
                }
            }
            else
            {
                if (offset.x > 0)
                {
                    x = 1;
                }
                else
                {
                    x = -1;
                }
            }
            //由于射线的起点是自身中点，如果不将自身的碰撞体关闭掉，射线的碰撞检测永远都会先碰撞到自身的collider
            boxCollider2d.enabled = false;
            RaycastHit2D hit = Physics2D.Linecast(targetPosition, targetPosition + new Vector2(x, y));
            boxCollider2d.enabled = true;
            if (hit.transform == null)//如果在这个方向上没有物体则前进
            {
                //如果Enemy与Player的目的地一样的话，他们同时移动就会造成Enemy与Player的重叠
                if((targetPosition + new Vector2(x, y))!=playerPosition)
                {
                    Move(targetPosition + new Vector2(x, y));
                }
            }
            else//如果前进方向有物体，则进行判断
            {
                if (hit.collider.tag == "Food" || hit.collider.tag == "Soda")
                {
                    //如果Enemy与Food重叠时，物理射线检测则会一直检测到的是Food，不能判断前面的物体是Wall还是Enemy
                    //这时需要先将重叠在一起的Food的Collider关闭，然后在判断前进方向是否有Wall或者Enemy
                    Collider2D hitFood = hit.collider;
                    hitFood.enabled = false;
                    this.boxCollider2d.enabled = false;
                    hit = Physics2D.Linecast(targetPosition, targetPosition + new Vector2(x, y));
                    this.boxCollider2d.enabled = true;
                    hitFood.enabled = true;                   
                    if (hit.collider == null)
                    {
                        if((targetPosition + new Vector2(x, y)) != playerPosition)
                        {
                            Move(targetPosition + new Vector2(x, y));
                        }
                    }
                    else if (hit.collider.tag != "Wall" && hit.collider.tag != "Enemy" && hit.collider.tag!="EnemyTarget")
                    {                       
                        if((targetPosition + new Vector2(x, y)) != playerPosition)
                        {                           
                            Move(targetPosition + new Vector2(x, y));
                        }
                    }                    
                }
                //else if (hit.collider.tag == "Enemy")
                //{
                //    float posX = (float)Math.Round(hit.collider.transform.position.x);
                //    float posY = (float)Math.Round(hit.collider.transform.position.y);
                //    Vector2 colliderPos = new Vector2(posX, posY);
                //    posX = (float)Math.Round(this.transform.position.x);
                //    posY=(float)(float)Math.Round(this.transform.position.y);
                //    Vector2 myPos = new Vector2(posX, posY);
                //   // 为了防止enemy移动过程中由于目的地一致而造成两个enemy重叠在一起不能移动的情况，设置一个计数器，如果两个enemy是碰撞了两次，则可以穿过彼此
                //   if(colliderPos==myPos)
                //   {
                //        Debug.Log("僵尸重叠啦");
                //        targetPosition += new Vector2(x, y);
                //   }
                //}
            }
            //Debug.Log("canMove:" + canMove);
            //Debug.Log("targetCheck" + targetCheck);
            //if (canMove && targetCheck)
            //{
            //    targetPosition += new Vector2(x, y);
            //}           
            //this.AttackCheck(playerPosition, targetPosition + new Vector2(x, y));
        }
        return checkPosition;
    }
    /// <summary>
    /// 在Enemy要进行移动时，先向其他的Enemy发送自身要移动到的目的地，然后让其他所有Enemy进行判断，
    /// 如果自身的目的地与接收到到的目的地一致的话就不进行移动
    /// 这里的int类型的检查标志位的3种情况为
    /// </summary>
    /// <param name="myTargetPos"></param>
    /// <param name="otherTargerPos"></param>
    //public void CheckPoint(Vector2 otherTargerPos)
    //{
    //    this.targetCheck = true;
    //    Debug.Log("myPos" + targetPosition);
    //    Debug.Log("ohterPos" + otherTargerPos);
    //    if (targetPosition == otherTargerPos)
    //    {
    //        this.targetCheck = false;
    //        Debug.Log("moveCheck:" + targetCheck);
    //    }      
    //}
    /// <summary>
    /// 向除了自身以外的所有Enemy广播发送自己的目的地
    /// </summary>
    //private void Broadcast()
    //{
    //    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
    //    foreach (GameObject temp in enemies)
    //    {
    //        if (this.transform.position != temp.transform.position)
    //        {
    //            temp.GetComponent<Enemy>().CheckPoint((this.targetPosition+moveDistance));
    //            //temp.SendMessage("CheckPoint", this.targetPosition);
    //        }
    //    }
    //}
    //private bool CheckTargetPos(Vector2 myTargetPos)
    //{
    //    bool check=true;
    //    int checkNum = 0;
    //    foreach (Vector2 checkPos in GameManager.Instance.lockPosList)
    //    {
    //        if(checkPos==myTargetPos)
    //        {
    //            checkNum++;
    //        }
    //    }
    //    if(checkNum>1)
    //    {
    //        check = false;
    //    }
    //    return check;
    //}
    //public void Move()
    //{
    //    //如果Enemy能够移动
    //    if(this.checkPosition!=Vector2.zero)
    //    {
    //        if (CheckTargetPos(this.checkPosition))
    //        {
    //            targetPosition = this.checkPosition;
    //        }
    //    }
    //}
}
