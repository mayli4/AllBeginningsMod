using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.UI.Chat;
using AllBeginningsMod.Utilities;

namespace AllBeginningsMod.Common.Dialogue;
//massive imrpovements needed, still heavy wip

public enum HorizontalAlignment {
    Left,
    Right
}

public class DialogueOption {
    public string Text { get; set; }
    public Action OnClick { get; set; }
}

public class DialogueData {
    public string Text { get; set; }
    public List<DialogueOption> Options { get; set; }
    public string SpeakerName { get; set; }

    public DialogueData(string text = "", List<DialogueOption> options = null, string speakerName = null) {
        Text = text;
        Options = options ?? new List<DialogueOption>();
        SpeakerName = speakerName;
    }
}

public class CustomDialogueSystem : ModSystem {
    internal CustomDialogueUIState CustomDialogueUI;
    private UserInterface _customDialogueUserInterface;

    public override void Load() {
        if (!Main.dedServ) {
            CustomDialogueUI = new CustomDialogueUIState();
            _customDialogueUserInterface = new UserInterface();
            _customDialogueUserInterface.SetState(CustomDialogueUI);
        }
    }

    public override void Unload() {
        if (!Main.dedServ) {
            CustomDialogueUI = null;
            _customDialogueUserInterface = null;
        }
    }

    public override void UpdateUI(GameTime gameTime) {
        if (!Main.dedServ && CustomDialogueUI != null && CustomDialogueUI.Visible) {
            _customDialogueUserInterface?.Update(gameTime);
        }
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
        if (Main.dedServ || CustomDialogueUI == null) {
            return;
        }

        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if(mouseTextIndex == -1)
            return;
        
        layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
            "AllBeginningsMod: Custom Dialogue",
            delegate {
                if (CustomDialogueUI.Visible) {
                    _customDialogueUserInterface.Draw(Main.spriteBatch, new GameTime());
                }
                return true;
            },
            InterfaceScaleType.UI)
        );
    }

    public static void ShowDialogue(NPC npc, DialogueData dialogue) {
        if (Main.dedServ) return;

        CustomDialogueSystem instance = ModContent.GetInstance<CustomDialogueSystem>();
        if (instance == null || instance.CustomDialogueUI == null) {
            return;
        }
        
        instance.CustomDialogueUI.OpenDialogue(npc, dialogue);
        
        Main.player[Main.myPlayer].talkNPC = npc.whoAmI;
        Main.npcChatCornerItem = 0;
        Main.npcChatText = "";
        Main.playerInventory = false;
    }

    public static void HideDialogue(NPC npc = null) {
        if (Main.dedServ) return;

        CustomDialogueSystem instance = ModContent.GetInstance<CustomDialogueSystem>();
        if (instance == null || instance.CustomDialogueUI == null) {
            return;
        }

        if (npc != null) {
            instance.CustomDialogueUI.HideDialogue(npc);
        } else {
            instance.CustomDialogueUI.HideAllDialogues();
        }
    }

    internal static void FinalHideCleanup() {
        if (Main.dedServ) return;

        Main.player[Main.myPlayer].talkNPC = -1;
        Main.playerInventory = false;
        Main.npcChatText = "";
        Main.blockInput = false;
        Main.player[Main.myPlayer].mouseInterface = false;
    }
}

public class CustomDialogueUIState : UIState {
    internal List<DialogueBox> _activeDialogueBoxes = new List<DialogueBox>();
    public bool Visible { get; private set; } = false;

    private const int DIALOGUE_PANEL_WIDTH = 700;
    private const int DIALOGUE_BOTTOM_OFFSET = 100;
    private const int DIALOGUE_SIDE_PADDING = 50;
    private const float DIALOGUE_BOX_OPACITY_FULL = 1.0f;

