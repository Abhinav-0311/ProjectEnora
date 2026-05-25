using UnityEngine;

public class InteractionMessages : MonoBehaviour
{
    public void InteractionMessage(string str)
    {
        if (NarrativeHUD.Instance != null)
            NarrativeHUD.Instance.ShowSubtitle(str, 4f);
        Debug.Log(str);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
