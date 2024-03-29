using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    [SerializeField] private MagicWand m_MagicWand;

    public GameObject[] m_Objects;
    
    void Start()
    {
        m_MagicWand.OnSwipe.AddListener(OnSwipHandler);
    }

    private void OnSwipHandler(SwipeDirection arg0)
    {
        var index = (int)arg0;
        if (index >= 0)
        {
            m_Objects[index].SetActive(false);
            StartCoroutine(DelayShow(index));
        }
    }

    IEnumerator DelayShow(int index)
    {
        yield return new WaitForSeconds(2);
        m_Objects[index].SetActive(true);
    }
}