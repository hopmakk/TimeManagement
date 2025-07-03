namespace TimeManagement.Models
{
    public class NotificationData
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string TypeStr { get; set; }
        public string CreateTime { get; set; }
        public NotificationType Type { get; set; }
        public static NotificationData Empty { get; } = new NotificationData(NotificationType.None, "");

        public NotificationData(NotificationType type, string title)
        {
            Type = type;
            Title = title;
            CreateTime = DateTime.Now.TimeOfDay.ToString().Substring(0, 5);

			switch (Type)
            {
                case NotificationType.Success:
                    TypeStr = "✓  Успешно";
                    break;
                case NotificationType.Hint:
                    TypeStr = "○  Совет";
                    break;
                case NotificationType.Warning:
                    TypeStr = "△  Внимание";
                    break;
                case NotificationType.Error:
                    TypeStr = " !   Ошибка";
                    break;
                case NotificationType.None:
                    TypeStr = "?  Неизвестно";
                    break;
            }
        }

    }


    public enum NotificationType
    {
        Success,
        Hint,
        Warning,
        Error,
        None
    }
}
