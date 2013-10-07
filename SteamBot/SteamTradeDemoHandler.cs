using SteamKit2;
using System.Collections.Generic;
using SteamTrade;

namespace SteamBot
{
    public class SteamTradeDemoHandler : UserHandler
    {
        // NEW ------------------------------------------------------------------
        private GenericInventory mySteamInventory = new GenericInventory();
        private GenericInventory OtherSteamInventory = new GenericInventory();
        private bool tested;
        // ----------------------------------------------------------------------

        public SteamTradeDemoHandler(Bot bot, SteamID sid) : base(bot, sid) { }

        public override bool OnFriendAdd()
        {
            return true;
        }

        public override void OnLoginCompleted()
        {
            Bot.SteamFriends.SetPersonaState(EPersonaState.LookingToTrade);
        }

        public override void OnChatRoomMessage(SteamID chatID, SteamID sender, string message)
        {
            Log.Info(Bot.SteamFriends.GetFriendPersonaName(sender) + ": " + message);
            base.OnChatRoomMessage(chatID, sender, message);
        }

        public override void OnFriendRemove() { }

        public override void OnMessage(string message, EChatEntryType type)
        {
            Bot.SteamFriends.SendChatMessage(OtherSID, type, Bot.ChatResponse);
        }

        public override bool OnTradeRequest()
        {
            return true;
        }

        public override void OnTradeError(string error)
        {
            Bot.SteamFriends.SendChatMessage(OtherSID,
                                              EChatEntryType.ChatMsg,
                                              "Oh, there was an error: " + error + "."
                                              );
            Bot.log.Warn(error);
            Bot.SteamFriends.SetPersonaState(EPersonaState.LookingToTrade);
        }

        public override void OnTradeTimeout()
        {
            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg,
                                              "Sorry, but you were AFK and the trade was canceled.");
            Bot.log.Info("User was kicked because he was AFK.");
            Bot.SteamFriends.SetPersonaState(EPersonaState.LookingToTrade);
        }

        public override void OnTradeInit()
        {
            Bot.SteamFriends.SetPersonaState(EPersonaState.Busy);
            // NEW -------------------------------------------------------------------------------
            List<int> contextId = new List<int>();
            tested = false;

            /*************************************************************************************
             * 
             * SteamInventory AppId = 753 
             * 
             *  Context Id      Description
             *      1           Gifts (Games), must be public on steam profile in order to work.
             *      3           Coupons
             *      6           Trading Cards, Emoticons & Backgrounds. 
             *  
             ************************************************************************************/

            contextId.Add(1);
            contextId.Add(3);
            contextId.Add(6);

            mySteamInventory.load(753, contextId, Bot.SteamClient.SteamID);
            OtherSteamInventory.load(753, contextId, OtherSID);

            if (!mySteamInventory.isLoaded | !OtherSteamInventory.isLoaded)
            {
                Trade.SendMessage("Couldn't open an inventory, type 'errors' for more info.");
            }
            Trade.SendMessage("Type 'test' to start.");
            // -----------------------------------------------------------------------------------
        }

        public override void OnTradeAddItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
        {
            // USELESS DEBUG MESSAGES -------------------------------------------------------------------------------
            Trade.SendMessage("Object AppID: " + inventoryItem.AppId);
            Trade.SendMessage("Object ContextId: " + inventoryItem.ContextId);

            switch (inventoryItem.AppId)
            {
                case 440:
                    Trade.SendMessage("TF2 Item Added.");
                    Trade.SendMessage("Name: " + schemaItem.Name);
                    Trade.SendMessage("Quality: " + inventoryItem.Quality);
                    Trade.SendMessage("Level: " + inventoryItem.Level);
                    Trade.SendMessage("Craftable: " + (inventoryItem.IsNotCraftable ? "No" : "Yes"));

                    if (true)
                    {
                        Bot.log.Warn(OtherSID + " added: " + schemaItem.Name + "with quality: " + inventoryItem.Quality + " with level: " + inventoryItem.Level + ", type: TF2 Item, item craftable: " + (inventoryItem.IsNotCraftable ? "No" : "Yes"));
                    }

                    break;

                case 753:
                    GenericInventory.ItemDescription tmpDescription = OtherSteamInventory.getDescription(inventoryItem.Id);
                    Trade.SendMessage("Steam Inventory Item Added.");
                    Trade.SendMessage("Type: " + tmpDescription.type);
                    Trade.SendMessage("Marketable: " + (tmpDescription.marketable ? "Yes" : "No"));
                    Bot.log.Error(OtherSID + " added: " + tmpDescription.name + ", type: " + tmpDescription.type + ", item marketable: " + (tmpDescription.marketable ? "Yes" : "No"));
                    break;

                default:
                    Trade.SendMessage("Unknown item");
                    break;
            }
            // ------------------------------------------------------------------------------------------------------
        }

