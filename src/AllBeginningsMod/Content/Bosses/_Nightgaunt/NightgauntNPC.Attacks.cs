using AllBeginningsMod.Utilities;
using System;

namespace AllBeginningsMod.Content.Bosses;

internal partial class NightgauntNPC {
    public void CrawlToPlayer() {
        var targetDelta = Main.MouseWorld - NPC.Center;
        _distanceToTarget = targetDelta.Length();
        _directionToTarget = targetDelta / _distanceToTarget;

        float targetRotation = _directionToTarget.ToRotation();
        float turnAmount = MathHelper.WrapAngle(targetRotation - NPC.rotation);

        NPC.rotation = Utils.AngleLerp(NPC.rotation, targetRotation, 0.05f);
        NPC.velocity += _directionToTarget * 0.05f;
        NPC.velocity *= 0.95f;

        _up = NPC.rotation.ToRotationVector2();
        _right = _up.RotatedBy(MathHelper.PiOver2);

        Vector2 bodyBasePosition = NPC.Center + new Vector2(0, 0);

        float totalBodyLength = _body.Skeleton.GetTotalLength();
        float swayMultiplier = 40f;

        Vector2 neutralBodyTarget = bodyBasePosition - _up * totalBodyLength;
        Vector2 swayOffset = _right * turnAmount * swayMultiplier;

        _body.TargetPosition = neutralBodyTarget + swayOffset;
    
        UpdateLimbState(ref _body, bodyBasePosition, 0.1f, 5f);
    
        float legTimerDecrement = 1f + Math.Abs(turnAmount) * 5;

        Vector2 rightShoulderPosition = RightShoulderPosition;
        Vector2 leftShoulderPosition = LeftShoulderPosition;
        Vector2 rightHipPosition = RightLegBasePosition;
        Vector2 leftHipPosition = LeftLegBasePosition;

        if(_handSwapTimer <= 0) {
            _handSwapTimer = Main.rand.Next(34, 42);
            
            var hSpeed = 2f;
            var armGrabOffset = 190f;
            float armVerticalSpread = 60f;
            
            if(_rightHandSwap) {
                _rightArm.TargetPosition = rightShoulderPosition + _directionToTarget * armGrabOffset + _right * armVerticalSpread;
                NPC.velocity += _right * hSpeed;
            } else {
                _leftArm.TargetPosition = leftShoulderPosition + _directionToTarget * armGrabOffset - _right * armVerticalSpread;
                NPC.velocity -= _right * hSpeed;
            }
            
            _rightHandSwap = !_rightHandSwap;
            NPC.velocity += _directionToTarget * 2.5f;
        } 
        else _handSwapTimer -= 1;
    
        if (_legSwapTimer <= 0) {
            _legSwapTimer = Main.rand.Next(30, 40);
            
            var legGrabBackward = -10f; 
            var legHorizontalSpread = 80f;
            var legVerticalSpread = 20f;
            
            if(_rightLegSwap) {
                _rightLeg.TargetPosition = rightHipPosition - _directionToTarget * legGrabBackward + _right * (legHorizontalSpread - legVerticalSpread);
            } else {
                _leftLeg.TargetPosition = leftHipPosition - _directionToTarget * legGrabBackward - _right * (legHorizontalSpread - legVerticalSpread);
            }
            _rightLegSwap = !_rightLegSwap;
        } 
        else _legSwapTimer -= (int)legTimerDecrement;
    
        UpdateLimbState(ref _rightArm, rightShoulderPosition, 0.15f, 5f);
        UpdateLimbState(ref _leftArm, leftShoulderPosition, 0.15f, 5f);
        UpdateLimbState(ref _rightLeg, rightHipPosition, 0.15f, 5f);
        UpdateLimbState(ref _leftLeg, leftHipPosition, 0.15f, 5f);
    }
}