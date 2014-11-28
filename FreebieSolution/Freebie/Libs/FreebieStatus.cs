using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Freebie.Libs
{
    public static class FreebieStatus
    {
        public static string AccountActive()
        {
            return "AC";
        }

        public static string AccountInActive()
        {
            return "IA";
        }

        public static string AccountHardSuspend()
        {
            return "HS";
        }


        public static string AccountActivated()
        {
            return "ACD";
        }

        public static string AccountPending()
        {
            return "AP";
        }

        public static string AccountPTUU()
        {
            return "PTUU";
        }

        public static string AccountPTU()
        {
            return "PTU";

        }
        public static string MobileActive()
        {
            return "AC";
        }

        public static string MobileInActive()
        {
            return "IA";
        }


        public static string MobileDeleted()
        {
            return "DL";
        }
    }
}