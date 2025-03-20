using System.Globalization;
using System.Text;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Helpers
{
    public class FormatHelper
    {
        public static string FormatProjectToString(Project? project)
        {
            if(project == null)
            {
                return "<NULL>";
            }

            switch (project?.ProjectType)
            {
                case 0:
                    return $"{project?.ProjectTitle}";
                case 1:
                    return $"{(project.IsArchived == 1 ? "(ARCHIV): " : "")}{project?.ProjectDescription}: {project?.ProjectTitle}";
                case 2:
                    return $"(PŘEDPROJEKT): {project?.ProjectTitle}";
                default:
                    return $"{project?.ProjectTitle}";
            }
        }

        public static string FormatTimeEntryToString(TimeEntry? timeEntry)
        {
            return $"{timeEntry?.Project?.ProjectDescription} - {timeEntry?.Project?.ProjectTitle}: {timeEntry?.EntryMinutes / 60.0} h - {timeEntry?.Description}";
        }

        public static string FormatTimeEntryTypeToString(TimeEntryType? timeEntryType)
        {
            return $"{timeEntryType?.Title ?? "<>"}";
        }

        public static string FormatTimeEntrySubTypeToString(TimeEntrySubType? timeEntrySubType)
        {
            return $"{timeEntrySubType?.Title ?? "<>"}";
        }

        public static string FormatUserToString(User? user)
        {
            return $"({user?.PersonalNumber}): {user?.FirstName} {user?.Surname} - {user?.UserGroup?.Title}";
        }

        public static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string normalized = text.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
