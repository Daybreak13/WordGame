using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace WordGame {

    public struct LetterColorEvent {
        public string key;
        public TileType tileType;

        public LetterColorEvent(string k, TileType t) {
            key = k;
            tileType = t;
        }
        static LetterColorEvent e;
        public static void Trigger(string k, TileType t) {
            e.key = k;
            e.tileType = t;
            MMEventManager.TriggerEvent(e);
        }
    }
}