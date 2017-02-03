﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CVARC.V2;
using UnityEngine;
using Assets;
using Assets.Dlc;
using Assets.Servers;
using CVARC.Core;
using UnityEngineInternal;
using UnityCommons;

public static class Dispatcher
{
    public const int TimeScale = UnityConstants.TimeScale;
    public static Loader Loader { get; private set; }
    public static IRunner CurrentRunner { get; private set; }
    public static bool UnityShutdown { get; private set; }

    static bool logPlayRequested;
    static readonly RunnersQueue queue = new RunnersQueue();
    static readonly Dictionary<string, GameObject> objectsCache = new Dictionary<string, GameObject>();
    static bool isGameOver;
    static bool switchingScenes;


    public static void Start()
    {
        Time.timeScale = TimeScale;

        Debug.Log("Logging types...");
        foreach(var e in Settings.Current.DebugTypes)
        {
            Debug.Log(e);
            Debugger.Settings.EnableType(e);
        }
        Debugger.Logger = Debug.Log;

        if (!Directory.Exists(UnityConstants.LogFolderRoot))
            Directory.CreateDirectory(UnityConstants.LogFolderRoot);
        

        Loader = new Loader();
        Debugger.Log("Loader ready. Starting: adding levels");
        Debugger.Log("======================= Tutorial competition:" + Settings.Current.TutorialCompetitions);

        
        //Loader.AddLevel("Demo", "Test", () => new DemoCompetitions.Level1());
        //Loader.AddLevel("RoboMovies", "Test", () => new RMCompetitions.Level1());
        //Loader.AddLevel("Pudge", "Level1", () => new PudgeCompetitions.Level1());
        //Debugger.Log(DebuggerMessageType.Initialization, "Lvl1 ready. Starting: lvl 2");
        //Loader.AddLevel("Pudge", "Level2", () => new PudgeCompetitions.Level2());
        //Debugger.Log(DebuggerMessageType.Initialization, "Lvl2 ready. Starting: lvl 3");
        //Loader.AddLevel("Pudge", "Level3", () => new PudgeCompetitions.Level3());
        //Debugger.Log(DebuggerMessageType.Initialization, "Lvl3 ready. Starting: lvl Test");
        //Loader.AddLevel("Pudge", "Test", () => new PudgeCompetitions.TestLevel());
        //Debugger.Log(DebuggerMessageType.Initialization, "LvlTest ready. All levels ready");
        //Loader.AddLevel("Demo", "Level1", () => new DemoCompetitions.Level1());
        //Debugger.Log(DebuggerMessageType.Initialization, "Demo Lvl1 ready");
        //Loader.AddLevel("TheBeachBots", "Test", () => new TBBCompetitions.Level1());

      
    }

    public static void FillLoader(IDlcEntryPoint entryPoint)
    {
        foreach (var level in entryPoint.GetLevels())
        {
            Loader.AddLevel(level.CompetitionsName, level.LevelName, () => level);
        }
    }

    public static void AddRunner(IRunner runner)
    {
        queue.EnqueueRunner(runner);
    }

    public static void IntroductionTick()
    {
        if (queue.HasReadyRunner())
            SwitchScene("Round");
        if (logPlayRequested)
        {
            logPlayRequested = false;
            SwitchScene("LogRound");
        }
    }

    public static void RoundStart()
    {
        CurrentRunner = queue.DequeueReadyRunner();
        isGameOver = false;
        CurrentRunner.InitializeWorld();

    }

    public static void RoundTick()
    {
        // конец игры
        if (isGameOver && CurrentRunner != null)
        {
            Debug.Log("game over. disposing");
            CurrentRunner.Dispose();
            CurrentRunner = null;
        }

        if (CurrentRunner == null)
        {
            // очищение, или переход в начало
            SwitchScene(queue.HasReadyRunner() ? "Round" : "Intro");
            return;
        }

        // прерывание
        if (queue.HasReadyRunner() && CurrentRunner.CanInterrupt)
            SetGameOver();
    }

    public static void RequestLogPlay()
    {
        logPlayRequested = true;
    }

    public static void LogEnd()
    {
        SwitchScene("Intro");
    }

    // самый ГЛОБАЛЬНЫЙ выход, из всей юнити. Вызывается из сцен.
    public static void OnDispose()
    {
        if (switchingScenes)
        {
            switchingScenes = false;
            return;
        }

        Debugger.Log("GLOBAL EXIT");
        
        if (CurrentRunner != null)
            CurrentRunner.Dispose();
        queue.DisposeRunners();
        UnityShutdown = true;
    }

    public static void SetGameOver()
    {
        isGameOver = true;
      
    }

    static void SwitchScene(string sceneName)
    {
        objectsCache.Clear();
        switchingScenes = true;
        Application.LoadLevel(sceneName);
    }
   
}
