﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class GameMainState : IGameState
{
    private string sceneName;
    private static List<string> levels = new List<string>{"RSLevel", "TetrisLevel", "BinaryLevel", "LightFeatures", "SpotlightTest"};
    private int levelNumber;

    public GameMainState(int levelNumber = 0) {
        this.levelNumber = levelNumber;
        sceneName = levels[Mathf.Min(levelNumber, levels.Count)];
    }

    public static void CheckForLevelIntegrity()
    {
        foreach (string levelName in levels)
        {
            Assert.IsTrue(Application.CanStreamedLevelBeLoaded(levelName), "Level " + levelName + " was listed in GameMain.cs but cannot be loaded.");
        }
    }

    public GameMainState GetStateForNextLevel() {
        return new GameMainState(levelNumber + 1);
    }

    public void SetAsActiveScene() {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
    }

    /*
     * Load this scene asynchronously.
     */
    public AsyncOperation loadAsynchronously()
    {
        return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    void IGameState.onEnter()
    {
        // No-op, this scene should be loaded asynchronously.
    }

    void IGameState.onExit()
    {
        SceneManager.UnloadSceneAsync(sceneName);
    }

    void IGameState.onPause()
    {
        SceneManager.GetSceneByName(sceneName).GetRootGameObjects()[0].SetActive(false);
    }

    void IGameState.onUnpause()
    {
        SceneManager.GetSceneByName(sceneName).GetRootGameObjects()[0].SetActive(true);
    }
}
