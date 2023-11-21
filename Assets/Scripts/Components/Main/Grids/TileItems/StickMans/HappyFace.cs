using DG.Tweening;
using UnityEngine;

namespace Components.Main.Grids.TileItems.StickMans
{
    public class HappyFace : Emoji
    {
        public override void DoAnim()
        {
            TweenContainer.Clear();
            _spriteRenderer.enabled = true;

            _transform.localScale = Vector3.zero;
            TweenContainer.AddTween = _transform.DOScale(Vector3.one, 0.5f);
            
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