    public override void OnInitialize() { }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);

        if (Visible) {
            Main.player[Main.myPlayer].mouseInterface = true;
            Main.blockInput = true;
        }
        
        for (int i = _activeDialogueBoxes.Count - 1; i >= 0; i--) {
            DialogueBox box = _activeDialogueBoxes[i];
            box.Update(gameTime);

            if (_activeDialogueBoxes.Count == 1) {
                box.TargetScreenPosition = new Vector2(
                    DIALOGUE_SIDE_PADDING + (DIALOGUE_PANEL_WIDTH / 2f),
                    Main.screenHeight - box.CalculatedHeight - DIALOGUE_BOTTOM_OFFSET
                );
                box.CurrentAlignment = HorizontalAlignment.Left;
            }
            else if (_activeDialogueBoxes.Count >= 2) {
                if (i == _activeDialogueBoxes.Count - 2) {
                    box.TargetScreenPosition = new Vector2(
                        DIALOGUE_SIDE_PADDING + (DIALOGUE_PANEL_WIDTH / 2f),
                        Main.screenHeight - box.CalculatedHeight - DIALOGUE_BOTTOM_OFFSET
                    );
                    box.CurrentAlignment = HorizontalAlignment.Left;
                }
                else if (i == _activeDialogueBoxes.Count - 1) {
                    box.TargetScreenPosition = new Vector2(
                        Main.screenWidth - DIALOGUE_SIDE_PADDING - (DIALOGUE_PANEL_WIDTH / 2f),
                        Main.screenHeight - box.CalculatedHeight - DIALOGUE_BOTTOM_OFFSET
                    );
                    box.CurrentAlignment = HorizontalAlignment.Right;
                }
                else {
                    box.StartClosing();
                    box.Opacity = 0f;
                }
            }
        }
        
        _activeDialogueBoxes.RemoveAll(box => !box.IsActive);

        Visible = _activeDialogueBoxes.Count > 0;

        if (!Visible) {
            CustomDialogueSystem.FinalHideCleanup();
        }
    }

    public override void Draw(SpriteBatch spriteBatch) {
        if (!Visible) return;

        foreach (var box in _activeDialogueBoxes) {
            box.Draw(spriteBatch);
        }

        base.Draw(spriteBatch);
    }

    public void OpenDialogue(NPC npc, DialogueData dialogue) {
        DialogueBox existingBox = _activeDialogueBoxes.Find(b => b.NpcSpeaking == npc);

        if (existingBox != null) {
            existingBox.SetDialogue(dialogue);
            _activeDialogueBoxes.Remove(existingBox);
            _activeDialogueBoxes.Add(existingBox);
        }
        else {
            DialogueBox newBox = new DialogueBox(npc, dialogue);
            _activeDialogueBoxes.Add(newBox);

            Vector2 initialTargetPosition;
            if (_activeDialogueBoxes.Count == 1) {
                initialTargetPosition = new Vector2(
                    DIALOGUE_SIDE_PADDING + (DIALOGUE_PANEL_WIDTH / 2f),
                    Main.screenHeight - newBox.CalculatedHeight - DIALOGUE_BOTTOM_OFFSET
                );
                newBox.CurrentAlignment = HorizontalAlignment.Left;
            } else {
                initialTargetPosition = new Vector2(
                    Main.screenWidth - DIALOGUE_SIDE_PADDING - (DIALOGUE_PANEL_WIDTH / 2f),
                    Main.screenHeight - newBox.CalculatedHeight - DIALOGUE_BOTTOM_OFFSET
                );
                newBox.CurrentAlignment = HorizontalAlignment.Right;
            }
            newBox.TargetScreenPosition = initialTargetPosition;
            newBox.CurrentDrawPosition = initialTargetPosition;
            newBox.Opacity = DIALOGUE_BOX_OPACITY_FULL;
        }

        UpdateButtonsForSpecificBox(GetLatestDialogueBox());
        
        Visible = true;
    }

    public void HideDialogue(NPC npc) {
        DialogueBox boxToClose = _activeDialogueBoxes.Find(b => b.NpcSpeaking == npc);
        if (boxToClose != null) {
            boxToClose.StartClosing();
        }
        UpdateButtonsForSpecificBox(GetLatestDialogueBox());
    }

    public void HideAllDialogues() {
        foreach (var box in _activeDialogueBoxes) {
            box.StartClosing();
        }
        UpdateButtonsForSpecificBox(null);
    }

    private void UpdateButtonsForSpecificBox(DialogueBox targetBox) {
        Elements.Clear();
        if (targetBox != null) {
            AddDialogueOptions(targetBox.CurrentDialogueData.Options, targetBox);
        }
    }
    
    private DialogueBox GetLatestDialogueBox() {
        if (_activeDialogueBoxes.Count > 0) {
            for (int i = _activeDialogueBoxes.Count - 1; i >= 0; i--) {
                if (!_activeDialogueBoxes[i].Closing) {
                    return _activeDialogueBoxes[i];
                }
            }
        }
        return null;
    }

    private void AddDialogueOptions(List<DialogueOption> options, DialogueBox associatedBox) {
        float totalButtonsWidth = 0;
        foreach (var option in options) {
            Vector2 stringSize = FontAssets.MouseText.Value.MeasureString(option.Text);
            totalButtonsWidth += stringSize.X + 20; // button width + padding
        }
        if (options.Count > 1) {
            totalButtonsWidth += (options.Count - 1) * 10;
        }

        float currentXOffset = 0;
        if (associatedBox.CurrentAlignment == HorizontalAlignment.Right) {
            
            //if the box is on the right, certain elements should also appear more towards the right
            currentXOffset = associatedBox.MainDialogueBoxRect.Width - totalButtonsWidth;
            if (currentXOffset < 0) currentXOffset = 0;
        } else {
            currentXOffset = 0;
        }


        foreach (var option in options) {
            Vector2 stringSize = FontAssets.MouseText.Value.MeasureString(option.Text);
            float buttonWidth = stringSize.X + 20;

            DialogueButton newButton = new DialogueButton(
                option.Text, 
                () => {
                option.OnClick?.Invoke();
                UpdateButtonsForSpecificBox(GetLatestDialogueBox());
                }, 
                new Vector2(currentXOffset, 0), 
                associatedBox
                );

            Append(newButton);
            currentXOffset += buttonWidth + 10;
        }
    }
}

