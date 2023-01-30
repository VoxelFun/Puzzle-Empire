using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour {

    static AnimationController animationController;

    readonly WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    private void Awake() {
        animationController = this;
    }

    private void OnEnable() {
        animationController = this;
    }

    public static void Start(AnimationType.Base @base) {
        animationController.StartCoroutine(animationController.Animate(@base));
    }

    private IEnumerator Animate(AnimationType.Base @base) {
        if(@base.trigger != null) @base.trigger.OnAnimationBegin();
        while (!@base.end) {
            @base.Animate();
            yield return waitForFixedUpdate;
        }
        if (@base.trigger != null) @base.trigger.OnAnimationEnd();
    }

    public static void Start(AnimationType.Base @base, float length) {
        animationController.StartCoroutine(animationController.Animate(@base, length));
    }

    private IEnumerator Animate(AnimationType.Base @base, float length) {
        length *= Engine.fps;
        if (@base.trigger != null) @base.trigger.OnAnimationBegin();
        for (float i = 0; i < length; i++) {
            @base.Animate(i / length);
            yield return waitForFixedUpdate;
        }
        @base.Animate(1);
        if (@base.trigger != null) @base.trigger.OnAnimationEnd();
    }

    public static void StartWithController(AnimationType.Base @base, float length) {
        animationController.StartCoroutine(animationController.AnimateWithController(@base, length));
    }

    private IEnumerator AnimateWithController(AnimationType.Base @base, float length) {
        length *= Engine.fps;
        if (@base.trigger != null) @base.trigger.OnAnimationBegin();
        for (float i = 0; i < length; i++) {
            @base.Animate(i / length);
            yield return waitForFixedUpdate;
            if (@base.WasModified())
                yield break;
        }
        @base.Animate(1);
        if (@base.trigger != null) @base.trigger.OnAnimationEnd();
    }

    public static void Stop() {
        animationController.StopAllCoroutines();
    }

}

public class AnimationType {

    #region Base

    public abstract class Base {

        public bool end;

        readonly public IAnimationTrigger trigger;

        public Base(IAnimationTrigger trigger) {
            this.trigger = trigger;
        }

        public abstract void Animate();
        public abstract void Animate(float progress);

        public virtual bool WasModified() { return false; }
    }

    public abstract class Transform : Base {
        readonly protected UnityEngine.Transform transform;

        public Transform(IAnimationTrigger trigger, UnityEngine.Transform transform) : base(trigger) {
            this.transform = transform;
        }
    }

    public abstract class Position : Transform {
        readonly protected Vector3 start;
        readonly protected Vector3 target;

        public Position(IAnimationTrigger trigger, UnityEngine.Transform transform, Vector3 target) : base(trigger, transform) {
            start = transform.localPosition;
            this.target = target;
        }
    }

    #endregion

    #region Canvas

    public class CanvasGroup : Base {
        readonly protected UnityEngine.CanvasGroup canvasGroup;

        readonly protected int startValue = 0;
        readonly protected int multiplier = 1;

        protected float value;

        public CanvasGroup(IAnimationTrigger trigger, UnityEngine.CanvasGroup canvasGroup, bool desc = true) : base(trigger) {
            this.canvasGroup = canvasGroup;
            if (desc) {
                startValue = 1; multiplier = -1;
            }
        }

        public override void Animate(float progress) {
            value = startValue + progress * multiplier;
            canvasGroup.alpha = value;
        }

        public override void Animate() {
            throw new System.NotImplementedException();
        }

        public override bool WasModified() {
            return value != canvasGroup.alpha;
        }

    }

    #endregion

    #region Position

    public class LerpPosition : Position {

        public LerpPosition(IAnimationTrigger trigger, UnityEngine.Transform transform, Vector3 target) : base(trigger, transform, target) {
            
        }

        public override void Animate(float progress) {
            transform.localPosition = Vector3.Lerp(start, target, progress);
        }

        public override void Animate() {
            throw new System.NotImplementedException();
        }
    }

    public class MoveTowards : Position {
        readonly protected float speed;

        public MoveTowards(IAnimationTrigger trigger, UnityEngine.Transform transform, Vector3 target, float speed) : base(trigger, transform, target) {
            this.speed = speed / Engine.fps;
        }

        public override void Animate(float progress) {
            throw new System.NotImplementedException();
        }

        public override void Animate() {
            Vector3 result = Vector3.MoveTowards(transform.localPosition, target, speed);
            transform.localPosition = result;
            if (target == result)
                end = true;
        }
    }

    public class MoveTowardsMany : MoveTowards {
        readonly protected UnityEngine.Transform[] transforms;

        public MoveTowardsMany(IAnimationTrigger trigger, Vector3 target, float speed, params UnityEngine.Transform[] transforms) : base(trigger, null, target, speed) {
            this.transforms = transforms;
        }

        public override void Animate(float progress) {
            throw new System.NotImplementedException();
        }

        public override void Animate() {
            Vector3 result = Vector3.MoveTowards(transform.localPosition, target, speed);
            foreach (UnityEngine.Transform transform in transforms)
                transform.localPosition = result;
            if (target == result)
                end = true;
        }
    }

    #endregion


}