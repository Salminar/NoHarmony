using System;
using System.IO;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;


namespace NoHarmony //Exp v0.9.7
{
    public abstract class NoHarmonyLoader : MBSubModuleBase
    {
        public bool Logging = true;
        public TypeLog ObjectsToLog = TypeLog.None;
        public LogLvl MinLogLvl = LogLvl.Info;
        public string LogFile = "NoHarmony.txt";

        /// <summary>
        /// Put NoHarmony Initialise code here
        /// </summary>
        public abstract void NoHarmonyInit();

        /// <summary>
        /// Use add and replace NoHarmony methods here to load your modules;
        /// </summary>
        public abstract void NoHarmonyLoad();
        //End config

        //Submodule methodes representing various game initialisation phases,
        /// <summary>
        /// Called before the main menu.
        /// </summary>
        protected override void OnSubModuleLoad()
        {
            NoHarmonyInit();
            BTasks = new List<NHLTask>();
            MTasks = new List<NHLTask>();
            IsInit = true;
            NoHarmonyLoad();
            Log(LogLvl.Info, "Pending tasks : " + BTasks.Count + " models, " + MTasks.Count + " behaviors.");
        }

        /// <summary>
        /// Called first in order, always executed. Models are loaded here usually.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="gameStarterObject"></param>
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (!(game.GameType is Campaign))
                return;
            CampaignGameStarter gameInitializer = (CampaignGameStarter)gameStarterObject;
            NHLModel(gameInitializer);
        }

        /// <summary>
        /// Executed after the initializer is integrated to the campaign object. You can't add models anymore.
        /// </summary>
        /// <param name="game"></param>
        public override void OnGameInitializationFinished(Game game)
        {
            if (!(game.GameType is Campaign campaign))
                return;
            NHLBehavior(campaign);
        }

        // NoHarmony core features past this point
        public enum TaskMode { Replace = 0, ReplaceOrAdd = 1, RemoveAndAdd = 2 }
        public enum TypeLog { None = 0, Models = 1, Behaviors = 2, All = 3 }
        public enum LogLvl { Tracking = 0, Info = 1, Warning = 2, Error = 3 }
        private bool IsInit = false;
        List<NHLTask> BTasks;
        List<NHLTask> MTasks;
        private struct NHLTask
        {
            public Type add, remove;
            public TaskMode mode;
            public NHLTask(Type a, Type b, TaskMode m)
            {
                add = a;
                remove = b;
                mode = m;
            }
        }



        /// <summary>
        /// Use it to add a campaignbehavior to the game. If the model might already be present use ReplaceBehavior instead.
        /// </summary>
        /// <typeparam name="AddType">The behavior you want to add.</typeparam>
        /// <param name="mode">Unused, only for compatibility with NoHarmony</param>
        protected void AddBehavior<AddType>(TaskMode mode = TaskMode.ReplaceOrAdd)
            where AddType : CampaignBehaviorBase
        {
            BTasks.Add(new NHLTask(typeof(AddType), null, TaskMode.ReplaceOrAdd));
        }

        /// <summary>
        /// Use it to add a model to the game. If the model might already be present use ReplaceModel instead.
        /// </summary>
        /// <typeparam name="AddType">The model you want to add.</typeparam>
        /// <param name="mode">Unused, only for compatibility with NoHarmony</param>
        protected void AddModel<AddType>(TaskMode mode = TaskMode.ReplaceOrAdd)
            where AddType : GameModel
        {
            MTasks.Add(new NHLTask(typeof(AddType), null, TaskMode.ReplaceOrAdd));
        }

        /// <summary>
        /// Use it to replace a behavior.
        /// </summary>
        /// <typeparam name="AddType"></typeparam>
        /// <typeparam name="ReplaceType"></typeparam>
        /// <param name="mode"></param>
        protected void ReplaceBehavior<AddType, ReplaceType>(TaskMode mode = TaskMode.ReplaceOrAdd)
            where ReplaceType : CampaignBehaviorBase
            where AddType : ReplaceType
        {
            BTasks.Add(new NHLTask(typeof(AddType), typeof(ReplaceType), mode));
        }

        /// <summary>
        /// Use it to replace a model.
        /// </summary>
        /// <typeparam name="AddType"></typeparam>
        /// <typeparam name="ReplaceType"></typeparam>
        /// <param name="mode"></param>
        protected void ReplaceModel<AddType, ReplaceType>(TaskMode mode = TaskMode.ReplaceOrAdd)
            where ReplaceType : GameModel
            where AddType : ReplaceType
        {
            MTasks.Add(new NHLTask(typeof(AddType), typeof(ReplaceType), mode));
        }

        private void NHLModel(CampaignGameStarter gameI)
        {
            IList<GameModel> models = gameI.Models as IList<GameModel>;
            foreach (NHLTask tmp in MTasks)
            {
                if (tmp.remove != null)
                {
                    for (int index = 0; index < models.Count; ++index)
                    {
                        if (models[index].GetType().IsAssignableFrom(tmp.remove))
                        {
                            if (tmp.add != null)
                                models[index] = (GameModel)Activator.CreateInstance(tmp.add);
                            else
                                models.RemoveAt(index);
                            break;
                        }
                    }
                }
                else
                {
                    gameI.AddModel((GameModel)Activator.CreateInstance(tmp.add));
                }
            }
            if (Logging && (ObjectsToLog == TypeLog.All || ObjectsToLog == TypeLog.Models))
            {
                Log(LogLvl.Tracking, "List of models :");
                for (int index = 0; index < models.Count; ++index)
                {
                    Log(LogLvl.Tracking, models[index].ToString());
                }
            }
        }

        private void NHLBehavior(Campaign campaign)
        {
            CampaignBehaviorManager cbm = (CampaignBehaviorManager)campaign.CampaignBehaviorManager;
            foreach (NHLTask tmp in BTasks)
            {
                if (tmp.remove != null)
                {
                    var cgb = typeof(Campaign).GetMethod("GetCampaignBehavior").MakeGenericMethod(tmp.remove).Invoke(campaign, null);
                    CampaignEvents.RemoveListeners(cgb);
                    typeof(CampaignBehaviorManager).GetMethod("RemoveBehavior").MakeGenericMethod(tmp.remove).Invoke(cbm, null);
                }
                if (tmp.add != null)
                    cbm.AddBehavior((CampaignBehaviorBase)Activator.CreateInstance(tmp.add));
            }
            if (Logging && (ObjectsToLog == TypeLog.All || ObjectsToLog == TypeLog.Behaviors))
            {
                Log(LogLvl.Tracking, "List of models :");
                var cbb = campaign.GetCampaignBehaviors<CampaignBehaviorBase>();
                foreach (CampaignBehaviorBase tmp in cbb)
                {
                    Log(LogLvl.Info, tmp.ToString());
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
                    message = "!![Error] " + message + " !!";
                    break;
                case LogLvl.Warning:
                    message = "![Warn] " + message;
                    break;
                case LogLvl.Info:
                    message = "[Info] " + message;
                    break;
                case LogLvl.Tracking:
                    message = "[Track] " + message;
                    break;
            }
            using (StreamWriter sw = new StreamWriter(LogFile, true))
                sw.WriteLine(DateTime.Now.ToString("dd/MM/yy HH:mm:ss.fff") + " > " + message);
        }
    }
}