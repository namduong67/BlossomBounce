using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Limbs : MonoBehaviour
{
    [SerializeField] private Rigidbody rigid;
    [SerializeField] private GameObject[] leftSpines;
    [SerializeField] private GameObject[] rightSpines;
    [SerializeField] private Transform leftTarget;
    [SerializeField] private Transform rightTarget;
    [SerializeField] private float rotateSpeed = 650f;

    public static bool isRotate = false;
    public static bool collided = false;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isRotate)
        {
            if (rigid.velocity.y <= 0.1f)
            {
                for (int i = 0; i < rightSpines.Length; i++)
                {
                    rightSpines[i].transform.rotation = Quaternion.RotateTowards(rightSpines[i].transform.rotation, rightTarget.rotation, rotateSpeed * Time.deltaTime); //(Vector3.back * 180 * Time.fixedDeltaTime * smoth);
                    if (rightSpines[i].transform.rotation.z >= 180)
                    {
                        isRotate = true;
                    }
                }

                for (int i = 0; i < leftSpines.Length; i++)
                {
                    leftSpines[i].transform.rotation = Quaternion.RotateTowards(leftSpines[i].transform.rotation, leftTarget.rotation, rotateSpeed * Time.deltaTime); //(Vector3.back * 180 * Time.fixedDeltaTime * smoth);
                    if (leftSpines[i].transform.rotation.z <= -180)
                    {
                        isRotate = true;
                    }
                }
            }
        }

        if (collided)
        {
            for (int i = 0; i < rightSpines.Length; i++)
            {
                rightSpines[i].transform.rotation = Quaternion.RotateTowards(rightSpines[i].transform.rotation, Quaternion.Euler(0, 0, 0), rotateSpeed * Time.deltaTime);
                if (rightSpines[i].transform.rotation.z <= 0)
                {
                    collided = false;
                }
            }

            for (int i = 0; i < leftSpines.Length; i++)
            {
                leftSpines[i].transform.rotation = Quaternion.RotateTowards(leftSpines[i].transform.rotation, Quaternion.Euler(0, 0, 0), rotateSpeed * Time.deltaTime);
                if (leftSpines[i].transform.rotation.z >= 0)
                {
                    collided = false;
                }
            }
        }
    }
}