        public override void OnTradeRemoveItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
        {
            // USELESS DEBUG MESSAGES -------------------------------------------------------------------------------
            //Trade.SendMessage("Object AppID: " + inventoryItem.AppId);
            //Trade.SendMessage("Object ContextId: " + inventoryItem.ContextId);

            switch (inventoryItem.AppId)
            {
                case 440:
                    if (true)
                    {
                        Bot.log.Warn(OtherSID + " removed: " + schemaItem.Name + "with quality: " + inventoryItem.Quality + " with level: " + inventoryItem.Level + ", type: TF2 Item, item craftable: " + (inventoryItem.IsNotCraftable ? "No" : "Yes"));
                    }
                    break;

                case 753:
                    GenericInventory.ItemDescription tmpDescription = OtherSteamInventory.getDescription(inventoryItem.Id);
                    Bot.log.Error(OtherSID + " removed: " + tmpDescription.name + ", type: " + tmpDescription.type + ", item marketable: " + (tmpDescription.marketable ? "Yes" : "No"));
                    break;

                default:
                    Trade.SendMessage("Unknown item");
                    break;
            }
            // ------------------------------------------------------------------------------------------------------

        }

        public override void OnTradeMessage(string message)
        {

            switch (message.ToLower())
            {
                case "errors":
                    if (OtherSteamInventory.errors.Count > 0)
                    {
                        Trade.SendMessage("User Errors:");
                        foreach (string error in OtherSteamInventory.errors)
                        {
                            Trade.SendMessage(" * " + error);
                            Bot.log.Error(" * " + error);
                        }
                    }

                    if (mySteamInventory.errors.Count > 0)
                    {
                        Trade.SendMessage("Bot Errors:");
                        foreach (string error in mySteamInventory.errors)
                        {
                            Trade.SendMessage(" * " + error);
                            Bot.log.Error(" * " + error);
                        }
                    }
                    break;

                case "test":
                    if (tested)
                    {
                        foreach (GenericInventory.Item item in mySteamInventory.items.Values)
                        {
                            Trade.RemoveItem(item);
                        }
                    }
                    else
                    {
                        Trade.SendMessage("Items on my bp: " + mySteamInventory.items.Count);
                        foreach (GenericInventory.Item item in mySteamInventory.items.Values)
                        {
                            Trade.AddItem(item);
                        }
                    }

                    tested = !tested;
                    break;

                case "remove":
                    foreach (var item in mySteamInventory.items)
                    {
                        Trade.RemoveItem(item.Value.assetid, item.Value.appid, item.Value.contextid);
                    }
                    break;
            }
        }

        public override void OnTradeReady(bool ready)
        {
            //Because SetReady must use its own version, it's important
            //we poll the trade to make sure everything is up-to-date.
            Trade.Poll();
            if (!ready)
            {
                Trade.SetReady(false);
            }
            else
            {
                if (Validate() | IsAdmin)
                {
                    Trade.SetReady(true);
                }
            }
        }

        public override void OnTradeAccept()
        {
            if (Validate() || IsAdmin)
            {
                //Even if it is successful, AcceptTrade can fail on
                //trades with a lot of items so we use a try-catch
                try
                {
                    Trade.AcceptTrade();
                }
                catch
                {
                    Log.Warn("The trade might have failed, but we can't be sure.");
                }

                Log.Success("Trade Complete!");
                Bot.SteamFriends.SetPersonaState(EPersonaState.LookingToTrade);

                // Since people seems getting a error like : There is already added an item with the same key, i found a way to fix this just to clear the descriptions and items dictionary.
                OtherSteamInventory.descriptions.Clear();
                OtherSteamInventory.items.Clear();
                Bot.log.Success("description/items other cleared Complete!");
                mySteamInventory.descriptions.Clear();
                mySteamInventory.items.Clear();
                Bot.log.Success("description/items me cleared Complete!");
            }
            OnTradeClose();
        }

        public override void OnTradeClose()
        {
            Bot.log.Warn("[USERHANDLER] TRADE CLOSED");
            Bot.CloseTrade();
            Bot.SteamFriends.SetPersonaState(EPersonaState.LookingToTrade);

            // Since people seems getting a error like : There is already added an item with the same key, i found a way to fix this just to clear the descriptions and items dictionary.
            OtherSteamInventory.descriptions.Clear();
            OtherSteamInventory.items.Clear();
            Bot.log.Warn("description/items other cleared Complete!");
            mySteamInventory.descriptions.Clear();
            mySteamInventory.items.Clear();
            Bot.log.Warn("description/items me cleared Complete!");
        }

        public bool Validate()
        {
            List<string> errors = new List<string>();
            errors.Add("This demo is meant to show you how to handle SteamInventory Items. Trade cannot be completed, unless you're and Admin.");

            // send the errors
            if (errors.Count != 0)
                Trade.SendMessage("There were errors in your trade: ");

            foreach (string error in errors)
            {
                Trade.SendMessage(error);
            }
            return errors.Count == 0;
        }
    }
}

