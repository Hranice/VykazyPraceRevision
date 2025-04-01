using System.Globalization;
using System.Text;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Helpers
{
    public class FormatHelper
    {
        public static string FormatProjectToString(Project? project)
        {
            if (project == null)
            {
                return "<NULL>";
            }

            if (project?.ProjectType == 1 || project?.ProjectType == 2)
            {
                return $"({project?.ProjectDescription}):{project?.ProjectTitle}";
            }

            else
            {
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

        public static string FormatTimeEntryTypeWithAfterCareToString(TimeEntryType? timeEntryType)
        {
            return $"(AfterCare): {timeEntryType?.Title ?? "<>"}";
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

        public static string FormatDateTimeToMonthAndYear(DateTime dateTime)
        {
            CultureInfo czechCulture = new CultureInfo("cs-CZ");
            return czechCulture.DateTimeFormat.GetMonthName(dateTime.Month).ToUpper() + " " + dateTime.Year;
        }

        public static string FormatUserGroupToString(UserGroup userGroup)
        {
            return $"{userGroup.Title}";
        }

        public static string GetWeekNumberAndRange(DateTime date)
        {
            CultureInfo czechCulture = new CultureInfo("cs-CZ");
            Calendar calendar = czechCulture.Calendar;
            CalendarWeekRule weekRule = czechCulture.DateTimeFormat.CalendarWeekRule;
            DayOfWeek firstDayOfWeek = czechCulture.DateTimeFormat.FirstDayOfWeek;

            int weekNumber = calendar.GetWeekOfYear(date, weekRule, firstDayOfWeek);

            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime startOfWeek = date.AddDays(-diff).Date;

            DateTime endOfWeek = startOfWeek.AddDays(6);

            return $"Týden {weekNumber} ({startOfWeek:dd. M.} – {endOfWeek:dd. M. yyyy})";
        }

    }
}
