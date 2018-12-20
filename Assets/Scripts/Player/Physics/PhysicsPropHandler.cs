﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using MLAPI;

public class PhysicsPropHandler : NetworkedBehaviour {
    private Dictionary<Type, PhysicsPlugin> plugins;

    private void Awake() {
        plugins = new Dictionary<Type, PhysicsPlugin>() {
            {typeof(Pushable), new Pusher(context: this)},
            {typeof(Grabable), new Grabber(context: this)}
        };

        foreach (PhysicsPlugin plugin in plugins.Values) {
            plugin.Awake();
        }
    }

    public T GetPlugin<T>() where T : PhysicsPlugin {
        return plugins.Values.Where(x => x is T).FirstOrDefault() as T;
    }

    public PhysicsPlugin GetPluginByProp<T>() where T : PhysicsProp {
        PhysicsPlugin plugin;
        plugins.TryGetValue(typeof(T), out plugin);
        return plugin;
    }

    private void Start() {
        foreach (PhysicsPlugin plugin in plugins.Values) {
            plugin.Start();
        }
    }

    public override void NetworkStart() {
        foreach (PhysicsPlugin plugin in plugins.Values) {
            plugin.NetworkStart();
        }
    }

    private void Update() {
        foreach (PhysicsPlugin plugin in plugins.Values) {
            if (plugin.enabled) plugin.Update();
        }
    }

    private void FixedUpdate() {
        foreach (PhysicsPlugin plugin in plugins.Values) {
            if (plugin.enabled) plugin.FixedUpdate();
        }
    }

    public override void OnDisabled() {
        foreach (PhysicsPlugin plugin in plugins.Values) {
            plugin.OnDisable();
        }
    }

    public override void OnDestroyed() {
        foreach (PhysicsPlugin plugin in plugins.Values) {
            plugin.OnDisable();
            plugin.OnDestroy();
        }
    }

    private void OnTriggerEnter(Collider other) {
        PhysicsProp[] props = other.GetComponents<PhysicsProp>();
        foreach (PhysicsProp prop in props) {
            PhysicsPlugin plugin = plugins[prop.GetType()];
            if (plugin.enabled) plugin.OnTriggerEnter(other, prop);
        }
    }

    private void OnTriggerStay(Collider other) {
        PhysicsProp[] props = other.GetComponents<PhysicsProp>();
        foreach (PhysicsProp prop in props) {
            PhysicsPlugin plugin = plugins[prop.GetType()];
            if (plugin.enabled) plugin.OnTriggerStay(other, prop);
        }
    }

    private void OnTriggerExit(Collider other) {
        PhysicsProp[] props = other.GetComponents<PhysicsProp>();
        foreach (PhysicsProp prop in props) {
            PhysicsPlugin plugin = plugins[prop.GetType()];
            if (plugin.enabled) plugin.OnTriggerExit(other, prop);
        }
    }

    private void OnCollisionEnter(Collision other) {
        PhysicsProp[] props = other.gameObject.GetComponents<PhysicsProp>();
        foreach (PhysicsProp prop in props) {
            PhysicsPlugin plugin = plugins[prop.GetType()];
            if (plugin.enabled) plugin.OnCollisionEnter(other, prop);
        }
    }

    private void OnCollisionStay(Collision other) {
        PhysicsProp[] props = other.gameObject.GetComponents<PhysicsProp>();
        foreach (PhysicsProp prop in props) {
            PhysicsPlugin plugin = plugins[prop.GetType()];
            if (plugin.enabled) plugin.OnCollisionStay(other, prop);
        }
    }

    private void OnCollisionExit(Collision other) {
        PhysicsProp[] props = other.gameObject.GetComponents<PhysicsProp>();
        foreach (PhysicsProp prop in props) {
            PhysicsPlugin plugin = plugins[prop.GetType()];
            if (plugin.enabled) plugin.OnCollisionExit(other, prop);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) {
        PhysicsProp[] props = hit.gameObject.GetComponents<PhysicsProp>();
        foreach (PhysicsProp prop in props) {
            PhysicsPlugin plugin = plugins[prop.GetType()];
            if (plugin.enabled) plugin.OnControllerColliderHit(hit, prop);
        }
    }

    public void HandleUse(GameObject other) {
        PhysicsProp[] props = other.GetComponents<PhysicsProp>();
        foreach (PhysicsProp prop in props) {
            PhysicsPlugin plugin = plugins[prop.GetType()];
            if (plugin.enabled) plugin.OnUse(prop);
        }
    }

    public bool IsServer() {
        return isServer;
    }
}
