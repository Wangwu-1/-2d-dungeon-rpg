using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animator_Atteck_Trigger_useless: MonoBehaviour
{

    private Player player;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponentInParent<Player>();
    }

    // Update is called once per frame
   // private void AnimatorTrigger()
   // {
   //   Player_useless.AtteckOver();
   //  }
}
