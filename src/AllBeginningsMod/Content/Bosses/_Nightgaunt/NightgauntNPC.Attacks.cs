using AllBeginningsMod.Utilities;
using System;

namespace AllBeginningsMod.Content.Bosses;

internal partial class NightgauntNPC {
    public void CrawlToPlayer() {
        var targetDelta = Target.Center - NPC.Center;
        _distanceToTarget = targetDelta.Length();
        _directionToTarget = targetDelta / _distanceToTarget;

        float targetRotation = _directionToTarget.ToRotation();
        float turnAmount = MathHelper.WrapAngle(targetRotation - NPC.rotation);

        float legTimerDecrement = 1f + Math.Abs(turnAmount) * 2;

        NPC.rotation = Utils.AngleLerp(NPC.rotation, targetRotation, 0.05f);
        NPC.velocity += _directionToTarget * 0.05f;
        NPC.velocity *= 0.95f;

        _up = NPC.rotation.ToRotationVector2();
        _right = _up.RotatedBy(MathHelper.PiOver2);

        Vector2 rightShoulderPosition = RightShoulderPosition;
        Vector2 leftShoulderPosition = LeftShoulderPosition;
        Vector2 rightHipPosition = RightLegBasePosition;
        Vector2 leftHipPosition = LeftLegBasePosition;

        if(_handSwapTimer <= 0) {
            _handSwapTimer = Main.rand.Next(34, 42);
            var hSpeed = 2f;
            var armGrabOffset = 160f;
            float armVerticalSpread = 30f;

            if(_rightHandSwap) {
                _rightArmTargetPosition = rightShoulderPosition + _directionToTarget * armGrabOffset + _right * armVerticalSpread;
                NPC.velocity += _right * hSpeed;
            }
            else {
                _leftArmTargetPosition = leftShoulderPosition + _directionToTarget * armGrabOffset - _right * armVerticalSpread;
                NPC.velocity -= _right * hSpeed;
            }

            _rightHandSwap = !_rightHandSwap;
            NPC.velocity += _directionToTarget * 2.5f;
        }
        else _handSwapTimer -= 1;

        var grabLerpSpeed = .15f;
        _rightArmEndPosition = Vector2.Lerp(_rightArmEndPosition, _rightArmTargetPosition, grabLerpSpeed);
        _leftArmEndPosition = Vector2.Lerp(_leftArmEndPosition, _leftArmTargetPosition, grabLerpSpeed);

        _rightArm.Update(rightShoulderPosition, _rightArmEndPosition);
        _leftArm.Update(leftShoulderPosition, _leftArmEndPosition);
    
        float anchorThreshold = 5f;
        _rightArmAnchored = Vector2.Distance(_rightArmEndPosition, _rightArmTargetPosition) < anchorThreshold;
        _leftArmAnchored = Vector2.Distance(_leftArmEndPosition, _leftArmTargetPosition) < anchorThreshold;
    
        if (_legSwapTimer <= 0) {
            _legSwapTimer = Main.rand.Next(30, 40);

            var legGrabBackward = 40f; 
            var legHorizontalSpread = 80f;
            var legVerticalSpread = 20f;

            if(_rightLegSwap) {
                _rightLegTargetPosition = rightHipPosition
                                          - _directionToTarget * legGrabBackward
                                          + _right * (legHorizontalSpread - legVerticalSpread);
            }
            else {
                _leftLegTargetPosition = leftHipPosition
                                         - _directionToTarget * legGrabBackward
                                         - _right * (legHorizontalSpread - legVerticalSpread);
            }
            _rightLegSwap = !_rightLegSwap;
        }
        else _legSwapTimer -= (int)legTimerDecrement;
    
        var legGrabLerpSpeed = .15f;
        _rightLegEndPosition = Vector2.Lerp(_rightLegEndPosition, _rightLegTargetPosition, legGrabLerpSpeed);
        _leftLegEndPosition = Vector2.Lerp(_leftLegEndPosition, _leftLegTargetPosition, legGrabLerpSpeed);

        _rightLeg.Update(rightHipPosition, _rightLegEndPosition);
        _leftLeg.Update(leftHipPosition, _leftLegEndPosition);
    
        _rightLegAnchored = Vector2.Distance(_rightLegEndPosition, _rightLegTargetPosition) < anchorThreshold;
        _leftLegAnchored = Vector2.Distance(_leftLegEndPosition, _leftLegTargetPosition) < anchorThreshold;
    }
}