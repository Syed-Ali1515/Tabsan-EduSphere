namespace Tabsan.EduSphere.Application.Notifications;

public sealed class NotificationEmailOptions
{
    public const string SectionName = "NotificationEmail";

    public bool Enabled { get; set; }

    public string SubjectPrefix { get; set; } = "[Tabsan EduSphere]";

    public string PortalUrl { get; set; } = "/Portal/Notifications";
}
