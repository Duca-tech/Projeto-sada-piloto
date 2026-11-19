using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Domain
{
    public static class Rotina
    {

        public static string TratarTexto(string texto)
        {
          if (string.IsNullOrEmpty(texto))
            {
                return texto;
            }

            string tratado = texto.Normalize(NormalizationForm.FormD);
            tratado = Regex.Replace(tratado ?? string.Empty, @"\s+", "");

            tratado = tratado.Trim().ToLower();

            tratado = Regex.Replace(tratado, @"[^a-z0-9\s]", "");

            tratado = Regex.Replace(tratado, @"\s+", " ").Trim();

            return tratado;
        }


        public static DateTime ConverterDataConvencional(string Data)
        {
            var Valida =  DateTime.TryParseExact(
                    Data,
                    "dd/MM/yyyy",
                    CultureInfo.GetCultureInfo("pt-BR"),
                    DateTimeStyles.None,
                    out var DataConvertida
                );

            if (!Valida)
            {
                throw new Exception("Data inválida.");

            }

            return DataConvertida;
        }

        public static bool ValidarCampoGuidString(string guid)
        {
            if(Guid.TryParse(guid, out Guid valor))
            {
                return true;
            }
            return false;

        }
    }
}
