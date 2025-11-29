using System;

class TestIbanValidation
{
    static void Main()
    {
        string iban = "IR190160000000000075345018";
        Console.WriteLine($"Testing IBAN: {iban}");
        
        // مرحله 1: 4 کاراکتر اول را به انتها منتقل می‌کنیم
        string rearranged = iban.Substring(4) + iban.Substring(0, 4);
        Console.WriteLine($"Rearranged: {rearranged}");
        
        // مرحله 2: تبدیل حروف به اعداد
        string numericString = "";
        foreach (char c in rearranged)
        {
            if (char.IsLetter(c))
            {
                numericString += (c - 'A' + 10).ToString();
            }
            else if (char.IsDigit(c))
            {
                numericString += c;
            }
        }
        Console.WriteLine($"Numeric String: {numericString}");
        
        // مرحله 3: محاسبه MOD 97
        int remainder = 0;
        for (int i = 0; i < numericString.Length; i++)
        {
            remainder = (remainder * 10 + int.Parse(numericString[i].ToString())) % 97;
        }
        Console.WriteLine($"Remainder: {remainder}");
        Console.WriteLine($"Valid: {remainder == 1}");
    }
}

