
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Verse;
using static WarOnDrug.Window_WarStatus;

namespace WarOnDrug.ContectOperation
{
    public class ContectMission : IExposable
    {
        private int UniqueID = Random.Range(10000,100000);//A randomo number to mask the true id in display
        private int id;
        private int tickLeft;
        public int mp = 0;
        public ContectMissionStatus missionStatus = ContectMissionStatus.Created;
        public contectActions action = contectActions.None;
        public Dictionary<ThingDef, int> resourcesNeeded;
        public Dictionary<ThingDef, int> resources;
        public Dictionary<ThingDef, int> reward;

        public int ID { get => id; }

        public ContectMission(List<ThingDef> resourcesItems, List<int> resourcesItemsCount)
        {

            resourcesNeeded = new Dictionary<ThingDef, int>(resourcesItems.Count);
            resources = new Dictionary<ThingDef, int>(resourcesItems.Count);
            for (int i = 0; i < resourcesItems.Count; i++)
            {
                if (resourcesNeeded.ContainsKey(resourcesItems[i]))
                {
                    resourcesNeeded[resourcesItems[i]] += resourcesItemsCount[i];
                }
                else
                {
                    resourcesNeeded.Add(resourcesItems[i], resourcesItemsCount[i]);
                    resources.Add(resourcesItems[i], 0);
                }

            }
            id = Find.UniqueIDsManager.GetNextQuestID();
        }

        public bool isResourcesEnough()
        {
            foreach (var item in resourcesNeeded)
            {
                if (resources[item.Key] < item.Value)
                {
                    return false;
                }
            }
            return true;
        }

        public string getDisplayID()
        {
            var tmpSource = ASCIIEncoding.ASCII.GetBytes(id.ToString() + UniqueID.ToString());
            byte[] tmpNewHash;
            tmpNewHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
            return System.BitConverter.ToString(tmpNewHash).Replace("-", "").Substring(0,6);
        }

        void IExposable.ExposeData()
        {
            Scribe_Values.Look(ref id, "ID");
            Scribe_Values.Look(ref mp, "mp");
            Scribe_Values.Look(ref missionStatus, "missionStatus");
            Scribe_Collections.Look(ref resourcesNeeded, "resourcesNeeded", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref resources, "resources", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref reward, "reward", LookMode.Def, LookMode.Value);
            Scribe_Values.Look(ref UniqueID, "UniqueID");
        }

        public bool Tick()
        {
            return true;
        }
    }

    public enum ContectMissionStatus
    {
        Created,
        WaitingResources,
        WaitingPerson,
        InProgress,
        Completed,
        Delivered
    }



}