public class DialogueBox {
    public NPC NpcSpeaking;
    public DialogueData CurrentDialogueData;

    private TextSnippet[] _parsedMessage;
    private string _title = "";
    private int _messageLen; 

    public bool IsActive = true;
    public bool Closing = false;

    public int BoxTimer = 0;
    public int TextTimer = 0;
    public int TitleTimer = 0;

    public float Opacity = 1f;
    public float FinalOpacity;

    public Vector2 CurrentDrawPosition;
    public Vector2 TargetScreenPosition;
    public HorizontalAlignment CurrentAlignment { get; set; } = HorizontalAlignment.Left;

    public const int DIALOGUE_PANEL_WIDTH = 700;
    public const int TITLE_BAR_HEIGHT = 36;
    public const int TITLE_BAR_OFFSET_FROM_MAIN_PANEL = 40;
    public const int TITLE_HORIZONTAL_OFFSET_FROM_EDGE = 20;
    public const int ANIMATION_DURATION = 30;

    public int CalculatedHeight { get; private set; }

    public Rectangle MainDialogueBoxRect;
    public Rectangle TitleBoxRect;
    public Vector2 ButtonRowBasePosition;

    public DialogueBox(NPC npc, DialogueData dialogueData) {
        NpcSpeaking = npc;
        BoxTimer = 0;
        SetDialogue(dialogueData);
        CurrentDrawPosition = TargetScreenPosition;
    }

    public void SetDialogue(DialogueData newDialogue) {
        CurrentDialogueData = newDialogue;
        _title = string.IsNullOrEmpty(newDialogue.SpeakerName) ? NpcSpeaking.GivenName : newDialogue.SpeakerName;
        
        SetMessage(newDialogue.Text); 
        
        if (!Closing && BoxTimer < ANIMATION_DURATION) {
            BoxTimer = 0;
        } else if (Closing) {
            Closing = false;
            BoxTimer = ANIMATION_DURATION;
        } else {
            BoxTimer = ANIMATION_DURATION;
        }
        IsActive = true;
    }

