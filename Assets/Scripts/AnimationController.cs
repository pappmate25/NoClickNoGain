using UnityEngine;
using UnityEngine.Serialization;

public class AnimationController : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("AnimationTime")]
    private FloatVariable animationTime;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        SetAnimationSpeedInSeconds(animationTime.Value);
    }

    //void OnClick()
    //{
    //    int currentStateHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
    //    float currentProgress = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

    //    // Plays the current animation but further into the animation. This is the only way to skip ahead in an animation in Unity in this context.
    //    // Multiplying with animator.speed is required because the normalizedTime parameter is not scaled with speed,
    //    // so we have to scale the amount we give it manually.
    //    animator.Play(currentStateHash, 0, currentProgress + (timeClickController.timeAddedPerClick * animator.speed));
    //}

    /// <summary>
    /// Sets the speed of the animation so that it completes in the given amount of duration.
    /// </summary>
    /// <param name="seconds">The time the animation should take in seconds.</param>
    void SetAnimationSpeedInSeconds(float seconds)
    {
        float animationLength = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        animator.speed = animationLength / seconds;
    }

    /*
    [SerializeField] private float speedUpOnClick = 3f;
    private float speedUpRemaining = 0;
    void SetAnimationSpeedWithSpeedUp()
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
    void OnClick()
    {
        speedUpRemaining += timeClickController.timeAddedPerClick;
    }*/
}
