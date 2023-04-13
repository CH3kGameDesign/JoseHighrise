using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMoveArray : MonoBehaviour
{
    public List<RectTransform> pointList = new List<RectTransform>();
    private int step = 1;
    private int lastStep = 0;
    private float timeSinceLastStep = 0;
    public float timer = 1;
    public AnimationCurve curve;
    private RectTransform RT;
    // Start is called before the first frame update
    void Start()
    {
        RT = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastStep += Time.deltaTime;
        RT.localScale = Vector3.Lerp(pointList[lastStep].localScale, pointList[step].localScale, curve.Evaluate(timeSinceLastStep / timer));
        RT.localRotation = Quaternion.Lerp(pointList[lastStep].localRotation, pointList[step].localRotation, curve.Evaluate(timeSinceLastStep / timer));
        if (timeSinceLastStep > timer)
        {
            timeSinceLastStep = 0;
            lastStep = step;
            step++;
            if (step >= pointList.Count)
                step = 0;
        }
    }
}
