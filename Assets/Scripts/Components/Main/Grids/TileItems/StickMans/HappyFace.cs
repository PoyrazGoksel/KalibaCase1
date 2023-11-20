using DG.Tweening;

namespace Components.Main.Grids.TileItems.StickMans
{
    public class HappyFace : Emoji
    {
        public override void DoAnim()
        {
            TweenContainer.Clear();
            _spriteRenderer.enabled = true;

            TweenContainer.AddTween = _transform.DOShakeRotation(1f, 15f);
            
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