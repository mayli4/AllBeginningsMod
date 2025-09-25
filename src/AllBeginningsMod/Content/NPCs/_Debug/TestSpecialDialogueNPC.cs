using AllBeginningsMod.Common.Dialogue;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.Localization;

namespace AllBeginningsMod.Content.NPCs;

public class TestSpecialDialogueNPC : ModNPC {
    public override string Texture => Textures.UI.KEY_AbandonedShackIcon;
    public override LocalizedText DisplayName => Keys.NPCs.TestSpecialDialogueNPC.DisplayName.GetText();

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 1;
    }

    public override void SetDefaults() {
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.width = 18;
        NPC.height = 40;
        NPC.aiStyle = -1;
        NPC.damage = 10;
        NPC.defense = 15;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0.5f;
        NPC.rarity = 1;
    }

    public override string GetChat() {
        var initialDialogue = new DialogueData() {
            Text = $"sup",
            SpeakerName = "oldbot",
            Options = new List<DialogueOption> {
                new DialogueOption {
                    Text = "hi",
                    OnClick = () => {
                        CustomDialogueSystem.ShowDialogue(this.NPC, new DialogueData {
                            Text = "asdasdasdasdasdasdasd, asdasdasdasdasdasd, [shake:asd], \n [wavy:asd], [evilng:asdasd]",
                            Options = new List<DialogueOption>
                            {
                                new DialogueOption { Text = "bye", OnClick = () => CustomDialogueSystem.HideDialogue(this.NPC) }
                            }
                        });
                    }
                },
                new DialogueOption {
                    Text = "talk 2 guide..",
                    OnClick = () => {
                        NPC? guideNpc = Main.npc[NPC.FindFirstNPC(NPCID.Guide)];
                        if (guideNpc.active) {
                            CustomDialogueSystem.ShowDialogue(guideNpc, new DialogueData {
                                Text = "im the guide whats up [shake:askdfhkjsdhfkjahs]",
                                Options = new List<DialogueOption> {
                                    new DialogueOption { Text = "bye", OnClick = () => CustomDialogueSystem.HideDialogue(guideNpc) }
                                }
                            });
                        }
                    }
                },
                new DialogueOption() {
                    Text = "bye",
                    OnClick = () => CustomDialogueSystem.HideDialogue(this.NPC)
                }
            }
        };
        
        CustomDialogueSystem.ShowDialogue(this.NPC, initialDialogue);
        
        return "";
    }
}