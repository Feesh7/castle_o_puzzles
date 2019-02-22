﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MLAPI;

[AddComponentMenu("IOEntities/IOLock")]
[RequireComponent(typeof(Collider))]
public class IOLock : IOEntity {
    public static string IOLOCK_CHANNEL = "MLAPI_INTERNAL";
    public DigitalState Locked;
    public bool takes_key = true;

    protected override void Startup() {
        // Set up output state callbacks for clients
        Locked.OnReceiveNetworkValue = SetLockedState;
        if (!isServer) return;
        
        // Set up the initial output state on the server
        SetLockedState(true);
    }

    public void SetLockedState (DigitalState input) {
        SetLockedState(input.state);
    }

    public void SetLockedState(bool state) {
        Locked.state = state;
    }

    #region Lock RPCs
    private bool HandleLockStateChangeRequest(uint clientID, bool lock_state) {
        (NetworkSharedItem item, int count) = InventoryManager.networkInv.GetFirstOwnedItem((int) clientID);
        if (item.name == "GoldKey" && count > 0) {
            if (takes_key) {
                InventoryManager.networkInv.RevokeItem(item.name, (int) clientID);
                InventoryManager.networkInv.RemoveItem(item.name);
            }
            Locked.state = lock_state;
            return true;
        }
        return false;
    }
    [ServerRPC]
    private void RPC_RequestLockStateChange(bool lock_state) {
        if (HandleLockStateChangeRequest(ExecutingRpcSender,lock_state))
            InvokeClientRpcOnClient(RPC_LockStateChangeFinish, ExecutingRpcSender, lock_state);
    }
    [ClientRPC]
    private void RPC_LockStateChangeFinish(bool locked) {
        if (takes_key && !locked) {
            PlayerDataList.Instance.get<InventoryManager>().actionSlots.ChangeSharedItem(null);
        }
    }
    public void RequestLockStateChange(bool lock_state) {
        if (!isServer) {
            InvokeServerRpc(RPC_RequestLockStateChange, lock_state, channel: IOLOCK_CHANNEL);
        }
        else {  
            uint id = NetworkingManager.singleton.LocalClientId;
            if (HandleLockStateChangeRequest(id, lock_state))
                RPC_LockStateChangeFinish(lock_state);
        }
    }
    #endregion
}