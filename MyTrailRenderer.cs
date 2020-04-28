using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyTrailRenderer : MonoBehaviour
{
    public int ClonesPerSecond = 10;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Transform tf;
    private List<SpriteRenderer> clones;
    public Vector3 scalePerSecond = new Vector3(1f, 1f, 1f);
    public Color colorPerSecond = new Color(255, 255, 255, 1f);
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        tf = GetComponent<Transform>();
        sr = GetComponent<SpriteRenderer>();
        clones = new List<SpriteRenderer>();
        StartCoroutine(trail());
    }

    void Update()
    {
        for (int i = 0; i < clones.Count; i++)
        {
            clones[i].color -= colorPerSecond * Time.deltaTime;
            clones[i].transform.localScale -= scalePerSecond * Time.deltaTime;
            if (clones[i].color.a <= 0f || clones[i].transform.localScale == Vector3.zero)
            {
                Destroy(clones[i].gameObject);
                clones.RemoveAt(i);
                i--;
            }
        }
    }

    IEnumerator trail()
    {
        for (; ; ) //while(true)        
        {
            if (rb.velocity != Vector2.zero)
            {
                var clone = new GameObject("trailClone");
                clone.transform.position = tf.position;
                clone.transform.localScale = tf.localScale;
                var cloneRend = clone.AddComponent<SpriteRenderer>();
                cloneRend.sprite = sr.sprite;
                cloneRend.sortingOrder = sr.sortingOrder - 1;
                clones.Add(cloneRend);
            }
            yield return new WaitForSeconds(1f / ClonesPerSecond);
        }
    }
}
