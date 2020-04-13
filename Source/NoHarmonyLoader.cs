using System;
using System.IO;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;

namespace NoHarmony // v0.9.4
{
    public abstract class NoHarmonyLoader : MBSubModuleBase
    {
        public bool Logging = true; //        Enable basic logging
        public bool NHLStopOnError = true; // Stop on error = true, try to continue = false
        public PhaseLog PhaseToLog = PhaseLog.All;
        public TypeLog ObjectsToLog = TypeLog.None;
        public LogLvl MinLogLvl = LogLvl.Info;
        public string LogFile = "NoHarmony.txt";


        /// <summary>
        /// Put here all the code ou want executed before the game mains's menu
        /// </summary>
        public abstract void NoHarmonyInit();

        /// <summary>
        /// Put here all behaviors and models you want NoHarmony to handle using method AddBehavior ReplaceBehavior AddModel ReplaceModel.
        /// Public NoHarmony variables can be changed here.
        /// </summary>
        public abstract void NoHarmonyLoad();



        /// <summary>
        /// Called before the main menu.
        /// </summary>
        protected override void OnSubModuleLoad()
        {
            NHLTodo = new List<NHLTask>();
            NHLLogging(PhaseLog.OnSubModuleLoad, null);
            NoHarmonyInit();
        }

        
        /// <summary>
        /// Called first in order, always executed. Models are loaded here usually.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="gameStarterObject"></param>
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            NoHarmonyLoad();
            if (!(game.GameType is Campaign))
            {
                return;
            }
            NHLLogging(PhaseLog.OnGameStart, gameStarterObject);
            CampaignGameStarter gameInitializer = (CampaignGameStarter)gameStarterObject;
            NHHandler(gameInitializer, AddToPhase.OnGameStart);
        }

        /// <summary>
        /// Executed after OngameStart and only for a new campaign. Campaign behaviors are loaded here usually. (Used when OnGameLoaded is not triggered)
        /// </summary>
        /// <param name="game"></param>
        /// <param name="starterObject"></param>
        public override void OnCampaignStart(Game game, object starterObject)
        {
            if (!(game.GameType is Campaign))
            {
                return;
            }
            NHLLogging(PhaseLog.OnCampaignStart, starterObject);
            CampaignGameStarter gameInitializer = (CampaignGameStarter)starterObject;
            NHHandler(gameInitializer, AddToPhase.OnCampaignStart);
        }

        /// <summary>
        /// Executed after OnCampaignStart, for new games only.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="initializerObject"></param>
        public override void OnNewGameCreated(Game game, object initializerObject)
        {
            if (!(game.GameType is Campaign))
            {
                return;
            }
            NHLLogging(PhaseLog.OnNewGameCreated, initializerObject);
            CampaignGameStarter gameInitializer = (CampaignGameStarter)initializerObject;
            NHHandler(gameInitializer, AddToPhase.OnNewGameCreated);
        }

        /// <summary>
        /// Executed when a game is loaded, after OngameStart. Campaign behaviors are loaded here usually. (used when OnCampaignStart is not triggered)
        /// </summary>
        /// <param name="game"></param>
        /// <param name="initializerObject"></param>
        public override void OnGameLoaded(Game game, object initializerObject)
        {
            if (!(game.GameType is Campaign))
            {
                return;
            }
            NHLLogging(PhaseLog.OnGameLoaded, initializerObject);
            CampaignGameStarter gameInitializer = (CampaignGameStarter)initializerObject;
            NHHandler(gameInitializer, AddToPhase.OnGameLoaded);
        }

        // NoHarmony core features past this point
        public enum ModeReplace { Replace = 0, ReplaceOrAdd = 1 }
        public enum PhaseLog { None, All, OnGameStart, OnCampaignStart, OnGameLoaded, OnNewGameCreated, OnSubModuleLoad }
        public enum AddToPhase { Auto, OnGameStart, OnCampaignStart, OnGameLoaded, OnNewGameCreated }
        public enum TypeLog { None = 0, Models = 1, Behaviors = 2, All = 3 }
        private List<NHLTask> NHLTodo;
        public enum LogLvl { Tracking = 0, Info = 1, Warning = 2, Error = 3 }

        private struct NHLTask
        {
            public Type add, replace;
            public ModeReplace mode;
            public AddToPhase phase;
            public NHLTask(Type a, Type b, ModeReplace m, AddToPhase p)
            {
                add = a;
                replace = b;
                mode = m;
                phase = p;
            }
        }

