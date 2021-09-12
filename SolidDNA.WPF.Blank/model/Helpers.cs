using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HormesaFILEIDS.model
{
    class Helpers
    {
        /// <summary>
        /// Formatea un partid pasandolo de numero al formato 000-000
        /// </summary>
        /// <param name="partId"></param>
        /// <returns></returns>
        public static string formatPartId(string partId)
        {
            if (!string.IsNullOrEmpty(partId))
            {
                return partId.ToString().PadLeft(6, '0').Insert(3, "-");
            }
            return string.Empty;
        }
    }
}
