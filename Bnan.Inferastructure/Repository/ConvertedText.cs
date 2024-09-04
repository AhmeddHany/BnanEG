using Bnan.Core.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bnan.Inferastructure.Repository
{
    public class ConvertedText : IConvertedText
    {
        public IUnitOfWork _unitOfWork;

        public ConvertedText(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public string Get_NoText(string Code_Number, string LangType)
        {
            if (Code_Number.Length > 3)
            {
                var code = Code_Number.Substring(0, 2);
                var Rank = Code_Number.Substring(3, 1);
                var Exist = _unitOfWork.CrMasSysConvertNoToText.FindAll(x => x.CrMasSysConvertNoToTextCode == code && x.CrMasSysConvertNoToTextType == Rank).FirstOrDefault();
                if (Exist != null)
                {
                    if (LangType == "A")
                    {
                        return Exist.CrMasSysConvertNoToTextArName;
                    }
                    else
                    {
                        return Exist.CrMasSysConvertNoToTextEnName;
                    }
                }
                return "";
            }
            return "";
        }

        public string Get_Currency(string Code_Number, string LangType)
        {
            if (Code_Number.Length > 3)
            {
                var code = Code_Number.Substring(0, 2);
                var Rank = Code_Number.Substring(3, 1);
                var Exist = _unitOfWork.CrMasSysConvertNoToText.FindAll(x => x.CrMasSysConvertNoToTextCode == code && x.CrMasSysConvertNoToTextType == Rank).FirstOrDefault();
                if (Exist != null)
                {
                    if (LangType == "A")
                    {
                        return Exist.CrMasSysConvertNoToTextArName;
                    }
                    else
                    {
                        return Exist.CrMasSysConvertNoToTextEnName;
                    }
                }
                return "";
            }
            return "";
        }

        public (string, string) ConvertNumber(string Our_No, string LangType)
        {
            var Fraction_Ar_Text = "";
            var Hundred_Ar_Text = "";
            var Thousand_Ar_Text = "";
            var Million_Ar_Text = "";

            var Fraction_En_Text = "";
            var Hundred_En_Text = "";
            var Thousand_En_Text = "";
            var Million_En_Text = "";

            var Fraction_Number = "";
            var Hundred_Number = "";
            var Thousand_Number = "";
            var Million_Number = "";

            var MyNumber = "";
            var My_Language = "";

            var Third_Ar_Text = "";
            var Second_Ar_Text = "";
            var First_Ar_Text = "";

            var Third_En_Text = "";
            var Second_En_Text = "";
            var First_En_Text = "";

            var Arbic_Currency = "";
            var English_Currency = "";

            var Category_Ar_Text = "";
            var Category_En_Text = "";

            var Code_Number = "";
            var Code_Currency = "";
            var Code_Category = "";

            var Merging_Ar_1 = "";
            var Merging_En_1 = "";

            var Merging_Ar_2 = "";
            var Merging_En_2 = "";

            var Merging_Ar_Category = "";
            var Merging_En_Category = "";

            var Merging_Ar_Currency = "";
            var Merging_En_Currency = "";

            var Link_Ar_No = "";
            var Link_En_No = "";

            double fracF = 0.0;
            double fracS = 0.0;
            //متغيرات الملايين
            double MF = 0.0;
            double MS = 0.0;
            double MT = 0.0;
            string MST = "";
            //متغيرات الاالف
            double TF = 0.0;
            double TS = 0.0;
            double TT = 0.0;
            string TST = "";
            //متغيرات المئات
            double HF = 0.0;
            double HS = 0.0;
            double HT = 0.0;
            string HST = "";

            var integerNumber = 0.0;
            My_Language = LangType;
            ////////////////////////// التحقق من وجود رقم عشري //////////////////////////////////////////////
            Our_No = Our_No.Replace(",", "");
            int firstspace = Our_No.IndexOf(".");
            if (firstspace < 0) // لايوجد رقم عشري
            {
                Fraction_Number = "00";
            }
            else if (firstspace >= 0) // يوجد رقم عشري
            {
                Fraction_Number = Our_No.Substring(firstspace + 1, Our_No.Length - firstspace - 1);

                if (Fraction_Number.Length > 1)
                {
                    Fraction_Number = Fraction_Number.Substring(0, 2);
                }
                else
                {
                    Fraction_Number = Fraction_Number + "0";
                }

                ///////////////////////////////////   نهاية التحقق من الرقم العشري    ////////////////////////////////////
                fracF = Convert.ToDouble(Fraction_Number.Substring(0, 1));
                fracS = Convert.ToDouble(Fraction_Number.Substring(1, 1));

                var Removed = Our_No.Substring(firstspace, Our_No.Length - firstspace);
                Our_No = Our_No.Replace(Removed, "");

                if (fracF == 0 && fracS == 1)  // واحد هللة
                {
                    Code_Currency = "27,0";   // هللة
                }
                else if (fracF == 0 && fracS == 2)  // هللتان
                {
                    Code_Currency = "28,0";   // هللتان
                }
                else if (fracF == 0 && fracS > 2)  // ارقام احاد فردية من (3)  (9)
                {
                    Code_Number = "1" + fracS + "," + "2"; // ثلاث : تسع
                    Code_Currency = "29,0";   // هللات

                    First_Ar_Text = Get_NoText(Code_Number, "A");
                    First_En_Text = Get_NoText(Code_Number, "E");
                }
                else if (fracF > 0 && fracS == 0)  // ارقام عقود في الكسر
                {
                    Code_Number = "1" + fracF + "," + "4";// عشرة | عشرون | تسعون
                    Code_Currency = "27,0";   // هللة

                    if (fracF == 1)
                    {
                        Code_Currency = "29,0";  // هللات
                    }
                    Second_Ar_Text = Get_NoText(Code_Number, "A");
                    Second_En_Text = Get_NoText(Code_Number, "E");

                }
                else if (fracF == 1 && fracS > 0)  // ارقام من  (11)  (19)
                {

                    Code_Number = "1" + fracS + "," + "5";//  تسعة عشرة :احدى عشرة
                    Code_Currency = "27,0";   // هللة

                    First_Ar_Text = Get_NoText(Code_Number, "A");
                    First_En_Text = Get_NoText(Code_Number, "E");

                }
                else if (fracF >= 2 && fracS > 0)  // الارقام الكسور من (21)  (99)
                {
                    Code_Currency = "27,0";   // هللة
                    Code_Number = "1" + fracS + "," + "2";// ثلاث : تسع

                    First_Ar_Text = Get_NoText(Code_Number, "A");
                    Second_En_Text = Get_NoText(Code_Number, "E");

                    Code_Number = "1" + fracF + "," + "4";// عشرون : تسعون

                    Second_Ar_Text = Get_NoText(Code_Number, "A");
                    First_En_Text = Get_NoText(Code_Number, "E");

                    Merging_Ar_1 = " و ";
                    Merging_En_1 = "-";

                }

                Merging_Ar_Currency = " ";
                Merging_En_Currency = " ";

                Arbic_Currency = Get_Currency(Code_Currency, "A");
                English_Currency = Get_Currency(Code_Currency, "E");

                Fraction_Ar_Text = First_Ar_Text + Merging_Ar_1 + Second_Ar_Text + Merging_Ar_2 + Merging_Ar_Currency + Arbic_Currency;
                Fraction_En_Text = First_En_Text + Merging_En_1 + Second_En_Text + Merging_En_2 + Merging_En_Currency + English_Currency;

                First_Ar_Text = "";
                Merging_Ar_1 = "";
                Merging_Ar_2 = "";
                Second_Ar_Text = "";

                First_En_Text = "";
                Merging_En_1 = "";
                Merging_En_2 = "";
                Second_En_Text = "";

                Arbic_Currency = "";
                English_Currency = "";
                Code_Currency = "";

                Merging_Ar_Currency = "";
                Merging_En_Currency = "";
            }

            MyNumber = Convert.ToDouble(Our_No).ToString("000000000");
            integerNumber = Convert.ToDouble(MyNumber);

            if (integerNumber > 999999999)
            {
                return ("الرقم كبير", "Big Number");

            }

            Million_Number = MyNumber.Substring(0, 3);
            Thousand_Number = MyNumber.Substring(3, 3);
            Hundred_Number = MyNumber.Substring(6, 3);

            // هناك الاف 
            TF = Convert.ToDouble(Thousand_Number.Substring(0, 1));
            TS = Convert.ToDouble(Thousand_Number.Substring(1, 1));
            TT = Convert.ToDouble(Thousand_Number.Substring(2, 1));
            TST = Thousand_Number.Substring(1, 2);

            //هناك المئات
            HF = Convert.ToDouble(Hundred_Number.Substring(0, 1));
            HS = Convert.ToDouble(Hundred_Number.Substring(1, 1));
            HT = Convert.ToDouble(Hundred_Number.Substring(2, 1));
            HST = Hundred_Number.Substring(1, 2);


            // ***********************   Start Million Number

            if (Million_Number != "000") //هناك ملايين
            {

                // ---------------------------------------------   Start Hundreds Of Millions    --------------------------------------------------

                MF = Convert.ToDouble(Million_Number.Substring(0, 1));
                MS = Convert.ToDouble(Million_Number.Substring(1, 1));
                MT = Convert.ToDouble(Million_Number.Substring(2, 1));
                MST = Million_Number.Substring(1, 2);

                if (MF > 0) //  هل يوجد مئات ملايين
                {
                    if (MF == 1) // هل يوجد مئة مليون
                    {
                        Code_Currency = "30,0";   // ريال
                        Code_Category = "19,0";  // مليون
                        Code_Number = "11,0"; // مئة

                        Third_Ar_Text = Get_NoText(Code_Number, "A");
                        Third_En_Text = Get_NoText(Code_Number, "E");
                    }
                    else if (MF == 2)  // هل يوجد مئتا مليون
                    {

                        Code_Currency = "30,0";   // ريال
                        Code_Category = "19,0";  // مليون
                        Code_Number = "14,0";  // مئتا

                        Third_Ar_Text = Get_NoText(Code_Number, "A");
                        Third_En_Text = Get_NoText(Code_Number, "E");
                    }
                    else if (MF > 2) // ارقام الملايين اكبر من 2
                    {
                        Code_Currency = "30,0";   // ريال
                        Code_Category = "19,0";  // مليون
                        Code_Number = "1" + MF + "," + "2";   // ثلاثة ¡ تسعة .......

                        First_Ar_Text = Get_NoText(Code_Number, "A");
                        First_En_Text = Get_NoText(Code_Number, "E");

                        Code_Number = "12,0"; // مئة

                        Third_Ar_Text = Get_NoText(Code_Number, "A");
                        Third_En_Text = Get_NoText(Code_Number, "E");

                        Merging_Ar_1 = " ";
                        Merging_En_1 = " ";
                    }
                }

                if (MST == "00" || MST == "")  // هل يوجد احاد وعشرات للمليون ؟ الاجابة لا يوجد
                {

                    Merging_Ar_Category = " ";
                    Merging_En_Category = " ";

                    Code_Category = "19,0"; //مليون
                    Category_Ar_Text = Get_NoText(Code_Category, "A");
                    Category_En_Text = Get_NoText(Code_Category, "E");

                    if (Hundred_Number == "000" && Thousand_Number == "000")  // لا يوجد مئات ولا الووف 
                    {

                        Merging_Ar_Currency = " ";
                        Merging_En_Currency = " ";

                        Code_Currency = "30,0";
                        Arbic_Currency = Get_Currency(Code_Currency, "A");
                        English_Currency = Get_Currency(Code_Currency, "E");

                        if (Fraction_Number != "00")  //  يوجد كسور
                        {
                            Link_Ar_No = " و ";
                            Link_En_No = " And ";
                        }
                    }
                    else  //  يوجد مئات او الووف
                    {
                        Link_Ar_No = " و ";
                        Link_En_No = " And ";
                    }
                }
                else  //  يوجد احاد وعشرات للمليون 
                {

                    Merging_Ar_Category = " ";
                    Merging_En_Category = "";

                    Category_Ar_Text = Get_NoText(Code_Category, "A");
                    //No English

                    Link_Ar_No = " و ";
                    Link_En_No = " And ";

                    if (MF == 0)
                    {
                        Link_Ar_No = "";
                        Link_En_No = "";
                        Merging_Ar_Category = "";
                        Merging_En_Category = "";
                    }
                }

                Million_Ar_Text = First_Ar_Text + Merging_Ar_1 + Second_Ar_Text + Merging_Ar_2 + Third_Ar_Text + Merging_Ar_Category + Category_Ar_Text + Merging_Ar_Currency + Arbic_Currency + Link_Ar_No;
                Million_En_Text = First_En_Text + Merging_En_1 + Second_En_Text + Merging_En_2 + Third_En_Text + Merging_En_Category + Category_En_Text + Merging_En_Currency + English_Currency + Link_En_No;


                Third_Ar_Text = "";
                Second_Ar_Text = "";
                First_Ar_Text = "";

                Third_En_Text = "";
                Second_En_Text = "";
                First_En_Text = "";

                Arbic_Currency = "";
                English_Currency = "";

                Category_Ar_Text = "";
                Category_En_Text = "";

                // 1  1  1
                Code_Currency = "";
                Code_Category = "";
                Code_Number = "";

                Merging_Ar_1 = "";
                Merging_Ar_2 = "";

                Merging_En_1 = "";
                Merging_En_2 = "";

                Merging_Ar_Currency = "";
                Merging_En_Currency = "";

                Link_Ar_No = "";
                Link_En_No = "";

            }

            // ---------------------------------------------   End Hundreds Of Millions    ----------------------------------------------------
            //Block_2
            // ---------------------------------------------   Start Ones && Tens Of Millions    ---------------------------------------------

            if (MST != "00" && MST != "")
            {   //  هل توجد عشرات للملايين
                if (MS == 0 && MT == 1)
                {     // احاد المليون 

                    Code_Category = "19,0"; // مليون error
                    Code_Currency = "30,0";   // ريال
                }
                else if (MS == 0 && MT == 2)
                {     // مليونان

                    Code_Currency = "30,0";   // ريال
                    Code_Category = "20,0";  // مليونان

                }
                else if (MS == 0 && MT > 2)
                {     // ثلاقة ملايين حتى تسعة ملايين(3)

                    Code_Category = "22,0"; // ملايين
                    Code_Number = "1" + MT + "," + "1";   // ثلاثة ¡ تسعة .......
                    Code_Currency = "30,0";   // ريال

                    First_Ar_Text = Get_NoText(Code_Number, "A");
                    First_En_Text = Get_NoText(Code_Number, "E");

                }
                else if (MS >= 1 && MT == 0)
                { // عشرات الملايين

                    Code_Category = "19,0"; // مليون
                    Code_Number = "1" + MS + "," + "4";   //  عشر | عشرون : تسعون .......
                    Code_Currency = "30,0";   // ريال

                    if (MS == 1)
                    {
                        Code_Category = "22,0"; // ملايين
                    }
                    Second_Ar_Text = Get_NoText(Code_Number, "A");
                    Second_En_Text = Get_NoText(Code_Number, "E");

                }
                else if (MS == 1 && MT > 0)
                {  // هل يوجد من 11- 19 مليون

                    Code_Category = "19,0"; // مليون
                    Code_Number = "1" + MT + "," + "5";   // احدى عشر ¡ تسعة عشر .......
                    Code_Currency = "30,0";   // ريال

                    First_Ar_Text = Get_NoText(Code_Number, "A");
                    First_En_Text = Get_NoText(Code_Number, "E");

                }
                else if (MS > 1 && MT > 0)  // هل يوجد 21 - مليون 99 
                {

                    Code_Category = "19,0"; // مليون
                    Code_Currency = "30,0";   // ريال

                    Code_Number = "1" + MT + "," + "1";   // ثلاثة ¡ تسعة .......

                    First_Ar_Text = Get_NoText(Code_Number, "A");
                    Second_En_Text = Get_NoText(Code_Number, "E");

                    Code_Number = "1" + MS + "," + "4";     // وعشرون ¡ وتسعون .......

                    Second_Ar_Text = Get_NoText(Code_Number, "A");
                    First_En_Text = Get_NoText(Code_Number, "E");

                    Merging_Ar_1 = " و ";
                    Merging_En_1 = "-";
                }

                Merging_Ar_Category = " ";
                Merging_En_Category = " ";
                if (MT < 3 && MS == 0)
                {
                    Merging_Ar_Category = "";
                    Merging_En_Category = "";
                }

                Category_Ar_Text = Get_NoText(Code_Category, "A");
                Category_En_Text = Get_NoText(Code_Category, "E");

                if (Hundred_Number == "000" && Thousand_Number == "000")  //  هل يوجد مئات ؟ لا يوجد ولا يوجد الووف ايضا
                {

                    Merging_Ar_Currency = " ";
                    Merging_En_Currency = " ";

                    Arbic_Currency = Get_Currency(Code_Currency, "A");
                    English_Currency = Get_Currency(Code_Currency, "E");

                }
                Link_Ar_No = "";
                Link_En_No = "";
                if (Fraction_Number != "00" || Hundred_Number != "000" || Thousand_Number != "000")  // يوجد كسور او مئات او الووف
                {
                    Link_Ar_No = " و ";
                    Link_En_No = " And ";
                }

                // ---------------------------------------------   End Ones && Tens Of Millions    ---------------------------------------------

                Million_Ar_Text = Million_Ar_Text + First_Ar_Text + Merging_Ar_1 + Second_Ar_Text + Merging_Ar_2 + Third_Ar_Text + Merging_Ar_Category + Category_Ar_Text + Merging_Ar_Currency + Arbic_Currency + Link_Ar_No;
                Million_En_Text = Million_En_Text + First_En_Text + Merging_En_1 + Second_En_Text + Merging_En_2 + Third_En_Text + Merging_En_Category + Category_En_Text + Merging_En_Currency + English_Currency + Link_En_No;

                Third_Ar_Text = "";
                Second_Ar_Text = "";
                First_Ar_Text = "";

                Third_En_Text = "";
                Second_En_Text = "";
                First_En_Text = "";

                Arbic_Currency = "";
                English_Currency = "";

                Category_Ar_Text = "";
                Category_En_Text = "";

                // 1  1  1
                Code_Currency = "";
                Code_Category = "";
                Code_Number = "";

                Merging_Ar_1 = "";
                Merging_Ar_2 = "";

                Merging_En_1 = "";
                Merging_En_2 = "";

                Merging_Ar_Currency = "";
                Merging_En_Currency = "";

                Link_Ar_No = "";
                Link_En_No = "";
            }

            // ***********************   End Million Number

            // ***********************   Start Thousand Number

            if (Thousand_Number != "000") //هناك الاالف
            {

                // ---------------------------------------------   Start Hundreds Of Thousands    --------------------------------------------------


                if (TF > 0) //  هل يوجد مئات الوف
                {
                    if (TF == 1) // هل يوجد مئة الف
                    {
                        Code_Currency = "30,0";   // ريال
                        Code_Category = "15,0";  // الف
                        Code_Number = "11,0"; // مئة

                        Third_Ar_Text = Get_NoText(Code_Number, "A");
                        Third_En_Text = Get_NoText(Code_Number, "E");
                    }
                    else if (TF == 2)  // هل يوجد مئتا الف
                    {

                        Code_Currency = "30,0";   // ريال
                        Code_Category = "15,0";  // الف
                        Code_Number = "14,0";  // مئتا

                        Third_Ar_Text = Get_NoText(Code_Number, "A");
                        Third_En_Text = Get_NoText(Code_Number, "E");
                    }
                    else if (TF > 2) // ارقام الوف اكبر من 2
                    {
                        Code_Currency = "30,0";   // ريال
                        Code_Category = "15,0";  // الف
                        Code_Number = "1" + TF + "," + "2";   // ثلاثة ¡ تسعة .......

                        First_Ar_Text = Get_NoText(Code_Number, "A");
                        First_En_Text = Get_NoText(Code_Number, "E");

                        Code_Number = "12,0"; // مئة

                        Third_Ar_Text = Get_NoText(Code_Number, "A");
                        Third_En_Text = Get_NoText(Code_Number, "E");

                        Merging_Ar_1 = " ";
                        Merging_En_1 = " ";
                    }
                }

                if (TST == "00" || TST == "")  // هل يوجد احاد وعشرات للالف ؟ الاجابة لا يوجد
                {

                    Merging_Ar_Category = " ";
                    Merging_En_Category = " ";

                    Code_Category = "15,0"; //الف
                    Category_Ar_Text = Get_NoText(Code_Category, "A");
                    Category_En_Text = Get_NoText(Code_Category, "E");

                    if (Hundred_Number == "000")  // لا يوجد مئات 
                    {

                        Merging_Ar_Currency = " ";
                        Merging_En_Currency = " ";

                        Code_Currency = "30,0";
                        Arbic_Currency = Get_Currency(Code_Currency, "A");
                        English_Currency = Get_Currency(Code_Currency, "E");

                        if (Fraction_Number != "00")  //  يوجد كسور
                        {
                            Link_Ar_No = " و ";
                            Link_En_No = " And ";
                        }
                    }
                    else  //  يوجد مئات
                    {
                        Link_Ar_No = " و ";
                        Link_En_No = " And ";
                    }
                }
                else  //  يوجد احاد وعشرات للالف 
                {
                    /////////////MMMMMM////////////////

                    Merging_Ar_Category = " ";
                    Merging_En_Category = "";

                    Category_Ar_Text = Get_NoText(Code_Category, "A");
                    //No English

                    Link_Ar_No = " و ";
                    Link_En_No = " And ";

                    if (TF == 0)
                    {
                        Link_Ar_No = "";
                        Link_En_No = "";
                        Merging_Ar_Category = "";
                        Merging_En_Category = "";
                    }
                }

                Thousand_Ar_Text = First_Ar_Text + Merging_Ar_1 + Second_Ar_Text + Merging_Ar_2 + Third_Ar_Text + Merging_Ar_Category + Category_Ar_Text + Merging_Ar_Currency + Arbic_Currency + Link_Ar_No;
                Thousand_En_Text = First_En_Text + Merging_En_1 + Second_En_Text + Merging_En_2 + Third_En_Text + Merging_En_Category + Category_En_Text + Merging_En_Currency + English_Currency + Link_En_No;

                Third_Ar_Text = "";
                Second_Ar_Text = "";
                First_Ar_Text = "";

                Third_En_Text = "";
                Second_En_Text = "";
                First_En_Text = "";

                Arbic_Currency = "";
                English_Currency = "";

                Category_Ar_Text = "";
                Category_En_Text = "";

                // 1  1  1
                Code_Currency = "";
                Code_Category = "";
                Code_Number = "";

                Merging_Ar_1 = "";
                Merging_Ar_2 = "";

                Merging_En_1 = "";
                Merging_En_2 = "";

                Merging_Ar_Currency = "";
                Merging_En_Currency = "";

                Link_Ar_No = "";
                Link_En_No = "";

            }

            // ---------------------------------------------   End Hundreds Of Thousands    ----------------------------------------------------
            //Block_2
            // ---------------------------------------------   Start Ones && Tens Of Thousands    ---------------------------------------------

            if (TST != "00" && TST != "")
            {   //  هل توجد عشرات الالوف
                if (TS == 0 && TT == 1)
                {     // احاد الوف 

                    Code_Category = "16,0"; // الف
                    Code_Currency = "30,0";   // ريال

                }
                else if (TS == 0 && TT == 2)
                {     // الفان

                    Code_Currency = "30,0";   // ريال
                    Code_Category = "17,0";  // الفان

                }
                else if (TS == 0 && TT > 2)
                {     // ثلاقة الاف حتى تسعة الاف(3)

                    Code_Category = "18,0"; // الف
                    Code_Number = "1" + TT + "," + "1";   // ثلاثة ¡ تسعة .......
                    Code_Currency = "30,0";   // ريال

                    First_Ar_Text = Get_NoText(Code_Number, "A");
                    First_En_Text = Get_NoText(Code_Number, "E");

                }
                else if (TS >= 1 && TT == 0)
                { // عشرات الاف

                    Code_Category = "15,0"; // الف
                    Code_Number = "1" + TS + "," + "4";   //  عشر | عشرون : تسعون .......
                    Code_Currency = "30,0";   // ريال

                    if (TS == 1)
                    {
                        Code_Category = "18,0"; // الاف
                    }
                    Second_Ar_Text = Get_NoText(Code_Number, "A");
                    Second_En_Text = Get_NoText(Code_Number, "E");

                }
                else if (TS == 1 && TT > 0)
                {  // هل يوجد من 11- 19 الف

                    Code_Category = "15,0"; // الف
                    Code_Number = "1" + TT + "," + "5";   // احدى عشر ¡ تسعة عشر .......
                    Code_Currency = "30,0";   // ريال

                    First_Ar_Text = Get_NoText(Code_Number, "A");
                    First_En_Text = Get_NoText(Code_Number, "E");

                }
                else if (TS > 1 && TT > 0)  // هل يوجد 21 - الف 99 
                {

                    Code_Category = "15,0"; // الف
                    Code_Currency = "30,0";   // ريال

                    Code_Number = "1" + TT + "," + "1";   // ثلاثة ¡ تسعة .......

                    First_Ar_Text = Get_NoText(Code_Number, "A");
                    Second_En_Text = Get_NoText(Code_Number, "E");

                    Code_Number = "1" + TS + "," + "4";     // وعشرون ¡ وتسعون .......

                    Second_Ar_Text = Get_NoText(Code_Number, "A");
                    First_En_Text = Get_NoText(Code_Number, "E");

                    Merging_Ar_1 = " و ";
                    Merging_En_1 = "-";
                }

                Merging_Ar_Category = " ";
                Merging_En_Category = " ";
                if (TT < 3 && TS == 0)
                {
                    Merging_Ar_Category = "";
                    Merging_En_Category = "";
                }

                Category_Ar_Text = Get_NoText(Code_Category, "A");
                Category_En_Text = Get_NoText(Code_Category, "E");

                if (Hundred_Number == "000")  // هل يوجد مئات ؟ لا يوجد
                {

                    Merging_Ar_Currency = " ";
                    Merging_En_Currency = " ";

                    Arbic_Currency = Get_Currency(Code_Currency, "A");
                    English_Currency = Get_Currency(Code_Currency, "E");

                }
                Link_Ar_No = "";
                Link_En_No = "";
                if (Fraction_Number != "00" || Hundred_Number != "000")  // يوجد كسور او مئات
                {
                    Link_Ar_No = " و ";
                    Link_En_No = " And ";
                }


                // ---------------------------------------------   End Ones && Tens Of Thousands    ---------------------------------------------

                Thousand_Ar_Text = Thousand_Ar_Text + First_Ar_Text + Merging_Ar_1 + Second_Ar_Text + Merging_Ar_2 + Third_Ar_Text + Merging_Ar_Category + Category_Ar_Text + Merging_Ar_Currency + Arbic_Currency + Link_Ar_No;
                Thousand_En_Text = Thousand_En_Text + First_En_Text + Merging_En_1 + Second_En_Text + Merging_En_2 + Third_En_Text + Merging_En_Category + Category_En_Text + Merging_En_Currency + English_Currency + Link_En_No;

                Third_Ar_Text = "";
                Second_Ar_Text = "";
                First_Ar_Text = "";

                Third_En_Text = "";
                Second_En_Text = "";
                First_En_Text = "";

                Arbic_Currency = "";
                English_Currency = "";

                Category_Ar_Text = "";
                Category_En_Text = "";

                // 1  1  1
                Code_Currency = "";
                Code_Category = "";
                Code_Number = "";

                Merging_Ar_1 = "";
                Merging_Ar_2 = "";

                Merging_En_1 = "";
                Merging_En_2 = "";

                Merging_Ar_Currency = "";
                Merging_En_Currency = "";

                Link_Ar_No = "";
                Link_En_No = "";
            }

            // ***********************   End Thousand Number

            // ***********************   Start Hundreds Number

            if (Hundred_Number != "000")
            {  // توجد ارقام الوف


                // ---------------------------------------------   Start Hundreds Of Hundred    --------------------------------------------------

                if (HF == 1)  // مئة
                {
                    Code_Currency = "30,0";   // ريال
                    Code_Number = "11,0";   // مئة

                    First_Ar_Text = Get_NoText(Code_Number, "A");
                    First_En_Text = Get_NoText(Code_Number, "E");

                }
                else if (HF == 2)   // مئتا
                {

                    Code_Currency = "30,0";   // ريال
                    Code_Number = "14,0"; // مئتا

                    if (Convert.ToDouble(HST) > 0 || Convert.ToDouble(Fraction_Number) > 0)  // مئتان
                    {
                        Code_Number = "13,0";
                    }
                    Third_Ar_Text = Get_NoText(Code_Number, "A");
                    Third_En_Text = Get_NoText(Code_Number, "E");
                }
                else if (HF > 2)  // اكبر من 200
                {

                    Code_Currency = "30,0";   // ريال
                    Code_Number = "1" + HF + "," + "2";   // ثلاث ¡ تسع .......

                    First_Ar_Text = Get_NoText(Code_Number, "A");
                    First_En_Text = Get_NoText(Code_Number, "E");

                    Code_Number = "12,0";  // مئة

                    Third_Ar_Text = Get_NoText(Code_Number, "A");
                    Third_En_Text = Get_NoText(Code_Number, "E");

                    Merging_Ar_1 = " ";
                    Merging_En_1 = " ";

                }

                if (HST == "00")  // هل يوجد احاد وعشرات في المئات ؟ لا يوجد
                {

                    Merging_Ar_Currency = " ";
                    Merging_En_Currency = " ";

                    Arbic_Currency = Get_Currency(Code_Currency, "A");
                    English_Currency = Get_Currency(Code_Currency, "E");

                    if (Fraction_Number != "00" && Fraction_Number != "")  // هل يوجد ارقام عشرية ؟ لا يوجد
                    {
                        Link_Ar_No = " و ";
                        Link_En_No = " And ";
                    }
                }
                else if (HF > 0)  // هل يوجد احاد وعشرات بدون مئات ؟ نعم يوجد
                {
                    Link_Ar_No = " و ";
                    Link_En_No = " And ";
                }

                Hundred_Ar_Text = First_Ar_Text + Merging_Ar_1 + Second_Ar_Text + Merging_Ar_2 + Third_Ar_Text + Merging_Ar_Currency + Arbic_Currency + Link_Ar_No;
                Hundred_En_Text = First_En_Text + Merging_En_1 + Second_En_Text + Merging_En_2 + Third_En_Text + Merging_En_Currency + English_Currency + Link_En_No;

                Third_Ar_Text = "";
                First_Ar_Text = "";

                Third_En_Text = "";
                First_En_Text = "";

                Arbic_Currency = "";
                English_Currency = "";

                Merging_Ar_1 = "";
                Merging_En_1 = "";

                Merging_Ar_Currency = "";
                Merging_En_Currency = "";

                Link_Ar_No = "";
                Link_En_No = "";

            }

            // ---------------------------------------------   End Hundreds Of Hundred    --------------------------------------------------


            // ---------------------------------------------   Start Ones  && Tens Of Hundred    ---------------------------------------------

            if (HST != "00")   //  هل الاحاد والعشرات بالمئات موجود ؟ نعم موجود
            {

                if (HS == 0 && HT == 1)  // ريال
                {
                    Code_Number = "11,2";
                    Code_Currency = "30,0";   // ريال

                    First_En_Text = Get_NoText(Code_Number, "E");
                }
                else if (HS == 0 && HT == 2)  // ريالان
                {
                    Code_Number = "31,0";
                    Code_Currency = "31,0";    // لايالان
                }
                else if (HS == 0 && HT > 2)  // الريالات المفردة من (3)  (9)
                {
                    Code_Number = "1" + HT + "," + "1";
                    Code_Currency = "32,0";    // ريال

                    First_Ar_Text = Get_NoText(Code_Number, "A");
                    First_En_Text = Get_NoText(Code_Number, "E");
                }
                else if (HS >= 1 && HT == 0)  // ارقام العقود في مئات الريالات
                {
                    Code_Number = "1" + HS + "," + "4";
                    Code_Currency = "33,0";    // ريالا

                    if (HS == 1)
                    {
                        Code_Currency = "32,0";  // ريالات
                    }
                    Second_Ar_Text = Get_NoText(Code_Number, "A");
                    Second_En_Text = Get_NoText(Code_Number, "E");

                }
                else if (HS == 1 && HT > 0)  // الارقام المرتبة بالريالات من (11)  (19)
                {
                    Code_Number = "1" + HT + "," + "5";
                    Code_Currency = "33,0";    // ريالا

                    First_Ar_Text = Get_NoText(Code_Number, "A");
                    First_En_Text = Get_NoText(Code_Number, "E");

                }
                else if (HS > 1 && HT > 0)  // الارقام المعطوفة من  (21)  (99)
                {
                    Code_Currency = "33,0";    // ريالا
                    Code_Number = "1" + HT + "," + "1";

                    First_Ar_Text = Get_NoText(Code_Number, "A");
                    Second_En_Text = Get_NoText(Code_Number, "E");

                    Code_Number = "1" + HS + "," + "4";

                    Second_Ar_Text = Get_NoText(Code_Number, "A");
                    First_En_Text = Get_NoText(Code_Number, "E");

                    Merging_Ar_1 = " و ";
                    Merging_En_1 = "-";

                }

                Merging_Ar_Currency = " ";
                Merging_En_Currency = " ";
                if (HT < 3 && HS == 0)
                {
                    Merging_Ar_Currency = "";
                    Merging_En_Currency = "";
                }
                Arbic_Currency = Get_Currency(Code_Currency, "A");
                English_Currency = Get_Currency(Code_Currency, "E");

                if (Fraction_Number != "00")  // هل يوجد ارقام عشرية ؟ نعم يوجد
                {
                    Link_Ar_No = " و ";
                    Link_En_No = " And ";
                }

                Hundred_Ar_Text = Hundred_Ar_Text + First_Ar_Text + Merging_Ar_1 + Second_Ar_Text + Merging_Ar_2 + Third_Ar_Text + Merging_Ar_Currency + Arbic_Currency + Link_Ar_No;
                Hundred_En_Text = Hundred_En_Text + First_En_Text + Merging_En_1 + Second_En_Text + Merging_En_2 + Third_En_Text + Merging_En_Currency + English_Currency + Link_En_No;

                Second_Ar_Text = "";
                Second_En_Text = "";

                First_Ar_Text = "";
                First_En_Text = "";

                Merging_Ar_1 = "";
                Merging_Ar_2 = "";

                Merging_En_1 = "";
                Merging_En_2 = "";

            }


            // =========================================================================================================================

            return (Million_Ar_Text + Thousand_Ar_Text + Hundred_Ar_Text + Fraction_Ar_Text, Million_En_Text + Thousand_En_Text + Hundred_En_Text + Fraction_En_Text);
        }
    }
}
