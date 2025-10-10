namespace AllBeginningsMod.Content.Bosses;

internal partial class NightgauntNPC {
    public void CrawlToPlayer() {
        var targetDelta = Target.Center - NPC.Center;
        _distanceToTarget = targetDelta.Length();
        _directionToTarget = targetDelta / _distanceToTarget;

        NPC.rotation = Utils.AngleLerp(NPC.rotation, _directionToTarget.ToRotation(), 0.05f);
        NPC.velocity += _directionToTarget * 0.05f;
        NPC.velocity *= 0.95f;

        _up = NPC.rotation.ToRotationVector2();
        _right = _up.RotatedBy(MathHelper.PiOver2);

        Vector2 rightShoulderPosition = RightShoulderPosition;
        Vector2 leftShoulderPosition = LeftShoulderPosition;

        if(_handSwapTimer == 0) {
            _handSwapTimer = Main.rand.Next(14, 22);

            var hSpeed = 2f;

            var grabOffset = 160f;
            if(_rightHandSwap) {
                _rightArmTargetPosition = rightShoulderPosition + _directionToTarget * grabOffset;
                NPC.velocity += _right * hSpeed;
            }
            else {
                _leftArmTargetPosition = leftShoulderPosition + _directionToTarget * grabOffset;
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
    }
}