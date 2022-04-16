using System.Collections.Generic;

namespace System.Application.Models
{
    partial class NoticeDTO : NotificationBuilder.IInterface, NotificationBuilder.ClickAction.IInterface
    {
        string NotificationBuilder.IInterface.Title => Title;

        string NotificationBuilder.IInterface.Content => Introduction;

        NotificationType NotificationBuilder.IInterface.Type => Type;

        bool NotificationBuilder.IInterface.AutoCancel => NotificationBuilder.DefaultAutoCancel;

        NotificationBuilder.ClickAction.IInterface? NotificationBuilder.IInterface.Click => this;

        IReadOnlyList<NotificationBuilder.ButtonAction.IInterface>? NotificationBuilder.IInterface.Buttons => null;

        string? NotificationBuilder.IInterface.ImageUri => Picture;

        NotificationBuilder.EImageDisplayType NotificationBuilder.IInterface.ImageDisplayType => default;

        Entrance NotificationBuilder.ClickAction.IInterface.Entrance => IsOpenBrowser ? Entrance.Browser : Entrance.Main;

        string? NotificationBuilder.ClickAction.IInterface.RequestUri =>Url;

        Action? NotificationBuilder.ClickAction.IInterface.Action => null;
    }
}
