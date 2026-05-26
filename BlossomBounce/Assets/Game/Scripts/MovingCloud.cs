using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingCloud : MonoBehaviour
{
    public float speedX = 1f;
    public float speedY = 0.1f;
    public float minX, maxX, minY, maxY;
    private void Start()
    {
        /*float randomScale = Random.Range(0.2f, 0.5f);
        float randomPosX = Random.Range(minX, maxX);
        float randomPosY = Random.Range(minY, maxY);
        transform.localScale = new Vector3(randomScale, randomScale, randomScale);
        transform.position = new Vector3(randomPosX, randomPosY, transform.position.z);*/
/*        float randomScale = Random.Range(0.2f, 0.5f);
        transform.localScale = new Vector3(randomScale, randomScale, randomScale);

        float randomPosX = Random.Range(minX, maxX);
        float randomPosY = Random.Range(minY+2, maxY-2);
        transform.localPosition = new Vector3(randomPosX, randomPosY, transform.localPosition.z);
*/
    }

    private void Update()
    {
        if(Input.GetMouseButton(0))
        {
            transform.localPosition = new Vector3(
                    transform.localPosition.x + (speedX * Time.deltaTime),
                    transform.localPosition.y + (speedY * Time.deltaTime),
                    transform.localPosition.z );
        }    
        else transform.localPosition = new Vector3(
                    transform.localPosition.x + (speedX * Time.deltaTime),
                    transform.localPosition.y ,
                    transform.localPosition.z);

        
        if (transform.position.x > maxX)
        {
            transform.position = new Vector3(minX, transform.position.y, transform.position.z);
        }
        if(transform.localPosition.y >maxY)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, minY, transform.localPosition.y);
        }
    }

    
}
