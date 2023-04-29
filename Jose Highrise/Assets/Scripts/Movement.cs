using Jose.Maps;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Movement : MonoBehaviour
{
    public static staticValuesClass staticValues = new staticValuesClass();
    [Header (">>>> Tinker Free Zone <<<<")]
    public referenceClass References;
    [Header("---- Sandbox ----")]
    public moveValueClass MoveValues;
    [Space(5)]
    public swapValueClass SwapValues;
    [Space(5)]
    public cameraValueClass CameraValues;

    [HideInInspector]
    public Vector2Int gridPos;
    [HideInInspector]
    public Vector2Int gridDir;

    public blackoutClass blackout;

    private timerClass timers = new timerClass();
    private inputClass inputs = new inputClass();

    [System.Serializable]
    public class staticValuesClass
    {
        public bool B_canMove = true;
        public bool B_dead = false;
        public bool B_finish = false;
        public int I_score = 0;
    }
    [System.Serializable]
    public class referenceClass
    {
        public Transform T_cam;
        public Rigidbody RB;
        public TileManager TM;
    }
    [System.Serializable]
    public class blackoutClass
    {
        public RectTransform mask;
        public Vector2 openSize = new Vector3(2000,2000);
        public AnimationCurve closingAnim;
        public AnimationCurve openAnim;
        public float openTimer = 0.5f;
        public float closeTimer = 0.3f;
        [HideInInspector]
        public float timeSinceStart = 0;
        [HideInInspector]
        public bool open = true;
        [HideInInspector]
        public int scene;
    }

    public class blockBreakClass
    {
        public Transform tar;
        public float timer = 0;
        public float delay = 0.3f;
        public float speed = 0.1f;
        public AnimationCurve scaleCurve = new AnimationCurve();
        public Vector3 startScale = Vector3.one;
        public float tarMultScale = 1.2f;
    }

    public class blockColorClass
    {
        public List<Block> blocks = new List<Block>();
        public float speed = 0.2f;
        public float delay = 0.2f;
        public float timer = 0;
        public Color color;
        public AnimationCurve colorCurve;
    }

    [System.Serializable]
    public class inputClass
    {
        public Vector2 v2_tarDir = Vector2.zero;
        public bool b_jump = false;
        public bool b_swap = false;
        public bool b_swapHeld = false;

        public bool b_grounded = false;
    }
    [System.Serializable]
    public class moveValueClass
    {
        public float F_moveSpeed;
        public float F_xResistanceMultiplier;
        [Space(10)]
        public float F_jumpForce;
        public float F_jumpDuration;
        public AnimationCurve F_jumpCurve;
        public float F_coyoteTime;
    }
    [System.Serializable]
    public class swapValueClass
    {
        public float initialSwapBreakDelay = 0.4f;
        public float subsequentSwapBreakDelay = 0.2f;
        public float swapSpeed = 0.1f;
        public AnimationCurve breakScaleCurve;
        public Color breakColor = Color.grey;
    }

    [System.Serializable]
    public class cameraValueClass
    {
        public float F_moveSpeed;
        public Vector3 offset;
        public Vector2 deadzone;
    }

    [System.Serializable]
    public class timerClass
    {
        public float f_hangTimer = 0;
        public float f_coyoteTimer = 0;
        public float f_jumpTimer = 0;
    }
    // Start is called before the first frame update
    void Start()
    {
        blackoutClass temp = CopyBlackout(blackout,true);
        staticValues.B_dead = false;
        staticValues.B_finish = false;
        staticValues.I_score = 0;
        StopCoroutine("openCloseBlackout");
        StartCoroutine("openCloseBlackout", temp);
    }

    // Update is called once per frame
    void Update()
    {
        if (staticValues.B_canMove)
        {
            Move();
            Jump();
            Swap();
        }
        
    }
    private void FixedUpdate()
    {
        CameraMovement();
    }

    private void CameraMovement()
    {
        Vector3 tarPos = References.RB.position + CameraValues.offset;
        tarPos.x = Mathf.MoveTowards(tarPos.x, References.T_cam.position.x, CameraValues.deadzone.x);
        tarPos.y = Mathf.MoveTowards(tarPos.y, References.T_cam.position.y, CameraValues.deadzone.y);
        References.T_cam.position = Vector3.Lerp(References.T_cam.position, tarPos, Time.deltaTime * CameraValues.F_moveSpeed);
    }

    private void Move()
    {
        Vector3 tarDir = Vector3.zero;
        if (inputs.v2_tarDir.magnitude > 0)
        {
            tarDir = new Vector3(inputs.v2_tarDir.x, 0, 0) * MoveValues.F_moveSpeed;
            if (Mathf.Abs(inputs.v2_tarDir.x) >= Mathf.Abs(inputs.v2_tarDir.y))
            {
                if (inputs.v2_tarDir.x > 0)
                    gridDir = new Vector2Int(1, 0);
                else
                    gridDir = new Vector2Int(-1, 0);
            }
            else
            {
                if (inputs.v2_tarDir.y > 0)
                    gridDir = new Vector2Int(0,1);
                else
                    gridDir = new Vector2Int(0,-1);
            }

        }
        tarDir.x -= References.RB.velocity.x * MoveValues.F_xResistanceMultiplier;
        References.RB.AddForce(tarDir * (Time.deltaTime / Time.fixedDeltaTime));
        gridPos = new Vector2Int(Mathf.RoundToInt(References.RB.position.x), Mathf.RoundToInt(References.RB.position.y));
    }

    private void Jump()
    {
        if (inputs.b_grounded && inputs.b_jump && timers.f_jumpTimer <= 0)
        {
            timers.f_jumpTimer = MoveValues.F_jumpDuration;
            References.RB.velocity = new Vector3(References.RB.velocity.x, 0, References.RB.velocity.z);
        }

        if (timers.f_jumpTimer > 0)
        {
            float force = MoveValues.F_jumpCurve.Evaluate(1 - (timers.f_jumpTimer / MoveValues.F_jumpDuration)) * MoveValues.F_jumpForce;

            References.RB.AddForce(Vector3.up * force * (Time.deltaTime / Time.fixedDeltaTime),ForceMode.Impulse);
            timers.f_jumpTimer -= Time.deltaTime;
        }

        if (timers.f_coyoteTimer > 0)
        {
            timers.f_coyoteTimer -= Time.deltaTime;
        }
        else
            inputs.b_grounded = false;
    }

    private void Swap()
    {
        if (inputs.b_swap)
        {
            if (!inputs.b_swapHeld)
            {
                inputs.b_swapHeld = true;
                Vector3Int tarBlock = new Vector3Int(gridPos.x + gridDir.x, gridPos.y + gridDir.y, 0);
                int blockID = References.TM.curMap.level[tarBlock.x, tarBlock.y] - 1;
                if (blockID >= 0)
                {
                    switch (References.TM.blockList.blocks[blockID].grabbability)
                    {
                        case Jose.Objects.ObjectList.grabEnum.none:
                            break;
                        case Jose.Objects.ObjectList.grabEnum.grab:
                            break;
                        case Jose.Objects.ObjectList.grabEnum.swap:
                            References.TM.curMap.level[tarBlock.x, tarBlock.y] = References.TM.curMap.level[gridPos.x, gridPos.y];
                            References.TM.curMap.level[gridPos.x, gridPos.y] = blockID + 1;
                            Transform block = References.TM.transform.GetChild(0).GetChild(tarBlock.x).GetChild(tarBlock.y).GetChild(0);
                            Transform block2 = References.TM.transform.GetChild(0).GetChild(gridPos.x).GetChild(gridPos.y);

                            Vector3 tarDir = new Vector3(gridDir.x, gridDir.y, 0);
                            TileManager.MoveClass tempMove = new TileManager.MoveClass();
                            tempMove.tarPos = Vector3.zero;
                            tempMove.startPos = tarDir;
                            tempMove.tar = block.GetChild(0);
                            tempMove.timer = SwapValues.swapSpeed;
                            References.TM.StartCoroutine("MoveToLocal", tempMove);

                            if (block2.childCount > 0)
                            {
                                TileManager.MoveClass tempMove2 = new TileManager.MoveClass();
                                tempMove2.tar = block2.GetChild(0);
                                tempMove2.startPos = -tarDir;
                                tempMove2.tarPos = Vector3.zero;
                                tempMove2.timer = SwapValues.swapSpeed;

                                block2.GetChild(0).GetComponent<Block>().gridPos = gridPos + gridDir;
                                block2.GetChild(0).parent = block.parent;
                                
                                References.TM.StartCoroutine("MoveToLocal", tempMove2);
                            }
                            TileManager.MoveClass tempMove3 = new TileManager.MoveClass();
                            tempMove3.startPos = -tarDir;
                            tempMove3.tarPos = Vector3.zero;
                            tempMove3.timer = SwapValues.swapSpeed;
                            tempMove3.tar = References.RB.transform.GetChild(0);
                            References.TM.StartCoroutine("MoveToLocal", tempMove3);

                            block.parent = block2;
                            block.GetComponent<Block>().gridPos = gridPos;
                            block.localPosition = Vector3.zero;

                            References.TM.UpdateSprites(gridPos);
                            References.TM.UpdateSprites(gridPos+gridDir);


                            References.RB.position = tarBlock;
                            
                            SwapCheck(block.GetComponent<Block>());
                            
                            break;
                        case Jose.Objects.ObjectList.grabEnum.grabAndSwap:
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        else
            inputs.b_swapHeld = false;
    }

    public void SwapCheck(Block temp)
    {
        List<Block> blocks = new List<Block>();
        blocks.Add(temp);
        List<int> delay = new List<int>();
        delay.Add(0);
        bool[] check = getBreakBools(temp);
        for (int i = 0; i < blocks.Count; i++)
        {
            for (int x = Mathf.Max(blocks[i].gridPos.x - 1,0); x < Mathf.Min(blocks[i].gridPos.x + 2,References.TM.curMap.width); x++)
            {
                Block temp2 = References.TM.GetBlock(new Vector2Int(x, blocks[i].gridPos.y));
                if (temp2 != null)
                {
                    if (!blocks.Contains(temp2))
                    {
                        bool[] check2 = getBreakBools(temp2);
                        for (int j = 0; j < 6; j++)
                        {
                            if (check2[j] && check[j])
                            {
                                blocks.Add(temp2);
                                delay.Add(delay[i] + 1);
                            }
                        }
                    }
                }
            }
            for (int y = Mathf.Max(blocks[i].gridPos.y - 1, 0); y < Mathf.Min(blocks[i].gridPos.y + 2, References.TM.curMap.height); y++)
            {
                Block temp2 = References.TM.GetBlock(new Vector2Int(blocks[i].gridPos.x, y));
                if (temp2 != null)
                {
                    if (!blocks.Contains(temp2))
                    {
                        bool[] check2 = getBreakBools(temp2);
                        for (int j = 0; j < 6; j++)
                        {
                            if (check2[j] && check[j])
                            {
                                blocks.Add(temp2);
                                delay.Add(delay[i] + 1);
                            }
                        }
                    }
                }
            }
        }
        if (blocks.Count > 1)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                References.TM.curMap.level[blocks[i].gridPos.x, blocks[i].gridPos.y] = 0;
                blockBreakClass bbc = new blockBreakClass();
                bbc.tar = blocks[i].transform;
                bbc.scaleCurve = SwapValues.breakScaleCurve;
                bbc.startScale = blocks[i].transform.GetChild(0).localScale;
                bbc.delay = SwapValues.initialSwapBreakDelay + delay[i] * SwapValues.subsequentSwapBreakDelay;
                StartCoroutine("BlockBreak", bbc);
            }
            blockColorClass bcc = new blockColorClass();
            bcc.blocks = blocks;
            bcc.color = SwapValues.breakColor;
            bcc.colorCurve = SwapValues.breakScaleCurve;
            StartCoroutine("BlockColor", bcc);
        }
    }

    bool[] getBreakBools(Block temp)
    {
        bool[] check = new bool[6];
        check[0] = temp.info.breaksWhenSwappedWith.aqua;
        check[1] = temp.info.breaksWhenSwappedWith.blue;
        check[2] = temp.info.breaksWhenSwappedWith.green;
        check[3] = temp.info.breaksWhenSwappedWith.orange;
        check[4] = temp.info.breaksWhenSwappedWith.purple;
        check[5] = temp.info.breaksWhenSwappedWith.red;
        return check;
    }

    public void OnHorizontal(InputAction.CallbackContext cnt)
    {
        inputs.v2_tarDir.x = cnt.ReadValue<float>();
    }

    public void OnVertical(InputAction.CallbackContext cnt)
    {
        inputs.v2_tarDir.y = cnt.ReadValue<float>();
    }

    public void OnSwap(InputAction.CallbackContext cnt)
    {
        inputs.b_swap = cnt.performed;
    }

    public void OnJump(InputAction.CallbackContext cnt)
    {
        inputs.b_jump = cnt.performed;
    }

    public void Death()
    {
        staticValues.B_dead = true;
        blackoutClass temp = CopyBlackout(blackout, false);
        References.RB.velocity = Vector3.zero;
        References.RB.isKinematic = true;
        References.RB.GetComponent<Collider>().enabled = false;
        StopCoroutine("openCloseBlackout");
        StartCoroutine("openCloseBlackout", temp);
    }

    public void FinishLevel()
    {
        staticValues.B_finish = true;
        StaticData.levelNum++;
        blackoutClass temp = CopyBlackout(blackout, false);
        References.RB.velocity = Vector3.zero;
        References.RB.isKinematic = true;
        References.RB.GetComponent<Collider>().enabled = false;

        if(StaticData.testingMode)
        {
            SceneManager.LoadScene(2);
            return;
        }
        if (StaticData.levelNum >= StaticData.currentPlaylist.maps.Count)
        {
            StaticData.levelNum = 0;
            temp.scene = 0;
        }
        else
        {
            TextAsset levelTemp = StaticData.currentPlaylist.maps[StaticData.levelNum];
            StaticData.currentMap = Deserialize<Map>(levelTemp.bytes);

        }
        StopCoroutine("openCloseBlackout");
        StartCoroutine("openCloseBlackout", temp);

    }

    private T Deserialize<T>(byte[] param)
    {
        using (MemoryStream ms = new MemoryStream(param))
        {
            IFormatter br = new BinaryFormatter();
            return (T)br.Deserialize(ms);
        }
    }

    public blackoutClass CopyBlackout(blackoutClass tempBlackout, bool open)
    {
        blackoutClass temp = new blackoutClass();
        temp.timeSinceStart = 0;
        temp.open = open;
        temp.openSize = tempBlackout.openSize;
        temp.openTimer = tempBlackout.openTimer;
        temp.closeTimer = tempBlackout.closeTimer;
        temp.closingAnim = tempBlackout.closingAnim;
        temp.openAnim = tempBlackout.openAnim;
        temp.mask = tempBlackout.mask;
        temp.scene = 1;
        return temp;
    }
    public void TriggerStay(Collider other)
    {
        if (other.tag == "Block")
        {
            Block block = other.GetComponent<Block>();
            if (block.info.canJumpOn)
            {
                inputs.b_grounded = true;
                timers.f_coyoteTimer = MoveValues.F_coyoteTime;
            }
            if (block.info.deadly)
            {
                if (!staticValues.B_dead)
                    Death();
            }
            if (block.info.finishOnTouch)
            {
                if (!staticValues.B_finish)
                    FinishLevel();
            }
            if (block.info.unlockOnTouch)
            {
                References.TM.curMap.level[block.gridPos.x, block.gridPos.y] = 0;
                GameObject.Destroy(other.gameObject);
                References.TM.UnlockAllBlocks();
            }
            if (block.info.gainPointsOnTouch)
            {
                References.TM.curMap.level[block.gridPos.x, block.gridPos.y] = 0;
                GameObject.Destroy(other.gameObject);
                References.TM.UpdateSprites(block.gridPos);

                staticValues.I_score++;
            }
        }
    }
    
    public void CollisionStay(Collision collision)
    {
        if (collision.collider.tag == "Block")
        {
            if (collision.collider.GetComponent<Block>().info.deadly)
            {
                if (!staticValues.B_dead)
                    Death();
            }
        }
    }

    public IEnumerator BlockBreak(blockBreakClass bbc)
    {
        while (bbc.timer < bbc.delay)
        {
            bbc.timer += 0.02f;
            yield return new WaitForSecondsRealtime(0.02f);
        }
        bbc.timer = 0;
        while (bbc.timer < bbc.speed)
        {
            bbc.timer += 0.02f;
            if (bbc.tar != null)
                bbc.tar.GetChild(0).localScale = Vector3.Lerp(bbc.startScale, bbc.startScale * bbc.tarMultScale, bbc.scaleCurve.Evaluate(bbc.timer / bbc.speed));
            else
                break;
            yield return new WaitForSecondsRealtime(0.02f);
        }
        if (bbc.tar != null)
            GameObject.Destroy(bbc.tar.gameObject);
    }

    public IEnumerator BlockColor(blockColorClass bcc)
    {
        while (bcc.timer < bcc.delay)
        {
            bcc.timer += 0.02f;
            yield return new WaitForSecondsRealtime(0.02f);
        }
        bcc.timer = 0;
        while (bcc.timer < bcc.speed)
        {
            bcc.timer += 0.02f;
            Color temp = Color.Lerp(Color.white, bcc.color, bcc.colorCurve.Evaluate(bcc.timer / bcc.speed));
            foreach (var item in bcc.blocks)
            {
                item.transform.GetChild(0).GetComponent<SpriteRenderer>().color = temp;
            }
            yield return new WaitForSecondsRealtime(0.02f);
        }
        foreach (var item in bcc.blocks)
        {
            item.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    private IEnumerator openCloseBlackout(blackoutClass move)
    {
        float timer = move.closeTimer;
        if (move.open)
            timer = move.openTimer;

        while (move.timeSinceStart < timer)
        {
            move.mask.anchoredPosition = Camera.main.WorldToScreenPoint(References.RB.position);
            move.timeSinceStart += Time.deltaTime;
            if (move.open && !staticValues.B_dead)
                move.mask.sizeDelta = Vector2.Lerp(Vector2.zero, move.openSize, move.openAnim.Evaluate(move.timeSinceStart / timer));
            else
                move.mask.sizeDelta = Vector2.Lerp(move.openSize, Vector2.zero, move.closingAnim.Evaluate(move.timeSinceStart / timer));
            yield return new WaitForSecondsRealtime(0.02f);
        }
        if (!move.open)
            SceneManager.LoadScene(move.scene);
    }

    public void OnRespawn()
    {
        if (!staticValues.B_dead)
            Death();
    }

    public void OnQuit()
    {
        if (StaticData.testingMode)
            SceneManager.LoadScene(2);
        else
            SceneManager.LoadScene(0);
    }
}
