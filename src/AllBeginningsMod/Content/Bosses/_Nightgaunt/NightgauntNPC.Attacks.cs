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

        if(_handSwapTimer <= 0) {
            _handSwapTimer = Main.rand.Next(34, 42);
            var hSpeed = 2f;
            var armGrabOffset = 160f;
            float armVerticalSpread = 30f;

            if(_rightHandSwap) {
                _rightArm.TargetPosition = RightShoulderPosition + _directionToTarget * armGrabOffset + _right * armVerticalSpread;
                NPC.velocity += _right * hSpeed;
            }
            else {
                _leftArm.TargetPosition = LeftShoulderPosition + _directionToTarget * armGrabOffset - _right * armVerticalSpread;
                NPC.velocity -= _right * hSpeed;
            }

            _rightHandSwap = !_rightHandSwap;
            NPC.velocity += _directionToTarget * 2.5f;
        }
        else {
            _handSwapTimer -= 1;
        }

        if (_legSwapTimer <= 0) {
            _legSwapTimer = Main.rand.Next(30, 40);
            var legGrabBackward = -10f; 
            var legHorizontalSpread = 80f;
            var legVerticalSpread = 20f;

            if(_rightLegSwap) {
                _rightLeg.TargetPosition = RightLegBasePosition - _directionToTarget * legGrabBackward + _right * (legHorizontalSpread - legVerticalSpread);
            }
            else {
                _leftLeg.TargetPosition = LeftLegBasePosition - _directionToTarget * legGrabBackward - _right * (legHorizontalSpread - legVerticalSpread);
            }
            _rightLegSwap = !_rightLegSwap;
        }
        else {
            _legSwapTimer -= (int)legTimerDecrement;
        }
    
        UpdateLimbState(ref _rightArm, RightShoulderPosition, 0.15f, 5f);
        UpdateLimbState(ref _leftArm, LeftShoulderPosition, 0.15f, 5f);
        UpdateLimbState(ref _rightLeg, RightLegBasePosition, 0.15f, 5f);
        UpdateLimbState(ref _leftLeg, LeftLegBasePosition, 0.15f, 5f);
    }
}