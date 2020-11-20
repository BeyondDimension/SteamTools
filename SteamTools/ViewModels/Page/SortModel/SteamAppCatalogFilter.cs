using Livet;
using SteamTool.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamTool.Core.Common;

namespace SteamTools.ViewModels
{
    public abstract class SteamAppCatalogFilter : NotificationObject
    {
        private readonly Action action;

        public abstract bool Predicate(SteamApp app);

        protected SteamAppCatalogFilter(Action updateAction)
        {
            this.action = updateAction;
        }

        protected void Update()
        {
            this.action?.Invoke();
        }
    }

    public class SteamAppNameFilter : SteamAppCatalogFilter
    {
        #region Text 变更通知

        private string _Text;

        public string Text
        {
            get { return this._Text; }
            set
            {
                if (this._Text != value)
                {
                    this._Text = value;
                    this.RaisePropertyChanged();
                    this.Update();
                }
            }
        }

        #endregion

        public SteamAppNameFilter(Action updateAction)
            : base(updateAction)
        {

        }

        public override bool Predicate(SteamApp app)
        {
            if (string.IsNullOrEmpty(Text))
                return true;
            if (!string.IsNullOrEmpty(Text) && (app.Name.IndexOf(this.Text, StringComparison.OrdinalIgnoreCase) > -1 || app.AppId.ToString().IndexOf(this.Text, StringComparison.OrdinalIgnoreCase) > -1))
                return true;
            return false;
        }
    }

    public class SteamAppTypeFilter : SteamAppCatalogFilter
    {
        #region Application 变更通知
        private bool _Application;

        public bool Application
        {
            get { return this._Application; }
            set
            {
                if (this._Application != value)
                {
                    this._Application = value;
                    this.RaisePropertyChanged();
                    this.Update();
                }
            }
        }
        #endregion

        #region Demo 变更通知
        private bool _Demo;

        public bool Demo
        {
            get { return this._Demo; }
            set
            {
                if (this._Demo != value)
                {
                    this._Demo = value;
                    this.RaisePropertyChanged();
                    this.Update();
                }
            }
        }

        #endregion

        #region DLC 变更通知
        private bool _DLC;

        public bool DLC
        {
            get { return this._DLC; }
            set
            {
                if (this._DLC != value)
                {
                    this._DLC = value;
                    this.RaisePropertyChanged();
                    this.Update();
                }
            }
        }

        #endregion

        #region Game 变更通知
        private bool _Game;

        public bool Game
        {
            get { return this._Game; }
            set
            {
                if (this._Game != value)
                {
                    this._Game = value;
                    this.RaisePropertyChanged();
                    this.Update();
                }
            }
        }

        #endregion

        #region Media 变更通知
        private bool _Media;

        public bool Media
        {
            get { return this._Media; }
            set
            {
                if (this._Media != value)
                {
                    this._Media = value;
                    this.RaisePropertyChanged();
                    this.Update();
                }
            }
        }

        #endregion

        #region Music 变更通知
        private bool _Music;

        public bool Music
        {
            get { return this._Music; }
            set
            {
                if (this._Music != value)
                {
                    this._Music = value;
                    this.RaisePropertyChanged();
                    this.Update();
                }
            }
        }

        #endregion

        #region Tool 变更通知
        private bool _Tool;

        public bool Tool
        {
            get { return this._Tool; }
            set
            {
                if (this._Tool != value)
                {
                    this._Tool = value;
                    this.RaisePropertyChanged();
                    this.Update();
                }
            }
        }

        #endregion

        #region Video 变更通知
        private bool _Video;

        public bool Video
        {
            get { return this._Video; }
            set
            {
                if (this._Video != value)
                {
                    this._Video = value;
                    this.RaisePropertyChanged();
                    this.Update();
                }
            }
        }

        #endregion

        #region Guide 变更通知
        private bool _Guide;

        public bool Guide
        {
            get { return this._Guide; }
            set
            {
                if (this._Guide != value)
                {
                    this._Guide = value;
                    this.RaisePropertyChanged();
                    this.Update();
                }
            }
        }

        #endregion

        #region Driver 变更通知
        private bool _Driver;

        public bool Driver
        {
            get { return this._Driver; }
            set
            {
                if (this._Driver != value)
                {
                    this._Driver = value;
                    this.RaisePropertyChanged();
                    this.Update();
                }
            }
        }

        #endregion

        #region HideApp Config 变更通知
        private bool _Config;

        public bool Config
        {
            get { return this._Config; }
            set
            {
                if (this._Config != value)
                {
                    this._Config = value;
                    this.RaisePropertyChanged();
                    this.Update();
                }
            }
        }

        #endregion

        #region Hardware 变更通知
        private bool _Hardware;

        public bool Hardware
        {
            get { return this._Hardware; }
            set
            {
                if (this._Hardware != value)
                {
                    this._Hardware = value;
                    this.RaisePropertyChanged();
                    this.Update();
                }
            }
        }

        #endregion

        #region Unknown 变更通知
        private bool _Unknown;

        public bool Unknown
        {
            get { return this._Unknown; }
            set
            {
                if (this._Unknown != value)
                {
                    this._Unknown = value;
                    this.RaisePropertyChanged();
                    this.Update();
                }
            }
        }

        #endregion

        public SteamAppTypeFilter(Action updateAction)
            : base(updateAction)
        {
            Game = true;
        }

        public override bool Predicate(SteamApp app)
        {
            if (Application && app.Type == SteamAppTypeEnum.Application)
                return true;
            if (Config && app.Type == SteamAppTypeEnum.Config)
                return true;
            if (Demo && app.Type == SteamAppTypeEnum.Demo)
                return true;
            if (DLC && app.Type == SteamAppTypeEnum.DLC)
                return true;
            if (Game && app.Type == SteamAppTypeEnum.Game)
                return true;
            if (Media && app.Type == SteamAppTypeEnum.Media)
                return true;
            if (Music && app.Type == SteamAppTypeEnum.Music)
                return true;
            if (Tool && app.Type == SteamAppTypeEnum.Tool)
                return true;
            if (Video && app.Type == SteamAppTypeEnum.Video)
                return true;
            if (Driver && app.Type == SteamAppTypeEnum.Driver)
                return true;
            if (Guide && app.Type == SteamAppTypeEnum.Guide)
                return true;
            if (Hardware && app.Type == SteamAppTypeEnum.Hardware)
                return true;
            if (Unknown && app.Type == SteamAppTypeEnum.Unknown)
                return true;
            return false;
        }
    }
}
