using System.Collections;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System;

public class Enemy_Skeleton : Entity,IPunObservable
{

    public PhotonView PV;
    bool isAttacking;
    [Header("Move info")]
    [SerializeField] private float moveSpeed = 1f;

    [Header("Player Detection")]
    [SerializeField] private float playerCheckDistance;
    [SerializeField] private LayerMask whatIsPlayer;
    private RaycastHit2D isPlayerDetected;

    public Player player;
    [SerializeField] private float experiencePoints = 90;

    protected override void Awake()
    {
        base.Awake();
        FindLocalPlayer();
    }

    protected override void Start()
    {
        base.Start();

        if (AttackTransform == null)
        {
            AttackTransform = transform;
        }
    }

    protected override void Update()
    {
        base.Update();

        // HpController();
        Movement();

        if (!isGrounded || isWallDetected) // 벽 혹은 땅쪽일 경우 방향 전환
        {
            FlipController();
        }
    }


    private void FlipController()
    {
        if (rb.velocity.x > 0 && !facingRight)
        {
            PV.RPC("FlipRPC", RpcTarget.AllBuffered, true);

        }
        else if (rb.velocity.x < 0 && facingRight)
        {
            PV.RPC("FlipRPC", RpcTarget.AllBuffered, false);

        }
    }

    [PunRPC]
    private void FlipRPC(bool faceRight)
    {
        facingRight = faceRight;
        facingDir = faceRight ? 1 : -1;
        transform.Rotate(0, 180, 0);
        hpBar.transform.Rotate(0,180,0);
        HpText.transform.Rotate(0,180,0);
    }

    private void FindLocalPlayer()
    {
        Player[] players = FindObjectsOfType<Player>();
        foreach (var player in players)
        {
            if (player.GetComponent<PhotonView>().IsMine)
            {
                this.player = player;
                break;
            }
        }
    }

    private void Movement()
    {
        if (isPlayerDetected) //플레이어 발견 여부에 따른 이동속도 및 행동
        {
            if (isPlayerDetected.distance > 1)
            {
                rb.velocity = new Vector2(moveSpeed * 1.5f * facingDir, rb.velocity.y); //적 발견시 이동
                Debug.Log("I see the player");
                isAttacking = false;
            }
            else //공격 사거리 내 플레이어 접근 시
            {
                Debug.Log("Attack " + isPlayerDetected.collider.gameObject.name);
                isAttacking = true;
                AttackPlayer(isPlayerDetected.collider.gameObject.GetComponent<PhotonView>().ViewID);
                // AttackPlayer();
            }
        }
        else //평상시
        {
            rb.velocity = new Vector2(moveSpeed * facingDir, rb.velocity.y);
        }
    }

    protected override void CollisionCheck()
    {
        base.CollisionCheck();
        isPlayerDetected = Physics2D.Raycast(transform.position, Vector2.right, playerCheckDistance * facingDir, whatIsPlayer);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + playerCheckDistance * facingDir, transform.position.y));
    }

    // public override void Hited(float _damageDone, Vector2 _hitDirection)
    // {
    //     base.Hited(_damageDone, _hitDirection);
    //     if (Hp <= 0)
    //     {
    //         Vector3 respawnPosition = transform.position;

    //         if (PhotonNetwork.IsMasterClient)
    //         {
    //             PhotonNetwork.Destroy(gameObject);
    //             EnemyManager enemyManager = FindObjectOfType<EnemyManager>();
    //             if (enemyManager != null && PhotonNetwork.IsMasterClient)
    //             {
    //                 enemyManager.RespawnEnemy(respawnPosition);
    //             }
    //         }
    //         else
    //         {
    //             PV.RPC("RequestDestroy", RpcTarget.MasterClient, PV.ViewID, respawnPosition);
    //         }
    //         // 적을 처치한 플레이어에게 경험치 부여
    //         foreach (PhotonView attacker in attackers)
    //         {
    //             if (attacker != null)
    //             {
    //                 attacker.RPC("AddExpRPC", attacker.Owner, experiencePoints);
    //             }
    //         }

    //     }
    // }

    [PunRPC]
    public void HitedRPC(float _damageDone, Vector2 _hitDirection)
    {
        Hp -= _damageDone; //체력 감소
        HpBarController(Hp);// 체력바 업데이트
        ShowDamageText(_damageDone, transform.position);// 데미지 표시 텍스트 메서드 호출

        if (Hp <= 0)
        {
            Vector3 respawnPosition = transform.position;

            if (PhotonNetwork.IsMasterClient)// 마스터 클라이언트 일 경우 
            {
                PhotonNetwork.Destroy(gameObject);//네트워크 상에서 적을 파괴함.
                EnemyManager enemyManager = FindObjectOfType<EnemyManager>();
                if (enemyManager != null && PhotonNetwork.IsMasterClient)
                {
                    enemyManager.RespawnEnemy(respawnPosition);// 적 리스폰
                }
            }
            else
            {
                PV.RPC("RequestDestroy", RpcTarget.MasterClient, PV.ViewID, respawnPosition); //마스터 클라이언트에게 파괴 요청.
            }

            // 적을 처치한 플레이어에게 경험치 부여
            foreach (PhotonView attacker in attackers)
            {
                if (attacker != null)
                {
                    QuestManager.instance.UpdateKillCount();
                    attacker.RPC("AddExpRPC", attacker.Owner, experiencePoints);                    
                }
            }
        }
    }




    [PunRPC]
    private void RequestDestroy(int viewID, Vector3 respawnPosition)
    {
        PhotonView enemyPV = PhotonView.Find(viewID);
        if (enemyPV != null && PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(enemyPV.gameObject);
            EnemyManager enemyManager = FindObjectOfType<EnemyManager>();
            if (enemyManager != null)
            {
                enemyManager.RespawnEnemy(respawnPosition);
            }
        }
    }

    public void AttackPlayer(int playerViewID)// 플레이어 공격
    {
        PhotonView playerPV = PhotonView.Find(playerViewID);
        if (playerPV != null && playerPV.IsMine)
        {
            Player player = playerPV.GetComponent<Player>();
            if (player != null && !player.invincible)
            {
                Debug.Log("플레이어 공격중");
                player.TakeDamage(damage);
            }
        }
    }
    public override void HpBarController(float hp)
    {
        base.HpBarController(hp);
        if (hpBar != null)
        {
            hpBar.fillAmount = hp / maxHp;
        }
        if (HpText != null)
        {
            HpText.text = hp.ToString("F0"); // 텍스트로 변환
        }
    }

    public override float Hp
    {
        get { return hp; }
        set
        {
            if (hp != value)
            {
                hp = Mathf.Clamp(value, 0, maxHp);
                HpBarController(hp);

            }
        }
    }


     public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Hp);
            // stream.SendNext(maxHp);
        }
        else
        {
            Hp = (float)stream.ReceiveNext();
            // maxHp = (float)stream.ReceiveNext();
            // HpBarController(hp);
        }
    }
}