    private void SetMessage(string newMessage) {
        _parsedMessage = ChatManager.ParseMessage(newMessage, Color.White).ToArray();
        _messageLen = Helper.GetCharCount(_parsedMessage);
        UpdateCalculatedHeight();
        TextTimer = 0;
        TitleTimer = 0;
    }

    private void UpdateCalculatedHeight() {
        if (_parsedMessage == null || _parsedMessage.Length == 0) {
            CalculatedHeight = 60;
            return;
        }
        Vector2 messageDims = ChatManager.GetStringSize(FontAssets.MouseText.Value, _parsedMessage, Vector2.One, DIALOGUE_PANEL_WIDTH - 40);
        CalculatedHeight = (int)messageDims.Y + 60;
    }

    public void Update(GameTime gameTime) {
        CurrentDrawPosition = TargetScreenPosition;

        if (!Closing && BoxTimer < ANIMATION_DURATION) {
            BoxTimer++;
        } else if (Closing && BoxTimer > 0) {
            BoxTimer--;
        } else if (Closing && BoxTimer <= 0) {
            IsActive = false;
        }

        float animationProgress = Easings.BezierEase(Math.Clamp((float)BoxTimer / ANIMATION_DURATION, 0, 1));
        
        FinalOpacity = Opacity * animationProgress;

        if (BoxTimer >= ANIMATION_DURATION / 2) {
            if (TextTimer < _messageLen) {
                TextTimer++;
            }
            if (TitleTimer < _title.Length) {
                TitleTimer++;
            }
        } else {
            TextTimer = 0;
            TitleTimer = 0;
        }
    }

    public void Draw(SpriteBatch spriteBatch) {
        if (!IsActive || _parsedMessage == null || _parsedMessage.Length == 0 || FinalOpacity <= 0) return;

        int mainWidth = DIALOGUE_PANEL_WIDTH;
        
        MainDialogueBoxRect = new Rectangle(
            (int)(CurrentDrawPosition.X - (mainWidth / 2)),
            (int)CurrentDrawPosition.Y,
            mainWidth,
            CalculatedHeight
        );

        float titleTextWidth = FontAssets.MouseText.Value.MeasureString(_title).X;
        int titleBoxWidth = (int)titleTextWidth + 40;
        
        float titleDrawX;
        float titleOriginX;
        
        if (CurrentAlignment == HorizontalAlignment.Left) {
            titleDrawX = MainDialogueBoxRect.Left + TITLE_HORIZONTAL_OFFSET_FROM_EDGE;
            titleOriginX = 0f;
        } else {
            titleDrawX = MainDialogueBoxRect.Right - TITLE_HORIZONTAL_OFFSET_FROM_EDGE;
            titleOriginX = 1f;
        }

        TitleBoxRect = new Rectangle(
            (int)(titleDrawX - (titleBoxWidth * titleOriginX)), 
            MainDialogueBoxRect.Top - TITLE_BAR_OFFSET_FROM_MAIN_PANEL,
            titleBoxWidth,
            TITLE_BAR_HEIGHT
        );
        
        if (TitleBoxRect.Left < 0) TitleBoxRect.X = 0;
        if (TitleBoxRect.Right > Main.screenWidth) TitleBoxRect.X = Main.screenWidth - TitleBoxRect.Width;


        ButtonRowBasePosition = new Vector2(
            MainDialogueBoxRect.Left,
            MainDialogueBoxRect.Bottom + 30
        );

        DrawBox(spriteBatch, MainDialogueBoxRect, new Color(30, 40, 80, 200) * FinalOpacity);
        DrawBox(spriteBatch, TitleBoxRect, new Color(50, 80, 155, 200) * FinalOpacity);

        if (_title.Length > 0 && TextTimer > 0) {
            Utils.DrawBorderString(spriteBatch, _title[..Math.Min(_title.Length, TitleTimer)], 
                new Vector2(titleDrawX, TitleBoxRect.Center.Y), Color.White * FinalOpacity, 1, titleOriginX, 0.5f);
        }

        if (TextTimer > 0) {
            Helper.DrawColorCodedStringShadow(
                spriteBatch, FontAssets.MouseText.Value, _parsedMessage,
                MainDialogueBoxRect.Location.ToVector2() + new Vector2(20, 20),
                Color.White * FinalOpacity, 0, Vector2.Zero, Vector2.One, out _, 
                MainDialogueBoxRect.Width - 40,
                false, Math.Min(_messageLen, TextTimer)
            );
        }
    }

