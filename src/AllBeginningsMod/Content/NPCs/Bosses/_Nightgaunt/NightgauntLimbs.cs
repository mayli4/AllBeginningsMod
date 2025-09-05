using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;

namespace AllBeginningsMod.Content.NPCs.Bosses;

//this sucks

public interface IConstraint {
    float Apply(float relativeAngle);
}

public enum BoneOrientation {
    Horizontal,
    Vertical
}

//todo: should be mutable, or have functions to change the source rect and offset

public readonly struct BoneTexture {
    public readonly Texture2D Texture;
    public readonly Rectangle? SourceRectangle;
    public readonly Vector2 DrawingOffset;
    public readonly BoneOrientation Orientation;

    public BoneTexture(Texture2D texture, Rectangle? sourceRectangle, BoneOrientation orientation, Vector2 drawingOffset = default) {
        Texture = texture;
        SourceRectangle = sourceRectangle;
        Orientation = orientation;
        DrawingOffset = drawingOffset;
    }
}

public class NightgauntBone {
    public float RelativeAngle;
    public float WorldAngle;
    public Vector2 StartPosition;
    public Vector2 EndPosition;

    public float Length { get; }
    public NightgauntBone Parent { get; }
    public IConstraint Constraint { get; }
    
    public BoneTexture BoneTexture { get; set; }
    
    public NightgauntBone(
        float length,
        NightgauntBone parent = null,
        BoneTexture texture = default,
        IConstraint constraint = null
    ) {
        Length = length;
        Parent = parent;
        Constraint = constraint;
        BoneTexture = texture;
    }
}

public class NightgauntLimb {
    private const float tolerance_amount = 0.5f;
    private const int max_iterations = 20;

    public Vector2 BasePosition;
    public readonly List<NightgauntBone> Bones = new();
    
    public float TotalLength { get; private set; }

    public Vector2 EndEffector => Bones.Count > 0 ? Bones.Last().EndPosition : BasePosition;

    public NightgauntLimb(Vector2 basePosition) {
        BasePosition = basePosition;
    }

    public void AddBone(float length, BoneTexture texture, IConstraint constraint = null) {
        if (length <= 0) {
            return;
        }

        NightgauntBone parent = Bones.Count > 0 ? Bones.Last() : null;
        var newBone = new NightgauntBone(
            length,
            parent,
            texture,
            constraint
        );
        
        Bones.Add(newBone);
        TotalLength += length;

        UpdateTransforms();
    }

    public void UpdateTransforms() {
        float parentWorldAngle;
        Vector2 currentStartPoint;

        foreach (var bone in Bones) {
            if (bone.Parent != null) {
                //bone is attached to parent
                currentStartPoint = bone.Parent.EndPosition;
                parentWorldAngle = bone.Parent.WorldAngle;
            }
            else {
                // first bone in da chain
                currentStartPoint = BasePosition;
                parentWorldAngle = 0f;
            }

            bone.StartPosition = currentStartPoint;
            //parents absolute angle + current bone relative angle
            bone.WorldAngle = Helper.NormalizeAngle(parentWorldAngle + bone.RelativeAngle);
            //start position + lemgth + worldangle dir
            bone.EndPosition = bone.StartPosition + new Vector2(
                    bone.Length * MathF.Cos(bone.WorldAngle),
                    bone.Length * MathF.Sin(bone.WorldAngle)
                );
        }
    }

