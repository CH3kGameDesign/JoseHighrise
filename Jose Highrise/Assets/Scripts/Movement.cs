using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    public static staticValuesClass staticValues = new staticValuesClass();
    [Header (">>>> Tinker Free Zone <<<<")]
    public referenceClass References;
    [Header("---- Sandbox ----")]
    public moveValueClass MoveValues;
    [Space(5)]
    public cameraValueClass CameraValues;

    private timerClass timers = new timerClass();
    private inputClass inputs = new inputClass();

    [System.Serializable]
    public class staticValuesClass
    {
        public bool B_canMove = true;
    }
    [System.Serializable]
    public class referenceClass
    {
        public Transform T_cam;
        public Rigidbody RB;
    }

    [System.Serializable]
    public class inputClass
    {
        public Vector2 v2_tarDir = Vector2.zero;
        public bool b_jump = false;
        public bool b_swap = false;

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
    public class cameraValueClass
    {
        public float F_moveSpeed;
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
        
    }

    // Update is called once per frame
    void Update()
    {
        if (staticValues.B_canMove)
        {
            Move();
            Jump();
        }
    }

    private void Move()
    {
        Vector3 tarDir = Vector3.zero;
        if (inputs.v2_tarDir.magnitude > 0)
        {
            tarDir = new Vector3(inputs.v2_tarDir.x, 0, 0) * MoveValues.F_moveSpeed;

        }
        tarDir.x -= References.RB.velocity.x * MoveValues.F_xResistanceMultiplier;
        References.RB.AddForce(tarDir * (Time.deltaTime / Time.fixedDeltaTime));
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

    public void TriggerStay(Collider other)
    {
        if (other.tag == "Ground")
        {
            inputs.b_grounded = true;
            timers.f_coyoteTimer = MoveValues.F_coyoteTime;
        }
    }
}
