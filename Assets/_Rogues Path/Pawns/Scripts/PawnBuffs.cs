using System.Collections.Generic;
using _Rogues_Path.Buffs.Scripts;
using _Rogues_Path.UI;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using UnityEngine;

namespace _Rogues_Path.Pawns {
    public partial class Pawn {
        public struct PawnBuffNames {
            public const string Block = "Block";
            public const string Poison = "Poison";
            public const string Regeneration = "Regeneration";
        }

        public static PawnBuffNames BuffNames = new();
        [SerializeField] private BuffsDictionary buffs = new();

        public void AddBuff(PawnBuff status, int count) {
            if (buffs.ContainsKey(status.Name)) {
                buffs[status.Name] += count;

                GetComponentInChildren<UIStatusDisplay>().AddBuff(status, buffs[status.Name]);

            }
            else {

                GetComponentInChildren<UIStatusDisplay>().AddBuff(status, count);

                buffs.Add(status.Name, count);
            }

            EventBus.Raise(
                new StatusChangedEvent {
                    Targets = new List<Pawn>() {
                        this
                    },
                    NewStatus = status,
                    Count = count
                });
        }

        public Dictionary<string, int> GetBuffs() => new Dictionary<string, int>(buffs);

        public bool TryRemoveBuff(PawnBuff status, int count) {
            if (buffs.ContainsKey(status.Name)) {
                if (buffs[status.Name] <= count) {
                    GetComponentInChildren<UIStatusDisplay>().RemoveBuff(status, buffs[status.Name]);

                    buffs.Remove(status.Name);
                    status.OnBuffRemoved();
                    Destroy(status.gameObject);
                }
                else {
                    buffs[status.Name] -= count;

                    if (buffs[status.Name] == 0) {
                        buffs.Remove(status.Name);
                        status.OnBuffRemoved();
                        Destroy(status.gameObject);
                    }
                }

                EventBus.Raise(
                    new StatusChangedEvent {
                        Targets = new List<Pawn>() {
                            this
                        },
                        NewStatus = status
                    });

                return true;
            }
            else {
                return false;
            }
        }

        public bool TryGetBuffCount(PawnBuff status, out int count) {
            if (buffs.ContainsKey(status.Name)) {
                count = buffs[status.Name];

                if (count == 0) {
                    return false;
                }
                else {
                    return true;
                }
            }
            else {
                count = -1;
                return false;
            }
        }

        private bool TryRemoveBuff(string status, int count) {
            if (buffs.ContainsKey(status)) {
                buffs[status] -= count;

                return true;
            }
            else {
                count = -1;
                return false;
            }
        }
    }
}