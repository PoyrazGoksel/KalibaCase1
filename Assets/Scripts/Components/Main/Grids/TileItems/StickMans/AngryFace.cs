using DG.Tweening;
using UnityEngine;

namespace Components.Main.Grids.TileItems.StickMans
{
    public class AngryFace : Emoji
    {
        public override void DoAnim()
        {
            TweenContainer.Clear();
            _spriteRenderer.enabled = true;

            TweenContainer.AddTween = _transform.DOPunchScale(1.05f * Vector3.one, 0.5f, 10, 0.45f);
            
            TweenContainer.AddedTween.onComplete += delegate
            {
                _spriteRenderer.enabled = false;
            };
            
            TweenContainer.AddedTween.onUpdate += delegate
            {
                _transform.eulerAngles = LookCamRot;
            };
        }
    }
}