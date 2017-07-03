using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Easy Broadcast", "LaserHydra", "2.1.0", ResourceId = 863)]
    [BaseEntity.Menu.Description("Broadcast a message to the server", "aaaa")]
    internal class EasyBroadcast : RustPlugin
    {
        private Dictionary<ulong, PlayerModel> activeList = new Dictionary<ulong, PlayerModel>();

        [HookMethod("OnPlayerInit")]
        public void OnPlayerInit(BasePlayer player)
        {
        }
    }

    [Serializable]
    internal struct SerializeModel
    {
        public ulong uID { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastLoggedIn { get; set; }
        public ulong TotalTime { get; set; }
    }

    internal class PlayerModel
    {
        public ulong uID { get; private set; }
        public bool IsOnline { get; private set; }
        public DateTime CurrentLoginTime { get; private set; }
        private ulong ttime;

        public ulong TotalTime
        {
            get { return (ulong) DateTime.Now.Subtract(CurrentLoginTime).TotalMinutes + ttime; }
            private set { ttime = value; }
        }

        public PlayerModel(BasePlayer p, bool tryFindExisting)
        {
            CreateOrLoadPlayer(p.userID, tryFindExisting);
        }

        private static string GetFile()
        {
            const string dir = @"./saves/";
            return Path.Combine(dir, "users.bin");
        }

        private static List<SerializeModel> LoadList()
        {
            using (Stream stream = File.Open(GetFile(), FileMode.Open))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (List<SerializeModel>) bformatter.Deserialize(stream);
            }
        }

        public void ExitAndSave()
        {
            IsOnline = false;
            Save();
        }

        public void Save()
        {
            var users = LoadList();
            for (var i = 0; i < users.Count; i++)
            {
                if (users[i].uID == uID)
                {
                    users[i] = this.GetSerializeObj();
                }
            }
            using (Stream stream = File.Open(GetFile(), FileMode.OpenOrCreate))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                bformatter.Serialize(stream, users);
            }
        }

        private void CreateOrLoadPlayer(ulong id, bool tryLoad = true)
        {
            if (tryLoad)
            {
                var users = LoadList();

                foreach (var user in users)
                {
                    if (user.uID != id) continue;
                    var u = user;
                    if (!user.IsOnline
                    ) //в случае, если запись была сделана, но игшрок не вышел сам(внезапное завершение)
                    {
                        u.LastLoggedIn = DateTime.Now;
                        u.IsOnline = true;
                    }
                    AssignParams(u);
                }
            }
            AssignParams(new SerializeModel
            {
                uID = id,
                IsOnline = true,
                LastLoggedIn = DateTime.Now,
                TotalTime = 0
            });
        }

        private void AssignParams(SerializeModel user)
        {
            IsOnline = user.IsOnline;
            uID = user.uID;
            CurrentLoginTime = user.LastLoggedIn;
            TotalTime = user.TotalTime;
        }

        private SerializeModel GetSerializeObj()
        {
            return new SerializeModel
            {
                uID = uID,
                IsOnline = IsOnline,
                LastLoggedIn = CurrentLoginTime,
                TotalTime = TotalTime
            };
        }
    }
}