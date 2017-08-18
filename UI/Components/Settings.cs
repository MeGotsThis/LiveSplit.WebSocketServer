﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public partial class Settings : UserControl
    {
        public bool AutoStart { get; set; }

        public ushort Port { get; set; }

        public string LocalIP { get; set; }

        public string GetIP()
        {
            IPAddress[] ipv4Addresses = Array.FindAll(
                Dns.GetHostEntry(string.Empty).AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);
            return ipv4Addresses[0].ToString();
        }

        public string PortString
        {
            get { return Port.ToString(); }
            set { Port = ushort.Parse(value); }
        }

        public Settings()
        {
            InitializeComponent();
            AutoStart = false;
            Port = 15721;
            LocalIP = GetIP();
            label3.Text = LocalIP;

            chkAutoStart.DataBindings.Add("Checked", this, "AutoStart", false, DataSourceUpdateMode.OnPropertyChanged);
            txtPort.DataBindings.Add("Text", this, "PortString", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            CreateSettingsNode(document, parent);
            return parent;
        }

        public int GetSettingsHashCode()
        {
            return CreateSettingsNode(null, null);
        }

        private int CreateSettingsNode(XmlDocument document, XmlElement parent)
        {
            return SettingsHelper.CreateSetting(document, parent, "AutoStart", AutoStart) ^
                SettingsHelper.CreateSetting(document, parent, "Port", PortString);
        }

        public void SetSettings(XmlNode settings)
        {
            AutoStart = SettingsHelper.ParseBool(settings["AutoStart"], false);
            PortString = SettingsHelper.ParseString(settings["Port"]);
        }
    }
}
