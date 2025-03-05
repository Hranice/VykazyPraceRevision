using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Helpers
{
    public class FormatHelper
    {
        public static string FormatProjectToString(Project? project)
        {
            return $"{project?.ProjectDescription} - {project?.ProjectTitle}";
        }

        public static string FormatTimeEntryToString(TimeEntry? timeEntry)
        {
            return $"{timeEntry?.Project?.ProjectDescription} - {timeEntry?.Project?.ProjectTitle}: {timeEntry?.EntryMinutes / 60.0} h - {timeEntry?.Description}";
        }

        public static string FormatTimeEntryTypeToString(TimeEntryType? timeEntryType)
        {
            return $"{timeEntryType?.Title ?? "<>"}";
        }

        public static string FormatUserToString(User? user)
        {
            return $"{user?.Id ?? 0} ({user?.PersonalNumber}): {user?.FirstName} {user?.Surname}";
        }
    }
}
