using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;

namespace NoHarmony
{
    public class NoHarmonyLoader : MBSubModuleBase
    {
        protected static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public static bool Logging = true; //        Enable basic logging
        public static bool NHLStopOnError = true; // Stop on error = true, try to continue = false
        public static PhaseLog NHLPhase = PhaseLog.All;
        public static TypeLog NHLType = TypeLog.None;


        /// <summary>
        /// Put here all behaviors and models you want NoHarmony to handle using method AddItem
        /// </summary>
        protected void NoHarmonyInit()
        {

        }





        /// <summary>
        /// Called first in order, always executed. Models are loaded here usually.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="gameStarterObject"></param>
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            NoHarmonyInit();
            if (!(game.GameType is Campaign))
            {
                return;
            }
            NHLLogging(PhaseLog.OnGameStart, gameStarterObject);
            
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
        }

        public enum ModeReplace { Replace = 0, ReplaceOrAdd = 1 }
        public enum PhaseLog { None, All, OnGameStart, OnCampaignStart, OnGameLoaded, OnNewGameCreated, OnSubModuleLoad }
        public enum AddToPhase { Auto, OnGameStart, OnCampaignStart, OnGameLoaded, OnNewGameCreated}
        public enum TypeLog { None = 0, Models = 1, Behaviors = 2, All = 3}
        private List<NHLTask> NHLTodo;

        private struct NHLTask {
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
        /// 
        /// </summary>
        /// <param name="addedObject">Model or Behavior type to add</param>
        /// <param name="replacedObject">Model or Behavior type you want replaced (not required)</param>
        /// <param name="mode">Mode used for replace</param>
        /// <param name="insert">When should the object be added. Auto let's NoHarmony decide.</param>
        protected void AddItem(Type addedObject, Type replacedObject = null, ModeReplace mode = ModeReplace.Replace, AddToPhase insert = AddToPhase.Auto)
        {
            if (typeof(CampaignBehaviorBase).IsAssignableFrom(addedObject))
            {
                if(insert == AddToPhase.Auto)
                {
                    NHLTodo.Add(new NHLTask(addedObject, replacedObject, mode, AddToPhase.OnCampaignStart));
                    NHLTodo.Add(new NHLTask(addedObject, replacedObject, mode, AddToPhase.OnGameLoaded));
                    return;
                }
                    
            }else if (typeof(GameModel).IsAssignableFrom(addedObject))
            {
                if (insert == AddToPhase.Auto)
                {
                    NHLTodo.Add(new NHLTask(addedObject, replacedObject, mode, AddToPhase.OnGameStart));
                    return;
                }
            }
            else if(Logging)
            {
                Log.Error("Init - "+ addedObject + " is not a valide model or behavior!");
                return;
            }
            NHLTodo.Add(new NHLTask(addedObject,replacedObject,mode,insert));
        }

        protected void NHHandler(CampaignGameStarter gameInitializer, AddToPhase call)
        {
            bool bInit = false, mInit = false;
            if (gameInitializer.CampaignBehaviors is IList<CampaignBehaviorBase> CampaignBehaviors)
            {
                bInit = true;
            }
            if (gameInitializer.Models is IList<GameModel> models)
            {
                mInit = true;
            }


            foreach ( NHLTask temp in NHLTodo)
            {
                if(temp.phase != call)
                {
                    continue;
                }

                if (typeof(CampaignBehaviorBase).IsAssignableFrom(temp.add))
                {
                    bool found = false;
                    for (int index = 0; index < CampaignBehaviors.Count; ++index)
                    {
                        if (CampaignBehaviors[index] is TBaseType)
                        {
                            found = true;
                            if (CampaignBehaviors[index] is TChildType)
                            {
                                Log.Info($"Child behavior {typeof(TChildType).Name} found, skipping.");
                            }
                            else
                            {
                                Log.Info($"Base behavior {typeof(TBaseType).Name} found. Replacing with child model {typeof(TChildType).Name}");
                                CampaignBehaviors[index] = Activator.CreateInstance<TChildType>();
                            }
                        }
                    }
                }

            }
        }

        /*protected void NHLAdd<TBaseType, TChildType>(IGameStarter gameStarterObject)
            where TBaseType : GameModel
            where TChildType : TBaseType
        {
            if (!(gameStarterObject.Models is IList<GameModel> models))
            {
                return;
            }

            bool found = false;
            for (int index = 0; index < models.Count; ++index)
            {
                if (models[index] is TBaseType)
                {
                    found = true;
                    if (models[index] is TChildType)
                    {
                        Log.Info($"Child model {typeof(TChildType).Name} found, skipping.");
                    }
                    else
                    {
                        Log.Info($"Base model {typeof(TBaseType).Name} found. Replacing with child model {typeof(TChildType).Name}");
                        models[index] = Activator.CreateInstance<TChildType>();
                    }
                }
            }

            if (!found)
            {
                Log.Info($"Base model {typeof(TBaseType).Name} was not found. Adding child model {typeof(TChildType).Name}");
                gameStarterObject.AddModel(Activator.CreateInstance<TChildType>());
            }
        }*/

        //logging stuff
        protected override void OnSubModuleLoad()
        {
            NLog.Config.LoggingConfiguration logConfig = new NLog.Config.LoggingConfiguration();
            NLog.Targets.FileTarget logFile = new NLog.Targets.FileTarget(LogFileTarget()) { FileName = LogFilePath() };

            logConfig.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logFile);
            NLog.LogManager.Configuration = logConfig;
            NHLLogging(PhaseLog.OnSubModuleLoad, null);
        }

        protected void NHLLogging(PhaseLog phase, object starterObject)
        {
            if (!Logging || (NHLPhase != PhaseLog.All && phase != NHLPhase))
                return;
            Log.Info(phase);
            if (starterObject == null)
                return;
            CampaignGameStarter gameStarter = (CampaignGameStarter)starterObject;
            if (gameStarter.Models is IList<GameModel> models && (NHLType == TypeLog.All || NHLType == TypeLog.Models))
            {
                Log.Info(" @ Model list");
                for (int index = 0; index < models.Count; ++index)
                {
                    Log.Info(index + " -> "+models[index].GetType().ToString());
                }
            }
            if (gameStarter.CampaignBehaviors is IList<CampaignBehaviorBase> cBehaviors && (NHLType == TypeLog.All || NHLType == TypeLog.Behaviors))
            {
                Log.Info(" @ Behavior list");
                for (int index = 0; index < cBehaviors.Count; ++index)
                {
                    Log.Info(index + " -> " + cBehaviors[index].GetType().ToString());
                }
            }
        }

        protected virtual string LogFileTarget()
        {
            return "NoHarmony";
        }

        protected virtual string LogFilePath()
        {
            // The default, relative path will place the log in $(GameFolder)\bin\Win64_Shipping_Client\
            return "NoHarmony.txt";
        }
    }
}
