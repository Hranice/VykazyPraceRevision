using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VykazyPrace.Core.Database.Models.OutlookEvents
{
    /// <summary>
    /// Stav položky z pohledu konkrétního uživatele.
    /// 0 = Default/Nezpracováno
    /// 1 = Written (uživatel chce vidět/pracovat)
    /// 2 = Hidden (dočasně neukazovat)
    /// 3 = Ignore/Tombstone (trvale neukazovat v seznamu)
    /// 4 = Archive (volitelné)
    /// </summary>
    public enum UserItemStateEnum
    {
        Default = 0,
        Written = 1,
        Hidden = 2,
        IgnoreTombstone = 3,
        Archive = 4
    }

}