    public void Solve(Vector2 target) {
        if (Bones.Count == 0) {
            return;
        }

        float distToBase = Vector2.Distance(BasePosition, target);
        if (distToBase > TotalLength) {
            //point the limb at the target
            Vector2 direction = Vector2.Normalize(target - BasePosition);
            float targetAngle = MathF.Atan2(direction.Y, direction.X);
            float parentAngle = 0f;

            foreach (var bone in Bones) {
                //angle for this bone is the diff between the absolute target angle, and the accumulated angle of ALL its parents
                float targetRelative = Helper.NormalizeAngle(targetAngle - parentAngle);
                bone.RelativeAngle = ApplyConstraint(bone, targetRelative);
                
                //accumulate angle for the next bone on the limb
                parentAngle = Helper.NormalizeAngle(parentAngle + bone.RelativeAngle);
            }
        }
        else {
            int iterations = 0;
            float distToTarget = Vector2.Distance(EndEffector, target);

            while (distToTarget > tolerance_amount && iterations < max_iterations) {
                for (int i = Bones.Count - 1; i >= 0; i--) {
                    var currentNightgauntBone = Bones[i];
                    Vector2 endEffectorPos = EndEffector;

                    Vector2 vecJointToEnd = endEffectorPos - currentNightgauntBone.StartPosition;
                    Vector2 vecJointToTarget = target - currentNightgauntBone.StartPosition;

                    //angle between the two vectors above, this is how much the current joint needs to rotate to point the end to the target
                    float angleCurrent = MathF.Atan2(vecJointToEnd.Y, vecJointToEnd.X);
                    float angleTarget = MathF.Atan2(vecJointToTarget.Y, vecJointToTarget.X);
                    float angleDelta = Helper.NormalizeAngle(angleTarget - angleCurrent);
                    
                    //apply transforms and constraints yo!
                    float targetRelativeAngle = Helper.NormalizeAngle(currentNightgauntBone.RelativeAngle + angleDelta);
                    currentNightgauntBone.RelativeAngle = ApplyConstraint(currentNightgauntBone, targetRelativeAngle);
                    
                    UpdateTransforms();
                }
                iterations++;
                distToTarget = Vector2.Distance(EndEffector, target);
            }
        }
        UpdateTransforms();
    }

    private float ApplyConstraint(NightgauntBone nightgauntBone, float targetRelativeAngle) {
        if (nightgauntBone.Constraint != null) {
            return nightgauntBone.Constraint.Apply(targetRelativeAngle);
        }
        return Helper.NormalizeAngle(targetRelativeAngle);
    }
    
    public void Draw(SpriteBatch spriteBatch) {
        foreach (var bone in Bones) {
            if (bone.BoneTexture.Texture == null) {
                continue;
            }
            
            var visuals = bone.BoneTexture;

            Vector2 baseOrigin;
            Rectangle? sourceRect = bone.BoneTexture.SourceRectangle;
            float baseRotation;
            
            float spriteWidth = sourceRect?.Width ?? visuals.Texture.Width;
            float spriteHeight = sourceRect?.Height ?? visuals.Texture.Height;

            if (sourceRect.HasValue) {
                baseOrigin = new Vector2(0, sourceRect.Value.Height / 2f);
            }
            else {
                baseOrigin = new Vector2(0, bone.BoneTexture.Texture.Height / 2f);
            }
            
            switch (visuals.Orientation) {
                case BoneOrientation.Vertical:
                    baseOrigin = new Vector2(spriteWidth / 2f, 0);
                    baseRotation = -MathHelper.PiOver2;
                    break;

                case BoneOrientation.Horizontal:
                default:
                    baseOrigin = new Vector2(0, spriteHeight / 2f);
                    baseRotation = 0f;
                    break;
            }
            
            Vector2 finalOrigin = baseOrigin - bone.BoneTexture.DrawingOffset;
            float finalRotation = bone.WorldAngle + baseRotation;

            Color lightColor = Lighting.GetColor(
                (int)(bone.StartPosition.X / 16f),
                (int)(bone.StartPosition.Y / 16f)
            );

            spriteBatch.Draw(
                bone.BoneTexture.Texture,
                bone.StartPosition - Main.screenPosition,
                sourceRect,
                lightColor,
                finalRotation,
                finalOrigin,
                1.0f,
                SpriteEffects.None,
                0f
            );
        }
        
        #if DEBUG
        foreach (var bone in Bones) {
            Vector2 screenStart = bone.StartPosition - Main.screenPosition;
            Vector2 screenEnd = bone.EndPosition - Main.screenPosition;
        
            Vector2 midPoint = (bone.StartPosition + bone.EndPosition) / 2f;
            Color lightColor = Lighting.GetColor( midPoint.ToTileCoordinates() );
        
            spriteBatch.DrawLine(
                screenStart,
                screenEnd,
                lightColor,
                1,
                TextureAssets.BlackTile.Value
            );
        }
        #endif
    }
}