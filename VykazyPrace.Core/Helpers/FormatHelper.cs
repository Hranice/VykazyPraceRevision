using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Core.Helpers
{
    public class FormatHelper
    {
        public static string FormatProjectToString(Project? project)
        {
            if (project == null)
            {
                return "<NULL>";
            }

            if (project.ProjectType == 1 || project.ProjectType == 2)
            {
                string desc = project.ProjectDescription?.PadLeft(7) ?? "".PadLeft(7);
                return $"{(project.IsArchived == 1 ? "(A) " : "")}{desc}: {project.ProjectTitle}";
            }
            else
            {
                return project.ProjectTitle ?? "";
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

        public static string FormatArrivalDepartureToString(ArrivalDeparture? ad)
        {
            if (ad == null)
            {
                return "<NULL>";
            }

            string arrival = ad.ArrivalTimestamp?.ToString("HH:mm") ?? "--:--";
            string departure = ad.DepartureTimestamp?.ToString("HH:mm") ?? "--:--";
            string reason = string.IsNullOrWhiteSpace(ad.DepartureReason) ? "" : $" ({ad.DepartureReason})";
            string user = ad.User != null ? FormatUserToString(ad.User) : $"UserId={ad.UserId}";

            return $"{ad.WorkDate:dd.MM.yyyy} | {user} | {arrival} - {departure}{reason} | " +
                   $"Hodiny: {ad.HoursWorked:0.##}, Přesčasy: {ad.HoursOvertime:0.##}";
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

        public static bool IsPreProject(string projectDescription)
        {
            return !Regex.IsMatch(projectDescription, @"^\d{4}[A-Z]\d{2}$");
        }

        public static int GetMonthNumberFromString(string month)
        {
            var monthNumber = month switch
            {
                "Leden" => 1,
                "Únor" => 2,
                "Březen" => 3,
                "Duben" => 4,
                "Květen" => 5,
                "Červen" => 6,
                "Červenec" => 7,
                "Srpen" => 8,
                "Září" => 9,
                "Říjen" => 10,
                "Listopad" => 11,
                "Prosinec" => 12,
                _ => throw new ArgumentException("Neplatný měsíc: " + month)
            };

            return monthNumber;
        }
    }
}
