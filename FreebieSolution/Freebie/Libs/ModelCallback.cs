using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Freebie.Models;
using System.Data;

namespace Freebie.Libs
{
    public static class ModelCallback
    {
        //private static EchoContext db = new EchoContext();

        public static void AfterCreateAccount(Account ac, string[] interests)
        {
            using (var db = new EchoContext())
            {
                ac.Created_By = ac.Account_No;
                ac.Updated_By = ac.Account_No;
                ac.Created_Dttm = DateTime.Now;
                ac.Updated_Dttm = DateTime.Now;

                //db.Entry(ac).State = EntityState.Modified;

                AccountMobile am = new AccountMobile();
                am.Mobile_Number = ac.First_Mobile_Number;
                am.Account_Id = ac.Account_Id;
                am.Status_Cd = FreebieStatus.MobileActive();
                am.Primary_Flag = true;
                am.Created_By = ac.Account_No;
                am.Updated_By = ac.Account_No;
                db.AccountMobiles.Add(am);

                AccountInterest aci = new AccountInterest();
                aci.Account_Id = ac.Account_Id;
                aci.I01_Food_Dining = interests.Contains("I01");
                aci.I02_Night_Life = interests.Contains("I02");
                aci.I03_Entertainment = interests.Contains("I03");
                aci.I04_Music_Movie = interests.Contains("I04");
                aci.I05_Sports_Fitness = interests.Contains("I05");
                aci.I06_Shopping_Fashion = interests.Contains("I06");
                aci.I07_Health_Beauty = interests.Contains("I07");
                aci.I08_Travel = interests.Contains("I08");
                aci.I09_Pets = interests.Contains("I09");
                aci.I10_Kids_Children = interests.Contains("I10");
                aci.I11_Home_Living = interests.Contains("I11");
                aci.I12_Finance_Investment = interests.Contains("I12");
                aci.I13_Technology_Gadget = interests.Contains("I13");
                aci.I14_Auto = interests.Contains("I14");
                aci.Created_By = ac.Account_No;
                aci.Updated_By = ac.Account_No;
                db.AccountInterests.Add(aci);

                db.SaveChanges();
            }
            
        }
    }
}