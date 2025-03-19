using UnityEngine;

public class AnimationController: MonoBehaviour
{
    [SerializeField] private float speedUpOnClick = 3f;
    
    private Animator animator;
    private TimeClickController timeClickController;

    void Start()
    {
        animator = GetComponent<Animator>();
        timeClickController = GetComponent<TimeClickController>();

        timeClickController.OnClick.AddListener(OnClick);
    }
    
    private float speedUpRemaining;
    
    void Update()
    {
       animator.speed = 10 / timeClickController.timeToFill;

       if (speedUpRemaining >= 0)
       {
           animator.speed *= speedUpOnClick;
           speedUpRemaining -= Time.deltaTime * speedUpOnClick;
       }
    }


    void OnClick()
    {
        speedUpRemaining += timeClickController.timeAddedPerClick;
    }
}