    internal void DrawBox(SpriteBatch spriteBatch, Rectangle rect, Color color) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        
        spriteBatch.Draw(pixel, rect, color);

        Color borderColor = Color.Lerp(color, Color.Black, 0.5f) * (color.A / 255f);
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height - 2, rect.Width, 2), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - 2, rect.Y, 2, rect.Height), borderColor);
    }

    public void StartClosing() {
        Closing = true;
    }
}

public class DialogueButton : UIElement
{
    private readonly string _message;
    private readonly Action _onClick;
    private readonly Vector2 _offset;

    private int _boxTimer;
    private float _hoverTime;

    private DialogueBox _associatedDialogueBox;

    public DialogueButton(string message, Action onClick, Vector2 offset, DialogueBox associatedDialogueBox) {
        _message = message;
        _onClick = onClick;
        _offset = offset;
        _associatedDialogueBox = associatedDialogueBox;
        
        Vector2 stringSize = FontAssets.MouseText.Value.MeasureString(_message);
        Width.Set(stringSize.X + 20, 0);
        Height.Set(32, 0);

        if (_associatedDialogueBox.BoxTimer >= DialogueBox.ANIMATION_DURATION) {
            _boxTimer = 30;
        } else {
            _boxTimer = -20;
        }
    }

    public override void Draw(SpriteBatch spriteBatch) {
        if (_associatedDialogueBox.IsActive && !_associatedDialogueBox.Closing && 
            _associatedDialogueBox.FinalOpacity >= 0.99f && _boxTimer < 30) {
            _boxTimer++;
        }
        
        if (_associatedDialogueBox.Closing && _associatedDialogueBox.FinalOpacity < 0.1f) return;

        Left.Set(_associatedDialogueBox.ButtonRowBasePosition.X + _offset.X, 0);
        Top.Set(_associatedDialogueBox.ButtonRowBasePosition.Y + _offset.Y, 0);

        Recalculate();

        CalculatedStyle dims = GetDimensions();

        float easedAmount = Easings.BezierEase(Math.Clamp(_boxTimer / 30f, 0, 1));
        float effectiveOpacity = easedAmount * _associatedDialogueBox.FinalOpacity;
        
        int mainBoxWidth = (int)dims.Width;
        
        Rectangle box = new Rectangle((int)dims.X, (int)dims.Y, mainBoxWidth, (int)dims.Height);
        box.Inflate((int)(_hoverTime * 4), (int)(_hoverTime * 4));

        _associatedDialogueBox.DrawBox(spriteBatch, box, new Color(50, 80, 155, 200) * effectiveOpacity);

        if (IsMouseHovering && _hoverTime < 1) {
            _hoverTime += 0.1f;
        } else if (!IsMouseHovering && _hoverTime > 0) {
            _hoverTime -= 0.1f;
        }

        Color textColor = IsMouseHovering ? Color.Yellow : Color.White;

        if (_boxTimer >= 0) {
            Utils.DrawBorderString(spriteBatch, _message, dims.ToRectangle().Center(), textColor * effectiveOpacity, 1f + _hoverTime * 0.05f, 0.5f, 0.4f);
        }

        base.Draw(spriteBatch);
    }

    public override void MouseOver(UIMouseEvent evt) {
        if (!_associatedDialogueBox.Closing && _associatedDialogueBox.FinalOpacity >= 0.99f) {
            base.MouseOver(evt);
        }
    }

    public override void MouseOut(UIMouseEvent evt) {
        if (!_associatedDialogueBox.Closing && _associatedDialogueBox.FinalOpacity >= 0.99f) {
            base.MouseOut(evt);
        }
    }

    public override void LeftClick(UIMouseEvent evt) {
        if (!_associatedDialogueBox.Closing && _associatedDialogueBox.FinalOpacity >= 0.99f) {
            _onClick?.Invoke();
            base.LeftClick(evt);
        }
    }
}