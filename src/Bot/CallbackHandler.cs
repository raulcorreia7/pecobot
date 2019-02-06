
using System;
using System.Collections.Generic;
using MightyPecoBot.Parsing;

namespace MightyPecoBot.Callbacks
{
    enum CallbackAction
    {
        CONTINUE,
        SKIP_OTHERS

    };

    class CallbackHandler
    {
        public List<Func<string, CallbackAction>> ToplevelCallback { get; } = new List<Func<string, CallbackAction>>();
        public List<Func<ChannelMessageEvent, CallbackAction>> Callbacks_ChannelMessage { get; } = new List<Func<ChannelMessageEvent, CallbackAction>>();
        public List<Func<UserActionUponChannel, CallbackAction>> Callbacks_JoinedChannel { get; } = new List<Func<UserActionUponChannel, CallbackAction>>();
        public List<Func<UserActionUponChannel, CallbackAction>> Callbacks_LeaveChannel { get; } = new List<Func<UserActionUponChannel, CallbackAction>>();
        public List<Func<SubscriptionEvent, CallbackAction>> Callbacks_Subscrition { get; } = new List<Func<SubscriptionEvent, CallbackAction>>();
        public List<Func<GiftEvent, CallbackAction>> Callbacks_SubGift { get; } = new List<Func<GiftEvent, CallbackAction>>();
        public List<Func<RaidingEvent, CallbackAction>> Callbacks_RaidingEvent { get; } = new List<Func<RaidingEvent, CallbackAction>>();
        public List<Func<RitualEvent, CallbackAction>> Callbacks_RitualEvent { get; } = new List<Func<RitualEvent, CallbackAction>>();
        public List<Func<BitsEvent, CallbackAction>> Callbacks_BitsEvent { get; } = new List<Func<BitsEvent, CallbackAction>>();
        public CallbackHandler()
        {
        }

        public void AddToplevelCallback(Func<string, CallbackAction> callback)
        {
            ToplevelCallback.Add(callback);
        }

        public void AddToChannelMessageCallback(Func<ChannelMessageEvent, CallbackAction> callback)
        {
            Callbacks_ChannelMessage.Add(callback);
        }

        public void AddToJoinedChannelCallback(Func<UserActionUponChannel, CallbackAction> callback)
        {
            Callbacks_JoinedChannel.Add(callback);
        }

        public void AddToLeaveChannelCallback(Func<UserActionUponChannel, CallbackAction> callback)
        {
            Callbacks_LeaveChannel.Add(callback);
        }

        public void AddToSubscriptionCallback(Func<SubscriptionEvent, CallbackAction> callback)
        {
            Callbacks_Subscrition.Add(callback);
        }
        public void AddToSubGiftCallback(Func<GiftEvent, CallbackAction> callback)
        {
            Callbacks_SubGift.Add(callback);
        }

        public void AddToRaidingEventCallback(Func<RaidingEvent, CallbackAction> callback)
        {
            Callbacks_RaidingEvent.Add(callback);
        }

        public void AddToRitualEventCallback(Func<RitualEvent, CallbackAction> callback)
        {
            Callbacks_RitualEvent.Add(callback);
        }

        public void AddToBitsEventCallback(Func<BitsEvent, CallbackAction> callback)
        {
            Callbacks_BitsEvent.Add(callback);
        }
        public void RunCallbacks(string data)
        {
            long start = DateTime.Now.Millisecond;
            foreach (var callback in ToplevelCallback)
            {
                callback(data);
            }
            long deltaTime = DateTime.Now.Millisecond - start;
            BotLogger.LogDebug($"It took: {deltaTime}ms to run callbacks.");
        }

        public void RunOnChannelMessageCallbacks(ChannelMessageEvent channelMessage)
        {
            BotLogger.LogMessage($"#{channelMessage.Channel} <{channelMessage.Username}> {channelMessage.Message}");
            foreach (var callback in this.Callbacks_ChannelMessage)
                if (callback(channelMessage) == CallbackAction.SKIP_OTHERS)
                    break;
        }

        public void RunOnJoinedChannelCallback(UserActionUponChannel information)
        {
            BotLogger.LogMessage($"{information.Username} joined the channel: #{information.Channel}");
            foreach (var callback in this.Callbacks_JoinedChannel)
                if (callback(information) == CallbackAction.SKIP_OTHERS)
                    break;
        }

        public void RunOnLeaveChannelCallback(UserActionUponChannel information)
        {
            BotLogger.LogMessage($"{information.Username} left the channel: #{information.Channel}");
            foreach (var callback in this.Callbacks_LeaveChannel)
                if (callback(information) == CallbackAction.SKIP_OTHERS)
                    break;
        }

        public void RunOnSubscribeCallback(SubscriptionEvent subEvent)
        {
            var re_subscribed = (subEvent.Subtype == "sub") ? "subscribed" : "resubscribed";
            BotLogger.LogMessage($"{subEvent.Username} {re_subscribed} the channel: #{subEvent.Channel}");
            foreach (var callback in this.Callbacks_Subscrition)
                if (callback(subEvent) == CallbackAction.SKIP_OTHERS)
                    break;
        }


        public void RunOnSubGiftCallback(GiftEvent giftevent)
        {
            BotLogger.LogMessage(giftevent.Message);
            foreach (var callback in this.Callbacks_SubGift)
                if (callback(giftevent) == CallbackAction.SKIP_OTHERS)
                    break;
        }


        public void RunOnRaidingCallbacks(RaidingEvent raidingEvent)
        {
            BotLogger.LogMessage(raidingEvent.Message);
            foreach (var callback in this.Callbacks_RaidingEvent)
                if (callback(raidingEvent) == CallbackAction.SKIP_OTHERS)
                    break;
        }


        public void RunOnRitualEvent(RitualEvent ritualEvent)
        {
            BotLogger.LogMessage(ritualEvent.EventMessage);
            foreach (var callback in this.Callbacks_RitualEvent)
                if (callback(ritualEvent) == CallbackAction.SKIP_OTHERS)
                    break;
        }

        public void RunOnBitsEvent(BitsEvent bitsEvent)
        {
            BotLogger.LogMessage($"{bitsEvent.Username} sent {bitsEvent.NumberOfBits} bits!");
            foreach (var callback in this.Callbacks_BitsEvent)
                if (callback(bitsEvent) == CallbackAction.SKIP_OTHERS)
                    break;
        }
    }
}