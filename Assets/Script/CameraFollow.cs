using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour

{

    [SerializeField] private float followSpeed = 0.1f; //ī�޶� ������� �ӵ�
    [SerializeField] private Vector3 offSet;

    public GameObject player;

    void Update()
    {
        transform.position= Vector3.Lerp(transform.position, player.transform.position+offSet,followSpeed);
    }
}
