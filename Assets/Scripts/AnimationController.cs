using UnityEngine;

public class AnimationController : MonoBehaviour
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

    private float speedUpRemaining = 0;

    void Update()
    {
        if (speedUpRemaining > 0)
        {
            SetAnimationSpeedInSeconds(timeClickController.timeToFill / speedUpOnClick);
            speedUpRemaining -= Time.deltaTime * speedUpOnClick;
        }
        else
        {
            SetAnimationSpeedInSeconds(timeClickController.timeToFill);
        }
    }

    /// <summary>
    /// Sets the speed of the animation so that it completes in the given amount of duration.
    /// </summary>
    /// <param name="seconds">The time the animation should take in seconds.</param>
    void SetAnimationSpeedInSeconds(float seconds)
    {
        float animationLength = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        animator.speed = animationLength / seconds;
    }


    void OnClick()
    {
        speedUpRemaining += timeClickController.timeAddedPerClick;
    }
}
