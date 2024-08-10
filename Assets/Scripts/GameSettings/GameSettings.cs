using System.Collections;
using System.Collections.Generic;
using Poker;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGameSettings", menuName = "GameSettings", order = 1)]
public class GameSettings : ScriptableObject
{
    public string settingsName;

    [Header("User Settings")] public int userPosition;
    public bool randomUserHand;
    public bool userHandSuited;
    public bool userHandPaired;
    public long userStartingMoney;
    public bool enableCustomUserHand;
    public List<Card> customUserHand;

    [Header("Other Players Settings")] public int numberOfPlayers;
    public bool randomPlayerHand;
    public bool playerHandSuited;
    public bool playerHandPaired;
    public long playerStartingMoney;
    public bool enableCustomPlayerList;
    public List<Player> customPlayerList;
    public bool enableCustomPlayerActionLog;

    [Header("Game Settings")] public bool randomGame;
    public long startingPotSize;
    public int communityCardSize;
    public bool enableCustomCommunityCards;

    public List<Card> customCommunityCards;

    [Header("Scenarios")] public BluffCases.BluffCase scenario;
}