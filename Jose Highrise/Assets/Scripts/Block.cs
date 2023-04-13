using Jose.Objects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [HideInInspector]
    public Vector2Int gridPos;
    public ObjectList.spriteClass info;
    // Update is called once per frame
    [HideInInspector]
    public float shootTimer;

    public void UpdateBlockInfo(Vector2Int tempPos, ObjectList.spriteClass tempInfo)
    {
        gridPos = tempPos;
        info = tempInfo;
    }


    public void Update()
    {
        if (info.shootInfo.bullet != null)
            ShootHandler();
        if (info.moveInfo.moveType != ObjectList.moveTypeEnum.doesntMove)
            MoveHandler();
    }
    public void ShootHandler()
    {
        shootTimer += Time.deltaTime;
        if (shootTimer > info.shootInfo.firerate)
        {
            shootTimer = 0;
            GameObject GO = Instantiate(info.shootInfo.bullet);
            GO.transform.position = transform.position + new Vector3(info.shootInfo.spawnOffset.x, info.shootInfo.spawnOffset.y,0);
            GO.GetComponent<Block>().info.moveInfo.dir = Vector3.Normalize(info.shootInfo.dir);
            if (info.shootInfo.destroyAfter > 0)
                GameObject.Destroy(GO, info.shootInfo.destroyAfter);
        }
    }

    public void MoveHandler()
    {
        /*
        if (info.moveInfo.faceDir)
            transform.forward = info.moveInfo.dir;
        */
        transform.position += new Vector3(info.moveInfo.dir.x,info.moveInfo.dir.y) * info.moveInfo.moveSpeed * Time.deltaTime;
    }
}