        /// <summary>
        /// Use it to add a campaignbehavior to the game. If the model might already be present use ReplaceBehavior instead.
        /// </summary>
        /// <typeparam name="AddType">The behavior you want to add.</typeparam>
        /// <param name="mode"></param>
        /// <param name="insert"></param>
        protected void AddBehavior<AddType>(ModeReplace mode = ModeReplace.ReplaceOrAdd, AddToPhase insert = AddToPhase.Auto)
            where AddType : CampaignBehaviorBase
        {
            AddItem(typeof(AddType), null, mode, insert);
        }

        /// <summary>
        /// Use it to add a model to the game. If the model might already be present use ReplaceModel instead.
        /// </summary>
        /// <typeparam name="AddType">The model you want to add.</typeparam>
        /// <param name="mode"></param>
        /// <param name="insert"></param>
        protected void AddModel<AddType>(ModeReplace mode = ModeReplace.ReplaceOrAdd, AddToPhase insert = AddToPhase.Auto)
            where AddType : GameModel
        {
            AddItem(typeof(AddType), null, mode, insert);
        }

        /// <summary>
        /// Use it to replace a behavior.
        /// </summary>
        /// <typeparam name="AddType"></typeparam>
        /// <typeparam name="ReplaceType"></typeparam>
        /// <param name="mode"></param>
        /// <param name="insert"></param>
        protected void ReplaceBehavior<AddType,ReplaceType>(ModeReplace mode = ModeReplace.ReplaceOrAdd, AddToPhase insert = AddToPhase.Auto)
            where ReplaceType : CampaignBehaviorBase
            where AddType : ReplaceType
        {
            AddItem(typeof(AddType), typeof(ReplaceType), mode, insert);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="AddType"></typeparam>
        /// <typeparam name="ReplaceType"></typeparam>
        /// <param name="mode"></param>
        /// <param name="insert"></param>
        protected void ReplaceModel<AddType, ReplaceType>(ModeReplace mode = ModeReplace.ReplaceOrAdd, AddToPhase insert = AddToPhase.Auto)
            where ReplaceType : GameModel
            where AddType : ReplaceType
        {
            AddItem(typeof(AddType), typeof(ReplaceType), mode, insert);
        }


        /// <summary>
        /// You should use AddBehavior AddModel ReplaceBehavior ReplaceModel instead of this method, they are less prone to errors
        /// Method to use in NoHarmonyLoader() to add models or campaignbehavior to the game.
        /// </summary>
        /// <param name="addedObject">Model or Behavior type to add (use "typeof()")</param>
        /// <param name="replacedObject">Model or Behavior type you want replaced (not required)</param>
        /// <param name="mode">Mode used for replace</param>
        /// <param name="insert">When should the object be added. Auto let's NoHarmony decide.</param>
        protected void AddItem(Type addedObject, Type replacedObject = null, ModeReplace mode = ModeReplace.ReplaceOrAdd, AddToPhase insert = AddToPhase.Auto)
        {
            if (typeof(CampaignBehaviorBase).IsAssignableFrom(addedObject))
            {
                if (insert == AddToPhase.Auto)
                {
                    NHLTodo.Add(new NHLTask(addedObject, replacedObject, mode, AddToPhase.OnCampaignStart));
                    NHLTodo.Add(new NHLTask(addedObject, replacedObject, mode, AddToPhase.OnGameLoaded));
                    return;
                }

            }
            else if (typeof(GameModel).IsAssignableFrom(addedObject))
            {
                if (insert == AddToPhase.Auto)
                {
                    NHLTodo.Add(new NHLTask(addedObject, replacedObject, mode, AddToPhase.OnGameStart));
                    return;
                }
            }
            else
            {
                Log(LogLvl.Error, "Loading error - " + addedObject + " is not a valide model or behavior!");
                return;
            }
            NHLTodo.Add(new NHLTask(addedObject, replacedObject, mode, insert));
        }

        protected void NHHandler(CampaignGameStarter gameInitializer, AddToPhase call)
        {
            IList<CampaignBehaviorBase> cBehaviors = gameInitializer.CampaignBehaviors as IList<CampaignBehaviorBase>;
            IList<GameModel> models = gameInitializer.Models as IList<GameModel>;

            foreach (NHLTask temp in NHLTodo)
            {
                if (temp.replace == typeof(GameModel) || temp.replace == typeof(CampaignBehaviorBase))
                {
                    continue;
                }
                if (temp.phase != call)
                {
                    continue;
                }

                if (typeof(CampaignBehaviorBase).IsAssignableFrom(temp.add))
                {
                    if (cBehaviors == null)
                    {
                        continue;
                    }
                    bool found = false;
                    for (int index = 0; index < cBehaviors.Count; ++index)
                    {
                        if (temp.replace != null && cBehaviors[index].GetType().IsAssignableFrom(temp.replace))
                        {
                            found = true;
                            if (cBehaviors[index].GetType() == temp.add)
                            {
                                Log(LogLvl.Warning, $"Behavior {temp.add.Name} already present.");
                            }
                            else
                            {
                                Log(LogLvl.Info, $"{temp.replace.Name} found. Replacing with {temp.add.Name}");
                                cBehaviors[index] = (CampaignBehaviorBase)Activator.CreateInstance(temp.add);
                            }
                        }
                    }
                    if (!found)
                    {
                        if (temp.replace != null && temp.mode == ModeReplace.Replace)
                        {
                            Log(LogLvl.Warning, $"Behavior {temp.replace.Name} not found.");
                            continue;
                        }
                        gameInitializer.AddBehavior((CampaignBehaviorBase)Activator.CreateInstance(temp.add));
                    }
                }
                if (typeof(GameModel).IsAssignableFrom(temp.add))
                {
                    if (models == null)
                    {
                        continue;
                    }
                    bool found = false;
                    for (int index = 0; index < models.Count; ++index)
                    {
                        if (temp.replace != null && models[index].GetType().IsAssignableFrom(temp.replace))
                        {
                            found = true;
                            if (models[index].GetType() == temp.add)
                            {
                                Log(LogLvl.Warning, $"Model {temp.add.Name} already present.");
                            }
                            else
                            {
                                Log(LogLvl.Info, $"{temp.replace.Name} found. Replacing with {temp.add.Name}");
                                models[index] = (GameModel)Activator.CreateInstance(temp.add);
                            }
                        }
                    }
                    if (!found)
                    {
                        if (temp.replace != null && temp.mode == ModeReplace.Replace)
                        {
                            Log(LogLvl.Warning, $"Model {temp.replace.Name} not found.");
                            continue;
                        }
                        gameInitializer.AddModel((GameModel)Activator.CreateInstance(temp.add));
                    }
                }

            }
        }

        //Logging Core
        public void Log(LogLvl mLvl, string message)
        {
            if (mLvl.CompareTo(MinLogLvl) < 0 || !Logging)
                return;
            switch (mLvl)
            {
                case LogLvl.Error:
                    message = "!!![Error] " + message + " !!!";
                    break;
                case LogLvl.Warning:
                    message = "[Warn]/!\\ " + message;
                    break;
                case LogLvl.Info:
                    message = "[Info] " + message;
                    break;
                case LogLvl.Tracking:
                    message = "[Track] " + message;
                    break;
            }
            using (StreamWriter sw = new StreamWriter(LogFile, true))
                sw.WriteLine(DateTime.Now.ToString("dd/mm/yy HH:mm:ss.fff") + " > " + message);
        }

        protected void NHLLogging(PhaseLog phase, object starterObject)
        {
            if (!Logging || (PhaseToLog != PhaseLog.All && phase != PhaseToLog))
                return;
            Log(LogLvl.Info, phase.ToString());
            if (starterObject == null)
                return;
            CampaignGameStarter gameStarter = (CampaignGameStarter)starterObject;
            if (gameStarter.Models is IList<GameModel> models && (ObjectsToLog == TypeLog.All || ObjectsToLog == TypeLog.Models))
            {
                Log(LogLvl.Tracking, " * Model list");
                for (int index = 0; index < models.Count; ++index)
                {
                    Log(LogLvl.Tracking, index + " -> " + models[index].GetType().ToString());
                }
            }
            if (gameStarter.CampaignBehaviors is IList<CampaignBehaviorBase> cBehaviors && (ObjectsToLog == TypeLog.All || ObjectsToLog == TypeLog.Behaviors))
            {
                Log(LogLvl.Tracking, " * Behavior list");
                for (int index = 0; index < cBehaviors.Count; ++index)
                {
                    Log(LogLvl.Tracking, index + " -> " + cBehaviors[index].GetType().ToString());
                }
            }
        }
    }
}