using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Freebie.Models;

namespace Freebie.Libs
{
    public static class QuotaCalculation
    {
        //private static EchoContext db = new EchoContext();
        public static Quota Calculate(Account ac, string selected_interests)
        {
            bool step1 = false;
            bool step2 = false;
            bool step3 = false;
            var db = new EchoContext();
            if (string.IsNullOrWhiteSpace(selected_interests))
            {
                selected_interests = "";
            }
            IEnumerable<Quota>  base_quotas = db.Quotas.Where(x => x.Quota_Type_Cd.Equals("B")).OrderBy(x => x.Quota_Cd);

            if (!string.IsNullOrEmpty(ac.First_Name)) {
                if (!string.IsNullOrEmpty(ac.Last_Name)) {
                    if (ac.Day_Of_Birth != null) {
                        if (ac.Month_Of_Birth != null) {
                            if (ac.Year_Of_Birth != null) {
                                if (!string.IsNullOrEmpty(ac.Gender_Cd)) {
                                    step1 = true;
                                }
                            }
                        }
                    }
                }
            }

            if (step1) {
                if (!string.IsNullOrWhiteSpace(ac.Income_Range_Cd)) {
                    if (!string.IsNullOrWhiteSpace(ac.Occupation_Cd)) {
                        if (!string.IsNullOrWhiteSpace(ac.Education_Cd)) {
                            if (!string.IsNullOrWhiteSpace(ac.Marital_Status_Cd))
                            {
                                if (ac.Children_Flag != null)
                                {
                                    if (!string.IsNullOrWhiteSpace(ac.Children_Flag))
                                    {
                                         step2 = true;
                                    }
                                }
                               
                            }
                        }
                    }
                }
            }

            if (step2) { 
               
                       // if (ac.Year_Of_Birth_Child1 != null || ac.Year_Of_Birth_Child2 != null || ac.Year_Of_Birth_Child3 != null) {
                            if (!string.IsNullOrEmpty(ac.Identification_Number)) {
                                string[] interest_arrs = selected_interests.Split(',');
                                if (interest_arrs.Length > 0) {
                                    if (interest_arrs[0] != "")
                                    {
                                        step3 = true;
                                    }
                                }
                            }
                       // }
 
            }

            if (step3 && step2 && step1)
            {
                return base_quotas.ElementAt(2);
            }
            else {
                if (step2 && step1)
                {
                    return base_quotas.ElementAt(1);
                }
                else {
                    if (step1) {
                        return base_quotas.ElementAt(0);
                    }
                }
            }
            return new Quota();
        }

        private static List<string> load_interest(AccountInterest aci)
        {
            List<string> arrs = new List<string>();

            if (aci.I01_Food_Dining) { arrs.Add("I01"); }
            if (aci.I02_Night_Life) { arrs.Add("I02"); }
            if (aci.I03_Entertainment) { arrs.Add("I03"); }
            if (aci.I04_Music_Movie) { arrs.Add("I04"); }
            if (aci.I05_Sports_Fitness) { arrs.Add("I05"); }
            if (aci.I06_Shopping_Fashion) { arrs.Add("I06"); }
            if (aci.I07_Health_Beauty) { arrs.Add("I07"); }
            if (aci.I08_Travel) { arrs.Add("I08"); }
            if (aci.I09_Pets) { arrs.Add("I09"); }
            if (aci.I10_Kids_Children) { arrs.Add("I10"); }
            if (aci.I11_Home_Living) { arrs.Add("I11"); }
            if (aci.I12_Finance_Investment) { arrs.Add("I12"); }
            if (aci.I13_Technology_Gadget) { arrs.Add("I13"); }
            if (aci.I14_Auto) { arrs.Add("I14"); }

            return arrs;
        }  
    }
}