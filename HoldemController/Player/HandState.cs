﻿using System;
using System.Collections.Generic;
using System.Linq;
using HoldemPlayerContract;

namespace HoldemController.Player
{
    public class HandState : IHand
    {
        private readonly List<HandAction> _actions = new List<HandAction>();
        private readonly Dictionary<int, PlayerHandState> _players;

        public HandState(IEnumerable<PlayerInfo> players, int smallBlind, int bigBlind)
        {
            _players = players.ToDictionary(p => p.PlayerNum, p=> new PlayerHandState(p));
            SmallBlind = smallBlind;
            BigBlind = bigBlind;
        }

        public int SmallBlind { get; }
        public int BigBlind { get; }
        public EStage Stage { get; private set; }
        
        public Card[] CommunityCards => new Card[5];
        
        public void SetCommunityCard(Card card, EBoardCardType cardType)
        {
            CommunityCards[(int)cardType] = card;
        }

        public void SetAction(EStage stage, int playerId, EActionType action, int amount)
        {
            CheckUpdateStage(stage);

            var player = _players[playerId];

            var playerAction = new PokerActionHistory(stage, action, amount);
            
            player.SetAction(playerAction);

            switch (action)
            {
                case EActionType.ActionBlind:
                case EActionType.ActionFold:
                case EActionType.ActionCheck:
                case EActionType.ActionCall:
                case EActionType.ActionRaise:
                    _actions.Add(new HandAction(player.Player, playerAction));
                    //player.SetAction(pAction);
                    break;
                case EActionType.ActionShow:
                    break;
                case EActionType.ActionWin:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        private void CheckUpdateStage(EStage stage)
        {
            if (stage != Stage)
            {
                if (stage != (Stage + 1) && stage != EStage.StageShowdown)
                {
                    throw new Exception($"Can not move from stage {Stage} to stage {stage}");
                }
                Stage = stage;
                foreach (var player in _players.Values)
                {
                    player.UpdateStage(stage);
                }
            }
        }

        public PlayerHandState GetPlayer(int playerId)
        {
            return _players[playerId];
        }
    }
}