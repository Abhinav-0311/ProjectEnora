using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
   
    public Animator anim;
    public void Togglebool(string boolname){
        anim.SetTrigger(boolname);
    }
}
