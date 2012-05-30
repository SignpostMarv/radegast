﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

using OpenMetaverse;
using OpenMetaverse.StructuredData;namespace Radegast.Bot{    public class AutoSitPreferences    {        public UUID Primitive { get; set; }        public string PrimitiveName { get; set; }        public bool Enabled { get; set; }        public static explicit operator AutoSitPreferences(OSD osd){            AutoSitPreferences prefs = new AutoSitPreferences            {                Primitive = UUID.Zero,                PrimitiveName = ""            };            if (osd != null && osd.Type == OSDType.Map)            {                OSDMap map = (OSDMap)osd;                prefs.Primitive = map.ContainsKey("Primitive") ? map["Primitive"].AsUUID() : UUID.Zero;                prefs.PrimitiveName = prefs.Primitive != UUID.Zero && map.ContainsKey("PrimitiveName") ? map["PrimitiveName"].AsString() : "";                prefs.Enabled = map.ContainsKey("Enabled") ? map["Enabled"].AsBoolean() : false;            }            return prefs;        }        public static implicit operator OSD(AutoSitPreferences prefs){            return (OSDMap)prefs;        }        public static explicit operator OSDMap(AutoSitPreferences prefs)        {            OSDMap map = new OSDMap(3);            map["Primitive"] = prefs.Primitive;            map["PrimitiveName"] = prefs.PrimitiveName;            map["Enabled"] = prefs.Enabled;            return map;        }        public static explicit operator AutoSitPreferences(Settings s){            return (s != null && s.ContainsKey("AutoSit")) ? (AutoSitPreferences)s["AutoSit"] : new AutoSitPreferences();        }    }    public class AutoSit    {        private const string c_label = "Use as Auto-Sit target";        private RadegastInstance m_instance;        private Timer m_Timer;        public AutoSit(RadegastInstance instance)        {            m_instance = instance;            m_Timer = new Timer(60000);            m_Timer.Elapsed += new ElapsedEventHandler((sender, args) => {                TrySit();            });            m_Timer.Enabled = false;        }        public AutoSitPreferences Preferences        {            get { return !m_instance.Client.Network.Connected ? null : (AutoSitPreferences)m_instance.ClientSettings; }            set {                m_instance.ClientSettings["AutoSit"] = value;                if (Preferences.Enabled)                {                    m_instance.ContextActionManager.RegisterContextAction(typeof(Primitive), c_label, PrimitiveContextAction);                }                else                {                    m_instance.ContextActionManager.DeregisterContextAction(typeof(Primitive), c_label);                }            }        }        public void PrimitiveContextAction(object sender, EventArgs e)        {            Primitive prim = (Primitive)sender;            Preferences = new AutoSitPreferences            {                Primitive = prim.ID,                PrimitiveName = prim.Properties != null ? prim.Properties.Name : "",                Enabled = Preferences.Enabled            };            if (prim.Properties == null)            {                m_instance.Client.Objects.ObjectProperties += Objects_ObjectProperties;                m_instance.Client.Objects.ObjectPropertiesUpdated += Objects_ObjectProperties;            }        }        public void Objects_ObjectProperties(object sender, ObjectPropertiesEventArgs e)        {            if (e.Properties.ObjectID == Preferences.Primitive)            {                Preferences = new AutoSitPreferences                {                    Primitive = Preferences.Primitive,                    PrimitiveName = e.Properties.Name,                    Enabled = Preferences.Enabled                };                m_instance.Client.Objects.ObjectProperties -= new EventHandler<ObjectPropertiesEventArgs>(Objects_ObjectProperties);            }        }        public void TrySit()        {            if (Preferences != null && m_instance.Client.Network.Connected)            {                if (Preferences.Enabled && Preferences.Primitive != UUID.Zero)                {                    m_instance.State.SetSitting(true, Preferences.Primitive);                    m_Timer.Enabled = true;                }                else                {                    m_Timer.Enabled = false;                }            }            else            {                m_Timer.Enabled = false; // being lazy here, just letting timer elapse rather than disabling on client disconnect            }        }    }}